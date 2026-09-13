using System;

namespace ProjectGame.HotFix.Gameplay.Network
{
    /// <summary>
    /// 当前 Peer 上所有 Gameplay 网络系统共享的会话时间轴。
    /// 由独立 GameplayNetworkTickDriver 推进，不依赖 NGO Tick 或 Unity Time。
    /// </summary>
    public sealed class NetworkSimulationClock
    {
        public int TickRate { get; }

        public float TickDeltaTime { get; }

        public uint CurrentTick { get; private set; }

        /// <summary>启动时对齐的服务器 Tick，之后随本地节拍推进；不是持续网络对时结果。</summary>
        public uint EstimatedServerTick { get; private set; }

        /// <summary>全局Tick推进完成后进行广播</summary>
        public event Action<uint> TickAdvanced;
        /// <summary>所有玩家完成本 Tick 后推进弹丸等后置系统，不依赖订阅先后顺序。</summary>
        public event Action<uint> TickCompleted;

        public NetworkSimulationClock(int tickRate)
        {
            if (tickRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(tickRate), tickRate, "TickRate 必须大于 0。");

            TickRate = tickRate;
            TickDeltaTime = 1f / tickRate;
        }

        /// <summary>开始或重新开始网络会话；不会派发 Gameplay Tick。</summary>
        public void ResetSession(uint startingTick = 0u, uint? startingServerTick = null)
        {
            CurrentTick = startingTick;
            EstimatedServerTick = startingServerTick ?? startingTick;
        }

        /// <summary>按 Gameplay 频率换算时间锚点；保留负时间及 uint 回绕语义。</summary>
        public uint GetTickAtTime(double timeSeconds)
        {
            if (double.IsNaN(timeSeconds) || double.IsInfinity(timeSeconds))
                throw new ArgumentOutOfRangeException(nameof(timeSeconds));

            return unchecked((uint)checked((long)Math.Floor(timeSeconds * TickRate)));
        }

        /// <summary>
        /// 推进并派发一个全局 Gameplay Tick。
        /// 正式运行时只允许 GameplayNetworkTickDriver 调用。
        /// </summary>
        public uint AdvanceOneTick()
        {
            CurrentTick = unchecked(CurrentTick + 1u);
            EstimatedServerTick = unchecked(EstimatedServerTick + 1u);
            TickAdvanced?.Invoke(CurrentTick);
            TickCompleted?.Invoke(CurrentTick);
            return CurrentTick;
        }
    }
}
