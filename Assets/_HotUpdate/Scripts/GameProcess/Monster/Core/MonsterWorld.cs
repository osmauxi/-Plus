using System;
using System.Collections.Generic;
using ProjectGame.Core.Monsters;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 房间级 Monster 数据容器。
    /// EntityId 是跨系统身份，StorageIndex 只表示当前密集 SoA 行。
    /// </summary>
    public sealed class MonsterWorld
    {
        private const int DefaultCapacity = 64;

        private MonsterMetaData[] _meta;
        private MonsterMotionData[] _motion;
        private MonsterHealthData[] _health;
        private MonsterStatusData[] _status;
        private MonsterAttackData[] _attack;
        private MonsterPresentationData[] _presentation;
        private ulong[] _entityIdByStorageIndex;
        private readonly Dictionary<ulong, int> _storageIndexByEntityId = new();
        private int[] _activeStorageIndices;
        private int[] _pendingDeathStorageIndices;
        private int _aliveCount;
        private int _pendingDeathCount;
        private uint _nextEntitySequence;

        public int StorageCount => _aliveCount;
        public int AliveCount => _aliveCount;
        public int Capacity => _meta.Length;

        internal MonsterMetaData[] Meta => _meta;
        internal MonsterMotionData[] Motion => _motion;
        internal MonsterStatusData[] Status => _status;
        internal MonsterAttackData[] Attack => _attack;
        internal MonsterPresentationData[] Presentation => _presentation;
        internal int[] ActiveStorageIndices => _activeStorageIndices;

        public MonsterWorld(int initialCapacity = DefaultCapacity)
        {
            int capacity = initialCapacity == 0 ? 0 : Math.Max(DefaultCapacity, initialCapacity);
            _meta = new MonsterMetaData[capacity];
            _motion = new MonsterMotionData[capacity];
            _health = new MonsterHealthData[capacity];
            _status = new MonsterStatusData[capacity];
            _attack = new MonsterAttackData[capacity];
            _presentation = new MonsterPresentationData[capacity];
            _entityIdByStorageIndex = new ulong[capacity];
            _activeStorageIndices = new int[capacity];
            _pendingDeathStorageIndices = new int[capacity];
        }

        public int Create(ushort configIndex, Vector2 position, float yaw, float maxHealth)
        {
            int storageIndex = _aliveCount;
            ulong entityId = MonsterRuntimeService.MonsterEntityIdBase + _nextEntitySequence++;
            _entityIdByStorageIndex[storageIndex] = entityId;
            _storageIndexByEntityId.Add(entityId, storageIndex);
            _meta[storageIndex] = new MonsterMetaData { ConfigIndex = configIndex, IsActive = true };
            _motion[storageIndex] = new MonsterMotionData
            {
                Position = position,
                Velocity = Vector2.zero,
                Yaw = yaw
            };
            _health[storageIndex] = new MonsterHealthData { CurrentHealth = maxHealth };
            _status[storageIndex] = new MonsterStatusData { SpeedMultiplier = 1f };
            _attack[storageIndex] = new MonsterAttackData { Phase = MonsterAttackPhase.Ready };
            _presentation[storageIndex] = default;
            _activeStorageIndices[storageIndex] = storageIndex;
            _aliveCount++;
            return storageIndex;
        }

        private bool Kill(int storageIndex)
        {
            if (!IsActive(storageIndex))
                return false;

            _meta[storageIndex].IsActive = false;
            _pendingDeathStorageIndices[_pendingDeathCount++] = storageIndex;
            return true;
        }

        public bool CompactDead()
        {
            if (_pendingDeathCount == 0)
                return false;

            for (int i = 0; i < _pendingDeathCount; i++)
            {
                int deadStorageIndex = _pendingDeathStorageIndices[i];
                _storageIndexByEntityId[_entityIdByStorageIndex[deadStorageIndex]] = -1;
            }

            int writeStorageIndex = 0;
            int initialAliveCount = _aliveCount;
            for (int readStorageIndex = 0; readStorageIndex < initialAliveCount; readStorageIndex++)
            {
                if (!_meta[readStorageIndex].IsActive)
                    continue;

                if (writeStorageIndex != readStorageIndex)
                {
                    CopyStorageRow(readStorageIndex, writeStorageIndex);
                    ulong movedEntityId = _entityIdByStorageIndex[writeStorageIndex];
                    _storageIndexByEntityId[movedEntityId] = writeStorageIndex;
                }

                writeStorageIndex++;
            }

            _aliveCount = writeStorageIndex;
            _pendingDeathCount = 0;
            return true;
        }

        public bool Damage(int storageIndex, float amount)
        {
            if (!IsActive(storageIndex))
                return false;

            ref MonsterHealthData health = ref _health[storageIndex];
            health.CurrentHealth = Mathf.Max(0f, health.CurrentHealth - amount);
            _presentation[storageIndex].HitSequence++;
            if (health.CurrentHealth <= 0f)
                Kill(storageIndex);
            return true;
        }

        public bool RecordHit(int storageIndex)
        {
            if (!IsActive(storageIndex))
                return false;

            _presentation[storageIndex].HitSequence++;
            return true;
        }

        public bool TryGetPosition(int storageIndex, out Vector2 position)
        {
            if (!IsActive(storageIndex))
            {
                position = default;
                return false;
            }

            position = _motion[storageIndex].Position;
            return true;
        }

        public bool TryGetHealthRatio(int storageIndex, float maxHealth, out float ratio)
        {
            if (!IsActive(storageIndex))
            {
                ratio = 0f;
                return false;
            }

            ratio = _health[storageIndex].CurrentHealth / maxHealth;
            return true;
        }

        public bool ApplySlow(int storageIndex, float speedMultiplier, uint currentTick, uint endTick)
        {
            if (!IsActive(storageIndex))
                return false;

            ref MonsterStatusData status = ref _status[storageIndex];
            if (MonsterMath.HasReached(currentTick, status.SlowEndTick) ||
                speedMultiplier < status.SpeedMultiplier)
                status.SpeedMultiplier = speedMultiplier;
            if (MonsterMath.IsNewer(endTick, status.SlowEndTick))
                status.SlowEndTick = endTick;
            return true;
        }

        public bool ApplyStun(int storageIndex, uint currentTick, uint endTick)
        {
            if (!IsActive(storageIndex))
                return false;

            ref MonsterStatusData status = ref _status[storageIndex];
            uint previousEnd = MonsterMath.HasReached(currentTick, status.StunEndTick)
                ? currentTick
                : status.StunEndTick;
            if (!MonsterMath.IsNewer(endTick, previousEnd))
                return true;
            if (_attack[storageIndex].Phase != MonsterAttackPhase.Ready)
                _attack[storageIndex].PhaseEndTick += endTick - previousEnd;
            status.StunEndTick = endTick;
            return true;
        }

        public bool Push(int storageIndex, Vector2 offset)
        {
            if (!IsActive(storageIndex))
                return false;

            _motion[storageIndex].Position += offset;
            return true;
        }

        internal bool IsStunned(int storageIndex, uint tick) =>
            !MonsterMath.HasReached(tick, _status[storageIndex].StunEndTick);

        internal float GetSpeedMultiplier(int storageIndex, uint tick)
        {
            ref MonsterStatusData status = ref _status[storageIndex];
            if (!MonsterMath.HasReached(tick, status.SlowEndTick))
                return status.SpeedMultiplier;

            status.SpeedMultiplier = 1f;
            return 1f;
        }

        public bool IsActive(int storageIndex) =>
            (uint)storageIndex < (uint)_aliveCount &&
            _meta[storageIndex].IsActive;

        internal ulong GetEntityId(int storageIndex) =>
            _entityIdByStorageIndex[storageIndex];

        internal int ResolveStorageIndex(ulong entityId) =>
            _storageIndexByEntityId[entityId];

        public void EnsureCapacity(int expectedTotalCount)
        {
            if (expectedTotalCount <= Capacity)
                return;

            int next = Math.Max(
                expectedTotalCount,
                Capacity == 0 ? DefaultCapacity : Capacity * 2);
            Array.Resize(ref _meta, next);
            Array.Resize(ref _motion, next);
            Array.Resize(ref _health, next);
            Array.Resize(ref _status, next);
            Array.Resize(ref _attack, next);
            Array.Resize(ref _presentation, next);
            Array.Resize(ref _entityIdByStorageIndex, next);
            Array.Resize(ref _activeStorageIndices, next);
            Array.Resize(ref _pendingDeathStorageIndices, next);
        }

        public void Clear()
        {
            _aliveCount = 0;
            _pendingDeathCount = 0;
            _nextEntitySequence = 0;
            _storageIndexByEntityId.Clear();
        }

        private void CopyStorageRow(int source, int destination)
        {
            _meta[destination] = _meta[source];
            _motion[destination] = _motion[source];
            _health[destination] = _health[source];
            _status[destination] = _status[source];
            _attack[destination] = _attack[source];
            _presentation[destination] = _presentation[source];
            _entityIdByStorageIndex[destination] = _entityIdByStorageIndex[source];
            _activeStorageIndices[destination] = destination;
        }
    }
}
