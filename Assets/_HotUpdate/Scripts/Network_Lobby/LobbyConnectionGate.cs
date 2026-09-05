using System;
using System.Collections.Generic;
using System.Text;
using ProjectGame.HotFix.Network.Runtime;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Netcode
{
    /// <summary>联网前即可用的会话准入门；Lobby Root 只保留大厅期网络状态。</summary>
    [RequireComponent(typeof(NetworkManager))]
    public sealed class LobbyConnectionGate : MonoBehaviour
    {
        [SerializeField, Min(1)] private int _maxPlayers = 4;
        private readonly Dictionary<ulong, string> _approvedPlayerIds = new();
        private NetworkManager _networkManager;

        private void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();
            _networkManager.ConnectionApprovalCallback += ApprovalCheck;
            _networkManager.OnServerStopped += HandleServerStopped;
        }

        private void OnDestroy()
        {
            if (_networkManager == null) return;
            _networkManager.ConnectionApprovalCallback -= ApprovalCheck;
            _networkManager.OnServerStopped -= HandleServerStopped;
        }

        public bool TryConsumeApprovedPlayerId(ulong clientId, out string persistentPlayerId)
        {
            if (_approvedPlayerIds.TryGetValue(clientId, out persistentPlayerId))
            {
                _approvedPlayerIds.Remove(clientId);
                return true;
            }
            return false;
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = false;
            response.CreatePlayerObject = false;
            string persistentId = Encoding.UTF8.GetString(request.Payload);
            if (string.IsNullOrWhiteSpace(persistentId))
            {
                response.Reason = "缺少玩家持久化 ID";
                return;
            }

            NetworkSceneMask activeMask = NetworkRuntimeBootstrap.Instance != null &&
                                          NetworkRuntimeBootstrap.Instance.IsInitialized
                ? NetworkRuntimeBootstrap.Instance.ScopeManager.ActiveSceneMask
                : NetworkSceneMask.None;
            bool lobbyUnavailable = activeMask != NetworkSceneMask.None && activeMask != NetworkSceneMask.Lobby;
            bool lobbyLocked = LobbyNetworkManager.Instance != null && LobbyNetworkManager.Instance.IsRoomLocked;
            if (lobbyUnavailable || lobbyLocked)
            {
                response.Reason = "游戏已经开始，无法加入！";
                return;
            }
            if (_networkManager.ConnectedClientsIds.Count >= _maxPlayers)
            {
                response.Reason = "房间人数已满！";
                return;
            }

            _approvedPlayerIds[request.ClientNetworkId] = persistentId;
            response.Approved = true;
        }

        private void HandleServerStopped(bool _) => _approvedPlayerIds.Clear();
    }
}
