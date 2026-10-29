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
        //常量，标记slot未链接到链表
        private const int Unlinked = -1;
        //常量，标记slot链接到溢出链表，不属于任何叶节点
        //用于处理超出四叉树范围的怪物，或者在叶节点中超过阈值的怪物
        private const int OverflowLeaf = -2;

        private struct Node
        {
            public Vector2 Center;
            public float HalfSize;
            public int FirstChild;
            public int HeadSlot;
            public int Count;
            public byte Depth;
            //Morton码用于编码节点再四叉树中的空间路径，根据象限掩码推算
            //如11 00，表示从根节点开始，先进入右上象限，再进入左下象限
            public uint MortonCode;
        }

        private readonly Vector2 _rootCenter;
        private readonly float _rootHalfSize;
        private readonly Node[] _nodes;
        private readonly int[] _queryStack;
        private Vector2[] _positions = Array.Empty<Vector2>();
        //链表结构，存储每个slot的下一个slot索引
        private int[] _nextSlot;
        //组成双向链表，方便删除slot时快速找到前一个slot
        private int[] _previousSlot;
        private int[] _leafBySlot;
        //当前树实际创建的节点数，不代表数组容量
        private int _nodeCount;
        //溢出链表头，存储所有不属于任何叶节点的slot索引
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
            //重置旧索引
            _positions = positions;
            EnsureSlotCapacity(slotCount);
            FillUnlinked(_nextSlot, 0);
            FillUnlinked(_previousSlot, 0);
            FillUnlinked(_leafBySlot, 0);
            _nodeCount = 0;
            _overflowHead = Unlinked;
            //重建基础树
            int root = CreateNode(_rootCenter, _rootHalfSize, 0, 0);
            BuildBaseTree(root);
            //插入活跃怪物
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
        /// <summary>
        /// 根据定义创建一个完整的基础四叉树，直到BaseDepth深度为止
        /// </summary>
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
        /// <summary>
        /// 将slot插入到四叉树中，找到对应的叶节点，如果叶节点超过阈值则进行分裂
        /// </summary>
        private void Insert(int slot, Vector2 position, int nodeIndex)
        {
            //判断是否越界
            if (!Contains(_nodes[0], position))
            {
                LinkAtHead(slot, OverflowLeaf);
                return;
            }
            //一直往下找，直到找到Slot位置对应的叶节点
            while (_nodes[nodeIndex].FirstChild >= 0)
                nodeIndex = SelectChild(nodeIndex, position);

            LinkAtHead(slot, nodeIndex);
            //叶子节点的怪物数量超过阈值且深度未达到最大深度，则进行分裂
            ref Node leaf = ref _nodes[nodeIndex];
            if (leaf.Count > SplitThreshold && leaf.Depth < MaxDepth)
                Split(nodeIndex);
        }

        private void Split(int nodeIndex)
        {
            ref Node node = ref _nodes[nodeIndex];
            int previousHead = node.HeadSlot;
            CreateChildren(nodeIndex);
            //将原来的叶节点的slot链表拆分到新的子节点中
            //有叶子的节点不会维护slot链表
            node.HeadSlot = Unlinked;
            node.Count = 0;
            //从头开始遍历slot链表，将每个slot重新插入到新的子节点中
            //逻辑上是拿出来再插入，实际上是更新slot的链表指针和叶节点索引
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
        /// <summary>
        /// 创建四个子节点，返回第一个子节点在_nodes数组中的索引
        /// </summary>
        private int CreateChildren(int nodeIndex)
        {
            ref Node node = ref _nodes[nodeIndex];
            float childHalfSize = node.HalfSize * 0.5f;
            //_nodeCount对应的是下一个新节点的索引
            int firstChild = _nodeCount;
            byte childDepth = (byte)(node.Depth + 1);
            //左移两位留出两位空间给象限掩码，左下=00，右下=01，左上=10，右上=11
            uint childMorton = node.MortonCode << 2;
            //算最新叶子节点的中心位置
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
        /// <summary>
        /// 根据位置选择对应的子节点索引，返回值是子节点在_nodes数组中的索引
        /// </summary>
        private int SelectChild(int nodeIndex, Vector2 position)
        {
            ref Node node = ref _nodes[nodeIndex];
            //算象限索引，左下=0，右下=1，左上=2，右上=3，子叶在_nodes数组中是连续的，
            //所以可以直接用FirstChild+象限索引得到子节点索引
            int quadrant = (position.x >= node.Center.x ? 1 : 0) |
                           (position.y >= node.Center.y ? 2 : 0);
            return node.FirstChild + quadrant;
        }
        /// <summary>
        /// 将slot插入到链表头部
        /// </summary>
        private void LinkAtHead(int slot, int leaf)
        {
            //这里维护的是slot链表
            int head = leaf == OverflowLeaf ? _overflowHead : _nodes[leaf].HeadSlot;
            //slot变成头部，他前面没有slot，后面是原来的头部slot
            _previousSlot[slot] = Unlinked;
            _nextSlot[slot] = head;
            //原来的头部slot的前面是新插入的slot
            if (head >= 0)
                _previousSlot[head] = slot;

            if (leaf == OverflowLeaf)
                _overflowHead = slot;
            else
            {
                //叶节点的头部slot变成新插入的slot
                //这里更新的是这个叶节点的链表头部索引，方便后续遍历这个叶节点的所有slot
                _nodes[leaf].HeadSlot = slot;
                //增加叶节点的怪物slot计数
                _nodes[leaf].Count++;
            }
            //记录slot属于哪个叶节点
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
        /// <summary>
        /// 标记所有slot为未链接状态，方便后续插入时使用
        /// </summary>
        private static void FillUnlinked(int[] values, int start)
        {
            for (int i = start; i < values.Length; i++)
                values[i] = Unlinked;
        }
    }
}
