using System;
using ProjectGame.HotFix.Gameplay.Map.Generation;

namespace ProjectGame.HotFix.Gameplay.Map.View
{
    /// <summary>
    /// 配表数据在地图运行时的只读房间模板 
    /// TemplateId 用于网络构建方案和配置查找，PoolId 用于 LocalObjectPool 实例化预制体 
    /// </summary>
    public sealed class RoomTemplateConfig
    {
        public int TemplateId { get; }
        public RoomType RoomType { get; }
        public MapStrategyMask AllowedStrategyMask { get; }
        public string PoolId { get; }
        public ConnectorMask SupportedConnectorMask { get; }
        public bool AllowUnusedConnectors { get; }
        public QuarterTurnMask AllowedRotations { get; }
        public int Priority { get; }
        public float Weight { get; }

        public RoomTemplateConfig(
            int templateId,
            RoomType roomType,
            MapStrategyMask allowedStrategyMask,
            string poolId,
            ConnectorMask supportedConnectorMask,
            bool allowUnusedConnectors,
            QuarterTurnMask allowedRotations,
            int priority,
            float weight)
        {
            TemplateId = templateId;
            RoomType = roomType;
            AllowedStrategyMask = allowedStrategyMask;
            PoolId = poolId?.Trim();
            SupportedConnectorMask = supportedConnectorMask;
            AllowUnusedConnectors = allowUnusedConnectors;
            AllowedRotations = allowedRotations;
            Priority = priority;
            Weight = weight;
        }

        public void Validate()
        {
            if (TemplateId <= 0)
                throw new InvalidOperationException("Room TemplateId 必须大于 0 ");

            if (RoomType == RoomType.None || !Enum.IsDefined(typeof(RoomType), RoomType))
                throw new InvalidOperationException($"Template {TemplateId} 的 RoomType 非法：{(int)RoomType}");

            if (AllowedStrategyMask == MapStrategyMask.None ||
                (AllowedStrategyMask & ~MapStrategyMask.All) != MapStrategyMask.None)
            {
                throw new InvalidOperationException(
                    $"Template {TemplateId} 的 AllowedStrategyMask 非法：{(int)AllowedStrategyMask}");
            }

            if (string.IsNullOrWhiteSpace(PoolId))
                throw new InvalidOperationException($"Template {TemplateId} 没有配置 PoolId ");

            if (SupportedConnectorMask == ConnectorMask.None ||
                (SupportedConnectorMask & ~ConnectorMask.All) != ConnectorMask.None)
            {
                throw new InvalidOperationException(
                    $"Template {TemplateId} 的 SupportedConnectorMask 非法：{(int)SupportedConnectorMask}");
            }

            if (AllowedRotations == QuarterTurnMask.None ||
                (AllowedRotations & ~QuarterTurnMask.All) != QuarterTurnMask.None)
            {
                throw new InvalidOperationException(
                    $"Template {TemplateId} 的 AllowedRotations 非法：{(int)AllowedRotations}");
            }

            if (Weight <= 0f || float.IsNaN(Weight) || float.IsInfinity(Weight))
                throw new InvalidOperationException($"Template {TemplateId} 的权重必须是大于 0 的有限数值 ");
        }

        public bool SupportsStrategy(MapLayoutStrategyType strategyType)
        {
            MapStrategyMask strategyMask = MapStrategyMaskUtility.FromStrategyType(strategyType);
            return strategyMask != MapStrategyMask.None && (AllowedStrategyMask & strategyMask) != 0;
        }
    }
}
