using System;
using ProjectGame.Core.Monsters;
using UnityEngine;

namespace ProjectGame.Core.Spatial
{
    /// <summary>
    /// 只保存群组区域、成员和发送调度状态的 AOT SoA 内核。
    /// 不依赖 Transform、NetworkManager 或 HotFix 对象。
    /// </summary>
    public sealed class MonsterNetworkGroupKernel
    {
        public const int GroupCapacity = 128;
        public const byte InnerInterval = 2;
        public const byte MiddleInterval = 3;
        public const byte OuterInterval = 6;

        private const int BaseDepth = MonsterQuadTreeKernel.BaseDepth;
        private const int RegionCapacity = 1 << (BaseDepth * 2);
        private const uint TierRefreshInterval = 6;
        private const int Unlinked = -1;

        private readonly int[] _regionIndexByCode = new int[RegionCapacity];
        private readonly uint[] _regionCodes = new uint[RegionCapacity];
        private readonly Vector2[] _regionCenters = new Vector2[RegionCapacity];
        private readonly float[] _regionHalfSizes = new float[RegionCapacity];
        private readonly byte[] _regionIntervals = new byte[RegionCapacity];
        private readonly int[] _regionFirstGroup = new int[RegionCapacity];
        private readonly int[] _regionLastGroup = new int[RegionCapacity];
        private readonly int[] _activeRegionIndices = new int[RegionCapacity];

        private int[] _groupRegionIndices = Array.Empty<int>();
        private int[] _groupNextInRegion = Array.Empty<int>();
        private int[] _groupCounts = Array.Empty<int>();
        private uint[] _groupMembershipRevisions = Array.Empty<uint>();
        private uint[] _groupPublishedMembershipRevisions = Array.Empty<uint>();
        private uint[] _groupNextPoseTicks = Array.Empty<uint>();
        private byte[] _groupIntervals = Array.Empty<byte>();
        private int[] _groupMembers = Array.Empty<int>();

        private int[] _groupBySlot = Array.Empty<int>();
        private int[] _slotIndexInGroup = Array.Empty<int>();
        private int _regionCount;
        private int _groupCount;

        public int GroupCount => _groupCount;

        public MonsterNetworkGroupKernel(int initialSlotCapacity = 64)
        {
            Array.Fill(_regionIndexByCode, Unlinked);
            Array.Fill(_regionFirstGroup, Unlinked);
            Array.Fill(_regionLastGroup, Unlinked);
            EnsureCapacity(initialSlotCapacity);
        }

        public void Clear()
        {
            Array.Fill(_regionIndexByCode, Unlinked);
            Array.Fill(_regionFirstGroup, Unlinked);
            Array.Fill(_regionLastGroup, Unlinked);
            Array.Fill(_groupBySlot, Unlinked);
            Array.Fill(_slotIndexInGroup, Unlinked);
            _regionCount = 0;
            _groupCount = 0;
        }

        public void EnsureCapacity(int slotCapacity)
        {
            if (_groupBySlot.Length < slotCapacity)
            {
                int previous = _groupBySlot.Length;
                int capacity = Math.Max(slotCapacity, previous == 0 ? 64 : previous * 2);
                Array.Resize(ref _groupBySlot, capacity);
                Array.Resize(ref _slotIndexInGroup, capacity);
                Array.Fill(_groupBySlot, Unlinked, previous, capacity - previous);
                Array.Fill(_slotIndexInGroup, Unlinked, previous, capacity - previous);
            }

            int requiredGroups = Math.Max(4, (slotCapacity + GroupCapacity - 1) / GroupCapacity);
            EnsureGroupCapacity(requiredGroups);
        }

        public void Refresh(
            MonsterMetaData[] meta,
            MonsterMotionData[] motion,
            int slotCount,
            Vector2[] playerPositions,
            int playerCount,
            Vector2 rootCenter,
            float rootHalfSize,
            uint tick)
        {
            EnsureCapacity(slotCount);

            for (int slot = 0; slot < slotCount; slot++)
            {
                int previousGroup = _groupBySlot[slot];
                if (!meta[slot].IsActive)
                {
                    if (previousGroup >= 0)
                        RemoveFromGroup(slot, previousGroup);
                    _groupBySlot[slot] = Unlinked;
                    _slotIndexInGroup[slot] = Unlinked;
                    continue;
                }

                uint regionCode = MonsterQuadTreeKernel.ComputeBaseRegionCode(
                    motion[slot].Position, rootCenter, rootHalfSize);
                if (previousGroup >= 0 && GetRegionCode(previousGroup) == regionCode)
                    continue;

                if (previousGroup >= 0)
                    RemoveFromGroup(slot, previousGroup);

                int nextGroup = GetGroupWithSpace(regionCode, rootCenter, rootHalfSize, playerPositions, playerCount, tick);
                AddToGroup(slot, nextGroup);
            }

            if (tick % TierRefreshInterval == 0)
                RefreshTiers(playerPositions, playerCount, tick);
        }

        public uint GetGroupId(int groupIndex) => unchecked((uint)(groupIndex + 1));
        public uint GetRegionCode(int groupIndex) => GetRegionCodeForGroup(groupIndex);
        public uint GetMembershipRevision(int groupIndex) => _groupMembershipRevisions[groupIndex];
        public uint GetPublishedMembershipRevision(int groupIndex) => _groupPublishedMembershipRevisions[groupIndex];
        public byte GetIntervalTicks(int groupIndex) => _groupIntervals[groupIndex];
        public uint GetNextPoseTick(int groupIndex) => _groupNextPoseTicks[groupIndex];
        public int GetMemberCount(int groupIndex) => _groupCounts[groupIndex];
        public ReadOnlySpan<int> GetMemberSlots(int groupIndex) =>
            _groupMembers.AsSpan(groupIndex * GroupCapacity, _groupCounts[groupIndex]);

        public bool IsMembershipChanged(int groupIndex) =>
            _groupPublishedMembershipRevisions[groupIndex] != _groupMembershipRevisions[groupIndex];

        public bool IsDue(int groupIndex, uint tick) =>
            MonsterNetworkTickMath.HasReached(tick, _groupNextPoseTicks[groupIndex]);

        public int CompareDueGroups(int left, int right)
        {
            int dueOrder = unchecked((int)(_groupNextPoseTicks[left] - _groupNextPoseTicks[right]));
            if (dueOrder != 0)
                return dueOrder;

            int intervalOrder = _groupIntervals[left].CompareTo(_groupIntervals[right]);
            return intervalOrder != 0 ? intervalOrder : left.CompareTo(right);
        }

        public void MarkPublished(int groupIndex, uint tick)
        {
            _groupPublishedMembershipRevisions[groupIndex] = _groupMembershipRevisions[groupIndex];
            _groupNextPoseTicks[groupIndex] = unchecked(tick + _groupIntervals[groupIndex]);
        }

        public void MarkMembershipPublished(int groupIndex) =>
            _groupPublishedMembershipRevisions[groupIndex] = _groupMembershipRevisions[groupIndex];

        private int GetGroupWithSpace(
            uint regionCode,
            Vector2 rootCenter,
            float rootHalfSize,
            Vector2[] playerPositions,
            int playerCount,
            uint tick)
        {
            int regionIndex = GetOrCreateRegion(
                regionCode, rootCenter, rootHalfSize, playerPositions, playerCount, tick);

            int groupIndex = _regionFirstGroup[regionIndex];
            while (groupIndex >= 0)
            {
                if (_groupCounts[groupIndex] < GroupCapacity)
                    return groupIndex;
                groupIndex = _groupNextInRegion[groupIndex];
            }

            EnsureGroupCapacity(_groupCount + 1);
            groupIndex = _groupCount++;
            byte interval = _regionIntervals[regionIndex];
            _groupRegionIndices[groupIndex] = regionIndex;
            _groupNextInRegion[groupIndex] = Unlinked;
            _groupCounts[groupIndex] = 0;
            _groupMembershipRevisions[groupIndex] = 0;
            _groupPublishedMembershipRevisions[groupIndex] = 0;
            _groupIntervals[groupIndex] = interval;
            _groupNextPoseTicks[groupIndex] = unchecked(tick + (uint)groupIndex % interval);

            int lastGroup = _regionLastGroup[regionIndex];
            if (lastGroup < 0)
                _regionFirstGroup[regionIndex] = groupIndex;
            else
                _groupNextInRegion[lastGroup] = groupIndex;
            _regionLastGroup[regionIndex] = groupIndex;
            return groupIndex;
        }

        private int GetOrCreateRegion(
            uint regionCode,
            Vector2 rootCenter,
            float rootHalfSize,
            Vector2[] playerPositions,
            int playerCount,
            uint tick)
        {
            int regionIndex = _regionIndexByCode[regionCode];
            if (regionIndex >= 0)
                return regionIndex;

            regionIndex = _regionCount++;
            _regionIndexByCode[regionCode] = regionIndex;
            _regionCodes[regionIndex] = regionCode;
            _activeRegionIndices[regionIndex] = regionIndex;
            _regionCenters[regionIndex] = MonsterQuadTreeKernel.ComputeBaseRegionCenter(
                regionCode, rootCenter, rootHalfSize);
            _regionHalfSizes[regionIndex] = rootHalfSize / (1 << BaseDepth);
            _regionFirstGroup[regionIndex] = Unlinked;
            _regionLastGroup[regionIndex] = Unlinked;
            _regionIntervals[regionIndex] = OuterInterval;
            RefreshRegionTier(regionIndex, playerPositions, playerCount, tick);
            return regionIndex;
        }

        private void RefreshTiers(Vector2[] playerPositions, int playerCount, uint tick)
        {
            for (int i = 0; i < _regionCount; i++)
                RefreshRegionTier(_activeRegionIndices[i], playerPositions, playerCount, tick);
        }

        private void RefreshRegionTier(int regionIndex, Vector2[] playerPositions, int playerCount, uint tick)
        {
            float nearestDistanceSquared = float.PositiveInfinity;
            Vector2 center = _regionCenters[regionIndex];
            float halfSize = _regionHalfSizes[regionIndex];
            for (int i = 0; i < playerCount; i++)
            {
                Vector2 position = playerPositions[i];
                float dx = Mathf.Max(0f, Mathf.Abs(position.x - center.x) - halfSize);
                float dz = Mathf.Max(0f, Mathf.Abs(position.y - center.y) - halfSize);
                nearestDistanceSquared = Mathf.Min(nearestDistanceSquared, dx * dx + dz * dz);
            }

            byte current = _regionIntervals[regionIndex];
            float innerDistance = current == InnerInterval ? 24f : 20f;
            float middleDistance = current <= MiddleInterval ? 46f : 40f;
            byte interval = nearestDistanceSquared <= innerDistance * innerDistance ? InnerInterval
                : nearestDistanceSquared <= middleDistance * middleDistance ? MiddleInterval
                : OuterInterval;
            if (interval == current)
                return;

            _regionIntervals[regionIndex] = interval;
            int groupIndex = _regionFirstGroup[regionIndex];
            while (groupIndex >= 0)
            {
                SetGroupInterval(groupIndex, interval, tick);
                groupIndex = _groupNextInRegion[groupIndex];
            }
        }

        private void SetGroupInterval(int groupIndex, byte interval, uint tick)
        {
            _groupIntervals[groupIndex] = interval;
            uint phaseTick = unchecked(tick + (uint)groupIndex % interval);
            if (MonsterNetworkTickMath.IsNewer(_groupNextPoseTicks[groupIndex], phaseTick))
                _groupNextPoseTicks[groupIndex] = phaseTick;
        }

        private void AddToGroup(int slot, int groupIndex)
        {
            int count = _groupCounts[groupIndex];
            _groupMembers[groupIndex * GroupCapacity + count] = slot;
            _groupCounts[groupIndex] = count + 1;
            _groupBySlot[slot] = groupIndex;
            _slotIndexInGroup[slot] = count;
            _groupMembershipRevisions[groupIndex]++;
        }

        private void RemoveFromGroup(int slot, int groupIndex)
        {
            int count = _groupCounts[groupIndex];
            int index = _slotIndexInGroup[slot];
            int lastIndex = count - 1;
            int lastSlot = _groupMembers[groupIndex * GroupCapacity + lastIndex];
            if (index != lastIndex)
            {
                _groupMembers[groupIndex * GroupCapacity + index] = lastSlot;
                _slotIndexInGroup[lastSlot] = index;
            }

            _groupCounts[groupIndex] = lastIndex;
            _groupMembershipRevisions[groupIndex]++;
            _groupBySlot[slot] = Unlinked;
            _slotIndexInGroup[slot] = Unlinked;
        }

        private uint GetRegionCodeForGroup(int groupIndex) =>
            _regionCodes[_groupRegionIndices[groupIndex]];

        private void EnsureGroupCapacity(int required)
        {
            if (_groupRegionIndices.Length >= required)
                return;

            int previous = _groupRegionIndices.Length;
            int capacity = Math.Max(required, previous == 0 ? 4 : previous * 2);
            Array.Resize(ref _groupRegionIndices, capacity);
            Array.Resize(ref _groupNextInRegion, capacity);
            Array.Resize(ref _groupCounts, capacity);
            Array.Resize(ref _groupMembershipRevisions, capacity);
            Array.Resize(ref _groupPublishedMembershipRevisions, capacity);
            Array.Resize(ref _groupNextPoseTicks, capacity);
            Array.Resize(ref _groupIntervals, capacity);
            Array.Resize(ref _groupMembers, capacity * GroupCapacity);
        }
    }

    internal static class MonsterNetworkTickMath
    {
        internal static bool HasReached(uint tick, uint deadline) => unchecked((int)(tick - deadline)) >= 0;
        internal static bool IsNewer(uint tick, uint previous) => unchecked((int)(tick - previous)) > 0;
    }
}
