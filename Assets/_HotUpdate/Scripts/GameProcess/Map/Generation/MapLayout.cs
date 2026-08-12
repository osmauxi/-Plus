using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map.Generation
{
    /// <summary>
    /// 一个未选择视觉模板的房间定义。
    /// 这是不可变生成数据，不保存IsCleared、IsDiscovered等运行时状态。
    /// </summary>
    public readonly struct MapRoomDefinition
    {
        public int RoomId { get; }
        public RoomType RoomType { get; }
        public Vector3 WorldPosition { get; }
        public Quaternion WorldRotation { get; }
        public int DistanceFromStart { get; }

        public MapRoomDefinition(int roomId, RoomType roomType, Vector3 worldPosition, Quaternion worldRotation, int distanceFromStart)
        {
            RoomId = roomId;
            RoomType = roomType;
            WorldPosition = worldPosition;
            WorldRotation = worldRotation;
            DistanceFromStart = distanceFromStart;
        }
    }

    /// <summary>
    /// 两个房间之间的一条逻辑连接。
    ///
    /// 这里暂时不指定门口或走廊模板，
    /// 那些信息由后续 MapVisualBuilder 根据具体房间资源决定。
    /// </summary>
    public readonly struct MapConnectionDefinition
    {
        public int ConnectionId { get; }
        public int RoomAId { get; }
        public int RoomBId { get; }

        public MapConnectionDefinition(int connectionId, int roomAId, int roomBId)
        {
            if (roomAId == roomBId)
                throw new ArgumentException("房间不能连接到自己。");

            ConnectionId = connectionId;
            RoomAId = roomAId;
            RoomBId = roomBId;
        }

        public int GetOtherRoomId(int roomId)
        {
            if (roomId == RoomAId)
                return RoomBId;

            if (roomId == RoomBId)
                return RoomAId;

            throw new ArgumentException($"Room {roomId} 不属于 Connection {ConnectionId}。", nameof(roomId));
        }
    }

    /// <summary>
    /// 一层地图的完整、不可变布局结果。
    /// </summary>
    public sealed class MapLayout
    {
        private readonly MapRoomDefinition[] _rooms;
        private readonly MapConnectionDefinition[] _connections;

        private readonly Dictionary<int, MapRoomDefinition> _roomsById = new();
        private readonly Dictionary<int, int[]> _neighborsByRoomId = new();

        public int Seed { get; }
        public int GenerationAttempt { get; }
        public MapLayoutStrategyType StrategyType { get; }
        public int StrategyVersion { get; }
        public float RoomScale { get; }
        public int StartRoomId { get; }
        public int BossRoomId { get; }

        public IReadOnlyList<MapRoomDefinition> Rooms => _rooms;
        public IReadOnlyList<MapConnectionDefinition> Connections => _connections;

        public MapLayout
            (
            int seed, 
            int generationAttempt, 
            MapLayoutStrategyType strategyType, 
            int strategyVersion,
            float roomScale,
            int startRoomId, 
            int bossRoomId, IReadOnlyList<MapRoomDefinition> rooms, 
            IReadOnlyList<MapConnectionDefinition> connections
            )
        {
            if (rooms == null || rooms.Count == 0)
                throw new ArgumentException("地图至少需要一个房间。", nameof(rooms));

            if (connections == null)
                throw new ArgumentNullException(nameof(connections));

            if (roomScale <= 0f || float.IsNaN(roomScale) || float.IsInfinity(roomScale))
                throw new ArgumentOutOfRangeException(nameof(roomScale), "房间缩放必须是大于 0 的有限数值。");

            Seed = seed;
            GenerationAttempt = generationAttempt;
            StrategyType = strategyType;
            StrategyVersion = strategyVersion;
            RoomScale = roomScale;
            StartRoomId = startRoomId;
            BossRoomId = bossRoomId;

            _rooms = new MapRoomDefinition[rooms.Count];
            _connections = new MapConnectionDefinition[connections.Count];

            //阅读生成的rooms列表，构建房间ID索引，并检查重复ID
            for (int i = 0; i < rooms.Count; i++)
            {
                MapRoomDefinition room = rooms[i];

                if (!_roomsById.TryAdd(room.RoomId, room))
                    throw new InvalidOperationException($"地图中存在重复 RoomId：{room.RoomId}");

                _rooms[i] = room;
            }

            for (int i = 0; i < connections.Count; i++)
                _connections[i] = connections[i];

            if (!_roomsById.ContainsKey(StartRoomId))
                throw new InvalidOperationException($"找不到起点房间：{StartRoomId}");

            if (!_roomsById.ContainsKey(BossRoomId))
                throw new InvalidOperationException($"找不到 Boss 房间：{BossRoomId}");

            BuildNeighborLookup();
        }

        public bool TryGetRoom(int roomId, out MapRoomDefinition room)
        {
            return _roomsById.TryGetValue(roomId, out room);
        }

        public IReadOnlyList<int> GetNeighborRoomIds(int roomId)
        {
            return _neighborsByRoomId.TryGetValue(roomId, out int[] neighbors) ? neighbors : Array.Empty<int>();
        }

        private void BuildNeighborLookup()
        {
            Dictionary<int, List<int>> temporaryLookup = new();

            foreach (int roomId in _roomsById.Keys)
                temporaryLookup.Add(roomId, new List<int>());

            //_connections包括所有房间之间的连接，遍历它们并填充邻居列表
            for (int i = 0; i < _connections.Length; i++)
            {
                MapConnectionDefinition connection = _connections[i];

                if (!temporaryLookup.ContainsKey(connection.RoomAId) || !temporaryLookup.ContainsKey(connection.RoomBId))
                    throw new InvalidOperationException($"Connection {connection.ConnectionId} 引用了不存在的房间。");

                temporaryLookup[connection.RoomAId].Add(connection.RoomBId);
                temporaryLookup[connection.RoomBId].Add(connection.RoomAId);
            }

            foreach (KeyValuePair<int, List<int>> pair in temporaryLookup)
            {
                //对每个房间的邻居列表进行排序，以便在后续使用中保持一致性
                pair.Value.Sort();
                _neighborsByRoomId.Add(pair.Key, pair.Value.ToArray());
            }
        }
    }
}
