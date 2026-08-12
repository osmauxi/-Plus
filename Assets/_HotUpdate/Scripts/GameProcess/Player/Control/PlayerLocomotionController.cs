using System;
using ProjectGame.HotFix.Gameplay.Player.Stamina;
using ProjectGame.HotFix.Gameplay.Player.State;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Movement
{
    /// <summary>
    /// 玩家基础 Locomotion 的运行时编排器。
    ///
    /// 负责把：
    /// 输入意图 → Gameplay状态 → 体力 → MovementProfile → PlayerMotor
    /// 串成一次完整模拟。
    ///
    /// 不负责读取 InputManager，也不负责 Camera / Animator / 网络同步。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMotor))]
    public sealed class PlayerLocomotionController : MonoBehaviour
    {
        [Header("玩家体力配置")]
        [Tooltip("控制冲刺消耗、体力恢复及耗尽解除条件。展开后可调整每个体力参数。")]
        [InspectorName("体力参数")]
        [SerializeField] private PlayerStaminaConfig _staminaConfig = new();

        private PlayerMotor _motor;

        private PlayerStateMachine _stateMachine;
        private PlayerStaminaLogic _staminaLogic;

        private PlayerControlState _controlState;
        private PlayerStaminaState _staminaState;

        public PlayerControlState ControlState => _controlState;
        public PlayerStaminaState StaminaState => _staminaState;
        public PlayerMotionState MotionState => _motor.MotionState;

        public float NormalizedStamina => _staminaState.Normalized(_staminaConfig.MaxStamina);

        private void Awake()
        {
            _motor = GetComponent<PlayerMotor>();

            if (_staminaConfig == null)
                throw new InvalidOperationException($"{nameof(PlayerLocomotionController)} 没有配置 StaminaConfig。");

            _stateMachine = new PlayerStateMachine();
            _staminaLogic = new PlayerStaminaLogic(_staminaConfig);

            ResetRuntimeState();
        }

        public void RestoreSimulationState(in PlayerControlState controlState, in PlayerStaminaState staminaState)
        {
            _controlState = controlState;
            _staminaState = staminaState;
        }

        /// <summary>
        /// 执行一次完整 Locomotion 模拟。
        ///
        /// 后续本地预测和 Server 权威模拟都应该尽量走同一个入口。
        /// </summary>
        public void Simulate(in PlayerLocomotionInput input, float deltaTime)
        {
            if (deltaTime <= 0f)
                return;

            if (_controlState.IsDead)
            {
                _motor.ResetMotion();
                _staminaLogic.Tick(ref _staminaState, false, deltaTime);
                return;
            }

            bool hasMoveInput = input.WorldMove.sqrMagnitude >
                                _motor.Config.MoveInputDeadZone * _motor.Config.MoveInputDeadZone;

            bool canSprint = _staminaLogic.CanSprint(_staminaState);

            PlayerStateInput stateInput = new(
                hasMoveInput,
                input.AimHeld,
                input.SprintHeld);

            // 所有 Locomotion 状态只在这里统一仲裁一次。
            _stateMachine.UpdateLocomotion(ref _controlState, stateInput, canSprint);

            bool isSprinting = _controlState.IsSprinting;

            // Sprint 是持续型特殊行为，每 Tick 消耗公共 Stamina。
            _staminaLogic.Tick(ref _staminaState, isSprinting, deltaTime);

            PlayerMovementProfile profile = ResolveMovementProfile(_controlState.LocomotionMode);

            Vector3 desiredFacing = ResolveDesiredFacing(input);

            PlayerMotionCommand motionCommand = new(
                input.WorldMove,
                desiredFacing);

            _motor.Simulate(motionCommand, profile, deltaTime);
        }

        /// <summary>
        /// 修改玩家顶层生命状态。
        /// Death 不属于 Locomotion，因此由顶层状态统一接管。
        /// </summary>
        public void SetLifeState(PlayerLifeState lifeState)
        {
            if (!_stateMachine.SetLifeState(ref _controlState, lifeState))
                return;

            if (lifeState == PlayerLifeState.Dead)
                _motor.ResetMotion();
        }

        public void ResetRuntimeState()
        {
            _controlState = PlayerControlState.CreateDefault();
            _staminaState = _staminaLogic.CreateInitialState();
            _motor.ResetMotion();
        }

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

        private Vector3 ResolveDesiredFacing(in PlayerLocomotionInput input)
        {
            // Aim 状态永远朝鼠标对应的世界方向。
            if (_controlState.IsAiming)
                return input.AimDirection;

            // Free / Sprint：只要有移动输入，就面朝移动方向。
            if (input.WorldMove.sqrMagnitude >
                _motor.Config.MoveInputDeadZone * _motor.Config.MoveInputDeadZone)
                return input.WorldMove.normalized;

            // 无输入时保持当前朝向。
            return Vector3.zero;
        }
    }
}
