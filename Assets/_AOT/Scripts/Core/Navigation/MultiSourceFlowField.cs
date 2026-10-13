using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace ProjectGame.Core.Navigation
{
    /// <summary>
    /// 流式寻路，贴合怪物追踪最近玩家的逻辑，多个玩家作为0点，
    /// 开始Dijkstra向外扩散并得到当前格子的成本和到达玩家格的方向
    /// 所有怪物不再走单独A*，直接取所在格算出来的方向
    /// </summary>
    public sealed class MultiSourceFlowField
    {
        private static readonly int[] MoveCost =
        {
            14, 10, 14,
            10,     10,
            14, 10, 14
        };

        private readonly int[] _neighborOffsets = new int[8];
        private int _neighborOffsetWidth = -1;

        private static readonly ProfilerMarker EnsureCapacityMarker =
            new("Monster.FlowField.Build.EnsureCapacity");
        private static readonly ProfilerMarker ResetBuffersMarker =
            new("Monster.FlowField.Build.ResetBuffers");
        private static readonly ProfilerMarker AddSourcesMarker =
            new("Monster.FlowField.Build.AddSources");
        private static readonly ProfilerMarker IntegrationFieldMarker =
            new("Monster.FlowField.Build.IntegrationField");

        private const float Diagonal = 0.70710678f;

        private static readonly Vector2[] DirectionToParent =
        {
            new( Diagonal,  Diagonal),
            new(0f,         1f),
            new(-Diagonal,  Diagonal),
            new(1f,         0f),
            new(-1f,        0f),
            new( Diagonal, -Diagonal),
            new(0f,        -1f),
            new(-Diagonal, -Diagonal)
        };

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

        public bool Build(NavigationGridKernelData grid, IReadOnlyList<FlowFieldSource> sources)
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));

            _width = grid.Width;
            _height = grid.Height;

            EnsureNeighborOffsets(_width);

            int count = checked(_width * _height);
            using (EnsureCapacityMarker.Auto())
                EnsureCapacity(count);

            using (ResetBuffersMarker.Auto())
            {
                Array.Fill(_costs, int.MaxValue, 0, count);
                Array.Fill(_sourceIndices, -1, 0, count);
                Array.Clear(_directions, 0, count);

                _open.Clear();
            }

            bool hasSources;
            using (AddSourcesMarker.Auto())
                hasSources = AddSources(grid, sources);

            if (!hasSources)
                return false;

            using (IntegrationFieldMarker.Auto())
                BuildIntegrationField(grid);

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

        private bool AddSources(
            NavigationGridKernelData grid,
            IReadOnlyList<FlowFieldSource> sources)
        {
            bool added = false;
            byte[] walkable = grid.Walkable;

            for (int i = 0; i < sources.Count; i++)
            {
                FlowFieldSource source = sources[i];
                int x = source.X;
                int y = source.Y;

                if ((uint)x >= (uint)_width ||
                    (uint)y >= (uint)_height)
                    continue;

                int index = y * _width + x;

                if (walkable[index] == 0)
                    continue;

                if (_costs[index] == 0)
                    continue;

                _costs[index] = 0;
                _sourceIndices[index] = source.SourceIndex;

                _open.Push(index, 0);
                added = true;
            }

            return added;
        }

        private void EnsureNeighborOffsets(int width)
        {
            if (_neighborOffsetWidth == width)
                return;

            _neighborOffsets[0] = -width - 1;
            _neighborOffsets[1] = -width;
            _neighborOffsets[2] = -width + 1;
            _neighborOffsets[3] = -1;
            _neighborOffsets[4] = 1;
            _neighborOffsets[5] = width - 1;
            _neighborOffsets[6] = width;
            _neighborOffsets[7] = width + 1;

            _neighborOffsetWidth = width;
        }

        private void BuildIntegrationField(NavigationGridKernelData grid)
        {
            byte[] connectionMask = grid.ConnectionMask;
            int[] traversalCost = grid.TraversalCost;

            int[] costs = _costs;
            int[] sourceIndices = _sourceIndices;
            Vector2[] directions = _directions;
            int[] neighborOffsets = _neighborOffsets;

            while (_open.Count > 0)
            {
                HeapNode node = _open.Pop();

                int current = node.Index;

                if (node.Cost != costs[current])
                    continue;

                byte mask = connectionMask[current];

                for (int i = 0; i < 8; i++)
                {
                    if ((mask & (1 << i)) == 0)
                        continue;

                    int next = current + neighborOffsets[i];

                    int newCost =
                        node.Cost +
                        MoveCost[i] +
                        traversalCost[next];

                    if (newCost >= costs[next])
                        continue;

                    costs[next] = newCost;
                    sourceIndices[next] = sourceIndices[current];
                    directions[next] = DirectionToParent[i];

                    _open.Push(next, newCost);
                }
            }
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
        private bool IsValid(int x, int y) =>
            (uint)x < (uint)_width && (uint)y < (uint)_height;

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
                    int child = right < _count &&
                                _items[right].Cost < _items[left].Cost
                        ? right
                        : left;

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
