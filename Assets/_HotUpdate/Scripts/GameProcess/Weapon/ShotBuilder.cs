using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Weapon
{
    /// <summary>
    /// 在服务器确定开枪后，生成一个具体的子弹，一定是服务器调用
    /// </summary>
    public sealed class ShotBuilder
    {
        private readonly ShotRepository _shots;
        private readonly ProjectileWorld _projectiles;
        private readonly IProjectileEffectDispatcher _effects;

        public ShotBuilder(ShotRepository shots, ProjectileWorld projectiles,
            IProjectileEffectDispatcher effects = null)
        {
            _shots = shots ?? throw new ArgumentNullException(nameof(shots));
            _projectiles = projectiles ?? throw new ArgumentNullException(nameof(projectiles));
            _effects = effects ?? NoneProjectileEffectDispatcher.Instance;
        }

        public ShotContext Build(
            ulong ownerEntityId,
            ushort weaponId,
            uint fireTick,
            uint shotSequence,
            Vector3 origin,
            Vector3 aimDirection,
            in WeaponStatSnapshot stats,
            ushort effectSetId,
            ICollection<ProjectileSpawn> spawnOutput = null)
        {
            if (!IsFinite(origin) || !IsFinite(aimDirection) || aimDirection.sqrMagnitude <= Mathf.Epsilon)
                throw new ArgumentException("枪口和瞄准方向必须是有限值，且瞄准方向不能为零。");

            ShotContext shot = new ShotContext
            {
                ShotId = _shots.AllocateShotId(),
                FireTick = fireTick,
                ShotSequence = shotSequence,
                OwnerEntityId = ownerEntityId,
                WeaponId = weaponId,
                RandomSeed = BuildSeed(ownerEntityId, shotSequence, fireTick),
                Origin = origin,
                AimDirection = aimDirection.normalized,
                StatSnapshotId = stats.Id,
                EffectSetId = effectSetId,
            };

            _shots.Register(shot, stats.ProjectileCount);
            BuildProjectiles(shot, stats, spawnOutput);
            return shot;
        }

        private void BuildProjectiles(in ShotContext shot, in WeaponStatSnapshot stats,
            ICollection<ProjectileSpawn> spawnOutput)
        {
            var random = new System.Random(unchecked((int)shot.RandomSeed));

            for (int i = 0; i < stats.ProjectileCount; i++)
            {
                //随机取出方向和暴击
                Vector3 direction = BuildSpreadDirection(shot.AimDirection, stats.SpreadAngle, random);
                bool isCritical = random.NextDouble() < stats.CritChance;

                var projectile = new ProjectileState
                {
                    ProjectileId = _projectiles.AllocateProjectileId(),
                    ShotId = shot.ShotId,
                    Position = shot.Origin,
                    Velocity = direction * stats.ProjectileSpeed,
                    RemainingLifeTime = stats.ProjectileLifeTime,
                    DamageMultiplier = isCritical ? stats.CritMultiplier : 1f,
                    SizeMultiplier = 1f,
                    PierceRemaining = stats.PierceCount,
                    BounceRemaining = stats.BounceCount,
                    Flags = isCritical ? ProjectileFlags.Critical : ProjectileFlags.None,
                };
                _effects.DispatchSpawn(ref projectile, new ProjectileSpawnEffectContext(shot, stats));
                _projectiles.Add(projectile);
                spawnOutput?.Add(new ProjectileSpawn(
                    shot, projectile, stats.ProjectileSize * projectile.SizeMultiplier));
            }
        }

        private static Vector3 BuildSpreadDirection(Vector3 baseDirection,float spreadAngle,System.Random random)
        {
            Vector3 forward = baseDirection.normalized;
            if (spreadAngle <= 0f)
                return forward;

            float angle = Mathf.Lerp(-spreadAngle * 0.5f,spreadAngle * 0.5f,(float)random.NextDouble());
            //仅涉及Y轴旋转，也就是X_Z平面旋转，基于我们的游戏特性
            return (Quaternion.AngleAxis(angle, Vector3.up) * forward).normalized;
        }
        //FNV-1a (Fowler–Noll–Vo)哈希算法
        private static uint BuildSeed(ulong ownerEntityId, uint shotSequence, uint fireTick)
        {
            unchecked
            {
                uint hash = 2166136261u;
                //64位ownerEntityId转32位Uint会被截断混入哈希池
                //取低32位算一次
                hash = (hash ^ (uint)ownerEntityId) * 16777619u;
                //>>32 取高32位再算一次，目的为将64位Ulong ID完全放进Seed的创建过程中
                hash = (hash ^ (uint)(ownerEntityId >> 32)) * 16777619u;
                hash = (hash ^ shotSequence) * 16777619u;
                hash = (hash ^ fireTick) * 16777619u;
                return hash;
            }
        }

        private static bool IsFinite(Vector3 value) =>
            !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
            !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
            !float.IsNaN(value.z) && !float.IsInfinity(value.z);
    }
}
