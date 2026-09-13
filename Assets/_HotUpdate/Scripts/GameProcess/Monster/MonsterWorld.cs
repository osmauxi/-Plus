using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>房间级 Monster 数据容器。Slot 在 Clear 前稳定，死亡 Slot 不复用。</summary>
    public sealed class MonsterWorld
    {
        private const int DefaultCapacity = 64;
        private MonsterMetaData[] _meta;
        private MonsterMotionData[] _motion;
        private MonsterTargetData[] _target;
        private MonsterMoveData[] _move;
        private MonsterAttackData[] _attack;
        private MonsterPresentationData[] _presentation;
        private int _slotCount;
        private int _aliveCount;

        public int SlotCount => _slotCount;
        public int AliveCount => _aliveCount;
        public int Capacity => _meta.Length;

        internal MonsterMetaData[] Meta => _meta;
        internal MonsterMotionData[] Motion => _motion;
        internal MonsterTargetData[] Target => _target;
        internal MonsterMoveData[] Move => _move;
        internal MonsterAttackData[] Attack => _attack;
        internal MonsterPresentationData[] Presentation => _presentation;

        public MonsterWorld(int initialCapacity = DefaultCapacity)
        {
            if (initialCapacity < 0) throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            int capacity = initialCapacity == 0 ? 0 : Math.Max(DefaultCapacity, initialCapacity);
            _meta = new MonsterMetaData[capacity];
            _motion = new MonsterMotionData[capacity];
            _target = new MonsterTargetData[capacity];
            _move = new MonsterMoveData[capacity];
            _attack = new MonsterAttackData[capacity];
            _presentation = new MonsterPresentationData[capacity];
        }

        public int Create(ushort configIndex, Vector2 position, float yaw)
        {
            if (!MonsterMath.IsFinite(position) || !MonsterMath.IsFinite(yaw))
                throw new ArgumentOutOfRangeException(nameof(position));
            if (_slotCount == int.MaxValue) throw new InvalidOperationException("Monster Slot 已耗尽。");
            EnsureCapacity(_slotCount + 1);
            int slot = _slotCount++;
            _meta[slot] = new MonsterMetaData { ConfigIndex = configIndex, IsActive = true };
            _motion[slot] = new MonsterMotionData { Position = position, Velocity = Vector2.zero, Yaw = yaw };
            _target[slot] = default;
            _move[slot] = default;
            _attack[slot] = new MonsterAttackData { Phase = MonsterAttackPhase.Ready };
            _presentation[slot] = default;
            _aliveCount++;
            return slot;
        }

        public bool Kill(int slot)
        {
            if ((uint)slot >= (uint)_slotCount || !_meta[slot].IsActive) return false;
            _meta[slot].IsActive = false;
            _aliveCount--;
            return true;
        }

        public bool IsActive(int slot) => (uint)slot < (uint)_slotCount && _meta[slot].IsActive;

        public void EnsureCapacity(int expectedTotalCount)
        {
            if (expectedTotalCount < 0) throw new ArgumentOutOfRangeException(nameof(expectedTotalCount));
            if (expectedTotalCount <= Capacity) return;
            int next = Capacity == 0 ? DefaultCapacity : Capacity;
            while (next < expectedTotalCount)
            {
                if (next > int.MaxValue / 2) { next = expectedTotalCount; break; }
                next *= 2;
            }
            Array.Resize(ref _meta, next);
            Array.Resize(ref _motion, next);
            Array.Resize(ref _target, next);
            Array.Resize(ref _move, next);
            Array.Resize(ref _attack, next);
            Array.Resize(ref _presentation, next);
        }

        public void Clear()
        {
            _slotCount = 0;
            _aliveCount = 0;
        }

        /// <summary>仅供尚未发布 SpawnBatch 的事务失败回滚；正常死亡和已发布 Slot 禁止调用。</summary>
        internal void RollbackUnpublishedTail(int startSlot)
        {
            if ((uint)startSlot > (uint)_slotCount) throw new ArgumentOutOfRangeException(nameof(startSlot));
            for (int slot = startSlot; slot < _slotCount; slot++)
                if (_meta[slot].IsActive) _aliveCount--;
            _slotCount = startSlot;
        }
    }
}
