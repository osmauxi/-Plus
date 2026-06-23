using ProjectGame.HotFix.Core.Network; 
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Netcode
{
    /// <summary>
    /// 管理大厅的连接、审批、玩家状态管理、掉线重连等网络逻辑
    /// </summary>
    public class LobbyNetworkManager : NetworkBehaviour
    {
        public static LobbyNetworkManager Instance { get; private set; }

        [Header("大厅网络配置")]
        [SerializeField] private int _maxPlayers = 4;

        //当前使用的联机策略 (通过按钮UI注入)
        private IMatchmakingStrategy _matchmakingStrategy;
        //房间是否可加入
        private bool _isRoomLocked = false;

        //断线重连玩家信息对照表：Key = PersistentPlayerId, Value = 状态
        private Dictionary<string, LobbyPlayerState> _disconnectedPlayers = new Dictionary<string, LobbyPlayerState>(5);
        //批准连接的客户端ID与持久化ID对照表：Key = ClientId, Value = PersistentPlayerId
        private Dictionary<ulong, string> _approvedClientIds = new Dictionary<ulong, string>();

        //大厅玩家状态列表，自动同步给所有客户端
        public NetworkList<LobbyPlayerState> LobbyPlayers = new NetworkList<LobbyPlayerState>();
        //状态变更事件，供UI层监听以刷新界面
        public event Action OnLobbyDataChanged;
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            //ConnectionApprovalCallback在客户端尝试连接时触发
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public override void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
            base.OnDestroy();
        }

        public override void OnNetworkSpawn()
        {
            //绑定名单变化事件
            LobbyPlayers.OnListChanged += HandleLobbyPlayersChanged;
        }

        public override void OnNetworkDespawn()
        {
            LobbyPlayers.OnListChanged -= HandleLobbyPlayersChanged;
        }

        #region 连接审批回调方法
        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = false;
            response.CreatePlayerObject = false;

            //解析客户端提交的Payload(在request中)，这里约定传入的是PersistentPlayerId的唯一标记字符串流
            string persistentId = Encoding.ASCII.GetString(request.Payload);
            Debug.Log($"[LobbyNetworkManager] 收到接入请求，玩家持久化ID: {persistentId}");

            //判定是不是断线重连的玩家
            if (_isRoomLocked)
            {
                if (_disconnectedPlayers.ContainsKey(persistentId))
                {
                    Debug.Log($"<color=green>[LobbyNetworkManager] 玩家重连: {persistentId}</color>");
                    response.Approved = true;
                    // TODO: 战斗场景内的重连生成逻辑
                    return;
                }
                else
                {
                    Debug.LogWarning($"[LobbyNetworkManager] 房间已锁定，拒绝新玩家加入: {persistentId}");
                    response.Reason = "游戏已经开始，无法加入！";
                    return;
                }
            }

            //检查房间人数上限
            if (NetworkManager.Singleton.ConnectedClientsIds.Count >= _maxPlayers)
            {
                Debug.LogWarning("[LobbyNetworkManager] 房间人数已满，拒绝加入。");
                response.Reason = "房间人数已满！";
                return;
            }

            //能走到这就通过。
            Debug.Log($"<color=green>[LobbyNetworkManager] 新玩家正在建立网络握手...</color>");
            response.Approved = true;
        }
        #endregion
        #region 玩家连接与断开连接
        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer)
                return;

            Debug.Log($"[LobbyNetworkManager] Client {clientId} 已物理连入！");

            //请求连接时提前提供了唯一标识，批准后在这里根据这个标识来恢复玩家数据或者创建新玩家数据
            if (_approvedClientIds.TryGetValue(clientId, out string persistentId))
            {
                //断线重连恢复数据
                if (_disconnectedPlayers.TryGetValue(persistentId, out LobbyPlayerState oldState))
                {
                    oldState.ClientId = clientId; //更新NGO的ClientId为当前连接的ClientId
                    oldState.IsReady = false;     //重连后默认取消准备状态
                    LobbyPlayers.Add(oldState);
                    _disconnectedPlayers.Remove(persistentId);
                    Debug.Log($"[LobbyNetworkManager] 重连成功，恢复数据: {persistentId}");
                }
                else
                {
                    //塞初始白板数据
                    LobbyPlayers.Add(new LobbyPlayerState
                    {
                        ClientId = clientId,
                        PersistentPlayerId = new FixedString64Bytes(persistentId),
                        PlayerName = new FixedString32Bytes($"Player_{clientId}"),
                        CharacterId = 1001, //默认角色ID
                        WeaponId = 2001,    //默认武器ID
                        IsReady = false
                    });
                    Debug.Log($"[大厅] 新玩家加入名单: ClientId {clientId}");
                }

                // 用完即删，防止内存泄漏
                _approvedClientIds.Remove(clientId);
            }
        }
        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;

            Debug.Log($"[LobbyNetworkManager] Client {clientId} 已断开连接！");

            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                //Host自己炸了，准备解散
                _disconnectedPlayers.Clear();
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
        private void HandleLobbyPlayersChanged(NetworkListEvent<LobbyPlayerState> changeEvent)
        {
            OnLobbyDataChanged?.Invoke();
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
        /// 客户端请求切换武器
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ChangeWeaponServerRpc(int newWeaponId, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;
            for (int i = 0; i < LobbyPlayers.Count; i++)
            {
                if (LobbyPlayers[i].ClientId == senderId)
                {
                    var state = LobbyPlayers[i];

                    //准备阶段不准切武器
                    if (state.IsReady) 
                        return;

                    state.WeaponId = newWeaponId;
                    LobbyPlayers[i] = state; //触发同步
                    break;
                }
            }
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
                Debug.Log("<color=yellow>[LobbyNetworkManager] 所有玩家准备就绪，可以载入战斗场景了！</color>");
                // TODO: 触发场景跃迁
            }
        }
        #endregion
        //策略层注入(供 UI 层发起连接时调用)
        public void SetMatchmakingStrategy(IMatchmakingStrategy strategy)
        {
            _matchmakingStrategy = strategy;
        }
    }
}