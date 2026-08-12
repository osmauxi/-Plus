using ProjectGame.HotFix.Gameplay.Player.Stamina;
using ProjectGame.HotFix.Gameplay.Player.State;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Sync
{
    /// <summary>
    /// 一个确定 Simulation Tick 的完整可恢复玩家状态。
    ///
    /// 用途：
    /// Prediction History
    /// Server Authority
    /// Reconciliation
    /// Rollback / Replay
    ///
    /// MotionPhase / LocalVelocity / Acceleration 等表现数据不属于这里。
    /// </summary>
    public struct PlayerSimulationState
    {
        public uint Tick;

        public Vector3 Position;
        public Quaternion Rotation;

        public Vector3 Velocity;
        public float AngularSpeed;

        public PlayerControlState ControlState;
        public PlayerStaminaState StaminaState;

        /// <summary>
        /// Server 已确认处理到的离散 Action Sequence。
        /// 后续客户端可以据此清理 Action History。
        /// </summary>
        public uint LastProcessedActionSequence;
    }
}