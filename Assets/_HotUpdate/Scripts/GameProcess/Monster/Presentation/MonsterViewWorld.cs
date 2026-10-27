using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>Monster 表现句柄；只承载池化实例和渲染资源，不参与权威 Pose 写入。</summary>
    public interface IMonsterViewHandle
    {
        GameObject Instance { get; }
        MonsterVATRenderProfile RenderProfile { get; }
        void BindSlot(int slot);
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
        private readonly MonsterVATRenderWorld _vatWorld;
        private IMonsterViewHandle[] _views = Array.Empty<IMonsterViewHandle>();
        private byte[] _attackSequence = Array.Empty<byte>();
        private byte[] _hitSequence = Array.Empty<byte>();
        private bool _disposed;

        public int SlotCount { get; private set; }
        public int ActiveViewCount { get; private set; }

        public MonsterViewWorld(IMonsterViewPool pool, MonsterViewCatalog catalog,
            MonsterPoseQuantizer quantizer, MonsterVATRenderWorld vatWorld)
        {
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _quantizer = quantizer ?? throw new ArgumentNullException(nameof(quantizer));
            _vatWorld = vatWorld ?? throw new ArgumentNullException(nameof(vatWorld));
        }
        /// <summary>
        /// 使用MonsterSpawnBatch从池中拿对应的预制件
        /// </summary>
        public void ApplySpawnBatch(MonsterSpawnBatch batch)
        {
            EnsureUsable();
            if (batch == null) 
                throw new ArgumentNullException(nameof(batch));
            if (batch.StartSlot != SlotCount)
                throw new InvalidOperationException(
                    $"Monster View SpawnBatch 必须连续追加：Expected={SlotCount}, Actual={batch.StartSlot}。");
            EnsureCapacity(checked(batch.StartSlot + batch.Count));

            int created = 0;
            try
            {
                for (; created < batch.Count; created++)
                {
                    int slot = batch.StartSlot + created;
                    if (_views[slot] != null)
                        throw new InvalidOperationException($"Monster View Slot={slot} 已占用。");
                    string poolId = _catalog.Get(batch.Items[created].ConfigIndex).LocalPoolId;
                    IMonsterViewHandle view = _pool.Rent(poolId) ??
                        throw new InvalidOperationException($"Monster View Pool={poolId} 返回空句柄。");
                    //解包后赋值
                    _views[slot] = view;
                    view.BindSlot(slot);
                    _quantizer.Unpack(batch.Items[created].PackedPose, out Vector2 position, out float yaw);
                    _vatWorld.Spawn(slot, view.RenderProfile, position, yaw);
                    _attackSequence[slot] = 0;
                    _hitSequence[slot] = 0;
                    ActiveViewCount++;
                }
                SlotCount += batch.Count;
            }
            catch
            {
                //失败把所有创建的全部return
                for (int i = created; i >= 0; i--)
                {
                    int slot = batch.StartSlot + i;
                    if ((uint)slot >= (uint)_views.Length || _views[slot] == null) 
                        continue;
                    IMonsterViewHandle view = _views[slot];
                    _views[slot] = null;
                    _vatWorld.Despawn(slot);
                    ActiveViewCount--;
                    _pool.Return(view);
                }
                throw;
            }
        }

        public void ApplyReplica(MonsterReplica replica, float interpolationAlpha)
        {
            EnsureUsable();
            if (replica == null) 
                throw new ArgumentNullException(nameof(replica));
            if (replica.SlotCount != SlotCount)
                throw new InvalidOperationException(
                    $"Monster View/Replica SlotCount 不一致：View={SlotCount}, Replica={replica.SlotCount}。");
            //遍历，从replica取插值的包，然后赋值
            for (int slot = 0; slot < SlotCount; slot++)
            {
                if (!replica.TryGetState(slot, interpolationAlpha, out MonsterReplicaState state))
                    throw new InvalidOperationException($"Monster Replica 缺少 Slot={slot}。");            
                ApplyState(slot, state.IsActive, state.Position, state.Yaw,
                    state.AttackSequence, state.HitSequence);
            }
        }

        public void Clear()
        {
            EnsureUsable();
            for (int slot = 0; slot < SlotCount; slot++)
                if (_views[slot] != null) ReturnSlot(slot, _views[slot]);
            SlotCount = 0;
            ActiveViewCount = 0;
            Array.Clear(_attackSequence, 0, _attackSequence.Length);
            Array.Clear(_hitSequence, 0, _hitSequence.Length);
        }

        public void Update(float deltaTime, float unscaledDeltaTime)
        {
            EnsureUsable();
            _vatWorld.Update(deltaTime, unscaledDeltaTime);
        }

        public void Render()
        {
            EnsureUsable();
            _vatWorld.Render();
        }

        private void ReturnSlot(int slot, IMonsterViewHandle view)
        {
            _views[slot] = null;
            _attackSequence[slot] = 0;
            _hitSequence[slot] = 0;
            ActiveViewCount--;
            _vatWorld.Despawn(slot);
            _pool.Return(view);
        }

        private void ApplyState(int slot, bool isActive, Vector2 position, float yaw,
            byte attackSequence, byte hitSequence)
        {
            IMonsterViewHandle view = _views[slot];
            if (!isActive)
            {
                //死了回池，这里指表现层，表现复用随便用，但是Slot内实际数据是不能退掉复用的
                if (view != null)
                    ReturnSlot(slot, view);
                return;
            }
            if (view == null)
                throw new InvalidOperationException($"Monster Active Slot={slot} 缺少 SpawnBatch View。");
            _vatWorld.SetPose(slot, position, yaw, Time.deltaTime);
            //只检测变不变，天然支持回绕
            if (attackSequence != _attackSequence[slot])
                _vatWorld.PlayAttack(slot);
            if (hitSequence != _hitSequence[slot])
                _vatWorld.PlayHit(slot);
            _attackSequence[slot] = attackSequence;
            _hitSequence[slot] = hitSequence;
        }

        private void EnsureCapacity(int count)
        {
            if (count <= _views.Length) 
                return;
            int capacity = Math.Max(64, _views.Length);
            while (capacity < count) 
                capacity = checked(capacity * 2);
            Array.Resize(ref _views, capacity);
            Array.Resize(ref _attackSequence, capacity);
            Array.Resize(ref _hitSequence, capacity);
        }

        private void EnsureUsable()
        {
            if (_disposed) 
                throw new ObjectDisposedException(nameof(MonsterViewWorld));
        }

        public void Dispose()
        {
            if (_disposed)
               return;
            Clear();
            _disposed = true;
        }
    }
}
