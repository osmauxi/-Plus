using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace ProjectGame.HotFix.Gameplay.Network
{
    /// <summary>
    /// Gameplay网络基础服务的会话级拥有者。
    /// 它不包含任何 Player、Weapon 或 Projectile 业务规则。
    /// </summary>
    public sealed class GameplayNetworkRuntime
    {
        public NetworkSimulationConfig Config { get; }

        public NetworkSimulationClock Clock { get; }

        public NetworkMessageTransport Transport { get; }

        public NetworkTransportStats Stats { get; }

        public IReadOnlyList<ulong> ConnectedClientIds => Transport.ConnectedClientIds;

        public IReadOnlyList<ulong> RemoteClientIds => Transport.RemoteClientIds;

        public ulong LocalClientId => Transport.LocalClientId;

        public ulong ServerClientId => Transport.ServerClientId;

        public bool HasRemoteClients => Transport.HasRemoteClients;

        public bool IsInitialized { get; private set; }

        public GameplayNetworkRuntime(NetworkManager networkManager, NetworkSimulationConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Config.Validate();

            Stats = new NetworkTransportStats();
            Clock = new NetworkSimulationClock(Config.TickRate);
            Transport = new NetworkMessageTransport(
                networkManager ?? throw new ArgumentNullException(nameof(networkManager)),
                Stats);
        }

        public void Initialize(uint startingTick, uint? startingServerTick = null)
        {
            if (IsInitialized)
                return;

            Config.Validate();
            Clock.ResetSession(startingTick, startingServerTick);
            Transport.Initialize();
            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (!IsInitialized)
                return;

            Transport.Shutdown();
            Clock.ResetSession();
            IsInitialized = false;
        }
    }
}
