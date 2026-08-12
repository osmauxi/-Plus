using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Core.NetworkEvents;
using ProjectGame.HotFix.Gameplay.Runtime;
using System;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Network
{
    /// <summary>
    /// 管理Gameplay阶段NetEventBus的初始化与关闭。
    /// </summary>
    public sealed class GameNetworkRuntime :MonoBehaviour,IGameRuntimeService
    {
        public bool IsInitialized { get; private set; }

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
            {
                return UniTask.CompletedTask;
            }

            NetworkManager networkManager = NetworkManager.Singleton;

            if (networkManager == null || !networkManager.IsListening)
            {
                throw new InvalidOperationException("NGO 尚未启动，无法初始化 Gameplay NetEventBus。");
            }

            cancellationToken.ThrowIfCancellationRequested();

            RegisterGameplayEvents();

            // 按照你最终的 NetEvents API 调整参数。
            NetEvents.Initialize(networkManager);

            IsInitialized = true;

            Debug.Log("[GameNetworkRuntime] Gameplay NetEventBus 初始化完成。");

            return UniTask.CompletedTask;
        }

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            if (!IsInitialized)
            {
                return UniTask.CompletedTask;
            }

            cancellationToken.ThrowIfCancellationRequested();

            NetEvents.Shutdown();

            IsInitialized = false;

            Debug.Log("[GameNetworkRuntime] Gameplay NetEventBus 已关闭。");

            return UniTask.CompletedTask;
        }

        private static void RegisterGameplayEvents()
        {
            // 只注册 GameRuntime 阶段会使用的网络事件。
            //
            // NetEvents.Register<PlayerFireEvent>();
            // NetEvents.Register<PlayerUseItemEvent>();
            // NetEvents.Register<MapGeneratedEvent>();
        }
    }
}
