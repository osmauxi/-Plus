using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using ProjectGame.HotFix.Core.Network;
using ProjectGame.HotFix.Lobby;
using UnityEngine.InputSystem;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// 负责统筹所有Presenter的生命周期，处理状态机切换等，连接不同的UI面板与运镜
    /// </summary>
    public class LobbyUIManager : MonoBehaviour
    {
        public static LobbyUIManager Instance { get; private set; }

        //这三个都是Overview3D的子系统，展台/模型资源生成/数据协调器
        public StandManager StandManager => _standManager;
        public AvatarResManager AvatarResManager => _avatarResManager;
        public LobbyOverviewCoordinator OverviewCoordinator => _overviewCoordinator;

        [Header("挂载所有的子面板 P 区")]
        [SerializeField] private BaseLobbyPresenter[] _presenters;

        [Header("展台系统")]
        [SerializeField] private LobbyStandLayout _standLayout;
        [SerializeField] private StandManager _standManager;
        [SerializeField] private AvatarResManager _avatarResManager;
        [SerializeField] private LobbyOverviewCoordinator _overviewCoordinator;

        [Header("运镜配置")]
        [SerializeField] private CinemachineBrain _cinemachineBrain;
        //UI面板延迟显示时间，防止运镜还没结束就显示UI
        [SerializeField, Min(0f)] private float _itemSelectUiDelay = 1f;
        [SerializeField, Min(0f)] private float _overviewUiDelay = 1f;

        private Dictionary<LobbyScreenState, BaseLobbyPresenter> _presenterDict;

        private LobbyScreenState _currentState = LobbyScreenState.None;

        private Coroutine _delayedViewCoroutine;

        public LobbyScreenState CurrentState => _currentState;

        /// <summary>
        /// 建立大厅 UI 单例并初始化 Presenter 与展位事件 
        /// </summary>
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializePresenters();
            BindStandManagerEvents();
        }

        /// <summary>首次进入大厅时激活 Overview 页面 </summary>
        private void Start()
        {
            // 初始默认进入概览界面
            ChangeScreen(LobbyScreenState.Overview);
        }

        /// <summary>
        /// 监听大厅通用返回键，并把请求交给当前 Presenter 处理 
        /// </summary>
        private void Update()
        {
            if (Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
                TryNavigateBack();
        }

        /// <summary>
        /// 销毁有效单例时解除展位事件并清空实例 
        /// </summary>
        private void OnDestroy()
        {
            if (Instance != this)
                return;

            CancelDelayedView();
            UnbindStandManagerEvents();
            Instance = null;
        }

        /// <summary>
        /// 建立页面状态到 Presenter 的映射 
        /// </summary>
        private void InitializePresenters()
        {
            _presenterDict = new Dictionary<LobbyScreenState, BaseLobbyPresenter>(5);

            foreach(var p in _presenters)
            {
                if(p.AssociatedState == LobbyScreenState.None)
                {
                    Debug.LogError($"[LobbyUIManager] 有面板未分配状态！物体名: {p.gameObject.name}");
                    continue;
                }

                _presenterDict[p.AssociatedState] = p;
            }
        }

        /// <summary>
        /// 通过状态获取对应的 Presenter
        /// </summary>
        public BaseLobbyPresenter GetPresenter(LobbyScreenState state)
        {
            return _presenterDict[state];
        }

        /// <summary>
        /// 请求当前页面执行自己的返回行为
        /// </summary>
        public bool TryNavigateBack()
        {
            if (_currentState == LobbyScreenState.None)
                return false;

            return _presenterDict[_currentState].TryHandleBackRequest();
        }

        /// <summary>
        /// 切换页面，并在返回 Overview 时延迟显示其 UI 
        /// </summary>
        public void ChangeScreen(LobbyScreenState newState)
        {
            if (_currentState == newState)
                return;

            CancelDelayedView();

            bool delayOverview = newState == LobbyScreenState.Overview && _currentState != LobbyScreenState.None;
            BaseLobbyPresenter presenter = _presenterDict[newState];
            ChangeScreenInternal(newState, !delayOverview);

            if (delayOverview)
            {
                _delayedViewCoroutine = StartCoroutine(ShowViewAfterCameraBlend(presenter,LobbyScreenState.Overview,_overviewUiDelay));
            }
        }

        /// <summary>
        /// 切换 Presenter 状态，并允许调用方控制新页面是否立即显示 
        /// </summary>
        private void ChangeScreenInternal(LobbyScreenState newState, bool showViewImmediately)
        {
            if (_currentState == newState)
                return;

            if (_currentState != LobbyScreenState.None)
                _presenterDict[_currentState].Sleep();

            _standManager.SetClickDetectionEnabled(newState == LobbyScreenState.Overview);
            _standManager.SetNameEditEnabled(
                newState == LobbyScreenState.ItemSelect && showViewImmediately);
            _presenterDict[newState].WakeUp(showViewImmediately);
            _currentState = newState;
        }

        #region StandManager 事件绑定

        /// <summary>
        /// 绑定展位点击、改名和空位事件 
        /// </summary>
        private void BindStandManagerEvents()
        {
            _standManager.OnStationClicked += HandleStandClicked;
            _standManager.OnPlayerNameChanged += HandlePlayerNameChangeRequest;
            _standManager.OnEmptyStandClicked += HandleEmptyStandClicked;
        }

        /// <summary>
        /// 解除展位点击、改名和空位事件 
        /// </summary>
        private void UnbindStandManagerEvents()
        {
            _standManager.OnStationClicked -= HandleStandClicked;
            _standManager.OnPlayerNameChanged -= HandlePlayerNameChangeRequest;
            _standManager.OnEmptyStandClicked -= HandleEmptyStandClicked;
        }

        /// <summary>
        /// 展位被点击 → 进入 ItemSelect 流程
        /// </summary>
        private void HandleStandClicked(int standIndex)
        {
            if (_currentState != LobbyScreenState.Overview || _delayedViewCoroutine != null)
                return;

            LobbyPlayerState targetData = _overviewCoordinator.GetStateForStand(standIndex).Value;
            var presenter = (ItemSelectPresenter)GetPresenter(LobbyScreenState.ItemSelect);

            if (standIndex == _overviewCoordinator.LocalPlayerStandIndex)
            {
                presenter.EnterWithCategory(ItemCategory.Weapon, targetData.WeaponId);
            }
            else
            {
                presenter.EnterAsReadonly(targetData);
            }

            // 运镜 + 状态切换
            EnterItemSelectFromStand(standIndex);
        }

        /// <summary>
        /// 玩家请求改名（弹出改名输入 UI）
        /// </summary>
        private void HandlePlayerNameChangeRequest(int standIndex, string newName)
        {
            _overviewCoordinator.RequestPlayerNameChange(newName);
        }

        /// <summary>
        /// 空展位被点击 → 预留接口
        /// </summary>
        private void HandleEmptyStandClicked(int standIndex)
        {
            Debug.Log($"[LobbyUIManager] 空展位被点击: standIndex={standIndex}，预留接口待实现");
            // TODO: 此处可扩展为加入该展位或提示操作
        }

        #endregion

        #region 页面切换与改名

        /// <summary>
        /// 从展位进入 ItemSelect（运镜 → 状态切换）
        /// </summary>
        public void EnterItemSelectFromStand(int standIndex)
        {
            CancelDelayedView();

            BaseLobbyPresenter presenter = _presenterDict[LobbyScreenState.ItemSelect];
            Transform cameraAnchor = _standLayout.GetCameraFocusPos(standIndex);
            presenter.SetVirtualCameraPose(cameraAnchor);

            ChangeScreenInternal(LobbyScreenState.ItemSelect, false);
            _delayedViewCoroutine = StartCoroutine(ShowViewAfterCameraBlend(presenter,LobbyScreenState.ItemSelect,_itemSelectUiDelay));
        }

        /// <summary>
        /// 等待最低延迟与 Cinemachine 运镜结束后显示目标页面 
        /// </summary>
        private IEnumerator ShowViewAfterCameraBlend(BaseLobbyPresenter presenter,LobbyScreenState targetState,float minimumDelay)
        {
            yield return null;

            float elapsed = 0f;
            while (elapsed < minimumDelay || _cinemachineBrain.IsBlending)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (_currentState == targetState)
            {
                presenter.ShowView();
                if (targetState == LobbyScreenState.ItemSelect)
                    _standManager.SetNameEditEnabled(true);
            }

            _delayedViewCoroutine = null;
        }

        /// <summary>取消尚未完成的页面延迟显示流程 </summary>
        private void CancelDelayedView()
        {
            if (_delayedViewCoroutine == null)
                return;

            StopCoroutine(_delayedViewCoroutine);
            _delayedViewCoroutine = null;
        }

        /// <summary>
        /// 提交改名（确认时调用）
        /// </summary>
        public void SubmitNameEdit(string newName)
        {
            _overviewCoordinator.RequestPlayerNameChange(newName);
        }

        #endregion
    }
}
