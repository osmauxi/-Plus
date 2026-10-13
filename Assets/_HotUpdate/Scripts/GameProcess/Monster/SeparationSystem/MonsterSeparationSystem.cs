using System;
using ProjectGame.Core.Navigation;
using Unity.Profiling;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 从 MonsterWorld 采集活跃位置，并由 AOT Kernel 计算分离速度。
    /// </summary>
    public sealed class MonsterSeparationSystem
    {
        private static readonly ProfilerMarker CaptureMarker =
            new("Monster.Separation.Capture");
        private static readonly ProfilerMarker KernelMarker =
            new("Monster.Separation.Kernel");

        private Vector2[] _positions = Array.Empty<Vector2>();
        private int[] _activeSlots = Array.Empty<int>();
        private int _activeCount;

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

        public void Build(MonsterWorld world)
        {
            EnsureCaptureCapacity(world.SlotCount);
            _activeCount = 0;

            using (CaptureMarker.Auto())
            {
                for (int slot = 0; slot < world.SlotCount; slot++)
                {
                    if (!world.Meta[slot].IsActive)
                        continue;

                    _positions[slot] = world.Motion[slot].Position;
                    _activeSlots[_activeCount++] = slot;
                }
            }

            using (KernelMarker.Auto())
            {
                _kernel.Build(
                    _positions,
                    _activeSlots,
                    _activeCount,
                    world.SlotCount);
            }
        }

        public Vector2 Get(int slot) => _kernel.Get(slot);

        private void EnsureCaptureCapacity(int count)
        {
            if (_positions.Length >= count)
                return;

            int capacity = Math.Max(
                count,
                _positions.Length == 0 ? 64 : _positions.Length * 2);

            Array.Resize(ref _positions, capacity);
            Array.Resize(ref _activeSlots, capacity);
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
