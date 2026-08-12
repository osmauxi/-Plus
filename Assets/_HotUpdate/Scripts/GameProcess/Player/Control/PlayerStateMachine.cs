namespace ProjectGame.HotFix.Gameplay.Player.State
{
    /// <summary>
    /// 当前 Tick 用于决定 Locomotion 状态的条件。
    /// 注意这里只放“状态判断需要的事实”，不直接引用 InputManager。
    /// </summary>
    public readonly struct PlayerStateInput
    {
        public bool HasMoveInput { get; }
        public bool AimHeld { get; }
        public bool SprintHeld { get; }

        public PlayerStateInput(bool hasMoveInput, bool aimHeld, bool sprintHeld)
        {
            HasMoveInput = hasMoveInput;
            AimHeld = aimHeld;
            SprintHeld = sprintHeld;
        }
    }

    /// <summary>
    /// 玩家基础 HFSM 的状态转换逻辑。
    ///
    /// 当前版本：
    /// Root: Alive / Dead
    /// Alive.Locomotion: Free / Aim / Sprint
    ///
    /// 自身不保存隐藏状态，所有状态都显式存在 PlayerControlState 中，
    /// 便于以后网络同步、预测和回滚。
    /// </summary>
    public sealed class PlayerStateMachine
    {
        /// <summary>
        /// 更新 Alive 下的 Locomotion 子状态。
        /// 返回本 Tick 是否发生状态变化。
        /// </summary>
        public bool UpdateLocomotion(ref PlayerControlState state, in PlayerStateInput input, bool canSprint)
        {
            if (!state.IsAlive)
                return false;

            PlayerLocomotionMode nextMode = ResolveLocomotion(input, canSprint);

            if (state.LocomotionMode == nextMode)
                return false;

            state.LocomotionMode = nextMode;
            return true;
        }

        public bool SetLifeState(ref PlayerControlState state, PlayerLifeState lifeState)
        {
            if (state.LifeState == lifeState)
                return false;

            state.LifeState = lifeState;

            // 从死亡恢复时始终先回到最稳定的 Free。
            if (lifeState == PlayerLifeState.Alive)
                state.LocomotionMode = PlayerLocomotionMode.Free;

            return true;
        }

        private static PlayerLocomotionMode ResolveLocomotion(in PlayerStateInput input, bool canSprint)
        {
            // Aim 优先于 Sprint。
            // 即使 Shift 仍然按住，Aim 期间也绝不会进入 Sprint。
            if (input.AimHeld)
                return PlayerLocomotionMode.Aim;

            if (input.SprintHeld && input.HasMoveInput && canSprint)
                return PlayerLocomotionMode.Sprint;

            return PlayerLocomotionMode.Free;
        }
    }
}