using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Core.Events;
using ProjectGame.HotFix.Gameplay.Events;
using ProjectGame.HotFix.Gameplay.Input;
using ProjectGame.HotFix.Gameplay.State;
using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace ProjectGame.HotFix.UI.Gameplay
{
    /// <summary>
    /// UIGameUIScene 的本机编排入口。HUD 共存，Screen / Modal 各自维护无重复的页面栈。
    /// Show 保留被覆盖页面；Hide 可关闭任意已打开页面；返回只作用于最上层页面。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameplayUIManager : MonoBehaviour, IGameplayUINavigation
    {
        public static GameplayUIManager Instance { get; private set; }

        [Header("场景 UI 根节点（模块 View 必须挂在所属层下）")]
        [SerializeField] private RectTransform _hudRoot;
        [SerializeField] private RectTransform _screenRoot;
        [SerializeField] private RectTransform _modalRoot;
        [SerializeField] private InputSystemUIInputModule _inputModule;
        [Header("只注册具体模块 Presenter；空列表可作为场景框架运行")]
        [SerializeField] private BaseGameplayPresenter[] _presenters = Array.Empty<BaseGameplayPresenter>();
        /// <summary>
        /// 通过ID保存所有UI模块
        /// </summary>
        private readonly Dictionary<GameplayUIId, Entry> _entries = new();
        /// <summary>
        /// 哪些UI处于打开状态
        /// </summary>
        private readonly HashSet<GameplayUIId> _open = new();
        /// <summary>
        /// Screen的打开顺序
        /// </summary>
        private readonly List<GameplayUIId> _screens = new();
        /// <summary>
        /// Modal的打开顺序
        /// </summary>
        private readonly List<GameplayUIId> _modals = new();
        /// <summary>
        /// 防止递归调用的队列
        /// </summary>
        private readonly Queue<Action> _commands = new();
        private EventSubscriptionGroup _subscriptions;
        private GameplayUIInputBridge _inputBridge;
        private InputManager _explicitInput;
        private bool _busy;
        private bool _started;
        private bool _lastCanNavigate;

        public bool IsInitialized { get; private set; }
        public bool IsGameplayActive { get; private set; }
        public GameplayUIId TopScreen => Peek(_screens);
        public GameplayUIId TopModal => Peek(_modals);
        public GameplayUIId TopUI => TopModal != GameplayUIId.None ? TopModal : TopScreen;
        public event Action<GameplayUIVisibilityChangedEvent> VisibilityChanged;
        public event Action<GameplayUITopChangedEvent> TopChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("[GameplayUIManager] 当前已存在场景 UI 管理器", this);
                enabled = false;
                return;
            }
            Instance = this;
            if (_inputModule != null) _inputModule.enabled = false;
        }

        private void Start()
        {
            _started = true;
            if (!IsInitialized && enabled) 
                InitializeScene();
        }

        private void OnEnable()
        {
            if (_started && !IsInitialized && Instance == this) InitializeScene();
        }

        private void InitializeScene()
        {
            try 
            {
                Initialize(_presenters); 
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                enabled = false;
            }
        }

        /// <summary>
        /// UI页面初始化入口，一次性完成所有面板的注册和初始化    
        /// </summary>
        public void Initialize(IEnumerable<IGameplayUIPresenter> presenters, InputManager inputManager = null)
        {
            if (IsInitialized) 
                return;
            if (presenters == null) 
                throw new ArgumentNullException(nameof(presenters));
            try
            {
                //读取所有P层和对应Policy缓存进entries
                foreach (IGameplayUIPresenter presenter in presenters)
                {
                    if (presenter == null || (presenter is UnityEngine.Object obj && obj == null))
                        throw new InvalidOperationException("Gameplay UI 注册列表包含空引用");
                    if (presenter.Id == GameplayUIId.None ||
                        !Enum.IsDefined(typeof(GameplayUIId), presenter.Id))
                        throw new InvalidOperationException($"Gameplay UI 标识无效：{presenter.Id}");
                    if (_entries.ContainsKey(presenter.Id))
                        throw new InvalidOperationException($"重复 Gameplay UI 标识：{presenter.Id}");
                    GameplayUIPolicy policy = presenter.Policy;
                    policy.Validate();
                    ValidateViewRoot(presenter, policy.Layer);
                    _entries.Add(presenter.Id, new Entry(presenter, policy));
                }
                //IGameplayUINavigation就是UIManager自己
                foreach (Entry entry in _entries.Values) 
                    entry.Presenter.Initialize(this);
                //初始化InputBridge
                _explicitInput = inputManager;
                _inputBridge = new GameplayUIInputBridge(_inputModule);
                _inputBridge.Bind(inputManager != null ? inputManager : InputManager.Instance);
                //注册观察者总线，_subscriptions管理所有事件的Dispose
                _subscriptions = new EventSubscriptionGroup();
                _subscriptions.Add(LocalEvents.Subscribe<GameplayUIRequest>(HandleRequest));
                _subscriptions.Add(LocalEvents.Subscribe<GameStateChangedEvent>(HandleGameState));
                IsInitialized = true;
                SetGameplayActive(GameStateController.Instance != null && GameStateController.Instance.IsPlaying);
            }
            catch 
            {
                Shutdown(); 
                throw; 
            }
        }
        /// <summary>
        /// 检查View是否在正确的层级
        /// </summary>
        private void ValidateViewRoot(IGameplayUIPresenter presenter, GameplayUILayer layer)
        {
            if (!(presenter is BaseGameplayPresenter component)) 
                return;
            Transform root = layer == GameplayUILayer.Hud ? _hudRoot :
                layer == GameplayUILayer.Screen ? _screenRoot : _modalRoot;
            if (component.ViewRoot == null)
                throw new InvalidOperationException($"{component.name} ({presenter.Id}) 未绑定 View");
            if (root != null && !component.ViewRoot.IsChildOf(root))
                throw new InvalidOperationException($"{component.name} 的 View 必须位于 {root.name} 层级下");
        }

        private void LateUpdate()
        {
            if (!IsInitialized) 
                return;
            try 
            {
                UpdateInput(); 
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                Shutdown();
                enabled = false;
            }
        }

        private void UpdateInput()
        {
            //因为场景会先于InputManager组件加载，这里也需要支持输入服务销毁、重建后的重新绑定
            //Bind内部有过滤，不会出现频繁重复绑定
            InputManager input = _explicitInput != null ? _explicitInput : InputManager.Instance;
            _inputBridge.Bind(input);
            _inputBridge.SetBlocked(HasInputBlocker());
            //强制触发一次Reconcile，主要是检测Input状态改变导致的转换
            if (_lastCanNavigate != _inputBridge.CanNavigate) 
                Enqueue(() => { });
            if (!IsGameplayActive || !_inputBridge.CanNavigate) 
                return;
            //在同一次输入更新里只消费一种导航，防止按键把打开和关闭连做两次。
            if (input.MenuBackPressedThisFrame) 
                TryNavigateBack();
            //如果当前没有Modal，且没有Screen/当前Screen允许离开，那么允许Toggle
            else if (input.PlayerStatusPressedThisFrame && TopModal == GameplayUIId.None &&
                     (TopScreen == GameplayUIId.None || _entries[TopScreen].Policy.CanCloseOnBack))
                Toggle(GameplayUIId.PlayerStatus);
        }

        private void HandleGameState(GameStateChangedEvent data)
            => SetGameplayActive(data.CurrentState == GameState.GamePlaying);

        /// <summary>
        /// 默认由GameState事件驱动，也支持本机场景装配或测试显式控制。
        /// </summary>
        public void SetGameplayActive(bool active)
        {
            if (!IsInitialized) 
                return;
            Enqueue(() =>
            {
                if (IsGameplayActive == active) 
                    return;
                IsGameplayActive = active;
                //清空状态之后打开初始HUD
                _open.Clear();
                _screens.Clear();
                _modals.Clear();
                if (active && _entries.ContainsKey(GameplayUIId.GameplayHUD))
                    _open.Add(GameplayUIId.GameplayHUD);
            });
        }

        public bool IsOpen(GameplayUIId id) => _open.Contains(id);
        public bool IsVisible(GameplayUIId id)
            => _entries.TryGetValue(id, out Entry entry) && entry.Presenter.IsVisible;

        public bool TryGetPresenter<T>(GameplayUIId id, out T presenter) where T : class, IGameplayUIPresenter
        {
            presenter = _entries.TryGetValue(id, out Entry entry) ? entry.Presenter as T : null;
            return presenter != null;
        }

        /// <summary>
        /// 请求让某个UI进入Open状态
        /// 幂等打开；已在栈中的页面不重复压栈，也不越过现有顶层页面
        /// </summary>
        public bool Show(GameplayUIId id)
        {
            //检查两次Open，第一次判定请求现在是否合法，因为Enqueue会进队列等执行，所以内部也进行一次判定
            if (!CanOpen(id)) 
                return false;
            return Enqueue(() => { if (CanOpen(id)) OpenInternal(id); });
        }

        /// <summary>
        /// 业务层强制关闭一个页面，幂等操作，所以不会关心UI的实际状态
        /// </summary>
        public bool Hide(GameplayUIId id)
        {
            if (!IsInitialized || !_entries.ContainsKey(id)) 
                return false;
            return Enqueue(() => CloseInternal(id));
        }

        /// <summary>
        /// 用户开关遵循CanCloseOnBack
        /// </summary>
        public bool Toggle(GameplayUIId id)
        {
            if (!IsInitialized || !_entries.TryGetValue(id, out Entry entry)) 
                return false;
            if (_open.Contains(id) && !entry.Policy.CanCloseOnBack) 
                return false;
            if (!_open.Contains(id) && !CanOpen(id)) 
                return false;
            return Enqueue(() =>
            {
                if (_open.Contains(id))
                {
                    if (entry.Policy.CanCloseOnBack) 
                        CloseInternal(id);
                }
                else if (CanOpen(id)) 
                    OpenInternal(id);
            });
        }

        public bool TryNavigateBack()
        {
            if (!IsInitialized || !IsGameplayActive) 
                return false;
            bool handled = false;
            bool deferred = _busy;
            Enqueue(() =>
            {
                GameplayUIId top = TopUI;
                if (top != GameplayUIId.None)
                {
                    Entry entry = _entries[top];
                    if (entry.Presenter.TryHandleBackRequest()) { handled = true; return; }
                    if (entry.Policy.CanCloseOnBack)
                    {
                        CloseInternal(top);
                        handled = true;
                        return;
                    }
                    // 必选页面保留在栈中，但仍可进入设置；禁止关闭的 Modal 则消费返回。
                    if (entry.Policy.Layer == GameplayUILayer.Modal) { handled = true; return; }
                }
                if (CanOpen(GameplayUIId.Settings))
                {
                    OpenInternal(GameplayUIId.Settings);
                    handled = true;
                }
            });
            return deferred || handled;
        }
        /// <summary>
        /// 只关闭当前最上层页面
        /// </summary>
        public bool CloseTop()
        {
            if (!IsInitialized || TopUI == GameplayUIId.None || !_entries[TopUI].Policy.CanCloseOnBack)
                return false;
            return Enqueue(() =>
            {
                if (TopUI != GameplayUIId.None && _entries[TopUI].Policy.CanCloseOnBack)
                    CloseInternal(TopUI);
            });
        }

        /// <summary>
        /// 关闭所有Screen和Modal，保留调用方对 HUD 的显隐选择。
        /// </summary>
        public bool CloseAllScreens()
            => IsInitialized && Enqueue(CloseScreensInternal);
        /// <summary>
        /// 将当前UI收回到HUD
        /// </summary>
        public bool ResetToGameplay()
        {
            if (!IsInitialized || !IsGameplayActive) 
                return false;
            return Enqueue(() =>
            {
                CloseScreensInternal();
                if (_entries.ContainsKey(GameplayUIId.GameplayHUD)) 
                    OpenInternal(GameplayUIId.GameplayHUD);
            });
        }

        private bool CanOpen(GameplayUIId id)
        {
            //Manager必须初始化，比如进入Gameplay阶段，页面必须已注册
            if (!IsInitialized || !IsGameplayActive || !_entries.TryGetValue(id, out Entry entry)) 
                return false;
            //如果页面不阻挡输入则允许，阻挡需要确保Input当前可以被接管
            return !entry.Policy.BlocksGameplayInput || (_inputBridge != null && _inputBridge.CanNavigate);
        }

        private void OpenInternal(GameplayUIId id)
        {
            if (!_open.Add(id)) 
                return;
            //根据Layer加入对应的Stack
            GameplayUILayer layer = _entries[id].Policy.Layer;
            if (layer == GameplayUILayer.Screen) 
                _screens.Add(id);
            if (layer == GameplayUILayer.Modal) 
                _modals.Add(id);
        }

        private void CloseInternal(GameplayUIId id)
        {
            _open.Remove(id);
            _screens.Remove(id);
            _modals.Remove(id);
        }
        /// <summary>
        /// 一次性清空所有Screen和Modal
        /// </summary>
        private void CloseScreensInternal()
        {
            foreach (GameplayUIId id in _screens) 
                _open.Remove(id);
            foreach (GameplayUIId id in _modals) 
                _open.Remove(id);
            _screens.Clear();
            _modals.Clear();
        }
        /// <summary>
        /// 关键方法，把所有UI状态修改串行化，并保证每次修改之后统一执行一次Reconcile
        /// 一个UI切换可能会触发回调进行其他页面切换，所以这里必须用队列保持秩序
        /// 不然会出现这个切换没有完成就在跑下一个的情况
        /// </summary>
        private bool Enqueue(Action command)
        {
            _commands.Enqueue(command);
            if (_busy) 
                return true;
            _busy = true;
            try
            {
                int remaining = 64;
                while (_commands.Count > 0 && IsInitialized)
                {
                    if (--remaining < 0)
                        throw new InvalidOperationException("UI 导航回调形成循环，已关闭管理器以释放输入");
                    //比较更新前previousTop与更新后来判定是否触发TopChanged
                    GameplayUIId previousTop = TopUI;
                    _commands.Dequeue().Invoke();
                    Reconcile();
                    if (previousTop != TopUI)
                    {
                        var change = new GameplayUITopChangedEvent(previousTop, TopUI);
                        TopChanged?.Invoke(change);
                        LocalEvents.Publish(change);
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                Shutdown();
                return false;
            }
            finally 
            {
                _busy = false; 
            }
        }
        /// <summary>
        /// 核心方法，OpenInternal等方法都只是在逻辑上修改，就是为了把实际修改集中到这里
        /// 根据当前完整逻辑状态，重新计算所有P层最终应该处于什么状态
        /// </summary>
        private void Reconcile()
        {
            //判定当前打开的场景有没有阻断输入的
            _inputBridge.SetBlocked(HasInputBlocker());
            _lastCanNavigate = _inputBridge.CanNavigate;
            //算HUD是否需要隐藏
            bool hideHud = HidesHud(TopScreen) || HidesHud(TopModal);
            var changes = new List<GameplayUIVisibilityChangedEvent>();
            //Manager对所有P层重新判定状态
            foreach (var pair in _entries)
            {
                GameplayUIId id = pair.Key;
                Entry entry = pair.Value;
                bool open = _open.Contains(id);
                //open状态，如果是HUD，当前顶层UI没有要求隐藏HUD则visible
                //如果是Screen，只显示最上层的那个
                //如果是Modal，是显示最上层的那个
                //Modal不会让TopScreen隐藏
                bool visible = false;
                if (open)
                {
                    if (entry.Policy.Layer == GameplayUILayer.Hud)
                        visible = !hideHud;
                    else if (entry.Policy.Layer == GameplayUILayer.Screen)
                        visible = TopScreen == id;
                    else if (entry.Policy.Layer == GameplayUILayer.Modal)
                        visible = TopModal == id;
                }
                //Screen可在Modal后方保持可见，但不接受任何交互
                //visible状态，必须是TopUI,不能是HUD，InputBridge必须允许导航
                bool isTopUI = id == TopUI;
                bool isNavigationPage = entry.Policy.Layer != GameplayUILayer.Hud;
                bool canReceiveInput = _inputBridge.CanNavigate;
                bool interactive = visible && isTopUI && isNavigationPage && canReceiveInput;

                bool changed = entry.Presenter.IsOpen != open || entry.Presenter.IsVisible != visible;
                //把这三个最终状态，转化成P层实际生命周期回调和View表现
                entry.Presenter.SetPresentation(open, visible, interactive);
                if (changed)
                    changes.Add(new GameplayUIVisibilityChangedEvent(id, open, visible));
            }
            //所有P层都已经提交本次状态后，提交到观察者
            foreach (GameplayUIVisibilityChangedEvent change in changes)
            {
                VisibilityChanged?.Invoke(change);
                LocalEvents.Publish(change);
            }
        }

        private bool HidesHud(GameplayUIId id)
            => id != GameplayUIId.None && _entries[id].Policy.HidesHud;

        private bool HasInputBlocker()
        {
            if (!IsGameplayActive)
                return false;
            foreach (GameplayUIId id in _open)
                if (_entries[id].Policy.BlocksGameplayInput) 
                    return true;
            return false;
        }

        private void HandleRequest(GameplayUIRequest request)
        {
            switch (request.Command)
            {
                case GameplayUICommand.Show: Show(request.Id); break;
                case GameplayUICommand.Hide: Hide(request.Id); break;
                case GameplayUICommand.Toggle: Toggle(request.Id); break;
                case GameplayUICommand.Back: TryNavigateBack(); break;
                case GameplayUICommand.CloseTop: CloseTop(); break;
                case GameplayUICommand.CloseAllScreens: CloseAllScreens(); break;
                case GameplayUICommand.ResetToGameplay: ResetToGameplay(); break;
            }
        }

        /// <summary>幂等释放。即使模块清理失败也继续释放其他模块、订阅与输入句柄。</summary>
        public void Shutdown()
        {
            IsInitialized = IsGameplayActive = false;
            _commands.Clear();
            _subscriptions?.Dispose();
            _subscriptions = null;
            foreach (Entry entry in _entries.Values)
            {
                try { entry.Presenter.Shutdown(); }
                catch (Exception exception) { Debug.LogException(exception, this); }
            }
            _entries.Clear();
            _open.Clear();
            _screens.Clear();
            _modals.Clear();
            _inputBridge?.Dispose();
            _inputBridge = null;
            _explicitInput = null;
            _lastCanNavigate = false;
        }

        private void OnDisable() => Shutdown();
        private void OnDestroy()
        {
            Shutdown();
            if (Instance == this) Instance = null;
            VisibilityChanged = null;
            TopChanged = null;
        }

        private static GameplayUIId Peek(List<GameplayUIId> stack)
            => stack.Count == 0 ? GameplayUIId.None : stack[stack.Count - 1];

        /// <summary>
        /// 同时缓存Policy和Presenter方便后续访问
        /// </summary>
        private sealed class Entry
        {
            public readonly IGameplayUIPresenter Presenter;
            public readonly GameplayUIPolicy Policy;
            public Entry(IGameplayUIPresenter presenter, GameplayUIPolicy policy)
            {
                Presenter = presenter;
                Policy = policy;
            }
        }
    }
}
