using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map.View
{
    /// <summary>
    /// 一个已实例化地图连接的运行时包装，将ID与实际View对象关联起来。
    /// ConnectionView只保存预制体引用；
    /// ConnectionId、房间关系、Portal和原始缩放保存在这里。
    /// </summary>
    public sealed class ConnectionViewRuntime
    {
        public int ConnectionId { get; }
        public int RoomAId { get; }
        public int RoomBId { get; }

        public RoomConnectorSlot ConnectorA { get; }
        public RoomConnectorSlot ConnectorB { get; }
        public ConnectionView View { get; }
        public bool UsesConnectionView => View != null;

        public Vector3 OriginalStretchScale { get; }

        public RoomPortalRuntime PortalToRoomA { get; }
        public RoomPortalRuntime PortalToRoomB { get; }

        public ConnectionViewRuntime(
            int connectionId,
            int roomAId,
            int roomBId,
            RoomConnectorSlot connectorA,
            RoomConnectorSlot connectorB,
            ConnectionView view,
            float roomScale)
        {
            ConnectorA = connectorA != null ? connectorA : throw new ArgumentNullException(nameof(connectorA));
            ConnectorB = connectorB != null ? connectorB : throw new ArgumentNullException(nameof(connectorB));
            View = view != null ? view : throw new ArgumentNullException(nameof(view));

            ValidateRoomScale(roomScale);

            if (view.StretchRoot == null)
                throw new InvalidOperationException($"Connection {connectionId} 的 ConnectionView 缺少 StretchRoot。");

            if (view.PortalA == null || view.PortalB == null)
                throw new InvalidOperationException($"Connection {connectionId} 的 ConnectionView 缺少 PortalA 或 PortalB。");

            ConnectionId = connectionId;
            RoomAId = roomAId;
            RoomBId = roomBId;
            OriginalStretchScale = view.StretchRoot.localScale;

            PortalToRoomA = new RoomPortalRuntime(
                connectionId,
                roomAId,
                roomBId,
                view.PortalA,
                connectorA.PortalWidth * roomScale,
                connectorA.PortalHeight * roomScale);
            PortalToRoomB = new RoomPortalRuntime(
                connectionId,
                roomBId,
                roomAId,
                view.PortalB,
                connectorB.PortalWidth * roomScale,
                connectorB.PortalHeight * roomScale);
        }

        private ConnectionViewRuntime(
            int connectionId,
            int roomAId,
            int roomBId,
            RoomConnectorSlot connectorA,
            RoomConnectorSlot connectorB,
            float roomScale)
        {
            ConnectorA = connectorA != null ? connectorA : throw new ArgumentNullException(nameof(connectorA));
            ConnectorB = connectorB != null ? connectorB : throw new ArgumentNullException(nameof(connectorB));
            View = null;

            ValidateRoomScale(roomScale);

            ConnectionId = connectionId;
            RoomAId = roomAId;
            RoomBId = roomBId;
            OriginalStretchScale = Vector3.one;

            PortalToRoomA = new RoomPortalRuntime(
                connectionId,
                roomAId,
                roomBId,
                connectorA.Anchor,
                -connectorA.Anchor.forward,
                connectorA.PortalWidth * roomScale,
                connectorA.PortalHeight * roomScale);
            PortalToRoomB = new RoomPortalRuntime(
                connectionId,
                roomBId,
                roomAId,
                connectorB.Anchor,
                -connectorB.Anchor.forward,
                connectorB.PortalWidth * roomScale,
                connectorB.PortalHeight * roomScale);
        }

        public static ConnectionViewRuntime CreateSeamless(
            int connectionId,
            int roomAId,
            int roomBId,
            RoomConnectorSlot connectorA,
            RoomConnectorSlot connectorB,
            float roomScale)
        {
            return new ConnectionViewRuntime(
                connectionId,
                roomAId,
                roomBId,
                connectorA,
                connectorB,
                roomScale);
        }

        public void SetLocked(bool locked)
        {
            if (View != null)
            {
                SetActive(View.BattleGateRoot, locked);
                return;
            }

            SetActive(ConnectorA.BattleGateRoot, locked);

            if (ConnectorB.BattleGateRoot != ConnectorA.BattleGateRoot)
                SetActive(ConnectorB.BattleGateRoot, locked);
        }

        public bool ContainsRoom(int roomId)
        {
            return roomId == RoomAId || roomId == RoomBId;
        }

        /// <summary>
        /// 获取从当前房间出发、进入另一个房间的 Portal。
        /// </summary>
        public RoomPortalRuntime GetPortalLeavingRoom(int currentRoomId)
        {
            if (currentRoomId == RoomAId)
                return PortalToRoomB;

            if (currentRoomId == RoomBId)
                return PortalToRoomA;

            throw new ArgumentException($"Room {currentRoomId} 不属于 Connection {ConnectionId}。", nameof(currentRoomId));
        }

        private static void ValidateRoomScale(float roomScale)
        {
            if (roomScale <= 0f || float.IsNaN(roomScale) || float.IsInfinity(roomScale))
                throw new ArgumentOutOfRangeException(nameof(roomScale), "房间缩放必须是大于 0 的有限数值。");
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
                target.SetActive(active);
        }
    }
}
