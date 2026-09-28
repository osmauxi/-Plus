using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Map.Generation;

namespace ProjectGame.HotFix.Gameplay.Map.View
{
    /// <summary>
    /// 一个已实例化房间的运行时包装，由MapVisualBuilder创建和持有 
    /// 将RoomID与具体View绑定，并缓存RoomConnectorSlot，便于后续连接逻辑使用 
    /// </summary>
    public sealed class RoomViewRuntime
    {
        private readonly RoomConnectorSlot[] _connectors;
        private readonly Dictionary<ConnectorDirection, RoomConnectorSlot> _directionalConnectors = new();

        public int RoomId { get; }
        public RoomType RoomType { get; }
        public RoomView View { get; }
        public RoomMinimapDefinition MinimapDefinition { get; }
        public IReadOnlyList<RoomConnectorSlot> Connectors => _connectors;

        public RoomViewRuntime(int roomId, RoomType roomType, RoomView view)
        {
            View = view != null ? view : throw new ArgumentNullException(nameof(view));
            RoomId = roomId;
            RoomType = roomType;
            MinimapDefinition = view.GetComponent<RoomMinimapDefinition>();

            // 房间实例注册时只扫描一次，后续全部使用缓存 
            _connectors = view.GetComponentsInChildren<RoomConnectorSlot>(true);

            for (int i = 0; i < _connectors.Length; i++)
            {
                RoomConnectorSlot connector = _connectors[i];

                if (connector.Direction == ConnectorDirection.None)
                    continue;

                if (!_directionalConnectors.TryAdd(connector.Direction, connector))
                    throw new InvalidOperationException($"Room {roomId} 中存在重复方向插槽：{connector.Direction}");
            }
        }

        public bool TryGetDirectionalConnector(ConnectorDirection direction, out RoomConnectorSlot connector)
        {
            return _directionalConnectors.TryGetValue(direction, out connector);
        }
    }
}
