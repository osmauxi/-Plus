using ProjectGame.Core.Navigation;
using ProjectGame.HotFix.Gameplay.Map;
using ProjectGame.HotFix.Gameplay.Navigation;
using ProjectGame.HotFix.Gameplay.Player;
using System.Collections.Generic;
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
        private readonly MonsterRuntimeCatalog _configs;
        private readonly MonsterPoseQuantizer _quantizer;

        private readonly RuntimeNavigationGrid _navigationGrid;
        private readonly RuntimeNavigationObstacleSystem _obstacleSystem;
        private readonly MonsterSimulation _simulation;
        private readonly MonsterMeleePhysicsResolver _melee = new();
        private readonly MonsterSpatialIndex _spatialIndex;
        private readonly Transform _roomRoot;

        private readonly MonsterPlayerTargetBuffer _players = new();
        private float _groundY;

        public MonsterWorld World { get; }
        public RuntimeNavigationGrid NavigationGrid => _navigationGrid;
        public MonsterSpatialIndex SpatialIndex => _spatialIndex;
        public Transform RoomRoot => _roomRoot;
        public float GroundY => _groundY;
        public float MonsterHitRadius { get; }
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
            int initialCapacity = 64)
        {
            _configs = configs;
            _quantizer = quantizer;
            _roomRoot = roomRoot;

            _navigationGrid = new RuntimeNavigationGrid(spatialData);
            _obstacleSystem = new RuntimeNavigationObstacleSystem(_navigationGrid, roomRoot);
            _simulation = new MonsterSimulation(configs,attackProfiles,_navigationGrid,roomRoot);

            World = new MonsterWorld(initialCapacity);
            _spatialIndex = new MonsterSpatialIndex(roomRoot, RoomSpatialData.HalfSize, initialCapacity);
            MonsterHitRadius = monsterHitRadius;
        }

        public void BeginRoom(int expectedTotalMonsterCount, float groundY)
        {
            World.Clear();
            World.EnsureCapacity(expectedTotalMonsterCount);
            _players.Clear();
            _groundY = groundY;
            _spatialIndex.Clear();
            _simulation.ResetDiagnostics();
        }

        public MonsterSpawnBatch Spawn(IReadOnlyList<MonsterSpawnPlan> plans)
        {
            int startSlot = World.SlotCount;
            World.EnsureCapacity(startSlot + plans.Count);

            for (int i = 0; i < plans.Count; i++)
            {
                MonsterSpawnPlan plan = plans[i];
                ref readonly MonsterRuntimeConfig config = ref _configs.Get(plan.ConfigIndex);
                int slot = World.Create(plan.ConfigIndex, plan.Position, plan.Yaw, config.MaxHealth);
                _spatialIndex.Add(World, slot);
            }

            return MonsterSpawnBatch.FromPlans(startSlot, plans, _quantizer);
        }

        public void Tick(PlayerManager manager, uint tick, float deltaTime)
        {
            _obstacleSystem.Tick();

            MonsterPlayerTargetCapture.Capture(manager, _players);
            _simulation.Tick(World, _players, tick, deltaTime, _groundY);
            //simulation过后，怪物位置可能发生变化，这里是触发四叉树的检测更新
            //实际更新是在等人真正查询的时候触发，根据标记判定是否先进行更新
            _spatialIndex.Invalidate();
        }

        /// <summary>
        /// 在 PhysicsSync 阶段之后结算本 Tick 由 WorldSimulation 产生的近战请求。
        /// </summary>
        public void ResolveCombat()
        {
            _simulation.ExecutePendingMeleeAttacks(_melee);
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

        public bool DamageMonster(int slot, float damage)
        {
            if (ProjectGame.HotFix.Gameplay.Benchmarking.BenchmarkMovementDriver.SuppressMonsterDamage)
                return World.Meta[slot].IsActive;

            bool damaged = World.Damage(slot, damage);
            if (damaged && !World.IsActive(slot))
                _spatialIndex.Remove(slot);
            return damaged;
        }

        public bool PushMonster(int slot, Vector2 offset)
        {
            if (!World.Push(slot, offset))
                return false;
            _spatialIndex.Relocate(World, slot);
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

        public Vector2 GetRawSeparation(int slot) => _simulation.GetRawSeparation(slot);

        public bool TryGetMonsterNavigationDebug(int slot, out MonsterNavigationDebugData data)
        {
            if (!World.IsActive(slot))
            {
                data = default;
                return false;
            }

            MonsterMotionData motion = World.Motion[slot];
            data = new MonsterNavigationDebugData(
                motion.Position,
                motion.Velocity,
                _simulation.GetRawSeparation(slot));
            return true;
        }

        public void CaptureFrame(uint serverTick, MonsterPresentationFrame destination) =>
            MonsterPresentationPackingSystem.CaptureInto(World, serverTick, _quantizer, destination);

        public void EndRoom()
        {
            World.Clear();
            _players.Clear();
            _spatialIndex.Clear();
            _groundY = 0f;
        }
    }
}
