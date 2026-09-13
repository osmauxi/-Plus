using System;
using System.Collections.Generic;

namespace ProjectGame.HotFix.Gameplay.Combat.Health
{
    /// <summary>
    /// 集中维护命中对象到 HealthEntity 的映射；T 可为 Collider，无需每个怪物/碰撞体挂 Health 脚本。
    /// 生成器在注册实体后绑定全部碰撞体；动态增加/替换碰撞体时重新 Bind。
    /// </summary>
    public sealed class HealthEntityBindings<T> : IDisposable where T : class
    {
        private readonly IHealthStateSource _source;
        private readonly Dictionary<T, HealthEntity> _entities;
        private readonly Dictionary<HealthEntity, HashSet<T>> _bindings = new Dictionary<HealthEntity, HashSet<T>>();
        private readonly IEqualityComparer<T> _comparer;
        private bool _disposed;
        public int Count => _entities.Count;

        public HealthEntityBindings(IHealthStateSource source, IEqualityComparer<T> comparer = null)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _comparer = comparer ?? EqualityComparer<T>.Default;
            _entities = new Dictionary<T, HealthEntity>(_comparer);
            _source.StateChanged += OnStateChanged;
        }

        /// <summary>原子替换实体的整组映射；冲突/空对象不会覆盖已有实体的绑定。</summary>
        public void Bind(HealthEntity entity, IEnumerable<T> objects)
        {
            EnsureUsable();
            if (!_source.TryGetHealth(entity, out _)) throw new ArgumentException("Health 实体未注册或代次已失效。", nameof(entity));
            if (objects == null) throw new ArgumentNullException(nameof(objects));
            var next = new HashSet<T>(_comparer);
            foreach (var item in objects)
            {
                if (item == null) throw new ArgumentException("命中对象不能为空。", nameof(objects));
                if (_entities.TryGetValue(item, out var owner) && owner != entity)
                    throw new InvalidOperationException("命中对象已经绑定给另一个实体。");
                next.Add(item);
            }
            Unbind(entity);
            if (next.Count == 0) return;
            _bindings.Add(entity, next);
            foreach (var item in next) _entities.Add(item, entity);
        }

        public bool TryResolve(T item, out HealthEntity entity)
        {
            EnsureUsable();
            entity = default;
            if (item == null || !_entities.TryGetValue(item, out var found)) return false;
            if (!_source.TryGetHealth(found, out _)) { Unbind(found); return false; }
            entity = found;
            return true;
        }

        public bool Unbind(HealthEntity entity)
        {
            EnsureUsable();
            if (!_bindings.TryGetValue(entity, out var objects)) return false;
            foreach (var item in objects) _entities.Remove(item);
            _bindings.Remove(entity);
            return true;
        }

        private void OnStateChanged(HealthChange change)
        {
            if (change.Kind == HealthChangeKind.Removed) Unbind(change.State.Entity);
        }

        private void EnsureUsable()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(HealthEntityBindings<T>));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _source.StateChanged -= OnStateChanged;
            _entities.Clear(); _bindings.Clear(); _disposed = true;
        }
    }
}
