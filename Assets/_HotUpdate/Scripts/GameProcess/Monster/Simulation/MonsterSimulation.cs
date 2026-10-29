using ProjectGame.Core.Navigation;
using ProjectGame.HotFix.Gameplay.Map;
using ProjectGame.HotFix.Gameplay.Navigation;
using System;
using System.Collections.Generic;
using Unity.Profiling;
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
        private readonly List<MonsterMeleeAttackRequest> _pendingMeleeAttacks = new(64);

        private int _lastTopologyVersion = -1;
        private int _lastCostVersion = -1;
        private int _lastTopologyReferenceCostVersion = -1;
        //Source变化触发重建的最小间隔
        private const uint SourceRebuildIntervalTicks = 12;
        //Source变化允许的最大过期时间，超过这个时间就强制重建FlowField
        private const uint SourceMaxStaleTicks = 24;
        //Source允许最大的距离变化，超过这个距离就强制重建FlowField
        private const int SourceRebuildDistanceCells = 2;
        //Cost变化触发重建的最小间隔
        private const uint CostRebuildIntervalTicks = 30;

        private static readonly ProfilerMarker TopologyRebuildMarker =
            new("Monster.FlowField.Rebuild.Topology");
        private static readonly ProfilerMarker SourceRebuildMarker =
            new("Monster.FlowField.Rebuild.Source");
        private static readonly ProfilerMarker CostRebuildMarker =
            new("Monster.FlowField.Rebuild.Cost");
        private static readonly ProfilerMarker FlowFieldBuildMarker =
            new("Monster.FlowField.Build");
        private static readonly ProfilerMarker TopologyReferenceBuildMarker =
            new("Monster.FlowField.Build.TopologyReference");
        private static readonly ProfilerMarker AcceptSourcesMarker =
            new("Monster.FlowField.Rebuild.AcceptSources");
        private static readonly ProfilerMarker BuildSourcesMarker =
            new("Monster.FlowField.Rebuild.BuildSources");
        private static readonly ProfilerMarker GetKernelDataMarker =
            new("Monster.FlowField.Build.GetKernelData");
        private static readonly ProfilerMarker SolveMarker =
            new("Monster.FlowField.Build.Solve");
        private static readonly ProfilerMarker DensityUpdateMarker =
            new("Monster.Simulation.DensityUpdate");
        private static readonly ProfilerMarker FlowFieldUpdateMarker =
            new("Monster.Simulation.FlowFieldUpdate");
        private static readonly ProfilerMarker SeparationBuildMarker =
            new("Monster.Simulation.SeparationBuild");
        private static readonly ProfilerMarker EntityTickMarker =
            new("Monster.Simulation.EntityTick");
        // EntityTick 总 Marker 已存在；这些子 Marker 只拆分主要阶段，
        // 不继续细化到单个数学表达式，避免量化时被 Marker 自身污染。
        private static readonly ProfilerMarker EntityTargetMarker =
            new("Monster.Simulation.EntityTick.Target");
        private static readonly ProfilerMarker EntityAttackMarker =
            new("Monster.Simulation.EntityTick.Attack");
        private static readonly ProfilerMarker EntityNavigationMarker =
            new("Monster.Simulation.EntityTick.Navigation");
        private static readonly ProfilerMarker EntitySeparationMarker =
            new("Monster.Simulation.EntityTick.Separation");
        private static readonly ProfilerMarker EntityConstraintMarker =
            new("Monster.Simulation.EntityTick.Constraint");

        private uint _nextSourceRebuildTick;
        private uint _nextCostRebuildTick;

        private readonly MonsterSeparationSystem _separation;
        private readonly RuntimeNavigationGrid _navigationGrid;
        private readonly MonsterNavigationDensitySystem _densitySystem;
        private readonly Transform _roomRoot;

        private readonly MultiSourceFlowField _flowField = new();
        private readonly MultiSourceFlowField _topologyReferenceFlowField = new();
        private readonly List<FlowFieldSource> _flowSources = new(MonsterPlayerTargetBuffer.MaximumCapacity);
        //当前玩家的实际位置
        private readonly Vector2Int[] _currentSourceCells = new Vector2Int[MonsterPlayerTargetBuffer.MaximumCapacity];
        //上一次重建FlowField时的Source列表是否有效，用于判断Source是否发生变化
        private readonly bool[] _currentSourceValid = new bool[MonsterPlayerTargetBuffer.MaximumCapacity];
        //当前FlowField认定的玩家位置
        private readonly Vector2Int[] _acceptedSourceCells = new Vector2Int[MonsterPlayerTargetBuffer.MaximumCapacity];
        //
        private readonly bool[] _acceptedSourceValid = new bool[MonsterPlayerTargetBuffer.MaximumCapacity];

        private int _acceptedSourceCount;

        private bool _sourcePending;
        private uint _sourceDirtySinceTick;
        private bool _hasFlowField;
        private bool _hasTopologyReferenceFlowField;

        public bool HasFlowField => _hasFlowField;
        public int PendingMeleeAttackCount => _pendingMeleeAttacks.Count;
        public int TopologyRebuildCount { get; private set; }
        public int SourceRebuildCount { get; private set; }
        public int CostRebuildCount { get; private set; }
        public uint LastRebuildTick { get; private set; }

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

            _separation = new MonsterSeparationSystem(roomRoot,RoomSpatialData.HalfSize,3f,4f);

            _densitySystem = new MonsterNavigationDensitySystem();
        }

        public void Tick(
            MonsterWorld world,
            MonsterPlayerTargetBuffer players,
            uint tick,
            float deltaTime,
            float groundY,
            MonsterOccupancySnapshot occupancySnapshot)
        {
            if (occupancySnapshot == null)
                throw new ArgumentNullException(nameof(occupancySnapshot));

            _pendingMeleeAttacks.Clear();
            players.UpdateNavigationCells(_roomRoot, _navigationGrid, groundY);

            using (DensityUpdateMarker.Auto())
                _densitySystem.Update(occupancySnapshot, _navigationGrid, tick);

            using (FlowFieldUpdateMarker.Auto())
                UpdateFlowField(players, tick, groundY);

            using (SeparationBuildMarker.Auto())
                _separation.Build(occupancySnapshot);

            using var entityTickScope = EntityTickMarker.Auto();

            for (int activeIndex = 0;
                 activeIndex < occupancySnapshot.ActiveCount;
                 activeIndex++)
            {
                int slot = occupancySnapshot.ActiveSlots[activeIndex];
                Vector2 position = world.Motion[slot].Position;

                bool hasTarget;
                Vector2 targetPosition;
                float targetDistanceSqr;
                Vector2 flowDirection;
                bool isRecovering;
                bool targetWalkable;
                using (EntityTargetMarker.Auto())
                {
                    hasTarget = TryGetFlowTarget(
                        position,
                        occupancySnapshot.FineCellOfSlot[slot],
                        players,
                        out targetPosition,
                        out targetDistanceSqr,
                        out flowDirection,
                        out isRecovering,
                        out targetWalkable);
                }

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

                using (EntityAttackMarker.Auto())
                    TickAttack(world, slot, config, tick, groundY, hasTarget, targetDistanceSqr);

                Vector2 navigationVelocity;
                using (EntityNavigationMarker.Auto())
                {
                    navigationVelocity = CalculateVelocity(
                        world, slot, config, tick,
                        hasTarget,
                        targetPosition,
                        flowDirection,
                        targetWalkable);
                }

                Vector2 velocity = navigationVelocity;

                if (world.Attack[slot].Phase == MonsterAttackPhase.Ready)
                {
                    using (EntitySeparationMarker.Auto())
                        velocity = ApplySeparation(world, slot, config, tick, velocity);
                }
                //进行一次SepartionForce的安全性检查，保证其不会把怪物推进墙中
                using (EntityConstraintMarker.Auto())
                {
                    velocity = ConstrainMovement(
                        world.Motion[slot].Position,
                        navigationVelocity,
                        velocity,
                        deltaTime,
                        groundY);
                }

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

        public bool TryGetTopologyReferenceFlowFieldDebugCell(
            int x,
            int y,
            out FlowFieldCellDebugData cell)
        {
            if (!_hasTopologyReferenceFlowField)
            {
                cell = default;
                return false;
            }

            return _topologyReferenceFlowField.TryGetDebugCell(x, y, out cell);
        }

        public bool IsFlowProgressCorrectionCell(int x, int y)
        {
            if (!_hasFlowField || !_hasTopologyReferenceFlowField ||
                !_flowField.TryGet(x, y, out Vector2 densityDirection, out _) ||
                !_topologyReferenceFlowField.TryGet(
                    x,
                    y,
                    out Vector2 topologyDirection,
                    out _))
            {
                return false;
            }

            return ShouldUseTopologyReferenceDirection(
                densityDirection,
                topologyDirection);
        }

        public Vector2 GetRawSeparation(int slot) => _separation.Get(slot);

        public void ExecutePendingMeleeAttacks(MonsterMeleePhysicsResolver resolver)
        {
            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver));

            for (int i = 0; i < _pendingMeleeAttacks.Count; i++)
            {
                MonsterMeleeAttackRequest request = _pendingMeleeAttacks[i];
                resolver.Execute(request.Origin, request.Direction, request.Profile, request.Damage);
            }

            _pendingMeleeAttacks.Clear();
        }

        /// <summary>
        /// 地块重建入口，每次Tick都会检查是否需要重建FlowField
        /// </summary>
        private void UpdateFlowField(MonsterPlayerTargetBuffer players, uint tick, float groundY)
        {
            CaptureCurrentSourceCells(players, groundY);

            bool sourceChanged = EvaluateSourceChange(
                players.Count,
                out bool sourceStructureChanged,
                out bool sourceMovedFarEnough);

            bool topologyChanged =
                _lastTopologyVersion != _navigationGrid.TopologyVersion;

            bool costChanged =
                _lastCostVersion != _navigationGrid.CostVersion;

            bool topologyReferenceCostChanged =
                _lastTopologyReferenceCostVersion !=
                _navigationGrid.TopologyReferenceCostVersion;

            if (sourceChanged)
            {
                if (!_sourcePending)
                {
                    _sourcePending = true;
                    _sourceDirtySinceTick = tick;
                }
            }
            else
            {
                _sourcePending = false;
            }

            //拓扑变化可能让旧路径失效，立即重建，并顺便消费最新Source
            if (topologyChanged)
            {
                using (TopologyRebuildMarker.Auto())
                    RebuildFlowFieldWithCurrentSources(players.Count, tick, true);
                TopologyRebuildCount++;
                LastRebuildTick = tick;

                return;
            }

            //玩家数量变化、Source有效性变化，不属于普通移动，立即更新
            if (sourceStructureChanged)
            {
                using (SourceRebuildMarker.Auto())
                    RebuildFlowFieldWithCurrentSources(players.Count, tick, true);
                SourceRebuildCount++;
                LastRebuildTick = tick;

                return;
            }

            bool sourceIntervalReached =
                MonsterMath.HasReached(tick, _nextSourceRebuildTick);

            bool sourceMaxStaleReached =
                _sourcePending &&
                MonsterMath.HasReached(
                    tick,
                    _sourceDirtySinceTick + SourceMaxStaleTicks);

            //普通移动：至少移动2格，并且满足刷新间隔；
            //或者变化虽然很小，但已经积压太久
            if (sourceChanged &&
                sourceIntervalReached &&
                (sourceMovedFarEnough || sourceMaxStaleReached))
            {
                using (SourceRebuildMarker.Auto())
                    RebuildFlowFieldWithCurrentSources(players.Count, tick, true);
                SourceRebuildCount++;
                LastRebuildTick = tick;

                return;
            }

            //Cost自己达到刷新窗口时，本来就要Build，
            //顺便把当前最新Source一起提交
            if (costChanged &&
                MonsterMath.HasReached(tick, _nextCostRebuildTick))
            {
                using (CostRebuildMarker.Auto())
                    RebuildFlowFieldWithCurrentSources(
                        players.Count,
                        tick,
                        sourceChanged || topologyReferenceCostChanged);
                CostRebuildCount++;
                LastRebuildTick = tick;
            }
        }

        public void ResetDiagnostics()
        {
            TopologyRebuildCount = 0;
            SourceRebuildCount = 0;
            CostRebuildCount = 0;
            LastRebuildTick = 0;
        }

        private void CaptureCurrentSourceCells(MonsterPlayerTargetBuffer players,float groundY)
        {
            MonsterPlayerTarget[] items = players.Items;

            for (int i = 0; i < players.Count; i++)
            {
                Vector2 position = items[i].Position;
                Vector3 worldPosition =
                    new Vector3(position.x, groundY, position.y);

                if (RoomSpatialUtility.WorldToCell(
                        _roomRoot,
                        worldPosition,
                        out Vector2Int cell))
                {
                    _currentSourceCells[i] = cell;
                    _currentSourceValid[i] = true;
                }
                else
                {
                    _currentSourceValid[i] = false;
                }
            }

            for (int i = players.Count;
                 i < MonsterPlayerTargetBuffer.MaximumCapacity;
                 i++)
            {
                _currentSourceValid[i] = false;
            }
        }
        private bool EvaluateSourceChange(int playerCount,out bool structureChanged,out bool movedFarEnough)
        {
            structureChanged = _acceptedSourceCount != playerCount;
            movedFarEnough = false;

            bool changed = structureChanged;

            int count = Mathf.Min(playerCount, _acceptedSourceCount);

            for (int i = 0; i < count; i++)
            {
                if (_currentSourceValid[i] != _acceptedSourceValid[i])
                {
                    structureChanged = true;
                    changed = true;
                    continue;
                }

                if (!_currentSourceValid[i])
                    continue;

                Vector2Int current = _currentSourceCells[i];
                Vector2Int accepted = _acceptedSourceCells[i];

                int dx = Mathf.Abs(current.x - accepted.x);
                int dy = Mathf.Abs(current.y - accepted.y);

                if (dx == 0 && dy == 0)
                    continue;

                changed = true;

                //八方向Grid使用Chebyshev距离更自然
                if (Mathf.Max(dx, dy) >= SourceRebuildDistanceCells)
                    movedFarEnough = true;
            }

            return changed;
        }
        private void RebuildFlowFieldWithCurrentSources(
            int playerCount,
            uint tick,
            bool rebuildTopologyReference)
        {
            using (AcceptSourcesMarker.Auto())
                AcceptCurrentSources(playerCount);

            using (BuildSourcesMarker.Auto())
                BuildAcceptedFlowSources();

            RebuildFlowField(tick, rebuildTopologyReference);
        }
        /// <summary>
        /// FlowField正式承认玩家Cell位置
        /// </summary>
        private void AcceptCurrentSources(int playerCount)
        {
            int oldCount = _acceptedSourceCount;
            _acceptedSourceCount = playerCount;

            for (int i = 0; i < playerCount; i++)
            {
                _acceptedSourceCells[i] = _currentSourceCells[i];
                _acceptedSourceValid[i] = _currentSourceValid[i];
            }

            for (int i = playerCount; i < oldCount; i++)
                _acceptedSourceValid[i] = false;

            _sourcePending = false;
        }

        private void BuildAcceptedFlowSources()
        {
            _flowSources.Clear();

            for (int i = 0; i < _acceptedSourceCount; i++)
            {
                if (!_acceptedSourceValid[i])
                    continue;

                AddPlayerFlowSources(
                    _acceptedSourceCells[i],
                    i);
            }
        }
        private void RebuildFlowField(uint tick, bool rebuildTopologyReference)
        {
            using (FlowFieldBuildMarker.Auto())
            {
                NavigationGridKernelData kernelData;
                using (GetKernelDataMarker.Auto())
                    kernelData = _navigationGrid.GetKernelData();

                using (SolveMarker.Auto())
                    _hasFlowField = _flowField.Build(kernelData, _flowSources);
            }

            if (rebuildTopologyReference)
            {
                using (TopologyReferenceBuildMarker.Auto())
                {
                    NavigationGridKernelData kernelData =
                        _navigationGrid.GetTopologyReferenceKernelData();
                    _hasTopologyReferenceFlowField =
                        _topologyReferenceFlowField.Build(kernelData, _flowSources);
                }
            }

            _lastTopologyVersion = _navigationGrid.TopologyVersion;
            _lastCostVersion = _navigationGrid.CostVersion;
            if (rebuildTopologyReference)
            {
                _lastTopologyReferenceCostVersion =
                    _navigationGrid.TopologyReferenceCostVersion;
            }

            _nextSourceRebuildTick =
                tick + SourceRebuildIntervalTicks;

            _nextCostRebuildTick =
                tick + CostRebuildIntervalTicks;
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

                _flowSources.Add(new FlowFieldSource(x, y, sourceIndex));
                added = true;
            }

            //极端情况下周围8格全被堵死，至少保留原来的兜底。
            if (!added && TryResolveWalkableCell(playerCell, out GridPoint fallback))
                _flowSources.Add(new FlowFieldSource(fallback.X, fallback.Y, sourceIndex));
        }
        private bool TryGetFlowTarget(
            Vector2 position,
            int cellIndex,
            MonsterPlayerTargetBuffer players,
                out Vector2 targetPosition,
                out float targetDistanceSqr,
                out Vector2 flowDirection,
                out bool isRecovering,
                out bool targetWalkable)
        {
            targetPosition = default;
            targetDistanceSqr = float.MaxValue;
            flowDirection = Vector2.zero;
            isRecovering = false;
            targetWalkable = false;

            if (!_hasFlowField)
                return false;

            if (cellIndex < 0)
                return false;

            int cellWidth = _navigationGrid.Width;
            int cellCount = checked(cellWidth * _navigationGrid.Height);
            if (cellIndex >= cellCount)
                return false;

            Vector2Int cell = new Vector2Int(
                cellIndex % cellWidth,
                cellIndex / cellWidth);

            //当前怪物已经处于非法格：不要读取这个格子的Flow，而是先找附近可达Flow Cell脱困。
            if (!_navigationGrid.IsWalkable(cell.x, cell.y))
            {
                if (!TryResolveReachableFlowCell(cell, out GridPoint recoveryCell, out int recoverySource))
                    return false;

                if ((uint)recoverySource >= (uint)players.Count)
                    return false;

                targetPosition = players.Items[recoverySource].Position;
                targetWalkable = players.IsCellWalkable(recoverySource);
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

            if (_hasTopologyReferenceFlowField &&
                _topologyReferenceFlowField.TryGet(
                    cell.x,
                    cell.y,
                    out Vector2 topologyDirection,
                    out int topologySourceIndex) &&
                ShouldUseTopologyReferenceDirection(
                    localDirection,
                    topologyDirection))
            {
                localDirection = topologyDirection;
                sourceIndex = topologySourceIndex;
            }

            if ((uint)sourceIndex >= (uint)players.Count)
                return false;

            targetPosition = players.Items[sourceIndex].Position;
            targetWalkable = players.IsCellWalkable(sourceIndex);
            targetDistanceSqr = (targetPosition - position).sqrMagnitude;

            if (localDirection.sqrMagnitude > 0.000001f)
            {
                Vector3 worldDirection = _roomRoot.TransformVector(
                    new Vector3(localDirection.x, 0f, localDirection.y));

                flowDirection = new Vector2(worldDirection.x, worldDirection.z).normalized;
            }

            return true;
        }

        private static bool ShouldUseTopologyReferenceDirection(
            Vector2 densityDirection,
            Vector2 topologyDirection)
        {
            return densityDirection.sqrMagnitude > 0.000001f &&
                   topologyDirection.sqrMagnitude > 0.000001f &&
                   Vector2.Dot(densityDirection, topologyDirection) <= 0f;
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
                _pendingMeleeAttacks.Add(new MonsterMeleeAttackRequest(
                    root + rotation * profile.OriginOffset,
                    rotation * Vector3.forward,
                    profile,
                    config.AttackDamage));
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
                bool targetWalkable)
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
                if (!targetWalkable)
                    return Vector2.zero;

                direction = offset.normalized;
            }

            return direction * config.MoveSpeed * world.GetSpeedMultiplier(slot, tick);
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
