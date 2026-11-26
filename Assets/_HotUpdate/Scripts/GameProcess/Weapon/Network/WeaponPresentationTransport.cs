using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Network;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Unity.Profiling;

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
        private const byte UnnamedChannel = 0x80;
        private const string FrameStatsName = "PG.Weapon.Presentation.Frame";
        private const int WriterInitialCapacity = 1024;
        private const int WriterMaxCapacity = 64 * 1024;
        private const int MaxEventsPerMessage = 128;

        private static readonly ProfilerMarker SendChunkMarker =
            new("Weapon.Network.Presentation.SendChunk");
        private static readonly ProfilerMarker SendUnnamedMessageMarker =
            new("Weapon.Network.Presentation.SendUnnamedMessage");

        private readonly NetworkMessageTransport _transport;
        private IWeaponPresentationReceiver _receiver;
        private bool _registeredHandler;

        public bool IsInitialized { get; private set; }

        public WeaponPresentationTransport(NetworkMessageTransport transport)
        {
            _transport = transport;
        }

        public void Initialize(IWeaponPresentationReceiver receiver)
        {
            if (IsInitialized)
                return;

            _receiver = receiver;
            if (_transport.IsClient)
            {
                _transport.RegisterUnnamedHandler(UnnamedChannel, OnFrameReceived);
                _registeredHandler = true;
            }

            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (!IsInitialized)
                return;

            if (_registeredHandler && _transport.IsInitialized)
                _transport.UnregisterUnnamedHandler(UnnamedChannel);

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
            if (targetClientId == NetworkManager.ServerClientId)
                return;

            int shotIndex = 0;
            int spawnIndex = 0;
            int impactIndex = 0;
            int removedIndex = 0;
            int specialEffectIndex = 0;
            int shotTotal = shots.Count;
            int spawnTotal = spawns.Count;
            int impactTotal = impacts.Count;
            int removedTotal = removed.Count;
            int specialEffectTotal = specialEffects.Count;

            while (shotIndex < shotTotal || spawnIndex < spawnTotal ||
                   impactIndex < impactTotal || removedIndex < removedTotal ||
                   specialEffectIndex < specialEffectTotal)
            {
                int remaining = MaxEventsPerMessage;
                int shotCount = Math.Min(shotTotal - shotIndex, remaining);
                remaining -= shotCount;
                int spawnCount = Math.Min(spawnTotal - spawnIndex, remaining);
                remaining -= spawnCount;
                int impactCount = Math.Min(impactTotal - impactIndex, remaining);
                remaining -= impactCount;
                int removedCount = Math.Min(removedTotal - removedIndex, remaining);
                remaining -= removedCount;
                int specialEffectCount = Math.Min(specialEffectTotal - specialEffectIndex, remaining);

                SendFrameChunk(
                    targetClientId,
                    serverTick,
                    shots,
                    shotIndex,
                    shotCount,
                    spawns,
                    spawnIndex,
                    spawnCount,
                    impacts,
                    impactIndex,
                    impactCount,
                    removed,
                    removedIndex,
                    removedCount,
                    specialEffects,
                    specialEffectIndex,
                    specialEffectCount);

                shotIndex += shotCount;
                spawnIndex += spawnCount;
                impactIndex += impactCount;
                removedIndex += removedCount;
                specialEffectIndex += specialEffectCount;
            }
        }

        private void SendFrameChunk(
            ulong targetClientId,
            uint serverTick,
            IReadOnlyList<ShotContext> shots,
            int shotIndex,
            int shotCount,
            IReadOnlyList<ProjectileSpawn> spawns,
            int spawnIndex,
            int spawnCount,
            IReadOnlyList<ProjectileImpact> impacts,
            int impactIndex,
            int impactCount,
            IReadOnlyList<ProjectileState> removed,
            int removedIndex,
            int removedCount,
            IReadOnlyList<WeaponSpecialVfxEvent> specialEffects,
            int specialEffectIndex,
            int specialEffectCount)
        {
            using (SendChunkMarker.Auto())
            using (FastBufferWriter writer = new(
                WriterInitialCapacity, Allocator.Temp, WriterMaxCapacity))
            {
                writer.TryBeginWrite(1);
                writer.WriteByte(UnnamedChannel);
                writer.WriteValueSafe(serverTick);
                WriteCount(writer, shotCount);
                WriteCount(writer, spawnCount);
                WriteCount(writer, impactCount);
                WriteCount(writer, removedCount);
                WriteCount(writer, specialEffectCount);

                for (int i = 0; i < shotCount; i++) WriteShot(writer, shots[shotIndex + i]);
                for (int i = 0; i < spawnCount; i++) WriteSpawn(writer, spawns[spawnIndex + i]);
                for (int i = 0; i < impactCount; i++) WriteImpact(writer, impacts[impactIndex + i]);
                for (int i = 0; i < removedCount; i++)
                    WriteProjectile(writer, removed[removedIndex + i]);
                for (int i = 0; i < specialEffectCount; i++)
                    WriteSpecialEffectVfx(writer, specialEffects[specialEffectIndex + i]);

                using (SendUnnamedMessageMarker.Auto())
                    _transport.SendUnnamedToClient(
                        targetClientId,
                        FrameStatsName,
                        writer,
                        NetworkDeliveryClass.ReliableFragmentedEvent);
            }
        }

        private void OnFrameReceived(ulong senderClientId, FastBufferReader reader)
        {
            if (senderClientId != NetworkManager.ServerClientId)
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
            return count;
        }
    }
}
