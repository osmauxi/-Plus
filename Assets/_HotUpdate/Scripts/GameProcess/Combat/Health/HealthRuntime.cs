using System;
using System.Collections.Generic;

namespace ProjectGame.HotFix.Gameplay.Combat.Health
{
    /// <summary>
    /// 单会话、单线程的权威 Health 模块。外部协调器显式调用 Step；无单例、Update 或场景生命周期订阅。
    /// 客户端使用 HealthReplica。命令入口不是网络 RPC，调用端必须先验证服务器权限。
    /// </summary>
    public sealed class HealthRuntime : IHealthStateSource, IHealthCommandSink, IHealthEventSource, IDisposable
    {
        private readonly HealthWorld _world = new HealthWorld();
        private readonly IHealthDefinitionSource _definitions;
        private readonly Dictionary<ulong, HealthChange> _dirty = new Dictionary<ulong, HealthChange>();
        private List<HealthCommand> _pending = new List<HealthCommand>();
        private List<HealthCommand> _batch = new List<HealthCommand>();
        private readonly List<HealthResult> _results = new List<HealthResult>();
        private bool _busy;
        private bool _disposed;
        private ulong _deltaBaseline;

        public Guid SessionId { get; }
        public uint MaxCommandAgeTicks { get; }
        public int MaxPendingCommands { get; }
        public int Count => _world.Entries.Count;
        public int PendingCount => _pending.Count;
        public bool HasStepped { get; private set; }
        public uint CurrentTick { get; private set; }
        public ulong Revision => _world.Revision;

        public event Action<HealthChange> StateChanged;
        public event Action<HealthResult> CommandResolved;
        public event Action<HealthResult> Died;
        public event Action<HealthResult> Revived;
        public event Action<HealthResult> ReactionRequested;

        public HealthRuntime(IHealthDefinitionSource definitions, uint maxCommandAgeTicks = 120,
            int maxPendingCommands = 4096, Guid sessionId = default)
        {
            _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
            if (maxCommandAgeTicks > int.MaxValue || maxPendingCommands < 1)
                throw new ArgumentOutOfRangeException(nameof(maxCommandAgeTicks));
            MaxCommandAgeTicks = maxCommandAgeTicks;
            MaxPendingCommands = maxPendingCommands;
            SessionId = sessionId == Guid.Empty ? Guid.NewGuid() : sessionId;
        }

        public HealthEntity Register(ulong entityId, int profileId, float initialShield = 0)
        {
            EnsureIdle();
            if (_world.Entries.ContainsKey(entityId)) throw new InvalidOperationException($"Health 实体 {entityId} 已注册。");
            var definition = _definitions.GetDefinition(profileId);
            if (definition == null) throw new InvalidOperationException("参数源返回了空 Health 定义。");
            if (!HealthMath.NonNegative(initialShield) || initialShield > definition.MaxShield)
                throw new ArgumentOutOfRangeException(nameof(initialShield));
            var entity = new HealthEntity(entityId, _world.NextGeneration());
            var state = new HealthSnapshot(entity, definition, definition.MaxHealth, initialShield, false, 0, false, _world.NextRevision());
            _world.Entries.Add(entityId, new HealthWorld.Entry { State = state });
            PublishLifecycle(new HealthChange(HealthChangeKind.Registered, state, state.Revision));
            return entity;
        }

        public bool Unregister(HealthEntity entity)
        {
            EnsureIdle();
            if (!_world.TryGet(entity, out var entry)) return false;
            ulong revision = _world.NextRevision();
            _world.Entries.Remove(entity.EntityId);
            _world.ForgetEvents(entity);
            // 排队请求保留并返回 UnknownTarget，避免请求静默消失；新代次绝不会承受旧请求。
            PublishLifecycle(new HealthChange(HealthChangeKind.Removed, entry.State, revision));
            return true;
        }

        public bool TryGetEntity(ulong entityId, out HealthEntity entity)
        {
            entity = default;
            if (!_world.Entries.TryGetValue(entityId, out var entry)) return false;
            entity = entry.State.Entity;
            return true;
        }

        public bool TryGetHealth(HealthEntity entity, out HealthSnapshot state)
        {
            state = default;
            if (!_world.TryGet(entity, out var entry)) return false;
            state = entry.State;
            return true;
        }

        public bool TryEnqueue(in HealthCommand command)
        {
            EnsureNotDisposed();
            if (_pending.Count >= MaxPendingCommands) return false;
            _pending.Add(command);
            return true;
        }

        /// <summary>
        /// 按权威入队顺序结算当前批次，然后发布通知。回调里新入队的请求留到下一 Tick。
        /// Tick 支持 uint 回绕，相邻调用跨度必须小于 2^31；未来请求返回 FutureTick，不隐式调度。
        /// </summary>
        public void Step(uint tick)
        {
            EnsureIdle();
            if (HasStepped && !HealthMath.IsNewer(tick, CurrentTick))
                throw new ArgumentOutOfRangeException(nameof(tick), "Health Tick 必须前进且跨度小于 2^31。");
            _busy = true;
            List<Exception> errors = null;
            try
            {
                CurrentTick = tick;
                HasStepped = true;
                _world.Prune(tick, MaxCommandAgeTicks);
                var swap = _batch; _batch = _pending; _pending = swap;
                _results.Clear();
                foreach (var command in _batch)
                {
                    var result = Resolve(command, tick);
                    _results.Add(result);
                    if (result.Code == HealthResultCode.Applied)
                        _dirty[result.After.Entity.EntityId] = new HealthChange(HealthChangeKind.Updated, result.After, result.After.Revision);
                }
                _batch.Clear();
                foreach (var result in _results)
                {
                    if (result.Code == HealthResultCode.Applied)
                        HealthNotifications.Send(StateChanged,
                            new HealthChange(HealthChangeKind.Updated, result.After, result.After.Revision), ref errors);
                    HealthNotifications.Send(CommandResolved, result, ref errors);
                    if (result.WasKilled) HealthNotifications.Send(Died, result, ref errors);
                    if (result.WasRevived) HealthNotifications.Send(Revived, result, ref errors);
                    if (result.ShouldReact) HealthNotifications.Send(ReactionRequested, result, ref errors);
                }
            }
            finally { _batch.Clear(); _results.Clear(); _busy = false; }
            HealthNotifications.ThrowIfAny(errors);
        }

        private HealthResult Resolve(in HealthCommand command, uint tick)
        {
            if (!command.IsValid) return HealthSystem.Reject(command, tick, HealthResultCode.InvalidCommand);
            if (HealthMath.IsNewer(command.Tick, tick)) return HealthSystem.Reject(command, tick, HealthResultCode.FutureTick);
            if (unchecked(tick - command.Tick) > MaxCommandAgeTicks) return HealthSystem.Reject(command, tick, HealthResultCode.Expired);
            if (!_world.TryGet(command.Target, out var entry)) return HealthSystem.Reject(command, tick, HealthResultCode.UnknownTarget);
            var key = new HealthWorld.EventKey(command);
            if (_world.SeenEvents.ContainsKey(key)) return HealthSystem.Reject(command, tick, HealthResultCode.Duplicate, entry.State);
            // 受击被无敌/门控/死亡拒绝后也已消费，不能在条件改变后重试同一次攻击。
            _world.SeenEvents.Add(key, command.Tick);
            return HealthSystem.Apply(_world, entry, command, tick);
        }

        /// <summary>无副作用全量，供新客户端和丢包补齐。必须发送给与 SessionId 对应的镜像。</summary>
        public HealthFrame CaptureFullSnapshot()
        {
            EnsureIdle();
            var changes = new List<HealthChange>(_world.Entries.Count);
            foreach (var entry in _world.Entries.Values)
                changes.Add(new HealthChange(HealthChangeKind.Registered, entry.State, entry.State.Revision));
            changes.Sort((a, b) => a.State.Entity.EntityId.CompareTo(b.State.Entity.EntityId));
            return new HealthFrame(SessionId, true, 0, Revision, changes);
        }

        /// <summary>消费全局 dirty 批次。一个网络发送器调用一次，并将同一帧广播；不能逐客户端调用。</summary>
        public HealthFrame CaptureDelta()
        {
            EnsureIdle();
            var changes = new List<HealthChange>(_dirty.Values);
            changes.Sort((a, b) => a.Revision.CompareTo(b.Revision));
            var frame = new HealthFrame(SessionId, false, _deltaBaseline, Revision, changes);
            _deltaBaseline = Revision;
            _dirty.Clear();
            return frame;
        }

        private void PublishLifecycle(in HealthChange change)
        {
            _dirty[change.State.Entity.EntityId] = change;
            List<Exception> errors = null;
            _busy = true;
            try { HealthNotifications.Send(StateChanged, change, ref errors); }
            finally { _busy = false; }
            HealthNotifications.ThrowIfAny(errors);
        }

        private void EnsureNotDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(HealthRuntime));
        }

        private void EnsureIdle()
        {
            EnsureNotDisposed();
            if (_busy) throw new InvalidOperationException("Health 批次/通知期间不能重入 Step、注册、注销、同步捕获或 Dispose；请延迟到阶段边界。");
        }

        /// <summary>会话结束释放数据与订阅；不是逐实体死亡事件。重新进入会话应创建新实例。</summary>
        public void Dispose()
        {
            if (_disposed) return;
            EnsureIdle();
            _world.Entries.Clear(); _world.SeenEvents.Clear(); _pending.Clear(); _batch.Clear(); _results.Clear(); _dirty.Clear();
            StateChanged = null; CommandResolved = null; Died = null; Revived = null; ReactionRequested = null;
            _disposed = true;
        }
    }
}
