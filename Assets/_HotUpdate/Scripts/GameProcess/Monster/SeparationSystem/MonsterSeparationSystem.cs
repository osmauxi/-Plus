using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 怪物局部分离系统：每Tick建立均匀空间网格，每个邻居Pair只计算一次
    /// 最终为每个Monster Slot输出一个Separation速度修正
    /// </summary>
    public sealed class MonsterSeparationSystem
    {
        /// <summary>
        /// 一次只看周围八个的四个，取两条边，保证不会重复遍历
        /// 在后续Grid的遍历中会把剩余四个补满，贴合Pair单次计算的设计
        /// </summary>
        private static readonly Vector2Int[] NeighborBuckets =
        {
            new(1, 0),   // E东
            new(0, 1),   // N北
            new(1, 1),   // NE东北
            new(-1, 1)   // NW东西
        };

        private readonly float _radius;
        private readonly float _radiusSqr;
        private readonly float _strength;
        private readonly float _bucketSize;

        private readonly Vector2 _origin;
        private readonly Vector2 _max;
        private readonly int _gridWidth;
        private readonly int _gridHeight;

        /// <summary>
        /// 桶头，单个Grid内的复数怪，只存第一个的Slot，指向_next中的索引，然后_next中指向下一个怪物索引
        /// 如单Grid存在Slot13 7 2，_bucketHeads[13] = 7,_next[7] = 2,_next[2] = -1,近似链表
        /// 避免了每个Grid都单独持有List，以及最差情况所有Slot在同一格导致维护大量空Slot的问题
        /// 因为本身单场房间中Slot不可复用，所以这里天然支持这样写
        /// </summary>
        private readonly int[] _bucketHeads;
        private int[] _next = Array.Empty<int>();
        /// <summary>
        /// 存每个Slot对应的偏移速度量
        /// </summary>
        private Vector2[] _separation = Array.Empty<Vector2>();

        public MonsterSeparationSystem(Transform roomRoot, float roomHalfSize, float radius, float strength)
        {
            if (roomRoot == null) 
                throw new ArgumentNullException(nameof(roomRoot));
            if (radius <= 0f) 
                throw new ArgumentOutOfRangeException(nameof(radius));
            if (strength < 0f) 
                throw new ArgumentOutOfRangeException(nameof(strength));

            _radius = radius;
            _radiusSqr = radius * radius;
            _strength = strength;
            _bucketSize = radius;

            CalculateWorldBounds(roomRoot, roomHalfSize, out _origin, out _max);

            _gridWidth = Mathf.Max(1, Mathf.CeilToInt((_max.x - _origin.x) / _bucketSize));
            _gridHeight = Mathf.Max(1, Mathf.CeilToInt((_max.y - _origin.y) / _bucketSize));
            _bucketHeads = new int[_gridWidth * _gridHeight];
        }

        public void Build(MonsterWorld world)
        {
            EnsureCapacity(world.SlotCount);

            Array.Fill(_bucketHeads, -1);
            Array.Clear(_separation, 0, world.SlotCount);

            BuildBuckets(world);
            CalculatePairs(world);
        }

        public Vector2 Get(int slot) =>
            (uint)slot < (uint)_separation.Length ? _separation[slot] : Vector2.zero;
        /// <summary>
        /// 遍历Slot，把怪物信息写进bucket和next
        /// </summary>
        private void BuildBuckets(MonsterWorld world)
        {
            for (int slot = 0; slot < world.SlotCount; slot++)
            {
                if (!world.Meta[slot].IsActive)
                    continue;

                int bucket = GetBucketIndex(world.Motion[slot].Position);
                if (bucket < 0)
                    continue;

                _next[slot] = _bucketHeads[bucket];
                _bucketHeads[bucket] = slot;
            }
        }

        private void CalculatePairs(MonsterWorld world)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                for (int x = 0; x < _gridWidth; x++)
                {
                    int bucket = ToBucketIndex(x, y);
                    int head = _bucketHeads[bucket];

                    if (head < 0)
                        continue;

                    CalculateInsideBucket(world, head);

                    for (int i = 0; i < NeighborBuckets.Length; i++)
                    {
                        int neighborX = x + NeighborBuckets[i].x;
                        int neighborY = y + NeighborBuckets[i].y;

                        if (!IsValidBucket(neighborX, neighborY))
                            continue;

                        int neighborHead = _bucketHeads[ToBucketIndex(neighborX, neighborY)];
                        if (neighborHead >= 0)
                            CalculateBetweenBuckets(world, head, neighborHead);
                    }
                }
            }
        }
        /// <summary>
        /// 根据头指针遍历所有链表Slot应用斥力
        /// </summary>
        private void CalculateInsideBucket(MonsterWorld world, int head)
        {
            for (int a = head; a >= 0; a = _next[a])
            {
                //从a之后开始，所以A-B永远只计算一次。
                for (int b = _next[a]; b >= 0; b = _next[b])
                    ApplyPair(world, a, b);
            }
        }

        private void CalculateBetweenBuckets(MonsterWorld world, int headA, int headB)
        {
            for (int a = headA; a >= 0; a = _next[a])
            {
                for (int b = headB; b >= 0; b = _next[b])
                    ApplyPair(world, a, b);
            }
        }

        private void ApplyPair(MonsterWorld world, int a, int b)
        {
            Vector2 offset = world.Motion[a].Position - world.Motion[b].Position;
            float distanceSqr = offset.sqrMagnitude;
            //过滤离得很远的
            if (distanceSqr >= _radiusSqr)
                return;

            Vector2 direction;
            float weight;
            //太近了，基本叠一起，随便取一个方向，满权重
            //在范围内就求权重，
            if (distanceSqr <= 0.000001f)
            {
                direction = GetOverlapDirection(a, b);
                weight = 1f;
            }
            else
            {
                float distance = Mathf.Sqrt(distanceSqr);
                direction = offset / distance;
                weight = 1f - distance / _radius;
            }

            Vector2 correction = direction * weight * _strength;

            _separation[a] += correction;
            _separation[b] -= correction;
        }

        private int GetBucketIndex(Vector2 position)
        {
            if (position.x < _origin.x || position.x > _max.x ||
                position.y < _origin.y || position.y > _max.y)
                return -1;

            int x = Mathf.Min(_gridWidth - 1, Mathf.FloorToInt((position.x - _origin.x) / _bucketSize));
            int y = Mathf.Min(_gridHeight - 1, Mathf.FloorToInt((position.y - _origin.y) / _bucketSize));

            return ToBucketIndex(x, y);
        }

        private int ToBucketIndex(int x, int y) => y * _gridWidth + x;

        private bool IsValidBucket(int x, int y) =>
            (uint)x < (uint)_gridWidth && (uint)y < (uint)_gridHeight;

        private void EnsureCapacity(int count)
        {
            if (_next.Length >= count)
                return;

            int capacity = Math.Max(count, _next.Length == 0 ? 64 : _next.Length * 2);
            Array.Resize(ref _next, capacity);
            Array.Resize(ref _separation, capacity);
        }

        private static Vector2 GetOverlapDirection(int a, int b)
        {
            int direction = ((a * 397) ^ b) & 3;

            return direction switch
            {
                0 => Vector2.right,
                1 => Vector2.left,
                2 => Vector2.up,
                _ => Vector2.down
            };
        }

        private static void CalculateWorldBounds(Transform roomRoot, float halfSize,
            out Vector2 min, out Vector2 max)
        {
            Vector3 a = roomRoot.TransformPoint(new Vector3(-halfSize, 0f, -halfSize));
            Vector3 b = roomRoot.TransformPoint(new Vector3(-halfSize, 0f, halfSize));
            Vector3 c = roomRoot.TransformPoint(new Vector3(halfSize, 0f, -halfSize));
            Vector3 d = roomRoot.TransformPoint(new Vector3(halfSize, 0f, halfSize));

            min = new Vector2(
                Mathf.Min(a.x, b.x, c.x, d.x),
                Mathf.Min(a.z, b.z, c.z, d.z));

            max = new Vector2(
                Mathf.Max(a.x, b.x, c.x, d.x),
                Mathf.Max(a.z, b.z, c.z, d.z));
        }
    }
}