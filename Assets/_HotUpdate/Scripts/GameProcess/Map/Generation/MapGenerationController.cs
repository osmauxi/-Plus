using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Gameplay.Map.View;
using ProjectGame.HotFix.Gameplay.Runtime;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map.Generation
{
    /// <summary>
    /// 地图阶段的网络编排器。
    ///
    /// Server：
    /// 1. 生成地图拓扑。
    /// 2. 选择视觉模板。
    /// 3. 构建本地地图。
    /// 4. 向 Client 发送同一份构建方案。
    /// 5. 等待所有 Client 完成地图构建。
    ///
    /// Client：
    /// 1. 接收 Server 的 MapBuildPlan。
    /// 2. 直接构建本地地图视觉。
    /// 3. 向 Server 报告 Ready。
    /// </summary>
    public sealed class MapGenerationController : NetworkBehaviour, IGameRuntimeService
    {
        [SerializeField] private RoomTemplateCatalog _templateCatalog;
        [SerializeField] private MapVisualBuilder _visualBuilder;

        [Header("图格生成")]
        [SerializeField] private GridMapGenerationProfile _gridProfile = new GridMapGenerationProfile();

        [Header("网络设置")]
        [Tooltip("Server 等待所有客户端完成地图视觉构建的最长时间。")]
        [SerializeField, Min(1f)] private float _clientReadyTimeoutSeconds = 20f;

        private readonly HashSet<ulong> _readyClientIds = new HashSet<ulong>();
        private readonly List<ulong> _clientTargetBuffer = new List<ulong>();

        private NetworkManager _networkManager;
        private IMapLayoutStrategy _layoutStrategy;
        private GridRoomTemplateSelector _templateSelector;

        private CancellationTokenSource _lifetimeCts;
        private CancellationTokenSource _clientBuildCts;

        private int _currentGenerationId;

        public bool IsInitialized { get; private set; }

        public int CurrentGenerationId => _currentGenerationId;
        public MapLayout CurrentLayout { get; private set; }
        public MapBuildPlan CurrentBuildPlan { get; private set; }

        private void Awake()
        {
            _lifetimeCts = new CancellationTokenSource();
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return;

            // NetworkBehaviour 必须已经完成 Spawn，才能安全使用 RPC。
            await UniTask.WaitUntil(() => IsSpawned, cancellationToken: cancellationToken);

            _networkManager = NetworkManager;

            if (_networkManager == null || !_networkManager.IsListening)
                throw new InvalidOperationException("NGO 尚未启动，无法初始化 MapGenerationController。");

            if (_templateCatalog == null || !_templateCatalog.IsInitialized)
                throw new InvalidOperationException("RoomTemplateCatalog 尚未初始化。");

            if (_visualBuilder == null || !_visualBuilder.IsInitialized)
                throw new InvalidOperationException("MapVisualBuilder 尚未初始化。");

            _gridProfile.Validate();

            _layoutStrategy = new GridGraphLayoutStrategy();
            _templateSelector = new GridRoomTemplateSelector();

            _networkManager.OnClientDisconnectCallback += OnClientDisconnected;

            IsInitialized = true;

            Debug.Log($"[{nameof(MapGenerationController)}] 初始化完成。");
        }

        /// <summary>
        /// Server 生成并构建当前层地图。
        /// 返回时保证当前所有已连接客户端都完成了地图视觉构建。
        /// </summary>
        public async UniTask<MapBuildResult> GenerateAndBuildAsync(int level, int playerCount, CancellationToken cancellationToken, int? seedOverride = null)
        {
            EnsureInitialized();
            EnsureServer();

            int seed = seedOverride ?? UnityEngine.Random.Range(1, int.MaxValue);
            int generationId = ++_currentGenerationId;

            MapGenerationSettings settings = _gridProfile.CreateSettings(seed);
            MapGenerationRequest request = new MapGenerationRequest(seed, level, playerCount, settings);

            MapLayout layout = _layoutStrategy.Generate(request);
            MapBuildPlan buildPlan = _templateSelector.Resolve(
                layout,
                _templateCatalog.Templates,
                _layoutStrategy.ConnectionMode);

            _readyClientIds.Clear();

            // Server 先完成自己的地图构建。
            // 构建失败时不会向其他客户端发送一份无效方案。
            await _visualBuilder.BuildAsync(buildPlan, cancellationToken);

            CurrentLayout = layout;
            CurrentBuildPlan = buildPlan;

            // Host同时也是一个Client，需要记录本地地图已经完成。
            if (_networkManager.IsClient)
                _readyClientIds.Add(_networkManager.LocalClientId);

            SendBuildPlanToRemoteClients(generationId, buildPlan);
            await WaitForAllConnectedClientsReadyAsync(generationId, cancellationToken);

            Debug.Log(
                $"[{nameof(MapGenerationController)}] 地图构建完成，Generation={generationId}，Seed={seed}，" +
                $"Scale={layout.RoomScale}，Rooms={layout.Rooms.Count}");

            return new MapBuildResult(generationId, layout, buildPlan);
        }

        /// <summary>
        /// 在客户端完成 Runtime Ready 后，可由上层重连流程调用，
        /// 向指定客户端重新发送当前地图构建方案。
        /// </summary>
        public void SendCurrentMapToClientServer(ulong clientId)
        {
            EnsureInitialized();
            EnsureServer();

            if (!CurrentBuildPlan.IsValid)
            {
                Debug.LogWarning($"[{nameof(MapGenerationController)}] 当前没有可发送的地图构建方案。");
                return;
            }

            if (!IsConnectedClient(clientId))
            {
                Debug.LogWarning($"[{nameof(MapGenerationController)}] Client {clientId} 当前未连接。");
                return;
            }

            ClientRpcParams rpcParams = CreateTargetClientRpcParams(clientId);
            ReceiveMapBuildPlanClientRpc(_currentGenerationId, CurrentBuildPlan, rpcParams);
        }

        /// <summary>
        /// Server 和所有 Client 清理当前地图。
        /// 通常在返回大厅或切换到下一层前调用。
        /// </summary>
        public async UniTask ClearCurrentMapAsync(CancellationToken cancellationToken)
        {
            EnsureInitialized();
            EnsureServer();

            int clearGenerationId = ++_currentGenerationId;

            ClearMapVisualsClientRpc(clearGenerationId);
            await _visualBuilder.ClearMapVisualsAsync(cancellationToken);

            CurrentLayout = null;
            CurrentBuildPlan = default;

            _readyClientIds.Clear();
        }

        private void SendBuildPlanToRemoteClients(int generationId, MapBuildPlan buildPlan)
        {
            _clientTargetBuffer.Clear();

            IReadOnlyList<ulong> connectedClientIds = _networkManager.ConnectedClientsIds;

            for (int i = 0; i < connectedClientIds.Count; i++)
            {
                ulong clientId = connectedClientIds[i];

                // Host 已经在本机直接 Build，不需要再接收自己的 ClientRpc。
                if (_networkManager.IsClient && clientId == _networkManager.LocalClientId)
                    continue;

                _clientTargetBuffer.Add(clientId);
            }

            if (_clientTargetBuffer.Count == 0)
                return;

            ClientRpcParams rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = _clientTargetBuffer.ToArray()
                }
            };

            ReceiveMapBuildPlanClientRpc(generationId, buildPlan, rpcParams);
        }

        [ClientRpc]
        private void ReceiveMapBuildPlanClientRpc(int generationId, MapBuildPlan buildPlan, ClientRpcParams clientRpcParams = default)
        {
            if (IsServer)
                return;

            HandleReceivedBuildPlanAsync(generationId, buildPlan, _lifetimeCts.Token).Forget();
        }

        private async UniTaskVoid HandleReceivedBuildPlanAsync(int generationId, MapBuildPlan buildPlan, CancellationToken lifetimeToken)
        {
            try
            {
                await UniTask.WaitUntil(() => IsInitialized, cancellationToken: lifetimeToken);

                //过滤迟到包。
                if (generationId < _currentGenerationId)
                    return;

                buildPlan.Validate();

                //新地图到达时取消旧构建防止上一层的异步加载晚于新地图完成后继续写入场景
                CancelClientBuild();

                _clientBuildCts = CancellationTokenSource.CreateLinkedTokenSource(lifetimeToken);
                CancellationToken buildToken = _clientBuildCts.Token;

                await _visualBuilder.BuildAsync(buildPlan, buildToken);

                _currentGenerationId = generationId;
                CurrentBuildPlan = buildPlan;
                CurrentLayout = buildPlan.ToLayout();

                ReportMapReadyServerRpc(generationId);
            }
            catch (OperationCanceledException)
            {
                //新地图或场景关闭导致的正常取消不报错
            }
            catch (Exception exception)
            {
                Debug.LogError($"[{nameof(MapGenerationController)}] Client 地图构建失败：\n{exception}");
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ReportMapReadyServerRpc(int generationId, ServerRpcParams rpcParams = default)
        {
            if (!IsInitialized || generationId != _currentGenerationId)
                return;

            ulong senderClientId = rpcParams.Receive.SenderClientId;

            if (!IsConnectedClient(senderClientId))
                return;

            if (_readyClientIds.Add(senderClientId))
                Debug.Log($"[{nameof(MapGenerationController)}] Client 地图 Ready：{senderClientId}");
        }

        private async UniTask WaitForAllConnectedClientsReadyAsync(int generationId, CancellationToken cancellationToken)
        {
            float deadline = Time.realtimeSinceStartup + _clientReadyTimeoutSeconds;

            while (!AreAllConnectedClientsReady(generationId))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (Time.realtimeSinceStartup >= deadline)
                {
                    string waitingClients = BuildWaitingClientText();
                    throw new TimeoutException($"等待客户端地图 Ready 超时。Generation={generationId}，Waiting={waitingClients}");
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }

        private bool AreAllConnectedClientsReady(int generationId)
        {
            if (generationId != _currentGenerationId)
                return false;

            IReadOnlyList<ulong> connectedClientIds = _networkManager.ConnectedClientsIds;

            for (int i = 0; i < connectedClientIds.Count; i++)
            {
                if (!_readyClientIds.Contains(connectedClientIds[i]))
                    return false;
            }

            return true;
        }

        private string BuildWaitingClientText()
        {
            IReadOnlyList<ulong> connectedClientIds = _networkManager.ConnectedClientsIds;
            List<ulong> waitingClients = new List<ulong>();

            for (int i = 0; i < connectedClientIds.Count; i++)
            {
                ulong clientId = connectedClientIds[i];

                if (!_readyClientIds.Contains(clientId))
                    waitingClients.Add(clientId);
            }

            return waitingClients.Count == 0 ? "None" : string.Join(",", waitingClients);
        }

        [ClientRpc]
        private void ClearMapVisualsClientRpc(int clearGenerationId)
        {
            if (IsServer)
                return;

            HandleClearMapAsync(clearGenerationId, _lifetimeCts.Token).Forget();
        }

        private async UniTaskVoid HandleClearMapAsync(int clearGenerationId, CancellationToken lifetimeToken)
        {
            try
            {
                if (clearGenerationId < _currentGenerationId)
                    return;

                CancelClientBuild();

                _currentGenerationId = clearGenerationId;

                await _visualBuilder.ClearMapVisualsAsync(lifetimeToken);

                CurrentLayout = null;
                CurrentBuildPlan = default;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogError($"[{nameof(MapGenerationController)}] Client 清理地图失败：\n{exception}");
            }
        }

        private ClientRpcParams CreateTargetClientRpcParams(ulong clientId)
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            };
        }

        private bool IsConnectedClient(ulong clientId)
        {
            IReadOnlyList<ulong> connectedClientIds = _networkManager.ConnectedClientsIds;

            for (int i = 0; i < connectedClientIds.Count; i++)
            {
                if (connectedClientIds[i] == clientId)
                    return true;
            }

            return false;
        }

        private void OnClientDisconnected(ulong clientId)
        {       
            //已断开的客户端不应继续阻塞当前地图 Ready 屏障。
            //是否结束游戏或等待重连，交给更上层的会话规则决定。
            _readyClientIds.Remove(clientId);
        }

        public async UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            if (!IsInitialized)
                return;

            if (_networkManager != null)
                _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;

            CancelClientBuild();

            await _visualBuilder.ClearMapVisualsAsync(cancellationToken);

            _readyClientIds.Clear();
            _clientTargetBuffer.Clear();

            CurrentLayout = null;
            CurrentBuildPlan = default;

            _layoutStrategy = null;
            _templateSelector = null;
            _networkManager = null;

            IsInitialized = false;

            Debug.Log($"[{nameof(MapGenerationController)}] 已关闭并清理。");
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException($"{nameof(MapGenerationController)} 尚未初始化。");
        }

        private void EnsureServer()
        {
            if (_networkManager == null || !_networkManager.IsServer)
                throw new InvalidOperationException("只有 Server 可以生成或清理地图。");
        }

        private void CancelClientBuild()
        {
            if (_clientBuildCts == null)
                return;

            _clientBuildCts.Cancel();
            _clientBuildCts.Dispose();
            _clientBuildCts = null;
        }

        public override void OnNetworkDespawn()
        {
            CancelClientBuild();
            base.OnNetworkDespawn();
        }

        private void OnDestroy()
        {
            if (_networkManager != null)
                _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;

            CancelClientBuild();

            if (_lifetimeCts != null)
            {
                _lifetimeCts.Cancel();
                _lifetimeCts.Dispose();
                _lifetimeCts = null;
            }
        }
    }
}
