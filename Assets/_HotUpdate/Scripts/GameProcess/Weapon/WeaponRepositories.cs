using System;
using System.Collections.Generic;

namespace ProjectGame.HotFix.Gameplay.Weapon
{
    public sealed class WeaponStatSnapshotRepository
    {
        /// <summary>
        /// 按照ID存的快照包，子弹通过ID实时获取他们对应的数值
        /// </summary>
        private readonly Dictionary<ushort, WeaponStatSnapshot> _snapshots = new();
        //32768开始的ID为动态生成的Effect快照，低位开始是配表生成的基础快照
        private ushort _nextRuntimeId = 32768;

        public void Register(in WeaponStatSnapshot snapshot)
        {
            //已发射弹丸引用的快照不能原地覆盖，所以不直接_snapshots[snapshot.Id] = 
            _snapshots.Add(snapshot.Id, snapshot);
        }

        public bool TryGet(ushort id, out WeaponStatSnapshot snapshot)
        {
            return _snapshots.TryGetValue(id, out snapshot);
        }

        public WeaponStatSnapshot Get(ushort id) => _snapshots[id];
        /// <summary>
        /// 给当前快照找一个没用的ID
        /// </summary>
        public ushort AllocateRuntimeId() => _nextRuntimeId++;

        public void Clear()
        {
            _snapshots.Clear();
            _nextRuntimeId = 32768;
        }
    }

    public sealed class EffectSetRepository
    {
        private readonly Dictionary<ushort, EffectSet> _sets = new()
        {
            [EffectSet.Empty.Id] = EffectSet.Empty,
        };
        private ushort _nextId = 1;

        public void Register(EffectSet set) => _sets.Add(set.Id, set);

        public ushort AllocateId() => _nextId++;

        public bool TryGet(ushort id, out EffectSet set)
        {
            return _sets.TryGetValue(id, out set);
        }

        public EffectSet Get(ushort id) => _sets[id];

        public void Clear()
        {
            _sets.Clear();
            _sets.Add(EffectSet.Empty.Id, EffectSet.Empty);
            _nextId = 1;
        }
    }
    /// <summary>
    /// 存一次射击的共享上下文，存本次Shot还剩多少子弹没有消失
    /// 外部ProjectileWorld可以通过ID查到这里的上下文
    /// </summary>
    public sealed class ShotRepository
    {
        private readonly Dictionary<ulong, ShotContext> _shots = new();
        //简单的引用计数，这里只存引用，不存实际信息的
        private readonly Dictionary<ulong, int> _references = new();
        private ulong _nextShotId = 1;
        public ulong AllocateShotId() => _nextShotId++;

        public int Count => _shots.Count;

        public void Register(in ShotContext shot, int projectileCount = 1)
        {
            _shots.Add(shot.ShotId, shot);
            _references.Add(shot.ShotId, projectileCount);
        }

        public ShotContext Get(ulong shotId) => _shots[shotId];

        public bool TryGet(ulong shotId, out ShotContext shot)
        {
            return _shots.TryGetValue(shotId, out shot);
        }

        public bool Remove(ulong shotId)
        {
            _references.Remove(shotId);
            return _shots.Remove(shotId);
        }

        public void RetainProjectiles(ulong shotId, int projectileCount)
        {
            _references[shotId] += projectileCount;
        }

        public void ReleaseProjectile(ulong shotId)
        {
            if (!_references.TryGetValue(shotId, out int count)) return;
            if (count <= 1) Remove(shotId);
            else _references[shotId] = count - 1;
        }

        public void Clear()
        {
            _shots.Clear();
            _references.Clear();
            _nextShotId = 1;
        }
    }
    /// <summary>
    /// 当前世界中存在的所有ProjectileState
    /// </summary>
    public sealed class ProjectileWorld
    {
        private const int InitialCapacity = 256;

        private uint[] _projectileIds = new uint[InitialCapacity];
        private ulong[] _shotIds = new ulong[InitialCapacity];
        private WeaponStatSnapshot[] _statSnapshots = new WeaponStatSnapshot[InitialCapacity];
        private ulong[] _ownerEntityIds = new ulong[InitialCapacity];
        private UnityEngine.Vector3[] _positions = new UnityEngine.Vector3[InitialCapacity];
        private UnityEngine.Vector3[] _velocities = new UnityEngine.Vector3[InitialCapacity];
        private float[] _remainingLifeTimes = new float[InitialCapacity];
        private float[] _damageMultipliers = new float[InitialCapacity];
        private float[] _sizeMultipliers = new float[InitialCapacity];
        private byte[] _pierceRemainings = new byte[InitialCapacity];
        private byte[] _bounceRemainings = new byte[InitialCapacity];
        private byte[] _generations = new byte[InitialCapacity];
        private ushort[] _hitCounts = new ushort[InitialCapacity];
        private ProjectileFlags[] _flags = new ProjectileFlags[InitialCapacity];
        private int _count;
        private uint _nextProjectileId = 1;

        public int Count => _count;

        internal uint[] ProjectileIds => _projectileIds;
        internal ulong[] ShotIds => _shotIds;
        internal WeaponStatSnapshot[] StatSnapshots => _statSnapshots;
        internal ulong[] OwnerEntityIds => _ownerEntityIds;
        internal UnityEngine.Vector3[] Positions => _positions;
        internal UnityEngine.Vector3[] Velocities => _velocities;
        internal float[] RemainingLifeTimes => _remainingLifeTimes;
        internal float[] SizeMultipliers => _sizeMultipliers;
        internal ProjectileFlags[] Flags => _flags;

        public uint AllocateProjectileId() => _nextProjectileId++;

        public void Add(in ProjectileState projectile, in WeaponStatSnapshot stats, ulong ownerEntityId)
        {
            EnsureCapacity(_count + 1);
            Write(_count, projectile);
            _statSnapshots[_count] = stats;
            _ownerEntityIds[_count] = ownerEntityId;
            _count++;
        }

        public void Read(int index, ref ProjectileState projectile)
        {
            projectile.ProjectileId = _projectileIds[index];
            projectile.ShotId = _shotIds[index];
            projectile.Position = _positions[index];
            projectile.Velocity = _velocities[index];
            projectile.RemainingLifeTime = _remainingLifeTimes[index];
            projectile.DamageMultiplier = _damageMultipliers[index];
            projectile.SizeMultiplier = _sizeMultipliers[index];
            projectile.PierceRemaining = _pierceRemainings[index];
            projectile.BounceRemaining = _bounceRemainings[index];
            projectile.Generation = _generations[index];
            projectile.HitCount = _hitCounts[index];
            projectile.Flags = _flags[index];
        }

        public ulong GetShotId(int index) => _shotIds[index];
        public ulong GetOwnerEntityId(int index) => _ownerEntityIds[index];
        public uint GetProjectileId(int index) => _projectileIds[index];
        public UnityEngine.Vector3 GetPosition(int index) => _positions[index];
        public UnityEngine.Vector3 GetVelocity(int index) => _velocities[index];
        public float GetRemainingLifeTime(int index) => _remainingLifeTimes[index];
        public float GetSizeMultiplier(int index) => _sizeMultipliers[index];
        public ProjectileFlags GetFlags(int index) => _flags[index];

        public void SetPosition(int index, UnityEngine.Vector3 value) => _positions[index] = value;
        public void SetRemainingLifeTime(int index, float value) => _remainingLifeTimes[index] = value;
        public void SetFlags(int index, ProjectileFlags value) => _flags[index] = value;

        public void Write(int index, in ProjectileState projectile)
        {
            _projectileIds[index] = projectile.ProjectileId;
            _shotIds[index] = projectile.ShotId;
            _positions[index] = projectile.Position;
            _velocities[index] = projectile.Velocity;
            _remainingLifeTimes[index] = projectile.RemainingLifeTime;
            _damageMultipliers[index] = projectile.DamageMultiplier;
            _sizeMultipliers[index] = projectile.SizeMultiplier;
            _pierceRemainings[index] = projectile.PierceRemaining;
            _bounceRemainings[index] = projectile.BounceRemaining;
            _generations[index] = projectile.Generation;
            _hitCounts[index] = projectile.HitCount;
            _flags[index] = projectile.Flags;
        }

        public void RemoveAtSwapBack(int index)
        {
            int lastIndex = _count - 1;
            if (index != lastIndex)
                Copy(lastIndex, index);

            _count = lastIndex;
        }

        public void Clear()
        {
            _count = 0;
            _nextProjectileId = 1;
        }

        private void EnsureCapacity(int required)
        {
            if (required <= _projectileIds.Length)
                return;

            int capacity = Math.Max(required, _projectileIds.Length * 2);
            Array.Resize(ref _projectileIds, capacity);
            Array.Resize(ref _shotIds, capacity);
            Array.Resize(ref _statSnapshots, capacity);
            Array.Resize(ref _ownerEntityIds, capacity);
            Array.Resize(ref _positions, capacity);
            Array.Resize(ref _velocities, capacity);
            Array.Resize(ref _remainingLifeTimes, capacity);
            Array.Resize(ref _damageMultipliers, capacity);
            Array.Resize(ref _sizeMultipliers, capacity);
            Array.Resize(ref _pierceRemainings, capacity);
            Array.Resize(ref _bounceRemainings, capacity);
            Array.Resize(ref _generations, capacity);
            Array.Resize(ref _hitCounts, capacity);
            Array.Resize(ref _flags, capacity);
        }

        private void Copy(int source, int destination)
        {
            _projectileIds[destination] = _projectileIds[source];
            _shotIds[destination] = _shotIds[source];
            _statSnapshots[destination] = _statSnapshots[source];
            _ownerEntityIds[destination] = _ownerEntityIds[source];
            _positions[destination] = _positions[source];
            _velocities[destination] = _velocities[source];
            _remainingLifeTimes[destination] = _remainingLifeTimes[source];
            _damageMultipliers[destination] = _damageMultipliers[source];
            _sizeMultipliers[destination] = _sizeMultipliers[source];
            _pierceRemainings[destination] = _pierceRemainings[source];
            _bounceRemainings[destination] = _bounceRemainings[source];
            _generations[destination] = _generations[source];
            _hitCounts[destination] = _hitCounts[source];
            _flags[destination] = _flags[source];
        }
    }
}
