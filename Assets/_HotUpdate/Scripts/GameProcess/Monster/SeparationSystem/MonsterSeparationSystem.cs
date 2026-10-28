using ProjectGame.Core.Navigation;
using Unity.Profiling;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 从房间级空间快照读取活跃位置和粗粒度桶，并由 AOT Kernel 计算分离速度。
    /// </summary>
    public sealed class MonsterSeparationSystem
    {
        private static readonly ProfilerMarker KernelMarker =
            new("Monster.Separation.Kernel");

        private readonly SeparationKernel _kernel;

        public MonsterSeparationSystem(
            Transform roomRoot,
            float roomHalfSize,
            float radius,
            float strength)
        {
            CalculateWorldBounds(
                roomRoot,
                roomHalfSize,
                out Vector2 origin,
                out Vector2 max);

            _kernel = new SeparationKernel(
                origin,
                max,
                radius,
                strength);
        }

        public void Build(MonsterOccupancySnapshot snapshot)
        {
            if (snapshot == null)
                throw new System.ArgumentNullException(nameof(snapshot));

            using (KernelMarker.Auto())
            {
                _kernel.BuildFromBuckets(
                    snapshot.Positions,
                    snapshot.BucketHeads,
                    snapshot.ActiveBuckets,
                    snapshot.ActiveBucketCount,
                    snapshot.NextSlot,
                    snapshot.SlotCount);
            }
        }

        public Vector2 Get(int slot) => _kernel.Get(slot);

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
