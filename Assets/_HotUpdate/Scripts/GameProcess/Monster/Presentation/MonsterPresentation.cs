using ProjectGame.Core.Monsters;
using ProjectGame.Core.Spatial;
using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 房间局部X/Z各12bit、Yaw8bit的独立完整数据
    /// </summary>
    public sealed class MonsterPoseQuantizer
    {
        private readonly MonsterQuantizationData _data;

        public MonsterPoseQuantizer(Vector2 origin, Vector2 size)
        {
            if (!MonsterMath.IsFinite(origin) || !MonsterMath.IsPositiveFinite(size.x) ||
                !MonsterMath.IsPositiveFinite(size.y))
                throw new ArgumentOutOfRangeException(nameof(size));
            _data = new MonsterQuantizationData(origin, size);
        }

        public uint Pack(Vector2 position, float yaw)
        {
            if (!MonsterMath.IsFinite(position) || !MonsterMath.IsFinite(yaw))
                throw new ArgumentOutOfRangeException(nameof(position));
            return MonsterPresentationPackKernel.PackPose(position, yaw, _data);
        }
        //反过来就是解包
        public void Unpack(uint packed, out Vector2 position, out float yaw)
        {
            MonsterPresentationPackKernel.UnpackPose(packed, _data, out position, out yaw);
        }

        internal MonsterQuantizationData Data => _data;
    }

    /// <summary>
    /// 整个房间这一Tick的表现快照
    /// </summary>
    public sealed class MonsterPresentationFrame
    {
        /// <summary>
        /// 掩码01表示slot的存活信息，一个byte表示八个slot，slot映射到这里取到自己的
        /// </summary>
        private byte[] _activeMask;
        private MonsterPackedState[] _activeStates;
        public uint ServerTick { get; private set; }
        public int SlotCount { get; private set; }
        public int ActiveCount { get; private set; }
        internal int ActiveMaskCount => (SlotCount + 7) / 8;
        internal byte[] ActiveMaskBuffer => _activeMask;
        internal MonsterPackedState[] ActiveStateBuffer => _activeStates;

        internal MonsterPresentationFrame(int slotCapacity)
        {
            _activeMask = Array.Empty<byte>();
            _activeStates = Array.Empty<MonsterPackedState>();
            EnsureCapacity(slotCapacity, slotCapacity);
        }

        internal void BeginWrite(uint serverTick, int slotCount, int activeCount)
        {
            EnsureCapacity(slotCount, activeCount);
            ServerTick = serverTick;
            SlotCount = slotCount;
            ActiveCount = activeCount;
            Array.Clear(_activeMask, 0, ActiveMaskCount);
        }

        internal void EnsureCapacity(int slotCapacity, int activeCapacity)
        {
            int maskCapacity = checked((slotCapacity + 7) / 8);
            if (_activeMask.Length < maskCapacity)
                Array.Resize(ref _activeMask, GrowCapacity(_activeMask.Length, maskCapacity));
            if (_activeStates.Length < activeCapacity)
                Array.Resize(ref _activeStates, GrowCapacity(_activeStates.Length, activeCapacity));
        }

        public bool IsActive(int slot)
        {
            if ((uint)slot >= (uint)SlotCount) throw new ArgumentOutOfRangeException(nameof(slot));
            return IsSet(_activeMask, slot);
        }
        /// <summary>
        /// 反向读bit，slot >> 3是找哪一个byte，等效/8，& 7剩最后三位0-7，对应这个byte的具体一位bit，等效%8
        /// 整个式子求slot对应的mask中的那个bit位是否为0
        /// </summary>
        internal static bool IsSet(byte[] mask, int slot) => (mask[slot >> 3] & (1 << (slot & 7))) != 0;

        private static int GrowCapacity(int current, int required)
        {
            int capacity = Math.Max(16, current);
            while (capacity < required)
                capacity = checked(capacity * 2);
            return capacity;
        }
    }

    public static class MonsterPresentationPackingSystem
    {
        private static readonly ProfilerMarker CaptureMarker = new ProfilerMarker("Monster.Presentation.Capture");
        private static readonly ProfilerMarker CaptureBeginWriteMarker = new ProfilerMarker("Monster.Presentation.Capture.BeginWrite");
        private static readonly ProfilerMarker CaptureCopyLoopMarker = new ProfilerMarker("Monster.Presentation.Capture.CopyLoop");
        private static readonly ProfilerMarker GroupCaptureMarker = new ProfilerMarker("Monster.Presentation.GroupCapture");
        private static readonly ProfilerMarker GroupCaptureBeginWriteMarker = new ProfilerMarker("Monster.Presentation.GroupCapture.BeginWrite");
        private static readonly ProfilerMarker GroupCaptureCopyLoopMarker = new ProfilerMarker("Monster.Presentation.GroupCapture.CopyLoop");

        /// <summary>
        /// 服务器如何生成这一帧
        /// </summary>
        public static void CaptureInto(
            MonsterWorld world,
            uint serverTick,
            int[] activeSlots,
            int activeCount,
            MonsterPackedState[] packedStateBySlot,
            MonsterPresentationFrame destination)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));

            using var captureScope = CaptureMarker.Auto();
            //slot用1bit表示Active；每只存活怪物包含4字节姿态和1字节离散表现序列。
            using (CaptureBeginWriteMarker.Auto())
                destination.BeginWrite(serverTick, world.SlotCount, activeCount);
            byte[] mask = destination.ActiveMaskBuffer;
            MonsterPackedState[] states = destination.ActiveStateBuffer;
            using (CaptureCopyLoopMarker.Auto())
            {
                for (int i = 0; i < activeCount; i++)
                {
                    int slot = activeSlots[i];
                    //对应位取或，把Slot对应位标记为1，active且不影响其他位
                    mask[slot >> 3] |= (byte)(1 << (slot & 7));
                    states[i] = packedStateBySlot[slot];
                }
            }
        }

        /// <summary>
        /// 按指定版本的成员表顺序，采集该组在serverTick的完整表现状态。
        /// 调用方在采集前完成成员变更，并传入更新后的成员表和版本。
        /// </summary>
        internal static void CaptureGroupInto(
            uint groupId,
            uint membershipRevision,
            uint serverTick,
            ReadOnlySpan<int> memberSlots,
            MonsterPackedState[] packedStateBySlot,
            MonsterGroupSnapshot destination)
        {
            using var captureScope = GroupCaptureMarker.Auto();

            using (GroupCaptureBeginWriteMarker.Auto())
                destination.BeginWrite(groupId, membershipRevision, serverTick, memberSlots.Length);

            MonsterPackedState[] states = destination.MemberBuffer;

            using (GroupCaptureCopyLoopMarker.Auto())
            {
                for (int i = 0; i < memberSlots.Length; i++)
                    states[i] = packedStateBySlot[memberSlots[i]];
            }
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
        public readonly byte HitSequence;

        public MonsterReplicaState(int slot, ushort configIndex, bool isActive,
            Vector2 position, float yaw, byte attackSequence, byte hitSequence)
        {
            Slot = slot;
            ConfigIndex = configIndex;
            IsActive = isActive;
            Position = position;
            Yaw = yaw;
            AttackSequence = attackSequence;
            HitSequence = hitSequence;
        }
    }

    /// <summary>
    /// 客户端仅持有前一帧的历史状态和当前最新状态，处理收包，应用包和输出插值包给View层
    /// </summary>
    public sealed class MonsterReplica
    {
        private static readonly ProfilerMarker ApplyMarker =
            new ProfilerMarker("Monster.Presentation.Apply");
        private static readonly ProfilerMarker ApplyFrameLoopMarker =
            new ProfilerMarker("Monster.Presentation.Apply.FrameLoop");
        private static readonly ProfilerMarker GroupApplyMarker =
            new ProfilerMarker("Monster.Replication.GroupApply");
        private static readonly ProfilerMarker GroupApplyLoopMarker =
            new ProfilerMarker("Monster.Replication.GroupApply.Loop");

        private ushort[] _config = Array.Empty<ushort>();
        private bool[] _active = Array.Empty<bool>();
        private Vector2[] _previousPosition = Array.Empty<Vector2>();
        private Vector2[] _currentPosition = Array.Empty<Vector2>();
        private float[] _previousYaw = Array.Empty<float>();
        private float[] _currentYaw = Array.Empty<float>();
        private uint[] _latestGroupPoseTickBySlot = Array.Empty<uint>();
        private float[] _groupPoseEndTimeBySlot = Array.Empty<float>();
        private float[] _groupPoseUpdatesPerSecondBySlot = Array.Empty<float>();
        //攻击为离散数据不需要previous量做插值
        private byte[] _attackSequence = Array.Empty<byte>();
        private byte[] _hitSequence = Array.Empty<byte>();
        /// <summary>
        /// 群组映射字典，方便使用群组ID快速找到对应的成员组和版本号
        /// </summary>
        private readonly Dictionary<uint, GroupMembership> _groupMemberships = new();
        private int[] _groupMemberSlots = Array.Empty<int>();

        private struct GroupMembership
        {
            public int Offset;
            public uint Revision;
            public int Count;
        }

        /// <summary>
        /// 这个房间最早允许接受多少Tick的包，防乱序的
        /// </summary>
        private uint _minimumAcceptedTick;
        private uint _latestTick;
        private bool _hasFrame;
        private int _activeCount;


        public int SlotCount { get; private set; }
        public uint LatestTick => _latestTick;
        public int ActiveCount => _activeCount;
        public int GroupPoseReadyCount { get; private set; }
        internal uint ViewRevision { get; private set; }
        /// <summary>
        /// 宣布新的房间开始，之前的网络状态作废，数组不清直接覆盖
        /// </summary>
        public void BeginRoom(uint minimumAcceptedTick)
        {
            SlotCount = 0;
            _minimumAcceptedTick = minimumAcceptedTick;
            _latestTick = 0;
            _hasFrame = false;
            _activeCount = 0;
            GroupPoseReadyCount = 0;
            ViewRevision = 0;
            Array.Clear(_active, 0, _active.Length);
            _groupMemberships.Clear();
        }
        /// <summary>
        /// 通过MonsterSpawnBatch与量化器解包数据，然后在Slot生成怪物
        /// </summary>
        public void ApplySpawnBatch(MonsterSpawnBatch batch, MonsterPoseQuantizer quantizer)
        {
            if (batch.StartSlot != SlotCount)
                throw new InvalidOperationException($"Monster SpawnBatch 必须连续追加：Expected={SlotCount}, Actual={batch.StartSlot}。");
            EnsureCapacity(checked(batch.StartSlot + batch.Count));
            for (int i = 0; i < batch.Count; i++)
            {
                int slot = batch.StartSlot + i;
                //解包数据
                MonsterSpawnData item = batch.Items[i];
                quantizer.Unpack(item.PackedPose, out Vector2 position, out float yaw);
                //接着slot沿序往下保存数据
                _config[slot] = item.ConfigIndex;
                _active[slot] = true;
                //初始previous = current
                _previousPosition[slot] = _currentPosition[slot] = position;
                _previousYaw[slot] = _currentYaw[slot] = yaw;
                // 首份群组姿态允许与房间起始 Tick 相同。
                _latestGroupPoseTickBySlot[slot] = unchecked(_minimumAcceptedTick - 1u);
                _groupPoseEndTimeBySlot[slot] = 0f;
                _groupPoseUpdatesPerSecondBySlot[slot] = 0f;
                _attackSequence[slot] = 0;
                _hitSequence[slot] = 0;
            }
            SlotCount += batch.Count;
            _activeCount += batch.Count;
            ViewRevision++;
        }

        internal void Despawn(int slot)
        {
            if ((uint)slot >= (uint)SlotCount || !_active[slot])
                return;

            _active[slot] = false;
            _activeCount--;
            if (_groupPoseUpdatesPerSecondBySlot[slot] != 0f)
                GroupPoseReadyCount--;
            ViewRevision++;
        }
        /// <summary>
        /// 收到一帧网络状态
        /// </summary>
        public MonsterFrameApplyResult Apply(MonsterPresentationFrame frame, MonsterPoseQuantizer quantizer)
        {
            using var applyScope = ApplyMarker.Auto();
            //一定比最低Tick新，不然视作此房间开始前的旧包
            if (MonsterMath.IsNewer(_minimumAcceptedTick, frame.ServerTick))
                return MonsterFrameApplyResult.BeforeRoomStart;
            //有帧了，此包还没有最新Tick新，视作旧包/乱序包
            if (_hasFrame && !MonsterMath.IsNewer(frame.ServerTick, _latestTick))
                return MonsterFrameApplyResult.Stale;
            //包SlotCount比最新的怪物数量大，没有沿序，视作少收了SpawnBatch包
            if (frame.SlotCount > SlotCount)
                return MonsterFrameApplyResult.MissingSpawnBatch;
            //包SlotCount比当前还少，视作客户端与实际情况出现不同步
            if (frame.SlotCount != SlotCount)
                return MonsterFrameApplyResult.SlotCountMismatch;

            int stateIndex = 0;
            int activeCount = 0;
            using (ApplyFrameLoopMarker.Auto())
            {
                for (int slot = 0; slot < SlotCount; slot++)
                {
                    //遍历Slot，死的不管
                    bool active = frame.IsActive(slot);
                    _active[slot] = active;
                    if (!active)
                        continue;
                    activeCount++;
                    _previousPosition[slot] = _currentPosition[slot];
                    _previousYaw[slot] = _currentYaw[slot];
                    ApplyPose(slot, frame.ActiveStateBuffer[stateIndex++], quantizer);
                }
            }
            _latestTick = frame.ServerTick;
            _hasFrame = true;
            _activeCount = activeCount;
            ViewRevision++;
            return MonsterFrameApplyResult.Applied;
        }

        internal bool ApplyGroupSnapshot(
            MonsterGroupSnapshot frame, MonsterPoseQuantizer quantizer, float arrivalTime, float duration)
        {
            using var applyScope = GroupApplyMarker.Auto();
            uint tick = frame.ServerTick;
            if (MonsterMath.IsNewer(_minimumAcceptedTick, tick) ||
                !_groupMemberships.TryGetValue(frame.GroupId, out GroupMembership membership) ||
                membership.Revision != frame.MembershipRevision ||
                membership.Count != frame.MemberCount)
                return false;

            float endTime = arrivalTime + duration;
            float updatesPerSecond = 1f / duration;
            MonsterPackedState[] states = frame.MemberBuffer;
            using (GroupApplyLoopMarker.Auto())
            {
                for (int i = 0; i < membership.Count; i++)
                {
                    int slot = _groupMemberSlots[membership.Offset + i];
                    // Spawn 可能还没到。按 Slot 比 Tick，防止跨组后的旧姿态回退。
                    if ((uint)slot >= (uint)SlotCount || !_active[slot] ||
                        !MonsterMath.IsNewer(tick, _latestGroupPoseTickBySlot[slot]))
                        continue;

                    // 上一段还在播放时，从收包时刻的画面姿态接上新目标。
                    float alpha = GetGroupPoseAlpha(slot, arrivalTime);
                    _previousPosition[slot] = Vector2.LerpUnclamped(_previousPosition[slot], _currentPosition[slot], alpha);
                    _previousYaw[slot] = Mathf.LerpAngle(_previousYaw[slot], _currentYaw[slot], alpha);
                    ApplyPose(slot, states[i], quantizer);

                    if (_groupPoseUpdatesPerSecondBySlot[slot] == 0f)
                        GroupPoseReadyCount++;
                    _latestGroupPoseTickBySlot[slot] = tick;
                    _groupPoseEndTimeBySlot[slot] = endTime;
                    _groupPoseUpdatesPerSecondBySlot[slot] = updatesPerSecond;
                }
            }

            ViewRevision++;
            return true;
        }

        private void ApplyPose(int slot, MonsterPackedState packed, MonsterPoseQuantizer quantizer)
        {
            quantizer.Unpack(packed.PackedPose, out Vector2 position, out float yaw);
            _currentPosition[slot] = position;
            _currentYaw[slot] = yaw;
            _attackSequence[slot] = packed.AttackSequence;
            _hitSequence[slot] = packed.HitSequence;
        }

        private float GetGroupPoseAlpha(int slot, float now) =>
            Mathf.Clamp01(1f + (now - _groupPoseEndTimeBySlot[slot]) * _groupPoseUpdatesPerSecondBySlot[slot]);

        public MonsterReplicaState GetState(int slot, float now)
        {
            float alpha = GetGroupPoseAlpha(slot, now);
            return new MonsterReplicaState(slot, _config[slot], _active[slot],
                Vector2.LerpUnclamped(_previousPosition[slot], _currentPosition[slot], alpha),
                Mathf.LerpAngle(_previousYaw[slot], _currentYaw[slot], alpha),
                _attackSequence[slot], _hitSequence[slot]);
        }

        internal void GetViewState(
            int slot,
            float now,
            out bool active,
            out Vector2 pose,
            out float yaw,
            out Vector2 targetPose,
            out float targetYaw,
            out float poseEndTime,
            out float updatesPerSecond,
            out byte attackSequence,
            out byte hitSequence)
        {
            active = _active[slot];
            attackSequence = _attackSequence[slot];
            hitSequence = _hitSequence[slot];
            poseEndTime = _groupPoseEndTimeBySlot[slot];
            updatesPerSecond = _groupPoseUpdatesPerSecondBySlot[slot];
            targetPose = _currentPosition[slot];
            targetYaw = _currentYaw[slot];

            float alpha = GetGroupPoseAlpha(slot, now);
            pose = Vector2.LerpUnclamped(_previousPosition[slot], targetPose, alpha);
            yaw = Mathf.LerpAngle(_previousYaw[slot], targetYaw, alpha);
        }

        internal Span<int> BeginGroupMembershipWrite(uint groupId, int count, out int offset)
        {
            const int blockSize = MonsterNetworkGroupKernel.GroupCapacity;
            if (!_groupMemberships.TryGetValue(groupId, out GroupMembership membership))
            {
                membership.Offset = _groupMemberships.Count * blockSize;
                int required = membership.Offset + blockSize;
                if (required > _groupMemberSlots.Length)
                    Array.Resize(ref _groupMemberSlots, Math.Max(required, _groupMemberSlots.Length * 2));
            }

            //直接覆盖数组。读取中断时，这段数据不再代表上一版完整成员表。
            membership.Count = 0;
            _groupMemberships[groupId] = membership;
            offset = membership.Offset;
            return _groupMemberSlots.AsSpan(offset, count);
        }

        internal void CommitGroupMembership(uint groupId, int offset, int count, uint revision)
        {
            _groupMemberships[groupId] = new GroupMembership
            {
                Offset = offset,
                Count = count,
                Revision = revision,
            };
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
            Array.Resize(ref _latestGroupPoseTickBySlot, capacity);
            Array.Resize(ref _groupPoseEndTimeBySlot, capacity);
            Array.Resize(ref _groupPoseUpdatesPerSecondBySlot, capacity);
            Array.Resize(ref _attackSequence, capacity);
            Array.Resize(ref _hitSequence, capacity);
        }
    }
}
