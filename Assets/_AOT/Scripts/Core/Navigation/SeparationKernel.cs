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

        private readonly float _inverseRadius;
        private readonly float _inverseBucketSize;
        private readonly float _radiusSqr;
        private readonly float _strength;

        private readonly Vector2 _origin;
        private readonly Vector2 _max;
        private readonly int _gridWidth;
        private readonly int _gridHeight;

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
            _neighborBucketIndices =
                new int[bucketCount * NeighborBucketCount];

            BuildNeighborLookup();
        }

        public void BuildFromBuckets(
            Vector2[] positions,
            int[] bucketHeads,
            int[] activeBuckets,
            int activeBucketCount,
            int[] nextSlot,
            int slotCount)
        {
            EnsureCapacity(slotCount);

            Array.Clear(_separation, 0, slotCount);
            CalculatePairs(
                positions,
                bucketHeads,
                activeBuckets,
                activeBucketCount,
                nextSlot);
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

        private void CalculatePairs(
            Vector2[] positions,
            int[] bucketHeads,
            int[] activeBuckets,
            int activeBucketCount,
            int[] nextSlot)
        {
            for (int active = 0; active < activeBucketCount; active++)
            {
                int bucket = activeBuckets[active];
                int head = bucketHeads[bucket];

                CalculateInsideBucket(positions, nextSlot, head);

                int neighborStart = bucket * NeighborBucketCount;

                for (int i = 0; i < NeighborBucketCount; i++)
                {
                    int neighborBucket =
                        _neighborBucketIndices[neighborStart + i];

                    if (neighborBucket < 0)
                        continue;

                    int neighborHead = bucketHeads[neighborBucket];

                    if (neighborHead >= 0)
                    {
                        CalculateBetweenBuckets(
                            positions,
                            head,
                            neighborHead,
                            nextSlot);
                    }
                }
            }
        }

        private void CalculateInsideBucket(
            Vector2[] positions,
            int[] nextSlot,
            int head)
        {
            for (int a = head; a >= 0; a = nextSlot[a])
            {
                for (int b = nextSlot[a]; b >= 0; b = nextSlot[b])
                    ApplyPair(positions, a, b);
            }
        }

        private void CalculateBetweenBuckets(
            Vector2[] positions,
            int headA,
            int headB,
            int[] nextSlot)
        {
            for (int a = headA; a >= 0; a = nextSlot[a])
            {
                for (int b = headB; b >= 0; b = nextSlot[b])
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

        private void EnsureCapacity(int count)
        {
            if (_separation.Length >= count)
                return;

            int capacity = Math.Max(
                count,
                _separation.Length == 0 ? 64 : _separation.Length * 2);

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
