using System;

namespace ProjectGame.HotFix.Gameplay.Map.Generation
{
    /// <summary>
    /// 房间的玩法类型。
    /// </summary>
    public enum RoomType : byte
    {
        None = 0,
        Start = 1,
        Combat = 2,
        Elite = 3,
        Treasure = 4,
        Shop = 5,
        Boss = 6
    }

    /// <summary>
    /// 地图空间布局策略。
    /// </summary>
    public enum MapLayoutStrategyType : byte
    {
        None = 0,
        GridGraph = 1,
        Corridor = 2
    }

    /// <summary>
    /// 房间模板允许参与的地图策略。该枚举只用于配置过滤，不等同于策略类型编号。
    /// </summary>
    [Flags]
    public enum MapStrategyMask : ushort
    {
        None = 0,
        GridGraph = 1 << 0,
        Corridor = 1 << 1,
        All = GridGraph | Corridor
    }

    /// <summary>
    /// 逻辑连接在视觉构建阶段的呈现方式。
    /// </summary>
    public enum ConnectionPresentationMode : byte
    {
        None = 0,
        Seamless = 1,
        ConnectionView = 2
    }

    public static class MapStrategyMaskUtility
    {
        public static MapStrategyMask FromStrategyType(MapLayoutStrategyType strategyType)
        {
            return strategyType switch
            {
                MapLayoutStrategyType.GridGraph => MapStrategyMask.GridGraph,
                MapLayoutStrategyType.Corridor => MapStrategyMask.Corridor,
                _ => MapStrategyMask.None
            };
        }
    }

    /// <summary>
    /// 地图生成配置
    /// </summary>
    public readonly struct MapGenerationSettings
    {
        public int MinRoomCount { get; }
        public int MaxRoomCount { get; }
        public int MinBossDistance { get; }
        public int MaxConnectionsPerRoom { get; }
        public float BaseRoomSize { get; }
        public float RoomScale { get; }
        public float RoomSpacing => BaseRoomSize * RoomScale;
        public int MaxGenerationAttempts { get; }

        public MapGenerationSettings(
            int minRoomCount,
            int maxRoomCount,
            int minBossDistance,
            int maxConnectionsPerRoom,
            float baseRoomSize,
            float roomScale,
            int maxGenerationAttempts)
        {
            MinRoomCount = minRoomCount;
            MaxRoomCount = maxRoomCount;
            MinBossDistance = minBossDistance;
            MaxConnectionsPerRoom = maxConnectionsPerRoom;
            BaseRoomSize = baseRoomSize;
            RoomScale = roomScale;
            MaxGenerationAttempts = maxGenerationAttempts;
        }

        public void Validate()
        {
            if (MinRoomCount < 2)
                throw new ArgumentOutOfRangeException(nameof(MinRoomCount), "地图至少需要起点和一个终点房间。");

            if (MaxRoomCount < MinRoomCount)
                throw new ArgumentOutOfRangeException(nameof(MaxRoomCount), "最大房间数不能小于最小房间数。");

            if (MinBossDistance < 1 || MinBossDistance >= MinRoomCount)
                throw new ArgumentOutOfRangeException(nameof(MinBossDistance), "Boss 最小距离必须大于 0，并小于最小房间数。");

            if (MaxConnectionsPerRoom < 2 || MaxConnectionsPerRoom > 4)
                throw new ArgumentOutOfRangeException(nameof(MaxConnectionsPerRoom), "网格房间最大连接数必须在 2 到 4 之间。");

            if (BaseRoomSize <= 0f || float.IsNaN(BaseRoomSize) || float.IsInfinity(BaseRoomSize))
                throw new ArgumentOutOfRangeException(nameof(BaseRoomSize), "房间基础边长必须是大于 0 的有限数值。");

            if (RoomScale <= 0f || float.IsNaN(RoomScale) || float.IsInfinity(RoomScale))
                throw new ArgumentOutOfRangeException(nameof(RoomScale), "房间缩放必须是大于 0 的有限数值。");

            if (MaxGenerationAttempts < 1)
                throw new ArgumentOutOfRangeException(nameof(MaxGenerationAttempts), "至少需要允许一次生成尝试。");
        }
    }

    /// <summary>
    /// 某一层地图的生成请求。
    /// </summary>
    public readonly struct MapGenerationRequest
    {
        public int Seed { get; }
        public int Level { get; }
        public int PlayerCount { get; }
        public MapGenerationSettings Settings { get; }

        public MapGenerationRequest(int seed, int level, int playerCount, MapGenerationSettings settings)
        {
            Seed = seed;
            Level = level;
            PlayerCount = playerCount;
            Settings = settings;
        }

        public void Validate()
        {
            if (Level < 0)
                throw new ArgumentOutOfRangeException(nameof(Level), "关卡层数不能小于 0。");

            if (PlayerCount < 1)
                throw new ArgumentOutOfRangeException(nameof(PlayerCount), "生成地图时至少需要一名玩家。");

            Settings.Validate();
        }
    }
}
