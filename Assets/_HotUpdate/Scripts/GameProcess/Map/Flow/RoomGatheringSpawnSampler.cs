using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Spawning;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map.Flow
{
    internal sealed class RoomGatheringSpawnSampler
    {
        public bool TrySampleMany(in RoomSpawnRegion region, Vector3 worldCenter, float worldRadius,
            int count, float minimumSpacing, System.Random random, List<Vector3> destination,
            int attemptsPerPoint)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (worldRadius < 0f || !float.IsFinite(worldRadius)) throw new ArgumentOutOfRangeException(nameof(worldRadius));
            if (minimumSpacing < 0f || !float.IsFinite(minimumSpacing)) throw new ArgumentOutOfRangeException(nameof(minimumSpacing));
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (attemptsPerPoint < 1) throw new ArgumentOutOfRangeException(nameof(attemptsPerPoint));

            destination.Clear();
            Vector3 center = region.Space.InverseTransformPoint(worldCenter);
            Vector3 half = region.LocalSize * 0.5f;
            float minimumX = region.LocalCenter.x - half.x;
            float maximumX = region.LocalCenter.x + half.x;
            float minimumZ = region.LocalCenter.z - half.z;
            float maximumZ = region.LocalCenter.z + half.z;
            float top = region.LocalCenter.y + half.y;
            float bottom = region.LocalCenter.y - half.y;
            float scaleX = Mathf.Max(0.0001f, Mathf.Abs(region.Space.lossyScale.x));
            float scaleZ = Mathf.Max(0.0001f, Mathf.Abs(region.Space.lossyScale.z));
            float radiusX = worldRadius / scaleX;
            float radiusZ = worldRadius / scaleZ;
            float spacingSqr = minimumSpacing * minimumSpacing;

            for (int pointIndex = 0; pointIndex < count; pointIndex++)
            {
                bool found = false;
                for (int attempt = 0; attempt < attemptsPerPoint; attempt++)
                {
                    float angle = (float)(random.NextDouble() * Math.PI * 2d);
                    float radius = Mathf.Sqrt((float)random.NextDouble());
                    float x = Mathf.Clamp(center.x + Mathf.Cos(angle) * radiusX * radius, minimumX, maximumX);
                    float z = Mathf.Clamp(center.z + Mathf.Sin(angle) * radiusZ * radius, minimumZ, maximumZ);

                    if (!TryRaycastGround(region, x, z, top, bottom, out Vector3 point) ||
                        !HasSpacing(destination, point, spacingSqr))
                        continue;

                    destination.Add(point);
                    found = true;
                    break;
                }

                if (found) continue;
                destination.Clear();
                return false;
            }

            return true;
        }

        private static bool TryRaycastGround(in RoomSpawnRegion region, float localX, float localZ,
            float localTop, float localBottom, out Vector3 point)
        {
            Vector3 top = region.Space.TransformPoint(new Vector3(localX, localTop, localZ));
            Vector3 bottom = region.Space.TransformPoint(new Vector3(localX, localBottom, localZ));
            Vector3 origin = top + Vector3.up * 2f;
            float distance = Mathf.Abs(origin.y - bottom.y) + 2f;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, distance,
                    region.ProbeMask, QueryTriggerInteraction.Ignore) &&
                (region.GroundMask.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                point = hit.point;
                return true;
            }

            point = default;
            return false;
        }

        private static bool HasSpacing(List<Vector3> accepted, Vector3 candidate, float spacingSqr)
        {
            for (int i = 0; i < accepted.Count; i++)
            {
                Vector2 offset = new Vector2(candidate.x - accepted[i].x, candidate.z - accepted[i].z);
                if (offset.sqrMagnitude < spacingSqr)
                    return false;
            }

            return true;
        }
    }
}
