using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Map.Generation;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map.View
{
    /// <summary>
    /// 已完成模板选择、可直接用于构建房间的最终数据。
    ///
    /// 与 MapRoomDefinition 的职责不同：
    /// MapRoomDefinition 只服务于地图生成；
    /// MapRoomBuildDefinition 同时服务于本地构建和网络同步。
    /// </summary>
    public readonly struct MapRoomBuildDefinition
    {
        public int RoomId { get; }
        public RoomType RoomType { get; }
        public int TemplateId { get; }
        public Vector3 WorldPosition { get; }
        public Quaternion LayoutRotation { get; }
        public int DistanceFromStart { get; }
        public ConnectorMask RequiredWorldConnectors { get; }
        public int RotationIndex { get; }

        /// <summary>
        /// 当前房间模板只允许四分之一圈旋转，因此无需额外同步 Quaternion。
        /// </summary>
        public Quaternion WorldRotation => Quaternion.Euler(0f, RotationIndex * 90f, 0f);

        /// <summary>
        /// 本地连接掩码完全由世界连接掩码和旋转次数决定，不重复存储。
        /// </summary>
        public ConnectorMask RequiredLocalConnectors => ConnectorMaskUtility.WorldToLocal(RequiredWorldConnectors, RotationIndex);

        public MapRoomBuildDefinition
        (
            int roomId,
            RoomType roomType,
            int templateId,
            Vector3 worldPosition,
            Quaternion layoutRotation,
            int distanceFromStart,
            ConnectorMask requiredWorldConnectors,
            int rotationIndex
        )
        {
            if (rotationIndex < 0 || rotationIndex > 3)
                throw new ArgumentOutOfRangeException(nameof(rotationIndex), "房间旋转索引必须在 0 到 3 之间。");

            RoomId = roomId;
            RoomType = roomType;
            TemplateId = templateId;
            WorldPosition = worldPosition;
            LayoutRotation = layoutRotation;
            DistanceFromStart = distanceFromStart;
            RequiredWorldConnectors = requiredWorldConnectors;
            RotationIndex = rotationIndex;
        }
    }

    /// <summary>
    /// 一层地图完成生成和模板选择后的唯一构建数据。
    ///
    /// Server 将同一份 MapBuildPlan 用于本地构建和 ClientRpc；
    /// Client 接收后直接构建，不再经过 Snapshot 与 VisualPlan 的双向转换。
    /// </summary>
    public struct MapBuildPlan : INetworkSerializable
    {
        private const int MaxRoomCount = 256;
        private const int MaxConnectionCount = 512;

        private int _seed;
        private int _generationAttempt;
        private byte _strategyTypeValue;
        private int _strategyVersion;
        private byte _connectionPresentationModeValue;
        private float _roomScale;
        private int _startRoomId;
        private int _bossRoomId;

        private MapRoomBuildDefinition[] _rooms;
        private MapConnectionDefinition[] _connections;

        public int Seed => _seed;
        public int GenerationAttempt => _generationAttempt;
        public MapLayoutStrategyType StrategyType => (MapLayoutStrategyType)_strategyTypeValue;
        public int StrategyVersion => _strategyVersion;
        public ConnectionPresentationMode ConnectionMode =>
            (ConnectionPresentationMode)_connectionPresentationModeValue;
        public float RoomScale => _roomScale;
        public int StartRoomId => _startRoomId;
        public int BossRoomId => _bossRoomId;

        public IReadOnlyList<MapRoomBuildDefinition> Rooms => _rooms ?? Array.Empty<MapRoomBuildDefinition>();
        public IReadOnlyList<MapConnectionDefinition> Connections => _connections ?? Array.Empty<MapConnectionDefinition>();

        public bool IsValid => _rooms != null && _rooms.Length > 0 && _connections != null;

        public MapBuildPlan(
            MapLayout layout,
            IReadOnlyList<MapRoomBuildDefinition> rooms,
            ConnectionPresentationMode connectionPresentationMode)
        {
            if (layout == null)
                throw new ArgumentNullException(nameof(layout));

            if (rooms == null || rooms.Count != layout.Rooms.Count)
                throw new ArgumentException("构建房间数量必须与 MapLayout 一致。", nameof(rooms));

            _seed = layout.Seed;
            _generationAttempt = layout.GenerationAttempt;
            _strategyTypeValue = (byte)layout.StrategyType;
            _strategyVersion = layout.StrategyVersion;
            _connectionPresentationModeValue = (byte)connectionPresentationMode;
            _roomScale = layout.RoomScale;
            _startRoomId = layout.StartRoomId;
            _bossRoomId = layout.BossRoomId;

            _rooms = new MapRoomBuildDefinition[rooms.Count];
            _connections = new MapConnectionDefinition[layout.Connections.Count];

            for (int i = 0; i < rooms.Count; i++)
                _rooms[i] = rooms[i];

            for (int i = 0; i < layout.Connections.Count; i++)
                _connections[i] = layout.Connections[i];

            Validate();
        }

        /// <summary>
        /// Client 在需要逻辑布局查询时，可由最终构建数据恢复 MapLayout。
        /// 这里只恢复数据，不重新执行地图生成或模板选择。
        /// </summary>
        public MapLayout ToLayout()
        {
            Validate();

            MapRoomDefinition[] layoutRooms = new MapRoomDefinition[_rooms.Length];

            for (int i = 0; i < _rooms.Length; i++)
            {
                MapRoomBuildDefinition room = _rooms[i];
                layoutRooms[i] = new MapRoomDefinition(room.RoomId, room.RoomType, room.WorldPosition, room.LayoutRotation, room.DistanceFromStart);
            }

            return new MapLayout(
                _seed,
                _generationAttempt,
                StrategyType,
                _strategyVersion,
                _roomScale,
                _startRoomId,
                _bossRoomId,
                layoutRooms,
                _connections);
        }

        public void Validate()
        {
            if (_rooms == null || _rooms.Length == 0 || _rooms.Length > MaxRoomCount)
                throw new InvalidOperationException($"地图构建房间数量无效：{_rooms?.Length ?? 0}");

            if (_connections == null || _connections.Length > MaxConnectionCount)
                throw new InvalidOperationException($"地图构建连接数量无效：{_connections?.Length ?? 0}");

            if (!Enum.IsDefined(typeof(MapLayoutStrategyType), StrategyType) || StrategyType == MapLayoutStrategyType.None)
                throw new InvalidOperationException($"未知地图策略：{_strategyTypeValue}");

            if (!Enum.IsDefined(typeof(ConnectionPresentationMode), ConnectionMode) ||
                ConnectionMode == ConnectionPresentationMode.None)
            {
                throw new InvalidOperationException($"未知连接呈现模式：{_connectionPresentationModeValue}");
            }

            if (_roomScale <= 0f || float.IsNaN(_roomScale) || float.IsInfinity(_roomScale))
                throw new InvalidOperationException($"地图房间缩放无效：{_roomScale}");

            HashSet<int> roomIds = new HashSet<int>();

            for (int i = 0; i < _rooms.Length; i++)
            {
                MapRoomBuildDefinition room = _rooms[i];

                if (!roomIds.Add(room.RoomId))
                    throw new InvalidOperationException($"MapBuildPlan 中存在重复 RoomId：{room.RoomId}");

                if (!Enum.IsDefined(typeof(RoomType), room.RoomType) || room.RoomType == RoomType.None)
                    throw new InvalidOperationException($"Room {room.RoomId} 的 RoomType 无效：{room.RoomType}");

                if (room.TemplateId <= 0)
                    throw new InvalidOperationException($"Room {room.RoomId} 的 TemplateId 无效：{room.TemplateId}");

                if (room.DistanceFromStart < 0)
                    throw new InvalidOperationException($"Room {room.RoomId} 的起点距离无效：{room.DistanceFromStart}");

                if (room.RotationIndex < 0 || room.RotationIndex > 3)
                    throw new InvalidOperationException($"Room {room.RoomId} 的旋转索引无效：{room.RotationIndex}");

                if ((room.RequiredWorldConnectors & ~ConnectorMask.All) != 0)
                    throw new InvalidOperationException($"Room {room.RoomId} 的世界连接掩码无效：{room.RequiredWorldConnectors}");
            }

            if (!roomIds.Contains(_startRoomId))
                throw new InvalidOperationException($"找不到起点房间：{_startRoomId}");

            if (!roomIds.Contains(_bossRoomId))
                throw new InvalidOperationException($"找不到 Boss 房间：{_bossRoomId}");

            HashSet<int> connectionIds = new HashSet<int>();

            for (int i = 0; i < _connections.Length; i++)
            {
                MapConnectionDefinition connection = _connections[i];

                if (!connectionIds.Add(connection.ConnectionId))
                    throw new InvalidOperationException($"MapBuildPlan 中存在重复 ConnectionId：{connection.ConnectionId}");

                if (connection.RoomAId == connection.RoomBId)
                    throw new InvalidOperationException($"Connection {connection.ConnectionId} 连接到了同一个房间。");

                if (!roomIds.Contains(connection.RoomAId) || !roomIds.Contains(connection.RoomBId))
                    throw new InvalidOperationException($"Connection {connection.ConnectionId} 引用了不存在的房间。");
            }
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _seed);
            serializer.SerializeValue(ref _generationAttempt);
            serializer.SerializeValue(ref _strategyTypeValue);
            serializer.SerializeValue(ref _strategyVersion);
            serializer.SerializeValue(ref _connectionPresentationModeValue);
            serializer.SerializeValue(ref _roomScale);
            serializer.SerializeValue(ref _startRoomId);
            serializer.SerializeValue(ref _bossRoomId);

            int roomCount = _rooms?.Length ?? 0;
            serializer.SerializeValue(ref roomCount);

            if (serializer.IsReader)
            {
                if (roomCount < 0 || roomCount > MaxRoomCount)
                    throw new InvalidOperationException($"接收到非法房间数量：{roomCount}");

                _rooms = new MapRoomBuildDefinition[roomCount];
            }

            for (int i = 0; i < roomCount; i++)
                SerializeRoom(serializer, i);

            int connectionCount = _connections?.Length ?? 0;
            serializer.SerializeValue(ref connectionCount);

            if (serializer.IsReader)
            {
                if (connectionCount < 0 || connectionCount > MaxConnectionCount)
                    throw new InvalidOperationException($"接收到非法连接数量：{connectionCount}");

                _connections = new MapConnectionDefinition[connectionCount];
            }

            for (int i = 0; i < connectionCount; i++)
                SerializeConnection(serializer, i);
        }

        private void SerializeRoom<T>(BufferSerializer<T> serializer, int roomIndex) where T : IReaderWriter
        {
            MapRoomBuildDefinition room = serializer.IsReader ? default : _rooms[roomIndex];

            int roomId = room.RoomId;
            byte roomTypeValue = (byte)room.RoomType;
            int templateId = room.TemplateId;
            Vector3 worldPosition = room.WorldPosition;
            Quaternion layoutRotation = room.LayoutRotation;
            int distanceFromStart = room.DistanceFromStart;
            byte requiredWorldMaskValue = (byte)room.RequiredWorldConnectors;
            byte rotationIndex = (byte)room.RotationIndex;

            serializer.SerializeValue(ref roomId);
            serializer.SerializeValue(ref roomTypeValue);
            serializer.SerializeValue(ref templateId);
            serializer.SerializeValue(ref worldPosition);
            serializer.SerializeValue(ref layoutRotation);
            serializer.SerializeValue(ref distanceFromStart);
            serializer.SerializeValue(ref requiredWorldMaskValue);
            serializer.SerializeValue(ref rotationIndex);

            if (serializer.IsReader)
            {
                _rooms[roomIndex] = new MapRoomBuildDefinition
                (
                    roomId,
                    (RoomType)roomTypeValue,
                    templateId,
                    worldPosition,
                    layoutRotation,
                    distanceFromStart,
                    (ConnectorMask)requiredWorldMaskValue,
                    rotationIndex
                );
            }
        }

        private void SerializeConnection<T>(BufferSerializer<T> serializer, int connectionIndex) where T : IReaderWriter
        {
            MapConnectionDefinition connection = serializer.IsReader ? default : _connections[connectionIndex];

            int connectionId = connection.ConnectionId;
            int roomAId = connection.RoomAId;
            int roomBId = connection.RoomBId;

            serializer.SerializeValue(ref connectionId);
            serializer.SerializeValue(ref roomAId);
            serializer.SerializeValue(ref roomBId);

            if (serializer.IsReader)
                _connections[connectionIndex] = new MapConnectionDefinition(connectionId, roomAId, roomBId);
        }
    }
}
