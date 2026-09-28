using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Spawning
{
    public readonly struct SpawnPose
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public SpawnPose(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }

    /// <summary>RoomView 提供的局部采样区域；不要求场景 Collider 设置 Static。</summary>
    public readonly struct RoomSpawnRegion
    {
        public readonly Transform Space;
        public readonly Vector3 LocalCenter;
        public readonly Vector3 LocalSize;
        public readonly LayerMask ProbeMask;
        public readonly LayerMask GroundMask;

        public RoomSpawnRegion(Transform space, Vector3 localCenter, Vector3 localSize,
            LayerMask probeMask, LayerMask groundMask)
        {
            Space = space != null ? space : throw new ArgumentNullException(nameof(space));
            if (!IsPositiveFinite(localSize.x) || !IsPositiveFinite(localSize.y) ||
                !IsPositiveFinite(localSize.z))
                throw new ArgumentOutOfRangeException(nameof(localSize));
            if (probeMask.value == 0) throw new ArgumentException("Spawn ProbeMask 不能为空。", nameof(probeMask));
            if (groundMask.value == 0) throw new ArgumentException("Spawn GroundMask 不能为空。", nameof(groundMask));
            LocalCenter = localCenter;
            LocalSize = localSize;
            ProbeMask = probeMask;
            GroundMask = groundMask;
        }

        public void GetWorldXZBounds(out Vector2 origin, out Vector2 size)
        {
            Vector3 half = LocalSize * 0.5f;
            Vector3[] corners =
            {
                new(LocalCenter.x - half.x, LocalCenter.y, LocalCenter.z - half.z),
                new(LocalCenter.x - half.x, LocalCenter.y, LocalCenter.z + half.z),
                new(LocalCenter.x + half.x, LocalCenter.y, LocalCenter.z - half.z),
                new(LocalCenter.x + half.x, LocalCenter.y, LocalCenter.z + half.z),
            };
            Vector2 minimum = new(float.MaxValue, float.MaxValue);
            Vector2 maximum = new(float.MinValue, float.MinValue);
            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 world = Space.TransformPoint(corners[i]);
                minimum = Vector2.Min(minimum, new Vector2(world.x, world.z));
                maximum = Vector2.Max(maximum, new Vector2(world.x, world.z));
            }
            origin = minimum;
            size = maximum - minimum;
        }

        private static bool IsPositiveFinite(float value) => value > 0f &&
            !float.IsNaN(value) && !float.IsInfinity(value);
    }

    /// <summary>
    /// 在 RoomView 的局部区域随机取点，从上向下查询最近 Collider；只有最近命中属于 GroundMask 才接受。
    /// 查询只依赖 Layer/Collider，不读取 GameObject Static 标记。
    /// </summary>
    public sealed class RoomGroundPointSampler
    {
        public bool TrySampleMany(in RoomSpawnRegion region, int count, float horizontalScale,
            float minimumSpacing, System.Random random, List<Vector3> destination,
            int attemptsPerPoint = 48, float rayPadding = 2f)
        {
            destination.Clear();
            Vector3 half = region.LocalSize * 0.5f;
            float sampleHalfX = half.x * horizontalScale;
            float sampleHalfZ = half.z * horizontalScale;
            float localTop = region.LocalCenter.y + half.y;
            float localBottom = region.LocalCenter.y - half.y;
            float spacingSqr = minimumSpacing * minimumSpacing;

            for (int pointIndex = 0; pointIndex < count; pointIndex++)
            {
                bool accepted = false;
                for (int attempt = 0; attempt < attemptsPerPoint; attempt++)
                {
                    float localX = region.LocalCenter.x +
                        Mathf.Lerp(-sampleHalfX, sampleHalfX, (float)random.NextDouble());
                    float localZ = region.LocalCenter.z +
                        Mathf.Lerp(-sampleHalfZ, sampleHalfZ, (float)random.NextDouble());
                    Vector3 top = region.Space.TransformPoint(new Vector3(localX, localTop, localZ));
                    Vector3 bottom = region.Space.TransformPoint(new Vector3(localX, localBottom, localZ));
                    Vector3 origin = top + Vector3.up * rayPadding;
                    float distance = Mathf.Abs(origin.y - bottom.y) + rayPadding;

                    if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, distance,
                            region.ProbeMask, QueryTriggerInteraction.Ignore))
                        continue;
                    if ((region.GroundMask.value & (1 << hit.collider.gameObject.layer)) == 0)
                        continue;
                    if (!HasSpacing(destination, hit.point, spacingSqr)) continue;
                    destination.Add(hit.point);
                    accepted = true;
                    break;
                }
                if (accepted) continue;
                destination.Clear();
                return false;
            }
            return true;
        }

        private static bool HasSpacing(List<Vector3> accepted, Vector3 candidate, float spacingSqr)
        {
            if (spacingSqr <= 0f) return true;
            for (int i = 0; i < accepted.Count; i++)
            {
                Vector2 offset = new(candidate.x - accepted[i].x, candidate.z - accepted[i].z);
                if (offset.sqrMagnitude < spacingSqr) return false;
            }
            return true;
        }
    }
}
