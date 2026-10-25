using System;
using Unity.Profiling;

namespace ProjectGame.HotFix.Gameplay.Network
{
    /// <summary>单个 Gameplay Tick 内固定执行的系统阶段。</summary>
    public enum GameplayTickPhase
    {
        PreSimulation,
        ActorSimulation,
        WorldSimulation,
        CombatSimulation,
        PostSimulation,
        Replication,
        Completed,
        Count,
    }

    /// <summary>
    /// 当前 Peer 上所有 Gameplay 网络系统共享的会话时间轴。
    /// 由独立 GameplayNetworkTickDriver 推进，不依赖 NGO Tick 或 Unity Time。
    /// </summary>
    public sealed class NetworkSimulationClock
    {
        private static readonly ProfilerMarker[] PhaseMarkers =
        {
            new("Gameplay.TickClock.PreSimulation"),
            new("Gameplay.TickClock.ActorSimulation"),
            new("Gameplay.TickClock.WorldSimulation"),
            new("Gameplay.TickClock.CombatSimulation"),
            new("Gameplay.TickClock.PostSimulation"),
            new("Gameplay.TickClock.Replication"),
            new("Gameplay.TickClock.Completed"),
        };

        private readonly Action<uint>[] _phaseHandlers =
            new Action<uint>[(int)GameplayTickPhase.Count];

        public int TickRate { get; }

        public float TickDeltaTime { get; }

        public uint CurrentTick { get; private set; }

        /// <summary>启动时对齐的服务器 Tick，之后随本地节拍推进；不是持续网络对时结果。</summary>
        public uint EstimatedServerTick { get; private set; }

        /// <summary>旧入口：等价于 ActorSimulation。新系统应显式订阅阶段。</summary>
        public event Action<uint> TickAdvanced
        {
            add => Subscribe(GameplayTickPhase.ActorSimulation, value);
            remove => Unsubscribe(GameplayTickPhase.ActorSimulation, value);
        }

        /// <summary>旧入口：等价于 Completed。新系统应显式订阅阶段。</summary>
        public event Action<uint> TickCompleted
        {
            add => Subscribe(GameplayTickPhase.Completed, value);
            remove => Unsubscribe(GameplayTickPhase.Completed, value);
        }

        public NetworkSimulationClock(int tickRate)
        {
            if (tickRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(tickRate), tickRate, "TickRate 必须大于 0。");

            TickRate = tickRate;
            TickDeltaTime = 1f / tickRate;
        }

        public void Subscribe(GameplayTickPhase phase, Action<uint> handler) =>
            _phaseHandlers[(int)phase] += handler;

        public void Unsubscribe(GameplayTickPhase phase, Action<uint> handler) =>
            _phaseHandlers[(int)phase] -= handler;

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
            for (int phase = 0; phase < (int)GameplayTickPhase.Count; phase++)
            {
                using (PhaseMarkers[phase].Auto())
                    _phaseHandlers[phase]?.Invoke(CurrentTick);
            }
            return CurrentTick;
        }
    }
}
