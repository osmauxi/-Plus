using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>Monster 表现句柄；只接收已经插值的 Pose 和一次性动画脉冲。</summary>
    public interface IMonsterViewHandle
    {
        void SetPose(Vector2 position, float yaw);
        void PlayAttack();
    }

    public interface IMonsterViewPool
    {
        IMonsterViewHandle Rent(string localPoolId);
        void Return(IMonsterViewHandle view);
    }

    /// <summary>
    /// 客户端 Slot→View 映射。SpawnBatch 创建 View，PresentationFrame 只更新状态；
    /// ActiveMask 关闭后立即返池，Slot 本身仍不复用。
    /// </summary>
    public sealed class MonsterViewWorld : IDisposable
    {
        private readonly IMonsterViewPool _pool;
        private readonly MonsterViewCatalog _catalog;
        private readonly MonsterPoseQuantizer _quantizer;
        private IMonsterViewHandle[] _views = Array.Empty<IMonsterViewHandle>();
        private byte[] _attackSequence = Array.Empty<byte>();
        private bool[] _hasAttackSequence = Array.Empty<bool>();
        private bool _disposed;

        public int SlotCount { get; private set; }
        public int ActiveViewCount { get; private set; }

        public MonsterViewWorld(IMonsterViewPool pool, MonsterViewCatalog catalog,
            MonsterPoseQuantizer quantizer)
        {
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _quantizer = quantizer ?? throw new ArgumentNullException(nameof(quantizer));
        }

        public void ApplySpawnBatch(MonsterSpawnBatch batch)
        {
            EnsureUsable();
            if (batch == null) throw new ArgumentNullException(nameof(batch));
            if (batch.StartSlot != SlotCount)
                throw new InvalidOperationException(
                    $"Monster View SpawnBatch 必须连续追加：Expected={SlotCount}, Actual={batch.StartSlot}。");
            EnsureCapacity(checked(batch.StartSlot + batch.Count));
            var poolIds = new string[batch.Count];
            for (int i = 0; i < batch.Count; i++)
            {
                int slot = batch.StartSlot + i;
                if (_views[slot] != null) throw new InvalidOperationException($"Monster View Slot={slot} 已占用。");
                poolIds[i] = _catalog.Get(batch.Items[i].ConfigIndex).LocalPoolId;
            }

            int created = 0;
            try
            {
                for (; created < batch.Count; created++)
                {
                    int slot = batch.StartSlot + created;
                    IMonsterViewHandle view = _pool.Rent(poolIds[created]) ??
                        throw new InvalidOperationException($"Monster View Pool={poolIds[created]} 返回空句柄。");
                    _views[slot] = view;
                    _quantizer.Unpack(batch.Items[created].PackedPose, out Vector2 position, out float yaw);
                    view.SetPose(position, yaw);
                    _attackSequence[slot] = 0;
                    _hasAttackSequence[slot] = false;
                    ActiveViewCount++;
                }
                SlotCount += batch.Count;
            }
            catch
            {
                for (int i = created; i >= 0; i--)
                {
                    int slot = batch.StartSlot + i;
                    if ((uint)slot >= (uint)_views.Length || _views[slot] == null) continue;
                    IMonsterViewHandle view = _views[slot];
                    _views[slot] = null;
                    ActiveViewCount--;
                    _pool.Return(view);
                }
                throw;
            }
        }

        public void ApplyReplica(MonsterReplica replica, float interpolationAlpha)
        {
            EnsureUsable();
            if (replica == null) throw new ArgumentNullException(nameof(replica));
            if (replica.SlotCount != SlotCount)
                throw new InvalidOperationException(
                    $"Monster View/Replica SlotCount 不一致：View={SlotCount}, Replica={replica.SlotCount}。");
            for (int slot = 0; slot < SlotCount; slot++)
            {
                if (!replica.TryGetState(slot, interpolationAlpha, out MonsterReplicaState state))
                    throw new InvalidOperationException($"Monster Replica 缺少 Slot={slot}。");
                IMonsterViewHandle view = _views[slot];
                if (!state.IsActive)
                {
                    if (view != null) ReturnSlot(slot, view);
                    continue;
                }
                if (view == null)
                    throw new InvalidOperationException($"Monster Active Slot={slot} 缺少 SpawnBatch View。");
                view.SetPose(state.Position, state.Yaw);
                if ((_hasAttackSequence[slot] && state.AttackSequence != _attackSequence[slot]) ||
                    (!_hasAttackSequence[slot] && state.AttackSequence != 0))
                    view.PlayAttack();
                _attackSequence[slot] = state.AttackSequence;
                _hasAttackSequence[slot] = true;
            }
        }

        public bool TryGetView(int slot, out IMonsterViewHandle view)
        {
            EnsureUsable();
            view = null;
            if ((uint)slot >= (uint)SlotCount) return false;
            view = _views[slot];
            return view != null;
        }

        public void Clear()
        {
            EnsureUsable();
            for (int slot = 0; slot < SlotCount; slot++)
                if (_views[slot] != null) ReturnSlot(slot, _views[slot]);
            SlotCount = 0;
            ActiveViewCount = 0;
            Array.Clear(_attackSequence, 0, _attackSequence.Length);
            Array.Clear(_hasAttackSequence, 0, _hasAttackSequence.Length);
        }

        private void ReturnSlot(int slot, IMonsterViewHandle view)
        {
            _views[slot] = null;
            _attackSequence[slot] = 0;
            _hasAttackSequence[slot] = false;
            ActiveViewCount--;
            _pool.Return(view);
        }

        private void EnsureCapacity(int count)
        {
            if (count <= _views.Length) return;
            int capacity = Math.Max(64, _views.Length);
            while (capacity < count) capacity = checked(capacity * 2);
            Array.Resize(ref _views, capacity);
            Array.Resize(ref _attackSequence, capacity);
            Array.Resize(ref _hasAttackSequence, capacity);
        }

        private void EnsureUsable()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MonsterViewWorld));
        }

        public void Dispose()
        {
            if (_disposed) return;
            Clear();
            _disposed = true;
        }
    }
}
