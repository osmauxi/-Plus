using System;
using Unity.Netcode;

namespace ProjectGame.HotFix.Gameplay.Network
{
    /// <summary>
    /// NGO与此上层网络框架之间唯一的Tick/Lifecycle桥梁
    /// 维护Tick运行和生命周期的初始化/销毁
    /// 运行时理论上只能创建并初始化一个实例，所有下层网络模块都依赖这个实例的生命周期来驱动Tick和生命周期
    /// </summary>
    public sealed class GameplayNetworkBootstrap
    {
        private readonly NetworkManager _networkManager;
        private bool _subscribedToNetworkTick;

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

            if (!_networkManager.IsListening || _networkManager.NetworkTickSystem == null)
                throw new InvalidOperationException("NGO 尚未开始监听，无法初始化 Gameplay 网络运行时。");

            int ngoTickRate = (int)_networkManager.NetworkConfig.TickRate;
            if (ngoTickRate != Runtime.Config.TickRate)
            {
                throw new InvalidOperationException(
                    $"Gameplay 网络 TickRate={Runtime.Config.TickRate}，NGO TickRate={ngoTickRate}，两者必须一致。");
            }

            Runtime.Initialize(ResolveStartingTick());

            _networkManager.NetworkTickSystem.Tick += HandleNetworkTick;
            _subscribedToNetworkTick = true;
            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (_subscribedToNetworkTick && _networkManager.NetworkTickSystem != null)
                _networkManager.NetworkTickSystem.Tick -= HandleNetworkTick;

            _subscribedToNetworkTick = false;
            Runtime.Shutdown();
            IsInitialized = false;
        }

        private void HandleNetworkTick()
        {
            if (IsInitialized)
                Runtime.Clock.AdvanceOneTick();
        }

        private uint ResolveStartingTick()
        {
            int networkTick = _networkManager.IsServer
                ? _networkManager.ServerTime.Tick
                : _networkManager.LocalTime.Tick;
            return unchecked((uint)networkTick);
        }
    }
}
