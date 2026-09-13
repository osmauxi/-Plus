using System;
using System.Collections.Generic;

namespace ProjectGame.HotFix.Gameplay.Combat.Health
{
    /// <summary>集中存储，不对外暴露写入口；只有 HealthRuntime 驱动 System 修改条目。</summary>
    internal sealed class HealthWorld
    {
        internal sealed class Entry
        {
            internal HealthSnapshot State;
            internal readonly Dictionary<GateKey, uint> Gates = new Dictionary<GateKey, uint>();
        }

        internal readonly struct GateKey : IEquatable<GateKey>
        {
            private readonly HealthEntity _source;
            private readonly ushort _channel;
            internal GateKey(HealthEntity source, ushort channel) { _source = source; _channel = channel; }
            public bool Equals(GateKey other) => _source == other._source && _channel == other._channel;
            public override bool Equals(object obj) => obj is GateKey other && Equals(other);
            public override int GetHashCode() => unchecked(_source.GetHashCode() * 397 ^ _channel);
        }

        internal readonly struct EventKey : IEquatable<EventKey>
        {
            internal readonly HealthEntity Target;
            private readonly HealthEntity _source;
            private readonly ulong _eventId;
            internal EventKey(in HealthCommand command)
            { Target = command.Target; _source = command.Source; _eventId = command.EventId; }
            public bool Equals(EventKey other) => Target == other.Target && _source == other._source && _eventId == other._eventId;
            public override bool Equals(object obj) => obj is EventKey other && Equals(other);
            public override int GetHashCode() => unchecked((Target.GetHashCode() * 397 ^ _source.GetHashCode()) * 397 ^ _eventId.GetHashCode());
        }

        internal readonly Dictionary<ulong, Entry> Entries = new Dictionary<ulong, Entry>();
        internal readonly Dictionary<EventKey, uint> SeenEvents = new Dictionary<EventKey, uint>();
        private readonly List<EventKey> _expiredEvents = new List<EventKey>();
        private readonly List<GateKey> _expiredGates = new List<GateKey>();
        private uint _generation;
        internal ulong Revision { get; private set; }

        internal uint NextGeneration()
        {
            if (_generation == uint.MaxValue) throw new InvalidOperationException("Health 代次耗尽，请建立新的会话。");
            return ++_generation;
        }

        internal ulong NextRevision()
        {
            if (Revision == ulong.MaxValue) throw new InvalidOperationException("Health 版本耗尽，请建立新的会话。");
            return ++Revision;
        }

        internal bool TryGet(HealthEntity entity, out Entry entry)
        {
            if (Entries.TryGetValue(entity.EntityId, out entry) && entry.State.Entity == entity) return true;
            entry = null;
            return false;
        }

        internal void Prune(uint tick, uint maxCommandAge)
        {
            _expiredEvents.Clear();
            foreach (var pair in SeenEvents)
                if (unchecked(tick - pair.Value) > maxCommandAge) _expiredEvents.Add(pair.Key);
            foreach (var key in _expiredEvents) SeenEvents.Remove(key);
            foreach (var entry in Entries.Values)
            {
                _expiredGates.Clear();
                foreach (var pair in entry.Gates)
                    if (unchecked(tick - pair.Value) >= entry.State.Definition.DamageGateTicks) _expiredGates.Add(pair.Key);
                foreach (var key in _expiredGates) entry.Gates.Remove(key);
            }
        }

        internal void ForgetEvents(HealthEntity entity)
        {
            _expiredEvents.Clear();
            foreach (var key in SeenEvents.Keys)
                if (key.Target == entity) _expiredEvents.Add(key);
            foreach (var key in _expiredEvents) SeenEvents.Remove(key);
        }
    }
}
