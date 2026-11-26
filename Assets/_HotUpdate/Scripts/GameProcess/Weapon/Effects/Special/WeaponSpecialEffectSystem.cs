using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Weapon.Effects.Special
{
    public readonly struct WeaponEffectEquipContext
    {
        public readonly ulong OwnerEntityId;
        public readonly EffectSet Effects;
        public readonly WeaponStatSnapshot Stats;
        public WeaponEffectEquipContext(ulong ownerEntityId, EffectSet effects,
            in WeaponStatSnapshot stats)
        {
            OwnerEntityId = ownerEntityId;
            Effects = effects;
            Stats = stats;
        }
    }

    public readonly struct WeaponEffectFireContext
    {
        public readonly ulong OwnerEntityId;
        public readonly uint Tick;
        public readonly ushort WeaponId;
        public readonly WeaponStatSnapshot Stats;
        public readonly EffectSet Effects;

        public WeaponEffectFireContext(ulong ownerEntityId, uint tick, ushort weaponId,
            in WeaponStatSnapshot stats, EffectSet effects)
        {
            OwnerEntityId = ownerEntityId;
            Tick = tick;
            WeaponId = weaponId;
            Stats = stats;
            Effects = effects;
        }
    }

    public readonly struct LightningEffectCommand
    {
        public readonly ulong TargetEntityId;
        public readonly ushort EffectSetId;
        public readonly Vector3 Origin;
        public readonly float Damage;
        public readonly byte JumpCount;
        public readonly byte BranchCount;
        public readonly float SearchRadius;
        public readonly bool ApplyInitialDamage;

        public LightningEffectCommand(in ProjectileHitContext context, float damage, byte jumpCount,
            byte branchCount = 3, float searchRadius = 5f, bool applyInitialDamage = true)
        {
            TargetEntityId = context.TargetEntityId;
            EffectSetId = context.Shot.EffectSetId;
            Origin = context.HitPoint;
            Damage = damage;
            JumpCount = jumpCount;
            BranchCount = branchCount;
            SearchRadius = searchRadius;
            ApplyInitialDamage = applyInitialDamage;
        }
    }

    public readonly struct StormCloudEffectCommand
    {
        public readonly ulong OwnerEntityId;
        public readonly float Radius;
        public readonly float StrikeDamage;
        public readonly float StrikeInterval;
        public readonly float OverloadDamage;
        public readonly byte OverloadJumpCount;
        public readonly float ExecuteThreshold;

        public StormCloudEffectCommand(in WeaponEffectEquipContext context, float radius,
            float strikeDamage, float strikeInterval, float overloadDamage,
            byte overloadJumpCount)
        {
            OwnerEntityId = context.OwnerEntityId;
            Radius = radius;
            StrikeDamage = strikeDamage;
            StrikeInterval = strikeInterval;
            OverloadDamage = overloadDamage;
            OverloadJumpCount = overloadJumpCount;
            ExecuteThreshold = ExecutionerEffectSystem.GetThreshold(context.Effects);
        }
    }

    public readonly struct ShieldEffectCommand
    {
        public readonly ulong OwnerEntityId;
        public readonly float Amount;

        public ShieldEffectCommand(in ProjectileHitContext context, float amount)
        {
            OwnerEntityId = context.Shot.OwnerEntityId;
            Amount = amount;
        }
    }

    public readonly struct ShieldCapacityEffectCommand
    {
        public readonly ushort EffectId;
        public readonly ulong OwnerEntityId;
        public readonly float CapacityContribution;

        public ShieldCapacityEffectCommand(ushort effectId, ulong ownerEntityId,
            float capacityContribution)
        {
            EffectId = effectId;
            OwnerEntityId = ownerEntityId;
            CapacityContribution = capacityContribution;
        }
    }

    public readonly struct ExecuteEffectCommand
    {
        public readonly ulong TargetEntityId;
        public readonly float HealthThreshold;

        public ExecuteEffectCommand(in ProjectileHitContext context, float healthThreshold)
        {
            TargetEntityId = context.TargetEntityId;
            HealthThreshold = healthThreshold;
        }
    }

    public readonly struct CrowdControlEffectCommand
    {
        public readonly ulong TargetEntityId;
        public readonly float Duration;

        public CrowdControlEffectCommand(in ProjectileHitContext context, float duration)
        {
            TargetEntityId = context.TargetEntityId;
            Duration = duration;
        }
    }

    public readonly struct SplitProjectileEffectCommand
    {
        public readonly ProjectileState Parent;
        public readonly ProjectileHitContext Context;
        public readonly ushort SplitCount;
        public readonly float SpreadAngle;
        public readonly float DamageRatio;

        public SplitProjectileEffectCommand(in ProjectileState parent,
            in ProjectileHitContext context, ushort splitCount,
            float spreadAngle, float damageRatio)
        {
            Parent = parent;
            Context = context;
            SplitCount = splitCount;
            SpreadAngle = spreadAngle;
            DamageRatio = damageRatio;
        }
    }

    public readonly struct RadialImpactEffectCommand
    {
        public readonly ushort EffectSetId;
        public readonly Vector3 Position;
        public readonly float Radius;
        public readonly float Damage;
        public readonly float Force;

        public RadialImpactEffectCommand(in ProjectileDestroyedEffectContext context,
            in ProjectileState projectile,
            float radius, float damage, float force)
        {
            EffectSetId = context.Shot.EffectSetId;
            Position = projectile.Position;
            Radius = radius;
            Damage = damage;
            Force = force;
        }
    }

    public readonly struct PersistentAreaEffectCommand
    {
        public readonly ulong SourceEntityId;
        public readonly ulong ShotId;
        public readonly Vector3 Position;
        public readonly float Radius;
        public readonly float Duration;
        public readonly float TickInterval;
        public readonly float DamagePerTick;
        public readonly float SlowRatio;

        public PersistentAreaEffectCommand(in ProjectileHitContext context, float radius,
            float duration, float tickInterval, float damagePerTick, float slowRatio)
        {
            SourceEntityId = context.Shot.OwnerEntityId;
            ShotId = context.Shot.ShotId;
            Position = context.HitPoint;
            Radius = radius;
            Duration = duration;
            TickInterval = tickInterval;
            DamagePerTick = damagePerTick;
            SlowRatio = slowRatio;
        }
    }

    /// <summary>特殊脚本只提交玩法命令，不直接依赖 Health、对象池、网络或场景单例。</summary>
    public interface IWeaponSpecialEffectCommandSink
    {
        void EmitLightning(in LightningEffectCommand command);
        void SpawnStormCloud(in StormCloudEffectCommand command);
        void RemoveStormCloud(ulong ownerEntityId);
        void AddShield(in ShieldEffectCommand command);
        void SetShieldCapacity(in ShieldCapacityEffectCommand command);
        void TryExecute(in ExecuteEffectCommand command);
        void ApplyCrowdControl(in CrowdControlEffectCommand command);
        void SpawnProjectiles(in SplitProjectileEffectCommand command);
        void EmitRadialImpact(in RadialImpactEffectCommand command);
        void TrySpawnDamageArea(in PersistentAreaEffectCommand command);
    }

    public abstract class WeaponSpecialEffectSystem
    {
        public abstract ushort EffectId { get; }
        protected IWeaponSpecialEffectCommandSink Commands { get; }

        protected WeaponSpecialEffectSystem(IWeaponSpecialEffectCommandSink commands)
        {
            Commands = commands;
        }

        public virtual void OnEquipped(byte level, in WeaponEffectEquipContext context) { }
        public virtual void OnUnequipped(ulong ownerEntityId) { }
        public virtual void BeforeFire(byte level, in WeaponEffectFireContext context) { }
        public virtual void AfterFire(byte level, in ShotContext shot, in WeaponStatSnapshot stats) { }
        public virtual void OnProjectileSpawn(byte level, ref ProjectileState projectile,
            in ProjectileSpawnEffectContext context) { }
        public virtual void OnProjectileHit(byte level, ref ProjectileState projectile,
            in ProjectileHitContext context) { }
        public virtual void OnProjectileBounce(byte level, ref ProjectileState projectile,
            in ShotContext shot, in WeaponStatSnapshot stats, Vector3 hitPoint, Vector3 hitNormal) { }
        public virtual void OnProjectileDestroyed(byte level, ref ProjectileState projectile,
            in ProjectileDestroyedEffectContext context) { }
    }

    /// <summary>类 ECS 调度器：EffectSet 是数据，按 ID 注册的纯 C# System 是行为。</summary>
    public sealed class WeaponSpecialEffectManager : IProjectileEffectDispatcher
    {
        private readonly EffectSetRepository _effectSets;
        private readonly Dictionary<ushort, WeaponSpecialEffectSystem> _systems = new();
        private readonly Dictionary<ulong, HashSet<ushort>> _ownerSpecialEffects = new();

        public WeaponSpecialEffectManager(EffectSetRepository effectSets)
        {
            _effectSets = effectSets;
        }

        public void Register(WeaponSpecialEffectSystem system) => _systems.Add(system.EffectId, system);

        public void DispatchOwnerState(ulong ownerEntityId, EffectSet set,
            in WeaponStatSnapshot stats)
        {
            var context = new WeaponEffectEquipContext(ownerEntityId, set, stats);
            var current = new HashSet<ushort>();
            for (int i = 0; i < set.Effects.Length; i++)
            {
                EffectSnapshot snapshot = set.Effects[i];
                if (!_systems.TryGetValue(snapshot.EffectId, out WeaponSpecialEffectSystem system))
                    continue;
                current.Add(snapshot.EffectId);
                system.OnEquipped(snapshot.Stack, context);
            }

            if (_ownerSpecialEffects.TryGetValue(ownerEntityId, out HashSet<ushort> previous))
                foreach (ushort effectId in previous)
                    if (!current.Contains(effectId))
                        _systems[effectId].OnUnequipped(ownerEntityId);

            if (current.Count == 0) _ownerSpecialEffects.Remove(ownerEntityId);
            else _ownerSpecialEffects[ownerEntityId] = current;
        }

        public void RemoveOwner(ulong ownerEntityId)
        {
            if (!_ownerSpecialEffects.TryGetValue(ownerEntityId, out HashSet<ushort> effects)) return;
            foreach (ushort effectId in effects)
                _systems[effectId].OnUnequipped(ownerEntityId);
            _ownerSpecialEffects.Remove(ownerEntityId);
        }

        public void ClearOwners()
        {
            foreach (KeyValuePair<ulong, HashSet<ushort>> owner in _ownerSpecialEffects)
                foreach (ushort effectId in owner.Value)
                    _systems[effectId].OnUnequipped(owner.Key);
            _ownerSpecialEffects.Clear();
        }

        public void DispatchBeforeFire(in WeaponEffectFireContext context)
        {
            for (int i = 0; i < context.Effects.Effects.Length; i++)
            {
                EffectSnapshot effect = context.Effects.Effects[i];
                if (_systems.TryGetValue(effect.EffectId, out WeaponSpecialEffectSystem system))
                    system.BeforeFire(effect.Stack, context);
            }
        }

        public void DispatchAfterFire(in ShotContext shot, in WeaponStatSnapshot stats)
        {
            EffectSet set = _effectSets.Get(shot.EffectSetId);
            for (int i = 0; i < set.Effects.Length; i++)
            {
                EffectSnapshot effect = set.Effects[i];
                if (_systems.TryGetValue(effect.EffectId, out WeaponSpecialEffectSystem system))
                    system.AfterFire(effect.Stack, shot, stats);
            }
        }

        public void DispatchSpawn(ref ProjectileState projectile, in ProjectileSpawnEffectContext context)
        {
            EffectSet set = _effectSets.Get(context.Shot.EffectSetId);
            for (int i = 0; i < set.Effects.Length; i++)
            {
                EffectSnapshot effect = set.Effects[i];
                if (_systems.TryGetValue(effect.EffectId, out WeaponSpecialEffectSystem system))
                    system.OnProjectileSpawn(effect.Stack, ref projectile, context);
            }
        }

        public void DispatchHit(ref ProjectileState projectile, in ProjectileHitContext context)
        {
            EffectSet set = _effectSets.Get(context.Shot.EffectSetId);
            for (int i = 0; i < set.Effects.Length; i++)
            {
                EffectSnapshot effect = set.Effects[i];
                if (_systems.TryGetValue(effect.EffectId, out WeaponSpecialEffectSystem system))
                    system.OnProjectileHit(effect.Stack, ref projectile, context);
            }
        }

        public void DispatchBounce(ref ProjectileState projectile, in ShotContext shot,
            in WeaponStatSnapshot stats, Vector3 hitPoint, Vector3 hitNormal)
        {
            EffectSet set = _effectSets.Get(shot.EffectSetId);
            for (int i = 0; i < set.Effects.Length; i++)
            {
                EffectSnapshot effect = set.Effects[i];
                if (_systems.TryGetValue(effect.EffectId, out WeaponSpecialEffectSystem system))
                    system.OnProjectileBounce(effect.Stack, ref projectile,
                        shot, stats, hitPoint, hitNormal);
            }
        }

        public void DispatchDestroyed(ref ProjectileState projectile, in ProjectileDestroyedEffectContext context)
        {
            EffectSet set = _effectSets.Get(context.Shot.EffectSetId);
            for (int i = 0; i < set.Effects.Length; i++)
            {
                EffectSnapshot effect = set.Effects[i];
                if (_systems.TryGetValue(effect.EffectId, out WeaponSpecialEffectSystem system))
                    system.OnProjectileDestroyed(effect.Stack, ref projectile, context);
            }
        }
    }
}
