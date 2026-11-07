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

        // 旧实现将根范围外的怪物挂到独立 overflow 链表，并在每次查询末尾无条件追加。
        // 当前房间 Grid 与四叉树覆盖范围一致，越界怪物不参与弹丸交互，因此不再维护该链表。

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
        public int NodeCount => _nodeCount;
        public int NodeCapacity => _nodes.Length;
        public Vector2 RootCenter => _rootCenter;
        public float RootSize => _rootHalfSize * 2f;
        /// <summary>
        /// 本轮查询收集到多少个怪物候选
        /// </summary>
        public int LastQueryCandidateCount { get; private set; }
        /// <summary>
        /// 本次查询访问了多少个四叉树节点
        /// </summary>
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
            LastQueryCandidateCount = 0;
            LastQueryVisitedNodeCount = 0;
        }

        public void Build(Vector2[] positions, byte[] activeMask, int slotCount)
        {
            //重置旧索引
            _positions = positions;
            FillUnlinked(_nextSlot, 0);
            FillUnlinked(_previousSlot, 0);
            FillUnlinked(_leafBySlot, 0);
            _nodeCount = 0;
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
            //使用栈来遍历四叉树，避免递归调用
            int stackCount = 1;
            _queryStack[0] = 0;

            while (stackCount > 0)
            {
                //弹出栈顶节点索引，访问该节点
                int nodeIndex = _queryStack[--stackCount];
                ref Node node = ref _nodes[nodeIndex];
                visitedNodeCount++;
                //判定当前节点是否与扫掠圆相交，如果不相交则跳过该节点及其子节点
                if (!SegmentIntersectsExpandedNode(start, end, radius, node))
                    continue;
                //FirstChild大于0说明他有叶子，将子节点索引压入栈中，
                //在下几轮循环中访问子节点并剔除不相交的叶节点
                if (node.FirstChild >= 0)
                {
                    _queryStack[stackCount++] = node.FirstChild;
                    _queryStack[stackCount++] = node.FirstChild + 1;
                    _queryStack[stackCount++] = node.FirstChild + 2;
                    _queryStack[stackCount++] = node.FirstChild + 3;
                    continue;
                }

                //叶节点，遍历该叶节点的slot链表，将所有slot索引加入结果数组
                for (int slot = node.HeadSlot; slot >= 0; slot = _nextSlot[slot])
                    results[resultCount++] = slot;
            }
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
                return;
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
            int head = _nodes[leaf].HeadSlot;
            //slot变成头部，他前面没有slot，后面是原来的头部slot
            _previousSlot[slot] = Unlinked;
            _nextSlot[slot] = head;
            //原来的头部slot的前面是新插入的slot
            if (head >= 0)
                _previousSlot[head] = slot;

            //叶节点的头部slot变成新插入的slot
            //这里更新的是这个叶节点的链表头部索引，方便后续遍历这个叶节点的所有slot
            _nodes[leaf].HeadSlot = slot;
            //增加叶节点的怪物slot计数
            _nodes[leaf].Count++;
            //记录slot属于哪个叶节点
            _leafBySlot[slot] = leaf;
        }

        private void Unlink(int slot, int leaf)
        {
            int previous = _previousSlot[slot];
            int next = _nextSlot[slot];
            if (previous >= 0)
                _nextSlot[previous] = next;
            else
                _nodes[leaf].HeadSlot = next;

            if (next >= 0)
                _previousSlot[next] = previous;

            _nodes[leaf].Count--;
            _nextSlot[slot] = Unlinked;
            _previousSlot[slot] = Unlinked;
            _leafBySlot[slot] = Unlinked;
        }

        public void EnsureCapacity(int count)
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

        /// <summary>
        /// 判定方法，返回线段是否与节点的包围盒相交，
        /// 包围盒是节点的正方形区域加上半径扩展
        /// </summary>
        private static bool SegmentIntersectsExpandedNode(
            Vector2 start,
            Vector2 end,
            float radius,
            in Node node)
        {
            //叶子节点本身矩形半长加上判定半径，这个扩张矩形范围就是判定的范围
            float halfSize = node.HalfSize + radius;
            //计算扩张矩形的最小点和最大点坐标，两个对角点确定一个矩形区域
            Vector2 min = node.Center - new Vector2(halfSize, halfSize);
            Vector2 max = node.Center + new Vector2(halfSize, halfSize);
            //delta取起点到终点的位移
            Vector2 delta = end - start;
            float enter = 0f;
            float exit = 1f;

            if (!ClipAxis(start.x, delta.x, min.x, max.x, ref enter, ref exit))
                return false;
            return ClipAxis(start.y, delta.y, min.y, max.y, ref enter, ref exit);
        }
        /// <summary>
        /// 线段与矩形相交的具体算法
        /// 在线段的某一个坐标轴上，计算线段处于矩形范围内的参数区间，并与之前的有效区间求交
        /// </summary>
        private static bool ClipAxis(
            float origin,
            float delta,
            float min,
            float max,
            ref float enter,
            ref float exit)
        {
            //当前轴上基本没有位移，说明线段在这个轴上是平行的，直接判断起点是否在矩形范围内
            //判定逻辑是先判定x轴，再判定y轴，如果x轴不在范围内就直接返回false，不会再判定y轴
            if (Mathf.Abs(delta) < 0.000001f)
                return origin >= min && origin <= max;
            //我们使用0-1参数化的t，线段的起点是0，终点是1，delta是终点相对于起点的位移
            //near与far是线段在当前轴上与矩形的交点参数，near是进入矩形的t，far是离开矩形的t
            float inverse = 1f / delta;
            float near = (min - origin) * inverse;
            float far = (max - origin) * inverse;
            //可能存在子弹反向移动的情况，这里就要交换一次值
            if (near > far)
            {
                float swap = near;
                near = far;
                far = swap;
            }
            //最终范围需要同时满足x轴和y轴的范围，所以我们取交集
            enter = Mathf.Max(enter, near);
            exit = Mathf.Min(exit, far);
            //enter <= exit说明0-1的t中存在一段区间在矩形范围内
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
