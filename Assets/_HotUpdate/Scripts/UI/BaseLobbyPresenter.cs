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
        protected bool _isWorking = false;
        public LobbyScreenState AssociatedState => _associatedState;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            ForceSleep();
        }
        protected virtual void Start()
        {
            if (LobbyNetworkManager.Instance != null)
            {
                LobbyNetworkManager.Instance.OnLobbyDataChanged += InterceptDataChanged;
            }
        }

        protected virtual void OnDestroy()
        {
            if (LobbyNetworkManager.Instance != null)
            {
                LobbyNetworkManager.Instance.OnLobbyDataChanged -= InterceptDataChanged;
            }
        }

        /// <summary>
        /// 唤醒面板：开启 UI、拉运镜、接管网络监听、刷新数据
        /// </summary>
        public void WakeUp()
        {
            _isWorking = true;
            //UI视觉显示
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            //提升虚拟相机优先级，使用Cinemachine自动计算推拉摇移
            if (_virtualCamera != null)
                _virtualCamera.Priority = 10;

            RenderView();
            
        }

        /// <summary>
        /// 停机面板：隐藏 UI、降低运镜、彻底注销网络监听
        /// </summary>
        public void Sleep()
        {
            _isWorking = false;

            ForceSleep();

            if (_virtualCamera != null)
                _virtualCamera.Priority = 0;
        }
        //不使用SetActive来控制UI的显示隐藏，而是通过CanvasGroup来控制交互和视觉效果，这样可以避免一些潜在的问题，
        //比如SetActive会导致组件的Awake/OnEnable等生命周期函数被调用
        private void ForceSleep()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        #region 可重写部分
        private void InterceptDataChanged()
        {
            if (!_isWorking)
                return; 

            RenderView();
        }

        protected abstract void RenderView();
        #endregion
    }

}
