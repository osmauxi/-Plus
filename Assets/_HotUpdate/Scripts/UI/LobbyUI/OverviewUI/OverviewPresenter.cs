using DG.Tweening;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using ProjectGame.HotFix.Netcode;
using ProjectGame.HotFix.Lobby;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// Presenter: 接管 OverviewView 和 JoinGameView 的事件 → 触发数据层变更
    /// 全面管控 JoinGameUI 的显隐动效、网络Host/Client状态机、按钮文字颜色切换
    /// </summary>
    public class OverviewPresenter : BaseLobbyPresenter
    {
        [Header("V层引用")]
        [SerializeField] private OverviewView _view;
        [SerializeField] private JoinGameView _joinGameView;

        [Header("网络")]
        [SerializeField] private float _connectionTimeout = 10f;

        //JoinGame状态机
        private enum JoinGameBtnState
        {
            Default,   // "加入房间" - 白色
            Hosting,   // "解散房间" - 红色
            Client     // "退出房间" - 红色
        }

        private JoinGameBtnState _joinBtnState = JoinGameBtnState.Default;
        private bool _isJoinGameUIVisible = false;
        private bool _isReady = false;
        private Coroutine _connectionTimeoutCoroutine;
        private Sequence _currentTweenSeq;
        private NetworkManager _netcodeManager;
        private LobbyNetworkManager _lobbyNetworkManager;

        // 端口
        private const ushort GamePort = 7777;

        #region 生命周期

        /// <summary>缓存 View 并绑定 Overview 页面事件。</summary>
        protected override void Awake()
        {
            base.Awake();
        }

        /// <summary>初始化加入房间和开始按钮的默认显示。</summary>
        protected override void Start()
        {
            base.Start();
            _netcodeManager = NetworkManager.Singleton;
            _lobbyNetworkManager = LobbyNetworkManager.Instance;
            BindEvents();
            _joinGameView.ResetToDefault();
            SetJoinBtnStateUI(JoinGameBtnState.Default);
            // 初始显示"开始游戏"
            _view.ResetStartGameBtnToDefault();
        }

        /// <summary>销毁时解除 Overview 页面事件。</summary>
        protected override void OnDestroy()
        {
            UnbindEvents();
            base.OnDestroy();
        }

        /// <summary>停止 Overview 页面交互并重置加入房间浮层。</summary>
        public override void Sleep()
        {
            base.Sleep();

            if (_isJoinGameUIVisible)
            {
                KillActiveTween();
                _joinGameView.ResetToDefault();
                _isJoinGameUIVisible = false;
            }

            StopConnectionTimeout();
        }

        #endregion

        #region 事件绑定

        /// <summary>绑定 View、NGO 和倒计时事件。</summary>
        private void BindEvents()
        {
            _view.OnReadyClicked += HandleReady;
            _view.OnJoinGameToggle += HandleJoinGameToggle;
            _view.OnSettingsClicked += HandleSettings;
            _view.OnEquipmentSlotClicked += HandleEquipmentSlot;

            _joinGameView.OnJoinGameClicked += HandleJoinGame;
            _joinGameView.OnCreateGameClicked += HandleCreateGame;
            _joinGameView.OnJoinSubmit += HandleJoinSubmit;

            _netcodeManager.OnServerStarted += OnServerStartedCallback;
            _netcodeManager.OnClientConnectedCallback += OnClientConnectedCallback;
            _netcodeManager.OnClientStopped += OnClientStoppedCallback;
            _netcodeManager.OnTransportFailure += OnTransportFailureCallback;

            // 监听倒计时事件
            _lobbyNetworkManager.OnReadyCountdownUpdated += OnCountdownUpdated;
            _lobbyNetworkManager.OnCountdownStarted += OnCountdownStarted;
            _lobbyNetworkManager.OnCountdownCancelled += OnCountdownCancelled;
        }

        /// <summary>解除 View、NGO 和倒计时事件。</summary>
        private void UnbindEvents()
        {
            _view.OnReadyClicked -= HandleReady;
            _view.OnJoinGameToggle -= HandleJoinGameToggle;
            _view.OnSettingsClicked -= HandleSettings;
            _view.OnEquipmentSlotClicked -= HandleEquipmentSlot;

            _joinGameView.OnJoinGameClicked -= HandleJoinGame;
            _joinGameView.OnCreateGameClicked -= HandleCreateGame;
            _joinGameView.OnJoinSubmit -= HandleJoinSubmit;

            if (_netcodeManager != null)
            {
                _netcodeManager.OnServerStarted -= OnServerStartedCallback;
                _netcodeManager.OnClientConnectedCallback -= OnClientConnectedCallback;
                _netcodeManager.OnClientStopped -= OnClientStoppedCallback;
                _netcodeManager.OnTransportFailure -= OnTransportFailureCallback;
            }

            if (_lobbyNetworkManager != null)
            {
                _lobbyNetworkManager.OnReadyCountdownUpdated -= OnCountdownUpdated;
                _lobbyNetworkManager.OnCountdownStarted -= OnCountdownStarted;
                _lobbyNetworkManager.OnCountdownCancelled -= OnCountdownCancelled;
            }
        }

        #endregion

        #region 抽象方法实现

        /// <summary>Overview 当前没有额外的二维数据需要刷新。</summary>
        protected override void RenderView()
        {
            // 留空，后续填充
        }

        #endregion

        #region OverviewView 事件处理

        /// <summary>
        /// 准备/开始游戏按钮
        /// - 单人模式(未联网或Host仅自己)：直接触发转场景
        /// - 多人模式：Toggle 准备状态，通知服务器
        /// </summary>
        private void HandleReady()
        {
            if (!_isWorking) 
                return;

            bool isSolo = !NetworkManager.Singleton.IsConnectedClient
                || (NetworkManager.Singleton.IsServer
                    && LobbyNetworkManager.Instance.LobbyPlayers.Count == 1);

            if (isSolo)
            {
                // 单人模式：确保Host已启动，然后直接转场
                if (!NetworkManager.Singleton.IsConnectedClient)
                {
                    LobbyUIManager.Instance.OverviewCoordinator.PrepareConnectionPayload();
                    var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                    transport.SetConnectionData("0.0.0.0", GamePort);
                    NetworkManager.Singleton.StartHost();
                }

                // 隐藏JoinGameUI
                if (_isJoinGameUIVisible)
                    HideJoinGameUI();

                Debug.Log("[OverviewPresenter] 单人模式 → 直接触发进入游戏！");
                LobbyNetworkManager.Instance.StartSinglePlayerAndEnterGame();
                return;
            }

            // 多人模式：Toggle 准备状态
            _isReady = !_isReady;

            if (_isReady)
            {
                _view.SetStartGameBtnState("取消准备", new Color(1f, 0.8f, 0.2f)); // 金黄色
            }
            else
            {
                _view.SetStartGameBtnState("准备就绪", Color.white);
            }

            LobbyNetworkManager.Instance.ToggleReadyServerRpc();
        }

        /// <summary>
        /// 装备槽点击：切换到 ItemSelect 面板，并根据索引跳转对应分类Tab
        /// 0=皮肤, 1=武器, 2=道具
        /// </summary>
        private void HandleEquipmentSlot(int slotIndex)
        {
            if (!_isWorking) return;

            ItemCategory category = slotIndex switch
            {
                0 => ItemCategory.Skin,
                1 => ItemCategory.Weapon,
                2 => ItemCategory.Item,
                _ => ItemCategory.Weapon
            };

            LobbyOverviewCoordinator coordinator = LobbyUIManager.Instance.OverviewCoordinator;
            int standIndex = coordinator.LocalPlayerStandIndex;

            // 获取当前已选ID
            int currentId = GetCurrentPlayerItemId(category);

            // 先设置 ItemSelectPresenter 的分类和当前ID
            var presenter = (ItemSelectPresenter)LobbyUIManager.Instance
                .GetPresenter(LobbyScreenState.ItemSelect);
            presenter.EnterWithCategory(category, currentId);

            // 运镜 + 切换状态（统一入口）
            LobbyUIManager.Instance.EnterItemSelectFromStand(standIndex);
        }

        /// <summary>
        /// 设置按钮：切换到 Setting 面板。
        /// </summary>
        private void HandleSettings()
        {
            if (!_isWorking) return;
            LobbyUIManager.Instance.ChangeScreen(LobbyScreenState.Setting);
        }

        /// <summary>
        /// 从 LobbyPlayers 中获取本地玩家某分类的当前已选ID
        /// </summary>
        private int GetCurrentPlayerItemId(ItemCategory category)
        {
            var player = LobbyUIManager.Instance.OverviewCoordinator.LocalPlayerData;
            return category switch
            {
                ItemCategory.Skin => player.CharacterId,
                ItemCategory.Weapon => player.WeaponId,
                ItemCategory.Item => player.ItemId,
                _ => throw new System.ArgumentOutOfRangeException(nameof(category), category, null)
            };
        }

        #endregion

        #region JoinGameUI状态机

        /// <summary>根据当前房间状态显示或执行加入房间操作。</summary>
        private void HandleJoinGameToggle()
        {
            if (!_isWorking) return;

            switch (_joinBtnState)
            {
                case JoinGameBtnState.Hosting:
                    HandleDissolveRoom();
                    break;

                case JoinGameBtnState.Client:
                    HandleLeaveRoom();
                    break;

                case JoinGameBtnState.Default:
                    if (_isJoinGameUIVisible)
                        HideJoinGameUI();
                    else
                        ShowJoinGameUI();
                    break;
            }
        }

        #region — JoinGameUI 显隐

        /// <summary>播放加入房间浮层的显示动画。</summary>
        private void ShowJoinGameUI()
        {
            if (_isJoinGameUIVisible) return;

            _isJoinGameUIVisible = true;
            KillActiveTween();
            _currentTweenSeq = _joinGameView.Show();
        }

        /// <summary>隐藏加入房间浮层并停止连接超时。</summary>
        private void HideJoinGameUI()
        {
            if (!_isJoinGameUIVisible) return;

            _isJoinGameUIVisible = false;
            _joinGameView.HideInputField();
            StopConnectionTimeout();

            KillActiveTween();
            _currentTweenSeq = _joinGameView.Hide();
        }

        #endregion

        #region — Host 流程

        /// <summary>准备连接载荷并启动本地 Host。</summary>
        private void HandleCreateGame()
        {
            if (!_isWorking) return;

            LobbyUIManager.Instance.OverviewCoordinator.PrepareConnectionPayload();
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData("0.0.0.0", GamePort);
            NetworkManager.Singleton.StartHost();

            HideJoinGameUI();
            SetJoinBtnStateUI(JoinGameBtnState.Hosting);
        }

        /// <summary>关闭当前 Host 房间并恢复默认 UI。</summary>
        private void HandleDissolveRoom()
        {
            NetworkManager.Singleton.Shutdown();
            _isReady = false;
            SetJoinBtnStateUI(JoinGameBtnState.Default);
        }

        #endregion

        #region — Client 流程

        /// <summary>显示客户端 IP 输入框。</summary>
        private void HandleJoinGame()
        {
            if (!_isWorking) return;
            _joinGameView.ShowInputField();
        }

        /// <summary>校验 IP、设置传输端点并启动客户端连接。</summary>
        private void HandleJoinSubmit(string ip)
        {
            if (!_isWorking) return;

            string cleanIP = ip.Trim().Replace("\u200B", "");
            if (string.IsNullOrWhiteSpace(cleanIP))
            {
                _joinGameView.SetInfText("<color=red>IP 不能为空！</color>");
                return;
            }

            _joinGameView.SetInfText($"正在连接 {cleanIP} ...");
            _joinGameView.HideInputField();

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(cleanIP, GamePort);

            LobbyUIManager.Instance.OverviewCoordinator.PrepareConnectionPayload();
            NetworkManager.Singleton.StartClient();
            StartConnectionTimeout();
        }

        /// <summary>关闭客户端连接并恢复默认 UI。</summary>
        private void HandleLeaveRoom()
        {
            // 通知Host删除该客户端数据，然后关闭
            LobbyNetworkManager.Instance.RemovePlayerServerRpc(
                NetworkManager.Singleton.LocalClientId);
            NetworkManager.Singleton.Shutdown();
            _isReady = false;
            SetJoinBtnStateUI(JoinGameBtnState.Default);
        }

        #endregion

        #region — 连接超时

        /// <summary>启动客户端连接超时计时。</summary>
        private void StartConnectionTimeout()
        {
            StopConnectionTimeout();
            _connectionTimeoutCoroutine = StartCoroutine(ConnectionTimeoutRoutine());
        }

        /// <summary>停止当前连接超时计时。</summary>
        private void StopConnectionTimeout()
        {
            if (_connectionTimeoutCoroutine != null)
            {
                StopCoroutine(_connectionTimeoutCoroutine);
                _connectionTimeoutCoroutine = null;
            }
        }

        /// <summary>在等待时间结束后关闭仍未成功的连接。</summary>
        private IEnumerator ConnectionTimeoutRoutine()
        {
            yield return new WaitForSeconds(_connectionTimeout);

            if (!NetworkManager.Singleton.IsConnectedClient)
            {
                _joinGameView.SetInfText("<color=red>连接超时，请检查IP地址</color>");
                _joinGameView.ShowInputField();
                NetworkManager.Singleton.Shutdown();
            }
        }

        #endregion

        #region — 按钮 UI 文字/颜色切换

        /// <summary>刷新加入房间按钮的文案和颜色状态。</summary>
        private void SetJoinBtnStateUI(JoinGameBtnState state)
        {
            _joinBtnState = state;

            switch (state)
            {
                case JoinGameBtnState.Default:
                    _view.SetJoinGameBtnText("加入房间");
                    _view.SetJoinGameBtnColor(Color.white);
                    break;

                case JoinGameBtnState.Hosting:
                    _view.SetJoinGameBtnText("解散房间");
                    _view.SetJoinGameBtnColor(Color.red);
                    break;

                case JoinGameBtnState.Client:
                    _view.SetJoinGameBtnText("退出房间");
                    _view.SetJoinGameBtnColor(Color.red);
                    break;
            }
        }

        #endregion

        #endregion

        #region 网络回调

        /// <summary>处理 Host 启动成功并刷新准备按钮。</summary>
        private void OnServerStartedCallback()
        {
            StopConnectionTimeout();
            // Host启动成功 → StartGameBtn 变为"准备就绪"
            _view.SetStartGameBtnState("准备就绪", Color.white);
            _isReady = false;
        }

        /// <summary>处理本地或远端客户端连接完成。</summary>
        private void OnClientConnectedCallback(ulong clientId)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                // Host 侧：客户端连接上来了
            }
            else if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                // Client 侧：成功连上 → StartGameBtn 变为"准备就绪"
                StopConnectionTimeout();
                _joinGameView.SetInfText("<color=green>连接成功！</color>");
                HideJoinGameUI();
                SetJoinBtnStateUI(JoinGameBtnState.Client);
                _view.SetStartGameBtnState("准备就绪", Color.white);
                _isReady = false;
            }
        }

        /// <summary>连接停止后恢复大厅按钮状态。</summary>
        private void OnClientStoppedCallback(bool wasHost)
        {
            _isReady = false;

            if (wasHost)
            {
                SetJoinBtnStateUI(JoinGameBtnState.Default);
                _view.ResetStartGameBtnToDefault();
            }
            else if (_joinBtnState == JoinGameBtnState.Client)
            {
                SetJoinBtnStateUI(JoinGameBtnState.Default);
                _view.ResetStartGameBtnToDefault();
            }

            StopConnectionTimeout();
        }

        /// <summary>传输层失败后展示错误并恢复输入。</summary>
        private void OnTransportFailureCallback()
        {
            _isReady = false;
            _joinGameView.SetInfText("<color=red>网络错误，请检查IP或端口</color>");
            _joinGameView.ShowInputField();
            SetJoinBtnStateUI(JoinGameBtnState.Default);
            _view.ResetStartGameBtnToDefault();
            StopConnectionTimeout();
        }

        #endregion

        #region 倒计时回调

        /// <summary>刷新准备倒计时剩余秒数。</summary>
        private void OnCountdownUpdated(float remaining)
        {
            _view.SetStartGameBtnState($"开始…{Mathf.CeilToInt(remaining)}", Color.yellow);
        }

        /// <summary>显示准备倒计时开始状态。</summary>
        private void OnCountdownStarted()
        {
            _view.SetStartGameBtnState("开始…", Color.yellow);
        }

        /// <summary>恢复准备倒计时取消后的按钮状态。</summary>
        private void OnCountdownCancelled()
        {
            _view.SetStartGameBtnState("准备就绪", Color.white);
        }

        #endregion

        #region 工具方法

        /// <summary>停止并清理当前 JoinGame 动画序列。</summary>
        private void KillActiveTween()
        {
            if (_currentTweenSeq != null && _currentTweenSeq.IsActive())
            {
                _currentTweenSeq.Kill();
            }
            _currentTweenSeq = null;
        }

        #endregion
    }
}
