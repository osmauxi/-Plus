using System;
using ProjectGame.Core.Navigation;
using ProjectGame.HotFix.Gameplay.Map;
using ProjectGame.HotFix.Gameplay.Navigation;
using Unity.Profiling;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 统计怪物密度并转换为运行时导航 Cost。
    /// </summary>
    public sealed class MonsterNavigationDensitySystem
    {
        private const int DensityRadiusCells = 2;
        private const int CostPerInfluence = 2;
        private const int MaxDensityCost = 40;
        private const uint UpdateIntervalTicks = 6;
        private const float SmoothFactor = 0.25f;

        private static readonly ProfilerMarker CaptureMarker =
            new("Monster.Density.Capture");
        private static readonly ProfilerMarker KernelMarker =
            new("Monster.Density.Kernel");

        private readonly Transform _roomRoot;
        private readonly DensityKernel _kernel;
        private uint _nextUpdateTick;

        public MonsterNavigationDensitySystem(Transform roomRoot)
        {
            _roomRoot =
                roomRoot ?? throw new ArgumentNullException(nameof(roomRoot));
            _kernel = new DensityKernel(
                DensityRadiusCells,
                CostPerInfluence,
                MaxDensityCost,
                SmoothFactor);
        }

        public bool Update(
            MonsterWorld world,
            RuntimeNavigationGrid grid,
            uint tick,
            float groundY)
        {
            if (!MonsterMath.HasReached(tick, _nextUpdateTick))
                return false;

            _nextUpdateTick = tick + UpdateIntervalTicks;

            int width = grid.Width;
            int height = grid.Height;
            int count = checked(width * height);

            _kernel.Prepare(count);

            using (CaptureMarker.Auto())
            {
                BuildMonsterCounts(
                    world,
                    width,
                    height,
                    groundY,
                    _kernel.MonsterCounts);
            }

            byte[] walkable = grid.GetWalkableKernel();

            using (KernelMarker.Auto())
                _kernel.Build(walkable, width, height);

            return grid.ReplaceDensityCost(_kernel.OutputCost);
        }

        private void BuildMonsterCounts(
            MonsterWorld world,
            int width,
            int height,
            float groundY,
            int[] monsterCounts)
        {
            for (int slot = 0; slot < world.SlotCount; slot++)
            {
                if (!world.Meta[slot].IsActive)
                    continue;

                Vector2 position = world.Motion[slot].Position;
                Vector3 worldPosition =
                    new(position.x, groundY, position.y);

                if (!RoomSpatialUtility.WorldToCell(
                        _roomRoot,
                        worldPosition,
                        out Vector2Int cell))
                    continue;

                if ((uint)cell.x >= (uint)width ||
                    (uint)cell.y >= (uint)height)
                    continue;

                monsterCounts[cell.y * width + cell.x]++;
            }
        }
    }
}
