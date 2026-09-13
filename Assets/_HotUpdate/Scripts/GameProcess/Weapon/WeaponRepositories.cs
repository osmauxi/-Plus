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
            Validate(snapshot);
            //已发射弹丸引用的快照不能原地覆盖，所以不直接_snapshots[snapshot.Id] = 
            _snapshots.Add(snapshot.Id, snapshot);
        }

        public bool TryGet(ushort id, out WeaponStatSnapshot snapshot)
        {
            return _snapshots.TryGetValue(id, out snapshot);
        }

        public WeaponStatSnapshot Get(ushort id)
        {
            if (!_snapshots.TryGetValue(id, out WeaponStatSnapshot snapshot))
                throw new KeyNotFoundException($"找不到 WeaponStatSnapshot：{id}");

            return snapshot;
        }
        /// <summary>
        /// 给当前快照找一个没用的ID
        /// </summary>
        public ushort AllocateRuntimeId()
        {
            for (int i = 0; i < ushort.MaxValue; i++)
            {
                ushort candidate = _nextRuntimeId++;
                //0在这里代表无效值，不使用
                if (_nextRuntimeId == 0) 
                    _nextRuntimeId = 1;
                if (candidate != 0 && !_snapshots.ContainsKey(candidate))
                    return candidate;
            }

            throw new InvalidOperationException("WeaponStatSnapshot ID 已耗尽，必须开始新会话。");
        }

        public void Clear()
        {
            _snapshots.Clear();
            _nextRuntimeId = 32768;
        }

        internal static void Validate(in WeaponStatSnapshot snapshot)
        {
            if (snapshot.Id == 0)
                throw new ArgumentException("WeaponStatSnapshot Id 不能为 0。", nameof(snapshot));
            if (!IsFiniteNonNegative(snapshot.Damage))
                throw new ArgumentException("Damage 必须是非负有限值。", nameof(snapshot));
            if (!IsFinitePositive(snapshot.FireRate))
                throw new ArgumentException("FireRate 必须大于 0。", nameof(snapshot));
            if (!IsFiniteNonNegative(snapshot.ReloadTime))
                throw new ArgumentException("ReloadTime 必须是非负有限值。", nameof(snapshot));
            if (snapshot.MagSize == 0)
                throw new ArgumentException("MagSize 必须大于 0。", nameof(snapshot));
            if (!IsFinitePositive(snapshot.ProjectileSpeed))
                throw new ArgumentException("ProjectileSpeed 必须大于 0。", nameof(snapshot));
            if (!IsFinitePositive(snapshot.ProjectileLifeTime))
                throw new ArgumentException("ProjectileLifeTime 必须大于 0。", nameof(snapshot));
            if (snapshot.ProjectileCount == 0)
                throw new ArgumentException("ProjectileCount 必须大于 0。", nameof(snapshot));
            if (!IsFiniteNonNegative(snapshot.CritChance) || snapshot.CritChance > 1f)
                throw new ArgumentException("CritChance 必须位于 0~1。", nameof(snapshot));
            if (!IsFiniteNonNegative(snapshot.CritMultiplier))
                throw new ArgumentException("CritMultiplier 必须是非负有限值。", nameof(snapshot));
            if (!IsFiniteNonNegative(snapshot.ProjectileSize))
                throw new ArgumentException("ProjectileSize 必须是非负有限值。", nameof(snapshot));
            if (!IsFiniteNonNegative(snapshot.SpreadAngle))
                throw new ArgumentException("SpreadAngle 必须是非负有限值。", nameof(snapshot));
        }

        private static bool IsFinitePositive(float value)
        {
            return value > 0f && !float.IsInfinity(value);
        }

        private static bool IsFiniteNonNegative(float value)
        {
            return value >= 0f && !float.IsInfinity(value);
        }
    }

    public sealed class EffectSetRepository
    {
        private readonly Dictionary<ushort, EffectSet> _sets = new()
        {
            [EffectSet.Empty.Id] = EffectSet.Empty,
        };
        private ushort _nextId = 1;

        public void Register(EffectSet set)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));

            if (set.Id == 0)
                throw new ArgumentException("EffectSet ID=0 只保留给 Empty。", nameof(set));
            _sets.Add(set.Id, set);
        }

        public ushort AllocateId()
        {
            for (int i = 0; i < ushort.MaxValue; i++)
            {
                ushort candidate = _nextId++;
                if (_nextId == 0) 
                    _nextId = 1;
                if (candidate != 0 && !_sets.ContainsKey(candidate))
                   return candidate;
            }

            throw new InvalidOperationException("EffectSet ID 已耗尽，必须开始新会话。");
        }

        public bool TryGet(ushort id, out EffectSet set)
        {
            return _sets.TryGetValue(id, out set);
        }

        public EffectSet Get(ushort id)
        {
            if (!_sets.TryGetValue(id, out EffectSet set))
                throw new KeyNotFoundException($"找不到 EffectSet：{id}");

            return set;
        }

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
            if (projectileCount <= 0) 
                throw new ArgumentOutOfRangeException(nameof(projectileCount));
            _shots.Add(shot.ShotId, shot);
            _references.Add(shot.ShotId, projectileCount);
        }

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
            if (projectileCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(projectileCount));
            if (!_references.TryGetValue(shotId, out int count))
                throw new KeyNotFoundException($"找不到 Shot：{shotId}");
            _references[shotId] = checked(count + projectileCount);
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
        private readonly List<ProjectileState> _projectiles = new();
        private uint _nextProjectileId = 1;

        public int Count => _projectiles.Count;

        public uint AllocateProjectileId()
        {
            uint id = _nextProjectileId;
            _nextProjectileId = unchecked(_nextProjectileId + 1u);
            if (_nextProjectileId == 0)
                _nextProjectileId = 1;

            return id;
        }

        public void Add(in ProjectileState projectile)
        {
            _projectiles.Add(projectile);
        }

        public ProjectileState Get(int index)
        {
            return _projectiles[index];
        }

        public void Set(int index, in ProjectileState projectile)
        {
            _projectiles[index] = projectile;
        }

        public void RemoveAtSwapBack(int index)
        {
            int lastIndex = _projectiles.Count - 1;
            if (index != lastIndex)
                _projectiles[index] = _projectiles[lastIndex];

            _projectiles.RemoveAt(lastIndex);
        }

        public void Clear()
        {
            _projectiles.Clear();
            _nextProjectileId = 1;
        }
    }
}
