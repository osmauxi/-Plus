using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// Slot 到 VAT 渲染状态的映射。每个 Slot 只保留数据，不创建表现 GameObject。
    /// </summary>
    public sealed class MonsterViewWorld : IDisposable
    {
        private readonly MonsterViewCatalog _catalog;
        private readonly MonsterVATRenderProfileCatalog _profiles;
        private readonly MonsterPoseQuantizer _quantizer;
        private readonly MonsterVATRenderWorld _vatWorld;
        private byte[] _attackSequence = Array.Empty<byte>();
        private byte[] _hitSequence = Array.Empty<byte>();
        private bool _disposed;

        public int SlotCount => _vatWorld.SlotCount;
        public int ActiveRenderCount => _vatWorld.ActiveCount;

        public MonsterViewWorld(
            MonsterViewCatalog catalog,
            MonsterVATRenderProfileCatalog profiles,
            MonsterPoseQuantizer quantizer,
            MonsterVATRenderWorld vatWorld)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
            _quantizer = quantizer ?? throw new ArgumentNullException(nameof(quantizer));
            _vatWorld = vatWorld ?? throw new ArgumentNullException(nameof(vatWorld));
        }

        public void ApplySpawnBatch(MonsterSpawnBatch batch)
        {
            EnsureUsable();
            if (batch == null)
                throw new ArgumentNullException(nameof(batch));
            if (batch.StartSlot != SlotCount)
                throw new InvalidOperationException(
                    $"Monster Render SpawnBatch 必须连续追加：Expected={SlotCount}, Actual={batch.StartSlot}。");

            EnsureCapacity(batch.StartSlot + batch.Count);
            for (int created = 0; created < batch.Count; created++)
            {
                int slot = batch.StartSlot + created;
                MonsterViewConfig config = _catalog.Get(batch.Items[created].ConfigIndex);
                MonsterVATRenderProfile profile = _profiles.Resolve(config.LocalPoolId);
                _quantizer.Unpack(
                    batch.Items[created].PackedPose,
                    out Vector2 position,
                    out float yaw);
                _vatWorld.Spawn(slot, profile, position, yaw);
                _attackSequence[slot] = 0;
                _hitSequence[slot] = 0;
            }
        }

        public void ApplyReplica(MonsterReplica replica, float interpolationAlpha)
        {
            EnsureUsable();
            if (replica == null)
                throw new ArgumentNullException(nameof(replica));
            if (replica.SlotCount != SlotCount)
                throw new InvalidOperationException(
                    $"Monster Render/Replica SlotCount 不一致：Render={SlotCount}, Replica={replica.SlotCount}。");

            for (int slot = 0; slot < SlotCount; slot++)
            {
                if (!replica.TryGetState(slot, interpolationAlpha, out MonsterReplicaState state))
                    throw new InvalidOperationException($"Monster Replica 缺少 Slot={slot}。");
                ApplyState(
                    slot,
                    state.IsActive,
                    state.Position,
                    state.Yaw,
                    state.AttackSequence,
                    state.HitSequence);
            }
        }

        public void Clear()
        {
            EnsureUsable();
            _vatWorld.Clear();
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

        private void ApplyState(
            int slot,
            bool isActive,
            Vector2 position,
            float yaw,
            byte attackSequence,
            byte hitSequence)
        {
            if (!isActive)
            {
                _vatWorld.Despawn(slot);
                return;
            }

            _vatWorld.SetPose(slot, position, yaw, Time.deltaTime);
            if (attackSequence != _attackSequence[slot])
                _vatWorld.PlayAttack(slot);
            if (hitSequence != _hitSequence[slot])
                _vatWorld.PlayHit(slot);
            _attackSequence[slot] = attackSequence;
            _hitSequence[slot] = hitSequence;
        }

        private void EnsureCapacity(int count)
        {
            if (count <= _attackSequence.Length)
                return;
            int capacity = Math.Max(64, _attackSequence.Length);
            while (capacity < count)
                capacity = checked(capacity * 2);
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
