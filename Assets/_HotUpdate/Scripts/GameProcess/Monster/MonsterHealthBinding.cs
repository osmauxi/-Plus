using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Combat.Health;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    public readonly struct MonsterDeathReaction
    {
        public readonly int Slot;
        public readonly DamageOriginType Origin;
        public readonly UnityEngine.Vector3 HitPoint;
        public readonly UnityEngine.Vector3 Direction;

        public MonsterDeathReaction(int slot, DamageOriginType origin,
            UnityEngine.Vector3 hitPoint, UnityEngine.Vector3 direction)
        {
            Slot = slot;
            Origin = origin;
            HitPoint = hitPoint;
            Direction = direction;
        }
    }

    /// <summary>
    /// 维护当前房间 Slot 与集中 HealthEntity 的双向关系。
    /// Health 死亡批次发布后只关闭 MonsterWorld Slot，不在回调内重入注销 Health。
    /// </summary>
    public sealed class MonsterHealthBinding : IDisposable
    {
        private readonly MonsterWorld _world;
        private readonly HealthRuntime _health;
        private readonly ulong _entityIdBase;
        private readonly Dictionary<HealthEntity, int> _slots = new Dictionary<HealthEntity, int>();
        private HealthEntity[] _healthBySlot = Array.Empty<HealthEntity>();
        private bool _disposed;

        public int Count => _slots.Count;
        public event Action<int, HealthResult> MonsterDied;
        public event Action<MonsterDeathReaction> DeathReactionRequested;

        /// <param name="entityIdBase">
        /// HealthRuntime 内为 Monster Slot 预留的 EntityId 起点。独立 Monster HealthRuntime 可使用 0；
        /// 与其他实体共享 Runtime 时由装配层提供无冲突区间。
        /// </param>
        public MonsterHealthBinding(MonsterWorld world, HealthRuntime health, ulong entityIdBase = 0)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _entityIdBase = entityIdBase;
            _health.Died += OnDied;
            _health.StateChanged += OnHealthChanged;
        }

        public HealthEntity Register(int slot, int healthProfileId, float initialShield = 0)
        {
            EnsureUsable();
            if (!_world.IsActive(slot)) throw new ArgumentOutOfRangeException(nameof(slot), "只能为存活 Monster Slot 注册 Health。");
            EnsureCapacity(slot + 1);
            if (_healthBySlot[slot].IsValid) throw new InvalidOperationException($"Monster Slot={slot} 已绑定 Health。");
            ulong entityId = checked(_entityIdBase + (ulong)slot);
            HealthEntity entity = _health.Register(entityId, healthProfileId, initialShield);
            _healthBySlot[slot] = entity;
            _slots.Add(entity, slot);
            return entity;
        }

        public bool TryGetHealth(int slot, out HealthEntity entity)
        {
            EnsureUsable();
            entity = default;
            if ((uint)slot >= (uint)_healthBySlot.Length || !_healthBySlot[slot].IsValid) return false;
            entity = _healthBySlot[slot];
            return true;
        }

        public bool TryGetSlot(HealthEntity entity, out int slot)
        {
            EnsureUsable();
            return _slots.TryGetValue(entity, out slot);
        }

        public bool Unregister(int slot)
        {
            EnsureUsable();
            if ((uint)slot >= (uint)_healthBySlot.Length || !_healthBySlot[slot].IsValid) return false;
            HealthEntity entity = _healthBySlot[slot];
            // HealthRuntime 的 Removed 通知会同步移除本地映射。
            return _health.Unregister(entity);
        }

        public void Clear()
        {
            EnsureUsable();
            for (int slot = 0; slot < _healthBySlot.Length; slot++)
            {
                HealthEntity entity = _healthBySlot[slot];
                if (entity.IsValid) _health.Unregister(entity);
            }
            _slots.Clear();
            Array.Clear(_healthBySlot, 0, _healthBySlot.Length);
        }

        private void OnDied(HealthResult result)
        {
            if (!_slots.TryGetValue(result.After.Entity, out int slot)) return;
            if (!_world.Kill(slot)) return;
            MonsterDied?.Invoke(slot, result);
            HealthImpact impact = result.Command.Impact;
            if (impact.Origin == DamageOriginType.Projectile)
                DeathReactionRequested?.Invoke(new MonsterDeathReaction(slot, impact.Origin,
                    impact.Point, -impact.Direction));
        }

        private void OnHealthChanged(HealthChange change)
        {
            if (change.Kind != HealthChangeKind.Removed || !_slots.TryGetValue(change.State.Entity, out int slot)) return;
            _slots.Remove(change.State.Entity);
            if ((uint)slot < (uint)_healthBySlot.Length && _healthBySlot[slot] == change.State.Entity)
                _healthBySlot[slot] = default;
        }

        private void EnsureCapacity(int count)
        {
            if (count <= _healthBySlot.Length) return;
            int capacity = Math.Max(64, _healthBySlot.Length);
            while (capacity < count) capacity = checked(capacity * 2);
            Array.Resize(ref _healthBySlot, capacity);
        }

        private void EnsureUsable()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MonsterHealthBinding));
        }

        public void Dispose()
        {
            if (_disposed) return;
            Clear();
            _health.Died -= OnDied;
            _health.StateChanged -= OnHealthChanged;
            MonsterDied = null;
            DeathReactionRequested = null;
            _disposed = true;
        }
    }
}
