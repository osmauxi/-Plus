using ProjectGame.HotFix.Gameplay.Map;
using ProjectGame.HotFix.Gameplay.Navigation;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 服务器怪物主循环：索敌、近战状态机、追踪和移动
    /// </summary>
    public sealed class MonsterSimulation
    {
        private readonly MonsterRuntimeCatalog _configs;
        private readonly MonsterAttackCatalog _attackProfiles;
        private readonly MonsterMeleePhysicsResolver _melee = new();

        private int _lastNavigationVersion = -1;
        private readonly MonsterSeparationSystem _separation;
        private readonly RuntimeNavigationGrid _navigationGrid;
        private readonly MonsterNavigationDensitySystem _densitySystem;
        private readonly Transform _roomRoot;

        private readonly MultiSourceFlowField _flowField = new();
        private readonly List<FlowFieldSource> _flowSources = new(MonsterPlayerTargetBuffer.MaximumCapacity);
        private readonly List<FlowFieldSource> _lastFlowSources = new(MonsterPlayerTargetBuffer.MaximumCapacity);

        private bool _hasFlowField;

        public bool HasFlowField => _hasFlowField;

        /// <summary>
        /// 寻路加强，将移动格设为玩家周围八格
        /// </summary>
        private static readonly Vector2Int[] FlowGoalOffsets =
        {
            new(-1, -1), new(0, -1), new(1, -1),
            new(-1,  0),             new(1,  0),
            new(-1,  1), new(0,  1), new(1,  1)
        };

        public MonsterSimulation(
            MonsterRuntimeCatalog configs,
            MonsterAttackCatalog attackProfiles,
            RuntimeNavigationGrid navigationGrid,
            Transform roomRoot)
        {
            _configs = configs ?? throw new ArgumentNullException(nameof(configs));
            _attackProfiles = attackProfiles ?? throw new ArgumentNullException(nameof(attackProfiles));
            _navigationGrid = navigationGrid ?? throw new ArgumentNullException(nameof(navigationGrid));
            _roomRoot = roomRoot ?? throw new ArgumentNullException(nameof(roomRoot));

            _separation = new MonsterSeparationSystem(
                roomRoot,
                RoomSpatialData.HalfSize,
                3f,
                4f);

            _densitySystem = new MonsterNavigationDensitySystem(roomRoot);
        }

        public void Tick(MonsterWorld world, MonsterPlayerTargetBuffer players, uint tick,
            float deltaTime, float groundY)
        {
            _densitySystem.Update(world, _navigationGrid, tick, groundY);
            UpdateFlowField(players, groundY);
            _separation.Build(world);

            for (int slot = 0; slot < world.SlotCount; slot++)
            {
                if (!world.Meta[slot].IsActive)
                    continue;

                bool hasTarget = TryGetFlowTarget(
                    world.Motion[slot].Position,
                    players,
                    groundY,
                    out Vector2 targetPosition,
                    out float targetDistanceSqr,
                    out Vector2 flowDirection,
                    out bool isRecovering);

                ref readonly MonsterRuntimeConfig config = ref _configs.Get(world.Meta[slot].ConfigIndex);

                if (world.IsStunned(slot, tick))
                {
                    world.Motion[slot].Velocity = Vector2.zero;
                    continue;
                }

                //已经被挤进非法格时，优先脱困，不攻击、不参与Separation。
                if (isRecovering)
                {
                    Vector2 recoveryVelocity = flowDirection * config.MoveSpeed * world.GetSpeedMultiplier(slot, tick);
                    Integrate(world, slot, recoveryVelocity, deltaTime);
                    continue;
                }

                TickAttack(world, slot, config, tick, groundY, hasTarget, targetDistanceSqr);

                Vector2 navigationVelocity = CalculateVelocity(
                    world, slot, config, tick,
                    hasTarget, targetPosition, flowDirection, groundY);

                Vector2 velocity = navigationVelocity;

                if (world.Attack[slot].Phase == MonsterAttackPhase.Ready)
                    velocity = ApplySeparation(world, slot, config, tick, velocity);
                //进行一次SepartionForce的安全性检查，保证其不会把怪物推进墙中
                velocity = ConstrainMovement(
                    world.Motion[slot].Position,
                    navigationVelocity,
                    velocity,
                    deltaTime,
                    groundY);

                Integrate(world, slot, velocity, deltaTime);
            }
        }

        public bool TryGetFlowFieldDebugCell(int x, int y, out FlowFieldCellDebugData cell)
        {
            if (!_hasFlowField)
            {
                cell = default;
                return false;
            }

            return _flowField.TryGetDebugCell(x, y, out cell);
        }

        public Vector2 GetRawSeparation(int slot) => _separation.Get(slot);

        private void UpdateFlowField(MonsterPlayerTargetBuffer players, float groundY)
        {
            _flowSources.Clear();

            MonsterPlayerTarget[] items = players.Items;

            for (int i = 0; i < players.Count; i++)
            {
                Vector2 position = items[i].Position;
                Vector3 worldPosition = new Vector3(position.x, groundY, position.y);

                if (!RoomSpatialUtility.WorldToCell(_roomRoot, worldPosition, out Vector2Int cell))
                    continue;

                AddPlayerFlowSources(cell, i);
            }

            bool sourcesChanged = !AreSameSources(_flowSources, _lastFlowSources);

            bool navigationChanged = _lastNavigationVersion != _navigationGrid.Version;

            if (!sourcesChanged && !navigationChanged)
                return;

            _hasFlowField = _flowField.Build(_navigationGrid, _flowSources);

            _lastNavigationVersion =_navigationGrid.Version;
            _lastFlowSources.Clear();

            for (int i = 0; i < _flowSources.Count; i++)
                _lastFlowSources.Add(_flowSources[i]);
        }
        private void AddPlayerFlowSources(Vector2Int playerCell, int sourceIndex)
        {
            bool added = false;

            for (int i = 0; i < FlowGoalOffsets.Length; i++)
            {
                int x = playerCell.x + FlowGoalOffsets[i].x;
                int y = playerCell.y + FlowGoalOffsets[i].y;

                if ((uint)x >= RoomSpatialData.GridSize ||
                    (uint)y >= RoomSpatialData.GridSize ||
                    !_navigationGrid.IsWalkable(x, y))
                    continue;

                _flowSources.Add(new FlowFieldSource(new GridPoint(x, y), sourceIndex));
                added = true;
            }

            //极端情况下周围8格全被堵死，至少保留原来的兜底。
            if (!added && TryResolveWalkableCell(playerCell, out GridPoint fallback))
                _flowSources.Add(new FlowFieldSource(fallback, sourceIndex));
        }
        private static bool AreSameSources(List<FlowFieldSource> a,List<FlowFieldSource> b)
        {
            if (a.Count != b.Count)
                return false;

            for (int i = 0; i < a.Count; i++)
            {
                FlowFieldSource x = a[i];
                FlowFieldSource y = b[i];

                if (x.SourceIndex != y.SourceIndex ||
                    x.Cell.X != y.Cell.X ||
                    x.Cell.Y != y.Cell.Y)
                    return false;
            }

            return true;
        }
        private bool TryGetFlowTarget(
                 Vector2 position,
                MonsterPlayerTargetBuffer players,
                float groundY,
                out Vector2 targetPosition,
                out float targetDistanceSqr,
                out Vector2 flowDirection,
                out bool isRecovering)
        {
            targetPosition = default;
            targetDistanceSqr = float.MaxValue;
            flowDirection = Vector2.zero;
            isRecovering = false;

            if (!_hasFlowField)
                return false;

            Vector3 worldPosition = new Vector3(position.x, groundY, position.y);

            if (!RoomSpatialUtility.WorldToCell(_roomRoot, worldPosition, out Vector2Int cell))
                return false;

            //当前怪物已经处于非法格：不要读取这个格子的Flow，而是先找附近可达Flow Cell脱困。
            if (!_navigationGrid.IsWalkable(cell.x, cell.y))
            {
                if (!TryResolveReachableFlowCell(cell, out GridPoint recoveryCell, out int recoverySource))
                    return false;

                if ((uint)recoverySource >= (uint)players.Count)
                    return false;

                targetPosition = players.Items[recoverySource].Position;
                targetDistanceSqr = (targetPosition - position).sqrMagnitude;

                Vector2 recoveryTarget = CellToWorld(recoveryCell);
                Vector2 recoveryOffset = recoveryTarget - position;

                if (recoveryOffset.sqrMagnitude <= 0.000001f)
                    return false;

                flowDirection = recoveryOffset.normalized;
                isRecovering = true;
                return true;
            }

            if (!_flowField.TryGet(cell.x, cell.y, out Vector2 localDirection, out int sourceIndex))
                return false;

            if ((uint)sourceIndex >= (uint)players.Count)
                return false;

            targetPosition = players.Items[sourceIndex].Position;
            targetDistanceSqr = (targetPosition - position).sqrMagnitude;

            if (localDirection.sqrMagnitude > 0.000001f)
            {
                Vector3 worldDirection = _roomRoot.TransformVector(
                    new Vector3(localDirection.x, 0f, localDirection.y));

                flowDirection = new Vector2(worldDirection.x, worldDirection.z).normalized;
            }

            return true;
        }
        /// <summary>
        /// 找最近的、同时存在有效Flow数据的Walkable Cell。
        /// </summary>
        private bool TryResolveReachableFlowCell(
            Vector2Int source,
            out GridPoint result,
            out int sourceIndex)
        {
            for (int radius = 1; radius < RoomSpatialData.GridSize; radius++)
            {
                int minX = Mathf.Max(0, source.x - radius);
                int maxX = Mathf.Min(RoomSpatialData.GridSize - 1, source.x + radius);
                int minY = Mathf.Max(0, source.y - radius);
                int maxY = Mathf.Min(RoomSpatialData.GridSize - 1, source.y + radius);

                int bestDistanceSqr = int.MaxValue;
                GridPoint best = default;
                int bestSource = -1;
                bool found = false;

                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        if (x != minX && x != maxX && y != minY && y != maxY)
                            continue;

                        if (!_navigationGrid.IsWalkable(x, y))
                            continue;

                        if (!_flowField.TryGet(x, y, out _, out int candidateSource))
                            continue;

                        int dx = x - source.x;
                        int dy = y - source.y;
                        int distanceSqr = dx * dx + dy * dy;

                        if (distanceSqr >= bestDistanceSqr)
                            continue;

                        bestDistanceSqr = distanceSqr;
                        best = new GridPoint(x, y);
                        bestSource = candidateSource;
                        found = true;
                    }
                }

                if (found)
                {
                    result = best;
                    sourceIndex = bestSource;
                    return true;
                }
            }

            result = default;
            sourceIndex = -1;
            return false;
        }

        /// <summary>
        /// 在玩家在阻挡格时，尝试走到玩家周围最近的可达格
        /// </summary>
        private bool TryResolveWalkableCell(Vector2Int source, out GridPoint result)
        {
            if (_navigationGrid.IsWalkable(source.x, source.y))
            {
                result = new GridPoint(source.x, source.y);
                return true;
            }

            for (int radius = 1; radius < RoomSpatialData.GridSize; radius++)
            {
                int minX = Mathf.Max(0, source.x - radius);
                int maxX = Mathf.Min(RoomSpatialData.GridSize - 1, source.x + radius);
                int minY = Mathf.Max(0, source.y - radius);
                int maxY = Mathf.Min(RoomSpatialData.GridSize - 1, source.y + radius);

                int bestDistanceSqr = int.MaxValue;
                GridPoint best = default;
                bool found = false;

                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        if (x != minX && x != maxX && y != minY && y != maxY)
                            continue;

                        if (!_navigationGrid.IsWalkable(x, y))
                            continue;

                        int dx = x - source.x;
                        int dy = y - source.y;
                        int distanceSqr = dx * dx + dy * dy;

                        if (distanceSqr >= bestDistanceSqr)
                            continue;

                        bestDistanceSqr = distanceSqr;
                        best = new GridPoint(x, y);
                        found = true;
                    }
                }

                if (found)
                {
                    result = best;
                    return true;
                }
            }

            result = default;
            return false;
        }

        private Vector2 ConstrainMovement(
            Vector2 position,
            Vector2 navigationVelocity,
            Vector2 desiredVelocity,
            float deltaTime,
            float groundY)
        {
            if (desiredVelocity.sqrMagnitude <= 0.000001f)
                return Vector2.zero;

            //Flow + Separation完整移动合法，直接使用
            if (IsMovementWalkable(position, desiredVelocity, deltaTime, groundY))
                return desiredVelocity;

            //完整方向撞墙，尝试沿Room Grid两个轴滑动
            Vector3 local = _roomRoot.InverseTransformVector(
                new Vector3(desiredVelocity.x, 0f, desiredVelocity.y));

            Vector2 slideX = LocalVelocityToWorld(new Vector3(local.x, 0f, 0f));
            Vector2 slideZ = LocalVelocityToWorld(new Vector3(0f, 0f, local.z));

            bool canX = slideX.sqrMagnitude > 0.000001f &&
                        IsMovementWalkable(position, slideX, deltaTime, groundY);

            bool canZ = slideZ.sqrMagnitude > 0.000001f &&
                        IsMovementWalkable(position, slideZ, deltaTime, groundY);

            if (canX && canZ)
            {
                float xScore = Vector2.Dot(slideX, navigationVelocity);
                float zScore = Vector2.Dot(slideZ, navigationVelocity);

                // 优先保留更符合Flow主方向的那个轴。
                if (Mathf.Abs(xScore - zScore) > 0.000001f)
                    return xScore > zScore ? slideX : slideZ;

                return slideX.sqrMagnitude >= slideZ.sqrMagnitude ? slideX : slideZ;
            }

            if (canX)
                return slideX;

            if (canZ)
                return slideZ;

            //Separation导致所有候选都非法，最后退回纯Flow。
            if (IsMovementWalkable(position, navigationVelocity, deltaTime, groundY))
                return navigationVelocity;

            return Vector2.zero;
        }
        private Vector2 LocalVelocityToWorld(Vector3 localVelocity)
        {
            Vector3 world = _roomRoot.TransformVector(localVelocity);
            return new Vector2(world.x, world.z);
        }

        /// <summary>
        /// 判断一次按此Force走会不会进墙
        /// </summary>
        private bool IsMovementWalkable(
            Vector2 position,
            Vector2 velocity,
            float deltaTime,
            float groundY)
        {
            Vector2 nextPosition = position + velocity * deltaTime;

            Vector3 worldPosition = new Vector3(
                nextPosition.x,
                groundY,
                nextPosition.y);

            return RoomSpatialUtility.WorldToCell(
                       _roomRoot,
                       worldPosition,
                       out Vector2Int cell) &&
                   _navigationGrid.IsWalkable(cell.x, cell.y);
        }
        private static bool FindNearestPlayer(Vector2 position, MonsterPlayerTargetBuffer players,
            out Vector2 targetPosition, out float targetDistanceSqr)
        {
            MonsterPlayerTarget[] items = players.Items;
            targetPosition = default;
            targetDistanceSqr = float.MaxValue;
            int nearest = -1;
            for (int i = 0; i < players.Count; i++)
            {
                float distance = (items[i].Position - position).sqrMagnitude;
                if (distance >= targetDistanceSqr)
                    continue;
                targetDistanceSqr = distance;
                nearest = i;
            }
            if (nearest < 0) return false;
            targetPosition = items[nearest].Position;
            return true;
        }

        private void TickAttack(MonsterWorld world, int slot, in MonsterRuntimeConfig config,
            uint tick, float groundY, bool hasTarget, float targetDistanceSqr)
        {
            ref MonsterAttackData attack = ref world.Attack[slot];
            if (attack.Phase == MonsterAttackPhase.Ready)
            {
                if (!hasTarget || targetDistanceSqr > config.AttackRangeSqr)
                    return;
                attack.Phase = MonsterAttackPhase.Windup;
                attack.PhaseEndTick = tick + config.WindupTicks;
                world.Presentation[slot].AttackSequence++;
                return;
            }

            if (!MonsterMath.HasReached(tick, attack.PhaseEndTick))
                return;

            if (attack.Phase == MonsterAttackPhase.Windup)
            {
                MonsterMotionData motion = world.Motion[slot];
                Quaternion rotation = Quaternion.Euler(0f, motion.Yaw, 0f);
                Vector3 root = new Vector3(motion.Position.x, groundY, motion.Position.y);
                ref readonly MonsterAttackProfile profile = ref _attackProfiles.Get(config.AttackProfileIndex);
                _melee.Execute(root + rotation * profile.OriginOffset,
                    rotation * Vector3.forward, profile, config.AttackDamage);
                attack.Phase = MonsterAttackPhase.Recovery;
                attack.PhaseEndTick = tick + config.RecoveryTicks;
            }
            else
            {
                attack.Phase = MonsterAttackPhase.Ready;
                attack.PhaseEndTick = 0;
            }
        }

        private Vector2 CalculateVelocity(
                MonsterWorld world,
                int slot,
                in MonsterRuntimeConfig config,
                uint tick,
                bool hasTarget,
                Vector2 targetPosition,
                Vector2 flowDirection,
                float groundY)
        {
            if (world.Attack[slot].Phase != MonsterAttackPhase.Ready || !hasTarget)
                return Vector2.zero;

            Vector2 position = world.Motion[slot].Position;
            Vector2 direction = flowDirection;

            if (direction.sqrMagnitude <= 0.000001f)
            {
                Vector2 offset = targetPosition - position;

                // 已经进入攻击范围，不再继续往玩家中心挤。
                if (offset.sqrMagnitude <= config.AttackRangeSqr)
                    return Vector2.zero;

                // 玩家卡进障碍时也不要继续钻墙。
                if (!CanDirectChaseTarget(targetPosition, groundY))
                    return Vector2.zero;

                direction = offset.normalized;
            }

            return direction * config.MoveSpeed * world.GetSpeedMultiplier(slot, tick);
        }

        private bool CanDirectChaseTarget(Vector2 targetPosition, float groundY)
        {
            Vector3 worldTarget = new Vector3(targetPosition.x, groundY, targetPosition.y);

            return RoomSpatialUtility.WorldToCell(_roomRoot, worldTarget, out Vector2Int targetCell) &&
                   _navigationGrid.IsWalkable(targetCell.x, targetCell.y);
        }

        private Vector2 CellToWorld(GridPoint cell)
        {
            Vector3 world = RoomSpatialUtility.CellToWorld(_roomRoot, cell.X, cell.Y);
            return new Vector2(world.x, world.z);
        }


        private Vector2 ApplySeparation(MonsterWorld world, int slot,
    in MonsterRuntimeConfig config, uint tick, Vector2 velocity)
        {
            velocity += _separation.Get(slot);

            float maxSpeed = config.MoveSpeed * world.GetSpeedMultiplier(slot, tick);
            float maxSpeedSqr = maxSpeed * maxSpeed;

            if (velocity.sqrMagnitude > maxSpeedSqr)
                velocity = velocity.normalized * maxSpeed;

            return velocity;
        }

        private static void Integrate(MonsterWorld world, int slot, Vector2 velocity, float deltaTime)
        {
            world.Motion[slot].Velocity = velocity;
            world.Motion[slot].Position += velocity * deltaTime;
            if (velocity.sqrMagnitude > 0.000001f)
                world.Motion[slot].Yaw = Mathf.Atan2(velocity.x, velocity.y) * Mathf.Rad2Deg;
        }
    }
}
