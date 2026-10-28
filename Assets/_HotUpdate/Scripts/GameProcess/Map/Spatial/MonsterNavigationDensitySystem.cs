using System;
using ProjectGame.Core.Navigation;
using Unity.Profiling;

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

        private readonly DensityKernel _kernel;
        private static readonly ProfilerMarker KernelMarker =
            new("Monster.Density.Kernel");
        private uint _nextUpdateTick;

        public MonsterNavigationDensitySystem()
        {
            _kernel = new DensityKernel(
                DensityRadiusCells,
                CostPerInfluence,
                MaxDensityCost,
                SmoothFactor);
        }

        public bool Update(
            MonsterOccupancySnapshot snapshot,
            RuntimeNavigationGrid grid,
            uint tick)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            if (!MonsterMath.HasReached(tick, _nextUpdateTick))
                return false;

            _nextUpdateTick = tick + UpdateIntervalTicks;

            int width = grid.Width;
            int height = grid.Height;
            int count = checked(width * height);

            if (snapshot.FineWidth != width || snapshot.FineHeight != height)
                throw new InvalidOperationException(
                    $"Density 快照尺寸不一致：Snapshot={snapshot.FineWidth}x{snapshot.FineHeight}，Grid={width}x{height}。");

            _kernel.Prepare(count);

            byte[] walkable = grid.GetWalkableKernel();

            using (KernelMarker.Auto())
                _kernel.Build(snapshot.FineCellCounts, walkable, width, height);

            return grid.ReplaceDensityCost(_kernel.OutputCost);
        }
    }
}
