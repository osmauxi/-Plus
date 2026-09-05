using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Core.Network;
using ProjectGame.HotFix.Core.Session;
using ProjectGame.HotFix.SceneFlow;
using ProjectGame.HotFix.Network.Runtime;
using System.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectGame.HotFix.Netcode
{
    /// <summary>
    /// 管理大厅的连接、审批、玩家状态管理、掉线重连等网络逻辑
    /// </summary>
    public class LobbyNetworkManager : NetworkBehaviour, IScopeBindable
    {
        public static LobbyNetworkManager Instance { get; private set; }
        public static event Action<LobbyNetworkManager> InstanceChanged;
        public bool IsRoomLocked => _isRoomLocked;

        [Header("大厅网络配置")]
        [SerializeField] private int _maxPlayers = 4;
        [SerializeField] private float _readyCountdownSeconds = 3f;
        [SerializeField, Min(1f)] private float _sessionPreparationTimeoutSeconds = 10f;

        // 可选的联机策略；局域网直连没有外部策略时使用 UnityTransport。
        private IMatchmakingStrategy _matchmakingStrategy;
        //房间是否可加入
        private bool _isRoomLocked = false;
        private bool _isStartingGame;
        private Coroutine _readyCountdownCoroutine;
        private readonly HashSet<ulong> _profileSubmittedClientIds = new HashSet<ulong>();
        private readonly HashSet<ulong> _sessionPreparedClientIds = new HashSet<ulong>();
        private int _sessionRevision;
        private string _sessionPreparationError;
        private NetworkManager _callbackManager;
        private NetworkScopeManager _scopeManager;
        // 倒计时事件：UI监听展示剩余秒数
        public event Action<float> OnReadyCountdownUpdated;
        public event Action OnCountdownStarted;
        public event Action OnCountdownCancelled;

        //断线重连玩家信息对照表：Key = PersistentPlayerId, Value = 状态
        private Dictionary<string, LobbyPlayerState> _disconnectedPlayers = new Dictionary<string, LobbyPlayerState>(5);
        //批准连接的客户端ID与持久化ID对照表：Key = ClientId, Value = PersistentPlayerId
        private LobbyConnectionGate _connectionGate;

        //大厅玩家状态列表，自动同步给所有客户端
        public NetworkList<LobbyPlayerState> LobbyPlayers = new NetworkList<LobbyPlayerState>();
        //状态变更事件，供UI层监听以刷新界面
        public event Action OnLobbyDataChanged;

        /// <summary>建立持久化大厅网络单例 </summary>
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            BindConnectionCallbacks();
            InstanceChanged?.Invoke(this);
        }

        /// <summary>绑定 NGO 连接审批、连接和断开事件 </summary>
        private void Start()
            => BindConnectionCallbacks();

        private void BindConnectionCallbacks()
        {
            if (_callbackManager != null) return;
            _callbackManager = NetworkManager.Singleton;
            if (_callbackManager == null) throw new InvalidOperationException("会话种子创建前必须先创建 NetworkManager");
            _callbackManager.OnClientConnectedCallback += OnClientConnected;
            _callbackManager.OnClientDisconnectCallback += OnClientDisconnected;
            _connectionGate = _callbackManager.GetComponent<LobbyConnectionGate>();
            if (_connectionGate == null)
                throw new InvalidOperationException("NetworkBootstrap 缺少 LobbyConnectionGate");
        }

        public UniTask BindAsync(NetworkScopeStageContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!context.TryGetRoot(NetworkPrefabId.NetworkSessionRoot, out NetworkObject root) ||
                root.GetComponent<GameSceneFlowController>() == null)
                throw new InvalidOperationException("LobbyNetworkRoot 找不到场景流程会话 Root");
            if (_scopeManager != null) _scopeManager.ScopeActivated -= HandleScopeActivated;
            _scopeManager = NetworkRuntimeBootstrap.Instance.ScopeManager;
            _scopeManager.ScopeActivated += HandleScopeActivated;
            return UniTask.CompletedTask;
        }

        private void HandleScopeActivated(NetworkSceneMask mask)
        {
            if (mask != NetworkSceneMask.Lobby) return;
            GameSessionContext.Clear();
            if (!IsServer) return;
            _isStartingGame = _isRoomLocked = false;
            _sessionPreparedClientIds.Clear();
            _sessionPreparationError = null;
            if (_readyCountdownCoroutine != null) StopCoroutine(_readyCountdownCoroutine);
            _readyCountdownCoroutine = null;
            for (int i = 0; i < LobbyPlayers.Count; i++)
            {
                LobbyPlayerState player = LobbyPlayers[i];
                player.IsReady = false;
                LobbyPlayers[i] = player;
            }
        }

        /// <summary>销毁时解除 NGO 回调并清空有效单例 </summary>
        public override void OnDestroy()
        {
            if (_callbackManager != null)
            {
                _callbackManager.OnClientConnectedCallback -= OnClientConnected;
                _callbackManager.OnClientDisconnectCallback -= OnClientDisconnected;
            }
            if (_scopeManager != null) _scopeManager.ScopeActivated -= HandleScopeActivated;
            if (Instance == this)
            {
                Instance = null;
                InstanceChanged?.Invoke(null);
            }
            base.OnDestroy();
        }

        /// <summary>网络对象生成后监听权威玩家名单变化 </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _isStartingGame = _isRoomLocked = false;
            _profileSubmittedClientIds.Clear();
            _sessionPreparedClientIds.Clear();
            _sessionRevision = 0;
            _sessionPreparationError = null;
            if (IsServer) LobbyPlayers.Clear();
            //绑定名单变化事件
            LobbyPlayers.OnListChanged += HandleLobbyPlayersChanged;
            if (IsServer)
            {
                foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
                    OnClientConnected(clientId);
            }
            OnLobbyDataChanged?.Invoke();
        }

        /// <summary>网络对象回收前解除玩家名单监听 </summary>
        public override void OnNetworkDespawn()
        {
            LobbyPlayers.OnListChanged -= HandleLobbyPlayersChanged;
            if (_scopeManager != null) _scopeManager.ScopeActivated -= HandleScopeActivated;
            _scopeManager = null;
            base.OnNetworkDespawn();
        }

        #region 玩家连接与断开连接
        /// <summary>在服务端为新连接或重连玩家创建权威大厅状态 </summary>
        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer)
                return;

            Debug.Log($"[LobbyNetworkManager] Client {clientId} 已物理连入！");

            //请求连接时提前提供了唯一标识，批准后在这里根据这个标识来恢复玩家数据或者创建新玩家数据
            if (_connectionGate != null && _connectionGate.TryConsumeApprovedPlayerId(clientId, out string persistentId))
            {
                //断线重连恢复数据
                if (_disconnectedPlayers.TryGetValue(persistentId, out LobbyPlayerState oldState))
                {
                    oldState.ClientId = clientId; //更新NGO的ClientId为当前连接的ClientId
                    oldState.IsReady = false;     //重连后默认取消准备状态
                    if (IsStandOccupied(oldState.StandIndex))
                        oldState.StandIndex = GetFirstAvailableStandIndex();
                    LobbyPlayers.Add(oldState);
                    _disconnectedPlayers.Remove(persistentId);
                    Debug.Log($"[LobbyNetworkManager] 重连成功，恢复数据: {persistentId}");
                }
                else
                {
                    LobbyPlayers.Add(CreateDefaultPlayerState(
                        clientId,
                        persistentId,
                        $"Player_{clientId}"));
                    Debug.Log($"[大厅] 新玩家加入名单: ClientId {clientId}");
                }

                // 用完即删，防止内存泄漏
            }
            else
            {
                // Host 自身若没有经过连接审批 Gate，则从本机 ConnectionData 取得身份。
                //为 Host 本地客户端生成默认初始数据
                string hostPersistentId = Encoding.UTF8.GetString(
                    NetworkManager.Singleton.NetworkConfig.ConnectionData);
                if (string.IsNullOrWhiteSpace(hostPersistentId))
                    hostPersistentId = $"Host_{clientId}";
                LobbyPlayers.Add(CreateDefaultPlayerState(clientId, hostPersistentId, "Host_Player"));
                Debug.Log($"[大厅] Host 本地玩家加入名单: ClientId {clientId}, PersistentId {hostPersistentId}");
            }
        }
        /// <summary>在服务端存档并移除断开连接的玩家 </summary>
        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;

            Debug.Log($"[LobbyNetworkManager] Client {clientId} 已断开连接！");

            _profileSubmittedClientIds.Remove(clientId);
            _sessionPreparedClientIds.Remove(clientId);

            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                //Host自己炸了，准备解散
                _disconnectedPlayers.Clear();
                _profileSubmittedClientIds.Clear();
                _sessionPreparedClientIds.Clear();
                LobbyPlayers.Clear();
                return;
            }

            //遍历找掉线人员
            for (int i = 0; i < LobbyPlayers.Count; i++)
            {
                if (LobbyPlayers[i].ClientId == clientId)
                {
                    var droppedPlayer = LobbyPlayers[i];

                    //把他的信息移到掉线字典去
                    _disconnectedPlayers[droppedPlayer.PersistentPlayerId.ToString()] = droppedPlayer;

                    //移除玩家名单，顺带触发UI更新事件
                    LobbyPlayers.RemoveAt(i);
                    Debug.Log($"[LobbyNetworkManager] 玩家掉线，已移出名单并存档: {droppedPlayer.PersistentPlayerId}");
                    break;
                }
            }
        }
        #endregion

        #region 全局状态管理
        /// <summary>把 NetworkList 变化转发给大厅显示层 </summary>
        private void HandleLobbyPlayersChanged(NetworkListEvent<LobbyPlayerState> changeEvent)
        {
            OnLobbyDataChanged?.Invoke();
            Debug.Log(LobbyPlayers.Count);
        }

        /// <summary>
        /// 连接建立后提交本地大厅资料，并由服务器写入名单 
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SubmitPlayerProfileServerRpc(
            FixedString32Bytes playerName,
            int characterId,
            int weaponId,
            int itemId,
            ServerRpcParams rpcParams = default)
        {
            ValidatePlayerName(playerName);
            ValidateCharacterId(characterId);
            ValidateWeaponId(weaponId);
            ValidateItemId(itemId);

            ulong senderClientId = rpcParams.Receive.SenderClientId;
            int playerIndex = FindPlayerIndex(senderClientId);
            LobbyPlayerState state = LobbyPlayers[playerIndex];
            if (state.IsReady && _profileSubmittedClientIds.Contains(senderClientId))
                return;

            state.PlayerName = playerName;
            state.CharacterId = characterId;
            state.WeaponId = weaponId;
            state.ItemId = itemId;
            LobbyPlayers[playerIndex] = state;
            _profileSubmittedClientIds.Add(senderClientId);
        }

        /// <summary>修改 RPC 发送者自己的玩家名字 </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ChangePlayerNameServerRpc(
            FixedString32Bytes playerName,
            ServerRpcParams rpcParams = default)
        {
            ValidatePlayerName(playerName);
            int playerIndex = FindPlayerIndex(rpcParams.Receive.SenderClientId);
            LobbyPlayerState state = LobbyPlayers[playerIndex];
            if (state.IsReady)
                return;

            state.PlayerName = playerName;
            LobbyPlayers[playerIndex] = state;
        }

        /// <summary>
        /// 客户端请求修改自己的准备状态
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ToggleReadyServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;
            for (int i = 0; i < LobbyPlayers.Count; i++)
            {
                if (LobbyPlayers[i].ClientId == senderId)
                {
                    LobbyPlayerState state = LobbyPlayers[i];
                    state.IsReady = !state.IsReady;

                    //重新赋值触发同步
                    LobbyPlayers[i] = state;

                    CheckAllReady();
                    break;
                }
            }
        }
        /// <summary>
        /// 客户端请求切换皮肤(角色模型)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ChangeCharacterServerRpc(int newCharacterId, ServerRpcParams rpcParams = default)
        {
            ValidateCharacterId(newCharacterId);
            ulong senderId = rpcParams.Receive.SenderClientId;
            for (int i = 0; i < LobbyPlayers.Count; i++)
            {
                if (LobbyPlayers[i].ClientId == senderId)
                {
                    var state = LobbyPlayers[i];
                    if (state.IsReady) return;
                    state.CharacterId = newCharacterId;
                    LobbyPlayers[i] = state;
                    break;
                }
            }
        }

        /// <summary>
        /// 客户端请求切换武器
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ChangeWeaponServerRpc(int newWeaponId, ServerRpcParams rpcParams = default)
        {
            ValidateWeaponId(newWeaponId);
            ulong senderId = rpcParams.Receive.SenderClientId;
            for (int i = 0; i < LobbyPlayers.Count; i++)
            {
                if (LobbyPlayers[i].ClientId == senderId)
                {
                    var state = LobbyPlayers[i];
                    if (state.IsReady) return;
                    state.WeaponId = newWeaponId;
                    LobbyPlayers[i] = state;
                    break;
                }
            }
        }

        /// <summary>
        /// 客户端请求切换道具
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ChangeItemServerRpc(int newItemId, ServerRpcParams rpcParams = default)
        {
            ValidateItemId(newItemId);
            ulong senderId = rpcParams.Receive.SenderClientId;
            for (int i = 0; i < LobbyPlayers.Count; i++)
            {
                if (LobbyPlayers[i].ClientId == senderId)
                {
                    var state = LobbyPlayers[i];
                    if (state.IsReady) return;
                    state.ItemId = newItemId;
                    LobbyPlayers[i] = state;
                    break;
                }
            }
        }

        /// <summary>
        /// Host请求移除指定玩家
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void RemovePlayerServerRpc(ulong targetClientId, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            ulong senderId = rpcParams.Receive.SenderClientId;
            // 仅Host可踢人
            if (senderId != NetworkManager.Singleton.LocalClientId) return;

            for (int i = 0; i < LobbyPlayers.Count; i++)
            {
                if (LobbyPlayers[i].ClientId == targetClientId)
                {
                    var removed = LobbyPlayers[i];
                    _disconnectedPlayers[removed.PersistentPlayerId.ToString()] = removed;
                    _profileSubmittedClientIds.Remove(targetClientId);
                    _sessionPreparedClientIds.Remove(targetClientId);
                    LobbyPlayers.RemoveAt(i);
                    Debug.Log($"[LobbyNetworkManager] Host移除了玩家 {targetClientId}");
                    break;
                }
            }

            // 断开该客户端的连接
            NetworkManager.Singleton.DisconnectClient(targetClientId);
        }

        /// <summary>
        /// 检测所有人是否都已准备
        /// </summary>
        private void CheckAllReady()
        {
            if (!IsServer) 
                return;
            if (LobbyPlayers.Count == 0) 
                return;

            bool allReady = true;
            foreach (var player in LobbyPlayers)
            {
                if (!player.IsReady)
                {
                    allReady = false;
                    break;
                }
            }

            if (allReady)
            {
                Debug.Log("<color=yellow>[LobbyNetworkManager] 所有玩家准备就绪，进入倒计时...</color>");
                // 取消之前的倒计时（如果有）
                if (_readyCountdownCoroutine != null)
                {
                    StopCoroutine(_readyCountdownCoroutine);
                    _readyCountdownCoroutine = null;
                }
                _readyCountdownCoroutine = StartCoroutine(ReadyCountdownCoroutine(_readyCountdownSeconds));
            }
            else
            {
                // 有人取消准备，取消倒计时
                if (_readyCountdownCoroutine != null)
                {
                    StopCoroutine(_readyCountdownCoroutine);
                    _readyCountdownCoroutine = null;
                    OnCountdownCancelled?.Invoke();
                    Debug.Log("<color=orange>[LobbyNetworkManager] 准备状态变更，倒计时已取消</color>");
                }
            }
        }

        /// <summary>
        /// 准备就绪倒计时协程
        /// </summary>
        private IEnumerator ReadyCountdownCoroutine(float duration)
        {
            _isRoomLocked = true;
            OnCountdownStarted?.Invoke();

            float remaining = duration;
            while (remaining > 0f)
            {
                OnReadyCountdownUpdated?.Invoke(remaining);
                yield return new WaitForSeconds(1f);
                remaining -= 1f;
            }

            OnReadyCountdownUpdated?.Invoke(0f);
            Debug.Log("<color=green>[LobbyNetworkManager] 倒计时结束，载入战斗场景！</color>");
            StartGameFlow(GameSessionMode.Multiplayer);
        }

        private void StartGameFlow(GameSessionMode mode)
        {
            if (!IsServer)
            {
                return;
            }

            if (_isStartingGame)
            {
                Debug.LogWarning("[LobbyNetworkManager] 已经在进入游戏流程中，忽略重复调用 ");
                return;
            }

            if (GameSceneFlowController.Instance == null)
            {
                Debug.LogError("[LobbyNetworkManager] GameSceneFlowController 未绑定，无法进入游戏 ");
                return;
            }

            _isStartingGame = true;
            _isRoomLocked = true;

            StartGameFlowAsync(mode).Forget();
        }

        /// <summary>
        /// 通知所有客户端加载游戏场景
        /// </summary>
        private async UniTaskVoid StartGameFlowAsync(GameSessionMode mode)
        {
            try
            {
                await NetworkSessionBootstrap.Instance.WaitForLobbyReadyAsync(this.GetCancellationTokenOnDestroy());
                await WaitForAllPlayerProfilesAsync();

                LobbyPlayerState[] lobbySnapshot = BuildLobbySnapshot();
                await PrepareGameSessionOnAllClientsAsync(mode, lobbySnapshot);

                await GameSceneFlowController.Instance.TransitionToGameSceneAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"[LobbyNetworkManager] 进入游戏流程异常: {e}");
                ClearPreparedGameSession();
                _isStartingGame = false;
                _isRoomLocked = false;
            }
        }

        /// <summary>等待所有已连接玩家把本地选择提交到服务器权威名单 </summary>
        private async UniTask WaitForAllPlayerProfilesAsync()
        {
            float deadline = Time.realtimeSinceStartup + _sessionPreparationTimeoutSeconds;

            while (!AreAllPlayerProfilesSubmitted())
            {
                if (Time.realtimeSinceStartup >= deadline)
                    throw new TimeoutException("等待大厅玩家资料提交超时，无法生成游戏会话快照 ");

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        private bool AreAllPlayerProfilesSubmitted()
        {
            IReadOnlyList<ulong> connectedClientIds = NetworkManager.Singleton.ConnectedClientsIds;
            if (connectedClientIds.Count == 0 || LobbyPlayers.Count != connectedClientIds.Count)
                return false;

            for (int i = 0; i < connectedClientIds.Count; i++)
            {
                ulong clientId = connectedClientIds[i];
                if (!_profileSubmittedClientIds.Contains(clientId) || !ContainsLobbyPlayer(clientId))
                    return false;
            }

            return true;
        }

        private bool ContainsLobbyPlayer(ulong clientId)
        {
            for (int i = 0; i < LobbyPlayers.Count; i++)
            {
                if (LobbyPlayers[i].ClientId == clientId)
                    return true;
            }

            return false;
        }

        private LobbyPlayerState[] BuildLobbySnapshot()
        {
            LobbyPlayerState[] snapshot = new LobbyPlayerState[LobbyPlayers.Count];
            for (int i = 0; i < LobbyPlayers.Count; i++)
                snapshot[i] = LobbyPlayers[i];

            Array.Sort(snapshot, (left, right) => left.ClientId.CompareTo(right.ClientId));
            return snapshot;
        }

        /// <summary>把同一份权威 Lobby 快照写入服务器和所有客户端的会话上下文 </summary>
        private async UniTask PrepareGameSessionOnAllClientsAsync(
            GameSessionMode mode,
            LobbyPlayerState[] lobbySnapshot)
        {
            int revision = ++_sessionRevision;
            _sessionPreparedClientIds.Clear();
            _sessionPreparationError = null;

            ConfigureGameSessionContext(mode, lobbySnapshot);
            if (NetworkManager.Singleton.IsClient)
                _sessionPreparedClientIds.Add(NetworkManager.Singleton.LocalClientId);

            PrepareGameSessionClientRpc(mode, lobbySnapshot, revision);

            float deadline = Time.realtimeSinceStartup + _sessionPreparationTimeoutSeconds;
            while (!AreAllConnectedClientsSessionPrepared())
            {
                if (!DoesConnectedRosterMatchSnapshot(lobbySnapshot))
                    throw new InvalidOperationException("会话准备期间玩家连接名单发生变化，已取消本次转场 ");

                if (!string.IsNullOrEmpty(_sessionPreparationError))
                    throw new InvalidOperationException(_sessionPreparationError);

                if (Time.realtimeSinceStartup >= deadline)
                    throw new TimeoutException("等待客户端写入 GameSessionContext 超时 ");

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        private static bool DoesConnectedRosterMatchSnapshot(
            IReadOnlyList<LobbyPlayerState> lobbySnapshot)
        {
            IReadOnlyList<ulong> connectedClientIds = NetworkManager.Singleton.ConnectedClientsIds;
            if (connectedClientIds.Count != lobbySnapshot.Count)
                return false;

            for (int i = 0; i < lobbySnapshot.Count; i++)
            {
                bool found = false;
                for (int j = 0; j < connectedClientIds.Count; j++)
                {
                    if (lobbySnapshot[i].ClientId != connectedClientIds[j])
                        continue;

                    found = true;
                    break;
                }

                if (!found)
                    return false;
            }

            return true;
        }

        private bool AreAllConnectedClientsSessionPrepared()
        {
            IReadOnlyList<ulong> connectedClientIds = NetworkManager.Singleton.ConnectedClientsIds;
            for (int i = 0; i < connectedClientIds.Count; i++)
            {
                if (!_sessionPreparedClientIds.Contains(connectedClientIds[i]))
                    return false;
            }

            return connectedClientIds.Count > 0;
        }

        [ClientRpc]
        private void PrepareGameSessionClientRpc(
            GameSessionMode mode,
            LobbyPlayerState[] lobbySnapshot,
            int revision)
        {
            try
            {
                ConfigureGameSessionContext(mode, lobbySnapshot);
                ConfirmGameSessionPreparedServerRpc(revision, true, string.Empty);
            }
            catch (Exception e)
            {
                ConfirmGameSessionPreparedServerRpc(revision, false, e.Message);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ConfirmGameSessionPreparedServerRpc(
            int revision,
            bool success,
            string errorMessage,
            ServerRpcParams rpcParams = default)
        {
            if (revision != _sessionRevision)
                return;

            ulong senderClientId = rpcParams.Receive.SenderClientId;
            if (!success)
            {
                _sessionPreparationError =
                    $"ClientId={senderClientId} 写入 GameSessionContext 失败：{errorMessage}";
                return;
            }

            _sessionPreparedClientIds.Add(senderClientId);
        }

        private static void ConfigureGameSessionContext(
            GameSessionMode mode,
            IReadOnlyList<LobbyPlayerState> lobbySnapshot)
        {
            PlayerSessionData[] players = new PlayerSessionData[lobbySnapshot.Count];
            for (int i = 0; i < lobbySnapshot.Count; i++)
            {
                LobbyPlayerState player = lobbySnapshot[i];
                players[i] = new PlayerSessionData(
                    player.ClientId,
                    player.PersistentPlayerId.ToString(),
                    player.PlayerName.ToString(),
                    player.CharacterId,
                    player.WeaponId,
                    player.ItemId);
            }

            GameSessionContext.Configure(mode, players);
        }

        private void ClearPreparedGameSession()
        {
            GameSessionContext.Clear();
            _sessionPreparedClientIds.Clear();
            _sessionPreparationError = null;

            if (IsServer && IsSpawned)
                ClearGameSessionClientRpc();
        }

        [ClientRpc]
        private void ClearGameSessionClientRpc()
        {
            GameSessionContext.Clear();
        }
        #endregion

        #region 单人模式
        /// <summary>
        /// 单人模式：直接启动Host并立即转场景
        /// </summary>
        public void StartSinglePlayerAndEnterGame()
            => StartSinglePlayerAsync().Forget(exception => Debug.LogError($"[LobbyNetworkManager] 单人启动失败：{exception}"));

        private async UniTask StartSinglePlayerAsync()
        {
            await NetworkSessionBootstrap.Instance.PrepareConnectionAsync(this.GetCancellationTokenOnDestroy());
            if (NetworkManager.Singleton.IsHost)
            {
                await NetworkSessionBootstrap.Instance.WaitForLobbyReadyAsync(this.GetCancellationTokenOnDestroy());
                OnSinglePlayerHostStarted(true);
                return;
            }

            var parameters = new MatchmakingParams
            {
                IpAddress = "127.0.0.1",
                Port = 7777,
                MaxPlayers = 1
            };

            bool success;
            if (_matchmakingStrategy != null)
            {
                success = await _matchmakingStrategy.StartHostAsync(parameters);
            }
            else
            {
                UnityTransport transport =
                    NetworkManager.Singleton.GetComponent<UnityTransport>();
                if (transport == null)
                {
                    Debug.LogError(
                        "[LobbyNetworkManager] 找不到 UnityTransport，无法启动单人 Host");
                    OnSinglePlayerHostStarted(false);
                    return;
                }

                transport.SetConnectionData(
                    "127.0.0.1",
                    parameters.Port,
                    "0.0.0.0");
                success = NetworkManager.Singleton.StartHost();
            }

            if (success) await NetworkSessionBootstrap.Instance.WaitForLobbyReadyAsync(this.GetCancellationTokenOnDestroy());
            OnSinglePlayerHostStarted(success);
        }

        /// <summary>Host 启动成功后锁定大厅并进入游戏场景 </summary>
        private void OnSinglePlayerHostStarted(bool success)
        {
            if (!success)
            {
                Debug.LogError("[LobbyNetworkManager] 单人模式启动Host失败！");
                return;
            }

            Debug.Log("[LobbyNetworkManager] 单人模式Host已启动，直接载入游戏场景...");
            // 单人模式不需要倒计时，直接转场
            StartGameFlow(GameSessionMode.SinglePlayer);
        }
        #endregion

        /// <summary>注入大厅使用的匹配策略 </summary>
        public void SetMatchmakingStrategy(IMatchmakingStrategy strategy)
        {
            _matchmakingStrategy = strategy;
        }

        /// <summary>使用有效配置和最低空闲展位创建服务器默认玩家数据 </summary>
        private LobbyPlayerState CreateDefaultPlayerState(
            ulong clientId,
            string persistentId,
            string playerName)
        {
            return new LobbyPlayerState
            {
                ClientId = clientId,
                PersistentPlayerId = new FixedString64Bytes(persistentId),
                PlayerName = new FixedString32Bytes(playerName),
                StandIndex = GetFirstAvailableStandIndex(),
                CharacterId = ConfigManager.Instance.GetTable<Config_Lobby_Skins>().Keys.Min(),
                WeaponId = ConfigManager.Instance.GetTable<Config_Lobby_Weapons>().Keys.Min(),
                ItemId = ConfigManager.Instance.GetTable<Config_Lobby_Items>().Keys.Min(),
                IsReady = false
            };
        }

        /// <summary>返回最低的未占用展位索引 </summary>
        private int GetFirstAvailableStandIndex()
        {
            for (int standIndex = 0; standIndex < _maxPlayers; standIndex++)
            {
                if (!IsStandOccupied(standIndex))
                    return standIndex;
            }

            throw new InvalidOperationException("大厅没有可用展位");
        }

        /// <summary>检查指定展位是否已被大厅玩家占用 </summary>
        private bool IsStandOccupied(int standIndex)
        {
            foreach (LobbyPlayerState player in LobbyPlayers)
            {
                if (player.StandIndex == standIndex)
                    return true;
            }

            return false;
        }

        /// <summary>通过 ClientId 查找权威玩家列表索引 </summary>
        private int FindPlayerIndex(ulong clientId)
        {
            for (int i = 0; i < LobbyPlayers.Count; i++)
            {
                if (LobbyPlayers[i].ClientId == clientId)
                    return i;
            }

            throw new InvalidOperationException($"大厅名单中不存在 ClientId={clientId}");
        }

        /// <summary>校验玩家名字可用于网络同步 </summary>
        private static void ValidatePlayerName(FixedString32Bytes playerName)
        {
            if (playerName.Length == 0)
                throw new ArgumentException("玩家名字不能为空", nameof(playerName));
        }

        /// <summary>校验皮肤 ID 存在于配置表 </summary>
        private static void ValidateCharacterId(int id)
        {
            if (!ConfigManager.Instance.GetTable<Config_Lobby_Skins>().ContainsKey(id))
                throw new ArgumentOutOfRangeException(nameof(id), id, "皮肤配置不存在");
        }

        /// <summary>校验武器 ID 存在于配置表 </summary>
        private static void ValidateWeaponId(int id)
        {
            if (!ConfigManager.Instance.GetTable<Config_Lobby_Weapons>().ContainsKey(id))
                throw new ArgumentOutOfRangeException(nameof(id), id, "武器配置不存在");
        }

        /// <summary>校验道具 ID 存在于配置表 </summary>
        private static void ValidateItemId(int id)
        {
            if (!ConfigManager.Instance.GetTable<Config_Lobby_Items>().ContainsKey(id))
                throw new ArgumentOutOfRangeException(nameof(id), id, "道具配置不存在");
        }
    }
}
