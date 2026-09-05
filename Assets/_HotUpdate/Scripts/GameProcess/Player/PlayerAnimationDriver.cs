using ProjectGame.HotFix.Gameplay.Player.Movement;
using ProjectGame.HotFix.Gameplay.Player.State;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectGame.HotFix.Gameplay.Player
{
    /// <summary>
    /// 消费 PlayerSyncController 的最终表现时间轴并驱动动态加载角色的 Animator 
    /// Owner 读取预测结果，Remote 读取插值结果；本类不需要区分网络身份 
    /// 这里只做表现映射：不能改变 Simulation、判定射击是否命中，或用动画播放进度结束换弹/受击 
    /// 模型由 PlayerAppearanceController 异步创建，因此驱动会持续检测 Animator 是否发生替换 
    /// </summary>
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerSyncController))]
    [RequireComponent(typeof(PlayerAppearanceController))]
    public sealed class PlayerAnimationDriver : MonoBehaviour
    {
        // 参数名在类型加载时只哈希一次，避免 LateUpdate 每帧做字符串查找 
        // 名称必须与 Player_Gameplay.controller 保持一致；重命名 Animator 参数时需同步修改这里 
        private static readonly int VelocityXHash = Animator.StringToHash("VelocityX");
        private static readonly int VelocityZHash = Animator.StringToHash("VelocityZ");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int HasMoveInputHash = Animator.StringToHash("HasMoveInput");
        private static readonly int LocomotionModeHash = Animator.StringToHash("LocomotionMode");
        private static readonly int IsDeadHash = Animator.StringToHash("IsDead");
        private static readonly int IsHitReactingHash = Animator.StringToHash("IsHitReacting");
        private static readonly int IsReloadingHash = Animator.StringToHash("IsReloading");
        private static readonly int IsPivotingHash = Animator.StringToHash("IsPivoting");
        private static readonly int ShootHash = Animator.StringToHash("Shoot");
        private static readonly int FireStateHash = Animator.StringToHash("Fire");

        [Header("参数阻尼")]
        [Tooltip("VelocityX/Z 追随模拟结果的表现阻尼秒数；只影响动画平滑，不影响角色真实速度 ")]
        [SerializeField, Min(0f)] private float _locomotionDampTime = 0.1f;
        [Tooltip("Speed 在各移动模式内部的 Idle、Walk/Jog 子树之间切换时的阻尼秒数 ")]
        [SerializeField, Min(0f)] private float _speedDampTime = 0.12f;
        [Tooltip("松开移动输入时强制回到的 Base Layer 状态路径 不同角色 Controller 可使用同一状态契约而不共享动画 ")]
        [SerializeField] private string _baseIdleStateName = "Base FullBody.Idle";
        [Tooltip("松开移动输入后回 Idle 的固定淡入时间 ")]
        [SerializeField, Min(0f)] private float _stopBlendDuration = 0.12f;

        [Header("上半身瞄准层")]
        [Tooltip("仅在瞄准时启用的 Generic 上半身覆盖层；负责瞄准姿势、射击和换弹 ")]
        [FormerlySerializedAs("_actionLayerName")]
        [SerializeField] private string _upperBodyLayerName = "UpperBody Aim";
        [Tooltip("瞄准上半身层淡入淡出的每秒权重变化速度 ")]
        [FormerlySerializedAs("_actionLayerBlendSpeed")]
        [SerializeField, Min(0f)] private float _upperBodyLayerBlendSpeed = 12f;

        [Header("上半身瞄准")]
        [Tooltip("躯干允许承受的解剖学安全角度；实际角度还会受 Simulation 的身体转向开始角度限制 ")]
        [SerializeField, Range(0f, 90f)] private float _upperBodyAimMaxYaw = 65f;
        [Tooltip("上半身开始/结束瞄准时的权重变化速度 只影响表现，不参与 Simulation ")]
        [SerializeField, Min(0f)] private float _upperBodyAimBlendSpeed = 10f;
        [Tooltip("上半身追随 AimYaw 的平滑时间 过小会生硬，过大会明显落后鼠标 ")]
        [SerializeField, Min(0f)] private float _upperBodyAimSmoothTime = 0.06f;
        [Tooltip("AimYaw 分配给 Spine、Chest、UpperChest 的相对权重；缺少的骨骼会被自动跳过并重新归一化 ")]
        [SerializeField] private Vector3 _upperBodyAimBoneWeights = new(0.2f, 0.35f, 0.45f);

        [Header("非瞄准转弯倾斜")]
        [Tooltip("非瞄准移动转弯时允许的最大身体侧倾角度 ")]
        [SerializeField, Range(0f, 30f)] private float _turnLeanMaxAngle = 3f;
        [Tooltip("达到最大侧倾所对应的角速度（度/秒） ")]
        [SerializeField, Min(1f)] private float _turnLeanAngularSpeed = 540f;
        [Tooltip("低于该移动速度时不追加侧倾，避免原地轻点方向键时模型左右摇摆 ")]
        [SerializeField, Min(0f)] private float _turnLeanMinSpeed = 1.5f;
        [Tooltip("达到该移动速度后才使用完整侧倾幅度；中间区域按速度平滑渐入 ")]
        [SerializeField, Min(0f)] private float _turnLeanFullSpeed = 5.5f;
        [Tooltip("侧倾追随角速度的平滑时间 ")]
        [SerializeField, Min(0f)] private float _turnLeanSmoothTime = 0.18f;

        [Header("急转表现")]
        [Tooltip("急转触发后 Pivot 动画最多占用表现层的秒数；到时后立即交还给当前移动状态 ")]
        [SerializeField, Min(0f)] private float _pivotVisualDuration = 0.3f;

        // 提供已经选择好 Owner/Server/Remote 时间轴的最终状态 
        private PlayerSyncController _syncController;
        // 提供异步加载、可替换的 ModelView.Animator 
        private PlayerAppearanceController _appearanceController;
        // 当前绑定的模型 Animator；模型未加载或被替换时可为空/改变 
        private Animator _animator;
        // -1 表示当前 Controller 没有瞄准上层；动态模型替换后会重新解析 
        private int _upperBodyLayerIndex = -1;
        // 最近已经映射到 Animator 的事件序号，用于预测 Replay 和重复 Snapshot 去重 
        private uint _lastShotSequence;
        // Shoot 是瞬时事件，需在动作层进入 Fire 并返回 Empty 前保持层权重 
        private bool _fireOverlayRequested;
        // 给 Animator 一帧时间消费刚设置的 Trigger，避免仍处于 Empty 时提前清除请求 
        private int _fireOverlayRequestFrame = -1;
        // Humanoid 躯干骨骼随动态模型一起替换，只能在 Animator 重新绑定后缓存 
        private Transform _spine;
        private Transform _chest;
        private Transform _upperChest;
        // 各角色预制件显式提供的表现根/专用 Pivot，用于给完整模型追加非瞄准转弯侧倾 
        // 不能依赖 Animator 每帧重写 Generic 的运动根，因此侧倾始终以缓存的初始局部旋转绝对赋值 
        private Transform _leanRoot;
        private Quaternion _leanRootBaseLocalRotation = Quaternion.identity;
        private bool _hasLeanRootBaseLocalRotation;
        private bool _hasCachedModelBones;
        // 当前平滑后的局部 AimYaw 和表现权重；二者都不属于可回滚 Gameplay 状态 
        private float _upperBodyAimYaw;
        private float _upperBodyAimYawVelocity;
        private float _upperBodyAimWeight;
        private float _turnLeanAngle;
        private float _turnLeanVelocity;
        private bool _wasSimulationPivoting;
        private float _pivotVisualTimeRemaining;
        private bool _hadMoveInput;
        private bool _stopBlendPending;

        /// <summary>缓存固定玩家根节点组件；Animator 本身可能尚未随模型加载完成，因此不在这里绑定 </summary>
        private void Awake()
        {
            _syncController = GetComponent<PlayerSyncController>();
            _appearanceController = GetComponent<PlayerAppearanceController>();
        }

        /// <summary>
        /// 在同步总控完成本帧预测/远端插值应用后，把最终状态映射到 Animator 
        /// 使用 LateUpdate 是表现时序选择，不参与固定 Tick Simulation，因此这里允许使用 Time.deltaTime 做视觉阻尼 
        /// </summary>
        private void LateUpdate()
        {
            if (!TryBindAnimator())
                return;

            float deltaTime = Time.deltaTime;
            PlayerMotionState motion = _syncController.MotionState;
            PlayerControlState control = _syncController.ControlState;
            PlayerActionRuntimeState action = _syncController.ActionState;
            float aimYaw = ResolveAimYaw(_syncController.AimDirection);

            // Controller 先由 LocomotionMode 选择整套全身树，再用 Speed 选择该模式内部的 Idle/Walk/Jog 
            // VelocityX/Z 只负责当前步态的二维方向混合 
            // 不再把方向乘 NormalizedSpeed，否则低速时方向点会错误地向二维树中心收缩 
            // Speed 接近零时禁止相除，避免停止帧放大浮点噪声或产生 NaN 
            Vector3 localDirection = motion.Speed > 0.001f
                ? motion.LocalVelocity / motion.Speed
                : Vector3.zero;

            _animator.SetFloat(
                VelocityXHash,
                Mathf.Clamp(localDirection.x, -1f, 1f),
                _locomotionDampTime,
                deltaTime);
            _animator.SetFloat(
                VelocityZHash,
                Mathf.Clamp(localDirection.z, -1f, 1f),
                _locomotionDampTime,
                deltaTime);
            _animator.SetFloat(
                SpeedHash,
                motion.Speed,
                _speedDampTime,
                deltaTime);
            _animator.SetBool(HasMoveInputHash, motion.HasMoveInput);

            // 直接消费可回滚 HFSM 的互斥移动模式，不再让 Animator 用 Speed 或输入自行猜测 Aim/Sprint 
            // PlayerLocomotionMode 的稳定映射为 Free=0、Aim=1、Sprint=2，需与 Controller 条件保持一致 
            _animator.SetInteger(LocomotionModeHash, (int)control.LocomotionMode);

            // HFSM 的其余完整枚举仍保留在 SimulationState；Animator 只消费会改变表现拓扑的最小事实 
            _animator.SetBool(IsDeadHash, control.IsDead);
            _animator.SetBool(IsHitReactingHash, control.IsHitReacting);
            _animator.SetBool(IsReloadingHash, control.IsReloading);
            // 当前只有一支强反向 Pivot 动画，但 Simulation 已经用速度与目标速度的夹角确认了
            // 这是急转 不能再要求目标必须落在正在快速旋转的 Root 局部 Backward 象限，
            // 否则同一次反转会被误分类成 Left/Right 而漏播 
            bool simulationPivoting =
                !control.IsAiming &&
                motion.IsPivoting;
            if (simulationPivoting && !_wasSimulationPivoting)
                _pivotVisualTimeRemaining = _pivotVisualDuration;
            else if (_pivotVisualTimeRemaining > 0f)
                _pivotVisualTimeRemaining = Mathf.Max(0f, _pivotVisualTimeRemaining - deltaTime);

            _wasSimulationPivoting = simulationPivoting;
            _animator.SetBool(IsPivotingHash, _pivotVisualTimeRemaining > 0f);

            // 不等待当前 Move/Sprint 过渡或实际速度完全衰减：只在输入下降沿请求一次回 Idle 
            // Pivot 仍拥有固定的表现窗口，窗口结束后才执行延后的停止请求 
            if (_hadMoveInput && !motion.HasMoveInput)
                _stopBlendPending = true;
            else if (motion.HasMoveInput)
                _stopBlendPending = false;

            if (_stopBlendPending &&
                _pivotVisualTimeRemaining <= 0f &&
                control.IsAlive &&
                !control.IsHitReacting)
            {
                _animator.CrossFadeInFixedTime(
                    _baseIdleStateName,
                    _stopBlendDuration,
                    0,
                    0f);
                _stopBlendPending = false;
            }

            _hadMoveInput = motion.HasMoveInput;

            ConsumeSequenceTriggers(action, control);
            UpdateUpperBodyLayerWeight(control, deltaTime);
            UpdateWeaponIK(control);
            UpdateTurnLean(motion, control, deltaTime);
            UpdateUpperBodyAim(control, aimYaw, deltaTime);
            UpdateWeaponAim(control);
        }

        /// <summary>
        /// 绑定 PlayerAppearanceController 当前模型的 Animator 
        /// 返回 false 表示模型尚未加载或 Animator 未启用；Animator 被替换时会重建事件消费基线 
        /// </summary>
        private bool TryBindAnimator()
        {
            Animator candidate = _appearanceController != null &&
                                 _appearanceController.ModelView != null
                ? _appearanceController.ModelView.Animator
                : null;

            if (candidate == _animator)
            {
                // Unity Play Mode 热重载会保留旧的 Animator 引用，但新加入的骨骼缓存字段可能仍为空 
                // 同一模型的 Avatar 也可能被外部重新绑定，因此这里允许按需补建缓存 
                if (_animator != null && !_hasCachedModelBones)
                    CacheUpperBodyBones();

                if (_animator == null)
                    return false;

                if (!_animator.isActiveAndEnabled)
                {
                    RestoreLeanRootRotation();
                    return false;
                }

                return true;
            }

            RestoreLeanRootRotation();
            _animator = candidate;
            _upperBodyLayerIndex = -1;
            _fireOverlayRequested = false;
            _fireOverlayRequestFrame = -1;
            _spine = null;
            _chest = null;
            _upperChest = null;
            _leanRoot = null;
            _leanRootBaseLocalRotation = Quaternion.identity;
            _hasLeanRootBaseLocalRotation = false;
            _hasCachedModelBones = false;
            _upperBodyAimYaw = 0f;
            _upperBodyAimYawVelocity = 0f;
            _upperBodyAimWeight = 0f;
            _turnLeanAngle = 0f;
            _turnLeanVelocity = 0f;
            _wasSimulationPivoting = false;
            _pivotVisualTimeRemaining = 0f;
            _hadMoveInput = false;
            _stopBlendPending = false;

            if (_animator == null)
                return false;

            _upperBodyLayerIndex = _animator.GetLayerIndex(_upperBodyLayerName);
            if (_upperBodyLayerIndex >= 0)
                _animator.SetLayerWeight(_upperBodyLayerIndex, 0f);

            // Gameplay Root 只能由确定性 Simulation 驱动；任何导入动画都不得通过 Root Motion 改写表现层父节点 
            _animator.applyRootMotion = false;

            // Humanoid 可自动解析标准骨骼；Generic 由各自 PlayerModelView 显式提供骨骼引用 
            CacheUpperBodyBones();

            // 新模型只从“当前”状态继续表现，不补播模型加载前或旧模型已经表现过的历史事件 
            PlayerActionRuntimeState action = _syncController.ActionState;
            _lastShotSequence = action.ShotSequence;
            return _animator.isActiveAndEnabled;
        }

        /// <summary>从当前 Animator 解析程序化瞄准和转弯侧倾使用的骨骼 </summary>
        private void CacheUpperBodyBones()
        {
            if (_animator == null)
                return;

            ProjectGame.HotFix.Character.PlayerModelView modelView =
                _appearanceController != null ? _appearanceController.ModelView : null;
            if (modelView != null)
            {
                // 模型根位于玩家 Simulation Root 之下，是安全的纯表现 Pivot 
                // 角色也可在自己的预制件中提供更细粒度的专用 Pivot；不要指定会被动画驱动的运动根骨骼 
                _leanRoot = modelView.LeanRoot != null
                    ? modelView.LeanRoot
                    : modelView.transform;
                _spine = modelView.AnimationSpine;
                _chest = modelView.AnimationChest;
                _upperChest = modelView.AnimationUpperChest;
            }

            if (_animator.isHuman)
            {
                if (_spine == null)
                    _spine = _animator.GetBoneTransform(HumanBodyBones.Spine);
                if (_chest == null)
                    _chest = _animator.GetBoneTransform(HumanBodyBones.Chest);
                if (_upperChest == null)
                    _upperChest = _animator.GetBoneTransform(HumanBodyBones.UpperChest);
            }

            if (_leanRoot != null)
            {
                _leanRootBaseLocalRotation = _leanRoot.localRotation;
                _hasLeanRootBaseLocalRotation = true;
            }

            _hasCachedModelBones = true;
        }

        /// <summary>
        /// 把同步后的世界平面 AimDirection 转换成玩家 Root 局部水平偏角 
        /// 正值向右、负值向左；无效方向返回 0，禁止把零向量解释成世界前方 
        /// </summary>
        private float ResolveAimYaw(Vector2 aimDirection)
        {
            if (aimDirection.sqrMagnitude <= 0.000001f)
                return 0f;

            Vector3 bodyForward = transform.forward;
            bodyForward.y = 0f;
            if (bodyForward.sqrMagnitude <= 0.000001f)
                return 0f;

            Vector3 worldAim = new(aimDirection.x, 0f, aimDirection.y);
            return Vector3.SignedAngle(bodyForward.normalized, worldAim.normalized, Vector3.up);
        }

        /// <summary>
        /// 在 Animator 已完成本帧采样后的 LateUpdate，给三段 Humanoid 躯干追加水平旋转 
        /// Root 开始跟随后，aimYaw 会随身体转向自然回落，于是形成“上半身先瞄准、Root 后跟随”的连续表现 
        /// 这里不写 Animator 参数、不移动玩家根节点，也不进入网络/回滚状态 
        /// </summary>
        private void UpdateUpperBodyAim(
            in PlayerControlState control,
            float aimYaw,
            float deltaTime)
        {
            bool hasAimBones = _spine != null || _chest != null || _upperChest != null;
            bool canAimUpperBody =
                hasAimBones &&
                control.IsAiming &&
                control.IsAlive &&
                !control.IsHitReacting &&
                !control.IsReloading &&
                _syncController.AimDirection.sqrMagnitude > 0.000001f;

            float simulationLimit = _syncController.AimBodyTurnStartAngle;
            float maxYaw = simulationLimit > 0f
                ? Mathf.Min(_upperBodyAimMaxYaw, simulationLimit)
                : _upperBodyAimMaxYaw;
            float targetYaw = canAimUpperBody
                ? Mathf.Clamp(aimYaw, -maxYaw, maxYaw)
                : 0f;

            _upperBodyAimYaw = _upperBodyAimSmoothTime <= 0f
                ? targetYaw
                : Mathf.SmoothDampAngle(
                    _upperBodyAimYaw,
                    targetYaw,
                    ref _upperBodyAimYawVelocity,
                    _upperBodyAimSmoothTime,
                    Mathf.Infinity,
                    deltaTime);

            float targetWeight = canAimUpperBody ? 1f : 0f;
            _upperBodyAimWeight = _upperBodyAimBlendSpeed <= 0f
                ? targetWeight
                : Mathf.MoveTowards(
                    _upperBodyAimWeight,
                    targetWeight,
                    _upperBodyAimBlendSpeed * deltaTime);

            float weightedYaw = _upperBodyAimYaw * _upperBodyAimWeight;
            if (Mathf.Abs(weightedYaw) <= 0.001f)
                return;

            // 只统计实际存在且配置为正数的骨骼，保证不同 Humanoid Avatar 上总旋转量一致 
            float spineWeight = _spine != null ? Mathf.Max(0f, _upperBodyAimBoneWeights.x) : 0f;
            float chestWeight = _chest != null ? Mathf.Max(0f, _upperBodyAimBoneWeights.y) : 0f;
            float upperChestWeight = _upperChest != null ? Mathf.Max(0f, _upperBodyAimBoneWeights.z) : 0f;
            float totalWeight = spineWeight + chestWeight + upperChestWeight;
            if (totalWeight <= 0.0001f)
                return;

            Vector3 worldUp = transform.up;
            ApplyBoneYaw(_spine, weightedYaw * spineWeight / totalWeight, worldUp);
            ApplyBoneYaw(_chest, weightedYaw * chestWeight / totalWeight, worldUp);
            ApplyBoneYaw(_upperChest, weightedYaw * upperChestWeight / totalWeight, worldUp);
        }

        /// <summary>
        /// 绕玩家世界 Up 给当前动画姿势追加偏航 父骨骼先旋转，子骨骼再追加，因此归一化权重之和就是最终躯干偏角 
        /// 使用世界轴可避免不同 Avatar 的骨骼局部轴朝向不一致导致反向或侧翻 
        /// </summary>
        private static void ApplyBoneYaw(Transform bone, float yaw, Vector3 worldUp)
        {
            if (bone == null || Mathf.Abs(yaw) <= 0.001f)
                return;

            bone.rotation = Quaternion.AngleAxis(yaw, worldUp) * bone.rotation;
        }

        /// <summary>
        /// 把单调事件序号映射为 Animator Trigger 
        /// 使用 TickMath.IsNewer 支持 uint 回绕，并忽略预测回滚造成的序号倒退 
        /// 本驱动对一次观察到的“序号变化”最多触发一次动画；实际武器/伤害消费者若需逐发处理，不能照搬这种表现合并策略 
        /// </summary>
        private void ConsumeSequenceTriggers(
            in PlayerActionRuntimeState action,
            in PlayerControlState control)
        {
            if (action.ShotSequence != _lastShotSequence)
            {
                if (TickMath.IsNewer(action.ShotSequence, _lastShotSequence))
                {
                    // 设计要求非瞄准时不启用上层；只有瞄准射击才进入 Generic 上半身 Fire 
                    if (control.IsAiming)
                    {
                        _animator.SetTrigger(ShootHash);
                        _fireOverlayRequested = true;
                        _fireOverlayRequestFrame = Time.frameCount;
                    }
                }

                _lastShotSequence = action.ShotSequence;
            }
        }

        /// <summary>
        /// 仅在 Fire/Reload 播放期间启用上半身动作层，让 Base FullBody 持续提供骨盆和双腿运动 
        /// Reload 使用可回滚的持续状态；Fire 使用事件序号启动，并在 Animator 返回 Empty 后结束表现请求 
        /// 受击或死亡会立即撤销动作表现，但不会在这里修改 Gameplay 状态 
        /// </summary>
        private void UpdateUpperBodyLayerWeight(in PlayerControlState control, float deltaTime)
        {
            if (_upperBodyLayerIndex < 0)
                return;

            bool allowUpperBody =
                control.IsAiming &&
                control.IsAlive &&
                !control.IsHitReacting;
            if (!allowUpperBody)
            {
                _fireOverlayRequested = false;
                _animator.ResetTrigger(ShootHash);
            }
            else if (_fireOverlayRequested && Time.frameCount > _fireOverlayRequestFrame + 1)
            {
                AnimatorStateInfo current = _animator.GetCurrentAnimatorStateInfo(_upperBodyLayerIndex);
                bool fireIsPlaying = current.shortNameHash == FireStateHash;

                if (_animator.IsInTransition(_upperBodyLayerIndex))
                {
                    AnimatorStateInfo next = _animator.GetNextAnimatorStateInfo(_upperBodyLayerIndex);
                    fireIsPlaying |= next.shortNameHash == FireStateHash;
                }

                if (!fireIsPlaying)
                    _fireOverlayRequested = false;
            }

            float targetWeight = allowUpperBody ? 1f : 0f;
            float currentWeight = _animator.GetLayerWeight(_upperBodyLayerIndex);
            _animator.SetLayerWeight(
                _upperBodyLayerIndex,
                Mathf.MoveTowards(currentWeight, targetWeight, deltaTime * _upperBodyLayerBlendSpeed));
        }

        /// <summary>
        /// Animator 完成本帧采样后，在 Generic 骨架根追加与角速度相反的侧倾 
        /// 只作用于非瞄准移动表现，不修改玩家 Root、碰撞体、预测或网络状态 
        /// </summary>
        private void UpdateTurnLean(
            in PlayerMotionState motion,
            in PlayerControlState control,
            float deltaTime)
        {
            if (_leanRoot == null || !_hasLeanRootBaseLocalRotation)
                return;

            bool canLean =
                !control.IsAiming &&
                control.IsAlive &&
                !control.IsHitReacting &&
                motion.IsMoving &&
                motion.PivotDirection == PlayerPivotDirection.None &&
                motion.Speed > _turnLeanMinSpeed;
            float normalizedAngularSpeed = canLean
                ? Mathf.Clamp(motion.AngularSpeed / Mathf.Max(1f, _turnLeanAngularSpeed), -1f, 1f)
                : 0f;
            float fullLeanSpeed = Mathf.Max(_turnLeanMinSpeed + 0.001f, _turnLeanFullSpeed);
            float speedWeight = canLean
                ? Mathf.InverseLerp(_turnLeanMinSpeed, fullLeanSpeed, motion.Speed)
                : 0f;
            float targetAngle = -normalizedAngularSpeed * _turnLeanMaxAngle * speedWeight;

            _turnLeanAngle = _turnLeanSmoothTime <= 0f
                ? targetAngle
                : Mathf.SmoothDampAngle(
                    _turnLeanAngle,
                    targetAngle,
                    ref _turnLeanVelocity,
                    _turnLeanSmoothTime,
                    Mathf.Infinity,
                    deltaTime);

            // 绝对写入而不是乘到上一帧结果上 Generic Animator 可能把顶层骨骼作为运动根提取，
            // 即使 applyRootMotion=false 也不保证每帧重置该骨骼；增量相乘会因此持续累积直至角色侧倒 
            _leanRoot.localRotation =
                _leanRootBaseLocalRotation *
                Quaternion.AngleAxis(_turnLeanAngle, Vector3.forward);
        }

        /// <summary>解绑、禁用 Animator 或关闭驱动时清除纯表现侧倾，避免模型保留上一帧姿势 </summary>
        private void RestoreLeanRootRotation()
        {
            if (_leanRoot != null && _hasLeanRootBaseLocalRotation)
                _leanRoot.localRotation = _leanRootBaseLocalRotation;

            _turnLeanAngle = 0f;
            _turnLeanVelocity = 0f;
        }

        private void OnDisable()
        {
            RestoreLeanRootRotation();
            _appearanceController?.AnimationBridge?.SetWeaponAimDirection(Vector3.zero, false);
        }

        /// <summary>
        /// 左手 IK 的目标仍由 CharacterAnimationBridge/WeaponView 管理；这里仅根据 Gameplay 状态控制是否允许求解 
        /// Reload、Hit 和 Dead 需要释放左手，让对应动作片段能够完整控制手臂 
        /// </summary>
        private void UpdateWeaponIK(in PlayerControlState control)
        {
            if (_appearanceController == null || _appearanceController.AnimationBridge == null)
                return;

            bool allowLeftHandIK =
                control.IsAlive &&
                !control.IsHitReacting &&
                !control.IsReloading;

            var animationBridge = _appearanceController.AnimationBridge;
            animationBridge.SetLeftHandIKAllowed(allowLeftHandIK);
            // 换弹由左手执行，右手继续锁住武器；死亡时才完全释放主握点 
            animationBridge.SetRightHandIKAllowed(control.IsAlive);
        }

        /// <summary>
        /// 把同步时间线中的世界平面 AimDirection 交给角色表现桥 
        /// 桥会在本驱动完成 Spine Aim 后旋转枪械，再执行双手 IK 
        /// </summary>
        private void UpdateWeaponAim(in PlayerControlState control)
        {
            if (_appearanceController == null || _appearanceController.AnimationBridge == null)
                return;

            Vector2 aimDirection = _syncController.AimDirection;
            bool allowWeaponAim =
                control.IsAiming &&
                control.IsAlive &&
                !control.IsHitReacting &&
                !control.IsReloading &&
                aimDirection.sqrMagnitude > 0.000001f;

            _appearanceController.AnimationBridge.SetWeaponAimDirection(
                new Vector3(aimDirection.x, 0f, aimDirection.y),
                allowWeaponAim);
        }
    }
}
