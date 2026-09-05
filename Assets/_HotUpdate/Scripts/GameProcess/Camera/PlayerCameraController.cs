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
    /// GameRuntime 的本地 Gameplay Camera 表现服务 
    ///
    /// 目标由GameplayCameraTargetRequestedEvent提供，本类不查询具体玩家类型 
    /// 多个请求按 Priority 和发布时间选择，临时镜头释放后会自动恢复默认玩家目标 
    ///
    /// Follow/LookAt 使用独立代理 Pivot：目标位置可硬更新，最终镜头位移由
    /// CinemachineTransposer 的阻尼完成，因此普通移动不是硬跟随；只有生成、传送和
    /// 超大位移才清除 Cinemachine 历史并立即对齐 
    ///
    /// 镜头自身的 Yaw / Zoom / Effect Modifier 等运动状态由 CameraMotionModel 负责计算，
    /// 本类只负责 Unity 生命周期、目标仲裁、输入翻译以及将最终结果应用到 Cinemachine 
    /// </summary>
    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    public sealed class PlayerCameraController : MonoBehaviour, IGameRuntimeService
    {
        #region Inspector Configuration

        [Header("场景引用")]
        [Tooltip("输出最终画面的世界相机，必须带有 CinemachineBrain 为空时会尝试使用 Main Camera ")]
        [SerializeField] private Camera _worldCamera;
        [Tooltip("本控制器驱动的 Cinemachine 虚拟相机 为空时会在子节点中查找 ")]
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [Tooltip("承载 Follow 位置与 Yaw 的代理节点；为空时运行时自动创建 ")]
        [SerializeField] private Transform _followPivot;
        [Tooltip("承载 LookAt 位置的代理节点；为空时运行时自动创建 ")]
        [SerializeField] private Transform _lookAtPivot;

        [Header("默认目标偏移")]
        [Tooltip("基于目标根节点的跟随高度 事件请求还可以叠加自己的 FollowOffset ")]
        [SerializeField] private Vector3 _defaultFollowTargetOffset = new(0f, 1.2f, 0f);
        [Tooltip("基于目标根节点的注视高度 事件请求还可以叠加自己的 LookAtOffset ")]
        [SerializeField] private Vector3 _defaultLookAtTargetOffset = new(0f, 1.2f, 0f);

        [Header("构图与软跟随")]
        [Tooltip("基础观察高度；实际高度还会叠加玩家缩放和持续/瞬时效果 ")]
        [Min(0f)][SerializeField] private float _viewHeight = 10f;
        [Tooltip("基础后退距离，与观察高度共同确定固定的俯视比例 ")]
        [Min(0f)][SerializeField] private float _viewDistance = 12f;
        [Tooltip("Cinemachine 在水平面跟随目标时的阻尼时间 ")]
        [Min(0f)][SerializeField] private float _horizontalDamping = 0.3f;
        [Tooltip("Cinemachine 在竖直方向跟随目标时的阻尼时间 ")]
        [Min(0f)][SerializeField] private float _verticalDamping = 0.18f;
        [Tooltip("Cinemachine 注视目标时的水平和竖直阻尼时间 ")]
        [Min(0f)][SerializeField] private float _aimDamping = 0.15f;
        [Tooltip("目标在画面中的水平构图位置，0 为左侧，1 为右侧 ")]
        [Range(0f, 1f)][SerializeField] private float _screenX = 0.5f;
        [Tooltip("目标在画面中的竖直构图位置，0 为底部，1 为顶部 ")]
        [Range(0f, 1f)][SerializeField] private float _screenY = 0.5f;

        [Header("旋转与缩放")]
        [Tooltip("初始化时的世界 Yaw 角度 ")]
        [SerializeField] private float _initialYaw;
        [Tooltip("每次离散旋转输入改变的 Yaw 角度 ")]
        [Min(1f)][SerializeField] private float _rotationStep = 90f;
        [Tooltip("Yaw 到达目标角度的平滑时间 ")]
        [Min(0f)][SerializeField] private float _rotationSmoothTime = 0.15f;
        [Tooltip("玩家缩放与镜头效果叠加后的最小观察高度 ")]
        [Min(0f)][SerializeField] private float _minViewHeight = 4f;
        [Tooltip("玩家缩放与镜头效果叠加后的最大观察高度 ")]
        [Min(0f)][SerializeField] private float _maxViewHeight = 18f;
        [Tooltip("滚轮输入转换为观察高度变化的系数 ")]
        [Min(0f)][SerializeField] private float _zoomSensitivity = 0.01f;
        [Tooltip("观察高度到达目标值的平滑时间 ")]
        [Min(0f)][SerializeField] private float _zoomSmoothTime = 0.1f;

        [Header("FOV")]
        [Tooltip("不含效果修饰器时的基础视野角 ")]
        [SerializeField] private float _baseFov = 60f;
        [Tooltip("所有 FOV 层叠加后的最小视野角 ")]
        [Min(1f)][SerializeField] private float _minFov = 35f;
        [Tooltip("所有 FOV 层叠加后的最大视野角 ")]
        [Min(1f)][SerializeField] private float _maxFov = 80f;
        [Tooltip("FOV 到达目标值的平滑时间 ")]
        [Min(0f)][SerializeField] private float _fovSmoothTime = 0.1f;

        [Header("瞬移判定")]
        [Tooltip("目标单帧位移超过该距离时视为传送，清除 Cinemachine 阻尼历史 ")]
        [Min(0.1f)][SerializeField] private float _automaticSnapDistance = 8f;

        [Header("运行时 Inspector 调试")]
        [Tooltip("启用后，在 Game 窗口按 K 会立即应用此组件 Inspector 中的全部摄像机参数 ")]
        [SerializeField] private bool _enableInspectorRefreshHotkey = true;

        [Header("Aim 构图")]
        [Tooltip("瞄准时镜头朝鼠标世界点方向移动的最大距离 ")]
        [Min(0f)][SerializeField] private float _maxAimLookAhead = 3f;

        [Tooltip("鼠标世界点距离角色小于该值时不产生镜头偏移 ")]
        [Min(0f)][SerializeField] private float _aimLookAheadDeadZone = 1f;

        [Tooltip("鼠标距离达到该值时获得完整的 MaxAimLookAhead ")]
        [Min(0.1f)][SerializeField] private float _fullAimLookAheadDistance = 8f;

        [Tooltip("瞄准前视偏移建立时的平滑时间 ")]
        [Min(0f)][SerializeField] private float _aimLookAheadSmoothTime = 0.12f;

        [Tooltip("退出瞄准后镜头回到玩家中心的时间 ")]
        [Min(0f)][SerializeField] private float _aimLookAheadReturnTime = 0.18f;

        [Header("Movement 构图")]
        [Tooltip("高速移动时镜头沿移动方向最大的前视距离 ")]
        [Min(0f)][SerializeField] private float _maxMovementLookAhead = 1.5f;

        [Tooltip("低于该视觉速度时不产生 Movement LookAhead，用于过滤站立抖动和微小插值 ")]
        [Min(0f)][SerializeField] private float _movementLookAheadDeadZoneSpeed = 0.5f;

        [Tooltip("达到该视觉速度时获得完整的 MaxMovementLookAhead ")]
        [Min(0.1f)][SerializeField] private float _fullMovementLookAheadSpeed = 7f;

        [Tooltip("镜头向移动方向建立前视偏移的速度 ")]
        [Min(0f)][SerializeField] private float _movementLookAheadSmoothTime = 0.18f;

        [Tooltip("停止移动后镜头回到角色中心的速度 ")]
        [Min(0f)][SerializeField] private float _movementLookAheadReturnTime = 0.25f;

        [Tooltip("Aim 时保留多少 Movement LookAhead 0 表示完全由 Aim 接管构图 ")]
        [Range(0f, 1f)][SerializeField] private float _aimMovementLookAheadWeight = 0f;

        [Header("Movement动态响应")]

        [Tooltip("用于求视觉加速度前，对 Render Pose 速度进行轻微平滑，避免帧间噪声被差分放大。")]
        [Min(0f)]
        [SerializeField] private float _movementVelocitySmoothTime = 0.04f;

        [Tooltip("加速、急停、反向等运动状态剧变时额外产生的最大前视偏移。")]
        [Min(0f)]
        [SerializeField] private float _maxAccelerationLookAhead = 0.6f;

        [Tooltip("低于该加速度时不产生 Acceleration LookAhead，用于过滤微小速度波动。")]
        [Min(0f)]
        [SerializeField] private float _accelerationLookAheadDeadZone = 2f;

        [Tooltip("达到该加速度后产生完整的 MaxAccelerationLookAhead。")]
        [Min(0.1f)]
        [SerializeField] private float _fullAccelerationLookAhead = 15f;

        [Tooltip("Acceleration Offset 建立的速度。")]
        [Min(0f)]
        [SerializeField] private float _accelerationLookAheadSmoothTime = 0.06f;

        [Tooltip("加速度消失后额外 Offset 回落的速度。")]
        [Min(0f)]
        [SerializeField] private float _accelerationLookAheadReturnTime = 0.12f;

        [Tooltip("Movement LookAhead 改变方向时围绕角色 Pivot 旋转的最大角速度（度/秒）。")]
        [Min(0f)]
        [SerializeField] private float _movementLookAheadTurnSpeed = 360f;
        #endregion

        #region Runtime State

        private readonly CameraCompositionModel _composition = new();

        private Vector3 _aimWorldPosition;
        private bool _hasAimWorldPosition;
        private IDisposable _aimTargetUpdatedSubscription;

        private readonly List<TargetRequest> _targetRequests = new();

        private IDisposable _targetRequestedSubscription;
        private IDisposable _targetReleasedSubscription;
        private IDisposable _snapRequestedSubscription;

        private CinemachineTransposer _transposer;
        private CinemachineComposer _composer;

        private TargetRequest _activeRequest;
        private long _requestSequence;

        // 用相邻 Camera Target 事实判断传送 目标自身的渲染补帧由 PlayerPresentationDriver 统一完成，
        // Camera 不能再建立第二条独立时间线 
        private Vector3 _lastRawFollowPosition;
        private bool _hasLastRawFollowPosition;

        // Yaw / Zoom / Modifier / Kick 等镜头自身运动状态全部由纯计算模块统一维护 
        private readonly CameraMotionModel _motion = new();

        private float _distanceToHeightRatio;
        private bool _snapRequested;

        // 主动切换到远距离目标且 Snap=false 时，首帧不能被“目标发生传送”的距离兜底误判 
        private bool _suppressAutomaticSnapOnce;

        #endregion

        #region Public State

        /// <summary>服务是否已完成引用解析、模型初始化和事件订阅 </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>当前用于输出 Gameplay 画面的世界相机 </summary>
        public Camera WorldCamera => _worldCamera;

        /// <summary>当前是否存在一个仍然有效的 Gameplay 跟随目标 </summary>
        public bool HasTarget => _activeRequest != null && _activeRequest.FollowTarget != null;

        #endregion

        #region Service Lifecycle and Frame Loop

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return UniTask.CompletedTask;

            cancellationToken.ThrowIfCancellationRequested();

            ResolveSceneReferences();
            ResolveCinemachinePipeline();

            _distanceToHeightRatio = _viewDistance / Mathf.Max(_viewHeight, 0.01f);

            // Controller 只向 Motion 提供配置，不再直接维护平滑过程中的内部状态 
            _motion.Reset(
                _initialYaw,
                _viewHeight,
                _rotationSmoothTime,
                _minViewHeight,
                _maxViewHeight,
                _zoomSmoothTime,
                _baseFov,
                _minFov,
                _maxFov,
                _fovSmoothTime);

            _composition.Reset(
                _maxAimLookAhead,
                _aimLookAheadDeadZone,
                _fullAimLookAheadDistance,
                _aimLookAheadSmoothTime,
                _aimLookAheadReturnTime,

                _maxMovementLookAhead,
                _movementLookAheadDeadZoneSpeed,
                _fullMovementLookAheadSpeed,
                _movementLookAheadSmoothTime,
                _movementLookAheadReturnTime,

                _movementVelocitySmoothTime,

                _maxAccelerationLookAhead,
                _accelerationLookAheadDeadZone,
                _fullAccelerationLookAhead,
                _accelerationLookAheadSmoothTime,
                _accelerationLookAheadReturnTime,

                _movementLookAheadTurnSpeed,
                _aimMovementLookAheadWeight);

            _aimTargetUpdatedSubscription =
                LocalEvents.Subscribe<CameraAimTargetUpdatedEvent>(HandleAimTargetUpdated);

            _targetRequestedSubscription =
                LocalEvents.Subscribe<GameplayCameraTargetRequestedEvent>(HandleTargetRequested);

            _targetReleasedSubscription =
                LocalEvents.Subscribe<GameplayCameraTargetReleasedEvent>(HandleTargetReleased);

            _snapRequestedSubscription =
                LocalEvents.Subscribe<GameplayCameraSnapRequestedEvent>(HandleSnapRequested);

            ConfigureCinemachine();

            IsInitialized = true;

            // 输入驱动器只观察最终世界相机，不需要认识本控制器或 Cinemachine 
            LocalEvents.Publish(new GameplayWorldCameraChangedEvent(_worldCamera));
            LocalEvents.Publish<GameplayCameraServiceReadyEvent>();

            Debug.Log($"[{nameof(PlayerCameraController)}] 初始化完成 ", this);

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
            {
                // Controller 负责把输入翻译成 Camera Command，
                // 具体 TargetYaw 如何维护、如何平滑由 Motion 自己决定 
                _motion.AddYaw(rotateStep * _rotationStep);
            }

            float zoom = inputManager.CameraZoom.y;

            if (!Mathf.Approximately(zoom, 0f))
            {
                // Base Zoom 只代表玩家主动调整的基础观察距离，
                // Aim / Explosion 等效果会在 Motion 内额外叠加 
                _motion.AddBaseZoom(-zoom * _zoomSensitivity);
            }
        }

        private void LateUpdate()
        {
            if (!IsInitialized)
                return;

            RemoveInvalidRequests();

            // Motion 生命周期不依赖当前是否存在 Camera Target 
            // 即使暂时失去目标，ZoomKick 等一次性状态也应该继续正常结束 
            _motion.Tick(Time.deltaTime);

            if (!HasTarget)
                return;

            Vector3 rawFollowPosition = ResolveFollowPosition(_activeRequest);
            Vector3 rawLookAtPosition = ResolveLookAtPosition(_activeRequest);

            // 瞬移只比较原始 Camera Target；Composition Offset 不能参与判断 
            bool automaticSnap =
                !_suppressAutomaticSnapOnce &&
                _hasLastRawFollowPosition &&
                (_lastRawFollowPosition - rawFollowPosition).sqrMagnitude >=
                _automaticSnapDistance * _automaticSnapDistance;

            _suppressAutomaticSnapOnce = false;

            if (_snapRequested || automaticSnap)
            {
                SnapToTargets(rawFollowPosition, rawLookAtPosition);
                return;
            }

            _lastRawFollowPosition = rawFollowPosition;
            _hasLastRawFollowPosition = true;

            // Movement LookAhead 使用 Target 的 Render Pose 推导视觉移动方向和速度 
            // 此时已经排除了 Teleport，因此大位移不会污染移动构图 
            _composition.UpdateMovementTarget(rawFollowPosition, Time.deltaTime);

            if (_composition.AimActive && _hasAimWorldPosition)
                _composition.UpdateAimTarget(rawFollowPosition, _aimWorldPosition);

            _composition.Tick(Time.deltaTime);

            Vector3 compositionOffset = _composition.CurrentOffset;

            // Follow 与 LookAt 同步偏移，本质是平移整套构图，而不是改变原有观察角度 
            _followPivot.position = rawFollowPosition + compositionOffset;
            _lookAtPivot.position = rawLookAtPosition + compositionOffset;

            _followPivot.rotation =
                Quaternion.Euler(0f, _motion.CurrentYaw, 0f);

            ApplyFollowOffset(_motion.CurrentViewHeight);
            ApplyFov(_motion.CurrentFov);
        }

        #endregion

        #region External Effect API

        /// <summary>设置或移除一个按效果标识区分的持续缩放修饰器 </summary>
        public void SetCameraZoomModifier(CameraEffectId id,float offset,float smoothTime)
        {
            _motion.SetZoomModifier(id, offset, smoothTime);
        }

        /// <summary>播放一次先建立、后释放的观察高度冲击 </summary>
        public void PlayCameraZoomKick(float offset,float attackSmoothTime,float releaseSmoothTime)
        {
            _motion.PlayZoomKick(offset, attackSmoothTime, releaseSmoothTime);
        }

        /// <summary>设置或移除一个按效果标识区分的持续 FOV 修饰器 </summary>
        public void SetCameraFovModifier(CameraEffectId id, float offset, float smoothTime)
        {
            _motion.SetFovModifier(id, offset, smoothTime);
        }

        /// <summary>播放一次先建立、后释放的 FOV 冲击 </summary>
        public void PlayCameraFovKick(float offset, float attackSmoothTime, float releaseSmoothTime)
        {
            _motion.PlayFovKick(offset, attackSmoothTime, releaseSmoothTime);
        }

        /// <summary>开启或关闭 Aim LookAhead 构图 </summary>
        public void SetAimComposition(bool active)
        {
            _composition.SetAimActive(active);

            if (!active)
                _hasAimWorldPosition = false;
        }

        #endregion

        #region Camera Event Handlers

        private void HandleTargetRequested(GameplayCameraTargetRequestedEvent eventData)
        {
            if (eventData.Requester == null || eventData.FollowTarget == null)
                return;

            RemoveRequestsFrom(eventData.Requester);

            _targetRequests.Add(new TargetRequest(eventData, ++_requestSequence));

            ResolveActiveRequest(eventData.Snap);

            // 新实例的 Input Driver 可能晚于 Camera Service 初始化才 Awake；目标绑定时重发一次
            // 世界相机，使对象池创建顺序不影响观察者接线 
            LocalEvents.Publish(new GameplayWorldCameraChangedEvent(_worldCamera));
        }

        private void HandleTargetReleased(GameplayCameraTargetReleasedEvent eventData)
        {
            if (eventData.Requester == null)
                return;

            bool activeRequesterReleased =
                _activeRequest != null &&
                SameRequester(_activeRequest.Requester, eventData.Requester);

            RemoveRequestsFrom(eventData.Requester);

            if (activeRequesterReleased)
                ResolveActiveRequest(snap: true);
        }

        private void HandleSnapRequested()
        {
            if (HasTarget)
                _snapRequested = true;
        }
        private void HandleAimTargetUpdated(CameraAimTargetUpdatedEvent eventData)
        {
            _aimWorldPosition = eventData.WorldPosition;
            _hasAimWorldPosition = true;
        }

        #endregion

        #region Target Arbitration and Snapping

        private void ResolveActiveRequest(bool snap)
        {
            TargetRequest best = null;

            for (int i = 0; i < _targetRequests.Count; i++)
            {
                TargetRequest candidate = _targetRequests[i];

                if (!candidate.IsValid)
                    continue;

                if (best == null ||
                    candidate.Priority > best.Priority ||
                    candidate.Priority == best.Priority && candidate.Sequence > best.Sequence)
                {
                    best = candidate;
                }
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

            if (targetChanged)
                _composition.ResetMovementTracking();

            // 首个有效目标必须建立确定初始画面；已有目标之间切换则尊重请求的 Snap，
            // Snap=false 时由 Cinemachine 阻尼自然过渡到新目标 
            _snapRequested |= snap || (!hadTarget && targetChanged);

            _suppressAutomaticSnapOnce =
                hadTarget &&
                targetChanged &&
                !snap;
        }

        private void SnapToTargets(Vector3 followPosition, Vector3 lookAtPosition)
        {
            _motion.Snap();

            // Teleport / Target Snap 重新建立 Movement 采样基准，
            // 防止大位移被下一帧误认为高速移动 
            _composition.Snap(followPosition);

            Vector3 compositionOffset = _composition.CurrentOffset;

            _followPivot.SetPositionAndRotation(
                followPosition + compositionOffset,
                Quaternion.Euler(0f, _motion.CurrentYaw, 0f));

            _lookAtPivot.position =
                lookAtPosition + compositionOffset;

            ApplyFollowOffset(_motion.CurrentViewHeight);
            ApplyFov(_motion.CurrentFov);

            _lastRawFollowPosition = followPosition;
            _hasLastRawFollowPosition = true;

            _virtualCamera.PreviousStateIsValid = false;
            _snapRequested = false;
        }

        /// <summary>清除上一目标的采样历史，防止对象池复用或重新绑定时继承旧位置 </summary>
        private void ResetTargetTracking()
        {
            _lastRawFollowPosition = default;
            _hasLastRawFollowPosition = false;

            // 新 Target 不能继承上一 Target 的视觉速度采样 
            _composition.ResetMovementTracking();
        }

        private void RemoveInvalidRequests()
        {
            bool removedActive = false;

            for (int i = _targetRequests.Count - 1; i >= 0; i--)
            {
                TargetRequest request = _targetRequests[i];

                if (request.IsValid)
                    continue;

                removedActive |=
                    ReferenceEquals(request, _activeRequest);

                _targetRequests.RemoveAt(i);
            }

            if (removedActive)
                ResolveActiveRequest(snap: true);
        }

        private void RemoveRequestsFrom(object requester)
        {
            for (int i = _targetRequests.Count - 1; i >= 0; i--)
            {
                if (SameRequester(
                    _targetRequests[i].Requester,
                    requester))
                {
                    _targetRequests.RemoveAt(i);
                }
            }
        }

        private static bool SameRequester(object left, object right)
        {
            return ReferenceEquals(left, right) ||
                Equals(left, right);
        }

        #endregion

        #region Cinemachine Setup and Application

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

            _transposer.m_BindingMode =
                CinemachineTransposer.BindingMode.LockToTargetNoRoll;

            _transposer.m_XDamping = _horizontalDamping;
            _transposer.m_YDamping = _verticalDamping;
            _transposer.m_ZDamping = _horizontalDamping;

            _composer.m_TrackedObjectOffset = Vector3.zero;
            _composer.m_HorizontalDamping = _aimDamping;
            _composer.m_VerticalDamping = _aimDamping;
            _composer.m_ScreenX = _screenX;
            _composer.m_ScreenY = _screenY;

            ApplyFollowOffset(_motion.CurrentViewHeight);
        }

        private void ApplyFollowOffset(float height)
        {
            float distance = height * _distanceToHeightRatio;

            _transposer.m_FollowOffset =
                new Vector3(0f, height, -distance);
        }
        /// <summary>Controller 只负责把 Motion 计算出的最终 Lens 状态应用到 Cinemachine </summary>
        private void ApplyFov(float fov)
        {
            _virtualCamera.m_Lens.FieldOfView = fov;
        }
        private void ResolveSceneReferences()
        {
            if (_worldCamera == null)
                _worldCamera = Camera.main;

            if (_worldCamera == null)
                throw new InvalidOperationException(
                    $"{nameof(PlayerCameraController)} 找不到 Main Camera ");

            if (_virtualCamera == null)
                _virtualCamera =
                    GetComponentInChildren<CinemachineVirtualCamera>(true);

            if (_virtualCamera == null)
                throw new InvalidOperationException(
                    $"{nameof(PlayerCameraController)} 未绑定 Virtual Camera ");

            if (_followPivot == null)
                _followPivot = CreatePivot("FollowPivot");

            if (_lookAtPivot == null)
                _lookAtPivot = CreatePivot("LookAtPivot");
        }

        private void ResolveCinemachinePipeline()
        {
            if (_worldCamera.GetComponent<CinemachineBrain>() == null)
            {
                throw new InvalidOperationException(
                    "Main Camera 缺少 CinemachineBrain ");
            }

            _transposer =
                _virtualCamera.GetCinemachineComponent<CinemachineTransposer>();

            if (_transposer == null)
            {
                _transposer =
                    _virtualCamera.AddCinemachineComponent<CinemachineTransposer>();
            }

            _composer =
                _virtualCamera.GetCinemachineComponent<CinemachineComposer>();

            if (_composer == null)
            {
                _composer =
                    _virtualCamera.AddCinemachineComponent<CinemachineComposer>();
            }
        }

        private Transform CreatePivot(string pivotName)
        {
            GameObject pivot = new(pivotName);

            pivot.transform.SetParent(transform, false);

            return pivot.transform;
        }

        #endregion

        #region Runtime Inspector Refresh

        /// <summary>
        /// 把 Inspector 当前值作为一组运行时配置重新应用，并保留仍然生效的持续效果 
        /// </summary>
        [ContextMenu("Debug/刷新 Inspector 摄像机参数")]
        private void RefreshCameraFromInspector()
        {
            if (!IsInitialized ||
                _virtualCamera == null ||
                _followPivot == null ||
                _lookAtPivot == null)
            {
                Debug.LogWarning(
                    $"[{nameof(PlayerCameraController)}] 尚未初始化，无法刷新 Inspector 摄像机参数 ",
                    this);

                return;
            }

            _distanceToHeightRatio =
                _viewDistance / Mathf.Max(_viewHeight, 0.01f);

            _motion.Reset(
                _initialYaw,
                _viewHeight,
                _rotationSmoothTime,
                _minViewHeight,
                _maxViewHeight,
                _zoomSmoothTime,
                _baseFov,
                _minFov,
                _maxFov,
                _fovSmoothTime,
                clearPersistentModifiers: false);

            bool aimCompositionActive = _composition.AimActive;

            _composition.Reset(
                _maxAimLookAhead,
                _aimLookAheadDeadZone,
                _fullAimLookAheadDistance,
                _aimLookAheadSmoothTime,
                _aimLookAheadReturnTime,

                _maxMovementLookAhead,
                _movementLookAheadDeadZoneSpeed,
                _fullMovementLookAheadSpeed,
                _movementLookAheadSmoothTime,
                _movementLookAheadReturnTime,

                _movementVelocitySmoothTime,

                _maxAccelerationLookAhead,
                _accelerationLookAheadDeadZone,
                _fullAccelerationLookAhead,
                _accelerationLookAheadSmoothTime,
                _accelerationLookAheadReturnTime,

                _movementLookAheadTurnSpeed,
                _aimMovementLookAheadWeight);

            _composition.SetAimActive(aimCompositionActive);

            ApplyCinemachineInspectorValues();

            _followPivot.rotation =
                Quaternion.Euler(0f, _motion.CurrentYaw, 0f);

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
                $"Damping=({_horizontalDamping:F2}, {_verticalDamping:F2}, {_aimDamping:F2}) ",
                this);
        }

        #endregion

        #region Target Position Resolution

        private Vector3 ResolveFollowPosition(TargetRequest request)
        {
            return request.FollowTarget.position +
                _defaultFollowTargetOffset +
                request.FollowOffset;
        }

        private Vector3 ResolveLookAtPosition(TargetRequest request)
        {
            Transform lookAt =
                request.LookAtTarget != null
                    ? request.LookAtTarget
                    : request.FollowTarget;

            return lookAt.position +
                _defaultLookAtTargetOffset +
                request.LookAtOffset;
        }

        #endregion

        #region Service Shutdown

        public UniTask ShutdownAsync(
            CancellationToken cancellationToken)
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
            {
                LocalEvents.Publish(
                    new GameplayWorldCameraChangedEvent(null));
            }

            IsInitialized = false;

            return UniTask.CompletedTask;
        }

        private void DisposeSubscriptions()
        {
            _targetRequestedSubscription?.Dispose();
            _targetReleasedSubscription?.Dispose();
            _snapRequestedSubscription?.Dispose();
            _aimTargetUpdatedSubscription?.Dispose();

            _aimTargetUpdatedSubscription = null;
            _targetRequestedSubscription = null;
            _targetReleasedSubscription = null;
            _snapRequestedSubscription = null;
        }

        private void OnDestroy()
        {
            DisposeSubscriptions();
        }

        #endregion

        #region Nested Types

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
                IsRequesterAlive(Requester) &&
                FollowTarget != null &&
                LookAtTarget != null;

            public TargetRequest(
                GameplayCameraTargetRequestedEvent request,
                long sequence)
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

                return requester is not UnityEngine.Object unityObject ||
                    unityObject != null;
            }
        }

        #endregion
    }
}
