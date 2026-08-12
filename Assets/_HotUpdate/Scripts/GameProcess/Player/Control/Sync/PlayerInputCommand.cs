using System;
using ProjectGame.HotFix.Gameplay.Player.Movement;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Sync
{
    [Flags]
    public enum PlayerInputButtons : byte
    {
        None = 0,
        AimHeld = 1 << 0,
        SprintHeld = 1 << 1,
    }

    /// <summary>
    /// 一个 Simulation Tick 对应的连续玩家输入。
    ///
    /// 这里只保存可持续、可被后续输入覆盖的状态：
    /// Move / AimDirection / Held Buttons。
    ///
    /// FirePressed、InteractPressed、DashPressed 等一次性动作不要塞进这里。
    /// </summary>
    public struct PlayerInputCommand
    {
        public uint Tick;

        /// <summary>世界空间 XZ 移动方向，x=WorldX，y=WorldZ。</summary>
        public Vector2 WorldMove;

        /// <summary>世界空间 XZ 瞄准方向。</summary>
        public Vector2 AimDirection;

        public PlayerInputButtons Buttons;

        public bool AimHeld => (Buttons & PlayerInputButtons.AimHeld) != 0;
        public bool SprintHeld => (Buttons & PlayerInputButtons.SprintHeld) != 0;

        public PlayerLocomotionInput ToLocomotionInput()
        {
            Vector3 worldMove = new(WorldMove.x, 0f, WorldMove.y);
            Vector3 aimDirection = new(AimDirection.x, 0f, AimDirection.y);
            return new PlayerLocomotionInput(worldMove, aimDirection, AimHeld, SprintHeld);
        }
    }
}