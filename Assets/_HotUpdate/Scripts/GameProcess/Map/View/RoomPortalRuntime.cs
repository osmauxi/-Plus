using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map.View
{
    /// <summary>
    /// 一个进入目标房间的数学入口平面，主要用来进行玩家出入房间的判定
    /// </summary>
    public sealed class RoomPortalRuntime
    {
        public int ConnectionId { get; }
        public int TargetRoomId { get; }
        public int OtherRoomId { get; }

        public Transform PortalTransform { get; }
        public Vector3 InwardNormal { get; }
        public float HalfWidth { get; }
        public float HalfHeight { get; }

        public RoomPortalRuntime(int connectionId, int targetRoomId, int otherRoomId, Transform portalTransform, float portalWidth, float portalHeight)
            : this(connectionId, targetRoomId, otherRoomId, portalTransform, portalTransform != null ? portalTransform.forward : Vector3.zero, portalWidth, portalHeight)
        {
        }

        public RoomPortalRuntime(
            int connectionId,
            int targetRoomId,
            int otherRoomId,
            Transform portalTransform,
            Vector3 inwardNormal,
            float portalWidth,
            float portalHeight)
        {
            if (portalTransform == null)
                throw new ArgumentNullException(nameof(portalTransform));

            if (inwardNormal.sqrMagnitude < 0.01f)
                throw new ArgumentException("Portal 朝向向量不能为空。", nameof(inwardNormal));

            ConnectionId = connectionId;
            TargetRoomId = targetRoomId;
            OtherRoomId = otherRoomId;
            PortalTransform = portalTransform;
            InwardNormal = inwardNormal.normalized;
            HalfWidth = Mathf.Max(0.05f, portalWidth * 0.5f);
            HalfHeight = Mathf.Max(0.05f, portalHeight * 0.5f);
        }
    }
}
