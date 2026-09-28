using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 房间级的Monster数据容器。Slot在触发Clear前是稳定的，
    /// 为了避免Slot复用导致的判定问题，这里的死亡Slot在单个房间内不进行复用
    /// </summary>
    public sealed class MonsterWorld
    {
        private const int DefaultCapacity = 64;
        //SoA建立实体索引
        private MonsterMetaData[] _meta;
        private MonsterMotionData[] _motion;
        private MonsterHealthData[] _health;
        private MonsterStatusData[] _status;
        private MonsterAttackData[] _attack;
        private MonsterPresentationData[] _presentation;
        private int _slotCount;
        private int _aliveCount;
        /// <summary>
        /// 已经使用了多少个Slot
        /// </summary>
        public int SlotCount => _slotCount;
        /// <summary>
        /// 活着的Slot有多少
        /// </summary>
        public int AliveCount => _aliveCount;
        /// <summary>
        /// 数组实际分配大小
        /// </summary>
        public int Capacity => _meta.Length;
        //暴露内部数组到Monster模块
        internal MonsterMetaData[] Meta => _meta;
        internal MonsterMotionData[] Motion => _motion;
        internal MonsterAttackData[] Attack => _attack;
        internal MonsterPresentationData[] Presentation => _presentation;

        public MonsterWorld(int initialCapacity = DefaultCapacity)
        {
            int capacity = initialCapacity == 0 ? 0 : Math.Max(DefaultCapacity, initialCapacity);
            _meta = new MonsterMetaData[capacity];
            _motion = new MonsterMotionData[capacity];
            _health = new MonsterHealthData[capacity];
            _status = new MonsterStatusData[capacity];
            _attack = new MonsterAttackData[capacity];
            _presentation = new MonsterPresentationData[capacity];
        }
        /// <summary>
        /// 创建一只怪
        /// </summary>
        public int Create(ushort configIndex, Vector2 position, float yaw, float maxHealth)
        {
            EnsureCapacity(_slotCount + 1);
            //只沿序往尾部加slot,不会干涉死掉的，或者空的位置
            int slot = _slotCount++;
            _meta[slot] = new MonsterMetaData { ConfigIndex = configIndex, IsActive = true };
            _motion[slot] = new MonsterMotionData { Position = position, Velocity = Vector2.zero, Yaw = yaw };
            _health[slot] = new MonsterHealthData { CurrentHealth = maxHealth };
            _status[slot] = new MonsterStatusData { SpeedMultiplier = 1f };
            _attack[slot] = new MonsterAttackData { Phase = MonsterAttackPhase.Ready };
            _presentation[slot] = default;
            _aliveCount++;
            return slot;
        }

        private bool Kill(int slot)
        {
            //无符号范围检查写法，兼顾为负数的情况
            if ((uint)slot >= (uint)_slotCount || !_meta[slot].IsActive) 
                return false;
            //只管IsActive，剩下的房间结束自己会清
            _meta[slot].IsActive = false;
            _aliveCount--;
            return true;
        }

        public bool Damage(int slot, float amount)
        {
            if (!IsActive(slot))
                return false;

            ref MonsterHealthData health = ref _health[slot];
            health.CurrentHealth = Mathf.Max(0f, health.CurrentHealth - amount);
            _presentation[slot].HitSequence++;
            if (health.CurrentHealth <= 0f)
                Kill(slot);
            return true;
        }

        public bool TryGetPosition(int slot, out Vector2 position)
        {
            if (!IsActive(slot))
            {
                position = default;
                return false;
            }
            position = _motion[slot].Position;
            return true;
        }

        public bool TryGetHealthRatio(int slot, float maxHealth, out float ratio)
        {
            if (!IsActive(slot))
            {
                ratio = 0f;
                return false;
            }
            ratio = _health[slot].CurrentHealth / maxHealth;
            return true;
        }

        public bool ApplySlow(int slot, float speedMultiplier, uint currentTick, uint endTick)
        {
            if (!IsActive(slot)) return false;
            ref MonsterStatusData status = ref _status[slot];
            if (MonsterMath.HasReached(currentTick, status.SlowEndTick) ||
                speedMultiplier < status.SpeedMultiplier)
                status.SpeedMultiplier = speedMultiplier;
            if (MonsterMath.IsNewer(endTick, status.SlowEndTick))
                status.SlowEndTick = endTick;
            return true;
        }

        public bool ApplyStun(int slot, uint currentTick, uint endTick)
        {
            if (!IsActive(slot)) return false;
            ref MonsterStatusData status = ref _status[slot];
            uint previousEnd = MonsterMath.HasReached(currentTick, status.StunEndTick)
                ? currentTick : status.StunEndTick;
            if (!MonsterMath.IsNewer(endTick, previousEnd)) return true;
            if (_attack[slot].Phase != MonsterAttackPhase.Ready)
                _attack[slot].PhaseEndTick += endTick - previousEnd;
            status.StunEndTick = endTick;
            return true;
        }

        public bool Push(int slot, Vector2 offset)
        {
            if (!IsActive(slot)) return false;
            _motion[slot].Position += offset;
            return true;
        }

        internal bool IsStunned(int slot, uint tick) =>
            !MonsterMath.HasReached(tick, _status[slot].StunEndTick);

        internal float GetSpeedMultiplier(int slot, uint tick)
        {
            ref MonsterStatusData status = ref _status[slot];
            if (!MonsterMath.HasReached(tick, status.SlowEndTick))
                return status.SpeedMultiplier;
            status.SpeedMultiplier = 1f;
            return 1f;
        }
        /// <summary>
        /// 这个Slot存在？且他是否还活着？
        /// <summary>
        public bool IsActive(int slot) => (uint)slot < (uint)_slotCount && _meta[slot].IsActive;

        /// <summary>
        /// 扩容方法，传统的X2扩容
        /// </summary>
        public void EnsureCapacity(int expectedTotalCount)
        {
            if (expectedTotalCount <= Capacity) 
                return;
            int next = Math.Max(expectedTotalCount, Capacity == 0 ? DefaultCapacity : Capacity * 2);
            Array.Resize(ref _meta, next);
            Array.Resize(ref _motion, next);
            Array.Resize(ref _health, next);
            Array.Resize(ref _status, next);
            Array.Resize(ref _attack, next);
            Array.Resize(ref _presentation, next);
        }

        public void Clear()
        {
            //这里只是修改判定，没有进行清理，下次使用会直接覆盖
            _slotCount = 0;
            _aliveCount = 0;
        }
         
    }
}
