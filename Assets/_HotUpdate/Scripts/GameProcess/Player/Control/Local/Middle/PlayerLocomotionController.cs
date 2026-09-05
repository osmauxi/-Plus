using System;
using ProjectGame.HotFix.Gameplay.Player.Stamina;
using ProjectGame.HotFix.Gameplay.Player.State;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Movement
{
    /// <summary>
    /// 状态模拟中层：消费本 Tick 的 PlayerLocomotionInput，更新生命/移动模式与体力状态，
    /// 再把高层状态解析成 PlayerMovementProfile 和 PlayerMotionCommand 交给 PlayerMotor 
    /// 输入可能来自本地预测或服务器权威模拟，因此这里不读取设备，也不处理网络包 
    /// 该类是普通 C# 对象，由 PlayerSyncController 组合根创建，再交给 PlayerSimulation 唯一持有 
    /// </summary>
    public sealed class PlayerLocomotionController
    {
        // 执行实际位移、速度和旋转计算的下层 Motor 
        private readonly PlayerMotor _motor;

        // 冲刺消耗、恢复延迟、恢复速度和耗尽解除比例等体力规则 
        private readonly PlayerStaminaConfig _staminaConfig;
        // 无隐藏状态的 HFSM Resolver；所有持续状态都保存在下面两个可序列化结构体中 
        private readonly PlayerStateMachine _stateMachine;

        // 当前生命、受击、战斗与 Locomotion 状态；决定本 Tick 哪些下层规则可运行 
        private PlayerControlState _controlState;
        // 当前体力、恢复延迟与耗尽锁定状态；必须进入回滚快照 
        private PlayerStaminaState _staminaState;
        // 受击、射击冷却、换弹计时与动作序号；必须进入回滚快照 
        private PlayerActionRuntimeState _actionState;
        // 最近一次输入的世界平面瞄准方向；即使 Root 未转向也要保留给同步和上半身表现 
        private Vector3 _aimDirection;
        // Aim Root 跟随的迟滞运行状态；会进入 SimulationState 以保证回滚重演一致 
        private bool _isAimBodyTurning;

        // 暴露给同步层捕获的当前高层控制状态 
        public PlayerControlState ControlState => _controlState;
        // 暴露给同步层捕获的当前体力状态 
        public PlayerStaminaState StaminaState => _staminaState;
        // 暴露给快照/回滚的动作计时与序号状态 
        public PlayerActionRuntimeState ActionState => _actionState;
        // 透传 Motor 最近一次模拟得到的运动表现状态 
        public PlayerMotionState MotionState => _motor.MotionState;
        /// <summary>最近输入的世界平面 Aim 方向；Root 不跟随时仍可供上半身偏转使用 </summary>
        public Vector3 AimDirection => _aimDirection;
        /// <summary>Root 当前是否处于 Start/Stop 迟滞区间中的持续跟随阶段 </summary>
        public bool IsAimBodyTurning => _isAimBodyTurning;
        // 当前体力相对最大体力的 0~1 比例，供 UI/表现读取 
        public float NormalizedStamina => _staminaState.Normalized(_staminaConfig.MaxStamina);

        /// <summary>注入下层 Motor（运动器）和体力参数，并创建默认 Alive/Free（存活/自由移动）状态 </summary>
        public PlayerLocomotionController(
            PlayerMotor motor,
            PlayerStaminaConfig staminaConfig,
            PlayerActionConfig actionConfig)
        {
            _motor = motor ?? throw new ArgumentNullException(nameof(motor));
            _staminaConfig = staminaConfig ?? throw new ArgumentNullException(nameof(staminaConfig));
            _stateMachine = new PlayerStateMachine(
                actionConfig ?? throw new ArgumentNullException(nameof(actionConfig)));

            _staminaConfig.Validate();
            ResetRuntimeState();
        }

        /// <summary>
        /// 回滚时恢复影响后续模拟的高层控制状态、体力状态和 Motor 运行时状态 
        /// 恢复顺序固定为高层状态在前、运动状态在后 
        /// </summary>
        public void RestoreSimulationState(
            in PlayerControlState controlState,
            in PlayerStaminaState staminaState,
            in PlayerActionRuntimeState actionState,
            bool isAimBodyTurning,
            in PlayerMotorRuntimeState motorState)
        {
            _controlState = controlState;
            _staminaState = staminaState;
            _actionState = actionState;
            _isAimBodyTurning = isAimBodyTurning;
            _motor.RestoreRuntimeState(motorState);
        }

        /// <summary>捕获位置、旋转、速度和角速度，供上层同步模拟组成完整快照 </summary>
        public PlayerMotorRuntimeState CaptureMotorRuntimeState() => _motor.CaptureRuntimeState();

        /// <summary>
        /// 推进一个固定模拟 Tick：先解析控制模式并更新体力，再选择移动配置并驱动 Motor 
        /// 死亡状态不会产生位移，但当前实现仍允许体力恢复计时继续推进 
        /// </summary>
        public void Simulate(in PlayerLocomotionInput input, float deltaTime)
        {
            if (deltaTime <= 0f)
                return;

            // 使用平方长度比较，避免每 Tick 求平方根；阈值也必须平方后再比较 
            bool hasMoveInput = input.WorldMove.sqrMagnitude >
                                _motor.Config.MoveInputDeadZone * _motor.Config.MoveInputDeadZone;

            _aimDirection = input.AimDirection;

            // IsExhausted 是迟滞锁定：体力刚恢复到正数仍不能冲刺，必须达到配置的解除比例 
            bool canSprint = !_staminaState.IsExhausted && _staminaState.Current > 0f;
            PlayerStateInput stateInput = new(hasMoveInput,input.AimHeld,input.SprintHeld,input.FireHeld,input.ReloadRequestSequence);

            _stateMachine.Simulate(ref _controlState,ref _actionState,stateInput,canSprint,deltaTime);

            if (_controlState.IsDead)
            {
                _isAimBodyTurning = false;
                _motor.ResetMotion();
                TickStamina(false, deltaTime);
                return;
            }

            bool isSprinting = _controlState.IsSprinting;
            TickStamina(isSprinting, deltaTime);

            PlayerMovementProfile profile = ResolveMovementProfile(_controlState.LocomotionMode);
            bool canUseLocomotion = _controlState.CanUseLocomotion;
            Vector3 worldMove = canUseLocomotion ? input.WorldMove : Vector3.zero;
            Vector3 desiredFacing = canUseLocomotion? ResolveDesiredFacing(input) : Vector3.zero;

            if (!canUseLocomotion)
                _isAimBodyTurning = false;

            _motor.Simulate(new PlayerMotionCommand(worldMove, desiredFacing), profile, deltaTime);
        }

        /// <summary>
        /// 修改玩家顶层生命状态 
        /// Death 不属于 Locomotion，因此由顶层状态统一接管 
        /// </summary>
        public void SetLifeState(PlayerLifeState lifeState)
        {
            _stateMachine.SetLifeState(ref _controlState, ref _actionState, lifeState);

            if (lifeState != PlayerLifeState.Dead)
                return;

            _isAimBodyTurning = false;
            _motor.ResetMotion();
        }

        /// <summary>
        /// 进入或刷新受击状态；调用方必须在权威 Gameplay 侧决定这次受击是否成立 
        /// </summary>
        public bool ApplyHit(float tickDeltaTime)
        {
            return _stateMachine.ApplyHit(
                ref _controlState,
                ref _actionState,
                tickDeltaTime);
        }

        /// <summary>
        /// 重建默认 Alive/Free 控制状态、满体力状态，并清空 Motor 惯性 
        /// 用于初始化或完整运行时重置，不等同于只恢复一份网络快照 
        /// </summary>
        public void ResetRuntimeState()
        {
            _controlState = PlayerControlState.CreateDefault();
            _staminaState = new PlayerStaminaState
            {
                Current = _staminaConfig.MaxStamina,
                RecoveryDelayRemaining = 0f,
                IsExhausted = false,
            };
            _actionState = default;
            _aimDirection = Vector3.zero;
            _isAimBodyTurning = false;
            _motor.ResetMotion();
        }

        /// <summary>把角色实体移动到指定位置和旋转，并清空全部运动惯性 </summary>
        public void Warp(Vector3 worldPosition, Quaternion worldRotation)
        {
            _motor.Warp(worldPosition, worldRotation);
        }

        /// <summary>
        /// 冲刺时消耗体力并刷新恢复延迟；非冲刺时先等待延迟，再恢复并解除耗尽锁定 
        /// </summary>
        private void TickStamina(bool isSprinting, float deltaTime)
        {
            if (isSprinting)
            {
                _staminaState.Current = Mathf.Max(
                    0f,
                    _staminaState.Current - _staminaConfig.SprintDrainPerSecond * deltaTime);
                _staminaState.RecoveryDelayRemaining = _staminaConfig.RecoveryDelay;

                if (_staminaState.Current <= 0f)
                    _staminaState.IsExhausted = true;

                return;
            }

            if (_staminaState.RecoveryDelayRemaining > 0f)
            {
                _staminaState.RecoveryDelayRemaining = Mathf.Max(
                    0f,
                    _staminaState.RecoveryDelayRemaining - deltaTime);
                return;
            }

            _staminaState.Current = Mathf.Min(
                _staminaConfig.MaxStamina,
                _staminaState.Current + _staminaConfig.RecoveryPerSecond * deltaTime);

            // 比例阈值形成“耗尽后必须恢复一段”的迟滞，避免 0 附近每 Tick 反复启停冲刺 
            float exhaustionReleaseThreshold =
                _staminaConfig.MaxStamina * _staminaConfig.ExhaustedRecoveryRatio;

            if (_staminaState.IsExhausted && _staminaState.Current >= exhaustionReleaseThreshold)
                _staminaState.IsExhausted = false;
        }

        /// <summary>把当前 LocomotionMode 映射到 Free/Aim/Sprint 对应的移动参数 </summary>
        private PlayerMovementProfile ResolveMovementProfile(PlayerLocomotionMode mode)
        {
            return mode switch
            {
                PlayerLocomotionMode.Free => _motor.Config.Free,
                PlayerLocomotionMode.Aim => _motor.Config.Aim,
                PlayerLocomotionMode.Sprint => _motor.Config.Sprint,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
            };
        }

        /// <summary>
        /// 解析本 Tick 目标朝向：Aim 使用带迟滞的 Root 跟随；Free/Sprint 朝移动方向；
        /// 没有 Root 转向意图时返回零向量，让 Motor 只衰减剩余角速度 
        /// </summary>
        private Vector3 ResolveDesiredFacing(in PlayerLocomotionInput input)
        {
            if (_controlState.IsAiming)
                return ResolveAimBodyFacing(input.AimDirection);

            _isAimBodyTurning = false;

            if (input.WorldMove.sqrMagnitude >
                _motor.Config.MoveInputDeadZone * _motor.Config.MoveInputDeadZone)
                return input.WorldMove.normalized;

            return Vector3.zero;
        }

        /// <summary>
        /// Aim 时允许上半身先承担偏转；夹角超过 StartAngle 才让 Root 跟随，
        /// 跟随到 StopAngle 后退出，避免在单一阈值附近反复切换 
        /// </summary>
        private Vector3 ResolveAimBodyFacing(Vector3 aimDirection)
        {
            aimDirection.y = 0f;
            float deadZone = _motor.Config.FacingDirectionDeadZone;

            if (aimDirection.sqrMagnitude <= deadZone * deadZone)
            {
                _isAimBodyTurning = false;
                return Vector3.zero;
            }

            Vector3 bodyFacing = _motor.MotionState.FacingDirection;
            bodyFacing.y = 0f;

            if (bodyFacing.sqrMagnitude <= deadZone * deadZone)
            {
                _isAimBodyTurning = false;
                return Vector3.zero;
            }

            aimDirection.Normalize();
            bodyFacing.Normalize();
            // Vector3.Angle 返回 0~180 的无符号最小夹角；这里只决定是否跟随，不需要左右符号 
            float angle = Vector3.Angle(bodyFacing, aimDirection);

            if (_isAimBodyTurning)
            {
                if (angle <= _motor.Config.AimBodyTurnStopAngle)                                       
                    _isAimBodyTurning = false;
            }
            else if (angle > _motor.Config.AimBodyTurnStartAngle)
            {
                _isAimBodyTurning = true;
            }

            return _isAimBodyTurning ? aimDirection : Vector3.zero;
        }
    }
}
