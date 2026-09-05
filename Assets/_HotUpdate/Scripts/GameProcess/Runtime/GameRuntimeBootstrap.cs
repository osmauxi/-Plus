using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Core.Session;
using ProjectGame.HotFix.Gameplay.Player;
using ProjectGame.HotFix.Gameplay.State;
using ProjectGame.HotFix.Network.Runtime;
using ProjectGame.HotFix.SceneFlow;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectGame.HotFix.Gameplay.Runtime
{
    /// <summary>GameRoot 的管线入口。子服务和关卡启动仍由 RunRuntimeAsync 统一编排。</summary>
    public class GameRuntimeBootstrap : NetworkBehaviour, IScopeBindable, IScopeInitializable,
        IScopeActivatable, IScopeShutdown
    {
        public static GameRuntimeBootstrap Instance { get; private set; }

        [Tooltip("按照数组顺序初始化，按照相反顺序关闭")]
        [SerializeField] private MonoBehaviour[] _runtimeServiceComponents;
        [SerializeField] private GameLevelFlowController _levelFlowController;
        [SerializeField] private string[] _requiredSceneNames = { "UIGameUIScene" };
        [Tooltip("本机子服务初始化及 Server 等待业务 RuntimeReady 的最长时间")]
        [SerializeField, Min(1f)] private float _runtimeReadyTimeoutSeconds = 45f;
        [Tooltip("等待角色、武器和 Animator 初始化的最长时间")]
        [SerializeField, Min(1f)] private float _playerRuntimeReadyTimeoutSeconds = 45f;

        private readonly GameRuntimeReadyState _runtimeReady = new();
        private readonly GameRuntimeReadyState _playerReady = new();
        private IGameRuntimeService[] _runtimeServices = Array.Empty<IGameRuntimeService>();
        private GameStateController _gameStateController;
        private NetworkScopeBarrier _scopeBarrier;
        private CancellationTokenSource _runtimeCts;
        private UniTaskCompletionSource _runtimeFinished;
        private UniTaskCompletionSource _playerMonitorFinished;
        private UniTaskCompletionSource _shutdownFinished;
        private int _revision;
        private int _startedServiceCount;
        private bool _prepared;
        private bool _activated;
        private bool _failed;
        private bool _stopping;

        public bool IsLocalRuntimeReady { get; private set; }
        public bool IsLocalPlayerRuntimeReady { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _runtimeCts = new CancellationTokenSource();
            _prepared = _activated = _failed = _stopping = false;
            _startedServiceCount = 0;
            _revision = 0;
            _runtimeFinished = _playerMonitorFinished = _shutdownFinished = null;
            IsLocalRuntimeReady = IsLocalPlayerRuntimeReady = false;
            // Spawn 仅登记；不能在这里触发子服务、地图或玩家生成。
        }

        public UniTask BindAsync(NetworkScopeStageContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!IsSpawned) throw new InvalidOperationException("GameRoot 尚未 Spawn");
            if (!context.TryGetRoot(NetworkPrefabId.NetworkSessionRoot, out NetworkObject sessionRoot))
                throw new InvalidOperationException("找不到 NetworkSessionRoot");
            _scopeBarrier = sessionRoot.GetComponent<NetworkScopeBarrier>();
            if (_scopeBarrier == null || !_scopeBarrier.IsSpawned)
                throw new InvalidOperationException("NetworkSessionRoot 缺少已 Spawn 的 NetworkScopeBarrier");
            _gameStateController = NetworkObject.GetComponentInChildren<GameStateController>(true);
            _revision = context.Revision;
            return UniTask.CompletedTask;
        }

        public UniTask InitializeAsync(NetworkScopeStageContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_revision != context.Revision || _scopeBarrier == null)
                throw new InvalidOperationException("GameRuntimeBootstrap 尚未 Bind");
            if (_gameStateController == null || !_gameStateController.IsSpawned)
                throw new InvalidOperationException("GameRoot 缺少已 Spawn 的 GameStateController");
            if (!GameSessionContext.IsConfigured)
                throw new InvalidOperationException("GameSessionContext 尚未由大厅准备完成");
            ValidateTimeout(_runtimeReadyTimeoutSeconds);
            ValidateTimeout(_playerRuntimeReadyTimeoutSeconds);
            CollectRuntimeServices();
            if (_levelFlowController == null || Array.IndexOf(_runtimeServices, _levelFlowController) < 0)
                throw new InvalidOperationException("GameLevelFlowController 必须属于当前 Root 的服务列表");
            if (_requiredSceneNames != null)
                foreach (string sceneName in _requiredSceneNames)
                    if (!string.IsNullOrWhiteSpace(sceneName) && !IsSceneLoaded(sceneName))
                        throw new InvalidOperationException($"必要场景尚未加载：{sceneName}");
            // 在任何端可能 Activate 前建立 Server 收件箱；Activate 不能清掉先到的结果。
            if (IsServer)
            {
                _runtimeReady.Begin(NetworkManager.ConnectedClientsIds, _revision);
                _playerReady.Begin(NetworkManager.ConnectedClientsIds, _revision);
            }
            _prepared = true;
            return UniTask.CompletedTask;
        }

        public void Activate(NetworkScopeStageContext context)
        {
            if (_activated) return;
            if (!_prepared || _stopping || context.Revision != _revision)
                throw new InvalidOperationException("GameRuntimeBootstrap 尚未完成本轮准备");
            _activated = true;
            _runtimeFinished = new UniTaskCompletionSource();
            CancellationToken token = _runtimeCts.Token;
            WatchLocalStartupAsync(token).Forget();
            RunRuntimeAsync(token).Forget();
        }

        private async UniTask RunRuntimeAsync(CancellationToken cancellationToken)
        {
            try
            {
                await WaitForGameStateControllerAsync(cancellationToken);
                if (IsServer) _gameStateController.ChangeStateServer(GameState.GameLoading);
                await InitializeServicesAsync(cancellationToken);
                await WaitForRequiredScenesAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                IsLocalRuntimeReady = true;
                NotifyReady(false, true, string.Empty);

                // 玩家尚未 Spawn。观察任务先启动，但不能在 Server 生成玩家前阻塞等待它。
                _playerMonitorFinished = new UniTaskCompletionSource();
                MonitorLocalPlayerRuntimeReadyAsync(cancellationToken).Forget();
                if (!IsServer) return;
                await _runtimeReady.WaitAsync(NetworkManager, _runtimeReadyTimeoutSeconds,
                    "Gameplay RuntimeReady", cancellationToken);
                if (_levelFlowController == null || !_levelFlowController.IsInitialized)
                    throw new InvalidOperationException("GameLevelFlowController 尚未初始化");
                await _levelFlowController.StartInitialLevelAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            catch (Exception exception) { FailRuntime(exception, false); }
            finally { _runtimeFinished.TrySetResult(); }
        }

        private async UniTask WatchLocalStartupAsync(CancellationToken cancellationToken)
        {
            try
            {
                double deadline = Time.realtimeSinceStartupAsDouble + _runtimeReadyTimeoutSeconds;
                while (!IsLocalRuntimeReady)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (Time.realtimeSinceStartupAsDouble >= deadline)
                        throw new TimeoutException("本机 Gameplay 服务初始化超时");
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            catch (Exception exception) { FailRuntime(exception, false); }
        }

        private async UniTask WaitForGameStateControllerAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _gameStateController != null && _gameStateController.IsSpawned,
                cancellationToken: cancellationToken);
        }

        private async UniTask InitializeServicesAsync(CancellationToken cancellationToken)
        {
            for (int i = 0; i < _runtimeServices.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                IGameRuntimeService service = _runtimeServices[i];
                // 失败的当前服务也有机会清理已经申请的部分资源。
                _startedServiceCount = i + 1;
                if (!service.IsInitialized) await service.InitializeAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                Debug.Log($"[GameRuntimeBootstrap] 已初始化服务：{service.GetType().Name}");
            }
        }

        private async UniTask WaitForRequiredScenesAsync(CancellationToken cancellationToken)
        {
            if (_requiredSceneNames == null) return;
            foreach (string sceneName in _requiredSceneNames)
                if (!string.IsNullOrWhiteSpace(sceneName))
                    await UniTask.WaitUntil(() => IsSceneLoaded(sceneName), cancellationToken: cancellationToken);
        }

        private static bool IsSceneLoaded(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            return scene.IsValid() && scene.isLoaded;
        }

        private void NotifyReady(bool players, bool succeeded, string error)
        {
            if (!IsClient || !IsSpawned) return;
            if (IsServer) RecordReady(_revision, NetworkManager.LocalClientId, players, succeeded, error);
            else ReportRuntimeReadyServerRpc(_revision, players, succeeded, GameRuntimeReadyState.LimitError(error));
        }

        [ServerRpc(RequireOwnership = false)]
        private void ReportRuntimeReadyServerRpc(int revision, bool players, bool succeeded, string error,
            ServerRpcParams rpcParams = default)
            => RecordReady(revision, rpcParams.Receive.SenderClientId, players, succeeded, error);

        private void RecordReady(int revision, ulong clientId, bool players, bool succeeded, string error)
        {
            if (!IsServer || !_prepared || _stopping || revision != _revision ||
                !NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            (players ? _playerReady : _runtimeReady).Complete(revision, clientId, succeeded, error);
        }

        private async UniTask MonitorLocalPlayerRuntimeReadyAsync(CancellationToken cancellationToken)
        {
            try
            {
                PlayerManager playerManager = PlayerManager.Instance;
                if (playerManager == null || !playerManager.IsInitialized)
                    throw new InvalidOperationException("PlayerManager 尚未初始化");
                await playerManager.WaitUntilAllPlayersInitializedAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                IsLocalPlayerRuntimeReady = true;
                NotifyReady(true, true, string.Empty);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            catch (Exception exception) { FailRuntime(exception, true); }
            finally { _playerMonitorFinished.TrySetResult(); }
        }

        public async UniTask WaitUntilAllPlayerRuntimesReadyAsync(CancellationToken cancellationToken)
        {
            if (!IsServer) throw new InvalidOperationException("只有 Server 可以等待 PlayerRuntimeReady");
            // Dedicated Server 的本机初始化不走客户端 ACK，也不能被跳过。
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _runtimeCts.Token);
            using var timeout = linked.CancelAfterSlim(TimeSpan.FromSeconds(_playerRuntimeReadyTimeoutSeconds), DelayType.Realtime);
            try
            {
                await _playerReady.WaitAsync(NetworkManager, _playerRuntimeReadyTimeoutSeconds,
                    "PlayerRuntimeReady", linked.Token);
                await UniTask.WaitUntil(() => IsLocalPlayerRuntimeReady, cancellationToken: linked.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && !_runtimeCts.IsCancellationRequested)
            {
                throw new TimeoutException("PlayerRuntimeReady 超时");
            }
        }

        private void FailRuntime(Exception exception, bool players)
        {
            if (_failed || _stopping) return;
            _failed = true;
            Debug.LogError($"[GameRuntimeBootstrap] 运行时启动失败，返回 Lobby：{exception}");
            NotifyReady(players, false, exception.Message);
            _runtimeCts?.Cancel();
            _scopeBarrier?.ReportRuntimeFailure(_revision, exception.Message);
        }

        private void CollectRuntimeServices()
        {
            if (_runtimeServiceComponents == null || _runtimeServiceComponents.Length == 0)
                throw new InvalidOperationException("GameRoot 未配置运行时服务");
            var services = new List<IGameRuntimeService>(_runtimeServiceComponents.Length);
            var seen = new HashSet<MonoBehaviour>();
            foreach (MonoBehaviour component in _runtimeServiceComponents)
            {
                if (component == null) throw new InvalidOperationException("运行时服务列表存在空引用");
                if (!seen.Add(component)) throw new InvalidOperationException($"重复服务：{component.name}");
                if (component.GetComponentInParent<NetworkObject>(true) != NetworkObject)
                    throw new InvalidOperationException($"服务不属于当前 GameRoot：{component.name}");
                if (!(component is IGameRuntimeService service))
                    throw new InvalidOperationException($"{component.GetType().Name} 未实现 IGameRuntimeService");
                if (component is IScopeInitializable || component is IScopeActivatable)
                    throw new InvalidOperationException($"{component.name} 同时被 Bootstrap 与管线驱动，会重复启动");
                services.Add(service);
            }
            _runtimeServices = services.ToArray();
        }

        public UniTask ShutdownScopeAsync(CancellationToken cancellationToken)
        {
            if (_shutdownFinished == null)
            {
                _shutdownFinished = new UniTaskCompletionSource();
                ShutdownServicesAsync().Forget();
            }
            return _shutdownFinished.Task.AttachExternalCancellation(cancellationToken);
        }

        private async UniTask ShutdownServicesAsync()
        {
            _stopping = true;
            _runtimeCts?.Cancel();
            try
            {
                if (_runtimeFinished != null) await _runtimeFinished.Task;
                if (_playerMonitorFinished != null) await _playerMonitorFinished.Task;
                List<Exception> failures = null;
                for (int i = _startedServiceCount - 1; i >= 0; i--)
                {
                    try { await _runtimeServices[i].ShutdownAsync(CancellationToken.None); }
                    catch (Exception exception)
                    {
                        (failures ??= new List<Exception>()).Add(exception);
                        Debug.LogError($"[GameRuntimeBootstrap] 关闭 {_runtimeServices[i].GetType().Name} 失败：{exception}");
                    }
                }
                if (failures != null) throw new AggregateException("GameRoot 服务关闭失败", failures);
                _startedServiceCount = 0;
                _shutdownFinished.TrySetResult();
            }
            catch (Exception exception) { _shutdownFinished.TrySetException(exception); }
            finally
            {
                IsLocalRuntimeReady = IsLocalPlayerRuntimeReady = false;
                _runtimeCts?.Dispose();
                _runtimeCts = null;
            }
        }

        public override void OnNetworkDespawn()
        {
            ShutdownScopeAsync(CancellationToken.None).Forget();
            base.OnNetworkDespawn();
        }

        public override void OnDestroy()
        {
            ShutdownScopeAsync(CancellationToken.None).Forget();
            if (Instance == this) Instance = null;
            base.OnDestroy();
        }

        private static void ValidateTimeout(float seconds)
        {
            if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds <= 0)
                throw new InvalidOperationException("Gameplay Ready 超时必须是有限正数");
        }
    }
}
