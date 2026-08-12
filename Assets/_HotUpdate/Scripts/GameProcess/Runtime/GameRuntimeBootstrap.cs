using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Gameplay.State;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ProjectGame.HotFix.Gameplay.Runtime
{
    /// <summary>
    /// GameRuntimeScene的运行时启动入口。
    /// </summary>
    public class GameRuntimeBootstrap : NetworkBehaviour
    {
        public static GameRuntimeBootstrap Instance { get; private set; }
        
        private GameStateController _gameStateController => GameStateController.Instance;

        [Tooltip("按照数组顺序初始化，按照相反顺序关闭。")]
        [SerializeField] private MonoBehaviour[] _runtimeServiceComponents;
        [SerializeField] private GameLevelFlowController _levelFlowController;

        [Tooltip("本地初始化完成后，还需要等待这些 Additive 场景加载完成。")]
        [SerializeField] private string[] _requiredSceneNames =
        {
            "UIGameUIScene"
        };

        private readonly HashSet<ulong> _readyClientIds = new();

        private IGameRuntimeService[] _runtimeServices = Array.Empty<IGameRuntimeService>();

        private CancellationTokenSource _runtimeCts;

        private int _initializedServiceCount;
        private bool _isShuttingDown;

        public bool IsLocalRuntimeReady { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            CollectRuntimeServices();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            CancelRuntime();

            _runtimeCts = new CancellationTokenSource();

            RunRuntimeAsync(_runtimeCts.Token).Forget();
        }

        public override void OnNetworkDespawn()
        {
            CancelRuntime();
            ShutdownServicesAsync().Forget();

            base.OnNetworkDespawn();
        }

        private void OnDestroy()
        {
            CancelRuntime();
            ShutdownServicesAsync().Forget();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private async UniTaskVoid RunRuntimeAsync(CancellationToken cancellationToken)
        {
            try
            {
                //等待GameStateController的初始化
                await WaitForGameStateControllerAsync(cancellationToken);

                if (IsServer)
                {
                    _gameStateController.ChangeStateServer(GameState.GameLoading);
                }

                //初始化注册进_runtimeServices内的所有服务。
                await InitializeServicesAsync(cancellationToken);

                //RuntimeScene 加载后显式等待必要的纯 UI Additive 场景。
                await WaitForRequiredScenesAsync(cancellationToken);

                IsLocalRuntimeReady = true;
                //向服务器提交自己的Runtime加载完成
                NotifyServerLocalRuntimeReady();

                //远端客户端报告完成后，本地启动流程到此结束。
                if (!IsServer)
                {
                    return;
                }

                await UniTask.WaitUntil(AreAllConnectedClientsReady,cancellationToken: cancellationToken);

                Debug.Log("[GameRuntimeBootstrap] 所有客户端运行时初始化完成。");

                if (_levelFlowController == null || !_levelFlowController.IsInitialized)
                    throw new InvalidOperationException("GameLevelFlowController 尚未初始化。");

                await _levelFlowController.StartInitialLevelAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning(
                    "[GameRuntimeBootstrap] 运行时初始化已取消。");
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[GameRuntimeBootstrap] 运行时初始化失败：{exception}");
            }
        }
        private async UniTask WaitForGameStateControllerAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() =>
                    _gameStateController != null &&
                    _gameStateController.IsSpawned,
                cancellationToken: cancellationToken);
        }

        private async UniTask InitializeServicesAsync(CancellationToken cancellationToken)
        {
            _initializedServiceCount = 0;

            for (int i = 0; i < _runtimeServices.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                IGameRuntimeService service = _runtimeServices[i];

                if (service.IsInitialized)
                {
                    _initializedServiceCount++;
                    continue;
                }

                await service.InitializeAsync(cancellationToken);

                _initializedServiceCount++;

                Debug.Log($"[GameRuntimeBootstrap] 已初始化服务：" + $"{service.GetType().Name}");
            }
        }

        private async UniTask WaitForRequiredScenesAsync(CancellationToken cancellationToken)
        {
            if (_requiredSceneNames == null)
            {
                return;
            }

            for (int i = 0; i < _requiredSceneNames.Length; i++)
            {
                string sceneName = _requiredSceneNames[i];

                if (string.IsNullOrWhiteSpace(sceneName))
                {
                    continue;
                }

                await UniTask.WaitUntil(() => IsSceneLoaded(sceneName),cancellationToken: cancellationToken);

                Debug.Log($"[GameRuntimeBootstrap] 必要场景已加载：{sceneName}");
            }
        }

        private static bool IsSceneLoaded(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);

            return scene.IsValid() && scene.isLoaded;
        }

        private void NotifyServerLocalRuntimeReady()
        {
            if (!IsClient)
            {
                return;
            }

            if (IsServer)
            {
                //Host不需要给发ServerRpc。
                MarkClientReady(NetworkManager.LocalClientId);
                return;
            }

            ReportRuntimeReadyServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void ReportRuntimeReadyServerRpc(ServerRpcParams rpcParams = default)
        {
            MarkClientReady(rpcParams.Receive.SenderClientId);
        }

        private void MarkClientReady(ulong clientId)
        {
            if (!IsServer)
            {
                return;
            }

            if (!_readyClientIds.Add(clientId))
            {
                return;
            }

            Debug.Log($"[GameRuntimeBootstrap] Client：{clientId} Runtime加载完成。");
        }

        private bool AreAllConnectedClientsReady()
        {
            if (!IsServer || NetworkManager == null || !NetworkManager.IsListening)
            {
                return false;
            }

            var connectedClientIds = NetworkManager.ConnectedClientsIds;

            if (connectedClientIds.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < connectedClientIds.Count; i++)
            {
                if (!_readyClientIds.Contains(connectedClientIds[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private void CollectRuntimeServices()
        {
            if (_runtimeServiceComponents == null || _runtimeServiceComponents.Length == 0)
            {
                _runtimeServices = Array.Empty<IGameRuntimeService>();
                return;
            }

            var services = new List<IGameRuntimeService>(_runtimeServiceComponents.Length);

            for (int i = 0;i < _runtimeServiceComponents.Length;i++)
            {
                MonoBehaviour component = _runtimeServiceComponents[i];

                if (component == null)
                {
                    continue;
                }

                if (!(component is IGameRuntimeService service))
                {
                    throw new InvalidOperationException(
                        $"{component.GetType().FullName} 被配置为运行时服务，" +
                        $"但没有实现 {nameof(IGameRuntimeService)}。");
                }

                services.Add(service);
            }

            _runtimeServices = services.ToArray();
        }

        private async UniTaskVoid ShutdownServicesAsync()
        {
            if (_isShuttingDown)
            {
                return;
            }

            _isShuttingDown = true;

            try
            {
                for (int i = _initializedServiceCount - 1;i >= 0;i--)
                {
                    IGameRuntimeService service = _runtimeServices[i];

                    try
                    {
                        await service.ShutdownAsync(CancellationToken.None);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError(
                            $"[GameRuntimeBootstrap] 关闭服务失败：" +
                            $"{service.GetType().Name}\n{exception}");
                    }
                }
            }
            finally
            {
                _initializedServiceCount = 0;
                IsLocalRuntimeReady = false;
                _readyClientIds.Clear();
                _isShuttingDown = false;
            }
        }

        private void CancelRuntime()
        {
            if (_runtimeCts == null)
            {
                return;
            }

            if (!_runtimeCts.IsCancellationRequested)
            {
                _runtimeCts.Cancel();
            }

            _runtimeCts.Dispose();
            _runtimeCts = null;
        }
    }
}
