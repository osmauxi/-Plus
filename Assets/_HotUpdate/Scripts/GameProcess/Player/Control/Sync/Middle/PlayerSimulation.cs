using ProjectGame.HotFix.Gameplay.Player.Movement;
using System;
using ProjectGame.HotFix.Gameplay.Player.Stamina;
using ProjectGame.HotFix.Gameplay.Player.State;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Sync
{
    /// <summary>
    /// 玩家同步模拟统一入口：把网络层 PlayerInputCommand 转成移动层输入，并推进一次固定 Tick 模拟 
    /// Owner 本地预测和 Server 权威模拟必须共用该入口，避免维护两套移动规则 
    /// 
    /// 不负责：
    /// 输入设备采集
    /// 网络发送
    /// Prediction History
    /// Rollback策略
    /// Animator / Camera
    ///
    /// 该类是普通 C# 对象，持有 PlayerLocomotionController；后者继续持有 PlayerMotor 
    /// </summary>
    public sealed class PlayerSimulation
    {
        // 捕获/恢复控制与体力状态，并把输入解析成 Motor 命令 
        private readonly PlayerLocomotionController _locomotion;

        /// <summary>注入并持有完整的 Locomotion（移动状态）模拟链 </summary>
        public PlayerSimulation(PlayerLocomotionController locomotion)
        {
            _locomotion = locomotion ?? throw new ArgumentNullException(nameof(locomotion));
        }

        /// <summary>最近一次模拟、恢复或重置后的运动表现状态 </summary>
        public PlayerMotionState MotionState => _locomotion.MotionState;

        /// <summary>当前生命状态和移动模式 </summary>
        public PlayerControlState ControlState => _locomotion.ControlState;

        /// <summary>当前体力、恢复延迟和耗尽状态 </summary>
        public PlayerStaminaState StaminaState => _locomotion.StaminaState;

        /// <summary>当前受击、射击冷却、换弹计时与动作序号 </summary>
        public PlayerActionRuntimeState ActionState => _locomotion.ActionState;

        /// <summary>当前体力相对最大体力的 0~1 比例 </summary>
        public float NormalizedStamina => _locomotion.NormalizedStamina;

        // 最近一次模拟或恢复后的规范化世界平面 Aim 方向；Root 不跟随时仍供 Animator 上半身使用 
        private Vector2 _aimDirection;

        /// <summary>最终模拟时间轴上的 XZ Aim 方向；零向量表示当前没有有效瞄准目标 </summary>
        public Vector2 AimDirection => _aimDirection;

        /// <summary>Aim Root 当前是否处于迟滞跟随阶段 </summary>
        public bool IsAimBodyTurning => _locomotion.IsAimBodyTurning;

        /// <summary>
        /// 用输入携带的 Tick 和配置的固定步长执行一次完整玩家模拟，并返回模拟后的可恢复状态 
        /// 移动层直接更新角色身体适配器所包装的 Transform，不逐层返回状态，因此模拟结束后统一调用 CaptureState 捕获结果 
        /// </summary>
        public PlayerSimulationState Simulate(in PlayerInputCommand input, float tickDeltaTime)
        {
            // 网络输入已经过净化，但恢复/测试入口也可能直接构造命令，因此在统一入口再次规范化方向 
            _aimDirection = NormalizeDirection(input.AimDirection);
            _locomotion.Simulate(input.ToLocomotionInput(), tickDeltaTime);
            return CaptureState(input.Tick);
        }

        /// <summary>
        /// 从 Motor 与 LocomotionController 捕获指定 Tick 的完整可恢复状态 
        /// tick 由调用方提供，因为本对象只负责模拟内容，不拥有网络时间轴 
        /// </summary>
        public PlayerSimulationState CaptureState(uint tick)
        {
            PlayerMotorRuntimeState motorState = _locomotion.CaptureMotorRuntimeState();

            return new PlayerSimulationState
            {
                Tick = tick,

                Position = motorState.Position,
                Rotation = motorState.Rotation,
                Velocity = motorState.Velocity,
                AngularSpeed = motorState.AngularSpeed,
                AimDirection = _aimDirection,
                IsAimBodyTurning = _locomotion.IsAimBodyTurning,
                PivotDirection = motorState.PivotDirection,
                PivotBoostTimeRemaining = motorState.PivotBoostTimeRemaining,
                PivotBoostSpeedBonus = motorState.PivotBoostSpeedBonus,

                ControlState = _locomotion.ControlState,
                StaminaState = _locomotion.StaminaState,
                ActionState = _locomotion.ActionState,
            };
        }

        /// <summary>
        /// 恢复到指定权威或历史状态 
        /// Owner 回滚时通常会在恢复后立即 Replay 尚未被 Server 确认的输入；
        /// Observer 插值也复用该方法把采样状态写回表现对象 
        /// </summary>
        public void RestoreState(in PlayerSimulationState state)
        {
            _aimDirection = NormalizeDirection(state.AimDirection);

            PlayerMotorRuntimeState motorState = new(
                state.Position,
                state.Rotation,
                state.Velocity,
                state.AngularSpeed,
                state.PivotDirection,
                state.PivotBoostTimeRemaining,
                state.PivotBoostSpeedBonus);

            // Locomotion 统一保证高层控制/体力状态先于 Motor 运动状态恢复 
            _locomotion.RestoreSimulationState(
                state.ControlState,
                state.StaminaState,
                state.ActionState,
                state.IsAimBodyTurning,
                motorState);
        }

        /// <summary>修改玩家顶层生命状态 </summary>
        public void SetLifeState(PlayerLifeState lifeState) => _locomotion.SetLifeState(lifeState);

        /// <summary>进入或刷新受击状态；实际伤害是否合法由服务器 Gameplay 决定 </summary>
        public bool ApplyHit(float tickDeltaTime) => _locomotion.ApplyHit(tickDeltaTime);

        /// <summary>重建默认控制、体力、瞄准和运动状态 </summary>
        public void ResetRuntimeState()
        {
            _aimDirection = Vector2.zero;
            _locomotion.ResetRuntimeState();
        }

        /// <summary>传送角色实体并清空 Motor（运动器）惯性 </summary>
        public void Warp(Vector3 position, Quaternion rotation) => _locomotion.Warp(position, rotation);

        /// <summary>
        /// 把任意非零二维方向压回单位圆 平方阈值避免对接近零的向量归一化后放大浮点噪声 
        /// </summary>
        private static Vector2 NormalizeDirection(Vector2 direction)
        {
            return direction.sqrMagnitude > 0.000001f ? direction.normalized : Vector2.zero;
        }
    }
}
