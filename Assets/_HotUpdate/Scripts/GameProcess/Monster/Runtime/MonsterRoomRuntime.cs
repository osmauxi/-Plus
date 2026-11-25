using ProjectGame.Core.Monsters;
using ProjectGame.Core.Navigation;
using ProjectGame.Core.Spatial;
using ProjectGame.HotFix.Gameplay.Map;
using ProjectGame.HotFix.Gameplay.Navigation;
using ProjectGame.HotFix.Gameplay.Player;
using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    public readonly struct MonsterNavigationDebugData
    {
        public readonly Vector2 Position;
        public readonly Vector2 FinalVelocity;
        public readonly Vector2 RawSeparation;

        public MonsterNavigationDebugData(
            Vector2 position,
            Vector2 finalVelocity,
            Vector2 rawSeparation)
        {
            Position = position;
            FinalVelocity = finalVelocity;
            RawSeparation = rawSeparation;
        }
    }

    /// <summary>
    /// 主要管房间级别的怪物状态、导航环境与模拟逻辑
    /// </summary>
    public sealed class MonsterRoomRuntime
    {
        private static readonly ProfilerMarker OccupancySnapshotBuildMarker =
            new("Monster.OccupancySnapshot.Build");

        private readonly MonsterRuntimeCatalog _configs;
        private readonly MonsterAttackCatalog _attackProfiles;
        private readonly MonsterPoseQuantizer _quantizer;

        private readonly RuntimeNavigationGrid _navigationGrid;
        private readonly RuntimeNavigationObstacleSystem _obstacleSystem;
        private readonly IMonsterSimulationBackend _simulation;
        private readonly MonsterOccupancySnapshot _occupancySnapshot;
        private readonly MonsterMeleePhysicsResolver _melee = new();
        private MonsterSimulationTickOutput _simulationOutput;
        private readonly MonsterSpatialIndex _spatialIndex;
        private readonly MonsterNetworkGroupKernel _networkGroups = new();
        private readonly Transform _roomRoot;
        private MonsterPackedState[] _packedStateByStorageIndex = Array.Empty<MonsterPackedState>();
        private ulong[] _groupEntityIds = Array.Empty<ulong>();

        private readonly MonsterPlayerTargetBuffer _players = new();
        private float _groundY;

        public MonsterWorld World { get; }
        public RuntimeNavigationGrid NavigationGrid => _navigationGrid;
        public MonsterSpatialIndex SpatialIndex => _spatialIndex;
        internal MonsterNetworkGroupKernel NetworkGroups => _networkGroups;
        public Transform RoomRoot => _roomRoot;
        public float GroundY => _groundY;
        public float MonsterHitRadius { get; }
        public MonsterSimulationBackendKind SimulationBackendKind { get; }
        public bool HasFlowField => _simulation.HasFlowField;
        public int TopologyRebuildCount => _simulation.TopologyRebuildCount;
        public int SourceRebuildCount => _simulation.SourceRebuildCount;
        public int CostRebuildCount => _simulation.CostRebuildCount;
        public uint LastFlowFieldRebuildTick => _simulation.LastRebuildTick;

        public MonsterRoomRuntime(
            MonsterRuntimeCatalog configs,
            MonsterAttackCatalog attackProfiles,
            MonsterPoseQuantizer quantizer,
            RoomSpatialData spatialData,
            Transform roomRoot,
            float monsterHitRadius,
            MonsterSimulationBackendKind simulationBackendKind,
            int initialCapacity = 64)
        {
            _configs = configs;
            _attackProfiles = attackProfiles;
            _quantizer = quantizer;
            _roomRoot = roomRoot;
            SimulationBackendKind = simulationBackendKind;

            _navigationGrid = new RuntimeNavigationGrid(spatialData);
            _obstacleSystem = new RuntimeNavigationObstacleSystem(_navigationGrid, roomRoot);
            _simulation = MonsterSimulationBackendFactory.Create(
                simulationBackendKind,
                configs,
                _navigationGrid,
                roomRoot);
            _occupancySnapshot = new MonsterOccupancySnapshot(
                roomRoot,
                _navigationGrid.Width,
                _navigationGrid.Height,
                RoomSpatialData.HalfSize,
                3f,
                initialCapacity);

            World = new MonsterWorld(initialCapacity);
            _spatialIndex = new MonsterSpatialIndex(roomRoot, RoomSpatialData.HalfSize, initialCapacity);
            EnsureMonsterCapacity(World.Capacity);
            MonsterHitRadius = monsterHitRadius;
        }

        public void BeginRoom(int expectedTotalMonsterCount, float groundY)
        {
            World.Clear();
            EnsureMonsterCapacity(expectedTotalMonsterCount);
            _players.Clear();
            _groundY = groundY;
            _occupancySnapshot.Clear();
            _spatialIndex.Clear();
            _networkGroups.Clear();
            _networkGroups.EnsureCapacity(expectedTotalMonsterCount);
            _simulation.ResetDiagnostics();
        }

        public MonsterSpawnBatch Spawn(IReadOnlyList<MonsterSpawnPlan> plans)
        {
            CompactDead();
            int startStorageIndex = World.StorageCount;
            EnsureMonsterCapacity(startStorageIndex + plans.Count);

            for (int i = 0; i < plans.Count; i++)
            {
                MonsterSpawnPlan plan = plans[i];
                ref readonly MonsterRuntimeConfig config = ref _configs.Get(plan.ConfigIndex);
                int storageIndex = World.Create(plan.ConfigIndex, plan.Position, plan.Yaw, config.MaxHealth);
                _spatialIndex.Add(World, storageIndex);
            }

            return MonsterSpawnBatch.FromPlans(startStorageIndex, plans, _quantizer, World);
        }

        public void Tick(PlayerManager manager, uint tick, float deltaTime)
        {
            _obstacleSystem.Tick();

            MonsterPlayerTargetCapture.Capture(manager, _players);
            _players.UpdateNavigationCells(_roomRoot, _navigationGrid, _groundY);
            using (OccupancySnapshotBuildMarker.Auto())
                _occupancySnapshot.Build(World, _groundY);
            MonsterSimulationTickInput input = new(
                World,
                _players,
                _occupancySnapshot,
                tick,
                deltaTime,
                _groundY);
            _simulationOutput = _simulation.Tick(input);
            //simulation过后，怪物位置可能发生变化，这里是触发四叉树的检测更新
            //实际更新是在等人真正查询的时候触发，根据标记判定是否先进行更新
            _spatialIndex.Invalidate();
        }

        /// <summary>
        /// 在 PhysicsSync 阶段之后结算本 Tick 由 WorldSimulation 产生的近战请求。
        /// </summary>
        public void ResolveCombat()
        {
            MonsterSimulationTickOutput output = _simulationOutput;
            _simulationOutput = default;
            for (int i = 0; i < output.MeleeAttackCount; i++)
            {
                MonsterAttackRequestData request = output.MeleeAttackRequests[i];
                MonsterMotionData motion = World.Motion[request.StorageIndex];
                Quaternion rotation = Quaternion.Euler(0f, motion.Yaw, 0f);
                Vector3 root = new(motion.Position.x, _groundY, motion.Position.y);
                ref readonly MonsterAttackProfile profile =
                    ref _attackProfiles.Get(request.AttackProfileIndex);
                _melee.Execute(
                    root + rotation * profile.OriginOffset,
                    rotation * Vector3.forward,
                    profile,
                    request.Damage);
            }
        }

        internal void PackPresentationStates()
        {
            MonsterPresentationPackKernel.PackActive(
                World.ActiveStorageIndices,
                World.AliveCount,
                World.Motion,
                World.Presentation,
                _quantizer.Data,
                _packedStateByStorageIndex);
        }

        /// <summary>
        /// 查询从start到end的扫掠圆形范围内的怪物候选者，子弹模拟不直接访问四叉树
        /// </summary>
        /// <param name="start">子弹开始位置</param>
        /// <param name="end">子弹结束位置</param>
        /// <param name="radius">子弹半径</param>
        /// <param name="results">怪物候选者列表</param>
        /// <returns>候选者数量</returns>
        public int QueryMonsterCandidates(
            Vector2 start,
            Vector2 end,
            float radius,
            int[] results) =>
            _spatialIndex.QuerySweptCircle(World, start, end, radius, results);

        public int QueryMonsterCircleCandidates(
            Vector2 center,
            float radius,
            int[] results) =>
            _spatialIndex.QuerySweptCircle(World, center, center, radius, results);

        public bool DamageMonster(int storageIndex, float damage)
        {
            if (ProjectGame.HotFix.Gameplay.Benchmarking.BenchmarkMovementDriver.SuppressMonsterDamage)
                return World.RecordHit(storageIndex);

            bool damaged = World.Damage(storageIndex, damage);
            if (damaged && !World.IsActive(storageIndex))
                _spatialIndex.Remove(storageIndex);
            return damaged;
        }

        internal bool CompactDead()
        {
            if (!World.CompactDead())
                return false;

            _spatialIndex.Clear();
            _networkGroups.Clear();
            return true;
        }

        public ulong GetMonsterEntityId(int storageIndex) => World.GetEntityId(storageIndex);

        public int ResolveMonsterStorageIndex(ulong entityId) => World.ResolveStorageIndex(entityId);

        public bool PushMonster(int storageIndex, Vector2 offset)
        {
            if (!World.Push(storageIndex, offset))
                return false;
            _spatialIndex.Relocate(World, storageIndex);
            return true;
        }

        public void SetObstacleBlocking(RuntimeNavigationObstacle obstacle, bool blocking) =>
            _obstacleSystem.SetBlocking(obstacle, blocking);

        public bool TryGetFlowFieldDebugCell(int x, int y, out FlowFieldCellDebugData cell) =>
            _simulation.TryGetFlowFieldDebugCell(x, y, out cell);

        public bool TryGetTopologyReferenceFlowFieldDebugCell(
            int x,
            int y,
            out FlowFieldCellDebugData cell) =>
            _simulation.TryGetTopologyReferenceFlowFieldDebugCell(x, y, out cell);

        public bool IsFlowProgressCorrectionCell(int x, int y) =>
            _simulation.IsFlowProgressCorrectionCell(x, y);

        public Vector2 GetRawSeparation(int storageIndex) => _simulation.GetRawSeparation(storageIndex);

        public bool TryGetMonsterNavigationDebug(int storageIndex, out MonsterNavigationDebugData data)
        {
            if (!World.IsActive(storageIndex))
            {
                data = default;
                return false;
            }

            MonsterMotionData motion = World.Motion[storageIndex];
            data = new MonsterNavigationDebugData(
                motion.Position,
                motion.Velocity,
                _simulation.GetRawSeparation(storageIndex));
            return true;
        }

        public void CaptureFrame(uint serverTick, MonsterPresentationFrame destination) =>
            MonsterPresentationPackingSystem.CaptureInto(
                World,
                serverTick,
                World.ActiveStorageIndices,
                World.AliveCount,
                _packedStateByStorageIndex,
                destination);

        internal void RefreshNetworkGroups(uint tick)
        {
            _networkGroups.Refresh(
                World.Motion,
                World.StorageCount,
                _players.Positions,
                _players.Count,
                _spatialIndex.RootCenter,
                _spatialIndex.RootSize * 0.5f,
                tick);
        }

        internal void CaptureGroupFrame(
            int groupIndex,
            uint serverTick,
            MonsterGroupSnapshot destination) =>
            MonsterPresentationPackingSystem.CaptureGroupInto(
                _networkGroups.GetGroupId(groupIndex),
                _networkGroups.GetMembershipRevision(groupIndex),
                serverTick,
                _networkGroups.GetMemberStorageIndices(groupIndex),
                _packedStateByStorageIndex,
                destination);

        internal ReadOnlySpan<ulong> GetGroupMemberEntityIds(int groupIndex)
        {
            int memberCount = _networkGroups.GetMemberCount(groupIndex);
            ReadOnlySpan<int> memberStorageIndices = _networkGroups.GetMemberStorageIndices(groupIndex);
            for (int i = 0; i < memberCount; i++)
                _groupEntityIds[i] = World.GetEntityId(memberStorageIndices[i]);
            return _groupEntityIds.AsSpan(0, memberCount);
        }

        public void EndRoom()
        {
            World.Clear();
            _players.Clear();
            _occupancySnapshot.Clear();
            _spatialIndex.Clear();
            _networkGroups.Clear();
            _groundY = 0f;
        }

        private void EnsureMonsterCapacity(int count)
        {
            World.EnsureCapacity(count);
            int capacity = World.Capacity;
            if (_packedStateByStorageIndex.Length < capacity)
                Array.Resize(ref _packedStateByStorageIndex, capacity);
            _occupancySnapshot.EnsureCapacity(capacity);
            _spatialIndex.EnsureCapacity(capacity);
            _simulation.EnsureCapacity(capacity);
            _networkGroups.EnsureCapacity(capacity);
            if (_groupEntityIds.Length < MonsterNetworkGroupKernel.GroupCapacity)
                Array.Resize(ref _groupEntityIds, MonsterNetworkGroupKernel.GroupCapacity);
        }


    }
}
