namespace ProjectGame.HotFix.Gameplay.Player.Sync
{
    /// <summary>
    /// 一次性 Gameplay Action 的公共包头。
    ///
    /// 具体动作以后自行携带：
    /// FireCommand
    /// DashCommand
    /// InteractCommand
    /// SkillCommand
    ///
    /// Delivery 策略不写进数据包，由发送端 Action Policy 决定。
    /// </summary>
    public struct PlayerActionCommandHeader
    {
        /// <summary>动作发生在哪个模拟 Tick。</summary>
        public uint Tick;

        /// <summary>客户端单调递增的动作序号，用于去重和确认。</summary>
        public uint Sequence;

        /// <summary>动作类型注册 ID。</summary>
        public ushort TypeId;
    }
}