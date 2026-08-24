using System;
using UnityEngine;

namespace ProjectGame.HotFix.Character
{
    /// <summary>
    /// Character Prefab 的通用动画桥接层。
    ///
    /// 这里只放 Lobby 与 Gameplay 都成立的动画语义，
    /// 不负责移动、攻击、跳跃等 Gameplay 专属状态。
    /// </summary>
    [DefaultExecutionOrder(200)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class CharacterAnimationBridge : MonoBehaviour
    {
        // 暂时沿用原 Animator 参数名，避免为了重构重新修改 Animator。
        private static readonly int EquipmentPoseHash = Animator.StringToHash("EquipmentPose");
        private static readonly int DoEquipHash = Animator.StringToHash("DoEquip");

        [Header("Weapon Horizontal Aim")]
        [Tooltip("独立于手臂的武器水平瞄准节点。为空时保留旧挂接方式，不执行枪口方向修正。")]
        [SerializeField] private Transform _weaponAimPivot;
        [Tooltip("枪械相对动画姿势允许追加的最大水平偏角。")]
        [SerializeField, Range(0f, 180f)] private float _weaponAimMaxYaw = 65f;
        [Tooltip("武器水平瞄准追赶目标方向的最大角速度（度/秒）。设置为 0 时立即对准。")]
        [SerializeField, Min(0f)] private float _weaponAimAngularSpeed = 900f;
        [Tooltip("在 Idle 自动捕获的枪口基础姿态上追加的 Pitch。当前输入仍只提供水平 AimDirection，不读取鼠标地面高度。")]
        [SerializeField, Range(-45f, 45f)] private float _weaponAimPitch;
        [Tooltip("在 Idle 自动捕获的枪口基础姿态上追加的 Roll；通常保持为 0。")]
        [SerializeField, Range(-45f, 45f)] private float _weaponAimRoll;
        [Tooltip("完整枪械姿态渐入/渐出的权重变化速度。")]
        [SerializeField, Min(0f)] private float _weaponAimPoseBlendSpeed = 12f;

        [Header("Weapon IK")]
        [Tooltip("左手 IK 权重每秒变化量；用于在换弹、受击、死亡和正常持枪之间平滑切换。")]
        [SerializeField, Min(0f)] private float _leftHandIkBlendSpeed = 12f;
        [Tooltip("右手 IK 权重每秒变化量；右手以武器主握点为目标。")]
        [SerializeField, Min(0f)] private float _rightHandIkBlendSpeed = 16f;

        [Header("Character Hand IK Calibration")]
        [Tooltip("角色右手骨骼相对武器 MainHandGrip 的旋转校准。只配置在角色预制件，不修改共享武器握点。")]
        [SerializeField] private Vector3 _rightHandRotationOffsetEuler;
        [Tooltip("角色左手骨骼相对武器 OffHandGrip 的旋转校准。")]
        [SerializeField] private Vector3 _leftHandRotationOffsetEuler;
        [SerializeField, Range(0f, 1f)] private float _rightHandPositionWeight = 1f;
        [SerializeField, Range(0f, 1f)] private float _rightHandRotationWeight = 1f;
        [SerializeField, Range(0f, 1f)] private float _leftHandPositionWeight = 1f;
        [SerializeField, Range(0f, 1f)] private float _leftHandRotationWeight = 1f;
        [Tooltip("右手相对动画姿势允许的最大旋转修正角；180 表示不限制。")]
        [SerializeField, Range(0f, 180f)] private float _rightHandMaxRotationCorrection = 180f;
        [Tooltip("左手相对动画姿势允许的最大旋转修正角；180 表示不限制。")]
        [SerializeField, Range(0f, 180f)] private float _leftHandMaxRotationCorrection = 180f;

        [Header("Generic Right Hand IK")]
        [Tooltip("Generic Avatar 的右上臂。Humanoid Avatar 会忽略这些引用并使用 Animator IK。")]
        [SerializeField] private Transform _genericRightUpperArm;
        [Tooltip("Generic Avatar 的右前臂。")]
        [SerializeField] private Transform _genericRightForearm;
        [Tooltip("Generic Avatar 的右手。")]
        [SerializeField] private Transform _genericRightHand;
        [Tooltip("可选的右肘朝向提示；为空时保持当前动画提供的弯肘平面。")]
        [SerializeField] private Transform _genericRightElbowHint;

        [Header("Generic Left Hand IK")]
        [Tooltip("Generic Avatar 的左上臂。Humanoid Avatar 会忽略这些引用并使用 Animator IK。")]
        [SerializeField] private Transform _genericLeftUpperArm;
        [Tooltip("Generic Avatar 的左前臂。")]
        [SerializeField] private Transform _genericLeftForearm;
        [Tooltip("Generic Avatar 的左手。")]
        [SerializeField] private Transform _genericLeftHand;
        [Tooltip("可选的左肘朝向提示；为空时保持当前动画提供的弯肘平面。")]
        [SerializeField] private Transform _genericLeftElbowHint;

        private Animator _animator;
        private WeaponView _weaponView;
        // Gameplay 只决定当前动作是否允许 IK；目标骨骼仍由已绑定 WeaponView 提供。
        private bool _leftHandIkAllowed = true;
        private bool _rightHandIkAllowed = true;
        private bool _weaponAimAllowed;
        private Vector3 _weaponAimWorldDirection;
        private Quaternion _weaponAimBaseLocalRotation;
        private bool _hasWeaponAimBaseLocalRotation;
        private WeaponView _weaponAimCalibratedWeapon;
        private Quaternion _weaponAimMuzzlePoseOffset;
        private bool _hasWeaponAimMuzzlePoseOffset;
        private float _weaponAimYaw;
        private float _weaponAimPoseWeight;
        private Vector3 _rightElbowBendDirection;
        private Vector3 _leftElbowBendDirection;
        private float _leftHandIkWeight;
        private float _rightHandIkWeight;

        public Animator Animator => _animator;
        public WeaponView CurrentWeaponView => _weaponView;
        public bool HasLeftHandIKTarget => _weaponView != null && _weaponView.OffHandGrip != null;
        public bool HasRightHandIKTarget => _weaponView != null && _weaponView.MainHandGrip != null;
        public bool HasGenericRightHandIKBones =>
            _genericRightUpperArm != null &&
            _genericRightForearm != null &&
            _genericRightHand != null;
        public bool HasGenericLeftHandIKBones =>
            _genericLeftUpperArm != null &&
            _genericLeftForearm != null &&
            _genericLeftHand != null;
        public float LeftHandIKWeight => _leftHandIkWeight;
        public float RightHandIKWeight => _rightHandIkWeight;
        public float WeaponAimYaw => _weaponAimYaw;
        public float WeaponAimPoseWeight => _weaponAimPoseWeight;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            CacheWeaponAimBaseRotation();
            _leftHandIkWeight = 0f;
            _rightHandIkWeight = 0f;
        }

        /// <summary>
        /// 绑定当前武器表现，并切换对应持械姿势。
        /// WeaponPose 的数值需要保持与现有 Animator / 配表一致。
        /// </summary>
        public void BindWeapon(WeaponView weaponView, WeaponPose pose)
        {
            _weaponView = weaponView;
            ResetWeaponAimCalibration();
            SetWeaponPose(pose);
        }

        public void UnbindWeapon()
        {
            _weaponView = null;
            ResetWeaponAimCalibration();
        }

        public void SetWeaponPose(WeaponPose pose)
        {
            Animator.SetInteger(EquipmentPoseHash, (int)pose);
        }

        public void TriggerEquip()
        {
            Animator.SetTrigger(DoEquipHash);
        }

        /// <summary>
        /// 允许上层表现驱动临时释放左手 IK。Lobby 不调用时默认启用；
        /// Gameplay 在 Reload/Hit/Dead 期间关闭，避免 IK 把动作片段中的左手强行拉回武器握点。
        /// </summary>
        public void SetLeftHandIKAllowed(bool allowed)
        {
            _leftHandIkAllowed = allowed;
        }

        /// <summary>
        /// 允许或释放右手主握点 IK。Gameplay 通常只在死亡时释放，
        /// 换弹期间仍让右手持枪，由左手动画完成弹匣动作。
        /// </summary>
        public void SetRightHandIKAllowed(bool allowed)
        {
            _rightHandIkAllowed = allowed;
        }

        /// <summary>
        /// 设置同步后的世界平面瞄准方向。这里只缓存表现意图，实际枪械旋转会在
        /// PlayerAnimationDriver 完成 Spine Aim 后、双手 IK 求解前统一执行。
        /// </summary>
        public void SetWeaponAimDirection(Vector3 worldDirection, bool allowed)
        {
            worldDirection.y = 0f;
            _weaponAimWorldDirection = worldDirection.sqrMagnitude > 0.000001f
                ? worldDirection.normalized
                : Vector3.zero;
            _weaponAimAllowed = allowed;
        }

        /// <summary>
        /// Lobby 与 Gameplay 共用双手武器 IK。
        /// Animator Controller 对应 Layer 需要开启 IK Pass。
        /// </summary>
        private void OnAnimatorIK(int layerIndex)
        {
            Animator animator = Animator;

            if (animator == null || !animator.isActiveAndEnabled)
                return;

            // Humanoid 继续使用 Unity 原生 Animator IK；Generic 在 LateUpdate 走显式双骨骼求解。
            if (!animator.isHuman)
                return;

            // 配置 WeaponAimPivot 的 Humanoid 也在 LateUpdate 使用统一双骨骼求解，
            // 保证顺序为 Spine Aim -> Weapon Aim -> Hand IK，避免原生 IK 早一帧读取旧目标。
            if (_weaponAimPivot != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
                return;
            }

            Transform offHandGrip = _weaponView != null ? _weaponView.OffHandGrip : null;
            Transform mainHandGrip = _weaponView != null ? _weaponView.MainHandGrip : null;
            Transform humanoidRightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            bool canSolveRight = IsIndependentIKTarget(mainHandGrip, humanoidRightHand);

            UpdateIKWeight(
                ref _rightHandIkWeight,
                _rightHandIkAllowed && canSolveRight,
                _rightHandIkBlendSpeed,
                Time.deltaTime);
            UpdateIKWeight(
                ref _leftHandIkWeight,
                _leftHandIkAllowed && offHandGrip != null,
                _leftHandIkBlendSpeed,
                Time.deltaTime);

            animator.SetIKPositionWeight(
                AvatarIKGoal.RightHand,
                _rightHandIkWeight * _rightHandPositionWeight);
            animator.SetIKRotationWeight(
                AvatarIKGoal.RightHand,
                _rightHandIkWeight * _rightHandRotationWeight);

            if (canSolveRight && _rightHandIkWeight > 0f)
            {
                animator.SetIKPosition(AvatarIKGoal.RightHand, mainHandGrip.position);
                Quaternion rightTargetRotation =
                    mainHandGrip.rotation *
                    Quaternion.Euler(_rightHandRotationOffsetEuler);
                if (_rightHandMaxRotationCorrection < 180f && humanoidRightHand != null)
                {
                    rightTargetRotation = Quaternion.RotateTowards(
                        humanoidRightHand.rotation,
                        rightTargetRotation,
                        _rightHandMaxRotationCorrection);
                }
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightTargetRotation);
            }

            animator.SetIKPositionWeight(
                AvatarIKGoal.LeftHand,
                _leftHandIkWeight * _leftHandPositionWeight);
            animator.SetIKRotationWeight(
                AvatarIKGoal.LeftHand,
                _leftHandIkWeight * _leftHandRotationWeight);

            if (offHandGrip == null || _leftHandIkWeight <= 0f)
                return;

            animator.SetIKPosition(AvatarIKGoal.LeftHand, offHandGrip.position);
            Quaternion leftTargetRotation =
                offHandGrip.rotation *
                Quaternion.Euler(_leftHandRotationOffsetEuler);
            Transform humanoidLeftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            if (_leftHandMaxRotationCorrection < 180f && humanoidLeftHand != null)
            {
                leftTargetRotation = Quaternion.RotateTowards(
                    humanoidLeftHand.rotation,
                    leftTargetRotation,
                    _leftHandMaxRotationCorrection);
            }
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftTargetRotation);
        }

        /// <summary>
        /// Animator 和 PlayerAnimationDriver 完成姿势后，先水平修正枪口方向，
        /// 再依据当前 Avatar 的左右臂骨骼把双手约束到武器握点。
        /// </summary>
        private void LateUpdate()
        {
            Animator animator = Animator;
            if (animator == null || !animator.isActiveAndEnabled)
                return;

            UpdateWeaponHorizontalAim(Time.deltaTime);

            // 没有独立 Aim Pivot 的 Humanoid 保持旧的原生 OnAnimatorIK 路径。
            if (animator.isHuman && _weaponAimPivot == null)
                return;

            Transform offHandGrip = _weaponView != null ? _weaponView.OffHandGrip : null;
            Transform mainHandGrip = _weaponView != null ? _weaponView.MainHandGrip : null;
            Transform rightUpperArm = animator.isHuman
                ? animator.GetBoneTransform(HumanBodyBones.RightUpperArm)
                : _genericRightUpperArm;
            Transform rightForearm = animator.isHuman
                ? animator.GetBoneTransform(HumanBodyBones.RightLowerArm)
                : _genericRightForearm;
            Transform rightHand = animator.isHuman
                ? animator.GetBoneTransform(HumanBodyBones.RightHand)
                : _genericRightHand;
            Transform leftUpperArm = animator.isHuman
                ? animator.GetBoneTransform(HumanBodyBones.LeftUpperArm)
                : _genericLeftUpperArm;
            Transform leftForearm = animator.isHuman
                ? animator.GetBoneTransform(HumanBodyBones.LeftLowerArm)
                : _genericLeftForearm;
            Transform leftHand = animator.isHuman
                ? animator.GetBoneTransform(HumanBodyBones.LeftHand)
                : _genericLeftHand;
            bool canSolveRight =
                HasArmBones(rightUpperArm, rightForearm, rightHand) &&
                IsIndependentIKTarget(mainHandGrip, rightHand);
            bool canSolveLeft =
                offHandGrip != null &&
                HasArmBones(leftUpperArm, leftForearm, leftHand);

            UpdateIKWeight(
                ref _rightHandIkWeight,
                _rightHandIkAllowed && canSolveRight,
                _rightHandIkBlendSpeed,
                Time.deltaTime);
            UpdateIKWeight(
                ref _leftHandIkWeight,
                _leftHandIkAllowed && canSolveLeft,
                _leftHandIkBlendSpeed,
                Time.deltaTime);

            // 武器挂在独立胸部根下，先把右手约束到主握点，再处理左手副握点。
            if (canSolveRight && _rightHandIkWeight > 0f)
            {
                SolveTwoBoneIK(
                    rightUpperArm,
                    rightForearm,
                    rightHand,
                    mainHandGrip,
                    Quaternion.Euler(_rightHandRotationOffsetEuler),
                    _genericRightElbowHint,
                    ref _rightElbowBendDirection,
                    _rightHandIkWeight * _rightHandPositionWeight,
                    _rightHandIkWeight * _rightHandRotationWeight,
                    _rightHandMaxRotationCorrection);
            }

            if (!canSolveLeft || _leftHandIkWeight <= 0f)
                return;

            SolveTwoBoneIK(
                leftUpperArm,
                leftForearm,
                leftHand,
                offHandGrip,
                Quaternion.Euler(_leftHandRotationOffsetEuler),
                _genericLeftElbowHint,
                ref _leftElbowBendDirection,
                _leftHandIkWeight * _leftHandPositionWeight,
                _leftHandIkWeight * _leftHandRotationWeight,
                _leftHandMaxRotationCorrection);
        }

        /// <summary>
        /// 每帧先恢复 Pivot 的预制件基准旋转。未瞄准时自动记录当前 Idle 枪口相对
        /// 水平 Forward 的完整姿态；瞄准时只替换水平 Forward，并保留该 Pitch/Roll。
        /// 配置 Pitch/Roll 仅作为角色专属的附加校准，不会覆盖手工调好的持枪姿势。
        /// </summary>
        private void UpdateWeaponHorizontalAim(float deltaTime)
        {
            if (_weaponAimPivot == null)
                return;

            if (!_hasWeaponAimBaseLocalRotation)
                CacheWeaponAimBaseRotation();

            if (!_hasWeaponAimBaseLocalRotation)
                return;

            _weaponAimPivot.localRotation = _weaponAimBaseLocalRotation;
            Quaternion basePivotRotation = _weaponAimPivot.rotation;

            Transform muzzle = _weaponView != null ? _weaponView.Muzzle : null;
            Vector3 neutralForward = muzzle != null
                ? Vector3.ProjectOnPlane(muzzle.forward, Vector3.up)
                : Vector3.zero;

            // 在非瞄准 Idle 也会执行捕获，确保参考的是用户当前调好的持枪姿势，
            // 而不是第一次按下瞄准后已经进入过渡的动画姿势。
            if (muzzle != null &&
                neutralForward.sqrMagnitude > 0.000001f &&
                (!_hasWeaponAimMuzzlePoseOffset || _weaponAimCalibratedWeapon != _weaponView))
            {
                Quaternion neutralHorizontalFrame = Quaternion.LookRotation(
                    neutralForward.normalized,
                    Vector3.up);
                _weaponAimMuzzlePoseOffset =
                    Quaternion.Inverse(neutralHorizontalFrame) *
                    muzzle.rotation;
                _weaponAimCalibratedWeapon = _weaponView;
                _hasWeaponAimMuzzlePoseOffset = true;
            }

            bool canAim =
                _weaponAimAllowed &&
                muzzle != null &&
                _hasWeaponAimMuzzlePoseOffset &&
                neutralForward.sqrMagnitude > 0.000001f &&
                _weaponAimWorldDirection.sqrMagnitude > 0.000001f;
            float targetYaw = 0f;

            if (canAim)
            {
                targetYaw = Mathf.Clamp(
                    Vector3.SignedAngle(
                        neutralForward.normalized,
                        _weaponAimWorldDirection,
                        Vector3.up),
                    -_weaponAimMaxYaw,
                    _weaponAimMaxYaw);
            }

            _weaponAimYaw = _weaponAimAngularSpeed <= 0f
                ? targetYaw
                : Mathf.MoveTowardsAngle(
                    _weaponAimYaw,
                    targetYaw,
                    _weaponAimAngularSpeed * deltaTime);

            float targetPoseWeight = canAim ? 1f : 0f;
            _weaponAimPoseWeight = _weaponAimPoseBlendSpeed <= 0f
                ? targetPoseWeight
                : Mathf.MoveTowards(
                    _weaponAimPoseWeight,
                    targetPoseWeight,
                    _weaponAimPoseBlendSpeed * deltaTime);

            if (muzzle == null || _weaponAimPoseWeight <= 0.001f)
                return;

            if (neutralForward.sqrMagnitude <= 0.000001f)
                return;

            Vector3 constrainedForward =
                Quaternion.AngleAxis(_weaponAimYaw, Vector3.up) *
                neutralForward.normalized;
            Quaternion desiredMuzzleRotation =
                Quaternion.LookRotation(constrainedForward, Vector3.up) *
                _weaponAimMuzzlePoseOffset *
                Quaternion.Euler(_weaponAimPitch, 0f, _weaponAimRoll);
            Quaternion pivotToMuzzleRotation =
                Quaternion.Inverse(basePivotRotation) * muzzle.rotation;
            Quaternion desiredPivotRotation =
                desiredMuzzleRotation *
                Quaternion.Inverse(pivotToMuzzleRotation);

            _weaponAimPivot.rotation = Quaternion.Slerp(
                basePivotRotation,
                desiredPivotRotation,
                _weaponAimPoseWeight);
        }

        private void CacheWeaponAimBaseRotation()
        {
            if (_weaponAimPivot == null)
            {
                _hasWeaponAimBaseLocalRotation = false;
                return;
            }

            _weaponAimBaseLocalRotation = _weaponAimPivot.localRotation;
            _hasWeaponAimBaseLocalRotation = true;
        }

        private void ResetWeaponAimCalibration()
        {
            _weaponAimCalibratedWeapon = null;
            _weaponAimMuzzlePoseOffset = Quaternion.identity;
            _hasWeaponAimMuzzlePoseOffset = false;
        }

        private static bool HasArmBones(Transform upperArm, Transform forearm, Transform hand)
        {
            return upperArm != null && forearm != null && hand != null;
        }

        private static void UpdateIKWeight(
            ref float currentWeight,
            bool shouldEnable,
            float blendSpeed,
            float deltaTime)
        {
            float targetWeight = shouldEnable ? 1f : 0f;
            float maxDelta = blendSpeed <= 0f
                ? 1f
                : blendSpeed * deltaTime;

            currentWeight = Mathf.MoveTowards(
                currentWeight,
                targetWeight,
                maxDelta);
        }

        /// <summary>
        /// 武器若仍是右手骨骼的子节点，右手追武器会形成逐帧反馈漂移；
        /// 这种旧预制件自动禁用右手 IK，只保留原有父子挂接表现。
        /// </summary>
        private static bool IsIndependentIKTarget(Transform target, Transform hand)
        {
            return target != null &&
                   hand != null &&
                   target != hand &&
                   !target.IsChildOf(hand);
        }

        private static void SolveTwoBoneIK(
            Transform upperArm,
            Transform forearm,
            Transform hand,
            Transform target,
            Quaternion targetRotationOffset,
            Transform elbowHint,
            ref Vector3 bendDirectionMemory,
            float positionWeight,
            float rotationWeight,
            float maxRotationCorrection)
        {
            positionWeight = Mathf.Clamp01(positionWeight);
            rotationWeight = Mathf.Clamp01(rotationWeight);
            if (positionWeight <= 0f && rotationWeight <= 0f)
                return;

            Quaternion upperOriginal = upperArm.rotation;
            Quaternion forearmOriginal = forearm.rotation;
            Quaternion handOriginal = hand.rotation;

            if (positionWeight > 0f)
            {
                Vector3 rootPosition = upperArm.position;
                Vector3 elbowPosition = forearm.position;
                Vector3 handPosition = hand.position;
                Vector3 targetPosition = Vector3.Lerp(
                    handPosition,
                    target.position,
                    positionWeight);

                float upperLength = Vector3.Distance(rootPosition, elbowPosition);
                float lowerLength = Vector3.Distance(elbowPosition, handPosition);
                Vector3 rootToTarget = targetPosition - rootPosition;
                float rawDistance = rootToTarget.magnitude;
                if (upperLength > 0.0001f && lowerLength > 0.0001f && rawDistance > 0.0001f)
                {
                    float targetDistance = Mathf.Clamp(
                        rawDistance,
                        Mathf.Abs(upperLength - lowerLength) + 0.0001f,
                        upperLength + lowerLength - 0.0001f);
                    Vector3 targetDirection = rootToTarget / rawDistance;
                    Vector3 bendDirection = ResolveBendDirection(
                        rootPosition,
                        elbowPosition,
                        targetDirection,
                        upperArm,
                        elbowHint,
                        ref bendDirectionMemory);

                    float along =
                        (upperLength * upperLength - lowerLength * lowerLength + targetDistance * targetDistance) /
                        (2f * targetDistance);
                    float perpendicular = Mathf.Sqrt(
                        Mathf.Max(0f, upperLength * upperLength - along * along));
                    Vector3 solvedElbow =
                        rootPosition +
                        targetDirection * along +
                        bendDirection * perpendicular;

                    upperArm.rotation =
                        Quaternion.FromToRotation(
                            elbowPosition - rootPosition,
                            solvedElbow - rootPosition) *
                        upperArm.rotation;

                    Vector3 solvedForearmPosition = forearm.position;
                    Vector3 solvedHandPosition = hand.position;
                    forearm.rotation =
                        Quaternion.FromToRotation(
                            solvedHandPosition - solvedForearmPosition,
                            targetPosition - solvedForearmPosition) *
                        forearm.rotation;

                    Quaternion upperSolved = upperArm.rotation;
                    Quaternion forearmSolved = forearm.rotation;
                    upperArm.rotation = Quaternion.Slerp(
                        upperOriginal,
                        upperSolved,
                        positionWeight);
                    forearm.rotation = Quaternion.Slerp(
                        forearmOriginal,
                        forearmSolved,
                        positionWeight);
                }
            }

            Quaternion calibratedTargetRotation =
                target.rotation * targetRotationOffset;
            if (maxRotationCorrection < 180f)
            {
                calibratedTargetRotation = Quaternion.RotateTowards(
                    handOriginal,
                    calibratedTargetRotation,
                    Mathf.Max(0f, maxRotationCorrection));
            }

            // 使用动画采样后的手腕姿势作为零点。RotationWeight=0 时保持动画手腕，
            // PositionWeight 仍可让整条手臂追随握点，避免为位置 IK 强制扭转手腕。
            hand.rotation = Quaternion.Slerp(
                handOriginal,
                calibratedTargetRotation,
                rotationWeight);
        }

        private static Vector3 ResolveBendDirection(
            Vector3 rootPosition,
            Vector3 elbowPosition,
            Vector3 targetDirection,
            Transform upperArm,
            Transform elbowHint,
            ref Vector3 bendDirectionMemory)
        {
            // 有 Hint 时直接使用 Pole 在目标轴垂直平面上的投影，不再根据当前动画肘部
            // 翻转符号。Fire 动画即使令手臂短暂伸直，也不会跳到另一侧。
            Vector3 candidate = elbowHint != null
                ? Vector3.ProjectOnPlane(
                    elbowHint.position - rootPosition,
                    targetDirection)
                : Vector3.ProjectOnPlane(
                    elbowPosition - rootPosition,
                    targetDirection);
            Vector3 remembered = Vector3.ProjectOnPlane(
                bendDirectionMemory,
                targetDirection);

            if (candidate.sqrMagnitude <= 0.000001f)
                candidate = remembered;

            if (candidate.sqrMagnitude <= 0.000001f)
                candidate = Vector3.ProjectOnPlane(upperArm.up, targetDirection);
            if (candidate.sqrMagnitude <= 0.000001f)
                candidate = Vector3.ProjectOnPlane(upperArm.forward, targetDirection);
            if (candidate.sqrMagnitude <= 0.000001f)
                candidate = Vector3.Cross(targetDirection, Vector3.up);
            if (candidate.sqrMagnitude <= 0.000001f)
                candidate = Vector3.Cross(targetDirection, Vector3.right);

            candidate.Normalize();

            // 没有显式 Hint 的旧角色使用上一帧方向保持半球连续；有 Hint 时由 Hint 全权决定。
            if (elbowHint == null &&
                remembered.sqrMagnitude > 0.000001f &&
                Vector3.Dot(candidate, remembered) < 0f)
            {
                candidate = -candidate;
            }

            bendDirectionMemory = candidate;
            return candidate;
        }

        private void OnDisable()
        {
            if (_weaponAimPivot != null && _hasWeaponAimBaseLocalRotation)
                _weaponAimPivot.localRotation = _weaponAimBaseLocalRotation;

            _weaponAimAllowed = false;
            _weaponAimWorldDirection = Vector3.zero;
            ResetWeaponAimCalibration();
            _weaponAimYaw = 0f;
            _weaponAimPoseWeight = 0f;
            _rightElbowBendDirection = Vector3.zero;
            _leftElbowBendDirection = Vector3.zero;
            _leftHandIkWeight = 0f;
            _rightHandIkWeight = 0f;
        }
    }
}
