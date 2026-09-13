using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Network
{
    /// <summary>
    /// 会话唯一的 Unity 固定步驱动器。Update 采集完输入后推进，LateUpdate 读取最终表现。
    /// 由 GameNetworkRuntime 显式启动/停止，不参与另一套服务生命周期。
    /// </summary>
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class GameplayNetworkTickDriver : MonoBehaviour
    {
        public const int MaxTicksPerFrame = 4;

        private NetworkSimulationClock _clock;
        private double _accumulatedSeconds;
        private int _initializedFrame;

        public bool IsInitialized => _clock != null;

        /// <summary>追帧上限丢弃的墙钟时间；用于识别长卡顿造成的会话时间偏移。</summary>
        public double DroppedTimeSeconds { get; private set; }

        public void Initialize(NetworkSimulationClock clock)
        {
            if (clock == null)
                throw new ArgumentNullException(nameof(clock));
            if (_clock == clock)
                return;
            if (IsInitialized)
                throw new InvalidOperationException("TickDriver 已绑定其他会话，请先 Shutdown。");

            _clock = clock;
            _accumulatedSeconds = 0d;
            DroppedTimeSeconds = 0d;
            _initializedFrame = Time.frameCount;
        }

        public void Shutdown()
        {
            _clock = null;
            _accumulatedSeconds = 0d;
        }

        private void Update()
        {
            // 初始化锚点已经包含本帧开始前的时间，不能再次累计这一帧完整的 deltaTime。
            if (Time.frameCount != _initializedFrame)
                Advance(Time.unscaledDeltaTime);
        }

        /// <summary>注入本帧经过的秒数。测试可手动驱动；正式运行只由本组件 Update 调用。</summary>
        public int Advance(double elapsedSeconds)
        {
            if (double.IsNaN(elapsedSeconds) || double.IsInfinity(elapsedSeconds) || elapsedSeconds < 0d)
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            if (!IsInitialized)
                return 0;

            NetworkSimulationClock clock = _clock;
            double interval = 1d / clock.TickRate;
            _accumulatedSeconds += elapsedSeconds;
            // 只补偿浮点减法误差，避免恰好到边界时少派发一个 Tick。
            double dueTicks = Math.Floor((_accumulatedSeconds + interval * 1e-9d) / interval);
            int tickCount = (int)Math.Min(dueTicks, MaxTicksPerFrame);
            _accumulatedSeconds = Math.Max(0d, _accumulatedSeconds - dueTicks * interval);
            DroppedTimeSeconds += Math.Max(0d, dueTicks - tickCount) * interval;

            int advanced = 0;
            while (advanced < tickCount && _clock == clock)
            {
                clock.AdvanceOneTick();
                advanced++;
            }
            return advanced;
        }

        private void OnDestroy() => Shutdown();
    }
}
