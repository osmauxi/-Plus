namespace ProjectGame.HotFix.Gameplay.Map.Generation
{
    /// <summary>
    /// 地图布局生成策略 
    /// </summary>
    public interface IMapLayoutStrategy
    {
        MapLayoutStrategyType StrategyType { get; }

        /// <summary>
        /// 声明该策略如何呈现房间之间的逻辑连接 
        /// </summary>
        ConnectionPresentationMode ConnectionMode { get; }

        /// <summary>
        /// 算法版本用于日志、存档和网络快照兼容 
        /// 修改生成规则后应递增版本号 
        /// </summary>
        int Version { get; }

        MapLayout Generate(MapGenerationRequest request);
    }
}
