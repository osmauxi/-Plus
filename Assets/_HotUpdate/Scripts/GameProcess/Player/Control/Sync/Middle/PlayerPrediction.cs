using System;
using UnityEngine;
using ProjectGame.HotFix.Gameplay.Player.State;

namespace ProjectGame.HotFix.Gameplay.Player.Sync
{
    /// <summary>
    /// Remote Owner Client 的固定 Tick 预测、权威校正、输入历史与发送节流策略。
    /// 该类管理预测历史与权威校正，不直接进行网络发送；Tick 编号和步长来自 PlayerSimulationClock，
    /// 真正模拟统一委托给 PlayerSimulation。
    /// </summary>
    public sealed class PlayerPrediction
    {
        // Owner 与 Server 共用的模拟入口；用于首次预测、回滚恢复和 Replay。
        private readonly PlayerSimulation _simulation;
        // 由 PlayerSyncController 创建并注入的纯 C# 固定步时钟；是预测 Tick 编号和单步时长的唯一来源。
        private readonly PlayerSimulationClock _clock;
        // 历史容量、校正阈值和输入发送间隔等同步参数。
        private readonly PlayerSyncConfig _config;
        // 按 Client Tick 保存输入；用于丢包冗余发送和回滚后的 Replay。
        private readonly TickRingBuffer<PlayerInputCommand> _inputHistory;
        // 按 Client Tick 保存预测状态；用于与同 Tick 的 Server 权威状态比较。
        private readonly TickRingBuffer<PlayerSimulationState> _stateHistory;
        // 最近一次真正交给 Transport 发送的输入内容，用于检测按钮/方向立即变化。
        private PlayerInputCommand _lastSentInput;
        // 已处理过的最新 Server 权威 Snapshot Tick；旧于或等于它的快照不再重复校正。
        private uint _lastConfirmedTick;
        // 最近一次发送输入的 Client Tick；用于计算活跃发送间隔和空闲心跳间隔。
        private uint _lastSentTick;
        // 标记 _lastSentInput/_lastSentTick 是否已经具有有效发送历史。
        private bool _hasLastSentInput;

        /// <summary>Owner 当前已经预测到的最新 Tick。</summary>
        public uint CurrentTick => _clock.CurrentTick;
        /// <summary>最近已经处理的 Server 权威 Tick。</summary>
        public uint LastConfirmedTick => _lastConfirmedTick;
        /// <summary>最近一次同 Tick 比较的位置误差（米）。</summary>
        public float LastPositionError { get; private set; }
        /// <summary>最近一次同 Tick 比较的旋转夹角误差（度）。</summary>
        public float LastRotationError { get; private set; }
        /// <summary>最近一次同 Tick 比较的速度向量误差。</summary>
        public float LastVelocityError { get; private set; }
        /// <summary>最近一次同 Tick 比较的体力数值误差。</summary>
        public float LastStaminaError { get; private set; }
        /// <summary>普通校正中执行 Restore + Replay 的累计次数。</summary>
        public int RollbackCount { get; private set; }
        /// <summary>历史无法支持普通回滚时，直接跳到权威状态的累计次数。</summary>
        public int HardResyncCount { get; private set; }
        /// <summary>最近一次普通回滚实际重演的输入 Tick 数。</summary>
        public int LastReplayTickCount { get; private set; }

        /// <summary>创建一条 Owner 预测时间轴，并按配置预分配输入/状态历史。</summary>
        public PlayerPrediction(
            PlayerSimulation simulation,
            PlayerSimulationClock clock,
            PlayerSyncConfig config)
        {
            _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _config.Validate();

            if (_clock.TickRate != _config.SimulationTickRate)
                throw new InvalidOperationException("预测时钟 TickRate 必须与同步配置一致。");

            _inputHistory = new TickRingBuffer<PlayerInputCommand>(_config.HistoryCapacity);
            _stateHistory = new TickRingBuffer<PlayerSimulationState>(_config.HistoryCapacity);
        }
        /// <summary>
        /// 使用 Controller 时钟已经派发的 Client Tick 保存输入、执行预测并保存结果。
        /// input 使用 ref 是因为写入后的 Tick 还要交给调用方发送。
        /// </summary>
        public PlayerSimulationState Predict(ref PlayerInputCommand input, uint tick)
        {
            if (tick != _clock.CurrentTick)
                throw new InvalidOperationException("预测 Tick 必须等于时钟当前派发的 Tick。");

            input.Tick = tick;
            _inputHistory.Store(tick, input);
            PlayerSimulationState state = _simulation.Simulate(input, _clock.TickDeltaTime);
            _stateHistory.Store(tick, state);
            return state;
        }
        /// <summary>
        /// 消费一份 Server 权威状态：过滤旧快照、计算同 Tick 误差，并在需要时 Restore + Replay。
        /// 返回 true 表示本地 Simulation 被普通回滚或 HardResync 改写；仅确认/忽略时返回 false。
        /// </summary>
        public bool Reconcile(in PlayerSimulationState serverState)
        {
            // 每份新权威状态开始处理时先清零“本次重演量”，避免沿用上一次统计。
            LastReplayTickCount = 0;

            // 旧于或等于最近确认 Tick 的快照已经处理过，直接忽略。
            if (!TickMath.IsNewer(serverState.Tick, _lastConfirmedTick))
                return false;

            // 权威 Tick 已领先本地预测，当前没有对应输入可 Replay，只能把预测时间轴前推到 Server。
            // 这可能来自启动时间轴、时钟漂移或极端弱网，并不只代表“网络迟滞”。
            if (TickMath.IsNewer(serverState.Tick, _clock.CurrentTick))
            {
                HardResync(serverState);
                return true;
            }

            if (!_stateHistory.TryGet(serverState.Tick, out PlayerSimulationState predictedState))
            {
                // 即便暂不校正，也记录已见过该权威 Tick，避免同一快照重复进入此分支。
                _lastConfirmedTick = serverState.Tick;

                // 由于预测存在，本地模拟Tick正常会领先服务器Tick。
                // 刚生成/重置时，第一批服务器快照可能早于本地历史起点；
                // 若差距仍小于历史容量，等待服务器快照追上即可，不能把本地时间轴倒退到旧快照。
                if (TickMath.Distance(_clock.CurrentTick, serverState.Tick) < _config.HistoryCapacity)
                    return false;

                // 差距已达到历史容量，说明所需预测状态已经不可恢复，执行硬同步。
                HardResync(serverState);
                return true;
            }

            // 只有同 Tick 状态才有可比性；先记录误差，再推进最近确认 Tick。
            UpdatePredictionError(predictedState, serverState);
            _lastConfirmedTick = serverState.Tick;

            if (!NeedsRollback(predictedState, serverState))
                return false;

            RollbackCount++;
            // 从权威 Tick 建立可信起点，再顺序重演它之后到 currentTick 的本地输入。
            _simulation.RestoreState(serverState);
            _stateHistory.Store(serverState.Tick, serverState);
            uint replayTick = serverState.Tick;

            while (TickMath.IsNewer(_clock.CurrentTick, replayTick))
            {
                replayTick = unchecked(replayTick + 1u);

                // 任一 Replay 输入缺失都会让之后状态失去确定起点，清空历史并回到 Server Tick。
                if (!_inputHistory.TryGet(replayTick, out PlayerInputCommand input))
                {
                    HardResync(serverState);
                    return true;
                }

                PlayerSimulationState replayedState = _simulation.Simulate(input, _clock.TickDeltaTime);
                _stateHistory.Store(replayTick, replayedState);
                LastReplayTickCount++;
            }

            return true;
        }

        /// <summary>
        /// 判断当前输入是否应该立即发送：首次输入、按钮/方向显著变化立即发；
        /// 其余输入按活跃发送间隔或空闲心跳间隔节流。
        /// </summary>
        public bool ShouldSend(in PlayerInputCommand input)
        {
            if (!_hasLastSentInput)
                return true;
            // 只允许 Tick 单调向前，防止旧输入或重复输入重新进入发送链路。
            if (!TickMath.IsNewer(input.Tick, _lastSentTick))
                return false;

            uint elapsed = TickMath.Distance(input.Tick, _lastSentTick);
            if (HasImmediateChange(input, _lastSentInput))
                return true;
            // 移动/瞄准/射击属于活跃输入，按较高频率发送；完全空闲只保留低频中立心跳。
            // Reload 是序号边沿，变化时已由 HasImmediateChange 强制立即发送，不需要持续归类为 active。
            bool active = input.HasMoveInput || input.AimHeld || input.FireHeld;
            return elapsed >= (active ? _config.InputSendIntervalTicks : _config.IdleHeartbeatTicks);
        }

        /// <summary>在 Transport 成功提交输入后，记录发送内容和 Tick，供下一次 ShouldSend 比较。</summary>
        public void MarkSent(in PlayerInputCommand input)
        {
            _lastSentInput = input;
            _lastSentTick = input.Tick;
            _hasLastSentInput = true;
        }

        /// <summary>读取指定 Tick 的历史输入，用于发送冗余或调试。</summary>
        public bool TryGetInput(uint tick, out PlayerInputCommand input) => _inputHistory.TryGet(tick, out input);
        /// <summary>读取指定 Tick 的预测状态，用于测试、调试或误差检查。</summary>
        public bool TryGetPredictedState(uint tick, out PlayerSimulationState state) => _stateHistory.TryGet(tick, out state);

        /// <summary>把预测 Tick 与最近确认 Tick 同时重置到 startingTick。</summary>
        public void Reset(uint startingTick = 0)
        {
            Reset(startingTick, startingTick);
        }

        /// <summary>
        /// 清空输入/状态/发送历史和调试指标，并分别设置本地预测起点与最近权威确认点。
        /// 两个 Tick 可不同，用于 Client LocalTime 领先 ServerTime 的生成阶段。
        /// </summary>
        public void Reset(uint startingTick, uint lastConfirmedTick)
        {
            _clock.Reset(startingTick);
            _lastConfirmedTick = lastConfirmedTick;
            _lastSentTick = startingTick;
            _hasLastSentInput = false;
            _lastSentInput = default;
            LastPositionError = 0f;
            LastRotationError = 0f;
            LastVelocityError = 0f;
            LastStaminaError = 0f;
            RollbackCount = 0;
            HardResyncCount = 0;
            LastReplayTickCount = 0;
            _inputHistory.Clear();
            _stateHistory.Clear();
        }

        /// <summary>
        /// 无法普通回滚时直接恢复权威状态，清空全部旧输入/状态历史，并从该 Server Tick 重新预测。
        /// </summary>
        private void HardResync(in PlayerSimulationState state)
        {
            _simulation.RestoreState(state);
            _inputHistory.Clear();
            _stateHistory.Clear();
            _stateHistory.Store(state.Tick, state);
            _clock.Reset(state.Tick);
            _lastConfirmedTick = state.Tick;
            _lastSentTick = state.Tick;
            _hasLastSentInput = false;
            HardResyncCount++;
            LastReplayTickCount = 0;
        }

        /// <summary>
        /// 使用已经由 UpdatePredictionError 计算的连续误差，并检查生命、移动模式和耗尽状态等离散差异。
        /// </summary>
        private bool NeedsRollback(in PlayerSimulationState predicted, in PlayerSimulationState server)
        {
            return LastPositionError > _config.PositionErrorThreshold ||
                   LastRotationError > _config.RotationErrorThreshold ||
                   LastVelocityError > _config.VelocityErrorThreshold ||
                   LastStaminaError > _config.StaminaErrorThreshold ||
                   predicted.ControlState.LifeState != server.ControlState.LifeState ||
                   predicted.ControlState.ReactionMode != server.ControlState.ReactionMode ||
                   predicted.ControlState.CombatMode != server.ControlState.CombatMode ||
                   predicted.ControlState.LocomotionMode != server.ControlState.LocomotionMode ||
                   predicted.IsAimBodyTurning != server.IsAimBodyTurning ||
                   predicted.PivotDirection != server.PivotDirection ||
                   predicted.StaminaState.IsExhausted != server.StaminaState.IsExhausted ||
                   HasActionStateMismatch(predicted.ActionState, server.ActionState);
        }

        /// <summary>
        /// 判断输入语义是否发生需要立即上行的变化：按钮、是否移动、移动方向或瞄准方向。
        /// </summary>
        private bool HasImmediateChange(in PlayerInputCommand current, in PlayerInputCommand previous)
        {
            // Reload 请求序号一旦变化必须立即上行；等待常规发送间隔可能让短时动作产生额外输入延迟。
            if (current.Buttons != previous.Buttons ||
                current.HasMoveInput != previous.HasMoveInput ||
                current.ReloadRequestSequence != previous.ReloadRequestSequence)
                return true;

            float thresholdSquared = _config.ImmediateMoveChangeThreshold * _config.ImmediateMoveChangeThreshold;
            return Vector2.SqrMagnitude(current.WorldMove - previous.WorldMove) > thresholdSquared ||
                   Vector2.SqrMagnitude(current.AimDirection - previous.AimDirection) > thresholdSquared;
        }

        /// <summary>
        /// 严格比较所有会改变未来 HFSM 结果或事件消费结果的动作字段。
        /// 这些字段是离散 Tick/序号，不使用浮点阈值；任一不一致都必须从权威 Tick Restore + Replay。
        /// </summary>
        private static bool HasActionStateMismatch(
            in PlayerActionRuntimeState predicted,
            in PlayerActionRuntimeState server)
        {
            return predicted.HitTicksRemaining != server.HitTicksRemaining ||
                   predicted.ReloadTicksRemaining != server.ReloadTicksRemaining ||
                   predicted.FireCooldownTicks != server.FireCooldownTicks ||
                   predicted.ShotSequence != server.ShotSequence ||
                   predicted.HitSequence != server.HitSequence ||
                   predicted.LastReloadRequestSequence != server.LastReloadRequestSequence;
        }

        /// <summary>计算同 Tick 预测状态与权威状态的连续数值误差，供阈值判定和调试展示。</summary>
        private void UpdatePredictionError(in PlayerSimulationState predicted, in PlayerSimulationState server)
        {
            LastPositionError = Vector3.Distance(predicted.Position, server.Position);
            LastRotationError = Quaternion.Angle(predicted.Rotation, server.Rotation);
            LastVelocityError = Vector3.Distance(predicted.Velocity, server.Velocity);
            LastStaminaError = Mathf.Abs(predicted.StaminaState.Current - server.StaminaState.Current);
        }
    }
}
