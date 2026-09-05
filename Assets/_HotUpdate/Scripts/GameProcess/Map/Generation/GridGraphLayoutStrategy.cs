using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map.Generation
{
    /// <summary>
    /// 基于四方向网格的树形房间布局策略 
    ///
    /// 特点：
    /// 1. 房间不会重叠 
    /// 2. 不创建环，任意房间到起点只有一条路径 
    /// 3. 先生成主路径，保证 Boss 的最低图距离 
    /// 4. Boss 从最远叶子节点中选取 
    /// </summary>
    /*
     * GridGraphLayoutStrategy 核心生成流程概述：
     *
     * 1. 起点初始化：将起点房间（Id: 0）放置在网格原点(0,0) 
     * 2. 构建主路径：优先延伸出一条满足最小 Boss 距离要求的主路径，保证地图的基础深度 
     * 3. 生成分支路径：不断收集当前拓扑网络中合法的边缘位置（严格防止房间重叠与物理粘连导致的隐藏环路），随机向外扩展分支，直到达到设定的目标房间总数 
     * 4. 计算拓扑深度：使用广度优先搜索（BFS）计算并记录所有房间距离起点的最短步数 
     * 5. 选定 Boss 房间：筛选出满足最小距离要求的所有叶子节点（仅有一条连接的房间），从中随机选取距离起点最远的房间作为关底 Boss 房 
     * 6. 坐标与数据转换：分配各个房间的具体类型（起点、战斗、Boss），并将内部的 2D 网格坐标依据房间间距转换为 3D 世界地图数据 
     * 7. 这里只决定房间的类型，不会决定房间具体使用此类型的哪种房间
     */
    public sealed class GridGraphLayoutStrategy : IMapLayoutStrategy
    {
        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        public MapLayoutStrategyType StrategyType => MapLayoutStrategyType.GridGraph;

        public ConnectionPresentationMode ConnectionMode => ConnectionPresentationMode.Seamless;

        public int Version => 2;

        public MapLayout Generate(MapGenerationRequest request)
        {
            request.Validate();

            for (int attempt = 0; attempt < request.Settings.MaxGenerationAttempts; attempt++)
            {
                // 每次失败尝试使用一个可复现的新种子，而不是UnityEngine.Random 
                int attemptSeed = unchecked(request.Seed + attempt * 7919);
                System.Random random = new System.Random(attemptSeed);

                if (TryGenerateLayout(request, random, attempt, out MapLayout layout))
                    return layout;
            }

            throw new InvalidOperationException($"地图生成失败 Seed={request.Seed}，Attempts={request.Settings.MaxGenerationAttempts}");
        }

        private bool TryGenerateLayout(MapGenerationRequest request, System.Random random, int attempt, out MapLayout layout)
        {
            MapGenerationSettings settings = request.Settings;
            int targetRoomCount = random.Next(settings.MinRoomCount, settings.MaxRoomCount + 1);

            List<RoomBuilder> rooms = new List<RoomBuilder>(targetRoomCount);
            List<MapConnectionDefinition> connections = new List<MapConnectionDefinition>(targetRoomCount - 1);
            Dictionary<Vector2Int, int> occupiedPositions = new Dictionary<Vector2Int, int>();

            RoomBuilder startRoom = new RoomBuilder(0, Vector2Int.zero);
            rooms.Add(startRoom);
            occupiedPositions.Add(Vector2Int.zero, startRoom.Id);

            
            //主路径长度至少等于 MinBossDistance 
            //通常取总房间数的一半，让 Boss 不至于离起点过近 
            int mainPathLength = Math.Min(targetRoomCount - 1, Math.Max(settings.MinBossDistance, targetRoomCount / 2));

            if (!TryBuildMainPath(rooms, connections, occupiedPositions, mainPathLength, settings.MaxConnectionsPerRoom, random))
            {
                layout = null;
                return false;
            }

            if (!TryBuildBranches(rooms, connections, occupiedPositions, targetRoomCount, settings.MaxConnectionsPerRoom, random))
            {
                layout = null;
                return false;
            }

            CalculateDistancesFromStart(rooms, startRoom.Id);

            int bossRoomId = SelectBossRoom(rooms, settings.MinBossDistance, random);

            if (bossRoomId < 0)
            {
                layout = null;
                return false;
            }

            AssignBaseRoomTypes(rooms, startRoom.Id, bossRoomId);

            List<MapRoomDefinition> roomDefinitions = BuildRoomDefinitions(rooms, settings.RoomSpacing);

            layout = new MapLayout(
                request.Seed,
                attempt,
                StrategyType,
                Version,
                settings.RoomScale,
                startRoom.Id,
                bossRoomId,
                roomDefinitions,
                connections);
            return true;
        }

        /// <summary>
        /// 生成一条不与自身重叠的主路径，保证地图具有足够深度 
        /// </summary>
        private static bool TryBuildMainPath(List<RoomBuilder> rooms, List<MapConnectionDefinition> connections, Dictionary<Vector2Int, int> occupiedPositions, int pathLength, int maxConnectionsPerRoom, System.Random random)
        {
            int currentRoomId = 0;

            for (int step = 0; step < pathLength; step++)
            {
                RoomBuilder currentRoom = rooms[currentRoomId];

                if (currentRoom.NeighborIds.Count >= maxConnectionsPerRoom)
                    return false;

                List<Vector2Int> candidates = CollectAvailablePositions(currentRoom, occupiedPositions);

                if (candidates.Count == 0)
                    return false;

                Vector2Int nextPosition = candidates[random.Next(candidates.Count)];
                currentRoomId = AddConnectedRoom(currentRoomId, nextPosition, rooms, connections, occupiedPositions);
            }

            return true;
        }

        /// <summary>
        /// 从所有可扩展边界中随机增加支路，直到达到目标房间数 
        /// </summary>
        private static bool TryBuildBranches(List<RoomBuilder> rooms, List<MapConnectionDefinition> connections, Dictionary<Vector2Int, int> occupiedPositions, int targetRoomCount, int maxConnectionsPerRoom, System.Random random)
        {
            while (rooms.Count < targetRoomCount)
            {
                
                //房间数量通常只有十几到几十个，
                //每次重新收集边界更容易保证正确性，不需要维护复杂的增量缓存 
                List<FrontierEdge> frontier = CollectFrontier(rooms, occupiedPositions, maxConnectionsPerRoom);

                if (frontier.Count == 0)
                    return false;

                FrontierEdge selectedEdge = frontier[random.Next(frontier.Count)];
                AddConnectedRoom(selectedEdge.ParentRoomId, selectedEdge.TargetPosition, rooms, connections, occupiedPositions);
            }

            return true;
        }

        private static List<Vector2Int> CollectAvailablePositions(RoomBuilder room, Dictionary<Vector2Int, int> occupiedPositions)
        {
            List<Vector2Int> candidates = new List<Vector2Int>(4);

            for (int i = 0; i < Directions.Length; i++)
            {
                Vector2Int targetPosition = room.GridPosition + Directions[i];

                if (occupiedPositions.ContainsKey(targetPosition))
                    continue;

                
                //新位置只能邻接当前父房间 
                //这样不会出现两个房间物理上贴在一起，却在逻辑图中没有连接的情况 
                if (CountOccupiedNeighbors(targetPosition, occupiedPositions) != 1)
                    continue;

                candidates.Add(targetPosition);
            }

            return candidates;
        }

        private static List<FrontierEdge> CollectFrontier(List<RoomBuilder> rooms, Dictionary<Vector2Int, int> occupiedPositions, int maxConnectionsPerRoom)
        {
            //存放所有可扩展的边界位置，每个边界位置都对应一个父房间 
            List<FrontierEdge> frontier = new List<FrontierEdge>();

            for (int roomIndex = 0; roomIndex < rooms.Count; roomIndex++)
            {
                RoomBuilder room = rooms[roomIndex];

                if (room.NeighborIds.Count >= maxConnectionsPerRoom)
                    continue;

                for (int directionIndex = 0; directionIndex < Directions.Length; directionIndex++)
                {
                    Vector2Int targetPosition = room.GridPosition + Directions[directionIndex];

                    if (occupiedPositions.ContainsKey(targetPosition))
                        continue;

                    //因为当前生成策略为树形，不允许生成环，所以新位置只能邻接当前父房间 
                    if (CountOccupiedNeighbors(targetPosition, occupiedPositions) != 1)
                        continue;

                    frontier.Add(new FrontierEdge(room.Id, targetPosition));
                }
            }

            return frontier;
        }

        private static int CountOccupiedNeighbors(Vector2Int position, Dictionary<Vector2Int, int> occupiedPositions)
        {
            int count = 0;

            for (int i = 0; i < Directions.Length; i++)
            {
                if (occupiedPositions.ContainsKey(position + Directions[i]))
                    count++;
            }

            return count;
        }

        private static int AddConnectedRoom(int parentRoomId, Vector2Int gridPosition, List<RoomBuilder> rooms, List<MapConnectionDefinition> connections, Dictionary<Vector2Int, int> occupiedPositions)
        {
            int roomId = rooms.Count;
            RoomBuilder newRoom = new RoomBuilder(roomId, gridPosition);

            rooms.Add(newRoom);
            occupiedPositions.Add(gridPosition, roomId);

            rooms[parentRoomId].NeighborIds.Add(roomId);
            newRoom.NeighborIds.Add(parentRoomId);

            int connectionId = connections.Count;
            connections.Add(new MapConnectionDefinition(connectionId, parentRoomId, roomId));

            return roomId;
        }

        /// <summary>
        /// BFS计算从起点到所有房间的最短路径距离 
        /// 将距离写入RoomBuilder.DistanceFromStart字段中 
        /// </summary>
        private static void CalculateDistancesFromStart(List<RoomBuilder> rooms, int startRoomId)
        {
            for (int i = 0; i < rooms.Count; i++)
                rooms[i].DistanceFromStart = -1;

            Queue<int> queue = new Queue<int>();
            rooms[startRoomId].DistanceFromStart = 0;
            queue.Enqueue(startRoomId);

            while (queue.Count > 0)
            {
                int currentRoomId = queue.Dequeue();
                RoomBuilder currentRoom = rooms[currentRoomId];

                for (int i = 0; i < currentRoom.NeighborIds.Count; i++)
                {
                    int neighborRoomId = currentRoom.NeighborIds[i];
                    RoomBuilder neighborRoom = rooms[neighborRoomId];

                    if (neighborRoom.DistanceFromStart >= 0)
                        continue;

                    neighborRoom.DistanceFromStart = currentRoom.DistanceFromStart + 1;
                    queue.Enqueue(neighborRoomId);
                }
            }
        }

        /// <summary>
        /// 从距离最远的叶子房间中选择 Boss 
        /// 叶子节点表示它只有一条连接，更适合作为关底房 
        /// </summary>
        private static int SelectBossRoom(List<RoomBuilder> rooms, int minBossDistance, System.Random random)
        {
            int farthestDistance = -1;
            List<int> candidates = new List<int>();
            //所有房间距离已经在CalculateDistancesFromStart中计算过了
            for (int i = 0; i < rooms.Count; i++)
            {
                RoomBuilder room = rooms[i];

                //不能是起点，只能是叶子节点，距离必须大于等于最小Boss距离
                if (room.Id == 0 || room.NeighborIds.Count != 1 || room.DistanceFromStart < minBossDistance)
                    continue;
                //发现了更远的房间，清空候选列表
                if (room.DistanceFromStart > farthestDistance)
                {
                    farthestDistance = room.DistanceFromStart;
                    candidates.Clear();
                    candidates.Add(room.Id);
                }
                else if (room.DistanceFromStart == farthestDistance)
                {
                    //发现了同样远的房间，加入候选列表
                    candidates.Add(room.Id);
                }
            }
            return candidates.Count == 0 ? -1 : candidates[random.Next(candidates.Count)];
        }

        //暂时只支持起点、Boss和普通战斗房间类型，后续可以扩展为商店、宝箱、剧情等特殊房间类型 
        private static void AssignBaseRoomTypes(List<RoomBuilder> rooms, int startRoomId, int bossRoomId)
        {
            for (int i = 0; i < rooms.Count; i++)
                rooms[i].RoomType = RoomType.Combat;

            rooms[startRoomId].RoomType = RoomType.Start;
            rooms[bossRoomId].RoomType = RoomType.Boss;
        }

        private static List<MapRoomDefinition> BuildRoomDefinitions(List<RoomBuilder> rooms, float roomSpacing)
        {
            List<MapRoomDefinition> definitions = new List<MapRoomDefinition>(rooms.Count);

            for (int i = 0; i < rooms.Count; i++)
            {
                RoomBuilder room = rooms[i];
                Vector3 worldPosition = new Vector3(room.GridPosition.x * roomSpacing, 0f, room.GridPosition.y * roomSpacing);

                definitions.Add(new MapRoomDefinition(room.Id, room.RoomType, worldPosition, Quaternion.identity, room.DistanceFromStart));
            }

            return definitions;
        }

        private sealed class RoomBuilder
        {
            public int Id { get; }
            public Vector2Int GridPosition { get; }
            public List<int> NeighborIds { get; } = new List<int>(4);

            public RoomType RoomType;
            public int DistanceFromStart;

            public RoomBuilder(int id, Vector2Int gridPosition)
            {
                Id = id;
                GridPosition = gridPosition;
                RoomType = RoomType.Combat;
                DistanceFromStart = -1;
            }
        }

        private readonly struct FrontierEdge
        {
            public int ParentRoomId { get; }
            public Vector2Int TargetPosition { get; }

            public FrontierEdge(int parentRoomId, Vector2Int targetPosition)
            {
                ParentRoomId = parentRoomId;
                TargetPosition = targetPosition;
            }
        }
    }
}
