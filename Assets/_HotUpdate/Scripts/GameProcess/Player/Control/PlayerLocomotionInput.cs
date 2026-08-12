using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Movement
{
    /// <summary>
    /// 一次玩家 Locomotion 模拟所需的完整输入。
    ///
    /// 本地玩家可以由 InputManager + Camera 生成；
    /// 服务器以后可以直接从网络 InputPayload 还原。
    ///
    /// 因此后续真正的运动逻辑不需要知道 Camera、鼠标或 Input System。
    /// </summary>
    public readonly struct PlayerLocomotionInput
    {
        /// <summary>
        /// 已转换到世界空间的 XZ 移动输入。
        /// 长度保留输入强度，通常为 0~1。
        /// </summary>
        public Vector3 WorldMove { get; }

        /// <summary>
        /// 鼠标瞄准对应的世界空间平面方向。
        /// 非 Aim 状态下可以为 Vector3.zero。
        /// </summary>
        public Vector3 AimDirection { get; }

        public bool AimHeld { get; }
        public bool SprintHeld { get; }

        public PlayerLocomotionInput(Vector3 worldMove, Vector3 aimDirection, bool aimHeld, bool sprintHeld)
        {
            WorldMove = worldMove;
            AimDirection = aimDirection;
            AimHeld = aimHeld;
            SprintHeld = sprintHeld;
        }

        public static PlayerLocomotionInput None => new(Vector3.zero, Vector3.zero, false, false);
    }
}