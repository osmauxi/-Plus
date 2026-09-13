using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ProjectGame.HotFix.Gameplay.Combat.Health
{
    /// <summary>传输无关的数据帧；网络适配器负责序列化、可信服务器校验和分包组装后一次性 Apply。</summary>
    public sealed class HealthFrame
    {
        public Guid SessionId { get; }
        public bool IsFullSnapshot { get; }
        public ulong FromRevision { get; }
        public ulong ToRevision { get; }
        public ReadOnlyCollection<HealthChange> Changes { get; }

        public HealthFrame(Guid sessionId, bool isFullSnapshot, ulong fromRevision, ulong toRevision,
            IEnumerable<HealthChange> changes)
        {
            if (sessionId == Guid.Empty || fromRevision > toRevision || (isFullSnapshot && fromRevision != 0))
                throw new ArgumentException("非法 Health 同步帧头。");
            if (changes == null) throw new ArgumentNullException(nameof(changes));
            var copy = new List<HealthChange>();
            var ids = new HashSet<ulong>();
            ulong latestChange = fromRevision;
            foreach (var change in changes)
            {
                var state = change.State;
                if (!state.Entity.IsValid || state.Definition == null || state.Revision == 0 || change.Revision == 0 ||
                    change.Revision > toRevision || !ids.Add(state.Entity.EntityId) ||
                    change.Kind > HealthChangeKind.Removed ||
                    (isFullSnapshot && change.Kind != HealthChangeKind.Registered) ||
                    (!isFullSnapshot && change.Revision <= fromRevision) ||
                    (change.Kind == HealthChangeKind.Removed ? state.Revision >= change.Revision : state.Revision != change.Revision))
                    throw new ArgumentException("Health 同步帧包含重复实体、非法版本或非法状态。", nameof(changes));
                copy.Add(change);
                latestChange = Math.Max(latestChange, change.Revision);
            }
            if (!isFullSnapshot && latestChange != toRevision)
                throw new ArgumentException("增量帧必须包含其最终版本，不能用空帧跳过状态变化。", nameof(changes));
            SessionId = sessionId; IsFullSnapshot = isFullSnapshot; FromRevision = fromRevision; ToRevision = toRevision;
            Changes = copy.AsReadOnly();
        }
    }

    public enum HealthFrameResult : byte { Applied, Stale, WrongSession, NeedsFullSnapshot, MissingBaseline }

    /// <summary>客户端只读镜像，不提供伤害/治疗入口；状态同步不会重放死亡奖励或受击表现。</summary>
    public sealed class HealthReplica : IHealthStateSource, IDisposable
    {
        private readonly Dictionary<ulong, HealthSnapshot> _states = new Dictionary<ulong, HealthSnapshot>();
        private bool _busy;
        private bool _disposed;
        public Guid SessionId { get; }
        public ulong Revision { get; private set; }
        public bool IsInitialized { get; private set; }
        public int Count => _states.Count;
        public event Action<HealthChange> StateChanged;

        public HealthReplica(Guid sessionId)
        {
            if (sessionId == Guid.Empty) throw new ArgumentException("客户端必须显式绑定权威会话。", nameof(sessionId));
            SessionId = sessionId;
        }

        public bool TryGetHealth(HealthEntity entity, out HealthSnapshot state)
        {
            if (_states.TryGetValue(entity.EntityId, out state) && state.Entity == entity) return true;
            state = default;
            return false;
        }

        public bool TryGetEntity(ulong entityId, out HealthEntity entity)
        {
            entity = default;
            if (!_states.TryGetValue(entityId, out var state)) return false;
            entity = state.Entity;
            return true;
        }

        public HealthFrameResult Apply(HealthFrame frame)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(HealthReplica));
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            if (_busy) throw new InvalidOperationException("HealthReplica 通知期间不能重入 Apply。");
            if (frame.SessionId != SessionId) return HealthFrameResult.WrongSession;
            if (!IsInitialized && !frame.IsFullSnapshot) return HealthFrameResult.NeedsFullSnapshot;
            if (IsInitialized && frame.ToRevision <= Revision) return HealthFrameResult.Stale;
            if (!frame.IsFullSnapshot && frame.FromRevision > Revision) return HealthFrameResult.MissingBaseline;

            var notifications = new List<HealthChange>();
            _busy = true;
            List<Exception> errors = null;
            try
            {
                if (frame.IsFullSnapshot)
                {
                    var incoming = new Dictionary<ulong, HealthSnapshot>();
                    foreach (var change in frame.Changes) incoming.Add(change.State.Entity.EntityId, change.State);
                    foreach (var old in _states.Values)
                        if (!incoming.TryGetValue(old.Entity.EntityId, out var next) || next.Entity != old.Entity)
                            notifications.Add(new HealthChange(HealthChangeKind.Removed, old, frame.ToRevision));
                    foreach (var next in incoming.Values)
                    {
                        bool existed = _states.TryGetValue(next.Entity.EntityId, out var old) && old.Entity == next.Entity;
                        if (!existed || old.Revision != next.Revision)
                            notifications.Add(new HealthChange(existed ? HealthChangeKind.Updated : HealthChangeKind.Registered, next, next.Revision));
                    }
                    _states.Clear();
                    foreach (var pair in incoming) _states.Add(pair.Key, pair.Value);
                }
                else
                {
                    foreach (var change in frame.Changes)
                    {
                        // 晚加入全量可能位于此 delta 的区间中间，只应用全量之后的条目。
                        if (change.Revision <= Revision) continue;
                        ulong id = change.State.Entity.EntityId;
                        bool existed = _states.TryGetValue(id, out var old);
                        if (change.Kind == HealthChangeKind.Removed)
                        {
                            // 同一帧内发生旧实体移除、新代次注册再移除时，压缩后的 tombstone 也必须清掉旧代次。
                            if (existed)
                            {
                                _states.Remove(id);
                                notifications.Add(new HealthChange(HealthChangeKind.Removed, old, change.Revision));
                            }
                            continue;
                        }
                        if (existed && old.Entity != change.State.Entity)
                            notifications.Add(new HealthChange(HealthChangeKind.Removed, old, change.Revision));
                        _states[id] = change.State;
                        notifications.Add(new HealthChange(existed && old.Entity == change.State.Entity
                            ? HealthChangeKind.Updated : HealthChangeKind.Registered, change.State, change.Revision));
                    }
                }
                Revision = frame.ToRevision;
                IsInitialized = true;
                foreach (var notification in notifications) HealthNotifications.Send(StateChanged, notification, ref errors);
            }
            finally { _busy = false; }
            HealthNotifications.ThrowIfAny(errors);
            return HealthFrameResult.Applied;
        }

        public void Dispose()
        {
            if (_disposed) return;
            if (_busy) throw new InvalidOperationException("HealthReplica 通知期间不能 Dispose。");
            _states.Clear();
            StateChanged = null;
            _disposed = true;
        }
    }
}
