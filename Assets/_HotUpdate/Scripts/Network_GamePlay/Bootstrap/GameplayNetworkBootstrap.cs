using System;
using Unity.Netcode;

namespace ProjectGame.HotFix.Gameplay.Network
{
    /// <summary>
    /// 初始化通用 Transport 和会话时间锚点；不订阅 NGO Tick。
    /// Gameplay 的推进由独立 TickDriver 负责。
    /// </summary>
    public sealed class GameplayNetworkBootstrap
    {
        private readonly NetworkManager _networkManager;

        public GameplayNetworkRuntime Runtime { get; }

        public bool IsInitialized { get; private set; }

        public GameplayNetworkBootstrap(NetworkManager networkManager, NetworkSimulationConfig config)
        {
            _networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            Runtime = new GameplayNetworkRuntime(_networkManager, config);
        }

        public void Initialize()
        {
            if (IsInitialized)
                return;

            if (!_networkManager.IsListening)
                throw new InvalidOperationException("NGO 尚未开始监听，无法初始化 Gameplay 网络运行时。");

            if (!_networkManager.IsServer && !_networkManager.IsConnectedClient)
                throw new InvalidOperationException("客户端尚未完成连接，无法取得 Gameplay 会话时间锚点。");

            // 只在启动时读一次同步秒数，按 Gameplay 频率换算，不能复用 NGO 的 Tick 编号。
            double serverTime = _networkManager.ServerTime.Time;
            double localTime = _networkManager.IsServer ? serverTime : _networkManager.LocalTime.Time;
            Runtime.Initialize(Runtime.Clock.GetTickAtTime(localTime), Runtime.Clock.GetTickAtTime(serverTime));

            IsInitialized = true;
        }

        public void Shutdown()
        {
            Runtime.Shutdown();
            IsInitialized = false;
        }
    }
}
