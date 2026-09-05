using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Network.Runtime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectGame.HotFix.SceneFlow
{
    /// <summary>
    /// Commit后非安全性Rollback，直接关闭NGO重新转到LobbyScene假装什么都没发生
    /// </summary>
    internal static class SceneFlowLobbyRecovery
    {
        internal static async UniTaskVoid ReturnLocallyAsync(NetworkManager manager, NetworkRuntimeBootstrap runtime,
            PhysicalSceneReference lobby, PhysicalSceneReference ui, PhysicalSceneReference game, float timeoutSeconds)
        {
            using var cancellation = new CancellationTokenSource();
            using var timeout = cancellation.CancelAfterSlim(TimeSpan.FromSeconds(timeoutSeconds), DelayType.Realtime);
            try
            {
                //回滚会关NetworkManager，等一帧不打断NGO的正在跑的业务
                await UniTask.Yield(PlayerLoopTiming.Update);
                NetworkSceneBackend backend = runtime != null ? runtime.SceneBackend : NetworkSceneBackend.Addressables;
                if (manager != null && manager.IsListening) 
                    manager.Shutdown();
                await SceneFlowLocalOperation.WaitAsync(
                    () => manager == null || (!manager.IsListening && !manager.ShutdownInProgress),
                    timeoutSeconds, "等待网络会话结束超时", cancellation.Token);

                if (runtime != null && runtime.IsInitialized) 
                    runtime.ResetAfterShutdown();
                //直接尝试卸载Runtime两个Scene加载LobbyScene
                if (backend == NetworkSceneBackend.Addressables)
                {
                    AddressableSceneLoadService loader = AddressableSceneLoadService.Shared;
                    await loader.LoadSceneAsync(lobby.AddressableAddress, LoadSceneMode.Additive, cancellation.Token);
                    await loader.UnloadSceneAsync(ui.AddressableAddress, cancellation.Token);
                    await loader.UnloadSceneAsync(game.AddressableAddress, cancellation.Token);
                }
                else
                {
                    AsyncOperation load = SceneManager.LoadSceneAsync(lobby.NgoSceneName, LoadSceneMode.Single);
                    await SceneFlowLocalOperation.WaitAsync(() => load != null && load.isDone,
                        timeoutSeconds, "返回 LobbyScene 超时", cancellation.Token);
                }
                Debug.Log("[SceneFlow] 已结束失效会话并返回本地 LobbyScene");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[SceneFlow] 本地 Lobby 恢复失败：{exception}");
            }
        }
    }
}
