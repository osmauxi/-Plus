using System;
using ProjectGame.Core.Spatial;
using ProjectGame.HotFix.Gameplay.Map;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 房间级、预移动阶段的怪物空间事实。
    /// 生命周期留在 HotFix，纯数组构建由 AOT Kernel 执行。
    /// </summary>
    public sealed class MonsterOccupancySnapshot
    {
        private readonly Transform _roomRoot;
        private readonly MonsterOccupancySnapshotKernel _kernel;

        public int SlotCount => _kernel.SlotCount;
        public int ActiveCount => _kernel.ActiveCount;
        public int ActiveBucketCount => _kernel.ActiveBucketCount;
        public Vector2[] Positions => _kernel.Positions;
        public int[] ActiveSlots => _kernel.ActiveSlots;
        public int[] FineCellOfSlot => _kernel.FineCellOfSlot;
        public int[] FineCellCounts => _kernel.FineCellCounts;
        public int[] BucketHeads => _kernel.BucketHeads;
        public int[] ActiveBuckets => _kernel.ActiveBuckets;
        public int[] NextSlot => _kernel.NextSlot;
        public int FineWidth => _kernel.FineWidth;
        public int FineHeight => _kernel.FineHeight;

        public MonsterOccupancySnapshot(
            Transform roomRoot,
            int fineWidth,
            int fineHeight,
            float roomHalfSize,
            float separationRadius,
            int initialCapacity)
        {
            _roomRoot = roomRoot ?? throw new ArgumentNullException(nameof(roomRoot));
            CalculateWorldBounds(roomRoot, roomHalfSize, out Vector2 min, out Vector2 max);
            _kernel = new MonsterOccupancySnapshotKernel(
                RoomSpatialData.HalfSize,
                RoomSpatialData.CellSize,
                fineWidth,
                fineHeight,
                min,
                max,
                separationRadius,
                initialCapacity);
        }

        public void Clear() => _kernel.Clear();

        public void EnsureCapacity(int count) => _kernel.EnsureCapacity(count);

        public void Build(MonsterWorld world, float groundY)
        {
            _kernel.Build(
                world.Meta,
                world.Motion,
                world.SlotCount,
                groundY,
                _roomRoot.worldToLocalMatrix);
        }

        private static void CalculateWorldBounds(
            Transform roomRoot,
            float halfSize,
            out Vector2 min,
            out Vector2 max)
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
