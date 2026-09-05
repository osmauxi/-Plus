using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using ProjectGame.HotFix.Network.Runtime;

namespace ProjectGame.HotFix.SceneFlow
{
    /// <summary>
    /// Server权威场景切换控制器，按
    /// 加载目标场景 → 准备网络Prefab → Spawn/Ready → 初始化 → Commit →
    /// 清理旧Scope → 卸载旧场景 → Activate 的固定顺序完成全端切场。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AddressableSceneBarrier))]
    [RequireComponent(typeof(NetworkScopeBarrier))]
    public sealed class GameSceneFlowController : NetworkBehaviour
    {
        public static GameSceneFlowController Instance { get; private set; }

        [Header("Addressable场景地址")]
        [SerializeField] private string _lobbySceneAddress = "Assets/_HotUpdate/Scenes/LobbyScene.unity";
        [SerializeField] private string _gameRuntimeSceneAddress = "Assets/_HotUpdate/Scenes/GameRunTimeScene.unity";
        [SerializeField] private string _gameUISceneAddress = "Assets/_HotUpdate/Scenes/UIGameUIScene.unity";

        [Header("NGO场景地址")]
        [SerializeField] private string _lobbyNgoSceneName = "LobbyScene";
        [SerializeField] private string _gameRuntimeNgoSceneName = "GameRunTimeScene";
        [SerializeField] private string _gameUINgoSceneName = "UIGameUIScene";

        [Header("网络准备屏障")]
        [SerializeField] private AddressableSceneBarrier _addressableSceneBarrier;
        [SerializeField] private NetworkScopeBarrier _networkScopeBarrier;
        [SerializeField, Min(5f)] private float _operationTimeoutSeconds = 45f;

        private CancellationTokenSource _flowCts;
        private bool _isTransitioning;
        private bool _isRecovering;
        private bool _activationRecoveryQueued;
        private bool _commitReached;
        private bool _localRecoveryStarted;
        private int _recoveryLobbyRevision = -1;

        public bool IsTransitioning => _isTransitioning;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _commitReached = _localRecoveryStarted = _isRecovering = _isTransitioning =
                _activationRecoveryQueued = false;
            _recoveryLobbyRevision = -1;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (_addressableSceneBarrier == null)
                _addressableSceneBarrier = GetComponent<AddressableSceneBarrier>();

            if (_networkScopeBarrier == null)
                _networkScopeBarrier = GetComponent<NetworkScopeBarrier>();
            _networkScopeBarrier.ActivationFailed += HandleActivationFailure;
        }

        public override void OnDestroy()
        {
            CancelCurrentFlow();
            if (_networkScopeBarrier != null)
                _networkScopeBarrier.ActivationFailed -= HandleActivationFailure;

            if (Instance == this)
                Instance = null;

            base.OnDestroy();
        }
        /// <summary>
        /// 场景切换流程：Lobby → GameRuntime + GameUI
        /// </summary>
        public UniTask TransitionToGameSceneAsync()
        {
            PhysicalSceneReference lobby = LobbyScene();
            var gameScenes = new[] { GameRuntimeScene(), GameUIScene() };

            return RunTransitionAsync(
                new SceneTransitionPlan(
                    NetworkSceneMask.Lobby,
                    NetworkSceneMask.GameRuntime | NetworkSceneMask.GameUI,
                    "正在进入游戏...",
                    gameScenes,
                    new[] { lobby }));
        }

        public UniTask TransitionToLobbySceneAsync()
        {

            NetworkSceneMask activeMask = RequireRuntime().ScopeManager.ActiveSceneMask;
            bool isInitialLobby = activeMask == NetworkSceneMask.None;
            var gameScenes = new[] { GameUIScene(), GameRuntimeScene() };

            return RunTransitionAsync(
                new SceneTransitionPlan(
                    isInitialLobby
                        ? NetworkSceneMask.None
                        : NetworkSceneMask.GameRuntime | NetworkSceneMask.GameUI,
                    NetworkSceneMask.Lobby,
                    isInitialLobby ? "正在进入大厅..." : "正在返回大厅...",
                    new[] { LobbyScene() },
                    isInitialLobby
                        ? Array.Empty<PhysicalSceneReference>()
                        : gameScenes));
        }

        public void CancelCurrentFlow()
        {
            if (_flowCts != null && !_flowCts.IsCancellationRequested)
                _flowCts.Cancel();
        }
        private async UniTask RunTransitionAsync(SceneTransitionPlan plan)
        {
            NetworkRuntimeBootstrap bootstrap = RequireRuntime();
            if (!IsServer)
            {
                Debug.LogWarning("只有 Server/Host 可以发起 SceneFlow");
                return;
            }

            if (_isTransitioning)
            {
                Debug.LogWarning("已有 SceneFlow 正在执行，忽略重复请求");
                return;
            }

            NetworkSceneMask activeMask = bootstrap.ScopeManager.ActiveSceneMask;
            if (activeMask == plan.TargetMask)
                return;
            //发出了与当前状态不符的切场请求，说明客户端可能存在异常状态。
            if (activeMask != plan.ExpectedSourceMask)
            {
                throw new InvalidOperationException($"不支持的 SceneFlow 起点：Current={activeMask}，" +$"Expected={plan.ExpectedSourceMask}");
            }

            _flowCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            CancellationToken cancellationToken = _flowCts.Token;
            _isTransitioning = true;
            _recoveryLobbyRevision = -1;

            var loadedTargets = new List<PhysicalSceneReference>();
            bool scopePrepareStarted = false;
            bool commitStarted = false;

            try
            {
                ShowLoadingClientRpc(plan.LoadingMessage);
                //物理加载阶段
                var backend = new SceneFlowBackendRouter(
                    bootstrap,
                    _addressableSceneBarrier,
                    _operationTimeoutSeconds);

                foreach (PhysicalSceneReference scene in plan.ScenesToLoad)
                {
                    //屏障部分失败时，也需要对所有端执行幂等Unload
                    //这里让Client进行场景的加载
                    loadedTargets.Add(scene);
                    await backend.LoadAsync(scene, cancellationToken);
                }
                //网络Scope屏障阶段
                //这里等待所有客户端完成场景所需NetworkPrefab的注册并返回ACK
                //且从Addressable加载了对应Root预制件
                scopePrepareStarted = true;
                await _networkScopeBarrier.PrepareForAllClientsAsync(
                    plan.TargetMask,
                    _operationTimeoutSeconds,
                    cancellationToken);

                //Server Spawn，SpawnPreparedScope完毕说明所有客户端都注册了NetworkPrefab
                //且Server进行了NetworkPrefab的Spawn
                _networkScopeBarrier.SpawnPreparedScope();
                //等待所有客户端的确认，确保所有客户端都已经收到Spawn，
                //完成后能确定目标Scope需要的Root已经在所有客户端生成
                await _networkScopeBarrier.WaitForRootsReadyForAllClientsAsync(
                    _operationTimeoutSeconds,
                    cancellationToken);

                //Bind + Initialize 是一个 RuntimeReady 屏障，合并收一次 ACK。
                //Bind阶段，让生成的Root绑定他自己跑逻辑需要的资源，建立引用关系
                //Initialize阶段，执行Root的初始化逻辑
                //每台机器先执行全部 Bind，再执行全部 Initialize。
                await _networkScopeBarrier.RunPreCommitStagesForAllClientsAsync(
                    _operationTimeoutSeconds,
                    cancellationToken);

                //Commit阶段，这时已经越过安全回滚边界
                //Commit只做为一次权威的状态切换标记，再收一次ACK，内部基本不带其他操作
                commitStarted = true;
                MarkCommitStartedClientRpc();
                await _networkScopeBarrier.CommitForAllClientsAsync(
                    _operationTimeoutSeconds,
                    cancellationToken);

                //Cleanup阶段
                //新场景已经成为正式状态，开始清理旧场景的网络组件，销毁和注销过期的NetworkObject
                //释放Addressable资源等
                await _networkScopeBarrier.CleanupObsoleteScopeForAllClientsAsync(
                    _operationTimeoutSeconds,
                    cancellationToken);
                //销毁旧场景
                foreach (PhysicalSceneReference scene in plan.ScenesToUnload)
                    await backend.UnloadAsync(scene, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                _networkScopeBarrier.ActivateForAllClients();

                Debug.Log($"[GameSceneFlowController] 切场完成：{activeMask} → {plan.TargetMask}");
            }
            catch (Exception exception)
            {
                //Commit前失败，撤销新Root，release新的prefab和Scene，回滚到旧状态
                //Commit后失败，无法回滚，交给后续Recovery流程处理
                if (!commitStarted)
                {
                    if (!await TryRollbackPreCommitAsync(bootstrap,loadedTargets,scopePrepareStarted))
                        await RecoverToLobbyAsync(bootstrap);
                }
                else
                {
                    await RecoverToLobbyAsync(bootstrap);
                }

                Debug.LogError($"[GameSceneFlowController] 切场失败：{exception}");
                throw;
            }
            finally
            {
                if (IsSpawned && IsServer) HideLoadingClientRpc();
                _isTransitioning = false;
                _flowCts?.Dispose();
                _flowCts = null;
            }
        }

        private async UniTask<bool> TryRollbackPreCommitAsync(
            NetworkRuntimeBootstrap bootstrap,
            List<PhysicalSceneReference> loadedTargets,
            bool scopePrepareStarted)
        {
            using (var rollbackCts = new CancellationTokenSource())
            {
                //_operationTimeoutSeconds为最大回滚等待时间
                using var timeout = rollbackCts.CancelAfterSlim(TimeSpan.FromSeconds(_operationTimeoutSeconds), DelayType.Realtime);
                bool succeeded = true;

                if (scopePrepareStarted)
                {
                    try
                    {
                        await _networkScopeBarrier.RollbackForAllClientsAsync(
                            _operationTimeoutSeconds,
                            rollbackCts.Token);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError($"[GameSceneFlowController] Scope Rollback 失败：{exception}");
                        // Do not unload dependencies under a Root/writer that could not be drained.
                        return false;
                    }
                }

                var backend = new SceneFlowBackendRouter(bootstrap,_addressableSceneBarrier,_operationTimeoutSeconds);
                //逆序卸载Scene
                for (int i = loadedTargets.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        await backend.UnloadAsync(loadedTargets[i],rollbackCts.Token);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError("[GameSceneFlowController] 回滚目标物理 Scene 失败：" + exception);
                        succeeded = false;
                    }
                }
                return succeeded;
            }
        }

        [ClientRpc]
        private void MarkCommitStartedClientRpc() => _commitReached = true;

        private void HandleActivationFailure(int revision, string error)
        {
            if (!IsServer || _isRecovering) return;
            Debug.LogError($"[GameSceneFlowController] Revision={revision} Activate 失败：{error}");
            if (revision == _recoveryLobbyRevision)
            {
                // A remote Lobby Activate can fail after the no-ACK recovery flow has returned.
                // It is terminal too; do not recursively rebuild the same failing Lobby.
                if (IsSpawned) ReturnToLobbyLocallyClientRpc();
                StartLocalLobbyRecovery();
                return;
            }
            if (_isTransitioning)
            {
                CancelCurrentFlow();
                if (!_activationRecoveryQueued)
                {
                    _activationRecoveryQueued = true;
                    RecoverAfterCurrentTransitionAsync().Forget();
                }
            }
            else RecoverAfterActivationFailureAsync().Forget();
        }

        /// <summary>
        /// Activate 的异步失败可能在切场 finally 前到达。等待当前流程真正退出后再恢复，
        /// 避免只取消已经越过最后一个 await 的流程而遗漏恢复。
        /// </summary>
        private async UniTaskVoid RecoverAfterCurrentTransitionAsync()
        {
            try
            {
                CancellationToken token = this.GetCancellationTokenOnDestroy();
                await UniTask.WaitUntil(() => !_isTransitioning, cancellationToken: token);
                if (!IsSpawned || NetworkManager == null || !NetworkManager.IsServer || !NetworkManager.IsListening)
                    return;

                NetworkRuntimeBootstrap runtime = NetworkRuntimeBootstrap.Instance;
                if (runtime == null || !runtime.IsInitialized ||
                    runtime.ScopeManager.ActiveSceneMask == NetworkSceneMask.Lobby)
                    return;

                RecoverAfterActivationFailureAsync().Forget();
            }
            catch (OperationCanceledException) { }
            finally { _activationRecoveryQueued = false; }
        }

        private async UniTaskVoid RecoverAfterActivationFailureAsync()
        {
            _isTransitioning = true;
            try { await RecoverToLobbyAsync(RequireRuntime()); }
            catch (Exception exception) { Debug.LogError($"[GameSceneFlowController] 返回大厅失败：{exception}"); }
            finally
            {
                _isTransitioning = false;
                if (IsSpawned && IsServer) HideLoadingClientRpc();
            }
        }

        /// <summary>Partial Commit is discarded; rebuild Lobby from each peer's actual resources.</summary>
        private async UniTask RecoverToLobbyAsync(NetworkRuntimeBootstrap bootstrap)
        {
            if (_isRecovering) return;
            _isRecovering = true;
            try
            {
                ShowLoadingClientRpc("正在返回大厅...");
                // Independent of the failed flow's cancellation token. Each operation is bounded.
                CancellationToken token = this.GetCancellationTokenOnDestroy();
                var backend = new SceneFlowBackendRouter(bootstrap, _addressableSceneBarrier, _operationTimeoutSeconds);
                await _networkScopeBarrier.ResetForRecoveryForAllClientsAsync(_operationTimeoutSeconds, token);
                await backend.LoadAsync(LobbyScene(), token);
                await _networkScopeBarrier.PrepareForAllClientsAsync(NetworkSceneMask.Lobby, _operationTimeoutSeconds, token);
                _recoveryLobbyRevision = _networkScopeBarrier.Revision;
                _networkScopeBarrier.SpawnPreparedScope();
                await _networkScopeBarrier.WaitForRootsReadyForAllClientsAsync(_operationTimeoutSeconds, token);
                await _networkScopeBarrier.RunPreCommitStagesForAllClientsAsync(_operationTimeoutSeconds, token);
                await _networkScopeBarrier.CommitForAllClientsAsync(_operationTimeoutSeconds, token);
                await _networkScopeBarrier.CleanupObsoleteScopeForAllClientsAsync(_operationTimeoutSeconds, token);
                await backend.UnloadAsync(GameUIScene(), token);
                await backend.UnloadAsync(GameRuntimeScene(), token);
                _networkScopeBarrier.ActivateForAllClients();
                Debug.Log("[GameSceneFlowController] 已恢复 Lobby Scope");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[GameSceneFlowController] 联机恢复失败，结束会话并返回大厅：{exception}");
                if (IsSpawned && IsServer) ReturnToLobbyLocallyClientRpc();
                // Allow the reliable notification to enter NGO's send loop before shutdown.
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                StartLocalLobbyRecovery();
            }
            finally { _isRecovering = false; }
        }

        [ClientRpc]
        private void ReturnToLobbyLocallyClientRpc() => StartLocalLobbyRecovery();

        public override void OnNetworkDespawn()
        {
            CancelCurrentFlow();
            // A lost connection cannot receive a recovery RPC. Committed peers still return locally.
            if (_commitReached) StartLocalLobbyRecovery();
            base.OnNetworkDespawn();
        }

        private void StartLocalLobbyRecovery()
        {
            if (_localRecoveryStarted) return;
            _localRecoveryStarted = true;
            SceneFlowLobbyRecovery.ReturnLocallyAsync(NetworkManager, NetworkRuntimeBootstrap.Instance,
                LobbyScene(), GameUIScene(), GameRuntimeScene(), _operationTimeoutSeconds).Forget();
        }

        private static NetworkRuntimeBootstrap RequireRuntime()
        {
            NetworkRuntimeBootstrap bootstrap = NetworkRuntimeBootstrap.Instance;
            if (bootstrap == null || !bootstrap.IsInitialized)
            {
                throw new InvalidOperationException(
                    "NetworkRuntimeBootstrap 必须在 StartHost/StartClient 前 Initialize");
            }

            return bootstrap;
        }

        private PhysicalSceneReference LobbyScene()
        {
            return new PhysicalSceneReference(
                _lobbySceneAddress,
                _lobbyNgoSceneName);
        }

        private PhysicalSceneReference GameRuntimeScene()
        {
            return new PhysicalSceneReference(
                _gameRuntimeSceneAddress,
                _gameRuntimeNgoSceneName);
        }

        private PhysicalSceneReference GameUIScene()
        {
            return new PhysicalSceneReference(
                _gameUISceneAddress,
                _gameUINgoSceneName);
        }

        [ClientRpc]
        private void ShowLoadingClientRpc(string message)
        {
            LoadingScreenService loading = FindLoadingScreenService();
            if (loading != null)
                loading.Show(message);
        }

        [ClientRpc]
        private void HideLoadingClientRpc()
        {
            LoadingScreenService loading = FindLoadingScreenService();
            if (loading != null)
                loading.HideAsync().Forget();
        }

        private static LoadingScreenService FindLoadingScreenService()
        {
            return FindObjectOfType<LoadingScreenService>(true);
        }
    }
}
