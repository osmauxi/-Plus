using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectGame.HotFix.SceneFlow
{
    /// <summary>
    /// Server 侧 NGO Integrated Scene Management 的薄包装。
    /// NGO 自己负责向所有客户端同步 Load/Unload。
    /// </summary>
    public sealed class NgoSceneLoadService
    {
        private readonly NetworkManager _networkManager;

        public NgoSceneLoadService(NetworkManager networkManager)
        {
            _networkManager = networkManager ??
                throw new ArgumentNullException(nameof(networkManager));
        }

        public async UniTask LoadSceneAsync(
            string sceneName,
            float timeoutSeconds,
            CancellationToken cancellationToken)
        {
            EnsureAvailable();
            ValidateSceneName(sceneName);

            Scene existing = SceneManager.GetSceneByName(sceneName);
            if (existing.IsValid() && existing.isLoaded)
                return;

            NetworkSceneManager sceneManager = _networkManager.SceneManager;
            bool completed = false;
            List<ulong> timedOutClients = null;

            void OnCompleted(
                string completedSceneName,
                LoadSceneMode loadMode,
                List<ulong> clientsCompleted,
                List<ulong> clientsTimedOut)
            {
                if (!string.Equals(
                        completedSceneName,
                        sceneName,
                        StringComparison.Ordinal))
                {
                    return;
                }

                timedOutClients = clientsTimedOut;
                completed = true;
            }

            sceneManager.OnLoadEventCompleted += OnCompleted;
            try
            {
                SceneEventProgressStatus status = sceneManager.LoadScene(
                    sceneName,
                    LoadSceneMode.Additive);

                if (status != SceneEventProgressStatus.Started)
                {
                    throw new InvalidOperationException(
                        $"NGO 场景加载未启动：{sceneName}，Status={status}");
                }

                await WaitForCompletionAsync(
                    () => completed,
                    timeoutSeconds,
                    $"NGO 场景加载超时：{sceneName}",
                    cancellationToken);

                ThrowIfClientsTimedOut(sceneName, timedOutClients, "加载");
            }
            finally
            {
                sceneManager.OnLoadEventCompleted -= OnCompleted;
            }
        }

        public async UniTask UnloadSceneAsync(
            string sceneName,
            float timeoutSeconds,
            CancellationToken cancellationToken)
        {
            EnsureAvailable();
            ValidateSceneName(sceneName);

            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded)
                return;

            NetworkSceneManager sceneManager = _networkManager.SceneManager;
            bool completed = false;
            List<ulong> timedOutClients = null;

            void OnCompleted(
                string completedSceneName,
                LoadSceneMode loadMode,
                List<ulong> clientsCompleted,
                List<ulong> clientsTimedOut)
            {
                if (!string.Equals(
                        completedSceneName,
                        sceneName,
                        StringComparison.Ordinal))
                {
                    return;
                }

                timedOutClients = clientsTimedOut;
                completed = true;
            }

            sceneManager.OnUnloadEventCompleted += OnCompleted;
            try
            {
                SceneEventProgressStatus status = sceneManager.UnloadScene(scene);
                if (status != SceneEventProgressStatus.Started)
                {
                    throw new InvalidOperationException(
                        $"NGO 场景卸载未启动：{sceneName}，Status={status}");
                }

                await WaitForCompletionAsync(
                    () => completed,
                    timeoutSeconds,
                    $"NGO 场景卸载超时：{sceneName}",
                    cancellationToken);

                ThrowIfClientsTimedOut(sceneName, timedOutClients, "卸载");
            }
            finally
            {
                sceneManager.OnUnloadEventCompleted -= OnCompleted;
            }
        }

        private void EnsureAvailable()
        {
            if (!_networkManager.IsServer || !_networkManager.IsListening)
                throw new InvalidOperationException("NGO Scene 操作只能由已启动的 Server 发起");

            if (!_networkManager.NetworkConfig.EnableSceneManagement)
                throw new InvalidOperationException("NGO Integrated Scene Management 未启用");

            if (_networkManager.SceneManager == null)
                throw new InvalidOperationException("NetworkSceneManager 不可用");
        }

        private static async UniTask WaitForCompletionAsync(
            Func<bool> isCompleted,
            float timeoutSeconds,
            string timeoutMessage,
            CancellationToken cancellationToken)
        {
            double deadline =
                Time.realtimeSinceStartupAsDouble + timeoutSeconds;

            while (!isCompleted())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (Time.realtimeSinceStartupAsDouble >= deadline)
                    throw new TimeoutException(timeoutMessage);

                await UniTask.Yield(
                    PlayerLoopTiming.Update,
                    cancellationToken);
            }
        }

        private static void ThrowIfClientsTimedOut(
            string sceneName,
            List<ulong> clientsTimedOut,
            string operation)
        {
            if (clientsTimedOut == null || clientsTimedOut.Count == 0)
                return;

            throw new TimeoutException(
                $"NGO 场景{operation}存在超时客户端：{sceneName}；" +
                string.Join(",", clientsTimedOut.Select(id => id.ToString())));
        }

        private static void ValidateSceneName(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                throw new ArgumentException("NGO 场景名不能为空", nameof(sceneName));
        }
    }
}
