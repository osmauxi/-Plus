using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Movement
{
    /// <summary>
    /// 状态模拟下层：根据本 Tick 的运动命令和当前移动配置，推进一次位置、速度与朝向 
    /// 只负责运动学与 IPlayerCharacterBody（角色实体接口）位移，不决定玩家是自由移动、瞄准还是冲刺，
    /// 这些高层状态由 PlayerLocomotionController 先解析为 PlayerMovementProfile 
    /// 该类是普通 C# 对象，由 PlayerSyncController 组合根创建，再交给上层 PlayerLocomotionController 唯一持有 
    /// </summary>
    public sealed class PlayerMotor
    {
        private const float PivotBoostTimeEpsilon = 0.0001f;

        // 实际提供位置、旋转和碰撞约束位移的角色实体适配器 
        private readonly IPlayerCharacterBody _body;

        // 自由、瞄准、冲刺三类运动响应，以及停止、转身等底层判定参数 
        private readonly PlayerMovementConfig _config;

        // 上一 Tick 实际位移反算出的水平速度；用于下一 Tick 的加速、减速和转向判定 
        private Vector3 _velocity;
        // 当前绕 Y 轴的有符号角速度（度/秒）；正值向目标角度正方向旋转 
        private float _angularSpeed;
        // 最近一次模拟得到的运动阶段；用于维持 Start/Move/Stop/Pivot 的连续表现语义 
        private MotionPhase _phase = MotionPhase.Idle;
        // Pivot 进入瞬间锁存的角色局部目标方向；阶段结束前保持不变，供各端选择同一动画 
        private PlayerPivotDirection _pivotDirection = PlayerPivotDirection.None;
        // Pivot 进入边沿从当时的 Move/Sprint Profile 锁存；切换 Shift 不会改变本次爆发 
        private float _pivotBoostTimeRemaining;
        private float _pivotBoostSpeedBonus;

        // 供中层根据控制模式选择 Free/Aim/Sprint 移动参数 
        public PlayerMovementConfig Config => _config;
        // 最近一次 Simulate 或 Restore 后生成的只读运动表现事实 
        public PlayerMotionState MotionState { get; private set; }

        /// <summary>注入角色实体和移动参数，校验配置并建立静止运动状态 </summary>
        public PlayerMotor(IPlayerCharacterBody body, PlayerMovementConfig config)
        {
            _body = body ?? throw new ArgumentNullException(nameof(body));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _config.Validate();
            ResetMotion();
        }

        /// <summary>
        /// 捕获回滚真正需要的 Motor 状态：位置、旋转、实际速度、角速度和短时 Pivot 爆发 
        /// MotionState 中的其余表现字段可在下一次模拟时重新派生，不进入权威快照 
        /// </summary>
        public PlayerMotorRuntimeState CaptureRuntimeState()
        {
            return new PlayerMotorRuntimeState(
                _body.Position,
                _body.Rotation,
                _velocity,
                _angularSpeed,
                _pivotDirection,
                _pivotBoostTimeRemaining,
                _pivotBoostSpeedBonus);
        }

        /// <summary>
        /// 恢复权威或历史 Motor 状态，并重建一份与恢复结果一致的 MotionState 
        /// 临时关闭 CharacterController 是为了避免直接写 Transform 时与 Controller 内部位置状态冲突，
        /// 不是为了“停止物理模拟” 
        /// </summary>
        public void RestoreRuntimeState(in PlayerMotorRuntimeState state)
        {
            // 角色实体适配器负责处理 CharacterController 与 Transform 的安全恢复顺序 
            _body.SetPose(state.Position, state.Rotation);

            _velocity = Flatten(state.Velocity);
            _angularSpeed = state.AngularSpeed;

            _pivotDirection = state.PivotDirection;
            _pivotBoostTimeRemaining = Mathf.Max(0f, state.PivotBoostTimeRemaining);
            _pivotBoostSpeedBonus = Mathf.Max(0f, state.PivotBoostSpeedBonus);
            if (_pivotBoostTimeRemaining <= PivotBoostTimeEpsilon || _pivotBoostSpeedBonus <= 0f)
                ClearPivotBoost();
            // PivotDirection=None 时沿用原有的速度恢复策略；非 None 则恢复 Pivot 阶段，保证 Replay 的方向锁存一致 
            _phase = _pivotDirection != PlayerPivotDirection.None
                ? MotionPhase.Pivot
                : _velocity.sqrMagnitude <= _config.StopSpeedThreshold * _config.StopSpeedThreshold
                    ? MotionPhase.Idle
                    : MotionPhase.Move;

            float speed = _velocity.magnitude;

            MotionState = new PlayerMotionState
            {
                Position = _body.Position,
                Velocity = _velocity,
                DesiredVelocity = Vector3.zero,
                Acceleration = Vector3.zero,
                MoveDirection = speed > _config.StopSpeedThreshold ? _velocity / speed : Vector3.zero,
                FacingDirection = Flatten(_body.Forward).normalized,
                DesiredFacingDirection = Vector3.zero,
                LocalVelocity = _body.InverseTransformDirection(_velocity),
                Speed = speed,
                NormalizedSpeed = 0f,
                AngularSpeed = _angularSpeed,
                Phase = _phase,
                PivotDirection = _pivotDirection,
                PivotBoostTimeRemaining = _pivotBoostTimeRemaining,
                PivotBoostSpeedBonus = _pivotBoostSpeedBonus,
                HasMoveInput = false,
                IsMoving = speed > _config.StopSpeedThreshold,
                IsPivoting = _pivotDirection != PlayerPivotDirection.None,
            };
        }

        /// <summary>
        /// 执行一个固定模拟 Tick 的运动计算 
        /// command 只描述世界移动和目标朝向；profile 描述当前控制模式允许的速度、加减速和旋转响应 
        /// Motor 不处理生命、瞄准、冲刺和体力规则，但会维护由运动结果派生的 MotionPhase/PlayerMotionState 
        /// </summary>
        public void Simulate(in PlayerMotionCommand command, in PlayerMovementProfile profile, float deltaTime)
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
            bool enteringPivot =
                isPivoting &&
                (_phase != MotionPhase.Pivot || _pivotDirection == PlayerPivotDirection.None);

            // 只在 Pivot 进入边沿选择方向；持续制动阶段不能随 Root 转向或速度缩小而切换动画 
            if (isPivoting)
            {
                if (enteringPivot)
                {
                    _pivotDirection = ResolvePivotDirection(desiredVelocity);
                    TryStartPivotBoost(profile);
                }
            }
            else
            {
                _pivotDirection = PlayerPivotDirection.None;
            }

            if (!hasMoveInput)
                ClearPivotBoost();

            bool isPivotBoosting =
                _pivotBoostTimeRemaining > PivotBoostTimeEpsilon &&
                _pivotBoostSpeedBonus > 0f &&
                worldMove.sqrMagnitude > 0.000001f;
            Vector3 effectiveDesiredVelocity = isPivotBoosting
                ? worldMove.normalized *
                  ((profile.MaxSpeed + _pivotBoostSpeedBonus) * inputMagnitude)
                : desiredVelocity;

            Vector3 simulatedVelocity = ResolveVelocity(
                previousVelocity,
                effectiveDesiredVelocity,
                hasMoveInput,
                isPivoting,
                isPivotBoosting,
                profile,
                deltaTime);

            UpdateRotation(
                desiredFacing,
                simulatedVelocity.magnitude,
                profile,
                deltaTime);

            Vector3 previousPosition = _body.Position;

            _body.Move(simulatedVelocity * deltaTime);

            // CharacterController 可能因为碰墙而没有真正走完 commanded displacement 
            // MotionState 应记录“真实运动”，而不是记录我们希望它运动多少 
            Vector3 actualDisplacement = Flatten(_body.Position - previousPosition);
            _velocity = actualDisplacement / deltaTime;

            Vector3 acceleration = (_velocity - previousVelocity) / deltaTime;

            TickPivotBoost(deltaTime);

            UpdateMotionState(
                effectiveDesiredVelocity,
                desiredFacing,
                acceleration,
                hasMoveInput,
                isPivoting,
                profile);
        }

        /// <summary>
        /// 根据当前朝向与移动方向的夹角决定是否应用后退速度倍率 
        /// </summary>
        private float ResolveDirectionalSpeedMultiplier(Vector3 worldMove, PlayerMovementProfile profile)
        {
            if (worldMove.sqrMagnitude <= 0.000001f)
                return 1f;

            Vector3 facing = Flatten(_body.Forward);

            if (facing.sqrMagnitude <= 0.000001f)
                return 1f;

            float directionDot = Vector3.Dot(facing.normalized, worldMove.normalized);
            float backwardDotThreshold = Mathf.Cos(profile.BackwardAngleThreshold * Mathf.Deg2Rad);

            return directionDot <= backwardDotThreshold
                ? profile.BackwardSpeedMultiplier
                : 1f;
        }

        /// <summary>
        /// 清空所有运动惯性 
        /// 死亡、硬控结束、换层前后等场景可以使用 
        /// </summary>
        public void ResetMotion()
        {
            _velocity = Vector3.zero;
            _angularSpeed = 0f;
            _phase = MotionPhase.Idle;
            _pivotDirection = PlayerPivotDirection.None;
            ClearPivotBoost();

            MotionState = new PlayerMotionState
            {
                Position = _body.Position,
                Velocity = Vector3.zero,
                DesiredVelocity = Vector3.zero,
                Acceleration = Vector3.zero,
                MoveDirection = Vector3.zero,
                FacingDirection = Flatten(_body.Forward).normalized,
                DesiredFacingDirection = Vector3.zero,
                LocalVelocity = Vector3.zero,
                Speed = 0f,
                NormalizedSpeed = 0f,
                AngularSpeed = 0f,
                Phase = MotionPhase.Idle,
                PivotDirection = PlayerPivotDirection.None,
                PivotBoostTimeRemaining = 0f,
                PivotBoostSpeedBonus = 0f,
                HasMoveInput = false,
                IsMoving = false,
                IsPivoting = false,
            };
        }

        /// <summary>
        /// 瞬移并清空 Motor 惯性 
        /// 角色实体适配器负责解决 CharacterController 激活期间直接修改 Transform 的一致性问题 
        /// </summary>
        public void Warp(Vector3 worldPosition, Quaternion worldRotation)
        {
            _body.SetPose(worldPosition, worldRotation);
            ResetMotion();
        }

        /// <summary>
        /// 在当前速度与目标速度之间推进一步；无输入时减速，强反向时先制动，其他情况按加/减速响应 
        /// </summary>
        private Vector3 ResolveVelocity(
            Vector3 currentVelocity,
            Vector3 desiredVelocity,
            bool hasMoveInput,
            bool isPivoting,
            bool isPivotBoosting,
            PlayerMovementProfile profile,
            float deltaTime)
        {
            if (!hasMoveInput)
                return Vector3.MoveTowards(currentVelocity, Vector3.zero, profile.Deceleration * deltaTime);

            // 爆发窗口覆盖普通 Pivot 制动：按剩余时间反推最低响应，确保短窗口内确实能达到
            // “当前模式最大速度 + 锁存加成”，而不是只提高一个永远追不到的速度上限 
            if (isPivotBoosting)
            {
                float timeToTarget = Mathf.Max(_pivotBoostTimeRemaining, deltaTime);
                float burstResponse = Mathf.Max(
                    profile.Acceleration,
                    Vector3.Distance(currentVelocity, desiredVelocity) / timeToTarget);
                return Vector3.MoveTowards(
                    currentVelocity,
                    desiredVelocity,
                    burstResponse * deltaTime);
            }

            // 强反向时先真正刹到接近 0 
            // 不直接朝反方向加速，因此会自然形成一段前冲制动距离 
            if (isPivoting)
                return Vector3.MoveTowards(currentVelocity, Vector3.zero, profile.PivotBrakeAcceleration * deltaTime);

            float currentSpeed = currentVelocity.magnitude;
            float desiredSpeed = desiredVelocity.magnitude;

            // 例如 Sprint → Aim 时，目标最大速度突然降低，
            // 此时应该使用 Deceleration，而不是普通 Acceleration 慢慢磨下来 
            float response = currentSpeed > desiredSpeed + 0.01f
                ? profile.Deceleration
                : profile.Acceleration;

            // 直线加速和弧线转向使用不同响应 旧逻辑只按速度差选择一个加速度，
            // 在保持 W 的同时切入 A/D 时，横向速度建立过慢，会形成明显的冰面式外抛轨迹 
            // 这里按方向夹角渐增响应：同向不变，90 度达到完整倍率；强反向仍由 Pivot 分支处理 
            if (currentSpeed > _config.StopSpeedThreshold && desiredSpeed > 0.001f)
            {
                float directionAngle = Vector3.Angle(currentVelocity, desiredVelocity);
                float directionWeight = Mathf.Clamp01(directionAngle / 90f);
                response *= Mathf.Lerp(
                    1f,
                    profile.DirectionChangeAccelerationMultiplier,
                    directionWeight);
            }

            return Vector3.MoveTowards(currentVelocity, desiredVelocity, response * deltaTime);
        }

        private void TryStartPivotBoost(PlayerMovementProfile profile)
        {
            // 一次爆发未结束前不允许连续方向抖动刷新计时，防止通过快速反复输入无限续杯 
            if (_pivotBoostTimeRemaining > PivotBoostTimeEpsilon)
                return;

            if (profile.PivotBoostDuration <= 0f || profile.PivotSpeedBonus <= 0f)
            {
                ClearPivotBoost();
                return;
            }

            _pivotBoostTimeRemaining = profile.PivotBoostDuration;
            _pivotBoostSpeedBonus = profile.PivotSpeedBonus;
        }

        private void TickPivotBoost(float deltaTime)
        {
            if (_pivotBoostTimeRemaining <= PivotBoostTimeEpsilon)
                return;

            _pivotBoostTimeRemaining = Mathf.Max(0f, _pivotBoostTimeRemaining - deltaTime);
            if (_pivotBoostTimeRemaining <= PivotBoostTimeEpsilon)
                ClearPivotBoost();
        }

        private void ClearPivotBoost()
        {
            _pivotBoostTimeRemaining = 0f;
            _pivotBoostSpeedBonus = 0f;
        }

        /// <summary>
        /// 判断当前高速运动方向与目标运动方向是否足够相反，需要进入 Pivot 制动阶段 
        /// </summary>
        private bool ShouldPivot(Vector3 currentVelocity, Vector3 desiredVelocity, bool hasMoveInput)
        {
            if (!hasMoveInput)
                return false;

            // Start 阶段允许玩家自由修正起步方向 只靠绝对速度阈值无法同时覆盖
            // Free 与 Sprint 的不同最大速度，因此在进入稳定 Move 前明确禁止 Pivot 
            if (_phase == MotionPhase.Start)
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

        /// <summary>
        /// 把 Pivot 的目标世界方向投影到进入瞬间的角色局部坐标，并归类为四个主方向 
        /// 使用锁存方向而不是每帧 LocalVelocity，可避免 Root 正在旋转时 BlendTree 跨象限跳变 
        /// </summary>
        private PlayerPivotDirection ResolvePivotDirection(Vector3 desiredVelocity)
        {
            if (desiredVelocity.sqrMagnitude <= 0.000001f)
                return PlayerPivotDirection.None;

            Vector3 localDirection = _body.InverseTransformDirection(desiredVelocity.normalized);
            if (Mathf.Abs(localDirection.x) > Mathf.Abs(localDirection.z))
                return localDirection.x >= 0f
                    ? PlayerPivotDirection.Right
                    : PlayerPivotDirection.Left;

            return localDirection.z >= 0f
                ? PlayerPivotDirection.Forward
                : PlayerPivotDirection.Backward;
        }

        /// <summary>
        /// 以角加速度、角减速度和最大角速度推进水平朝向，并在接近目标时避免越过目标角度 
        /// </summary>
        private void UpdateRotation(
            Vector3 desiredFacing,
            float planarSpeed,
            PlayerMovementProfile profile,
            float deltaTime)
        {
            float currentYaw = _body.Rotation.eulerAngles.y;

            // 静止/低速时转身不需要保留高速奔跑的重量感 按速度连续衰减响应倍率，
            // 同时强化启动与刹停，特别是快速 A→S/W→D 时可更快消除上一方向的角速度 
            float lowSpeedWeight = 1f - Mathf.Clamp01(
                planarSpeed / _config.LowSpeedRotationBoostThreshold);
            float rotationResponseMultiplier = Mathf.Lerp(
                1f,
                _config.StationaryRotationResponseMultiplier,
                lowSpeedWeight);
            float rotationAcceleration =
                profile.RotationAcceleration * rotationResponseMultiplier;
            float rotationDeceleration =
                profile.RotationDeceleration * rotationResponseMultiplier;
            float maxRotationSpeed =
                profile.MaxRotationSpeed * Mathf.Lerp(
                    1f,
                    _config.StationaryMaxRotationSpeedMultiplier,
                    lowSpeedWeight);

            // 没有目标朝向时只让剩余角速度自然衰减，不产生新的旋转意图 
            if (desiredFacing.sqrMagnitude <= 0.000001f)
            {
                _angularSpeed = Mathf.MoveTowards(_angularSpeed, 0f, rotationDeceleration * deltaTime);

                if (Mathf.Abs(_angularSpeed) > 0.01f)
                    _body.Rotation = Quaternion.Euler(0f, currentYaw + _angularSpeed * deltaTime, 0f);

                return;
            }

            float targetYaw = Mathf.Atan2(desiredFacing.x, desiredFacing.z) * Mathf.Rad2Deg;
            float angleDelta = Mathf.DeltaAngle(currentYaw, targetYaw);

            if (Mathf.Abs(angleDelta) <= _config.RotationSnapAngle && Mathf.Abs(_angularSpeed) <= 1f)
            {
                _body.Rotation = Quaternion.Euler(0f, targetYaw, 0f);
                _angularSpeed = 0f;
                return;
            }

            float deceleration = Mathf.Max(rotationDeceleration, 0.001f);

            // 以当前角速度继续制动所需的角距离 
            // 进入这段距离后提前减速，避免 RotateTowards 那种恒定角速度突然停住 
            float stoppingAngle = (_angularSpeed * _angularSpeed) / (2f * deceleration);

            float targetAngularSpeed = Mathf.Abs(angleDelta) <= stoppingAngle
                ? 0f
                : Mathf.Sign(angleDelta) * maxRotationSpeed;

            float response = Mathf.Approximately(targetAngularSpeed, 0f)
                ? rotationDeceleration
                : rotationAcceleration;

            _angularSpeed = Mathf.MoveTowards(_angularSpeed, targetAngularSpeed, response * deltaTime);

            float rotationStep = _angularSpeed * deltaTime;

            // 当前旋转方向已经朝着目标，而且本 Tick 会越过目标时直接落在目标角度 
            if (Mathf.Sign(rotationStep) == Mathf.Sign(angleDelta) && Mathf.Abs(rotationStep) >= Mathf.Abs(angleDelta))
            {
                _body.Rotation = Quaternion.Euler(0f, targetYaw, 0f);
                _angularSpeed = 0f;
                return;
            }

            _body.Rotation = Quaternion.Euler(0f, currentYaw + rotationStep, 0f);
        }

        /// <summary>
        /// 根据本 Tick 的真实位移、目标值与输入状态，生成供动画和调试读取的 PlayerMotionState 
        /// </summary>
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
                Position = _body.Position,
                Velocity = _velocity,
                DesiredVelocity = desiredVelocity,
                Acceleration = acceleration,
                MoveDirection = speed > _config.StopSpeedThreshold ? _velocity / speed : Vector3.zero,
                FacingDirection = Flatten(_body.Forward).normalized,
                DesiredFacingDirection = desiredFacing,
                LocalVelocity = _body.InverseTransformDirection(_velocity),
                Speed = speed,
                NormalizedSpeed = normalizedSpeed,
                AngularSpeed = _angularSpeed,
                Phase = _phase,
                PivotDirection = _pivotDirection,
                PivotBoostTimeRemaining = _pivotBoostTimeRemaining,
                PivotBoostSpeedBonus = _pivotBoostSpeedBonus,
                HasMoveInput = hasMoveInput,
                IsMoving = speed > _config.StopSpeedThreshold,
                IsPivoting = isPivoting,
            };
        }

        /// <summary>
        /// 把输入、速度与 Pivot 判定归类为 Idle/Start/Move/Stop/Pivot 阶段 
        /// </summary>
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

        /// <summary>清除垂直分量，保证当前地面移动模型只在 XZ 平面计算 </summary>
        private static Vector3 Flatten(Vector3 value)
        {
            value.y = 0f;
            return value;
        }
    }
}
