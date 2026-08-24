using System;

namespace ProjectGame.HotFix.Gameplay.Player.Sync
{
    /// <summary>
    /// 单个玩家同步端点使用的纯 C# 固定步时钟。
    /// 当前版本只负责保存 TickRate（每秒固定步数）、计算 TickDeltaTime（单步时长）、
    /// 维护 CurrentTick（当前固定步编号）以及按外部驱动推进一个 Tick。
    ///
    /// 时钟本身不依赖 Unity、NGO 或 Time；当前由 PlayerSyncController 接收 NGO NetworkTickSystem.Tick
    /// 后调用 AdvanceOneTick。后续更换为独立累加器时，只需替换 Controller 的驱动方式。
    /// </summary>
    public sealed class PlayerSimulationClock
    {
        /// <summary>每秒应执行的固定模拟 Tick 数。</summary>
        public int TickRate { get; }

        /// <summary>单个固定模拟 Tick 对应的秒数。</summary>
        public float TickDeltaTime { get; }

        /// <summary>当前已经推进完成的最新 Tick 编号。</summary>
        public uint CurrentTick { get; private set; }

        /// <summary>创建指定频率的固定步时钟；初始 Tick 为 0。</summary>
        public PlayerSimulationClock(int tickRate)
        {
            if (tickRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(tickRate), tickRate, "TickRate 必须大于 0。");

            TickRate = tickRate;
            TickDeltaTime = 1f / tickRate;
        }

        /// <summary>
        /// 把时钟重新对齐到指定 Tick。
        /// 用于 Spawn（网络生成）、Hard Resync（硬同步）、Warp（强制传送）和会话重置。
        /// </summary>
        public void Reset(uint tick = 0u)
        {
            CurrentTick = tick;
        }

        /// <summary>
        /// 推进并返回下一个 Tick。使用 unchecked 保持 uint 达到最大值后自然回绕到 0，
        /// 与 TickMath（回绕安全的 Tick 比较工具）的语义一致。
        /// </summary>
        public uint AdvanceOneTick()
        {
            CurrentTick = unchecked(CurrentTick + 1u);
            return CurrentTick;
        }
    }
}
