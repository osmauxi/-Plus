using Cinemachine;
using ProjectGame.HotFix.Netcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// 所有大厅MVP P层脚本的基类，提供生命周期管理，强制约束子类的生命周期
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseLobbyPresenter : MonoBehaviour
    {
        [Header("基础配置")]
        [SerializeField] private LobbyScreenState _associatedState;

        [Header("相机运镜绑定")]
        [Tooltip("当本UI激活时，自动提升权重的虚拟相机")]
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;

        private CanvasGroup _canvasGroup;
        private LobbyNetworkManager _lobbyNetworkManager;
        protected bool _isWorking = false;
        public LobbyScreenState AssociatedState => _associatedState;

        /// <summary>
        /// 处理当前页面的返回请求；根页面默认不消费该请求 
        /// </summary>
        public virtual bool TryHandleBackRequest()
        {
            return false;
        }

        /// <summary>
        /// 缓存 CanvasGroup 并把 Presenter 置为休眠状态 
        /// </summary>
        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _virtualCamera.Priority = 0;
            ForceSleep();
        }
        /// <summary>订阅大厅权威数据变化事件 </summary>
        protected virtual void Start()
        {
            LobbyNetworkManager.InstanceChanged += HandleLobbyNetworkManagerChanged;
            HandleLobbyNetworkManagerChanged(LobbyNetworkManager.Instance);
        }

        /// <summary>销毁时解除大厅权威数据事件 </summary>
        protected virtual void OnDestroy()
        {
            LobbyNetworkManager.InstanceChanged -= HandleLobbyNetworkManagerChanged;
            if (_lobbyNetworkManager != null)
                _lobbyNetworkManager.OnLobbyDataChanged -= InterceptDataChanged;
        }

        private void HandleLobbyNetworkManagerChanged(LobbyNetworkManager manager)
        {
            if (_lobbyNetworkManager == manager) return;
            if (_lobbyNetworkManager != null)
                _lobbyNetworkManager.OnLobbyDataChanged -= InterceptDataChanged;
            _lobbyNetworkManager = manager;
            if (_lobbyNetworkManager != null)
                _lobbyNetworkManager.OnLobbyDataChanged += InterceptDataChanged;
        }

        /// <summary>
        /// 唤醒面板：开启 UI、拉运镜、接管网络监听、刷新数据
        /// </summary>
        public void WakeUp(bool showViewImmediately = true)
        {
            _isWorking = true;
            if (showViewImmediately)
                ShowView();
            else
                SetViewVisible(false);

            //提升虚拟相机优先级，使用Cinemachine自动计算推拉摇移
            _virtualCamera.Priority = 10;

            RenderView();
        }

        /// <summary>
        /// 只显示 Presenter 视图，不改变工作状态或虚拟相机 
        /// </summary>
        public virtual void ShowView()
        {
            SetViewVisible(true);
        }

        /// <summary>让 Presenter 虚拟相机完整采用指定锚点的位置和旋转 </summary>
        public void SetVirtualCameraPose(Transform cameraAnchor)
        {
            _virtualCamera.transform.SetPositionAndRotation(
                cameraAnchor.position,
                cameraAnchor.rotation);
        }

        /// <summary>
        /// 隐藏 UI、降低镜头优先级
        /// </summary>
        public virtual void Sleep()
        {
            _isWorking = false;

            ForceSleep();

            _virtualCamera.Priority = 0;
        }

        //不使用SetActive来控制UI的显示隐藏，而是通过CanvasGroup来控制交互和视觉效果，这样可以避免一些潜在的问题，
        //比如SetActive会导致组件的Awake/OnEnable等生命周期函数被调用
        /// <summary>
        /// 立即隐藏并禁用当前 Presenter 的 CanvasGroup 
        /// </summary>
        private void ForceSleep()
        {
            SetViewVisible(false);
        }

        /// <summary>
        /// 统一控制 Presenter 的透明度和射线交互 
        /// </summary>
        private void SetViewVisible(bool visible)
        {
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }

        #region 可重写部分
        /// <summary>
        /// 仅在 Presenter 工作时把网络数据变化转发给渲染方法 
        /// </summary>
        private void InterceptDataChanged()
        {
            if (!_isWorking)
                return; 

            RenderView();
        }

        /// <summary>
        /// 由具体 Presenter 刷新当前页面内容 
        /// </summary>
        protected abstract void RenderView();
        #endregion
    }

}
