using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Monsters;
using Unity.Profiling;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Weapon
{
    /// <summary>
    /// 服务器子弹数据模拟。碰撞、伤害接口、表现通知分别处理
    /// </summary>
    public sealed class ProjectileSimulation
    {
        private readonly ProjectileWorld _world;
        private readonly ShotRepository _shots;
        private readonly WeaponStatSnapshotRepository _stats;
        private readonly ProjectileSimulationConfig _config;
        private readonly MonsterRuntimeService _monsters;
        private readonly IProjectileEffectDispatcher _effects;

        public event Action<ProjectileImpact> Impact;
        public event Action<ProjectileState> Removed;

        private const int OverlapBufferSize = 8;
        private const int CastBufferSize = 16;

        private readonly Collider[] _overlapBuffer = new Collider[OverlapBufferSize];
        private readonly RaycastHit[] _castBuffer = new RaycastHit[CastBufferSize];
        private int[] _monsterCandidates = Array.Empty<int>();
        private readonly Dictionary<uint, HashSet<ulong>> _hitEntities = new();
        private readonly Stack<HashSet<ulong>> _hitEntityPool = new();

        private static readonly ProfilerMarker MonsterNarrowPhaseMarker =
            new("Weapon.Projectile.MonsterNarrowPhase");

        private enum ContactKind : byte
        {
            None,
            World,
            Monster,
        }

        private struct Contact
        {
            public ContactKind Kind;
            public int MonsterSlot;
            public Vector3 Point;
            public Vector3 Normal;
            public float Distance;
        }

        public ProjectileSimulation(ProjectileWorld world, ShotRepository shots,
            WeaponStatSnapshotRepository stats, ProjectileSimulationConfig config,
            MonsterRuntimeService monsters,
            IProjectileEffectDispatcher effects = null)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _shots = shots ?? throw new ArgumentNullException(nameof(shots));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _monsters = monsters ?? throw new ArgumentNullException(nameof(monsters));
            _effects = effects ?? NoneProjectileEffectDispatcher.Instance;
        }

        /// <summary>
        /// 遍历所有子弹，模拟它，还活着就写回，死了就删
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f) 
                return;
            for (int i = _world.Count - 1; i >= 0; i--)
            {
                var projectile = _world.Get(i);
                if (Simulate(ref projectile, deltaTime)) 
                    _world.Set(i, projectile);
                else
                {
                    if (_shots.TryGet(projectile.ShotId, out ShotContext shot) &&
                        _stats.TryGet(shot.StatSnapshotId, out WeaponStatSnapshot stats))
                        _effects.DispatchDestroyed(ref projectile,
                            new ProjectileDestroyedEffectContext(shot, stats));
                    _world.RemoveAtSwapBack(i);
                    if (_hitEntities.Remove(projectile.ProjectileId, out HashSet<ulong> hitEntities))
                    {
                        hitEntities.Clear();
                        _hitEntityPool.Push(hitEntities);
                    }
                    _shots.ReleaseProjectile(projectile.ShotId);
                    Removed?.Invoke(projectile);
                }
            }
        }

        private bool Simulate(ref ProjectileState projectile, float deltaTime)
        {
            //取出上下文和快照
            if (!_shots.TryGet(projectile.ShotId, out var shot) || !_stats.TryGet(shot.StatSnapshotId, out var stats)) 
                return false;
            //先算当前Tick最多运动世界
            float travelTime = Mathf.Min(deltaTime, projectile.RemainingLifeTime);
            projectile.RemainingLifeTime -= deltaTime;
            if (travelTime <= 0) 
                return false;
            //用速度求出当前Tick的位移
            float remaining = projectile.Velocity.magnitude * travelTime;
            //碰撞检测半径，这里碰撞检测类似拿一个碰撞球沿路径扫过去
            float radius = Mathf.Max(_config.MinimumRadius, stats.ProjectileSize * projectile.SizeMultiplier * 0.5f);
            int collisionCount = 0;

            Vector3 direction = projectile.Velocity.normalized;
            //仅出生后的第一次模拟执行一次Overlap检测
            if ((projectile.Flags & ProjectileFlags.SpawnChecked) == 0)
            {
                projectile.Flags |= ProjectileFlags.SpawnChecked;

                if (TryInitialOverlap(projectile, shot, radius, direction, out var initialHit))
                {
                    if (collisionCount++ >= Mathf.Max(1, _config.MaxCollisionsPerTick))
                        return false;

                    if (!ResolveContact(ref projectile, shot, stats, radius, direction, ref remaining, initialHit))
                        return false;
                }
            }
            //Mathf.Epsilon为比0大的极限最小浮点值，防浮点误差
            while(remaining > Mathf.Epsilon)
            {
                //已到最大碰撞限制就死掉，防止卡死
                if(collisionCount++ >= Mathf.Max(1, _config.MaxCollisionsPerTick))
                    return false;
                //每次循环重新取方向，考虑弹射的情况
                direction = projectile.Velocity.normalized;
                //碰撞检测没找到，直接加位置结束
                if(!TryCast(projectile, shot, radius, direction, remaining, out var hit))
                {
                    projectile.Position += direction * remaining;
                    break;
                }

                if(!ResolveContact(ref projectile, shot, stats, radius, direction, ref remaining, hit))
                    return false;
            }
            return projectile.RemainingLifeTime > 0f;
        }
        /// <summary>
        /// 已经得到 Contact 以后，如何处理这次碰撞
        /// 返回bool代表碰撞处理完成后，当前子弹能否继续存活进行下一次模拟
        /// </summary>
        private bool ResolveContact(
              ref ProjectileState projectile,
              in ShotContext shot,
              in WeaponStatSnapshot stats,
              float radius,
              Vector3 direction,
              ref float remaining,
              in Contact hit)
        {
            //先推进到碰撞位置，碰撞是球体，hit.Point是不准确的
            projectile.Position += direction * hit.Distance;
            remaining = Mathf.Max(0, remaining - hit.Distance);
            bool isMonsterContact = hit.Kind == ContactKind.Monster;
            ulong targetEntityId = 0;
            bool hasTarget = false;

            if (isMonsterContact)
            {
                hasTarget = _monsters.IsMonsterAlive(hit.MonsterSlot);
                if (hasTarget)
                    targetEntityId = MonsterRuntimeService.MonsterEntityIdBase + (uint)hit.MonsterSlot;
            }

            ProjectileHitResolution resolution = ProjectileHitResolution.Destroy;
            float damage = stats.Damage * projectile.DamageMultiplier;
            float projectileSize = stats.ProjectileSize * projectile.SizeMultiplier;
            bool isCritical = (projectile.Flags & ProjectileFlags.Critical) != 0;
            float vfxWeight = ProjectileImpactVfxWeight.Calculate(damage, projectileSize, isCritical);
            //是可攻击对象
            if (isMonsterContact)
            {
                if (hasTarget)
                {
                    _monsters.DamageMonster(hit.MonsterSlot, damage);

                    RememberHit(projectile.ProjectileId, targetEntityId);
                    projectile.HitCount++;
                    _effects.DispatchHit(ref projectile,
                        new ProjectileHitContext(shot, stats, targetEntityId, hit.Point, hit.Normal));

                    if (projectile.PierceRemaining > 0)
                    {
                        projectile.PierceRemaining--;
                        resolution = ProjectileHitResolution.Continue;
                    }
                }
            }
            //是墙壁
            else
            {
                if (projectile.BounceRemaining > 0)
                {
                    var reflected = Vector3.Reflect(direction, hit.Normal);
                    if (reflected.sqrMagnitude > Mathf.Epsilon)
                    {
                        projectile.BounceRemaining--;
                        //反弹改变方向不改变速度
                        projectile.Velocity = reflected.normalized * projectile.Velocity.magnitude;
                        _effects.DispatchBounce(ref projectile, shot, stats, hit.Point, hit.Normal);
                        //将当前位置推出表面，防止出现下一轮检测到同一面
                        projectile.Position = hit.Point + hit.Normal * (radius + _config.SurfaceOffset);
                        resolution = ProjectileHitResolution.Continue;
                    }
                }
            }

            if (resolution == ProjectileHitResolution.Continue)
            {
                //位置加上偏移，也是防止检测到同一表面
                float offset = Mathf.Max(0.001f, _config.SurfaceOffset);
                projectile.Position += projectile.Velocity.normalized * offset;
                remaining = Mathf.Max(0, remaining - offset);
            }

            //表现层事件在伤害、特殊 Effect、穿透和反弹全部结算后统一发布。
            //Projectile 快照已经包含最终速度、剩余次数和命中次数，外部无需重复计算。
            Impact?.Invoke(new ProjectileImpact
            {
                Shot = shot,
                Projectile = projectile,
                Point = hit.Point,
                Normal = hit.Normal,
                HasTarget = hasTarget,
                TargetEntityId = hasTarget ? targetEntityId : 0,
                VfxWeight = vfxWeight,
                Resolution = resolution,
            });

            return resolution == ProjectileHitResolution.Continue;
        }

        private bool HasHit(uint projectileId, ulong entityId) =>
            _hitEntities.TryGetValue(projectileId, out HashSet<ulong> entities) && entities.Contains(entityId);

        private void RememberHit(uint projectileId, ulong entityId)
        {
            if (!_hitEntities.TryGetValue(projectileId, out HashSet<ulong> entities))
            {
                entities = _hitEntityPool.Count > 0 ? _hitEntityPool.Pop() : new HashSet<ulong>();
                _hitEntities.Add(projectileId, entities);
            }
            entities.Add(entityId);
        }

        /// <summary>
        /// 为运行中生成的新弹体继承一次已经结算过的目标命中。
        /// 分裂弹出生在命中表面，下一 Tick 必须忽略它的出生宿主，避免被初始 Overlap 立即回收。
        /// </summary>
        internal void MarkTargetAsHit(uint projectileId, ulong targetEntityId) =>
            RememberHit(projectileId, targetEntityId);


        /// <summary>
        /// 无Collider条件下的怪物碰撞检测，使用怪物的圆形受击半径进行判定
        /// </summary>
        private bool TryFindMonsterContact(
            in ProjectileState projectile,
            in ShotContext shot,
            float projectileRadius,
            Vector3 direction,
            float maxDistance,
            bool initialOverlap,
            out Contact contact)
        {
            contact = default;
            MonsterRoomRuntime room = _monsters.AuthorityRoom;
            if (room == null)
                return false;

            Vector2 start = new Vector2(projectile.Position.x, projectile.Position.z);
            Vector2 end = start + new Vector2(direction.x, direction.z) * maxDistance;
            Vector2 planarDirection = new Vector2(direction.x, direction.z);
            float planarMagnitude = planarDirection.magnitude;
            if (planarMagnitude > 0.0001f) planarDirection /= planarMagnitude;
            //将子弹碰撞半径与怪物的圆形受击半径相加，得到一个组合半径，用于判断子弹与怪物的碰撞
            float monsterHitRadius = room.MonsterHitRadius;
            float combinedRadius = projectileRadius + monsterHitRadius;
            float combinedRadiusSq = combinedRadius * combinedRadius;
            float nearest = float.PositiveInfinity;
            bool found = false;

            EnsureMonsterCandidateCapacity(room.World.SlotCount);
            int candidateCount = room.QueryMonsterCandidates(
                start, initialOverlap ? start : end, combinedRadius, _monsterCandidates);

            using (MonsterNarrowPhaseMarker.Auto())
            {
                for (int i = 0; i < candidateCount; i++)
                {
                    int slot = _monsterCandidates[i];
                    if (!room.World.Meta[slot].IsActive)
                        continue;

                    ulong entityId = MonsterRuntimeService.MonsterEntityIdBase + (uint)slot;
                    if (entityId == shot.OwnerEntityId || HasHit(projectile.ProjectileId, entityId))
                        continue;

                    Vector2 center = room.World.Motion[slot].Position;
                    Vector2 toCenter = center - start;
                    float pathDistance;

                    if (initialOverlap)
                    {
                        if (toCenter.sqrMagnitude > combinedRadiusSq)
                            continue;
                        pathDistance = 0f;
                    }
                    else
                    {
                        if (toCenter.sqrMagnitude <= combinedRadiusSq)
                            pathDistance = 0f;
                        else
                        {
                            if (planarMagnitude <= 0.0001f)
                                continue;
                            float projection = Vector2.Dot(toCenter, planarDirection);
                            float discriminant = projection * projection -
                                                   (toCenter.sqrMagnitude - combinedRadiusSq);
                            if (projection < 0f || discriminant < 0f)
                                continue;

                            float horizontalDistance = projection - Mathf.Sqrt(discriminant);
                            if (horizontalDistance < 0f)
                                continue;
                            pathDistance = horizontalDistance / planarMagnitude;
                            if (pathDistance > maxDistance)
                                continue;
                        }
                    }

                    if (pathDistance >= nearest)
                        continue;

                    Vector3 projectileAtHit = projectile.Position + direction * pathDistance;
                    Vector2 normal2 = new Vector2(
                        projectileAtHit.x - center.x,
                        projectileAtHit.z - center.y);
                    if (normal2.sqrMagnitude > 0.0001f)
                        normal2.Normalize();
                    else if (planarMagnitude > 0.0001f)
                        normal2 = -planarDirection;
                    else
                        normal2 = Vector2.right;

                    contact = new Contact
                    {
                        Kind = ContactKind.Monster,
                        MonsterSlot = slot,
                        Point = new Vector3(
                            center.x + normal2.x * monsterHitRadius,
                            projectileAtHit.y,
                            center.y + normal2.y * monsterHitRadius),
                        Normal = new Vector3(normal2.x, 0f, normal2.y),
                        Distance = pathDistance
                    };

                    nearest = pathDistance;
                    found = true;
                }
            }

            return found;
        }

        private void EnsureMonsterCandidateCapacity(int slotCount)
        {
            if (_monsterCandidates.Length >= slotCount)
                return;
            int capacity = Math.Max(slotCount, _monsterCandidates.Length == 0 ? 64 : _monsterCandidates.Length * 2);
            Array.Resize(ref _monsterCandidates, capacity);
        }

        private bool TryCast(in ProjectileState projectile, in ShotContext shot, float radius,
            Vector3 direction, float distance, out Contact contact)
        {
            int mask = _config.WorldMask.value;
            //碰撞信息会存进缓存的_castBuffer中，无额外GC
            int count = Physics.SphereCastNonAlloc(
                            projectile.Position,
                            radius,
                            direction,
                            _castBuffer,
                            distance,
                            mask,
                            QueryTriggerInteraction.Collide);
            contact = default;
            float nearest = float.PositiveInfinity;
            //拿能跑距离内的所有碰撞体
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = _castBuffer[i];

                if (hit.collider == null || hit.distance >= nearest)
                    continue;

                nearest = hit.distance;

                contact = new Contact
                {
                    Kind = ContactKind.World,
                    MonsterSlot = -1,
                    Point = hit.point,
                    Normal = hit.normal,
                    Distance = hit.distance,
                };
            }

            if (TryFindMonsterContact(projectile, shot, radius, direction, distance, false, out Contact monsterContact) &&
                                     (contact.Kind == ContactKind.None || monsterContact.Distance < contact.Distance))
                contact = monsterContact;

            return contact.Kind != ContactKind.None;
        }
        /// <summary>
        /// 在子弹生成时进行判定，针对子弹生成在碰撞体内部的情况
        /// </summary>
        private bool TryInitialOverlap(
            in ProjectileState projectile,
            in ShotContext shot,
            float radius,
            Vector3 direction,
            out Contact contact)
        {
            int count = Physics.OverlapSphereNonAlloc(
                projectile.Position,
                radius,
                _overlapBuffer,
                _config.WorldMask.value,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                Collider collider = _overlapBuffer[i];
                if (collider == null)
                    continue;

                Vector3 point = collider.ClosestPoint(projectile.Position);
                Vector3 normal = projectile.Position - point;

                contact = new Contact
                {
                    Kind = ContactKind.World,
                    MonsterSlot = -1,
                    Point = point,
                    Normal = normal.sqrMagnitude > 0.0001f ? normal.normalized : -direction,
                    Distance = 0f
                };

                return true;
            }

            return TryFindMonsterContact(
                projectile, shot, radius, direction, 0f, true, out contact);
        }
    }

}
