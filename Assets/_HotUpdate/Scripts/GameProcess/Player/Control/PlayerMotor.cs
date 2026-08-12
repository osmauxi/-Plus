using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Movement
{
    public readonly struct PlayerMotorRuntimeState
    {
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Vector3 Velocity { get; }
        public float AngularSpeed { get; }

        public PlayerMotorRuntimeState(Vector3 position, Quaternion rotation, Vector3 velocity, float angularSpeed)
        {
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            AngularSpeed = angularSpeed;
        }
    }

    /// <summary>
    /// 玩家平面运动的唯一执行者。
    ///
    /// 负责：
    /// 1. DesiredVelocity → 实际 Velocity。
    /// 2. 普通加速 / 减速。
    /// 3. 强反向输入时的 Pivot 制动。
    /// 4. 带角加速度的朝向变化。
    /// 5. CharacterController 碰撞移动。
    /// 6. 输出统一 PlayerMotionState。
    ///
    /// 不负责：
    /// Input、FSM、Animator、Camera、Stamina、网络同步。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [Header("玩家移动配置")]
        [Tooltip("包含自由移动、瞄准、冲刺及运动状态判定的全部参数。展开后可分别调整各移动模式。")]
        [InspectorName("移动参数")]
        [SerializeField] private PlayerMovementConfig _config = new();

        private CharacterController _characterController;

        private Vector3 _velocity;
        private float _angularSpeed;
        private MotionPhase _phase = MotionPhase.Idle;

        public PlayerMovementConfig Config => _config;
        public PlayerMotionState MotionState { get; private set; }

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            if (_config == null)
                throw new InvalidOperationException($"{nameof(PlayerMotor)} 没有配置 {nameof(PlayerMovementConfig)}。");

            _config.Validate();
            ResetMotion();
        }

        public PlayerMotorRuntimeState CaptureRuntimeState()
        {
            return new PlayerMotorRuntimeState(transform.position, transform.rotation, _velocity, _angularSpeed);
        }

        public void RestoreRuntimeState(in PlayerMotorRuntimeState state)
        {
            bool wasEnabled = _characterController.enabled;

            if (wasEnabled)
                _characterController.enabled = false;

            transform.SetPositionAndRotation(state.Position, state.Rotation);

            if (wasEnabled)
                _characterController.enabled = true;

            _velocity = Flatten(state.Velocity);
            _angularSpeed = state.AngularSpeed;

            // MotionPhase 是表现事实，恢复后让下一次 Simulate 重新计算即可。
            _phase = _velocity.sqrMagnitude <= _config.StopSpeedThreshold * _config.StopSpeedThreshold ? MotionPhase.Idle : MotionPhase.Move;

            float speed = _velocity.magnitude;

            MotionState = new PlayerMotionState
            {
                Position = transform.position,
                Velocity = _velocity,
                DesiredVelocity = Vector3.zero,
                Acceleration = Vector3.zero,
                MoveDirection = speed > _config.StopSpeedThreshold ? _velocity / speed : Vector3.zero,
                FacingDirection = Flatten(transform.forward).normalized,
                DesiredFacingDirection = Vector3.zero,
                LocalVelocity = transform.InverseTransformDirection(_velocity),
                Speed = speed,
                NormalizedSpeed = 0f,
                AngularSpeed = _angularSpeed,
                Phase = _phase,
                HasMoveInput = false,
                IsMoving = speed > _config.StopSpeedThreshold,
                IsPivoting = false,
            };
        }

        /// <summary>
        /// 执行一次玩家运动模拟。
        ///
        /// command 只描述“想怎么动”；
        /// profile 描述当前 Locomotion 模式的运动响应；
        /// Motor 自身不知道 Free / Aim / Sprint。
        /// </summary>
        public void Simulate(in PlayerMotionCommand command, PlayerMovementProfile profile, float deltaTime)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            if (deltaTime <= 0f)
                return;

            Vector3 worldMove = Flatten(command.WorldMove);
            float inputMagnitude = Mathf.Clamp01(worldMove.magnitude);
            bool hasMoveInput = inputMagnitude > _config.MoveInputDeadZone;

            if (hasMoveInput)
                worldMove = worldMove.normalized * inputMagnitude;
            else
                worldMove = Vector3.zero;

            float speedMultiplier = ResolveDirectionalSpeedMultiplier(worldMove, profile);
            Vector3 desiredVelocity = worldMove * (profile.MaxSpeed * speedMultiplier);

            Vector3 desiredFacing = Flatten(command.DesiredFacingDirection);

            if (desiredFacing.sqrMagnitude > _config.FacingDirectionDeadZone * _config.FacingDirectionDeadZone)
                desiredFacing.Normalize();
            else
                desiredFacing = Vector3.zero;

            Vector3 previousVelocity = _velocity;

            bool isPivoting = ShouldPivot(previousVelocity, desiredVelocity, hasMoveInput);

            Vector3 simulatedVelocity = ResolveVelocity(
                previousVelocity,
                desiredVelocity,
                hasMoveInput,
                isPivoting,
                profile,
                deltaTime);

            UpdateRotation(desiredFacing, profile, deltaTime);

            Vector3 previousPosition = transform.position;

            _characterController.Move(simulatedVelocity * deltaTime);

            // CharacterController 可能因为碰墙而没有真正走完 commanded displacement。
            // MotionState 应记录“真实运动”，而不是记录我们希望它运动多少。
            Vector3 actualDisplacement = Flatten(transform.position - previousPosition);
            _velocity = actualDisplacement / deltaTime;

            Vector3 acceleration = (_velocity - previousVelocity) / deltaTime;

            UpdateMotionState(
                desiredVelocity,
                desiredFacing,
                acceleration,
                hasMoveInput,
                isPivoting,
                profile);
        }

        private float ResolveDirectionalSpeedMultiplier(Vector3 worldMove, PlayerMovementProfile profile)
        {
            if (worldMove.sqrMagnitude <= 0.000001f)
                return 1f;

            Vector3 facing = Flatten(transform.forward);

            if (facing.sqrMagnitude <= 0.000001f)
                return 1f;

            float directionDot = Vector3.Dot(facing.normalized, worldMove.normalized);
            float backwardDotThreshold = Mathf.Cos(profile.BackwardAngleThreshold * Mathf.Deg2Rad);

            return directionDot <= backwardDotThreshold
                ? profile.BackwardSpeedMultiplier
                : 1f;
        }

        /// <summary>
        /// 清空所有运动惯性。
        /// 死亡、硬控结束、换层前后等场景可以使用。
        /// </summary>
        public void ResetMotion()
        {
            _velocity = Vector3.zero;
            _angularSpeed = 0f;
            _phase = MotionPhase.Idle;

            MotionState = new PlayerMotionState
            {
                Position = transform.position,
                Velocity = Vector3.zero,
                DesiredVelocity = Vector3.zero,
                Acceleration = Vector3.zero,
                MoveDirection = Vector3.zero,
                FacingDirection = Flatten(transform.forward).normalized,
                DesiredFacingDirection = Vector3.zero,
                LocalVelocity = Vector3.zero,
                Speed = 0f,
                NormalizedSpeed = 0f,
                AngularSpeed = 0f,
                Phase = MotionPhase.Idle,
                HasMoveInput = false,
                IsMoving = false,
                IsPivoting = false,
            };
        }

        /// <summary>
        /// 瞬移并清空 Motor 惯性。
        /// CharacterController 激活期间直接大距离修改 Transform 容易留下不一致，
        /// 因此这里统一暂时关闭 Controller。
        /// </summary>
        public void Warp(Vector3 worldPosition, Quaternion worldRotation)
        {
            bool wasEnabled = _characterController.enabled;

            if (wasEnabled)
                _characterController.enabled = false;

            transform.SetPositionAndRotation(worldPosition, worldRotation);

            if (wasEnabled)
                _characterController.enabled = true;

            ResetMotion();
        }

        private Vector3 ResolveVelocity(
            Vector3 currentVelocity,
            Vector3 desiredVelocity,
            bool hasMoveInput,
            bool isPivoting,
            PlayerMovementProfile profile,
            float deltaTime)
        {
            if (!hasMoveInput)
                return Vector3.MoveTowards(currentVelocity, Vector3.zero, profile.Deceleration * deltaTime);

            // 强反向时先真正刹到接近 0。
            // 不直接朝反方向加速，因此会自然形成一段前冲制动距离。
            if (isPivoting)
                return Vector3.MoveTowards(currentVelocity, Vector3.zero, profile.PivotBrakeAcceleration * deltaTime);

            float currentSpeed = currentVelocity.magnitude;
            float desiredSpeed = desiredVelocity.magnitude;

            // 例如 Sprint → Aim 时，目标最大速度突然降低，
            // 此时应该使用 Deceleration，而不是普通 Acceleration 慢慢磨下来。
            float response = currentSpeed > desiredSpeed + 0.01f
                ? profile.Deceleration
                : profile.Acceleration;

            return Vector3.MoveTowards(currentVelocity, desiredVelocity, response * deltaTime);
        }

        private bool ShouldPivot(Vector3 currentVelocity, Vector3 desiredVelocity, bool hasMoveInput)
        {
            if (!hasMoveInput)
                return false;

            float currentSpeed = currentVelocity.magnitude;

            if (currentSpeed < _config.PivotMinSpeed)
                return false;

            if (desiredVelocity.sqrMagnitude <= 0.000001f)
                return false;
            //点乘判定当前速度与目标速度方向是否相反，若点乘小于阈值则认为是反向输入
            float directionDot = Vector3.Dot(currentVelocity.normalized, desiredVelocity.normalized);
            return directionDot <= _config.PivotDirectionDotThreshold;
        }

        private void UpdateRotation(Vector3 desiredFacing, PlayerMovementProfile profile, float deltaTime)
        {
            float currentYaw = transform.eulerAngles.y;

            // 没有目标朝向时只让剩余角速度自然衰减，不产生新的旋转意图。
            if (desiredFacing.sqrMagnitude <= 0.000001f)
            {
                _angularSpeed = Mathf.MoveTowards(_angularSpeed, 0f, profile.RotationDeceleration * deltaTime);

                if (Mathf.Abs(_angularSpeed) > 0.01f)
                    transform.rotation = Quaternion.Euler(0f, currentYaw + _angularSpeed * deltaTime, 0f);

                return;
            }

            float targetYaw = Mathf.Atan2(desiredFacing.x, desiredFacing.z) * Mathf.Rad2Deg;
            float angleDelta = Mathf.DeltaAngle(currentYaw, targetYaw);

            if (Mathf.Abs(angleDelta) <= _config.RotationSnapAngle && Mathf.Abs(_angularSpeed) <= 1f)
            {
                transform.rotation = Quaternion.Euler(0f, targetYaw, 0f);
                _angularSpeed = 0f;
                return;
            }

            float deceleration = Mathf.Max(profile.RotationDeceleration, 0.001f);

            // 以当前角速度继续制动所需的角距离。
            // 进入这段距离后提前减速，避免 RotateTowards 那种恒定角速度突然停住。
            float stoppingAngle = (_angularSpeed * _angularSpeed) / (2f * deceleration);

            float targetAngularSpeed = Mathf.Abs(angleDelta) <= stoppingAngle
                ? 0f
                : Mathf.Sign(angleDelta) * profile.MaxRotationSpeed;

            float response = Mathf.Approximately(targetAngularSpeed, 0f)
                ? profile.RotationDeceleration
                : profile.RotationAcceleration;

            _angularSpeed = Mathf.MoveTowards(_angularSpeed, targetAngularSpeed, response * deltaTime);

            float rotationStep = _angularSpeed * deltaTime;

            // 当前旋转方向已经朝着目标，而且这一帧会越过目标时直接落在目标点。
            if (Mathf.Sign(rotationStep) == Mathf.Sign(angleDelta) && Mathf.Abs(rotationStep) >= Mathf.Abs(angleDelta))
            {
                transform.rotation = Quaternion.Euler(0f, targetYaw, 0f);
                _angularSpeed = 0f;
                return;
            }

            transform.rotation = Quaternion.Euler(0f, currentYaw + rotationStep, 0f);
        }

        private void UpdateMotionState(
            Vector3 desiredVelocity,
            Vector3 desiredFacing,
            Vector3 acceleration,
            bool hasMoveInput,
            bool isPivoting,
            PlayerMovementProfile profile)
        {
            float speed = _velocity.magnitude;
            float normalizedSpeed = profile.MaxSpeed <= 0.001f ? 0f : Mathf.Clamp01(speed / profile.MaxSpeed);

            _phase = ResolveMotionPhase(hasMoveInput, isPivoting, speed, normalizedSpeed);

            MotionState = new PlayerMotionState
            {
                Position = transform.position,
                Velocity = _velocity,
                DesiredVelocity = desiredVelocity,
                Acceleration = acceleration,
                MoveDirection = speed > _config.StopSpeedThreshold ? _velocity / speed : Vector3.zero,
                FacingDirection = Flatten(transform.forward).normalized,
                DesiredFacingDirection = desiredFacing,
                LocalVelocity = transform.InverseTransformDirection(_velocity),
                Speed = speed,
                NormalizedSpeed = normalizedSpeed,
                AngularSpeed = _angularSpeed,
                Phase = _phase,
                HasMoveInput = hasMoveInput,
                IsMoving = speed > _config.StopSpeedThreshold,
                IsPivoting = isPivoting,
            };
        }

        private MotionPhase ResolveMotionPhase(bool hasMoveInput, bool isPivoting, float speed, float normalizedSpeed)
        {
            if (isPivoting)
                return MotionPhase.Pivot;

            if (!hasMoveInput)
                return speed <= _config.StopSpeedThreshold ? MotionPhase.Idle : MotionPhase.Stop;

            if (speed <= _config.StopSpeedThreshold)
                return MotionPhase.Start;

            if ((_phase == MotionPhase.Idle || _phase == MotionPhase.Start || _phase == MotionPhase.Stop) &&
                normalizedSpeed < _config.StartToMoveNormalizedSpeed)
                return MotionPhase.Start;

            return MotionPhase.Move;
        }

        private static Vector3 Flatten(Vector3 value)
        {
            value.y = 0f;
            return value;
        }
    }
}
