using ProjectGame.HotFix.Gameplay.Navigation;
using System;

namespace ProjectGame.HotFix.Gameplay.Map
{
    public readonly struct NavigationGridCellDebugData
    {
        public readonly RoomSpatialCell StaticCell;
        public readonly bool DynamicBlocked;
        public readonly ushort AdditionalCost;
        public readonly ushort DensityCost;

        public bool IsWalkable => StaticCell == RoomSpatialCell.Floor && !DynamicBlocked;
        public int TotalAdditionalCost => AdditionalCost + DensityCost;

        public NavigationGridCellDebugData(
            RoomSpatialCell staticCell,
            bool dynamicBlocked,
            ushort additionalCost,
            ushort densityCost)
        {
            StaticCell = staticCell;
            DynamicBlocked = dynamicBlocked;
            AdditionalCost = additionalCost;
            DensityCost = densityCost;
        }
    }

    /// <summary>
    /// RoomSpatialData之上的运行时导航层，实现动态避障的关键
    /// 静态地图保持只读，动态阻挡与动态Cost全部维护在这里
    /// </summary>
    public sealed class RuntimeNavigationGrid : INavigationGrid
    {
        private readonly RoomSpatialData _staticData;

        private readonly ushort[] _additionalCost;
        private readonly ushort[] _densityCost;
        private readonly bool[] _dynamicBlocked;

        public int Version { get; private set; }

        public int Width => _staticData.Width;
        public int Height => _staticData.Height;

        public RuntimeNavigationGrid(RoomSpatialData staticData)
        {
            _staticData = staticData ?? throw new ArgumentNullException(nameof(staticData));

            int count = checked(Width * Height);
            _dynamicBlocked = new bool[count];
            _additionalCost = new ushort[count];
            _densityCost = new ushort[count];
        }

        public bool IsWalkable(int x, int y)
        {
            if (!IsValid(x, y))
                return false;

            int index = ToIndex(x, y);

            return _staticData.IsWalkable(x, y) &&
                   !_dynamicBlocked[index];
        }

        public int GetAdditionalCost(int x, int y)
        {
            if (!IsValid(x, y))
                return int.MaxValue;

            int index = ToIndex(x, y);
            return _additionalCost[index] + _densityCost[index];
        }

        public bool TryGetDebugCell(int x, int y, out NavigationGridCellDebugData cell)
        {
            if (!IsValid(x, y))
            {
                cell = default;
                return false;
            }

            int index = ToIndex(x, y);
            cell = new NavigationGridCellDebugData(
                _staticData.Get(x, y),
                _dynamicBlocked[index],
                _additionalCost[index],
                _densityCost[index]);
            return true;
        }

        public bool ReplaceDensityCost(ushort[] source)
        {
            if (source == null || source.Length != _densityCost.Length)
                throw new ArgumentException("DensityCost尺寸与NavigationGrid不一致。", nameof(source));

            bool changed = false;

            for (int i = 0; i < source.Length; i++)
            {
                if (_densityCost[i] == source[i])
                    continue;

                _densityCost[i] = source[i];
                changed = true;
            }

            if (changed)
                Version++;

            return changed;
        }
        public void SetBlocked(int x, int y, bool blocked)
        {
            if (!IsValid(x, y))
                return;

            int index = ToIndex(x, y);

            if (_dynamicBlocked[index] == blocked)
                return;

            _dynamicBlocked[index] = blocked;
            Version++;
        }

        public void SetAdditionalCost(int x, int y, ushort cost)
        {
            if (!IsValid(x, y))
                return;

            int index = ToIndex(x, y);
            if (_additionalCost[index] == cost)
                return;

            _additionalCost[index] = cost;
            Version++;
        }

        public void AddAdditionalCost(int x, int y, ushort cost)
        {
            if (!IsValid(x, y))
                return;

            int index = ToIndex(x, y);
            int value = _additionalCost[index] + cost;
            ushort next = (ushort)Math.Min(value, ushort.MaxValue);

            if (_additionalCost[index] == next)
                return;

            _additionalCost[index] = next;
            Version++;
        }

        public void ClearDynamicBlocked()
        {
            if (!ContainsBlockedCell())
                return;

            Array.Clear(_dynamicBlocked, 0, _dynamicBlocked.Length);
            Version++;
        }

        public void ClearAdditionalCost()
        {
            if (!ContainsNonZero(_additionalCost))
                return;

            Array.Clear(_additionalCost, 0, _additionalCost.Length);
            Version++;
        }

        public void ClearRuntime()
        {
            bool changed = ContainsBlockedCell() ||
                           ContainsNonZero(_additionalCost) ||
                           ContainsNonZero(_densityCost);

            if (!changed)
                return;

            Array.Clear(_dynamicBlocked, 0, _dynamicBlocked.Length);
            Array.Clear(_additionalCost, 0, _additionalCost.Length);
            Array.Clear(_densityCost, 0, _densityCost.Length);
            Version++;
        }

        private bool ContainsBlockedCell()
        {
            for (int i = 0; i < _dynamicBlocked.Length; i++)
            {
                if (_dynamicBlocked[i])
                    return true;
            }

            return false;
        }

        private static bool ContainsNonZero(ushort[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != 0)
                    return true;
            }

            return false;
        }

        private int ToIndex(int x, int y) => y * Width + x;

        private bool IsValid(int x, int y) =>
            (uint)x < (uint)Width &&
            (uint)y < (uint)Height;
    }
}
