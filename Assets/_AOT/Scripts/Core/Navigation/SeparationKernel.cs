using System;
using UnityEngine;

namespace ProjectGame.Core.Navigation
{
    public sealed class SeparationKernel
    {
        private static readonly Vector2Int[] NeighborBuckets =
        {
            new(1, 0),
            new(0, 1),
            new(1, 1),
            new(-1, 1)
        };

        private const int NeighborBucketCount = 4;

        private readonly int[] _neighborBucketIndices;
        private readonly int[] _activeBuckets;
        private int _activeBucketCount;

        private readonly float _inverseRadius;
        private readonly float _inverseBucketSize;
        private readonly float _radiusSqr;
        private readonly float _strength;

        private readonly Vector2 _origin;
        private readonly Vector2 _max;
        private readonly int _gridWidth;
        private readonly int _gridHeight;

        private readonly int[] _bucketHeads;
        private int[] _next = Array.Empty<int>();
        private Vector2[] _separation = Array.Empty<Vector2>();

        public SeparationKernel(
            Vector2 origin,
            Vector2 max,
            float radius,
            float strength)
        {
            _origin = origin;
            _max = max;
            _radiusSqr = radius * radius;
            _inverseRadius = 1f / radius;
            _strength = strength;
            _inverseBucketSize = 1f / radius;

            _gridWidth = Mathf.Max(
                1,
                Mathf.CeilToInt((_max.x - _origin.x) * _inverseBucketSize));
            _gridHeight = Mathf.Max(
                1,
                Mathf.CeilToInt((_max.y - _origin.y) * _inverseBucketSize));

            int bucketCount = _gridWidth * _gridHeight;
            _bucketHeads = new int[bucketCount];
            _activeBuckets = new int[bucketCount];
            _neighborBucketIndices =
                new int[bucketCount * NeighborBucketCount];

            BuildNeighborLookup();
        }

        public void Build(
            Vector2[] positions,
            int[] activeSlots,
            int activeCount,
            int slotCount)
        {
            EnsureCapacity(slotCount);

            Array.Fill(_bucketHeads, -1);
            Array.Clear(_separation, 0, slotCount);
            _activeBucketCount = 0;

            BuildBuckets(positions, activeSlots, activeCount);
            CalculatePairs(positions);
        }

        public Vector2 Get(int slot) => _separation[slot];

        private void BuildNeighborLookup()
        {
            Array.Fill(_neighborBucketIndices, -1);

            for (int y = 0; y < _gridHeight; y++)
            {
                for (int x = 0; x < _gridWidth; x++)
                {
                    int bucket = y * _gridWidth + x;
                    int start = bucket * NeighborBucketCount;

                    for (int i = 0; i < NeighborBucketCount; i++)
                    {
                        int neighborX = x + NeighborBuckets[i].x;
                        int neighborY = y + NeighborBuckets[i].y;

                        if ((uint)neighborX >= (uint)_gridWidth ||
                            (uint)neighborY >= (uint)_gridHeight)
                            continue;

                        _neighborBucketIndices[start + i] =
                            neighborY * _gridWidth + neighborX;
                    }
                }
            }
        }

        private void BuildBuckets(
            Vector2[] positions,
            int[] activeSlots,
            int activeCount)
        {
            for (int i = 0; i < activeCount; i++)
            {
                int slot = activeSlots[i];
                int bucket = GetBucketIndex(positions[slot]);

                if (bucket < 0)
                    continue;

                if (_bucketHeads[bucket] < 0)
                    _activeBuckets[_activeBucketCount++] = bucket;

                _next[slot] = _bucketHeads[bucket];
                _bucketHeads[bucket] = slot;
            }
        }

        private void CalculatePairs(Vector2[] positions)
        {
            for (int active = 0; active < _activeBucketCount; active++)
            {
                int bucket = _activeBuckets[active];
                int head = _bucketHeads[bucket];

                CalculateInsideBucket(positions, head);

                int neighborStart = bucket * NeighborBucketCount;

                for (int i = 0; i < NeighborBucketCount; i++)
                {
                    int neighborBucket =
                        _neighborBucketIndices[neighborStart + i];

                    if (neighborBucket < 0)
                        continue;

                    int neighborHead = _bucketHeads[neighborBucket];

                    if (neighborHead >= 0)
                    {
                        CalculateBetweenBuckets(
                            positions,
                            head,
                            neighborHead);
                    }
                }
            }
        }

        private void CalculateInsideBucket(Vector2[] positions, int head)
        {
            for (int a = head; a >= 0; a = _next[a])
            {
                for (int b = _next[a]; b >= 0; b = _next[b])
                    ApplyPair(positions, a, b);
            }
        }

        private void CalculateBetweenBuckets(
            Vector2[] positions,
            int headA,
            int headB)
        {
            for (int a = headA; a >= 0; a = _next[a])
            {
                for (int b = headB; b >= 0; b = _next[b])
                    ApplyPair(positions, a, b);
            }
        }

        private void ApplyPair(Vector2[] positions, int a, int b)
        {
            Vector2 positionA = positions[a];
            Vector2 positionB = positions[b];

            float dx = positionA.x - positionB.x;
            float dy = positionA.y - positionB.y;
            float distanceSqr = dx * dx + dy * dy;

            if (distanceSqr >= _radiusSqr)
                return;

            float correctionX;
            float correctionY;

            if (distanceSqr <= 0.000001f)
            {
                Vector2 direction = GetOverlapDirection(a, b);
                correctionX = direction.x * _strength;
                correctionY = direction.y * _strength;
            }
            else
            {
                float distance = Mathf.Sqrt(distanceSqr);
                float scale =
                    (1f / distance - _inverseRadius) * _strength;

                correctionX = dx * scale;
                correctionY = dy * scale;
            }

            ref Vector2 separationA = ref _separation[a];
            ref Vector2 separationB = ref _separation[b];

            separationA.x += correctionX;
            separationA.y += correctionY;
            separationB.x -= correctionX;
            separationB.y -= correctionY;
        }

        private int GetBucketIndex(Vector2 position)
        {
            if (position.x < _origin.x ||
                position.x > _max.x ||
                position.y < _origin.y ||
                position.y > _max.y)
                return -1;

            int x = (int)((position.x - _origin.x) * _inverseBucketSize);
            int y = (int)((position.y - _origin.y) * _inverseBucketSize);

            if (x >= _gridWidth)
                x = _gridWidth - 1;

            if (y >= _gridHeight)
                y = _gridHeight - 1;

            return y * _gridWidth + x;
        }

        private void EnsureCapacity(int count)
        {
            if (_next.Length >= count)
                return;

            int capacity = Math.Max(
                count,
                _next.Length == 0 ? 64 : _next.Length * 2);

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
    }
}
