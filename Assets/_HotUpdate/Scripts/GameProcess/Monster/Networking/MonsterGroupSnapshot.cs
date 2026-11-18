using System;
using ProjectGame.Core.Monsters;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 怪物分层层级
    /// </summary>
    public enum MonsterNetworkTier : byte
    {
        Inner = 0,
        Middle = 1,
        Outer = 2,
    }


    /// <summary>
    /// 一个群组在某个ServerTick的完整表现状态。
    /// 第一版不做增量，快照包含群组当前所有成员。
    /// </summary>
    public sealed class MonsterGroupSnapshot
    {
        // [0, MemberCount) 内的状态与 MembershipRevision 对应的成员表按下标一一对应。
        private MonsterPackedState[] _states;

        public uint GroupId { get; private set; }
        public uint MembershipRevision { get; private set; }
        public uint ServerTick { get; private set; }
        public int MemberCount { get; private set; }

        internal MonsterPackedState[] MemberBuffer => _states;

        internal MonsterGroupSnapshot(int initialCapacity)
        {
            _states = new MonsterPackedState[initialCapacity];
        }

        internal void BeginWrite(
             uint groupId,
             uint membershipRevision,
             uint serverTick,
             int memberCount)
        {
            if (memberCount > _states.Length)
                Array.Resize(ref _states, Math.Max(memberCount, _states.Length * 2));

            GroupId = groupId;
            MembershipRevision = membershipRevision;
            ServerTick = serverTick;
            MemberCount = memberCount;
        }
    }
}