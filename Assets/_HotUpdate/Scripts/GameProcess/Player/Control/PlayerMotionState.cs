using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Movement
{
    /// <summary>
    /// 当前运动的表现阶段。
    ///
    /// 不是 Gameplay FSM State。
    /// Animator / Camera 可以读取它，但不能依赖它驱动 Gameplay。
    /// </summary>
    public enum MotionPhase : byte
    {
        Idle = 0,
        Start = 1,
        Move = 2,
        Stop = 3,
        Pivot = 4,
    }

    /// <summary>
    /// 上层向 PlayerMotor 提交的一帧运动意图。
    ///
    /// WorldMove:
    /// 世界空间移动输入，XZ 平面，长度建议 0~1。
    ///
    /// DesiredFacingDirection:
    /// 世界空间期望朝向。
    /// Free/Sprint 通常等于移动方向；
    /// Aim 通常等于玩家指向鼠标世界坐标的方向。
    /// </summary>
    public readonly struct PlayerMotionCommand
    {
        public Vector3 WorldMove { get; }
        public Vector3 DesiredFacingDirection { get; }

        public PlayerMotionCommand(Vector3 worldMove, Vector3 desiredFacingDirection)
        {
            WorldMove = worldMove;
            DesiredFacingDirection = desiredFacingDirection;
        }

        public static PlayerMotionCommand None => new(Vector3.zero, Vector3.zero);
    }

    /// <summary>
    /// PlayerMotor 一次模拟结束后的运动事实快照。
    ///
    /// Animator、Camera、Footstep、VFX 等表现系统应优先读取这里，
    /// 不要分别重新推导玩家运动状态。
    /// </summary>
    public struct PlayerMotionState
    {
        public Vector3 Position { get; internal set; }

        /// <summary>CharacterController 碰撞处理后的真实世界速度。</summary>
        public Vector3 Velocity { get; internal set; }

        /// <summary>本帧希望达到的世界速度。</summary>
        public Vector3 DesiredVelocity { get; internal set; }

        /// <summary>由实际速度变化计算得到的本帧加速度。</summary>
        public Vector3 Acceleration { get; internal set; }

        /// <summary>实际世界移动方向。</summary>
        public Vector3 MoveDirection { get; internal set; }

        /// <summary>角色当前实际朝向。</summary>
        public Vector3 FacingDirection { get; internal set; }

        /// <summary>上层要求的目标朝向。</summary>
        public Vector3 DesiredFacingDirection { get; internal set; }

        /// <summary>
        /// 相对于角色当前朝向的实际速度。
        /// x = 左右，z = 前后。
        /// Aim 四向动画主要使用这个值。
        /// </summary>
        public Vector3 LocalVelocity { get; internal set; }

        public float Speed { get; internal set; }
        public float NormalizedSpeed { get; internal set; }

        /// <summary>当前绕 Y 轴实际使用的角速度，单位 degree/s。</summary>
        public float AngularSpeed { get; internal set; }

        public MotionPhase Phase { get; internal set; }

        public bool HasMoveInput { get; internal set; }
        public bool IsMoving { get; internal set; }
        public bool IsPivoting { get; internal set; }
    }
}