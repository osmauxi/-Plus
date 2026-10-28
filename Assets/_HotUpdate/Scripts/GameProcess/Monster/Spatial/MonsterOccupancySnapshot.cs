using System;
using ProjectGame.HotFix.Gameplay.Map;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 房间级、预移动阶段的怪物空间事实。
    /// Density 和 Separation 共享本对象，避免各自重复扫描 MonsterWorld。
    /// </summary>
    public sealed class MonsterOccupancySnapshot
    {
        private readonly Transform _roomRoot;
        private readonly int _fineWidth;
        private readonly int _fineHeight;
        private readonly Vector2 _bucketOrigin;
        private readonly Vector2 _bucketMax;
        private readonly float _inverseBucketSize;
        private readonly int _bucketWidth;
        private readonly int _bucketHeight;

        private Vector2[] _positions = Array.Empty<Vector2>();
        private int[] _activeSlots = Array.Empty<int>();
        private int[] _fineCellOfSlot = Array.Empty<int>();
        private int[] _fineCellCounts = Array.Empty<int>();
        private int[] _bucketHeads = Array.Empty<int>();
        private int[] _activeBuckets = Array.Empty<int>();
        private int[] _nextSlot = Array.Empty<int>();

        public int SlotCount { get; private set; }
        public int ActiveCount { get; private set; }
        public int ActiveBucketCount { get; private set; }

        public Vector2[] Positions => _positions;
        public int[] ActiveSlots => _activeSlots;
        public int[] FineCellOfSlot => _fineCellOfSlot;
        public int[] FineCellCounts => _fineCellCounts;
        public int[] BucketHeads => _bucketHeads;
        public int[] ActiveBuckets => _activeBuckets;
        public int[] NextSlot => _nextSlot;
        public int FineWidth => _fineWidth;
        public int FineHeight => _fineHeight;

        public MonsterOccupancySnapshot(
            Transform roomRoot,
            int fineWidth,
            int fineHeight,
            float roomHalfSize,
            float separationRadius,
            int initialCapacity)
        {
            _roomRoot = roomRoot ?? throw new ArgumentNullException(nameof(roomRoot));
            _fineWidth = Mathf.Max(1, fineWidth);
            _fineHeight = Mathf.Max(1, fineHeight);
            CalculateWorldBounds(
                roomRoot,
                roomHalfSize,
                out _bucketOrigin,
                out _bucketMax);

            _inverseBucketSize = 1f / Mathf.Max(0.0001f, separationRadius);
            _bucketWidth = Mathf.Max(
                1,
                Mathf.CeilToInt((_bucketMax.x - _bucketOrigin.x) * _inverseBucketSize));
            _bucketHeight = Mathf.Max(
                1,
                Mathf.CeilToInt((_bucketMax.y - _bucketOrigin.y) * _inverseBucketSize));

            _fineCellCounts = new int[checked(_fineWidth * _fineHeight)];
            _bucketHeads = new int[checked(_bucketWidth * _bucketHeight)];
            _activeBuckets = new int[_bucketHeads.Length];
            EnsureCapacity(Math.Max(0, initialCapacity));
            Clear();
        }

        public void Clear()
        {
            SlotCount = 0;
            ActiveCount = 0;
            ActiveBucketCount = 0;
            Array.Clear(_fineCellCounts, 0, _fineCellCounts.Length);
            Array.Fill(_bucketHeads, -1);
        }

        public void Build(MonsterWorld world, float groundY)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            SlotCount = world.SlotCount;
            EnsureCapacity(SlotCount);
            ActiveCount = 0;
            ActiveBucketCount = 0;
            Array.Clear(_fineCellCounts, 0, _fineCellCounts.Length);
            Array.Fill(_bucketHeads, -1);

            for (int slot = 0; slot < SlotCount; slot++)
            {
                _fineCellOfSlot[slot] = -1;

                if (!world.Meta[slot].IsActive)
                    continue;

                Vector2 position = world.Motion[slot].Position;
                _positions[slot] = position;
                _activeSlots[ActiveCount++] = slot;

                Vector3 worldPosition = new(position.x, groundY, position.y);
                if (RoomSpatialUtility.WorldToCell(
                        _roomRoot,
                        worldPosition,
                        out Vector2Int cell) &&
                    (uint)cell.x < (uint)_fineWidth &&
                    (uint)cell.y < (uint)_fineHeight)
                {
                    int cellIndex = cell.y * _fineWidth + cell.x;
                    _fineCellOfSlot[slot] = cellIndex;
                    _fineCellCounts[cellIndex]++;
                }

                int bucket = GetBucketIndex(position);
                if (bucket < 0)
                    continue;

                if (_bucketHeads[bucket] < 0)
                    _activeBuckets[ActiveBucketCount++] = bucket;

                _nextSlot[slot] = _bucketHeads[bucket];
                _bucketHeads[bucket] = slot;
            }
        }

        private int GetBucketIndex(Vector2 position)
        {
            if (position.x < _bucketOrigin.x ||
                position.x > _bucketMax.x ||
                position.y < _bucketOrigin.y ||
                position.y > _bucketMax.y)
                return -1;

            int x = (int)((position.x - _bucketOrigin.x) * _inverseBucketSize);
            int y = (int)((position.y - _bucketOrigin.y) * _inverseBucketSize);

            if (x >= _bucketWidth)
                x = _bucketWidth - 1;
            if (y >= _bucketHeight)
                y = _bucketHeight - 1;

            return y * _bucketWidth + x;
        }

        private void EnsureCapacity(int count)
        {
            if (_positions.Length >= count)
                return;

            int capacity = Math.Max(
                count,
                _positions.Length == 0 ? 64 : _positions.Length * 2);

            Array.Resize(ref _positions, capacity);
            Array.Resize(ref _activeSlots, capacity);
            Array.Resize(ref _fineCellOfSlot, capacity);
            Array.Resize(ref _nextSlot, capacity);
        }

        private static void CalculateWorldBounds(
            Transform roomRoot,
            float halfSize,
            out Vector2 min,
            out Vector2 max)
        {
            Vector3 a = roomRoot.TransformPoint(
                new Vector3(-halfSize, 0f, -halfSize));
            Vector3 b = roomRoot.TransformPoint(
                new Vector3(-halfSize, 0f, halfSize));
            Vector3 c = roomRoot.TransformPoint(
                new Vector3(halfSize, 0f, -halfSize));
            Vector3 d = roomRoot.TransformPoint(
                new Vector3(halfSize, 0f, halfSize));

            min = new Vector2(
                Mathf.Min(a.x, b.x, c.x, d.x),
                Mathf.Min(a.z, b.z, c.z, d.z));
            max = new Vector2(
                Mathf.Max(a.x, b.x, c.x, d.x),
                Mathf.Max(a.z, b.z, c.z, d.z));
        }
    }
}
