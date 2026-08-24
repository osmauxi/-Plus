using System;
using System.Collections.Generic;
using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Core.Events;
using ProjectGame.HotFix.Gameplay.Events;
using ProjectGame.HotFix.Gameplay.Input;
using ProjectGame.HotFix.Gameplay.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectGame.HotFix.Gameplay.CameraSystem
{
    /// <summary>
    /// GameRuntime 的本地 Gameplay Camera 表现服务。
    ///
    /// 目标由 GameplayCameraTargetRequestedEvent 提供，本类不查询 PlayerManager、
    /// NetworkObject 或具体玩家类型。多个请求按 Priority 和发布时间选择，临时镜头释放后
    /// 会自动恢复默认玩家目标。
    ///
    /// Follow/LookAt 使用独立代理 Pivot：目标位置可硬更新，最终镜头位移由
    /// CinemachineTransposer 的阻尼完成，因此普通移动不是硬跟随；只有生成、传送和
    /// 超大位移才清除 Cinemachine 历史并立即对齐。
    /// </summary>
    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    public sealed class PlayerCameraController : MonoBehaviour, IGameRuntimeService
    {
        [Header("场景引用")]
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private Transform _followPivot;
        [SerializeField] private Transform _lookAtPivot;

        [Header("默认目标偏移")]
        [Tooltip("基于目标根节点的跟随高度。事件请求还可以叠加自己的 FollowOffset。")]
        [SerializeField] private Vector3 _defaultFollowTargetOffset = new(0f, 1.2f, 0f);
        [Tooltip("基于目标根节点的注视高度。事件请求还可以叠加自己的 LookAtOffset。")]
        [SerializeField] private Vector3 _defaultLookAtTargetOffset = new(0f, 1.2f, 0f);

        [Header("构图与软跟随")]
        [Min(0f)] [SerializeField] private float _viewHeight = 10f;
        [Min(0f)] [SerializeField] private float _viewDistance = 12f;
        [Min(0f)] [SerializeField] private float _horizontalDamping = 0.3f;
        [Min(0f)] [SerializeField] private float _verticalDamping = 0.18f;
        [Min(0f)] [SerializeField] private float _aimDamping = 0.15f;
        [Range(0f, 1f)] [SerializeField] private float _screenX = 0.5f;
        [Range(0f, 1f)] [SerializeField] private float _screenY = 0.5f;

        [Header("旋转与缩放")]
        [SerializeField] private float _initialYaw;
        [Min(1f)] [SerializeField] private float _rotationStep = 90f;
        [Min(0f)] [SerializeField] private float _rotationSmoothTime = 0.15f;
        [Min(0f)] [SerializeField] private float _minViewHeight = 4f;
        [Min(0f)] [SerializeField] private float _maxViewHeight = 18f;
        [Min(0f)] [SerializeField] private float _zoomSensitivity = 0.01f;
        [Min(0f)] [SerializeField] private float _zoomSmoothTime = 0.1f;

        [Header("瞬移判定")]
        [Tooltip("目标单帧位移超过该距离时视为传送，清除 Cinemachine 阻尼历史。")]
        [Min(0.1f)] [SerializeField] private float _automaticSnapDistance = 8f;

        [Header("运行时 Inspector 调试")]
        [Tooltip("启用后，在 Game 窗口按 K 会立即应用此组件 Inspector 中的全部摄像机参数。")]
        [SerializeField] private bool _enableInspectorRefreshHotkey = true;

        private readonly List<TargetRequest> _targetRequests = new();
        private IDisposable _targetRequestedSubscription;
        private IDisposable _targetReleasedSubscription;
        private IDisposable _snapRequestedSubscription;

        private CinemachineTransposer _transposer;
        private CinemachineComposer _composer;
        private TargetRequest _activeRequest;
        private long _requestSequence;

        // 用相邻 Camera Target 事实判断传送。目标自身的渲染补帧由 PlayerPresentationDriver 统一完成，
        // Camera 不能再建立第二条独立时间线。
        private Vector3 _lastRawFollowPosition;
        private bool _hasLastRawFollowPosition;

        private float _distanceToHeightRatio;
        private float _targetYaw;
        private float _currentYaw;
        private float _yawVelocity;
        private float _targetViewHeight;
        private float _currentViewHeight;
        private float _heightVelocity;
        private bool _snapRequested;
        // 主动切换到远距离目标且 Snap=false 时，首帧不能被“目标发生传送”的距离兜底误判。
        private bool _suppressAutomaticSnapOnce;

        public bool IsInitialized { get; private set; }
        public Camera WorldCamera => _worldCamera;
        public bool HasTarget => _activeRequest != null && _activeRequest.FollowTarget != null;

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return UniTask.CompletedTask;

            cancellationToken.ThrowIfCancellationRequested();
            ResolveSceneReferences();
            ResolveCinemachinePipeline();

            _distanceToHeightRatio = _viewDistance / Mathf.Max(_viewHeight, 0.01f);
            _targetYaw = _initialYaw;
            _currentYaw = _initialYaw;
            _targetViewHeight = Mathf.Clamp(_viewHeight, _minViewHeight, _maxViewHeight);
            _currentViewHeight = _targetViewHeight;

            _targetRequestedSubscription =
                LocalEvents.Subscribe<GameplayCameraTargetRequestedEvent>(HandleTargetRequested);
            _targetReleasedSubscription =
                LocalEvents.Subscribe<GameplayCameraTargetReleasedEvent>(HandleTargetReleased);
            _snapRequestedSubscription =
                LocalEvents.Subscribe<GameplayCameraSnapRequestedEvent>(HandleSnapRequested);

            ConfigureCinemachine();
            IsInitialized = true;

            // 输入驱动器只观察最终世界相机，不需要认识本控制器或 Cinemachine。
            LocalEvents.Publish(new GameplayWorldCameraChangedEvent(_worldCamera));
            LocalEvents.Publish<GameplayCameraServiceReadyEvent>();
            Debug.Log($"[{nameof(PlayerCameraController)}] 初始化完成。", this);
            return UniTask.CompletedTask;
        }

        private void Update()
        {
            if (!IsInitialized)
                return;

            Keyboard keyboard = Keyboard.current;
            if (_enableInspectorRefreshHotkey &&
                keyboard != null &&
                keyboard.kKey.wasPressedThisFrame)
            {
                RefreshCameraFromInspector();
            }

            InputManager inputManager = InputManager.Instance;
            if (inputManager == null || !inputManager.IsInitialized)
                return;

            float rotateStep = inputManager.CameraRotateStep;
            if (!Mathf.Approximately(rotateStep, 0f))
                _targetYaw += rotateStep * _rotationStep;

            float zoom = inputManager.CameraZoom.y;
            if (!Mathf.Approximately(zoom, 0f))
            {
                _targetViewHeight = Mathf.Clamp(
                    _targetViewHeight - zoom * _zoomSensitivity,
                    _minViewHeight,
                    _maxViewHeight);
            }
        }

        private void LateUpdate()
        {
            if (!IsInitialized)
                return;

            RemoveInvalidRequests();

            if (!HasTarget)
                return;

            Vector3 rawFollowPosition = ResolveFollowPosition(_activeRequest);
            Vector3 rawLookAtPosition = ResolveLookAtPosition(_activeRequest);

            // 瞬移必须比较相邻 Simulation 事实，而不能比较正在插值的 Pivot；
            // 否则正常插值产生的暂时距离也会被误判成传送。
            bool automaticSnap = !_suppressAutomaticSnapOnce &&
                _hasLastRawFollowPosition &&
                (_lastRawFollowPosition - rawFollowPosition).sqrMagnitude >=
                _automaticSnapDistance * _automaticSnapDistance;
            _suppressAutomaticSnapOnce = false;
            _lastRawFollowPosition = rawFollowPosition;
            _hasLastRawFollowPosition = true;

            if (_snapRequested || automaticSnap)
            {
                SnapToTargets(rawFollowPosition, rawLookAtPosition);
                return;
            }

            // Camera Target 已经是逐帧 Render Pose；这里只复制事实位置。
            // 软跟随、构图滞后和镜头手感仍由 Cinemachine 阻尼负责。
            _followPivot.position = rawFollowPosition;
            _lookAtPivot.position = rawLookAtPosition;

            _currentYaw = Mathf.SmoothDampAngle(
                _currentYaw,
                _targetYaw,
                ref _yawVelocity,
                _rotationSmoothTime);
            _currentViewHeight = Mathf.SmoothDamp(
                _currentViewHeight,
                _targetViewHeight,
                ref _heightVelocity,
                _zoomSmoothTime);

            _followPivot.rotation = Quaternion.Euler(0f, _currentYaw, 0f);
            ApplyFollowOffset(_currentViewHeight);
        }

        private void HandleTargetRequested(GameplayCameraTargetRequestedEvent eventData)
        {
            if (eventData.Requester == null || eventData.FollowTarget == null)
                return;

            RemoveRequestsFrom(eventData.Requester);
            _targetRequests.Add(new TargetRequest(eventData, ++_requestSequence));
            ResolveActiveRequest(eventData.Snap);

            // 新实例的 Input Driver 可能晚于 Camera Service 初始化才 Awake；目标绑定时重发一次
            // 世界相机，使对象池创建顺序不影响观察者接线。
            LocalEvents.Publish(new GameplayWorldCameraChangedEvent(_worldCamera));
        }

        private void HandleTargetReleased(GameplayCameraTargetReleasedEvent eventData)
        {
            if (eventData.Requester == null)
                return;

            bool activeRequesterReleased =
                _activeRequest != null && SameRequester(_activeRequest.Requester, eventData.Requester);
            RemoveRequestsFrom(eventData.Requester);

            if (activeRequesterReleased)
                ResolveActiveRequest(snap: true);
        }

        private void HandleSnapRequested()
        {
            if (HasTarget)
                _snapRequested = true;
        }

        private void ResolveActiveRequest(bool snap)
        {
            TargetRequest best = null;

            for (int i = 0; i < _targetRequests.Count; i++)
            {
                TargetRequest candidate = _targetRequests[i];
                if (!candidate.IsValid)
                    continue;

                if (best == null || candidate.Priority > best.Priority ||
                    candidate.Priority == best.Priority && candidate.Sequence > best.Sequence)
                    best = candidate;
            }

            bool hadTarget = _activeRequest != null;
            bool targetChanged = !ReferenceEquals(_activeRequest, best);
            _activeRequest = best;

            if (best == null)
            {
                _virtualCamera.Follow = null;
                _virtualCamera.LookAt = null;
                ResetTargetTracking();
                return;
            }

            _virtualCamera.Follow = _followPivot;
            _virtualCamera.LookAt = _lookAtPivot;
            // 首个有效目标必须建立确定初始画面；已有目标之间切换则尊重请求的 Snap，
            // Snap=false 时由 Cinemachine 阻尼自然过渡到新目标。
            _snapRequested |= snap || (!hadTarget && targetChanged);
            _suppressAutomaticSnapOnce = hadTarget && targetChanged && !snap;
        }

        private void SnapToTargets(Vector3 followPosition, Vector3 lookAtPosition)
        {
            _currentYaw = _targetYaw;
            _currentViewHeight = _targetViewHeight;
            _yawVelocity = 0f;
            _heightVelocity = 0f;

            _followPivot.SetPositionAndRotation(
                followPosition,
                Quaternion.Euler(0f, _currentYaw, 0f));
            _lookAtPivot.position = lookAtPosition;
            ApplyFollowOffset(_currentViewHeight);

            _lastRawFollowPosition = followPosition;
            _hasLastRawFollowPosition = true;

            // 清除上一目标/传送前的 Cinemachine 历史，避免镜头从旧房间缓慢飞回。
            _virtualCamera.PreviousStateIsValid = false;
            _snapRequested = false;
        }

        /// <summary>清除上一目标的采样历史，防止对象池复用或重新绑定时继承旧位置。</summary>
        private void ResetTargetTracking()
        {
            _lastRawFollowPosition = default;
            _hasLastRawFollowPosition = false;
        }

        private void ConfigureCinemachine()
        {
            _virtualCamera.Follow = null;
            _virtualCamera.LookAt = null;

            ApplyCinemachineInspectorValues();
        }

        private void ApplyCinemachineInspectorValues()
        {
            if (_transposer == null || _composer == null)
                return;

            _transposer.m_BindingMode = CinemachineTransposer.BindingMode.LockToTargetNoRoll;
            _transposer.m_XDamping = _horizontalDamping;
            _transposer.m_YDamping = _verticalDamping;
            _transposer.m_ZDamping = _horizontalDamping;

            _composer.m_TrackedObjectOffset = Vector3.zero;
            _composer.m_HorizontalDamping = _aimDamping;
            _composer.m_VerticalDamping = _aimDamping;
            _composer.m_ScreenX = _screenX;
            _composer.m_ScreenY = _screenY;

            ApplyFollowOffset(_currentViewHeight);
        }

        /// <summary>
        /// 运行时把 Inspector 当前值作为一整组快照重新应用。除了 Cinemachine 参数，
        /// 还要重建距离比例、缩放/旋转目标并清除一次历史，否则只改字段看不到确定结果。
        /// </summary>
        [ContextMenu("Debug/刷新 Inspector 摄像机参数")]
        private void RefreshCameraFromInspector()
        {
            if (!IsInitialized || _virtualCamera == null || _followPivot == null || _lookAtPivot == null)
            {
                Debug.LogWarning(
                    $"[{nameof(PlayerCameraController)}] 尚未初始化，无法刷新 Inspector 摄像机参数。",
                    this);
                return;
            }

            _distanceToHeightRatio = _viewDistance / Mathf.Max(_viewHeight, 0.01f);
            _targetYaw = _initialYaw;
            _currentYaw = _initialYaw;
            _yawVelocity = 0f;
            _targetViewHeight = Mathf.Clamp(_viewHeight, _minViewHeight, _maxViewHeight);
            _currentViewHeight = _targetViewHeight;
            _heightVelocity = 0f;

            ApplyCinemachineInspectorValues();
            _followPivot.rotation = Quaternion.Euler(0f, _currentYaw, 0f);

            if (HasTarget)
            {
                SnapToTargets(
                    ResolveFollowPosition(_activeRequest),
                    ResolveLookAtPosition(_activeRequest));
            }
            else
            {
                _virtualCamera.PreviousStateIsValid = false;
            }

            Debug.Log(
                $"[{nameof(PlayerCameraController)}] 已按 Inspector 刷新摄像机：" +
                $"Height={_viewHeight:F2}, Distance={_viewDistance:F2}, " +
                $"Yaw={_initialYaw:F2}, Screen=({_screenX:F2}, {_screenY:F2}), " +
                $"Damping=({_horizontalDamping:F2}, {_verticalDamping:F2}, {_aimDamping:F2})。",
                this);
        }

        private void ApplyFollowOffset(float height)
        {
            float distance = height * _distanceToHeightRatio;
            _transposer.m_FollowOffset = new Vector3(0f, height, -distance);
        }

        private void ResolveSceneReferences()
        {
            if (_worldCamera == null)
                _worldCamera = Camera.main;

            if (_worldCamera == null)
                throw new InvalidOperationException($"{nameof(PlayerCameraController)} 找不到 Main Camera。");

            if (_virtualCamera == null)
                _virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>(true);

            if (_virtualCamera == null)
                throw new InvalidOperationException($"{nameof(PlayerCameraController)} 未绑定 Virtual Camera。");

            if (_followPivot == null)
                _followPivot = CreatePivot("FollowPivot");

            if (_lookAtPivot == null)
                _lookAtPivot = CreatePivot("LookAtPivot");
        }

        private void ResolveCinemachinePipeline()
        {
            if (_worldCamera.GetComponent<CinemachineBrain>() == null)
                throw new InvalidOperationException("Main Camera 缺少 CinemachineBrain。");

            _transposer = _virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (_transposer == null)
                _transposer = _virtualCamera.AddCinemachineComponent<CinemachineTransposer>();

            _composer = _virtualCamera.GetCinemachineComponent<CinemachineComposer>();
            if (_composer == null)
                _composer = _virtualCamera.AddCinemachineComponent<CinemachineComposer>();
        }

        private Transform CreatePivot(string pivotName)
        {
            GameObject pivot = new(pivotName);
            pivot.transform.SetParent(transform, false);
            return pivot.transform;
        }

        private Vector3 ResolveFollowPosition(TargetRequest request)
        {
            return request.FollowTarget.position + _defaultFollowTargetOffset + request.FollowOffset;
        }

        private Vector3 ResolveLookAtPosition(TargetRequest request)
        {
            Transform lookAt = request.LookAtTarget != null ? request.LookAtTarget : request.FollowTarget;
            return lookAt.position + _defaultLookAtTargetOffset + request.LookAtOffset;
        }

        private void RemoveInvalidRequests()
        {
            bool removedActive = false;

            for (int i = _targetRequests.Count - 1; i >= 0; i--)
            {
                TargetRequest request = _targetRequests[i];
                if (request.IsValid)
                    continue;

                removedActive |= ReferenceEquals(request, _activeRequest);
                _targetRequests.RemoveAt(i);
            }

            if (removedActive)
                ResolveActiveRequest(snap: true);
        }

        private void RemoveRequestsFrom(object requester)
        {
            for (int i = _targetRequests.Count - 1; i >= 0; i--)
            {
                if (SameRequester(_targetRequests[i].Requester, requester))
                    _targetRequests.RemoveAt(i);
            }
        }

        private static bool SameRequester(object left, object right)
        {
            return ReferenceEquals(left, right) || Equals(left, right);
        }

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            DisposeSubscriptions();
            _targetRequests.Clear();
            _activeRequest = null;
            ResetTargetTracking();

            if (_virtualCamera != null)
            {
                _virtualCamera.Follow = null;
                _virtualCamera.LookAt = null;
            }

            if (IsInitialized)
                LocalEvents.Publish(new GameplayWorldCameraChangedEvent(null));

            IsInitialized = false;
            return UniTask.CompletedTask;
        }

        private void DisposeSubscriptions()
        {
            _targetRequestedSubscription?.Dispose();
            _targetReleasedSubscription?.Dispose();
            _snapRequestedSubscription?.Dispose();
            _targetRequestedSubscription = null;
            _targetReleasedSubscription = null;
            _snapRequestedSubscription = null;
        }

        private void OnDestroy()
        {
            DisposeSubscriptions();
        }

        private sealed class TargetRequest
        {
            public readonly object Requester;
            public readonly Transform FollowTarget;
            public readonly Transform LookAtTarget;
            public readonly Vector3 FollowOffset;
            public readonly Vector3 LookAtOffset;
            public readonly int Priority;
            public readonly long Sequence;

            public bool IsValid =>
                IsRequesterAlive(Requester) && FollowTarget != null && LookAtTarget != null;

            public TargetRequest(GameplayCameraTargetRequestedEvent request, long sequence)
            {
                Requester = request.Requester;
                FollowTarget = request.FollowTarget;
                LookAtTarget = request.LookAtTarget;
                FollowOffset = request.FollowOffset;
                LookAtOffset = request.LookAtOffset;
                Priority = request.Priority;
                Sequence = sequence;
            }

            private static bool IsRequesterAlive(object requester)
            {
                if (requester == null)
                    return false;

                return requester is not UnityEngine.Object unityObject || unityObject != null;
            }
        }

    }
}
