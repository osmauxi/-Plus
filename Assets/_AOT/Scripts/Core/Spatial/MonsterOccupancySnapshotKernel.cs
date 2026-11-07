using System;
using ProjectGame.Core.Monsters;
using UnityEngine;

namespace ProjectGame.Core.Spatial
{
    /// <summary>构建预移动阶段怪物占用快照的纯数组内核。</summary>
    public sealed class MonsterOccupancySnapshotKernel
    {
        private readonly float _fineHalfSize;
        private readonly float _fineCellSize;
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
        private readonly int[] _fineCellCounts;
        private readonly int[] _bucketHeads;
        private readonly int[] _activeBuckets;
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

        public MonsterOccupancySnapshotKernel(
            float fineHalfSize,
            float fineCellSize,
            int fineWidth,
            int fineHeight,
            Vector2 bucketOrigin,
            Vector2 bucketMax,
            float bucketSize,
            int initialCapacity)
        {
            _fineHalfSize = fineHalfSize;
            _fineCellSize = fineCellSize;
            _fineWidth = fineWidth;
            _fineHeight = fineHeight;
            _bucketOrigin = bucketOrigin;
            _bucketMax = bucketMax;
            _inverseBucketSize = 1f / bucketSize;
            _bucketWidth = Mathf.Max(
                1,
                Mathf.CeilToInt((_bucketMax.x - _bucketOrigin.x) * _inverseBucketSize));
            _bucketHeight = Mathf.Max(
                1,
                Mathf.CeilToInt((_bucketMax.y - _bucketOrigin.y) * _inverseBucketSize));
            _fineCellCounts = new int[checked(_fineWidth * _fineHeight)];
            _bucketHeads = new int[checked(_bucketWidth * _bucketHeight)];
            _activeBuckets = new int[_bucketHeads.Length];
            EnsureCapacity(initialCapacity);
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

        public void Build(
            MonsterMetaData[] meta,
            MonsterMotionData[] motion,
            int slotCount,
            float groundY,
            Matrix4x4 roomWorldToLocal)
        {
            SlotCount = slotCount;
            ActiveCount = 0;
            ActiveBucketCount = 0;
            Array.Clear(_fineCellCounts, 0, _fineCellCounts.Length);
            Array.Fill(_bucketHeads, -1);

            for (int slot = 0; slot < slotCount; slot++)
            {
                _fineCellOfSlot[slot] = -1;
                if (!meta[slot].IsActive)
                    continue;

                Vector2 position = motion[slot].Position;
                _positions[slot] = position;
                _activeSlots[ActiveCount++] = slot;

                Vector3 local = roomWorldToLocal.MultiplyPoint3x4(
                    new Vector3(position.x, groundY, position.y));
                int cellX = Mathf.FloorToInt((local.x + _fineHalfSize) / _fineCellSize);
                int cellY = Mathf.FloorToInt((local.z + _fineHalfSize) / _fineCellSize);
                if ((uint)cellX < (uint)_fineWidth &&
                    (uint)cellY < (uint)_fineHeight)
                {
                    int cellIndex = cellY * _fineWidth + cellX;
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

        public void EnsureCapacity(int count)
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
    }
}
