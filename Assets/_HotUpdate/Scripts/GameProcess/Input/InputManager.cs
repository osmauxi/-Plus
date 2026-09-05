using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Core.Events;
using ProjectGame.HotFix.Gameplay.Events;
using ProjectGame.HotFix.Gameplay.Runtime;
using ProjectGame.HotFix.Gameplay.State;
using ProjectGame.HotFix.Settings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectGame.HotFix.Gameplay.Input
{
    /// <summary>
    /// 本地输入当前由哪个层级消费 
    /// </summary>
    public enum InputContext : byte
    {
        Disabled = 0,
        Gameplay = 1,
        UI = 2,
    }

    /// <summary>
    /// GameRuntimeScene 内唯一的本地输入入口 
    /// 负责加载玩家改键、切换 ActionMap，并隔离 Gameplay 与 UI 输入 
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InputManager : MonoBehaviour, IGameRuntimeService
    {
        private const string MoveActionName = "Move";
        private const string JumpActionName = "Jump";
        private const string InteractActionName = "Interact";
        private const string FireActionName = "Fire";
        private const string AimActionName = "Aim";
        private const string ReloadActionName = "Reload";
        private const string SprintActionName = "Sprint";
        private const string CameraRotateActionName = "CameraRotate";
        private const string CameraZoomActionName = "CameraZoom";
        public static InputManager Instance { get; private set; }

        [Header("Input Actions")]
        [SerializeField] private InputActionAsset _inputActionsTemplate;
        [SerializeField] private string _gameplayActionMapName = "Gameplay";
        [SerializeField] private string _uiActionMapName = "UI";

        [Header("Cursor")]
        [SerializeField] private bool _showCursorInGameplay = true;
        [SerializeField] private CursorLockMode _gameplayCursorLockMode = CursorLockMode.None;
        [SerializeField] private bool _showCursorInUI = true;

        private readonly List<ContextRequest> _contextRequests = new();

        private InputActionAsset _runtimeInputActions;
        private InputActionMap _gameplayActionMap;
        private InputActionMap _uiActionMap;

        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _interactAction;
        private InputAction _fireAction;
        private InputAction _aimAction;
        private InputAction _reloadAction;
        private InputAction _sprintAction;
        private InputAction _cameraRotateAction;
        private InputAction _cameraZoomAction;

        private SettingSaveService _settingSaveService;
        private InputRebindService _inputRebindService;
        private IDisposable _gameStateSubscription;

        private InputContext _baseContext = InputContext.Disabled;
        private int _nextContextRequestId;

        public bool IsInitialized { get; private set; }
        public bool LastBindingLoadSucceeded { get; private set; }
        public InputContext BaseContext => _baseContext;
        public InputContext CurrentContext { get; private set; } = InputContext.Disabled;
        public bool IsGameplayInputEnabled =>
            IsInitialized && CurrentContext == InputContext.Gameplay && _gameplayActionMap.enabled;
        public bool IsUIInputEnabled =>
            IsInitialized && CurrentContext == InputContext.UI &&
            (_uiActionMap == null || _uiActionMap.enabled);
        public bool HasUIActionMap => _uiActionMap != null;
        public InputActionAsset RuntimeInputActions => _runtimeInputActions;

        public Vector2 Move => IsGameplayInputEnabled ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
        public bool JumpPressedThisFrame => IsGameplayInputEnabled && _jumpAction.WasPressedThisFrame();
        public bool InteractPressedThisFrame => IsGameplayInputEnabled && _interactAction.WasPressedThisFrame();
        public bool FirePressedThisFrame => IsGameplayInputEnabled && _fireAction.WasPressedThisFrame();
        public bool FireHeld => IsGameplayInputEnabled && _fireAction.IsPressed();
        public bool AimHeld => IsGameplayInputEnabled && _aimAction.IsPressed();
        public bool ReloadPressedThisFrame => IsGameplayInputEnabled && _reloadAction.WasPressedThisFrame();

        public bool SprintHeld => IsGameplayInputEnabled && _sprintAction.IsPressed();
        /// <summary>
        /// 离散相机旋转输入 默认 Q=-1、E=+1，只在按下边沿返回一次，
        /// 避免按住按键后每帧连续叠加 90 度 
        /// </summary>
        public float CameraRotateStep =>
            IsGameplayInputEnabled && _cameraRotateAction.WasPressedThisFrame()
                ? Mathf.Sign(_cameraRotateAction.ReadValue<float>())
                : 0f;
        /// <summary>鼠标滚轮等连续缩放输入；当前只消费 Y 分量 </summary>
        public Vector2 CameraZoom =>
            IsGameplayInputEnabled ? _cameraZoomAction.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 PointerPosition =>
            Mouse.current == null ? Vector2.zero : Mouse.current.position.ReadValue();

        public event Action<InputContext, InputContext> ContextChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            ShutdownInternal();

            if (Instance == this)
                Instance = null;
        }

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return UniTask.CompletedTask;

            cancellationToken.ThrowIfCancellationRequested();

            if (_inputActionsTemplate == null)
                throw new InvalidOperationException($"{nameof(InputManager)} 未绑定 InputActionAsset ");

            try
            {
                _runtimeInputActions = Instantiate(_inputActionsTemplate);
                _runtimeInputActions.name = $"{_inputActionsTemplate.name}_Runtime";
                _runtimeInputActions.Disable();

                ResolveActionMapsAndActions();

                _settingSaveService = new SettingSaveService();
                _inputRebindService = new InputRebindService(_runtimeInputActions);
                LastBindingLoadSucceeded = ReloadPlayerBindingsInternal();

                _gameStateSubscription =
                    LocalEvents.Subscribe<GameStateChangedEvent>(HandleGameStateChanged);

                _baseContext = ResolveBaseContext(GameStateController.Instance);
                IsInitialized = true;
                ApplyResolvedContext(true);

                Debug.Log(
                    $"[{nameof(InputManager)}] 初始化完成 " +
                    $"Context={CurrentContext}, BindingOverrides={LastBindingLoadSucceeded}, " +
                    $"UIActionMap={HasUIActionMap}");

                return UniTask.CompletedTask;
            }
            catch
            {
                ShutdownInternal();
                throw;
            }
        }

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            ShutdownInternal();
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 设置没有临时 UI/禁用请求时使用的基础输入上下文 
        /// </summary>
        public void SetBaseContext(InputContext context)
        {
            EnsureInitialized();

            if (_baseContext == context)
                return;

            _baseContext = context;
            ApplyResolvedContext(false);
        }

        /// <summary>
        /// 临时占用输入上下文 返回的句柄释放后自动恢复到下一层请求或基础上下文 
        /// 支持多个 UI 叠加并允许句柄乱序释放 
        /// </summary>
        public IDisposable AcquireContext(InputContext context, object owner = null)
        {
            EnsureInitialized();

            int requestId = ++_nextContextRequestId;
            _contextRequests.Add(new ContextRequest(requestId, context, owner));
            ApplyResolvedContext(false);

            return new ContextLease(this, requestId);
        }

        /// <summary>
        /// 清理指定拥有者创建的全部上下文请求，供 UI 被强制销毁时兜底 
        /// </summary>
        public void ReleaseContexts(object owner)
        {
            if (!IsInitialized || owner == null)
                return;

            bool removed = false;
            for (int i = _contextRequests.Count - 1; i >= 0; i--)
            {
                if (!ReferenceEquals(_contextRequests[i].Owner, owner))
                    continue;

                _contextRequests.RemoveAt(i);
                removed = true;
            }

            if (removed)
                ApplyResolvedContext(false);
        }

        /// <summary>
        /// 从 user_settings.json 重新读取并应用当前玩家的按键预设 
        /// </summary>
        public bool ReloadPlayerBindings()
        {
            EnsureInitialized();
            LastBindingLoadSucceeded = ReloadPlayerBindingsInternal();
            return LastBindingLoadSucceeded;
        }

        /// <summary>
        /// 应用外部设置界面产生的 Binding Override，并保持当前输入上下文不变 
        /// </summary>
        public bool ApplyBindingOverrides(string bindingOverridesJson)
        {
            EnsureInitialized();

            _runtimeInputActions.Disable();
            bool succeeded;
            try
            {
                succeeded = _inputRebindService.ApplyBindingOverrides(bindingOverridesJson);
                LastBindingLoadSucceeded = succeeded;
            }
            finally
            {
                ApplyActionMapState(CurrentContext);
            }

            return succeeded;
        }

        public string SaveBindingOverridesAsJson()
        {
            EnsureInitialized();
            return _inputRebindService.SaveBindingOverridesAsJson();
        }

        private bool ReloadPlayerBindingsInternal()
        {
            GameUserSettingsData settings = _settingSaveService.Load();
            _runtimeInputActions.Disable();

            bool succeeded;
            try
            {
                succeeded = _inputRebindService.ApplyBindingOverrides(
                    settings.InputBindingOverridesJson);
            }
            finally
            {
                if (IsInitialized)
                    ApplyActionMapState(CurrentContext);
            }

            return succeeded;
        }

        private void ResolveActionMapsAndActions()
        {
            _gameplayActionMap = _runtimeInputActions.FindActionMap(
                _gameplayActionMapName, true);
            _uiActionMap = string.IsNullOrWhiteSpace(_uiActionMapName)
                ? null
                : _runtimeInputActions.FindActionMap(_uiActionMapName, false);

            _moveAction = RequireGameplayAction(MoveActionName);
            _jumpAction = RequireGameplayAction(JumpActionName);
            _interactAction = RequireGameplayAction(InteractActionName);
            _fireAction = RequireGameplayAction(FireActionName);
            _aimAction = RequireGameplayAction(AimActionName);
            _reloadAction = RequireGameplayAction(ReloadActionName);
            _sprintAction = RequireGameplayAction(SprintActionName);
            _cameraRotateAction = RequireGameplayAction(CameraRotateActionName);
            _cameraZoomAction = RequireGameplayAction(CameraZoomActionName);
        }

        private InputAction RequireGameplayAction(string actionName)
        {
            return _gameplayActionMap.FindAction(actionName, true);
        }

        private void HandleGameStateChanged(GameStateChangedEvent eventData)
        {
            SetBaseContext(ResolveBaseContext(eventData.CurrentState));
        }

        private static InputContext ResolveBaseContext(GameStateController controller)
        {
            return controller == null
                ? InputContext.Disabled
                : ResolveBaseContext(controller.CurrentState);
        }

        private static InputContext ResolveBaseContext(GameState gameState)
        {
            return gameState == GameState.GamePlaying
                ? InputContext.Gameplay
                : InputContext.Disabled;
        }

        private void ReleaseContext(int requestId)
        {
            if (!IsInitialized)
                return;

            int index = _contextRequests.FindIndex(request => request.Id == requestId);
            if (index < 0)
                return;

            _contextRequests.RemoveAt(index);
            ApplyResolvedContext(false);
        }

        private void ApplyResolvedContext(bool force)
        {
            InputContext resolved = _contextRequests.Count == 0
                ? _baseContext
                : _contextRequests[_contextRequests.Count - 1].Context;

            InputContext previous = CurrentContext;
            if (!force && previous == resolved)
                return;

            CurrentContext = resolved;
            ApplyActionMapState(resolved);
            ApplyCursorState(resolved);

            if (previous != resolved)
                ContextChanged?.Invoke(previous, resolved);
        }

        private void ApplyActionMapState(InputContext context)
        {
            if (_runtimeInputActions == null)
                return;

            _runtimeInputActions.Disable();

            switch (context)
            {
                case InputContext.Gameplay:
                    _gameplayActionMap.Enable();
                    break;
                case InputContext.UI:
                    _uiActionMap?.Enable();
                    break;
                case InputContext.Disabled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(context), context, null);
            }
        }

        private void ApplyCursorState(InputContext context)
        {
            if (!Application.isPlaying)
                return;

            switch (context)
            {
                case InputContext.Gameplay:
                    Cursor.visible = _showCursorInGameplay;
                    Cursor.lockState = _gameplayCursorLockMode;
                    break;
                case InputContext.UI:
                    Cursor.visible = _showCursorInUI;
                    Cursor.lockState = CursorLockMode.None;
                    break;
                case InputContext.Disabled:
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(context), context, null);
            }
        }

        private void ShutdownInternal()
        {
            _gameStateSubscription?.Dispose();
            _gameStateSubscription = null;

            _inputRebindService?.Dispose();
            _inputRebindService = null;
            _settingSaveService = null;

            _runtimeInputActions?.Disable();
            if (_runtimeInputActions != null)
            {
                if (Application.isPlaying)
                    Destroy(_runtimeInputActions);
                else
                    DestroyImmediate(_runtimeInputActions);
            }

            _runtimeInputActions = null;
            _gameplayActionMap = null;
            _uiActionMap = null;
            _moveAction = null;
            _jumpAction = null;
            _interactAction = null;
            _fireAction = null;
            _aimAction = null;
            _reloadAction = null;
            _sprintAction = null;
            _cameraRotateAction = null;
            _cameraZoomAction = null;

            _contextRequests.Clear();
            _nextContextRequestId = 0;
            _baseContext = InputContext.Disabled;
            CurrentContext = InputContext.Disabled;
            LastBindingLoadSucceeded = false;
            IsInitialized = false;
            ContextChanged = null;

            if (Application.isPlaying)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException($"{nameof(InputManager)} 尚未初始化 ");
        }

        private readonly struct ContextRequest
        {
            public readonly int Id;
            public readonly InputContext Context;
            public readonly object Owner;

            public ContextRequest(int id, InputContext context, object owner)
            {
                Id = id;
                Context = context;
                Owner = owner;
            }
        }

        private sealed class ContextLease : IDisposable
        {
            private InputManager _manager;
            private readonly int _requestId;

            public ContextLease(InputManager manager, int requestId)
            {
                _manager = manager;
                _requestId = requestId;
            }

            public void Dispose()
            {
                if (_manager == null)
                    return;

                _manager.ReleaseContext(_requestId);
                _manager = null;
            }
        }
    }
}
