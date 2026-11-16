using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    internal sealed class MonsterNetworkGroupIndex
    {
        internal const int GroupCapacity = 128;
        internal const byte InnerInterval = 2;
        internal const byte MiddleInterval = 3;
        internal const byte OuterInterval = 6;
        private const uint TierRefreshInterval = 6;
        private static readonly ProfilerMarker RefreshMarker = new("Monster.Network.GroupBuild");
        private static readonly ProfilerMarker RefreshTiersMarker = new("Monster.Network.InterestRefresh");

        private readonly Dictionary<uint, Region> _regions = new();
        private readonly List<Group> _groups = new();
        private Group[] _groupBySlot = Array.Empty<Group>();

        internal IReadOnlyList<Group> Groups => _groups;

        internal void Refresh(MonsterWorld world, MonsterSpatialIndex spatialIndex,
            ReadOnlySpan<Vector2> playerPositions, uint tick)
        {
            using var refreshScope = RefreshMarker.Auto();
            if (_groupBySlot.Length < world.Capacity)
                Array.Resize(ref _groupBySlot, world.Capacity);

            var meta = world.Meta;
            var motion = world.Motion;
            for (int slot = 0; slot < world.SlotCount; slot++)
            {
                Group previous = _groupBySlot[slot];
                if (!meta[slot].IsActive)
                {
                    if (previous != null)
                    {
                        previous.Remove(slot);
                        _groupBySlot[slot] = null;
                    }

                    continue;
                }

                // 基础区域只依赖权威坐标和四叉树根边界。
                uint regionCode = spatialIndex.GetBaseRegionCode(motion[slot].Position);
                if (previous != null && previous.RegionCode == regionCode)
                    continue;

                previous?.Remove(slot);
                Group next = GetGroupWithSpace(regionCode, spatialIndex, playerPositions, tick);
                next.Add(slot);
                _groupBySlot[slot] = next;
            }

            if (tick % TierRefreshInterval == 0)
            {
                using var refreshTiersScope = RefreshTiersMarker.Auto();
                foreach (Region region in _regions.Values)
                    region.RefreshTier(playerPositions, tick);
            }
        }

        internal void Clear()
        {
            Array.Clear(_groupBySlot, 0, _groupBySlot.Length);
            _regions.Clear();
            _groups.Clear();
        }

        private Group GetGroupWithSpace(uint regionCode, MonsterSpatialIndex spatialIndex,
            ReadOnlySpan<Vector2> playerPositions, uint tick)
        {
            if (!_regions.TryGetValue(regionCode, out Region region))
            {
                region = new Region(spatialIndex.GetBaseRegionCenter(regionCode), spatialIndex.BaseRegionHalfSize);
                region.RefreshTier(playerPositions, tick);
                _regions.Add(regionCode, region);
            }

            foreach (Group group in region.Groups)
            {
                if (group.Count < GroupCapacity)
                    return group;
            }

            var created = new Group((uint)_groups.Count + 1, regionCode, region.IntervalTicks, tick);
            region.Groups.Add(created);
            _groups.Add(created);
            return created;
        }

        private sealed class Region
        {
            private readonly Vector2 _center;
            private readonly float _halfSize;

            internal readonly List<Group> Groups = new();
            internal byte IntervalTicks { get; private set; } = OuterInterval;

            internal Region(Vector2 center, float halfSize)
            {
                _center = center;
                _halfSize = halfSize;
            }

            internal void RefreshTier(ReadOnlySpan<Vector2> playerPositions, uint tick)
            {
                float nearestDistanceSquared = float.PositiveInfinity;
                foreach (Vector2 position in playerPositions)
                {
                    float dx = Mathf.Max(0f, Mathf.Abs(position.x - _center.x) - _halfSize);
                    float dz = Mathf.Max(0f, Mathf.Abs(position.y - _center.y) - _halfSize);
                    nearestDistanceSquared = Mathf.Min(nearestDistanceSquared, dx * dx + dz * dz);
                }

                // 退出半径略大于进入半径，避免队友贴着边界走时频繁换层。
                float innerDistance = IntervalTicks == InnerInterval ? 24f : 20f;
                float middleDistance = IntervalTicks <= MiddleInterval ? 46f : 40f;
                byte interval = nearestDistanceSquared <= innerDistance * innerDistance ? InnerInterval
                    : nearestDistanceSquared <= middleDistance * middleDistance ? MiddleInterval
                    : OuterInterval;
                if (interval == IntervalTicks)
                    return;

                IntervalTicks = interval;
                foreach (Group group in Groups)
                    group.SetInterval(interval, tick);
            }
        }

        internal sealed class Group
        {
            private readonly int[] _slots = new int[GroupCapacity];

            internal uint Id { get; }
            internal uint RegionCode { get; }
            internal uint MembershipRevision { get; private set; }
            internal uint PublishedMembershipRevision { get; set; }
            internal byte IntervalTicks { get; private set; }
            internal uint NextPoseTick { get; set; }
            internal int Count { get; private set; }
            internal ReadOnlySpan<int> MemberSlots => _slots.AsSpan(0, Count);

            internal Group(uint id, uint regionCode, byte intervalTicks, uint tick)
            {
                Id = id;
                RegionCode = regionCode;
                IntervalTicks = intervalTicks;
                // 同层组分摊到各个 Tick，避免所有组落在同一相位。
                NextPoseTick = unchecked(tick + (Id - 1u) % intervalTicks);
            }

            internal void SetInterval(byte intervalTicks, uint tick)
            {
                IntervalTicks = intervalTicks;
                uint phaseTick = unchecked(tick + (Id - 1u) % intervalTicks);
                // 保留已经到期的工作，避免换层让超预算的组重新排队。
                if (MonsterMath.IsNewer(NextPoseTick, phaseTick))
                    NextPoseTick = phaseTick;
            }

            internal void Add(int slot)
            {
                _slots[Count++] = slot;
                MembershipRevision++;
            }

            internal void Remove(int slot)
            {
                int index = Array.IndexOf(_slots, slot, 0, Count);
                // 用末项填补空位，版本随成员顺序一起更新。
                _slots[index] = _slots[Count - 1];
                Count--;
                MembershipRevision++;
            }
        }
    }
}
