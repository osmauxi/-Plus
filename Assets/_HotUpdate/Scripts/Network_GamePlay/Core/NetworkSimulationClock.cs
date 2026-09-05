using System;

namespace ProjectGame.HotFix.Gameplay.Network
{
    /// <summary>
    /// 当前 Peer 上所有 Gameplay 网络系统共享的会话时间轴。
    /// 该时钟只由 GameplayNetworkBootstrap 响应 NGO Tick 后推进。
    /// </summary>
    public sealed class NetworkSimulationClock
    {
        public int TickRate { get; }

        public float TickDeltaTime { get; }

        public uint CurrentTick { get; private set; }

        /// <summary>全局Tick推进完成后进行广播</summary>
        public event Action<uint> TickAdvanced;

        public NetworkSimulationClock(int tickRate)
        {
            if (tickRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(tickRate), tickRate, "TickRate 必须大于 0。");

            TickRate = tickRate;
            TickDeltaTime = 1f / tickRate;
        }

        /// <summary>开始或重新开始网络会话；不会派发 Gameplay Tick。</summary>
        public void ResetSession(uint startingTick = 0u)
        {
            CurrentTick = startingTick;
        }

        /// <summary>
        /// 推进并派发一个全局 Gameplay Tick。
        /// 正式运行时只允许 GameplayNetworkBootstrap 调用。
        /// </summary>
        public uint AdvanceOneTick()
        {
            CurrentTick = unchecked(CurrentTick + 1u);
            TickAdvanced?.Invoke(CurrentTick);
            return CurrentTick;
        }
    }
}
