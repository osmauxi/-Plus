using ProjectGame.HotFix.Gameplay.Player.Movement;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Sync
{
    /// <summary>
    /// 玩家同步模拟统一入口。
    ///
    /// Owner Prediction 与 Server Authority 必须尽量调用同一套 Simulate。
    ///
    /// 不负责：
    /// 输入设备采集
    /// 网络发送
    /// Prediction History
    /// Rollback策略
    /// Animator / Camera
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMotor))]
    [RequireComponent(typeof(PlayerLocomotionController))]
    public sealed class PlayerSimulation : MonoBehaviour
    {
        private PlayerMotor _motor;
        private PlayerLocomotionController _locomotion;

        public uint LastProcessedActionSequence { get; private set; }

        private void Awake()
        {
            _motor = GetComponent<PlayerMotor>();
            _locomotion = GetComponent<PlayerLocomotionController>();
        }

        /// <summary>
        /// 执行一个固定 Simulation Tick。
        /// </summary>
        public PlayerSimulationState Simulate(in PlayerInputCommand input, float tickDeltaTime)
        {
            _locomotion.Simulate(input.ToLocomotionInput(), tickDeltaTime);
            return CaptureState(input.Tick);
        }

        public PlayerSimulationState CaptureState(uint tick)
        {
            PlayerMotorRuntimeState motorState = _motor.CaptureRuntimeState();

            return new PlayerSimulationState
            {
                Tick = tick,

                Position = motorState.Position,
                Rotation = motorState.Rotation,
                Velocity = motorState.Velocity,
                AngularSpeed = motorState.AngularSpeed,

                ControlState = _locomotion.ControlState,
                StaminaState = _locomotion.StaminaState,

                LastProcessedActionSequence = LastProcessedActionSequence,
            };
        }

        /// <summary>
        /// 回滚到指定权威状态。
        /// 调用后通常立即 Replay 尚未被服务器确认的 Input。
        /// </summary>
        public void RestoreState(in PlayerSimulationState state)
        {
            _locomotion.RestoreSimulationState(state.ControlState, state.StaminaState);

            PlayerMotorRuntimeState motorState = new(
                state.Position,
                state.Rotation,
                state.Velocity,
                state.AngularSpeed);

            _motor.RestoreRuntimeState(motorState);

            LastProcessedActionSequence = state.LastProcessedActionSequence;
        }

        public void MarkActionProcessed(uint sequence)
        {
            if (TickMath.IsNewer(sequence, LastProcessedActionSequence))
                LastProcessedActionSequence = sequence;
        }
    }
}