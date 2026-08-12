namespace ProjectGame.HotFix.Gameplay.Player.State
{
    /// <summary>
    /// 玩家最高层生命状态。
    /// </summary>
    public enum PlayerLifeState : byte
    {
        Alive = 0,
        Dead = 1,
    }

    /// <summary>
    /// Alive 状态下的基础移动模式。
    /// Dash / Knockback 等以后属于 Alive 下的另一类动作状态，
    /// 不放进这里。
    /// </summary>
    public enum PlayerLocomotionMode : byte
    {
        Free = 0,
        Aim = 1,
        Sprint = 2,
    }

    /// <summary>
    /// 可直接进入玩家模拟快照的 Gameplay 状态。
    /// 不保存 Animator / Camera 等表现状态。
    /// </summary>
    public struct PlayerControlState
    {
        public PlayerLifeState LifeState;
        public PlayerLocomotionMode LocomotionMode;

        public bool IsAlive => LifeState == PlayerLifeState.Alive;
        public bool IsDead => LifeState == PlayerLifeState.Dead;
        public bool IsAiming => IsAlive && LocomotionMode == PlayerLocomotionMode.Aim;
        public bool IsSprinting => IsAlive && LocomotionMode == PlayerLocomotionMode.Sprint;

        public static PlayerControlState CreateDefault()
        {
            return new PlayerControlState
            {
                LifeState = PlayerLifeState.Alive,
                LocomotionMode = PlayerLocomotionMode.Free,
            };
        }
    }
}