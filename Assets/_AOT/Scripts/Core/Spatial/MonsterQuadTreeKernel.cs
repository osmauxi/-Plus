using System;
using UnityEngine;

namespace ProjectGame.Core.Spatial
{
    public readonly struct MonsterQuadTreeNodeData
    {
        public readonly Vector2 Center;
        public readonly float Size;
        public readonly byte Depth;
        public readonly int MonsterCount;
        public readonly bool IsLeaf;
        public readonly uint MortonCode;

        public MonsterQuadTreeNodeData(
            Vector2 center,
            float size,
            byte depth,
            int monsterCount,
            bool isLeaf,
            uint mortonCode)
        {
            Center = center;
            Size = size;
            Depth = depth;
            MonsterCount = monsterCount;
            IsLeaf = isLeaf;
            MortonCode = mortonCode;
        }
    }

    /// <summary>纯数据四叉树内核；房间生命周期与 MonsterWorld 适配保留在 HotFix。</summary>
    public sealed class MonsterQuadTreeKernel
    {
        public const int BaseDepth = 4;
        public const int MaxDepth = 6;
        public const int SplitThreshold = 16;

        private const int Unlinked = -1;
        private const int OverflowLeaf = -2;

        private struct Node
        {
            public Vector2 Center;
            public float HalfSize;
            public int FirstChild;
            public int HeadSlot;
            public int Count;
            public byte Depth;
            public uint MortonCode;
        }

        private readonly Vector2 _rootCenter;
        private readonly float _rootHalfSize;
        private readonly Node[] _nodes;
        private readonly int[] _queryStack;
        private Vector2[] _positions = Array.Empty<Vector2>();
        private int[] _nextSlot;
        private int[] _previousSlot;
        private int[] _leafBySlot;
        private int _nodeCount;
        private int _overflowHead = Unlinked;

        public int NodeCount => _nodeCount;
        public int NodeCapacity => _nodes.Length;
        public Vector2 RootCenter => _rootCenter;
        public float RootSize => _rootHalfSize * 2f;
        public int LastQueryCandidateCount { get; private set; }
        public int LastQueryVisitedNodeCount { get; private set; }

        public MonsterQuadTreeKernel(
            Vector2 rootCenter,
            float rootHalfSize,
            int initialMonsterCapacity)
        {
            _rootCenter = rootCenter;
            _rootHalfSize = rootHalfSize;
            int maximumNodeCount = FullTreeNodeCount(MaxDepth);
            _nodes = new Node[maximumNodeCount];
            _queryStack = new int[maximumNodeCount];
            _nextSlot = new int[initialMonsterCapacity];
            _previousSlot = new int[initialMonsterCapacity];
            _leafBySlot = new int[initialMonsterCapacity];
            FillUnlinked(_nextSlot, 0);
            FillUnlinked(_previousSlot, 0);
            FillUnlinked(_leafBySlot, 0);
        }

        public void SetPositionBuffer(Vector2[] positions) => _positions = positions;

        public void Clear()
        {
            _nodeCount = 0;
            _overflowHead = Unlinked;
            LastQueryCandidateCount = 0;
            LastQueryVisitedNodeCount = 0;
        }

        public void Build(Vector2[] positions, byte[] activeMask, int slotCount)
        {
            _positions = positions;
            EnsureSlotCapacity(slotCount);
            FillUnlinked(_nextSlot, 0);
            FillUnlinked(_previousSlot, 0);
            FillUnlinked(_leafBySlot, 0);
            _nodeCount = 0;
            _overflowHead = Unlinked;

            int root = CreateNode(_rootCenter, _rootHalfSize, 0, 0);
            BuildBaseTree(root);
            for (int slot = 0; slot < slotCount; slot++)
            {
                if (activeMask[slot] != 0)
                    Insert(slot, positions[slot], 0);
            }
        }

        public void Add(int slot, Vector2 position)
        {
            EnsureSlotCapacity(slot + 1);
            _positions[slot] = position;
            Insert(slot, position, 0);
        }

        public void Remove(int slot)
        {
            int leaf = _leafBySlot[slot];
            if (leaf == Unlinked)
                return;

            Unlink(slot, leaf);
        }

        public void Relocate(int slot, Vector2 position)
        {
            _positions[slot] = position;
            Remove(slot);
            Insert(slot, position, 0);
        }

        public int QuerySweptCircle(
            Vector2 start,
            Vector2 end,
            float radius,
            int[] results)
        {
            int resultCount = 0;
            int visitedNodeCount = 0;
            int stackCount = 1;
            _queryStack[0] = 0;

            while (stackCount > 0)
            {
                int nodeIndex = _queryStack[--stackCount];
                ref Node node = ref _nodes[nodeIndex];
                visitedNodeCount++;

                if (!SegmentIntersectsExpandedNode(start, end, radius, node))
                    continue;

                if (node.FirstChild >= 0)
                {
                    _queryStack[stackCount++] = node.FirstChild;
                    _queryStack[stackCount++] = node.FirstChild + 1;
                    _queryStack[stackCount++] = node.FirstChild + 2;
                    _queryStack[stackCount++] = node.FirstChild + 3;
                    continue;
                }

                for (int slot = node.HeadSlot; slot >= 0; slot = _nextSlot[slot])
                    results[resultCount++] = slot;
            }

            for (int slot = _overflowHead; slot >= 0; slot = _nextSlot[slot])
                results[resultCount++] = slot;

            LastQueryCandidateCount = resultCount;
            LastQueryVisitedNodeCount = visitedNodeCount;
            return resultCount;
        }

        public bool TryGetNode(int nodeIndex, out MonsterQuadTreeNodeData data)
        {
            if ((uint)nodeIndex >= (uint)_nodeCount)
            {
                data = default;
                return false;
            }

            ref Node node = ref _nodes[nodeIndex];
            data = new MonsterQuadTreeNodeData(
                node.Center,
                node.HalfSize * 2f,
                node.Depth,
                node.Count,
                node.FirstChild < 0,
                node.MortonCode);
            return true;
        }

        private void BuildBaseTree(int nodeIndex)
        {
            if (_nodes[nodeIndex].Depth >= BaseDepth)
                return;

            int firstChild = CreateChildren(nodeIndex);
            BuildBaseTree(firstChild);
            BuildBaseTree(firstChild + 1);
            BuildBaseTree(firstChild + 2);
            BuildBaseTree(firstChild + 3);
        }

        private int CreateNode(Vector2 center, float halfSize, byte depth, uint mortonCode)
        {
            int index = _nodeCount++;
            _nodes[index] = new Node
            {
                Center = center,
                HalfSize = halfSize,
                FirstChild = Unlinked,
                HeadSlot = Unlinked,
                Depth = depth,
                MortonCode = mortonCode,
            };
            return index;
        }

        private void Insert(int slot, Vector2 position, int nodeIndex)
        {
            if (!Contains(_nodes[0], position))
            {
                LinkAtHead(slot, OverflowLeaf);
                return;
            }

            while (_nodes[nodeIndex].FirstChild >= 0)
                nodeIndex = SelectChild(nodeIndex, position);

            LinkAtHead(slot, nodeIndex);

            ref Node leaf = ref _nodes[nodeIndex];
            if (leaf.Count > SplitThreshold && leaf.Depth < MaxDepth)
                Split(nodeIndex);
        }

        private void Split(int nodeIndex)
        {
            ref Node node = ref _nodes[nodeIndex];
            int previousHead = node.HeadSlot;
            CreateChildren(nodeIndex);
            node.HeadSlot = Unlinked;
            node.Count = 0;

            while (previousHead >= 0)
            {
                int slot = previousHead;
                previousHead = _nextSlot[slot];
                _nextSlot[slot] = Unlinked;
                _previousSlot[slot] = Unlinked;
                _leafBySlot[slot] = Unlinked;
                Insert(slot, _positions[slot], nodeIndex);
            }
        }

        private int CreateChildren(int nodeIndex)
        {
            ref Node node = ref _nodes[nodeIndex];
            float childHalfSize = node.HalfSize * 0.5f;
            int firstChild = _nodeCount;
            byte childDepth = (byte)(node.Depth + 1);
            uint childMorton = node.MortonCode << 2;

            CreateNode(node.Center + new Vector2(-childHalfSize, -childHalfSize), childHalfSize,
                childDepth, childMorton);
            CreateNode(node.Center + new Vector2(childHalfSize, -childHalfSize), childHalfSize,
                childDepth, childMorton | 1u);
            CreateNode(node.Center + new Vector2(-childHalfSize, childHalfSize), childHalfSize,
                childDepth, childMorton | 2u);
            CreateNode(node.Center + new Vector2(childHalfSize, childHalfSize), childHalfSize,
                childDepth, childMorton | 3u);
            node.FirstChild = firstChild;
            return firstChild;
        }

        private int SelectChild(int nodeIndex, Vector2 position)
        {
            ref Node node = ref _nodes[nodeIndex];
            int quadrant = (position.x >= node.Center.x ? 1 : 0) |
                           (position.y >= node.Center.y ? 2 : 0);
            return node.FirstChild + quadrant;
        }

        private void LinkAtHead(int slot, int leaf)
        {
            int head = leaf == OverflowLeaf ? _overflowHead : _nodes[leaf].HeadSlot;
            _previousSlot[slot] = Unlinked;
            _nextSlot[slot] = head;
            if (head >= 0)
                _previousSlot[head] = slot;

            if (leaf == OverflowLeaf)
                _overflowHead = slot;
            else
            {
                _nodes[leaf].HeadSlot = slot;
                _nodes[leaf].Count++;
            }
            _leafBySlot[slot] = leaf;
        }

        private void Unlink(int slot, int leaf)
        {
            int previous = _previousSlot[slot];
            int next = _nextSlot[slot];
            if (previous >= 0)
                _nextSlot[previous] = next;
            else if (leaf == OverflowLeaf)
                _overflowHead = next;
            else
                _nodes[leaf].HeadSlot = next;

            if (next >= 0)
                _previousSlot[next] = previous;

            if (leaf >= 0)
                _nodes[leaf].Count--;
            _nextSlot[slot] = Unlinked;
            _previousSlot[slot] = Unlinked;
            _leafBySlot[slot] = Unlinked;
        }

        private void EnsureSlotCapacity(int count)
        {
            if (_leafBySlot.Length >= count)
                return;

            int previousCapacity = _leafBySlot.Length;
            int capacity = Math.Max(count, previousCapacity == 0 ? 64 : previousCapacity * 2);
            Array.Resize(ref _nextSlot, capacity);
            Array.Resize(ref _previousSlot, capacity);
            Array.Resize(ref _leafBySlot, capacity);
            FillUnlinked(_nextSlot, previousCapacity);
            FillUnlinked(_previousSlot, previousCapacity);
            FillUnlinked(_leafBySlot, previousCapacity);
        }

        private static bool Contains(in Node node, Vector2 point) =>
            point.x >= node.Center.x - node.HalfSize &&
            point.x <= node.Center.x + node.HalfSize &&
            point.y >= node.Center.y - node.HalfSize &&
            point.y <= node.Center.y + node.HalfSize;

        private static bool SegmentIntersectsExpandedNode(
            Vector2 start,
            Vector2 end,
            float radius,
            in Node node)
        {
            float halfSize = node.HalfSize + radius;
            Vector2 min = node.Center - new Vector2(halfSize, halfSize);
            Vector2 max = node.Center + new Vector2(halfSize, halfSize);
            Vector2 delta = end - start;
            float enter = 0f;
            float exit = 1f;

            if (!ClipAxis(start.x, delta.x, min.x, max.x, ref enter, ref exit))
                return false;
            return ClipAxis(start.y, delta.y, min.y, max.y, ref enter, ref exit);
        }

        private static bool ClipAxis(
            float origin,
            float delta,
            float min,
            float max,
            ref float enter,
            ref float exit)
        {
            if (Mathf.Abs(delta) < 0.000001f)
                return origin >= min && origin <= max;

            float inverse = 1f / delta;
            float near = (min - origin) * inverse;
            float far = (max - origin) * inverse;
            if (near > far)
            {
                float swap = near;
                near = far;
                far = swap;
            }

            enter = Mathf.Max(enter, near);
            exit = Mathf.Min(exit, far);
            return enter <= exit;
        }

        private static int FullTreeNodeCount(int depth)
        {
            int count = 0;
            int levelCount = 1;
            for (int level = 0; level <= depth; level++)
            {
                count += levelCount;
                levelCount *= 4;
            }
            return count;
        }

        private static void FillUnlinked(int[] values, int start)
        {
            for (int i = start; i < values.Length; i++)
                values[i] = Unlinked;
        }
    }
}
