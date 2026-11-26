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
            _data = new MonsterQuantizationData(origin, size);
        }

        public uint Pack(Vector2 position, float yaw) =>
            MonsterPresentationPackKernel.PackPose(position, yaw, _data);
        //反过来就是解包
        public void Unpack(uint packed, out Vector2 position, out float yaw)
        {
            MonsterPresentationPackKernel.UnpackPose(packed, _data, out position, out yaw);
        }

        internal MonsterQuantizationData Data => _data;
    }

    /// <summary>
    /// 整个房间这一Tick的表现快照。状态按当前密集 StorageIndex 顺序保存，身份单独并行保存。
    /// </summary>
    public sealed class MonsterPresentationFrame
    {
        private ulong[] _activeEntityIds;
        private MonsterPackedState[] _activeStates;
        public uint ServerTick { get; private set; }
        public int ActiveCount { get; private set; }
        internal ulong[] ActiveEntityIdBuffer => _activeEntityIds;
        internal MonsterPackedState[] ActiveStateBuffer => _activeStates;

        internal MonsterPresentationFrame(int initialCapacity)
        {
            _activeEntityIds = Array.Empty<ulong>();
            _activeStates = Array.Empty<MonsterPackedState>();
            EnsureCapacity(initialCapacity);
        }

        internal void BeginWrite(uint serverTick, int activeCount)
        {
            EnsureCapacity(activeCount);
            ServerTick = serverTick;
            ActiveCount = activeCount;
        }

        internal void EnsureCapacity(int activeCapacity)
        {
            if (_activeEntityIds.Length < activeCapacity)
                Array.Resize(ref _activeEntityIds, GrowCapacity(_activeEntityIds.Length, activeCapacity));
            if (_activeStates.Length < activeCapacity)
                Array.Resize(ref _activeStates, GrowCapacity(_activeStates.Length, activeCapacity));
        }

        private static int GrowCapacity(int current, int required)
        {
            int capacity = Math.Max(16, current);
            while (capacity < required)
                capacity *= 2;
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
            int[] activeStorageIndices,
            int activeCount,
            MonsterPackedState[] packedStateByStorageIndex,
            MonsterPresentationFrame destination)
        {
            using var captureScope = CaptureMarker.Auto();
            //每只存活怪物包含身份、4字节姿态和1字节离散表现序列。
            using (CaptureBeginWriteMarker.Auto())
                destination.BeginWrite(serverTick, activeCount);
            ulong[] entityIds = destination.ActiveEntityIdBuffer;
            MonsterPackedState[] states = destination.ActiveStateBuffer;
            using (CaptureCopyLoopMarker.Auto())
            {
                for (int i = 0; i < activeCount; i++)
                {
                    int storageIndex = activeStorageIndices[i];
                    entityIds[i] = world.GetEntityId(storageIndex);
                    states[i] = packedStateByStorageIndex[storageIndex];
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
            ReadOnlySpan<int> memberStorageIndices,
            MonsterPackedState[] packedStateByStorageIndex,
            MonsterGroupSnapshot destination)
        {
            using var captureScope = GroupCaptureMarker.Auto();

            using (GroupCaptureBeginWriteMarker.Auto())
                destination.BeginWrite(groupId, membershipRevision, serverTick, memberStorageIndices.Length);

            MonsterPackedState[] states = destination.MemberBuffer;

            using (GroupCaptureCopyLoopMarker.Auto())
            {
                for (int i = 0; i < memberStorageIndices.Length; i++)
                    states[i] = packedStateByStorageIndex[memberStorageIndices[i]];
            }
        }

    }

    public enum MonsterFrameApplyResult : byte
    {
        Applied,
        Stale,
        BeforeRoomStart,
        MissingSpawnBatch,
    }

    public readonly struct MonsterReplicaState
    {
        public readonly int ViewSlot;
        public readonly ushort ConfigIndex;
        public readonly bool IsActive;
        public readonly Vector2 Position;
        public readonly float Yaw;
        public readonly byte AttackSequence;
        public readonly byte HitSequence;

        public MonsterReplicaState(int viewSlot, ushort configIndex, bool isActive,
            Vector2 position, float yaw, byte attackSequence, byte hitSequence)
        {
            ViewSlot = viewSlot;
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
        private readonly Dictionary<ulong, int> _viewSlotByEntityId = new();
        private bool[] _active = Array.Empty<bool>();
        private Vector2[] _previousPosition = Array.Empty<Vector2>();
        private Vector2[] _currentPosition = Array.Empty<Vector2>();
        private float[] _previousYaw = Array.Empty<float>();
        private float[] _currentYaw = Array.Empty<float>();
        private uint[] _latestGroupPoseTickByViewSlot = Array.Empty<uint>();
        private float[] _groupPoseEndTimeByViewSlot = Array.Empty<float>();
        private float[] _groupPoseUpdatesPerSecondByViewSlot = Array.Empty<float>();
        //攻击为离散数据不需要previous量做插值
        private byte[] _attackSequence = Array.Empty<byte>();
        private byte[] _hitSequence = Array.Empty<byte>();
        /// <summary>
        /// 群组映射字典，方便使用群组ID快速找到对应的成员组和版本号
        /// </summary>
        private readonly Dictionary<uint, GroupMembership> _groupMemberships = new();
        private ulong[] _groupMemberEntityIds = Array.Empty<ulong>();
        private int[] _frameStorageIndices = Array.Empty<int>();

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


        public int ViewSlotCount { get; private set; }
        public uint LatestTick => _latestTick;
        public int ActiveCount => _activeCount;
        public int GroupPoseReadyCount { get; private set; }
        internal uint ViewRevision { get; private set; }
        /// <summary>
        /// 宣布新的房间开始，之前的网络状态作废，数组不清直接覆盖
        /// </summary>
        public void BeginRoom(uint minimumAcceptedTick)
        {
            ViewSlotCount = 0;
            _minimumAcceptedTick = minimumAcceptedTick;
            _latestTick = 0;
            _hasFrame = false;
            _activeCount = 0;
            GroupPoseReadyCount = 0;
            ViewRevision = 0;
            Array.Clear(_active, 0, _active.Length);
            _groupMemberships.Clear();
            _viewSlotByEntityId.Clear();
        }
        /// <summary>
        /// 通过MonsterSpawnBatch与量化器解包数据，然后在本地表现槽位生成怪物。
        /// 网络身份从批次显式读取，不再由表现槽位推导。
        /// </summary>
        public void ApplySpawnBatch(MonsterSpawnBatch batch, MonsterPoseQuantizer quantizer)
        {
            int startViewSlot = ViewSlotCount;
            EnsureCapacity(startViewSlot + batch.Count);
            for (int i = 0; i < batch.Count; i++)
            {
                int viewSlot = startViewSlot + i;
                //解包数据
                MonsterSpawnData item = batch.Items[i];
                quantizer.Unpack(item.PackedPose, out Vector2 position, out float yaw);
                //本地 ViewSlot 沿序追加，网络身份单独建立映射。
                _viewSlotByEntityId.Add(item.EntityId, viewSlot);
                _config[viewSlot] = item.ConfigIndex;
                _active[viewSlot] = true;
                //初始previous = current
                _previousPosition[viewSlot] = _currentPosition[viewSlot] = position;
                _previousYaw[viewSlot] = _currentYaw[viewSlot] = yaw;
                // 首份群组姿态允许与房间起始 Tick 相同。
                _latestGroupPoseTickByViewSlot[viewSlot] = unchecked(_minimumAcceptedTick - 1u);
                _groupPoseEndTimeByViewSlot[viewSlot] = 0f;
                _groupPoseUpdatesPerSecondByViewSlot[viewSlot] = 0f;
                _attackSequence[viewSlot] = 0;
                _hitSequence[viewSlot] = 0;
            }
            ViewSlotCount += batch.Count;
            _activeCount += batch.Count;
            ViewRevision++;
        }

        internal void Despawn(ulong entityId)
        {
            if (!_viewSlotByEntityId.TryGetValue(entityId, out int viewSlot) || !_active[viewSlot])
                return;

            _active[viewSlot] = false;
            _viewSlotByEntityId.Remove(entityId);
            _activeCount--;
            if (_groupPoseUpdatesPerSecondByViewSlot[viewSlot] != 0f)
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
            if (frame.ActiveCount > _frameStorageIndices.Length)
                Array.Resize(ref _frameStorageIndices, frame.ActiveCount);

            ulong[] entityIds = frame.ActiveEntityIdBuffer;
            for (int i = 0; i < frame.ActiveCount; i++)
            {
                if (!_viewSlotByEntityId.TryGetValue(entityIds[i], out _frameStorageIndices[i]))
                    return MonsterFrameApplyResult.MissingSpawnBatch;
            }

            for (int viewSlot = 0; viewSlot < ViewSlotCount; viewSlot++)
            {
                if (_active[viewSlot] && _groupPoseUpdatesPerSecondByViewSlot[viewSlot] != 0f)
                    GroupPoseReadyCount--;
                _active[viewSlot] = false;
            }

            using (ApplyFrameLoopMarker.Auto())
            {
                for (int stateIndex = 0; stateIndex < frame.ActiveCount; stateIndex++)
                {
                    int viewSlot = _frameStorageIndices[stateIndex];
                    _active[viewSlot] = true;
                    _previousPosition[viewSlot] = _currentPosition[viewSlot];
                    _previousYaw[viewSlot] = _currentYaw[viewSlot];
                    ApplyPose(viewSlot, frame.ActiveStateBuffer[stateIndex], quantizer);
                }
            }
            _latestTick = frame.ServerTick;
            _hasFrame = true;
            _activeCount = frame.ActiveCount;
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
                    ulong entityId = _groupMemberEntityIds[membership.Offset + i];
                    // Spawn 可能还没到。按身份解析失败时跳过，防止跨组后的旧姿态回退。
                    if (!_viewSlotByEntityId.TryGetValue(entityId, out int viewSlot) || !_active[viewSlot] ||
                        !MonsterMath.IsNewer(tick, _latestGroupPoseTickByViewSlot[viewSlot]))
                        continue;

                    // 上一段还在播放时，从收包时刻的画面姿态接上新目标。
                    float alpha = GetGroupPoseAlpha(viewSlot, arrivalTime);
                    _previousPosition[viewSlot] = Vector2.LerpUnclamped(_previousPosition[viewSlot], _currentPosition[viewSlot], alpha);
                    _previousYaw[viewSlot] = Mathf.LerpAngle(_previousYaw[viewSlot], _currentYaw[viewSlot], alpha);
                    ApplyPose(viewSlot, states[i], quantizer);

                    if (_groupPoseUpdatesPerSecondByViewSlot[viewSlot] == 0f)
                        GroupPoseReadyCount++;
                    _latestGroupPoseTickByViewSlot[viewSlot] = tick;
                    _groupPoseEndTimeByViewSlot[viewSlot] = endTime;
                    _groupPoseUpdatesPerSecondByViewSlot[viewSlot] = updatesPerSecond;
                }
            }

            ViewRevision++;
            return true;
        }

        private void ApplyPose(int viewSlot, MonsterPackedState packed, MonsterPoseQuantizer quantizer)
        {
            quantizer.Unpack(packed.PackedPose, out Vector2 position, out float yaw);
            _currentPosition[viewSlot] = position;
            _currentYaw[viewSlot] = yaw;
            _attackSequence[viewSlot] = packed.AttackSequence;
            _hitSequence[viewSlot] = packed.HitSequence;
        }

        private float GetGroupPoseAlpha(int viewSlot, float now) =>
            Mathf.Clamp01(1f + (now - _groupPoseEndTimeByViewSlot[viewSlot]) * _groupPoseUpdatesPerSecondByViewSlot[viewSlot]);

        public MonsterReplicaState GetState(int viewSlot, float now)
        {
            float alpha = GetGroupPoseAlpha(viewSlot, now);
            return new MonsterReplicaState(viewSlot, _config[viewSlot], _active[viewSlot],
                Vector2.LerpUnclamped(_previousPosition[viewSlot], _currentPosition[viewSlot], alpha),
                Mathf.LerpAngle(_previousYaw[viewSlot], _currentYaw[viewSlot], alpha),
                _attackSequence[viewSlot], _hitSequence[viewSlot]);
        }

        internal void GetViewState(
            int viewSlot,
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
            active = _active[viewSlot];
            attackSequence = _attackSequence[viewSlot];
            hitSequence = _hitSequence[viewSlot];
            poseEndTime = _groupPoseEndTimeByViewSlot[viewSlot];
            updatesPerSecond = _groupPoseUpdatesPerSecondByViewSlot[viewSlot];
            targetPose = _currentPosition[viewSlot];
            targetYaw = _currentYaw[viewSlot];

            float alpha = GetGroupPoseAlpha(viewSlot, now);
            pose = Vector2.LerpUnclamped(_previousPosition[viewSlot], targetPose, alpha);
            yaw = Mathf.LerpAngle(_previousYaw[viewSlot], targetYaw, alpha);
        }

        internal Span<ulong> BeginGroupMembershipWrite(uint groupId, int count, out int offset)
        {
            const int blockSize = MonsterNetworkGroupKernel.GroupCapacity;
            if (!_groupMemberships.TryGetValue(groupId, out GroupMembership membership))
            {
                membership.Offset = _groupMemberships.Count * blockSize;
                int required = membership.Offset + blockSize;
                if (required > _groupMemberEntityIds.Length)
                    Array.Resize(ref _groupMemberEntityIds, Math.Max(required, _groupMemberEntityIds.Length * 2));
            }

            //直接覆盖数组。读取中断时，这段数据不再代表上一版完整成员表。
            membership.Count = 0;
            _groupMemberships[groupId] = membership;
            offset = membership.Offset;
            return _groupMemberEntityIds.AsSpan(offset, count);
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
            while (capacity < count) capacity *= 2;
            Array.Resize(ref _config, capacity);
            if (_frameStorageIndices.Length < capacity)
                Array.Resize(ref _frameStorageIndices, capacity);
            Array.Resize(ref _active, capacity);
            Array.Resize(ref _previousPosition, capacity);
            Array.Resize(ref _currentPosition, capacity);
            Array.Resize(ref _previousYaw, capacity);
            Array.Resize(ref _currentYaw, capacity);
            Array.Resize(ref _latestGroupPoseTickByViewSlot, capacity);
            Array.Resize(ref _groupPoseEndTimeByViewSlot, capacity);
            Array.Resize(ref _groupPoseUpdatesPerSecondByViewSlot, capacity);
            Array.Resize(ref _attackSequence, capacity);
            Array.Resize(ref _hitSequence, capacity);
        }
    }
}
