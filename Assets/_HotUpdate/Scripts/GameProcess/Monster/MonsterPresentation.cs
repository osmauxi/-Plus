using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>房间局部 X/Z 各 12 bit、Yaw 8 bit 的独立完整 Pose。</summary>
    public sealed class MonsterPoseQuantizer
    {
        private const int PositionMax = 4095;
        private readonly Vector2 _origin;
        private readonly Vector2 _size;

        public MonsterPoseQuantizer(Vector2 origin, Vector2 size)
        {
            if (!MonsterMath.IsFinite(origin) || !MonsterMath.IsPositiveFinite(size.x) ||
                !MonsterMath.IsPositiveFinite(size.y))
                throw new ArgumentOutOfRangeException(nameof(size));
            _origin = origin;
            _size = size;
        }

        public uint Pack(Vector2 position, float yaw)
        {
            if (!MonsterMath.IsFinite(position) || !MonsterMath.IsFinite(yaw))
                throw new ArgumentOutOfRangeException(nameof(position));
            int x = Mathf.RoundToInt(Mathf.Clamp01((position.x - _origin.x) / _size.x) * PositionMax);
            int z = Mathf.RoundToInt(Mathf.Clamp01((position.y - _origin.y) / _size.y) * PositionMax);
            float normalizedYaw = Mathf.Repeat(yaw, 360f);
            int packedYaw = Mathf.RoundToInt(normalizedYaw * (256f / 360f)) & 0xFF;
            return (uint)(x | (z << 12) | (packedYaw << 24));
        }

        public void Unpack(uint packed, out Vector2 position, out float yaw)
        {
            int x = (int)(packed & 0xFFF);
            int z = (int)((packed >> 12) & 0xFFF);
            int packedYaw = (int)(packed >> 24);
            position = new Vector2(
                _origin.x + x * (_size.x / PositionMax),
                _origin.y + z * (_size.y / PositionMax));
            yaw = packedYaw * (360f / 256f);
        }
    }

    public readonly struct MonsterPackedState
    {
        public readonly uint PackedPose;
        public readonly byte State;
        public byte AttackSequence => State;
        public MonsterPackedState(uint packedPose, byte state)
        { PackedPose = packedPose; State = state; }
    }

    /// <summary>ActiveMask 后只排列存活 Slot 的 5B 表现状态；单帧可独立恢复当前表现。</summary>
    public sealed class MonsterPresentationFrame
    {
        private readonly byte[] _activeMask;
        private readonly MonsterPackedState[] _activeStates;
        private readonly IReadOnlyList<byte> _readOnlyActiveMask;
        private readonly IReadOnlyList<MonsterPackedState> _readOnlyActiveStates;
        public uint ServerTick { get; }
        public int SlotCount { get; }
        public IReadOnlyList<byte> ActiveMask => _readOnlyActiveMask;
        public IReadOnlyList<MonsterPackedState> ActiveStates => _readOnlyActiveStates;

        public MonsterPresentationFrame(uint serverTick, int slotCount,
            IReadOnlyList<byte> activeMask, IReadOnlyList<MonsterPackedState> activeStates)
        {
            if (slotCount < 0) throw new ArgumentOutOfRangeException(nameof(slotCount));
            if (activeMask == null) throw new ArgumentNullException(nameof(activeMask));
            if (activeStates == null) throw new ArgumentNullException(nameof(activeStates));
            int maskLength = (slotCount + 7) / 8;
            if (activeMask.Count != maskLength) throw new ArgumentException("Monster ActiveMask 长度与 SlotCount 不一致。", nameof(activeMask));
            _activeMask = new byte[maskLength];
            for (int i = 0; i < maskLength; i++) _activeMask[i] = activeMask[i];
            if (maskLength > 0 && (slotCount & 7) != 0)
            {
                int validBits = slotCount & 7;
                int invalidMask = ~((1 << validBits) - 1) & 0xFF;
                if ((_activeMask[maskLength - 1] & invalidMask) != 0)
                    throw new ArgumentException("Monster ActiveMask 在 SlotCount 外设置了 bit。", nameof(activeMask));
            }
            int activeCount = 0;
            for (int slot = 0; slot < slotCount; slot++) if (IsSet(_activeMask, slot)) activeCount++;
            if (activeStates.Count != activeCount)
                throw new ArgumentException("Monster ActiveStates 数量与 ActiveMask 不一致。", nameof(activeStates));
            _activeStates = new MonsterPackedState[activeCount];
            for (int i = 0; i < activeCount; i++) _activeStates[i] = activeStates[i];
            _readOnlyActiveMask = Array.AsReadOnly(_activeMask);
            _readOnlyActiveStates = Array.AsReadOnly(_activeStates);
            ServerTick = serverTick;
            SlotCount = slotCount;
        }

        public bool IsActive(int slot)
        {
            if ((uint)slot >= (uint)SlotCount) throw new ArgumentOutOfRangeException(nameof(slot));
            return IsSet(_activeMask, slot);
        }

        internal static bool IsSet(byte[] mask, int slot) => (mask[slot >> 3] & (1 << (slot & 7))) != 0;
    }

    public static class MonsterPresentationPackingSystem
    {
        public static MonsterPresentationFrame Capture(MonsterWorld world, uint serverTick,
            MonsterPoseQuantizer quantizer)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (quantizer == null) throw new ArgumentNullException(nameof(quantizer));
            var mask = new byte[(world.SlotCount + 7) / 8];
            var states = new MonsterPackedState[world.AliveCount];
            MonsterMetaData[] meta = world.Meta;
            MonsterMotionData[] motion = world.Motion;
            MonsterPresentationData[] presentation = world.Presentation;
            int stateIndex = 0;
            for (int slot = 0; slot < world.SlotCount; slot++)
            {
                if (!meta[slot].IsActive) continue;
                mask[slot >> 3] |= (byte)(1 << (slot & 7));
                states[stateIndex++] = new MonsterPackedState(
                    quantizer.Pack(motion[slot].Position, motion[slot].Yaw),
                    presentation[slot].AttackSequence);
            }
            if (stateIndex != states.Length)
                throw new InvalidOperationException("Monster AliveCount 与 Active Slot 数量不一致。");
            return new MonsterPresentationFrame(serverTick, world.SlotCount, mask, states);
        }
    }

    public enum MonsterFrameApplyResult : byte
    {
        Applied,
        Stale,
        BeforeRoomStart,
        MissingSpawnBatch,
        SlotCountMismatch,
    }

    public readonly struct MonsterReplicaState
    {
        public readonly int Slot;
        public readonly ushort ConfigIndex;
        public readonly bool IsActive;
        public readonly Vector2 Position;
        public readonly float Yaw;
        public readonly byte AttackSequence;

        public MonsterReplicaState(int slot, ushort configIndex, bool isActive,
            Vector2 position, float yaw, byte attackSequence)
        {
            Slot = slot;
            ConfigIndex = configIndex;
            IsActive = isActive;
            Position = position;
            Yaw = yaw;
            AttackSequence = attackSequence;
        }
    }

    /// <summary>客户端仅保存网络状态和统一的 A/B 插值历史，不持有 GameObject。</summary>
    public sealed class MonsterReplica
    {
        private ushort[] _config = Array.Empty<ushort>();
        private bool[] _active = Array.Empty<bool>();
        private Vector2[] _previousPosition = Array.Empty<Vector2>();
        private Vector2[] _currentPosition = Array.Empty<Vector2>();
        private float[] _previousYaw = Array.Empty<float>();
        private float[] _currentYaw = Array.Empty<float>();
        private byte[] _attackSequence = Array.Empty<byte>();
        private uint _minimumAcceptedTick;
        private uint _latestTick;
        private bool _hasFrame;

        public int SlotCount { get; private set; }
        public uint LatestTick => _latestTick;
        public bool HasFrame => _hasFrame;

        public void BeginRoom(uint minimumAcceptedTick)
        {
            SlotCount = 0;
            _minimumAcceptedTick = minimumAcceptedTick;
            _latestTick = 0;
            _hasFrame = false;
            Array.Clear(_active, 0, _active.Length);
        }

        public void ApplySpawnBatch(MonsterSpawnBatch batch, MonsterPoseQuantizer quantizer)
        {
            if (batch == null) throw new ArgumentNullException(nameof(batch));
            if (quantizer == null) throw new ArgumentNullException(nameof(quantizer));
            if (batch.StartSlot != SlotCount)
                throw new InvalidOperationException($"Monster SpawnBatch 必须连续追加：Expected={SlotCount}, Actual={batch.StartSlot}。");
            EnsureCapacity(checked(batch.StartSlot + batch.Count));
            for (int i = 0; i < batch.Count; i++)
            {
                int slot = batch.StartSlot + i;
                MonsterSpawnData item = batch.Items[i];
                quantizer.Unpack(item.PackedPose, out Vector2 position, out float yaw);
                _config[slot] = item.ConfigIndex;
                _active[slot] = true;
                _previousPosition[slot] = _currentPosition[slot] = position;
                _previousYaw[slot] = _currentYaw[slot] = yaw;
                _attackSequence[slot] = 0;
            }
            SlotCount += batch.Count;
        }

        public MonsterFrameApplyResult Apply(MonsterPresentationFrame frame, MonsterPoseQuantizer quantizer)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            if (quantizer == null) throw new ArgumentNullException(nameof(quantizer));
            if (MonsterMath.IsNewer(_minimumAcceptedTick, frame.ServerTick))
                return MonsterFrameApplyResult.BeforeRoomStart;
            if (_hasFrame && !MonsterMath.IsNewer(frame.ServerTick, _latestTick))
                return MonsterFrameApplyResult.Stale;
            if (frame.SlotCount > SlotCount) return MonsterFrameApplyResult.MissingSpawnBatch;
            if (frame.SlotCount != SlotCount) return MonsterFrameApplyResult.SlotCountMismatch;

            int stateIndex = 0;
            for (int slot = 0; slot < SlotCount; slot++)
            {
                bool active = frame.IsActive(slot);
                _active[slot] = active;
                if (!active) continue;
                MonsterPackedState packed = frame.ActiveStates[stateIndex++];
                quantizer.Unpack(packed.PackedPose, out Vector2 position, out float yaw);
                _previousPosition[slot] = _currentPosition[slot];
                _previousYaw[slot] = _currentYaw[slot];
                _currentPosition[slot] = position;
                _currentYaw[slot] = yaw;
                _attackSequence[slot] = packed.AttackSequence;
            }
            _latestTick = frame.ServerTick;
            _hasFrame = true;
            return MonsterFrameApplyResult.Applied;
        }

        public bool TryGetState(int slot, float alpha, out MonsterReplicaState state)
        {
            state = default;
            if ((uint)slot >= (uint)SlotCount) return false;
            alpha = Mathf.Clamp01(alpha);
            state = new MonsterReplicaState(slot, _config[slot], _active[slot],
                Vector2.LerpUnclamped(_previousPosition[slot], _currentPosition[slot], alpha),
                Mathf.LerpAngle(_previousYaw[slot], _currentYaw[slot], alpha),
                _attackSequence[slot]);
            return true;
        }

        private void EnsureCapacity(int count)
        {
            if (count <= _config.Length) return;
            int capacity = Math.Max(64, _config.Length);
            while (capacity < count) capacity = checked(capacity * 2);
            Array.Resize(ref _config, capacity);
            Array.Resize(ref _active, capacity);
            Array.Resize(ref _previousPosition, capacity);
            Array.Resize(ref _currentPosition, capacity);
            Array.Resize(ref _previousYaw, capacity);
            Array.Resize(ref _currentYaw, capacity);
            Array.Resize(ref _attackSequence, capacity);
        }
    }
}
