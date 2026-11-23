using System;
using Unity.Profiling;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// ViewSlot 到 VAT 渲染状态的映射。每个 ViewSlot 只保留数据，不创建表现 GameObject。
    /// </summary>
    public sealed class MonsterViewWorld : IDisposable
    {
        private static readonly ProfilerMarker ApplyReplicaMarker =
            new("Monster.View.ApplyReplica");
        private static readonly ProfilerMarker ApplyReplicaLoopMarker =
            new("Monster.View.ApplyReplica.Loop");

        private readonly MonsterViewCatalog _catalog;
        private readonly MonsterVATRenderProfileCatalog _profiles;
        private readonly MonsterPoseQuantizer _quantizer;
        private readonly MonsterVATRenderWorld _vatWorld;
        private byte[] _attackSequence = Array.Empty<byte>();
        private byte[] _hitSequence = Array.Empty<byte>();
        private uint _lastReplicaRevision;
        private bool _disposed;

        public int ViewSlotCount => _vatWorld.ViewSlotCount;
        public int ActiveRenderCount => _vatWorld.ActiveCount;

        public MonsterViewWorld(
            MonsterViewCatalog catalog,
            MonsterVATRenderProfileCatalog profiles,
            MonsterPoseQuantizer quantizer,
            MonsterVATRenderWorld vatWorld)
        {
            _catalog = catalog;
            _profiles = profiles;
            _quantizer = quantizer;
            _vatWorld = vatWorld;
        }

        public void ApplySpawnBatch(MonsterSpawnBatch batch)
        {
            EnsureUsable();
            int startViewSlot = ViewSlotCount;
            EnsureCapacity(startViewSlot + batch.Count);
            for (int created = 0; created < batch.Count; created++)
            {
                int viewSlot = startViewSlot + created;
                MonsterViewConfig config = _catalog.Get(batch.Items[created].ConfigIndex);
                MonsterVATRenderProfile profile = _profiles.Resolve(config.LocalPoolId);
                _quantizer.Unpack(
                    batch.Items[created].PackedPose,
                    out Vector2 position,
                    out float yaw);
                _vatWorld.Spawn(viewSlot, profile, position, yaw);
                _attackSequence[viewSlot] = 0;
                _hitSequence[viewSlot] = 0;
            }
        }

        internal void SyncReplica(MonsterReplica replica, float now)
        {
            using var applyScope = ApplyReplicaMarker.Auto();
            using (ApplyReplicaLoopMarker.Auto())
            {
                for (int viewSlot = 0; viewSlot < ViewSlotCount; viewSlot++)
                {
                    replica.GetViewState(
                        viewSlot,
                        now,
                        out bool active,
                        out Vector2 pose,
                        out float yaw,
                        out Vector2 targetPose,
                        out float targetYaw,
                        out float poseEndTime,
                        out float updatesPerSecond,
                        out byte attackSequence,
                        out byte hitSequence);
                    if (!active)
                    {
                        _vatWorld.Despawn(viewSlot);
                        continue;
                    }

                    _vatWorld.SetTargetPose(
                        viewSlot,
                        pose,
                        yaw,
                        targetPose,
                        targetYaw,
                        poseEndTime,
                        updatesPerSecond,
                        Time.deltaTime);
                    if (attackSequence != _attackSequence[viewSlot])
                        _vatWorld.PlayAttack(viewSlot);
                    if (hitSequence != _hitSequence[viewSlot])
                        _vatWorld.PlayHit(viewSlot);
                    _attackSequence[viewSlot] = attackSequence;
                    _hitSequence[viewSlot] = hitSequence;
                }
            }
            _lastReplicaRevision = replica.ViewRevision;
        }

        public void Clear()
        {
            EnsureUsable();
            _vatWorld.Clear();
            Array.Clear(_attackSequence, 0, _attackSequence.Length);
            Array.Clear(_hitSequence, 0, _hitSequence.Length);
        }

        public void Update(
            MonsterReplica replica,
            float deltaTime,
            float unscaledDeltaTime,
            float now)
        {
            EnsureUsable();
            if (_lastReplicaRevision != replica.ViewRevision)
                SyncReplica(replica, now);
            _vatWorld.Update(deltaTime, unscaledDeltaTime, now);
        }

        public void Render()
        {
            EnsureUsable();
            _vatWorld.Render();
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
