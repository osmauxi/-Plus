using System;
using System.Threading;
using UnityEngine;

namespace ProjectGame.HotFix.UI.Gameplay
{
    /// <summary>
    /// P层只能通过接口向上层管理器发出切换请求，生命周期受上层管理器控制
    /// </summary>
    public abstract class BaseGameplayPresenter : MonoBehaviour, IGameplayUIPresenter
    {
        [SerializeField] private bool _overridePolicy;
        [SerializeField] private GameplayUIPolicy _policy;

        private CancellationTokenSource _visibleCts;
        protected IGameplayUINavigation Navigator { get; private set; }
        protected abstract BaseGameplayUIView UntypedView { get; }
        public abstract GameplayUIId Id { get; }
        public GameplayUIPolicy Policy => _overridePolicy ? _policy : GameplayUIPolicy.For(Id);
        public bool IsInitialized { get; private set; }
        //Interactive必然Visible，Visible必然Open，三个bool对应四种状态
        public bool IsOpen { get; private set; }
        public bool IsVisible { get; private set; }
        public bool IsInteractive { get; private set; }
        public Transform ViewRoot => UntypedView == null ? null : UntypedView.transform;
        /// <summary>
        ///给下层MVP预留的异步方法CTS
        /// </summary>
        protected CancellationToken VisibleToken => _visibleCts?.Token ?? new CancellationToken(true);

        void IGameplayUIPresenter.Initialize(IGameplayUINavigation navigation)
        {
            if (IsInitialized) 
                return;
            if (UntypedView == null)
                throw new InvalidOperationException($"{name} ({Id}) 未绑定 View");
            //获取请求页面切换的能力
            Navigator = navigation ?? throw new ArgumentNullException(nameof(navigation));
            UntypedView.InitializeHidden();
            IsInitialized = true;
            try 
            {
                OnInitialize(); 
            }
            catch 
            {
                Shutdown(); throw; 
            }
        }

        void IGameplayUIPresenter.SetPresentation(bool open, bool visible, bool interactive)
        {
            if (!IsInitialized) 
                throw new InvalidOperationException($"{Id} 尚未初始化");
            //先强行将状态合法化，也就是先满足基本的关系
            visible &= open;
            interactive &= visible;
            //保存一次旧状态，发现没变就不管
            bool wasOpen = IsOpen, wasVisible = IsVisible, wasInteractive = IsInteractive;
            if (wasOpen == open && wasVisible == visible && wasInteractive == interactive) 
                return;
            IsOpen = open;
            IsVisible = visible;
            IsInteractive = interactive;

            bool opened = !wasOpen && open;
            bool closed = wasOpen && !open;
            bool shown = !wasVisible && visible;
            bool hidden = wasVisible && !visible;
            bool interactionChanged = wasInteractive != interactive;
            //这里的两个判定是先进行Visible生命周期的资源的准备/回收
            if (hidden) 
                CancelVisibleTasks();
            if (shown) 
                _visibleCts = new CancellationTokenSource();

            if (!interactive)
                UntypedView.SetPresentation(visible, false);

            if (opened) 
                OnOpened();
            //先渲染一次再现形
            if (shown)
            {
                RenderView();
                OnShown();
            }

            if (hidden)
               OnHidden();
            if (closed) 
               OnClosed();

            UntypedView.SetPresentation(visible, interactive);
            if (wasInteractive != interactive) 
                OnInteractionChanged(interactive);
        }

        public void Refresh()
        {
            if (IsInitialized && IsVisible) RenderView();
        }

        public virtual bool TryHandleBackRequest() => false;
        void IGameplayUIPresenter.Shutdown() => Shutdown();

        private void Shutdown()
        {
            if (!IsInitialized) 
                return;
            IsInitialized = false;
            bool wasOpen = IsOpen, wasVisible = IsVisible;
            IsOpen = IsVisible = IsInteractive = false;
            try 
            {
                CancelVisibleTasks(); 
            }
            finally
            {
                try
                {
                    if (UntypedView != null) 
                        UntypedView.SetPresentation(false, false);
                    if (wasVisible) 
                        OnHidden();
                    if (wasOpen) 
                        OnClosed();
                }
                finally
                {
                    try 
                    {
                        OnShutdown(); 
                    }
                    finally 
                    {
                        Navigator = null; 
                    }
                }
            }
        }

        private void CancelVisibleTasks()
        {
            CancellationTokenSource source = _visibleCts;
            _visibleCts = null;
            if (source == null) 
                return;
            try 
            {
                source.Cancel(); 
            }
            finally 
            {
                source.Dispose(); 
            }
        }

        protected virtual void OnDestroy() => Shutdown();
        protected virtual void OnInitialize() { }
        protected virtual void OnOpened() { }
        protected virtual void OnClosed() { }
        protected virtual void OnShown() { }
        protected virtual void OnHidden() { }
        protected virtual void OnInteractionChanged(bool interactive) { }
        protected virtual void OnShutdown() { }
        protected abstract void RenderView();
    }

    /// <summary>具体模块继承此类型；Model 可通过自身公开 Bind 方法注入。</summary>
    public abstract class BaseGameplayPresenter<TView> : BaseGameplayPresenter
        where TView : BaseGameplayUIView
    {
        [SerializeField] private TView _view;
        protected TView View => _view;
        protected override BaseGameplayUIView UntypedView => _view;
    }
}
