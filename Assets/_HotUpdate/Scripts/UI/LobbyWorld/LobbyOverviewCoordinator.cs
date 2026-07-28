using System;
using System.Linq;
using System.Text;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Core.Network;
using ProjectGame.HotFix.Netcode;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Lobby
{
    /// <summary>
    /// 大厅 3D 概览的数据编排层。统一选择本地或 NGO 数据源，并同时驱动展位 UI 与模型。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LobbyOverviewCoordinator : MonoBehaviour
    {
        private const string PersistentIdKey = "Lobby.PersistentPlayerId";

        [SerializeField] private LobbyStandLayout _layout;
        [SerializeField] private StandManager _standManager;
        [SerializeField] private AvatarResManager _avatarResManager;
        [SerializeField] private LobbyNetworkManager _networkManager;

        private LobbyPlayerState _localDraft;
        private LobbyPlayerState?[] _visibleStates;
        private bool _profileSubmittedForSession;
        private NetworkManager _netcodeManager;

        public int LocalPlayerStandIndex { get; private set; }
        public LobbyPlayerState LocalPlayerData => GetLocalPlayerData();
        public string PersistentPlayerId => _localDraft.PersistentPlayerId.ToString();

        /// <summary>
        /// 创建默认本地数据、绑定数据源事件并完成首次渲染。
        /// </summary>
        private void Start()
        {
            _visibleStates = new LobbyPlayerState?[_layout.Count];
            _localDraft = CreateDefaultLocalPlayer();
            _netcodeManager = NetworkManager.Singleton;

            _networkManager.OnLobbyDataChanged += HandleLobbyDataChanged;
            _netcodeManager.OnClientStopped += HandleClientStopped;

            ApplyCurrentSnapshot();
        }

        /// <summary>
        /// 销毁时解除大厅与 NGO 生命周期事件。
        /// </summary>
        private void OnDestroy()
        {
            _networkManager.OnLobbyDataChanged -= HandleLobbyDataChanged;
            if (_netcodeManager != null)
                _netcodeManager.OnClientStopped -= HandleClientStopped;
        }

        /// <summary>
        /// 取得当前显示在指定展位上的玩家状态。
        /// </summary>
        public LobbyPlayerState? GetStateForStand(int standIndex) => _visibleStates[standIndex];

        /// <summary>
        /// 在启动 Host 或 Client 前写入玩家ID信息，后续连接时会被服务器读取并注册到权威名单中。
        /// </summary>
        public void PrepareConnectionPayload()
        {
            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(PersistentPlayerId);
            _profileSubmittedForSession = false;
        }

        /// <summary>
        /// 根据当前连接状态修改本地或网络角色。
        /// </summary>
        public void RequestCharacterChange(int characterId)
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                _networkManager.ChangeCharacterServerRpc(characterId);
                return;
            }

            EnsureSkinExists(characterId);
            _localDraft.CharacterId = characterId;
            ApplyLocalSnapshot();
        }

        /// <summary>
        /// 根据当前连接状态修改本地或网络武器。
        /// </summary>
        public void RequestWeaponChange(int weaponId)
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                _networkManager.ChangeWeaponServerRpc(weaponId);
                return;
            }

            EnsureWeaponExists(weaponId);
            _localDraft.WeaponId = weaponId;
            ApplyLocalSnapshot();
        }

        /// <summary>
        /// 根据当前连接状态修改本地或网络道具。
        /// </summary>
        public void RequestItemChange(int itemId)
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                _networkManager.ChangeItemServerRpc(itemId);
                return;
            }

            EnsureItemExists(itemId);
            _localDraft.ItemId = itemId;
            ApplyLocalSnapshot();
        }

        /// <summary>
        /// 校验并修改本地玩家名字。
        /// </summary>
        public void RequestPlayerNameChange(string playerName)
        {
            string sanitizedName = ValidatePlayerName(playerName);
            _localDraft.PlayerName = new FixedString32Bytes(sanitizedName);

            if (NetworkManager.Singleton.IsConnectedClient)
            {
                _networkManager.ChangePlayerNameServerRpc(_localDraft.PlayerName);
                return;
            }

            ApplyLocalSnapshot();
        }

        /// <summary>
        /// 处理权威名单变化，并在首次注册后提交本地资料。
        /// </summary>
        private void HandleLobbyDataChanged()
        {
            ApplyCurrentSnapshot();

            if (_profileSubmittedForSession || !NetworkManager.Singleton.IsConnectedClient)
                return;

            ulong localClientId = NetworkManager.Singleton.LocalClientId;
            bool localPlayerRegistered = false;
            foreach (LobbyPlayerState player in _networkManager.LobbyPlayers)
            {
                if (player.ClientId != localClientId)
                    continue;

                localPlayerRegistered = true;
                break;
            }

            if (!localPlayerRegistered || !_networkManager.IsSpawned)
                return;

            _profileSubmittedForSession = true;
            _networkManager.SubmitPlayerProfileServerRpc(
                _localDraft.PlayerName,
                _localDraft.CharacterId,
                _localDraft.WeaponId,
                _localDraft.ItemId);
        }

        /// <summary>
        /// 连接关闭后恢复离线显示。
        /// </summary>
        private void HandleClientStopped(bool wasHost)
        {
            _profileSubmittedForSession = false;
            ApplyLocalSnapshot();
        }

        /// <summary>
        /// 根据 NGO 连接状态选择当前应该展示的数据源。
        /// </summary>
        private void ApplyCurrentSnapshot()
        {
            if (NetworkManager.Singleton.IsConnectedClient && _networkManager.LobbyPlayers.Count > 0)
                ApplyNetworkSnapshot();
            else
                ApplyLocalSnapshot();
        }

        /// <summary>
        /// 把本地玩家渲染到默认展位。
        /// </summary>
        private void ApplyLocalSnapshot()
        {
            Array.Clear(_visibleStates, 0, _visibleStates.Length);
            LocalPlayerStandIndex = 0;
            _localDraft.StandIndex = LocalPlayerStandIndex;
            _visibleStates[LocalPlayerStandIndex] = _localDraft;
            RenderVisibleStates();
        }

        /// <summary>
        /// 按服务器分配的稳定展位构造网络快照。
        /// </summary>
        private void ApplyNetworkSnapshot()
        {
            Array.Clear(_visibleStates, 0, _visibleStates.Length);
            LocalPlayerStandIndex = -1;
            ulong localClientId = NetworkManager.Singleton.LocalClientId;

            foreach (LobbyPlayerState player in _networkManager.LobbyPlayers)
            {
                if ((uint)player.StandIndex >= (uint)_visibleStates.Length)
                    throw new InvalidOperationException($"服务器下发了无效展位：{player.StandIndex}");

                if (_visibleStates[player.StandIndex].HasValue)
                    throw new InvalidOperationException($"展位 {player.StandIndex} 被多个玩家占用");

                _visibleStates[player.StandIndex] = player;
                if (player.ClientId == localClientId)
                    LocalPlayerStandIndex = player.StandIndex;
            }

            RenderVisibleStates();
        }

        /// <summary>
        /// 使用同一份快照同步刷新展位 UI 和模型。
        /// </summary>
        private void RenderVisibleStates()
        {
            bool showReadyState = NetworkManager.Singleton.IsConnectedClient;
            for (int i = 0; i < _visibleStates.Length; i++)
            {
                bool isLocalPlayer = i == LocalPlayerStandIndex && _visibleStates[i].HasValue;
                _standManager.RenderStand(i,_visibleStates[i],isLocalPlayer,showReadyState);
                _avatarResManager.ApplyStandState(i, _visibleStates[i]);
            }
        }

        /// <summary>
        /// 优先返回权威名单中的本地玩家，否则返回离线草稿。
        /// </summary>
        private LobbyPlayerState GetLocalPlayerData()
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                ulong localClientId = NetworkManager.Singleton.LocalClientId;
                foreach (LobbyPlayerState player in _networkManager.LobbyPlayers)
                {
                    if (player.ClientId == localClientId)
                        return player;
                }
            }

            return _localDraft;
        }

        /// <summary>
        /// 从有效配置表创建首次进入大厅的本地默认数据。
        /// </summary>
        private static LobbyPlayerState CreateDefaultLocalPlayer()
        {
            string persistentId = PlayerPrefs.GetString(PersistentIdKey);
            if (string.IsNullOrEmpty(persistentId))
            {
                persistentId = Guid.NewGuid().ToString("N");
                PlayerPrefs.SetString(PersistentIdKey, persistentId);
                PlayerPrefs.Save();
            }

            int skinId = ConfigManager.Instance.GetTable<Config_Lobby_Skins>().Keys.Min();
            int weaponId = ConfigManager.Instance.GetTable<Config_Lobby_Weapons>().Keys.Min();
            int itemId = ConfigManager.Instance.GetTable<Config_Lobby_Items>().Keys.Min();

            return new LobbyPlayerState
            {
                ClientId = ulong.MaxValue - 1,
                PersistentPlayerId = new FixedString64Bytes(persistentId),
                PlayerName = new FixedString32Bytes("Player"),
                StandIndex = 0,
                CharacterId = skinId,
                WeaponId = weaponId,
                ItemId = itemId,
                IsReady = false
            };
        }

        /// <summary>
        /// 清理并校验玩家名字长度。
        /// </summary>
        private static string ValidatePlayerName(string playerName)
        {
            string result = playerName.Trim();
            if (result.Length == 0)
                throw new ArgumentException("玩家名字不能为空", nameof(playerName));
            if (Encoding.UTF8.GetByteCount(result) > 29)
                throw new ArgumentException("玩家名字不能超过 29 个 UTF-8 字节", nameof(playerName));
            return result;
        }

        /// <summary>确保本地选择的皮肤 ID 存在。</summary>
        private static void EnsureSkinExists(int id)
        {
            if (!ConfigManager.Instance.GetTable<Config_Lobby_Skins>().ContainsKey(id))
                throw new ArgumentOutOfRangeException(nameof(id), id, "皮肤配置不存在");
        }

        /// <summary>确保本地选择的武器 ID 存在。</summary>
        private static void EnsureWeaponExists(int id)
        {
            if (!ConfigManager.Instance.GetTable<Config_Lobby_Weapons>().ContainsKey(id))
                throw new ArgumentOutOfRangeException(nameof(id), id, "武器配置不存在");
        }

        /// <summary>确保本地选择的道具 ID 存在。</summary>
        private static void EnsureItemExists(int id)
        {
            if (!ConfigManager.Instance.GetTable<Config_Lobby_Items>().ContainsKey(id))
                throw new ArgumentOutOfRangeException(nameof(id), id, "道具配置不存在");
        }
    }
}
