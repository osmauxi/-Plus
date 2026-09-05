using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Core.NetworkEvents;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using ProjectGame.HotFix.Gameplay.Runtime;
using System;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Network
{
    /// <summary>
    /// Gameplay 阶段网络服务的 Unity 生命周期入口。
    /// 负责建立通用 GameplayNetworkRuntime，并在其上注册当前仍属 Player 专用的同步协议。
    /// </summary>
    public sealed class GameNetworkRuntime : MonoBehaviour, IGameRuntimeService
    {
        [Tooltip("整个 Gameplay 网络会话共享的 Tick 配置，必须与 NGO TickRate 一致。")]
        [SerializeField] private NetworkSimulationConfig _networkSimulationConfig = new();

        public bool IsInitialized { get; private set; }

        /// <summary>Weapon、Projectile、Player 等系统共享的通用网络运行时。</summary>
        public static GameplayNetworkRuntime Gameplay { get; private set; }

        /// <summary>Player 专用协议适配层；底层收发已经委托给通用 Transport。</summary>
        public static PlayerSyncTransport PlayerSync { get; private set; }

        private GameplayNetworkBootstrap _gameplayNetworkBootstrap;

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
            {
                return UniTask.CompletedTask;
            }

            NetworkManager networkManager = NetworkManager.Singleton;

            if (networkManager == null || !networkManager.IsListening)
            {
                throw new InvalidOperationException("NGO 尚未启动，无法初始化 Gameplay 网络运行时。");
            }

            cancellationToken.ThrowIfCancellationRequested();

            RegisterGameplayEvents();

            NetEvents.Initialize(networkManager);

            try
            {
                _gameplayNetworkBootstrap = new GameplayNetworkBootstrap(
                    networkManager,
                    _networkSimulationConfig);
                _gameplayNetworkBootstrap.Initialize();
                Gameplay = _gameplayNetworkBootstrap.Runtime;

                PlayerSync = new PlayerSyncTransport(Gameplay.Transport);
                PlayerSync.Initialize();
            }
            catch
            {
                PlayerSync?.Shutdown();
                PlayerSync = null;
                Gameplay = null;
                _gameplayNetworkBootstrap?.Shutdown();
                _gameplayNetworkBootstrap = null;
                NetEvents.Shutdown();
                throw;
            }

            IsInitialized = true;

            Debug.Log("[GameNetworkRuntime] Gameplay 通用网络运行时与 PlayerSync 协议初始化完成 ");

            return UniTask.CompletedTask;
        }

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            if (!IsInitialized)
            {
                return UniTask.CompletedTask;
            }

            cancellationToken.ThrowIfCancellationRequested();

            PlayerSync?.Shutdown();
            PlayerSync = null;

            _gameplayNetworkBootstrap?.Shutdown();
            _gameplayNetworkBootstrap = null;
            Gameplay = null;

            NetEvents.Shutdown();

            IsInitialized = false;

            Debug.Log("[GameNetworkRuntime] Gameplay 网络运行时已关闭 ");

            return UniTask.CompletedTask;
        }

        private static void RegisterGameplayEvents()
        {
            // 只注册 GameRuntime 阶段会使用的网络事件 
            //
            // NetEvents.Register<PlayerFireEvent>();
            // NetEvents.Register<PlayerUseItemEvent>();
            // NetEvents.Register<MapGeneratedEvent>();
        }
    }
}
