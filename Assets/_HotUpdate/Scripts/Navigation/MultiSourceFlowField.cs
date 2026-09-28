using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Navigation
{
    public readonly struct FlowFieldSource
    {
        public readonly GridPoint Cell;
        public readonly int SourceIndex;

        public FlowFieldSource(GridPoint cell, int sourceIndex)
        {
            Cell = cell;
            SourceIndex = sourceIndex;
        }
    }

    public readonly struct FlowFieldCellDebugData
    {
        public readonly int Cost;
        public readonly Vector2 Direction;
        public readonly int SourceIndex;

        public bool IsReachable => Cost != int.MaxValue;
        public bool IsSource => Cost == 0;

        public FlowFieldCellDebugData(int cost, Vector2 direction, int sourceIndex)
        {
            Cost = cost;
            Direction = direction;
            SourceIndex = sourceIndex;
        }
    }

    /// <summary>
    /// 流式寻路，贴合怪物追踪最近玩家的逻辑，多个玩家作为0点，
    /// 开始Dijkstra向外扩散并得到当前格子的成本和到达玩家格的方向
    /// 所有怪物不再走单独A*，直接取所在格算出来的方向
    /// </summary>
    public sealed class MultiSourceFlowField
    {
        private static readonly int[] NeighborX = { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static readonly int[] NeighborY = { -1, -1, -1, 0, 0, 1, 1, 1 };
        
        /// <summary>
        /// 每个对应Cell到最近可达玩家的最低路径成本
        /// </summary>
        private int[] _costs = Array.Empty<int>();
        /// <summary>
        /// Monster使用的流式场，存Grid的方向
        /// </summary>
        private Vector2[] _directions = Array.Empty<Vector2>();
        /// <summary>
        /// 这个Cell最终属于哪个玩家
        /// </summary>
        private int[] _sourceIndices = Array.Empty<int>();

        private readonly IntCostMinHeap _open = new();

        private int _width;
        private int _height;

        public bool Build(INavigationGrid grid, IReadOnlyList<FlowFieldSource> sources)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (sources == null) throw new ArgumentNullException(nameof(sources));

            _width = grid.Width;
            _height = grid.Height;

            int count = checked(_width * _height);
            EnsureCapacity(count);

            Array.Fill(_costs, int.MaxValue, 0, count);
            Array.Fill(_sourceIndices, -1, 0, count);
            Array.Clear(_directions, 0, count);

            _open.Clear();

            if (!AddSources(grid, sources))
                return false;

            BuildIntegrationField(grid);
            BuildDirectionField(grid);

            return true;
        }
        /// <summary>
        /// Monster读取方向方法
        /// </summary>
        public bool TryGet(int x, int y, out Vector2 direction, out int sourceIndex)
        {
            if ((uint)x >= (uint)_width || (uint)y >= (uint)_height)
            {
                direction = default;
                sourceIndex = -1;
                return false;
            }

            int index = ToIndex(x, y);

            if (_costs[index] == int.MaxValue)
            {
                direction = default;
                sourceIndex = -1;
                return false;
            }

            direction = _directions[index];
            sourceIndex = _sourceIndices[index];
            return true;
        }

        public int GetCost(int x, int y)
        {
            if ((uint)x >= (uint)_width || (uint)y >= (uint)_height)
                return int.MaxValue;

            return _costs[ToIndex(x, y)];
        }

        public bool TryGetDebugCell(int x, int y, out FlowFieldCellDebugData cell)
        {
            if (!IsValid(x, y))
            {
                cell = default;
                return false;
            }

            int index = ToIndex(x, y);
            cell = new FlowFieldCellDebugData(
                _costs[index],
                _directions[index],
                _sourceIndices[index]);
            return true;
        }

        private bool AddSources(IAStarGrid grid, IReadOnlyList<FlowFieldSource> sources)
        {
            bool added = false;

            for (int i = 0; i < sources.Count; i++)
            {
                FlowFieldSource source = sources[i];
                int x = source.Cell.X;
                int y = source.Cell.Y;

                if (!IsValid(x, y) || !grid.IsWalkable(x, y))
                    continue;

                int index = ToIndex(x, y);

                if (_costs[index] == 0)
                    continue;

                _costs[index] = 0;
                _sourceIndices[index] = source.SourceIndex;

                _open.Push(index, 0);

                added = true;
            }

            return added;
        }

        private void BuildIntegrationField(INavigationGrid grid)
        {
            while (_open.Count > 0)
            {
                //从未扩散完的Cell拿到Cost最低的
                HeapNode node = _open.Pop();

                int current = node.Index;

                //这个节点已经被更短的路径更新过了，旧Heap数据直接丢弃
                //因为小的肯定先出先算，所以后来的不相等肯定是更大的
                if(node.Cost != _costs[current])
                    continue;

                int currentX = current % _width;
                int currentY = current / _width;
                //检查八向邻居
                for (int i = 0; i < NeighborX.Length; i++)
                {
                    int offsetX = NeighborX[i];
                    int offsetY = NeighborY[i];

                    int nextX = currentX + offsetX;
                    int nextY = currentY + offsetY;
                    //过滤越界，过滤阻挡格
                    if (!IsValid(nextX, nextY) || !grid.IsWalkable(nextX, nextY))
                        continue;
                    //过滤斜走穿墙角
                    if (!CanMoveDiagonal(grid, currentX, currentY, offsetX, offsetY))
                        continue;
                    //算新成本，如果更小就更新
                    int next = ToIndex(nextX, nextY);
                    int moveCost = offsetX != 0 && offsetY != 0 ? 14 : 10;
                    // Cell 权重表示进入目标格需要支付的额外代价。
                    int newCost = node.Cost + moveCost + grid.GetAdditionalCost(nextX, nextY);

                    if (newCost >= _costs[next])
                        continue;

                    _costs[next] = newCost;
                    _sourceIndices[next] = _sourceIndices[current];

                    _open.Push(next, newCost);
                }
            }
        }

        /// <summary>
        /// Integration把权重算出来，在这里把权重换成箭头方向，指向周围八格权重最小的
        /// </summary>
        private void BuildDirectionField(IAStarGrid grid)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int current = ToIndex(x, y);
                    int currentCost = _costs[current];

                    if (currentCost == int.MaxValue || currentCost == 0)
                    {
                        _directions[current] = Vector2.zero;
                        continue;
                    }

                    int bestCost = currentCost;
                    int bestOffsetX = 0;
                    int bestOffsetY = 0;

                    for (int i = 0; i < NeighborX.Length; i++)
                    {
                        int offsetX = NeighborX[i];
                        int offsetY = NeighborY[i];

                        int nextX = x + offsetX;
                        int nextY = y + offsetY;

                        if (!IsValid(nextX, nextY) || !grid.IsWalkable(nextX, nextY))
                            continue;

                        if (!CanMoveDiagonal(grid, x, y, offsetX, offsetY))
                            continue;

                        int nextCost = _costs[ToIndex(nextX, nextY)];

                        if (nextCost >= bestCost)
                            continue;

                        bestCost = nextCost;
                        bestOffsetX = offsetX;
                        bestOffsetY = offsetY;
                    }

                    _directions[current] = new Vector2(bestOffsetX, bestOffsetY).normalized;
                }
            }
        }

        private bool CanMoveDiagonal(IAStarGrid grid, int x, int y, int offsetX, int offsetY)
        {
            if (offsetX == 0 || offsetY == 0)
                return true;

            return grid.IsWalkable(x + offsetX, y) &&
                   grid.IsWalkable(x, y + offsetY);
        }

        private void EnsureCapacity(int count)
        {
            if (_costs.Length >= count)
                return;

            _costs = new int[count];
            _directions = new Vector2[count];
            _sourceIndices = new int[count];
        }

        private int ToIndex(int x, int y) => y * _width + x;
        private bool IsValid(int x, int y) => (uint)x < (uint)_width && (uint)y < (uint)_height;
        private readonly struct HeapNode
        {
            public readonly int Index;
            public readonly int Cost;

            public HeapNode(int index, int cost)
            {
                Index = index;
                Cost = cost;
            }
        }
        /// <summary>
        /// 手动小顶堆
        /// </summary>
        private sealed class IntCostMinHeap
        {
            private HeapNode[] _items = new HeapNode[64];
            private int _count;

            public int Count => _count;

            public void Clear() => _count = 0;

            public void Push(int index, int cost)
            {
                EnsureCapacity(_count + 1);

                int child = _count++;
                _items[child] = new HeapNode(index, cost);

                while (child > 0)
                {
                    int parent = (child - 1) >> 1;

                    if (_items[parent].Cost <= cost)
                        break;

                    _items[child] = _items[parent];
                    child = parent;
                }

                _items[child] = new HeapNode(index, cost);
            }

            public HeapNode Pop()
            {
                HeapNode result = _items[0];
                HeapNode last = _items[--_count];

                if (_count == 0)
                    return result;

                int parent = 0;

                while (true)
                {
                    int left = parent * 2 + 1;

                    if (left >= _count)
                        break;

                    int right = left + 1;
                    int child = right < _count && _items[right].Cost < _items[left].Cost ? right : left;

                    if (_items[child].Cost >= last.Cost)
                        break;

                    _items[parent] = _items[child];
                    parent = child;
                }

                _items[parent] = last;
                return result;
            }

            private void EnsureCapacity(int count)
            {
                if (_items.Length >= count)
                    return;

                Array.Resize(ref _items, _items.Length * 2);
            }
        }
    }
}
