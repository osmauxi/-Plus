using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Network;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Weapon.Presentation
{
    /// <summary>Weapon 表现协议的本地接收端。只接收服务端已经确认的表现事实。</summary>
    public interface IWeaponPresentationReceiver
    {
        void ReceiveShot(in ShotContext shot);
        void ReceiveProjectileSpawn(in ProjectileSpawn spawn);
        void ReceiveProjectileImpact(in ProjectileImpact impact);
        void ReceiveProjectileRemoved(in ProjectileState projectile);
        void ReceiveSpecialEffectVfx(in WeaponSpecialVfxEvent effect);
    }

    /// <summary>
    /// Weapon 表现层的 Named Message 协议。
    /// 同一个服务器 Tick 内的开枪、出生、命中、移除和特殊表现合并成一条消息。
    /// </summary>
    public sealed class WeaponPresentationTransport
    {
        private const string FrameMessageName = "PG.Weapon.Presentation.Frame";
        private const int WriterInitialCapacity = 1024;
        private const int WriterMaxCapacity = 64 * 1024;
        private const int MaxEventsPerFrame = 4096;

        private readonly NetworkMessageTransport _transport;
        private IWeaponPresentationReceiver _receiver;
        private bool _registeredHandler;

        public bool IsInitialized { get; private set; }

        public WeaponPresentationTransport(NetworkMessageTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public void Initialize(IWeaponPresentationReceiver receiver)
        {
            if (IsInitialized)
                return;
            if (!_transport.IsInitialized)
                throw new InvalidOperationException("Gameplay 通用 Transport 尚未初始化。");

            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            if (_transport.IsClient)
            {
                _transport.RegisterHandler(FrameMessageName, OnFrameReceived);
                _registeredHandler = true;
            }

            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (!IsInitialized)
                return;

            if (_registeredHandler && _transport.IsInitialized)
                _transport.UnregisterHandler(FrameMessageName);

            _registeredHandler = false;
            _receiver = null;
            IsInitialized = false;
        }

        public void SendFrame(
            ulong targetClientId,
            uint serverTick,
            IReadOnlyList<ShotContext> shots,
            IReadOnlyList<ProjectileSpawn> spawns,
            IReadOnlyList<ProjectileImpact> impacts,
            IReadOnlyList<ProjectileState> removed,
            IReadOnlyList<WeaponSpecialVfxEvent> specialEffects)
        {
            EnsureInitialized();
            if (!_transport.IsServer || targetClientId == NetworkManager.ServerClientId)
                return;

            ValidateCount(shots?.Count ?? 0, nameof(shots));
            ValidateCount(spawns?.Count ?? 0, nameof(spawns));
            ValidateCount(impacts?.Count ?? 0, nameof(impacts));
            ValidateCount(removed?.Count ?? 0, nameof(removed));
            ValidateCount(specialEffects?.Count ?? 0, nameof(specialEffects));

            using FastBufferWriter writer = new(
                WriterInitialCapacity, Allocator.Temp, WriterMaxCapacity);

            writer.WriteValueSafe(serverTick);
            WriteCount(writer, shots?.Count ?? 0);
            WriteCount(writer, spawns?.Count ?? 0);
            WriteCount(writer, impacts?.Count ?? 0);
            WriteCount(writer, removed?.Count ?? 0);
            WriteCount(writer, specialEffects?.Count ?? 0);

            if (shots != null)
                for (int i = 0; i < shots.Count; i++) WriteShot(writer, shots[i]);
            if (spawns != null)
                for (int i = 0; i < spawns.Count; i++) WriteSpawn(writer, spawns[i]);
            if (impacts != null)
                for (int i = 0; i < impacts.Count; i++) WriteImpact(writer, impacts[i]);
            if (removed != null)
                for (int i = 0; i < removed.Count; i++) WriteProjectile(writer, removed[i]);
            if (specialEffects != null)
                for (int i = 0; i < specialEffects.Count; i++)
                    WriteSpecialEffectVfx(writer, specialEffects[i]);

            _transport.SendToClient(
                targetClientId,
                FrameMessageName,
                writer,
                specialEffects != null && specialEffects.Count > 0
                    ? NetworkDeliveryClass.ReliableEvent
                    : NetworkDeliveryClass.UnreliableEvent);
        }

        private void OnFrameReceived(ulong senderClientId, FastBufferReader reader)
        {
            if (!_transport.IsClient || senderClientId != NetworkManager.ServerClientId || _receiver == null)
                return;

            try
            {
                // Tick 当前用于协议诊断；各事件自身保留 FireTick/最终位置等必要数据。
                reader.ReadValueSafe(out uint _);
                int shotCount = ReadCount(reader);
                int spawnCount = ReadCount(reader);
                int impactCount = ReadCount(reader);
                int removedCount = ReadCount(reader);
                int specialEffectCount = ReadCount(reader);

                for (int i = 0; i < shotCount; i++)
                {
                    ShotContext shot = ReadShot(reader);
                    _receiver.ReceiveShot(shot);
                }

                for (int i = 0; i < spawnCount; i++)
                {
                    ProjectileSpawn spawn = ReadSpawn(reader);
                    _receiver.ReceiveProjectileSpawn(spawn);
                }

                for (int i = 0; i < impactCount; i++)
                {
                    ProjectileImpact impact = ReadImpact(reader);
                    _receiver.ReceiveProjectileImpact(impact);
                }

                for (int i = 0; i < removedCount; i++)
                {
                    ProjectileState projectile = ReadProjectile(reader);
                    _receiver.ReceiveProjectileRemoved(projectile);
                }

                for (int i = 0; i < specialEffectCount; i++)
                {
                    WeaponSpecialVfxEvent effect = ReadSpecialEffectVfx(reader);
                    _receiver.ReceiveSpecialEffectVfx(effect);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[{nameof(WeaponPresentationTransport)}] 表现消息解析失败：{exception.Message}");
            }
        }

        private static void WriteShot(FastBufferWriter writer, in ShotContext shot)
        {
            writer.WriteValueSafe(shot.ShotId);
            writer.WriteValueSafe(shot.FireTick);
            writer.WriteValueSafe(shot.ShotSequence);
            writer.WriteValueSafe(shot.OwnerEntityId);
            writer.WriteValueSafe(shot.WeaponId);
            writer.WriteValueSafe(shot.RandomSeed);
            writer.WriteValueSafe(shot.Origin);
            writer.WriteValueSafe(shot.AimDirection);
            writer.WriteValueSafe(shot.StatSnapshotId);
            writer.WriteValueSafe(shot.EffectSetId);
        }

        private static ShotContext ReadShot(FastBufferReader reader)
        {
            ShotContext shot = default;
            reader.ReadValueSafe(out shot.ShotId);
            reader.ReadValueSafe(out shot.FireTick);
            reader.ReadValueSafe(out shot.ShotSequence);
            reader.ReadValueSafe(out shot.OwnerEntityId);
            reader.ReadValueSafe(out shot.WeaponId);
            reader.ReadValueSafe(out shot.RandomSeed);
            reader.ReadValueSafe(out shot.Origin);
            reader.ReadValueSafe(out shot.AimDirection);
            reader.ReadValueSafe(out shot.StatSnapshotId);
            reader.ReadValueSafe(out shot.EffectSetId);
            return shot;
        }

        private static void WriteSpawn(FastBufferWriter writer, in ProjectileSpawn spawn)
        {
            writer.WriteValueSafe(spawn.SpawnTick);
            writer.WriteValueSafe(spawn.VisualSize);
            WriteProjectile(writer, spawn.Projectile);
        }

        private static ProjectileSpawn ReadSpawn(FastBufferReader reader)
        {
            reader.ReadValueSafe(out uint spawnTick);
            reader.ReadValueSafe(out float visualSize);
            ProjectileState projectile = ReadProjectile(reader);
            ShotContext shot = new()
            {
                ShotId = projectile.ShotId,
            };
            return new ProjectileSpawn(shot, projectile, visualSize, spawnTick);
        }

        private static void WriteImpact(FastBufferWriter writer, in ProjectileImpact impact)
        {
            WriteProjectile(writer, impact.Projectile);
            writer.WriteValueSafe(impact.Point);
            writer.WriteValueSafe(impact.Normal);
            writer.WriteValueSafe(impact.HasTarget);
            writer.WriteValueSafe(impact.TargetEntityId);
            writer.WriteValueSafe(impact.VfxWeight);
            writer.WriteValueSafe((byte)impact.Resolution);
        }

        private static ProjectileImpact ReadImpact(FastBufferReader reader)
        {
            ProjectileState projectile = ReadProjectile(reader);
            ProjectileImpact impact = new()
            {
                Shot = new ShotContext { ShotId = projectile.ShotId },
                Projectile = projectile,
            };
            reader.ReadValueSafe(out impact.Point);
            reader.ReadValueSafe(out impact.Normal);
            reader.ReadValueSafe(out impact.HasTarget);
            reader.ReadValueSafe(out impact.TargetEntityId);
            reader.ReadValueSafe(out impact.VfxWeight);
            reader.ReadValueSafe(out byte resolution);
            impact.Resolution = (ProjectileHitResolution)resolution;
            return impact;
        }

        private static void WriteProjectile(FastBufferWriter writer, in ProjectileState projectile)
        {
            writer.WriteValueSafe(projectile.ProjectileId);
            writer.WriteValueSafe(projectile.ShotId);
            writer.WriteValueSafe(projectile.Position);
            writer.WriteValueSafe(projectile.Velocity);
            writer.WriteValueSafe(projectile.RemainingLifeTime);
            writer.WriteValueSafe(projectile.DamageMultiplier);
            writer.WriteValueSafe(projectile.SizeMultiplier);
            writer.WriteValueSafe(projectile.PierceRemaining);
            writer.WriteValueSafe(projectile.BounceRemaining);
            writer.WriteValueSafe(projectile.Generation);
            writer.WriteValueSafe(projectile.HitCount);
            writer.WriteValueSafe((ushort)projectile.Flags);
        }

        private static ProjectileState ReadProjectile(FastBufferReader reader)
        {
            ProjectileState projectile = default;
            reader.ReadValueSafe(out projectile.ProjectileId);
            reader.ReadValueSafe(out projectile.ShotId);
            reader.ReadValueSafe(out projectile.Position);
            reader.ReadValueSafe(out projectile.Velocity);
            reader.ReadValueSafe(out projectile.RemainingLifeTime);
            reader.ReadValueSafe(out projectile.DamageMultiplier);
            reader.ReadValueSafe(out projectile.SizeMultiplier);
            reader.ReadValueSafe(out projectile.PierceRemaining);
            reader.ReadValueSafe(out projectile.BounceRemaining);
            reader.ReadValueSafe(out projectile.Generation);
            reader.ReadValueSafe(out projectile.HitCount);
            reader.ReadValueSafe(out ushort flags);
            projectile.Flags = (ProjectileFlags)flags;
            return projectile;
        }

        private static void WriteSpecialEffectVfx(
            FastBufferWriter writer, in WeaponSpecialVfxEvent effect)
        {
            writer.WriteValueSafe((byte)effect.Type);
            writer.WriteValueSafe(effect.InstanceId);
            writer.WriteValueSafe(effect.OwnerEntityId);
            writer.WriteValueSafe(effect.Position);
            writer.WriteValueSafe(effect.TargetPosition);
            writer.WriteValueSafe(effect.Radius);
            writer.WriteValueSafe(effect.Duration);
        }

        private static WeaponSpecialVfxEvent ReadSpecialEffectVfx(FastBufferReader reader)
        {
            reader.ReadValueSafe(out byte type);
            reader.ReadValueSafe(out ulong instanceId);
            reader.ReadValueSafe(out ulong ownerEntityId);
            reader.ReadValueSafe(out Vector3 position);
            reader.ReadValueSafe(out Vector3 targetPosition);
            reader.ReadValueSafe(out float radius);
            reader.ReadValueSafe(out float duration);
            return new WeaponSpecialVfxEvent(
                (WeaponSpecialVfxType)type,
                instanceId,
                ownerEntityId,
                position,
                targetPosition,
                radius,
                duration);
        }

        private static void WriteCount(FastBufferWriter writer, int count) =>
            writer.WriteValueSafe((ushort)count);

        private static int ReadCount(FastBufferReader reader)
        {
            reader.ReadValueSafe(out ushort count);
            if (count > MaxEventsPerFrame)
                throw new InvalidOperationException($"单帧 Weapon 表现事件过多：{count}");
            return count;
        }

        private static void ValidateCount(int count, string name)
        {
            if (count < 0 || count > MaxEventsPerFrame)
                throw new ArgumentOutOfRangeException(name, count, $"单帧事件数必须位于 0~{MaxEventsPerFrame}。");
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException($"{nameof(WeaponPresentationTransport)} 尚未初始化。");
        }
    }
}
