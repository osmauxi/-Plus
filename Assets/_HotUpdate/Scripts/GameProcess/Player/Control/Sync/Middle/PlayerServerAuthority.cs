using System;

namespace ProjectGame.HotFix.Gameplay.Player.Sync
{
    /// <summary>
    /// 单个玩家的 Server 权威输入时间轴 
    /// 负责净化/分类 Client 输入、为每个固定 Tick 选择唯一输入，并通过 PlayerSimulation 产生权威状态 
    /// 不负责消息解析、Owner 身份校验和网络发送；这些属于 PlayerSyncTransport/PlayerSyncController 
    /// </summary>
    public sealed class PlayerServerAuthority
    {
        /// <summary>标记某个权威 Tick 最终采用的输入来源，供指标和测试判断 </summary>
        public enum ResolveResult : byte
        {
            // 输入原始 Tick 与下一权威 Tick 完全一致 
            Exact = 0,
            // 最新输入已经迟到，但仍在允许窗口内，被重标为下一权威 Tick 
            RetimedLate = 1,
            // 短时缺包，有限次数沿用上一份已解析输入 
            ReusedPrevious = 2,
            // 没有可用输入或沿用超限，使用全零安全输入 
            Neutral = 3,
        }

        /// <summary>包含 NaN/Infinity 等无法净化数值而被拒绝的输入总数 </summary>
        public int InvalidInputCount { get; private set; }

        // 执行权威玩家模拟、捕获和恢复状态的统一入口 
        private readonly PlayerSimulation _simulation;
        // 由 PlayerSyncController 创建并注入的纯 C# 固定步时钟；是权威 Tick 编号和单步时长的唯一来源 
        private readonly PlayerSimulationClock _clock;
        // 输入历史容量、未来/迟到窗口和 Hold 上限等参数 
        private readonly PlayerSyncConfig _config;
        // 保存尚未处理的未来输入；以原始 Client Tick 为键 
        private readonly TickRingBuffer<PlayerInputCommand> _inputBuffer;
        // 最近一次实际执行的输入；缺包时可在 MaxInputHoldTicks 内沿用 
        private PlayerInputCommand _lastResolvedInput;
        // Server 已经完成权威模拟的最新 Tick；与 Clock 当前正在派发的 Tick 分开记录 
        private uint _lastProcessedTick;
        // 当前会话曾接受到的最高未来 Tick，用于估算前方已缓冲输入量 
        private uint _highestReceivedTick;
        // 等待下一权威 Tick 消费的最新迟到输入；仍保留其原始 Client Tick 
        private PlayerInputCommand _pendingLateInput;
        // _pendingLateInput 的原始 Client Tick，用于与 Exact 输入比较新旧 
        private uint _pendingLateSourceTick;
        // Server 已观察到的最新 Client 输入 Tick；阻止旧包/冗余覆盖更新意图 
        private uint _latestObservedClientTick;
        // 连续没有 Exact/RetimedLate 输入的 Tick 数；决定还能沿用上一输入多久 
        private int _missingInputStreak;
        // 标记 _lastResolvedInput 是否已有有效值 
        private bool _hasLastResolvedInput;
        // 标记当前是否存在待消费的迟到输入 
        private bool _hasPendingLateInput;
        // 标记 _latestObservedClientTick 是否已有有效值 
        private bool _hasLatestObservedClientTick;

        /// <summary>最近一次权威模拟完成后的完整玩家状态 </summary>
        public PlayerSimulationState CurrentState { get; private set; }
        /// <summary>Server 已经完成权威模拟的最新 Tick </summary>
        public uint LastProcessedTick => _lastProcessedTick;
        /// <summary>最高已接收 Tick 比已处理 Tick 领先的数量；仅用于观察未来输入缓冲深度 </summary>
        public int BufferedAheadTicks => TickMath.IsNewer(_highestReceivedTick, _lastProcessedTick)
            ? (int)Math.Min(TickMath.Distance(_highestReceivedTick, _lastProcessedTick), int.MaxValue)
            : 0;
        /// <summary>最近一个权威 Tick 的输入来源 </summary>
        public ResolveResult LastInputResolveResult { get; private set; }
        /// <summary>通过净化与时间边界、进入 Future Buffer 或 Pending Late 的输入数 </summary>
        public int AcceptedInputCount { get; private set; }
        /// <summary>未来缓冲中重复 Tick，或不比最新 Client Tick 更新而被拒绝的输入数 </summary>
        public int DuplicateInputCount { get; private set; }
        /// <summary>到达时原始 Tick 已处理完的输入数；其中可能包含后来被 RetimedLate 救回的最新输入 </summary>
        public int OutdatedInputCount { get; private set; }
        /// <summary>领先 Server 超过 MaxFutureInputTicks 而被拒绝的输入数 </summary>
        public int InvalidFutureInputCount { get; private set; }
        /// <summary>权威模拟实际执行 Exact 输入的 Tick 数 </summary>
        public int ExactInputTickCount { get; private set; }
        /// <summary>被接受为 Pending Late 候选的最新迟到输入数 </summary>
        public int RetimedLateAcceptedInputCount { get; private set; }
        /// <summary>权威模拟实际执行 RetimedLate 输入的 Tick 数 </summary>
        public int RetimedLateInputTickCount { get; private set; }
        /// <summary>权威模拟实际沿用上一输入的 Tick 数 </summary>
        public int ReusedInputTickCount { get; private set; }
        /// <summary>权威模拟实际执行中立输入的 Tick 数 </summary>
        public int NeutralInputTickCount { get; private set; }

        /// <summary>创建 Server 权威时间轴并按配置预分配未来输入历史 </summary>
        public PlayerServerAuthority(
            PlayerSimulation simulation,
            PlayerSimulationClock clock,
            PlayerSyncConfig config)
        {
            _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _config.Validate();

            if (_clock.TickRate != _config.SimulationTickRate)
                throw new InvalidOperationException("服务器权威时钟 TickRate 必须与同步配置一致 ");

            _inputBuffer = new TickRingBuffer<PlayerInputCommand>(_config.HistoryCapacity);
        }

        /// <summary>
        /// 从指定权威状态建立时间轴起点，恢复 Simulation，并清空输入历史和所有统计 
        /// </summary>
        public void Initialize(in PlayerSimulationState initialState)
        {
            _simulation.RestoreState(initialState);
            ResetInputBuffer(initialState.Tick);
            CurrentState = initialState;
        }
        /// <summary>
        /// 接收一条已通过 Transport Owner 路由的 Client 输入 
        /// 先净化数值，再按过期、允许迟到、合法未来、重复/乱序四种情况分类 
        /// 返回 true 只表示输入进入 Future Buffer 或 Pending Late，不表示它一定会被实际执行 
        /// </summary>
        public bool PushInput(in PlayerInputCommand input)
        {
            if (!input.TrySanitize(out PlayerInputCommand sanitized))
            {
                InvalidInputCount++;
                return false;
            }

            // 原始 Tick 已处理完：只允许“窗口内且比所有已观察 Client Tick 更新”的最新意图进入 Pending Late 
            if (!TickMath.IsNewer(sanitized.Tick, _lastProcessedTick))
            {
                OutdatedInputCount++;

                // lateByTicks 是该输入比 Server 已处理 Tick 落后的年龄 
                uint lateByTicks = TickMath.Distance(_lastProcessedTick, sanitized.Tick);
                // 包内旧冗余和乱序旧包不能覆盖更新的 Pending Late 
                bool isNewestClientInput = !_hasLatestObservedClientTick || TickMath.IsNewer(sanitized.Tick, _latestObservedClientTick);
                if (_config.MaxLateInputRetimingTicks <= 0 || lateByTicks > _config.MaxLateInputRetimingTicks || !isNewestClientInput)
                    return false;

                _pendingLateInput = sanitized;
                _pendingLateSourceTick = sanitized.Tick;
                _hasPendingLateInput = true;
                _latestObservedClientTick = sanitized.Tick;
                _hasLatestObservedClientTick = true;
                AcceptedInputCount++;
                RetimedLateAcceptedInputCount++;
                return true;
            }
            // 合法未来输入必须落在有限窗口内，避免异常 Tick 挤占固定容量历史 
            if (TickMath.Distance(sanitized.Tick, _lastProcessedTick) > _config.MaxFutureInputTicks)
            {
                InvalidFutureInputCount++;
                return false;
            }
            // 相同 Tick 已缓存，或该 Tick 不比最近观察到的 Client Tick 更新时，视为重复/旧冗余 
            if (_inputBuffer.Contains(sanitized.Tick) ||
                (_hasLatestObservedClientTick && !TickMath.IsNewer(sanitized.Tick, _latestObservedClientTick)))
            {
                DuplicateInputCount++;
                return false;
            }

            _inputBuffer.Store(sanitized.Tick, sanitized);
            if (TickMath.IsNewer(sanitized.Tick, _highestReceivedTick))
                _highestReceivedTick = sanitized.Tick;
            _latestObservedClientTick = sanitized.Tick;
            _hasLatestObservedClientTick = true;
            AcceptedInputCount++;
            return true;
        }
        /// <summary>
        /// 为下一个 Server Tick 解析唯一输入、累计来源指标并执行一次权威模拟 
        /// 输入优先级固定为 Exact &gt; RetimedLate &gt; ReusedPrevious &gt; Neutral 
        /// </summary>
        public PlayerSimulationState SimulateNextTick(uint tick)
        {
            uint expectedTick = unchecked(_lastProcessedTick + 1u);
            if (tick != expectedTick || tick != _clock.CurrentTick)
                throw new InvalidOperationException("权威模拟 Tick 必须是时钟当前派发的下一个连续 Tick ");

            PlayerInputCommand input = ResolveNextInput(tick, out ResolveResult result);
            LastInputResolveResult = result;
            switch (result)
            {
                case ResolveResult.Exact:
                    ExactInputTickCount++;
                    break;
                case ResolveResult.RetimedLate:
                    RetimedLateInputTickCount++;
                    break;
                case ResolveResult.ReusedPrevious:
                    ReusedInputTickCount++;
                    break;
                default:
                    NeutralInputTickCount++;
                    break;
            }
            CurrentState = _simulation.Simulate(input, _clock.TickDeltaTime);
            _lastProcessedTick = tick;
            return CurrentState;
        }

        /// <summary>传送、复活或完整重置后，从新状态重新初始化权威时间轴 </summary>
        public void Reset(in PlayerSimulationState state) => Initialize(state);

        /// <summary>
        /// 为下一权威 Tick 选择输入 
        /// Exact 拥有最高优先级；没有 Exact 才消费一次 Pending Late；之后有限 Hold；最后回中立 
        /// </summary>
        private PlayerInputCommand ResolveNextInput(uint nextTick, out ResolveResult result)
        {
            if (_inputBuffer.TryGet(nextTick, out PlayerInputCommand exactInput))
            {
                _inputBuffer.Remove(nextTick);
                // Exact 的时间语义最准确；不比它更新的迟到候选不再需要保留 
                if (_hasPendingLateInput && !TickMath.IsNewer(_pendingLateSourceTick, exactInput.Tick))
                    _hasPendingLateInput = false;
                _lastResolvedInput = exactInput;
                _hasLastResolvedInput = true;
                _missingInputStreak = 0;
                result = ResolveResult.Exact;
                return exactInput;
            }

            if (_hasPendingLateInput)
            {
                // 只有真正消费时才替换 Tick；接收阶段保留原始 Tick 才能正确比较输入新旧 
                PlayerInputCommand retimed = _pendingLateInput.WithTick(nextTick);
                _hasPendingLateInput = false;
                _lastResolvedInput = retimed;
                _hasLastResolvedInput = true;
                _missingInputStreak = 0;
                result = ResolveResult.RetimedLate;
                return retimed;
            }

            _missingInputStreak++;
            if (_hasLastResolvedInput && _missingInputStreak <= _config.MaxInputHoldTicks)
            {
                // Hold 只修改 Tick，沿用上一份输入意图，并受 MaxInputHoldTicks 限制以防卡键 
                PlayerInputCommand reused = _lastResolvedInput.WithTick(nextTick);
                _lastResolvedInput = reused;
                result = ResolveResult.ReusedPrevious;
                return reused;
            }

            // 沿用次数耗尽后强制回到中立，确保丢失松键包也不会让角色永久移动/冲刺 
            PlayerInputCommand neutral = PlayerInputCommand.CreateNeutral(nextTick);
            // 中立输入只清持续按钮，Reload 序号必须保持“状态机已消费值” 
            // 使用 CurrentState 而不是 _lastResolvedInput：后者可能仍带有尚未进入权威状态的请求语义 
            // 如果这里回到 0，非零已消费值会被误判为另一次 Reload 请求 
            neutral.ReloadRequestSequence = CurrentState.ActionState.LastReloadRequestSequence;
            _lastResolvedInput = neutral;
            _hasLastResolvedInput = true;
            result = ResolveResult.Neutral;
            return neutral;
        }

        /// <summary>清空全部输入时间轴状态和调试计数，并把已处理 Tick 设置为指定起点 </summary>
        private void ResetInputBuffer(uint tick)
        {
            InvalidInputCount = 0;
            _inputBuffer.Clear();
            _clock.Reset(tick);
            _lastProcessedTick = tick;
            _highestReceivedTick = tick;
            _lastResolvedInput = default;
            _pendingLateInput = default;
            _pendingLateSourceTick = tick;
            _latestObservedClientTick = tick;
            _hasLastResolvedInput = false;
            _hasPendingLateInput = false;
            _hasLatestObservedClientTick = false;
            _missingInputStreak = 0;
            AcceptedInputCount = 0;
            DuplicateInputCount = 0;
            OutdatedInputCount = 0;
            InvalidFutureInputCount = 0;
            ExactInputTickCount = 0;
            RetimedLateAcceptedInputCount = 0;
            RetimedLateInputTickCount = 0;
            ReusedInputTickCount = 0;
            NeutralInputTickCount = 0;
        }
    }
}
