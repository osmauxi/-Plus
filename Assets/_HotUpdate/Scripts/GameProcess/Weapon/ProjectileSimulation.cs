using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Monsters;
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
        private readonly IProjectileEffectDispatcher _effects;

        public event Action<ProjectileImpact> Impact;
        public event Action<ProjectileState> Removed;

        private const int OverlapBufferSize = 8;
        private const int CastBufferSize = 16;

        private readonly Collider[] _overlapBuffer = new Collider[OverlapBufferSize];
        private readonly RaycastHit[] _castBuffer = new RaycastHit[CastBufferSize];
        private readonly Dictionary<uint, HashSet<ulong>> _hitEntities = new();

        private struct Contact
        {
            public Collider Collider;
            public Vector3 Point;
            public Vector3 Normal;
            public float Distance;
        }

        public ProjectileSimulation(ProjectileWorld world, ShotRepository shots,
            WeaponStatSnapshotRepository stats, ProjectileSimulationConfig config,
            IProjectileEffectDispatcher effects = null)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _shots = shots ?? throw new ArgumentNullException(nameof(shots));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _config = config ?? throw new ArgumentNullException(nameof(config));
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
                    _hitEntities.Remove(projectile.ProjectileId);
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
            //开始判定碰撞性质
            //Unity Layer本身有索引，1左移将其转换为对应索引的掩码，按位与判定TargetMask是否包含物体的Layer
            //LayerMask.Value为多选后处理好了的场景掩码，所以这里可以不做转换直接按位与
            bool targetLayer = (_config.TargetMask.value & (1 << hit.Collider.gameObject.layer)) != 0;
            ulong targetEntityId = 0;
            MonsterViewHandle monster = null;
            bool hasTarget = targetLayer && TryResolveTarget(hit.Collider,
                out targetEntityId, out _, out monster);
            ProjectileHitResolution resolution = ProjectileHitResolution.Destroy;
            float damage = stats.Damage * projectile.DamageMultiplier;
            float projectileSize = stats.ProjectileSize * projectile.SizeMultiplier;
            bool isCritical = (projectile.Flags & ProjectileFlags.Critical) != 0;
            float vfxWeight = ProjectileImpactVfxWeight.Calculate(damage, projectileSize, isCritical);
            //是可攻击对象
            if (targetLayer)
            {
                if (hasTarget)
                {
                    MonsterRuntimeService.Instance.DamageMonster(monster.Slot, damage);

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

        /// <summary>
        /// 忽略自己，已死亡目标，已经命中过的对象
        /// </summary>
        private bool IsIgnored(Collider collider, in ShotContext shot, uint projectileId)
        {
            if (!TryResolveTarget(collider, out ulong entityId, out bool isAlive, out _))
                return false;

            return entityId == shot.OwnerEntityId || HasHit(projectileId, entityId) || !isAlive;
        }

        private static bool TryResolveTarget(Collider collider, out ulong entityId, out bool isAlive,
            out MonsterViewHandle monster)
        {
            monster = collider.GetComponentInParent<MonsterViewHandle>();
            if (monster != null && monster.Slot >= 0)
            {
                entityId = MonsterRuntimeService.MonsterEntityIdBase + (uint)monster.Slot;
                MonsterRuntimeService runtime = MonsterRuntimeService.Instance;
                isAlive = runtime != null && runtime.IsMonsterAlive(monster.Slot);
                return true;
            }

            entityId = 0;
            isAlive = false;
            return false;
        }

        private bool HasHit(uint projectileId, ulong entityId) =>
            _hitEntities.TryGetValue(projectileId, out HashSet<ulong> entities) && entities.Contains(entityId);

        private void RememberHit(uint projectileId, ulong entityId)
        {
            if (!_hitEntities.TryGetValue(projectileId, out HashSet<ulong> entities))
            {
                entities = new HashSet<ulong>();
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

        private bool TryCast(in ProjectileState projectile, in ShotContext shot, float radius,
            Vector3 direction, float distance, out Contact contact)
        {
            int mask = _config.TargetMask | _config.WorldMask;
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

                if (hit.collider == null ||
                    hit.distance >= nearest ||
                    IsIgnored(hit.collider, shot, projectile.ProjectileId))
                    continue;

                nearest = hit.distance;

                contact = new Contact
                {
                    Collider = hit.collider,
                    Point = hit.point,
                    Normal = hit.normal,
                    Distance = hit.distance,
                };
            }

            return contact.Collider != null;
        }
        /// <summary>
        /// 在子弹生成时进行判定，针对子弹生成在碰撞体内部的情况
        /// </summary>
        private bool TryInitialOverlap(in ProjectileState projectile,in ShotContext shot,float radius,Vector3 direction,out Contact contact)
        {
            int mask = _config.TargetMask | _config.WorldMask;
            int count = Physics.OverlapSphereNonAlloc(
                projectile.Position,
                radius,
                _overlapBuffer,
                mask,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                Collider collider = _overlapBuffer[i];
                if (collider == null || IsIgnored(collider, shot, projectile.ProjectileId)) continue;

                Vector3 point = collider.ClosestPoint(projectile.Position);
                Vector3 normal = projectile.Position - point;

                contact = new Contact
                {
                    Collider = collider,
                    Point = point,
                    Normal = normal.sqrMagnitude > 0.0001f ? normal.normalized : -direction,
                    Distance = 0f,
                };

                return true;
            }

            contact = default;
            return false;
        }
    }

}
