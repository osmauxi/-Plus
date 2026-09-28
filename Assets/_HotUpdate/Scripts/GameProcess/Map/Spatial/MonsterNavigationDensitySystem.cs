using System;
using ProjectGame.HotFix.Gameplay.Map;
using ProjectGame.HotFix.Gameplay.Navigation;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 低频统计怪物密度，并转换为NavigationGrid的软Cost
    /// </summary>
    public sealed class MonsterNavigationDensitySystem
    {
        private const int DensityRadiusCells = 2;
        private const int CostPerInfluence = 2;
        private const int MaxDensityCost = 40;
        private const uint UpdateIntervalTicks = 6;
        private const float SmoothFactor = 0.25f;

        private readonly Transform _roomRoot;

        private int[] _influence = Array.Empty<int>();
        private float[] _smoothedCost = Array.Empty<float>();
        private ushort[] _outputCost = Array.Empty<ushort>();

        private uint _nextUpdateTick;

        public MonsterNavigationDensitySystem(Transform roomRoot)
        {
            _roomRoot = roomRoot ?? throw new ArgumentNullException(nameof(roomRoot));
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

            int count = checked(grid.Width * grid.Height);
            EnsureCapacity(count);
            Array.Clear(_influence, 0, count);

            BuildInfluence(world, grid, groundY);
            BuildCost(grid, count);

            return grid.ReplaceDensityCost(_outputCost);
        }

        private void BuildInfluence(
            MonsterWorld world,
            RuntimeNavigationGrid grid,
            float groundY)
        {
            for (int slot = 0; slot < world.SlotCount; slot++)
            {
                if (!world.Meta[slot].IsActive)
                    continue;

                Vector2 position = world.Motion[slot].Position;
                Vector3 worldPosition = new Vector3(position.x, groundY, position.y);

                if (!RoomSpatialUtility.WorldToCell(
                        _roomRoot, worldPosition, out Vector2Int cell))
                    continue;

                AddInfluence(grid, cell.x, cell.y);
            }
        }

        private void AddInfluence(RuntimeNavigationGrid grid, int centerX, int centerY)
        {
            for (int y = -DensityRadiusCells; y <= DensityRadiusCells; y++)
            {
                for (int x = -DensityRadiusCells; x <= DensityRadiusCells; x++)
                {
                    int targetX = centerX + x;
                    int targetY = centerY + y;

                    if ((uint)targetX >= (uint)grid.Width ||
                        (uint)targetY >= (uint)grid.Height ||
                        !grid.IsWalkable(targetX, targetY))
                        continue;

                    int distance = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                    int weight = DensityRadiusCells + 1 - distance;

                    _influence[targetY * grid.Width + targetX] += weight;
                }
            }
        }

        private void BuildCost(RuntimeNavigationGrid grid, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int targetCost = Mathf.Min(
                    _influence[i] * CostPerInfluence,
                    MaxDensityCost);

                _smoothedCost[i] = Mathf.Lerp(
                    _smoothedCost[i],
                    targetCost,
                    SmoothFactor);

                _outputCost[i] = (ushort)Mathf.RoundToInt(_smoothedCost[i]);
            }
        }

        private void EnsureCapacity(int count)
        {
            if (_influence.Length == count)
                return;

            _influence = new int[count];
            _smoothedCost = new float[count];
            _outputCost = new ushort[count];
        }
    }
}