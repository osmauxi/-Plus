using ProjectGame.Core.Navigation;
using ProjectGame.HotFix.Gameplay.Navigation;
using System;
using UnityEngine;

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

        private static readonly int[] NeighborX = { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static readonly int[] NeighborY = { -1, -1, -1, 0, 0, 1, 1, 1 };

        private readonly byte[] _staticWalkable;
        private readonly byte[] _walkableCache;
        private readonly byte[] _connectionMask;
        private readonly int[] _combinedCost;

        private int _cachedTopologyVersion = -1;
        private int _cachedCostVersion = -1;

        //避障拓扑版本号，每次动态阻挡信息改变时+1
        public int TopologyVersion { get; private set; }
        //避障Cost版本号，每次动态Cost信息改变时+1
        public int CostVersion { get; private set; }

        public int Width => _staticData.Width;
        public int Height => _staticData.Height;

        public RuntimeNavigationGrid(RoomSpatialData staticData)
        {
            _staticData = staticData ?? throw new ArgumentNullException(nameof(staticData));

            int count = checked(Width * Height);

            _dynamicBlocked = new bool[count];
            _additionalCost = new ushort[count];
            _densityCost = new ushort[count];

            _staticWalkable = new byte[count];
            _walkableCache = new byte[count];
            _connectionMask = new byte[count];
            _combinedCost = new int[count];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = y * Width + x;
                    _staticWalkable[index] =
                        _staticData.IsWalkable(x, y) ? (byte)1 : (byte)0;
                }
            }
        }
        public byte[] GetWalkableKernel()
        {
            RefreshTopologyCache();
            return _walkableCache;
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

        public NavigationGridKernelData GetKernelData()
        {
            RefreshTopologyCache();
            RefreshCostCache();

            return new NavigationGridKernelData(
                Width,
                Height,
                _walkableCache,
                _connectionMask,
                _combinedCost);
        }
        private void RefreshTopologyCache()
        {
            if (_cachedTopologyVersion == TopologyVersion)
                return;

            int width = Width;
            int height = Height;
            int count = width * height;

            //先得到当前最终Walkable状态
            for (int i = 0; i < count; i++)
            {
                _walkableCache[i] =
                    _staticWalkable[i] != 0 && !_dynamicBlocked[i]
                        ? (byte)1
                        : (byte)0;
            }

            //将八方向能否移动提前压进一个byte
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int current = y * width + x;

                    if (_walkableCache[current] == 0)
                    {
                        _connectionMask[current] = 0;
                        continue;
                    }

                    byte mask = 0;

                    for (int i = 0; i < 8; i++)
                    {
                        int offsetX = NeighborX[i];
                        int offsetY = NeighborY[i];

                        int nextX = x + offsetX;
                        int nextY = y + offsetY;

                        if ((uint)nextX >= (uint)width ||
                            (uint)nextY >= (uint)height)
                            continue;

                        int next = nextY * width + nextX;

                        if (_walkableCache[next] == 0)
                            continue;

                        //斜向仍然保持原来的禁止穿墙角规则
                        if (offsetX != 0 && offsetY != 0)
                        {
                            int sideX = y * width + nextX;
                            int sideY = nextY * width + x;

                            if (_walkableCache[sideX] == 0 ||
                                _walkableCache[sideY] == 0)
                                continue;
                        }

                        mask |= (byte)(1 << i);
                    }

                    _connectionMask[current] = mask;
                }
            }

            _cachedTopologyVersion = TopologyVersion;
        }
        private void RefreshCostCache()
        {
            if (_cachedCostVersion == CostVersion)
                return;

            for (int i = 0; i < _combinedCost.Length; i++)
                _combinedCost[i] = _additionalCost[i] + _densityCost[i];

            _cachedCostVersion = CostVersion;
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
                CostVersion++;

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
            TopologyVersion++;
        }

        public void SetAdditionalCost(int x, int y, ushort cost)
        {
            if (!IsValid(x, y))
                return;

            int index = ToIndex(x, y);
            if (_additionalCost[index] == cost)
                return;

            _additionalCost[index] = cost;
            CostVersion++;
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
            CostVersion++;
        }

        public void ClearDynamicBlocked()
        {
            if (!ContainsBlockedCell())
                return;

            Array.Clear(_dynamicBlocked, 0, _dynamicBlocked.Length);
            TopologyVersion++;
        }

        public void ClearAdditionalCost()
        {
            if (!ContainsNonZero(_additionalCost))
                return;

            Array.Clear(_additionalCost, 0, _additionalCost.Length);
            CostVersion++;
        }

        public void ClearRuntime()
        {
            bool topologyChanged = ContainsBlockedCell();
            bool costChanged = ContainsNonZero(_additionalCost) || ContainsNonZero(_densityCost);

            if (!topologyChanged && !costChanged)
                return;

            Array.Clear(_dynamicBlocked, 0, _dynamicBlocked.Length);
            Array.Clear(_additionalCost, 0, _additionalCost.Length);
            Array.Clear(_densityCost, 0, _densityCost.Length);
            if (topologyChanged)
                TopologyVersion++;
            if (costChanged)
                CostVersion++;
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
