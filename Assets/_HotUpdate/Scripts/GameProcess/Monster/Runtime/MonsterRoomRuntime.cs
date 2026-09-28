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
    /// 一个房间内的服务器怪物状态、导航环境与模拟入口
    /// </summary>
    public sealed class MonsterRoomRuntime
    {
        private readonly MonsterRuntimeCatalog _configs;
        private readonly MonsterPoseQuantizer _quantizer;

        private readonly RuntimeNavigationGrid _navigationGrid;
        private readonly RuntimeNavigationObstacleSystem _obstacleSystem;
        private readonly MonsterSimulation _simulation;
        private readonly Transform _roomRoot;

        private readonly MonsterPlayerTargetBuffer _players = new();
        private float _groundY;

        public MonsterWorld World { get; }
        public RuntimeNavigationGrid NavigationGrid => _navigationGrid;
        public Transform RoomRoot => _roomRoot;
        public float GroundY => _groundY;
        public bool HasFlowField => _simulation.HasFlowField;

        public MonsterRoomRuntime(
            MonsterRuntimeCatalog configs,
            MonsterAttackCatalog attackProfiles,
            MonsterPoseQuantizer quantizer,
            RoomSpatialData spatialData,
            Transform roomRoot,
            int initialCapacity = 64)
        {
            _configs = configs;
            _quantizer = quantizer;
            _roomRoot = roomRoot;

            _navigationGrid = new RuntimeNavigationGrid(spatialData);
            _obstacleSystem = new RuntimeNavigationObstacleSystem(_navigationGrid, roomRoot);
            _simulation = new MonsterSimulation(configs, attackProfiles, _navigationGrid, roomRoot);

            World = new MonsterWorld(initialCapacity);
        }

        public void BeginRoom(int expectedTotalMonsterCount, float groundY)
        {
            World.Clear();
            World.EnsureCapacity(expectedTotalMonsterCount);
            _players.Clear();
            _groundY = groundY;
        }

        public MonsterSpawnBatch Spawn(IReadOnlyList<MonsterSpawnPlan> plans)
        {
            int startSlot = World.SlotCount;
            World.EnsureCapacity(startSlot + plans.Count);

            for (int i = 0; i < plans.Count; i++)
            {
                MonsterSpawnPlan plan = plans[i];
                ref readonly MonsterRuntimeConfig config = ref _configs.Get(plan.ConfigIndex);
                World.Create(plan.ConfigIndex, plan.Position, plan.Yaw, config.MaxHealth);
            }

            return MonsterSpawnBatch.FromPlans(startSlot, plans, _quantizer);
        }

        public void Tick(PlayerManager manager, uint tick, float deltaTime)
        {
            _obstacleSystem.Tick();

            MonsterPlayerTargetCapture.Capture(manager, _players);
            _simulation.Tick(World, _players, tick, deltaTime, _groundY);
        }

        public void SetObstacleBlocking(RuntimeNavigationObstacle obstacle, bool blocking) =>
            _obstacleSystem.SetBlocking(obstacle, blocking);

        public bool TryGetFlowFieldDebugCell(int x, int y, out FlowFieldCellDebugData cell) =>
            _simulation.TryGetFlowFieldDebugCell(x, y, out cell);

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

        public MonsterPresentationFrame CaptureFrame(uint serverTick) =>
            MonsterPresentationPackingSystem.Capture(World, serverTick, _quantizer);

        public void EndRoom()
        {
            World.Clear();
            _players.Clear();
            _groundY = 0f;
        }
    }
}
