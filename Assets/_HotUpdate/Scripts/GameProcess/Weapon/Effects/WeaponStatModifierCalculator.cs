using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Weapon.Effects
{
    public readonly struct EffectOwnerStatSnapshot
    {
        public readonly float ShieldCapacity;
        public EffectOwnerStatSnapshot(float shieldCapacity) => ShieldCapacity = shieldCapacity;
    }
    /// <summary>
    /// 一次计算的结果包
    /// </summary>
    public readonly struct WeaponEffectCalculation
    {
        public readonly WeaponStatSnapshot WeaponStats;
        public readonly EffectOwnerStatSnapshot OwnerStats;

        public WeaponEffectCalculation(in WeaponStatSnapshot weaponStats, in EffectOwnerStatSnapshot ownerStats)
        {
            WeaponStats = weaponStats;
            OwnerStats = ownerStats;
        }
    }
    /// <summary>
    /// 表示当前权威应用对应那一份EffectSet以及其对应结果，类似一个绑定操作
    /// </summary>
    public readonly struct WeaponEffectApplication
    {
        public readonly EffectSet Effects;
        public readonly WeaponEffectCalculation Calculation;

        public WeaponEffectApplication(EffectSet effects, in WeaponEffectCalculation calculation)
        {
            Effects = effects;
            Calculation = calculation;
        }
    }

    /// <summary>
    /// 按Effect获取顺序进行数值计算，根据基础Stats与EffectSet输出最终状态
    /// </summary>
    public sealed class WeaponStatModifierCalculator
    {
        private readonly WeaponEffectCatalog _catalog;

        public WeaponStatModifierCalculator(WeaponEffectCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public WeaponEffectCalculation Calculate(
            in WeaponStatSnapshot baseStats,
            EffectSet effectSet,
            ushort statSnapshotId,
            float tickDeltaTime,
            float baseShieldCapacity = 0f)
        {
            if (effectSet == null) 
                throw new ArgumentNullException(nameof(effectSet));
            if (tickDeltaTime <= 0f || float.IsNaN(tickDeltaTime) || float.IsInfinity(tickDeltaTime))
                throw new ArgumentOutOfRangeException(nameof(tickDeltaTime));
            if (!Finite(baseShieldCapacity) || baseShieldCapacity < 0f)
                throw new ArgumentOutOfRangeException(nameof(baseShieldCapacity));

            var values = new MutableStats(baseStats, baseShieldCapacity);
            //按顺序开始Apply
            if (effectSet.AcquisitionOrder.Length > 0)
            {
                for (int i = 0; i < effectSet.AcquisitionOrder.Length; i++)
                    ApplyEffect(ref values, effectSet.AcquisitionOrder[i]);
            }
            else
            {
                //兼容旧的仅聚合EffectSet，目前这部分不走
                for (int i = 0; i < effectSet.Effects.Length; i++)
                    for (int level = 0; level < effectSet.Effects[i].Stack; level++)
                        ApplyEffect(ref values, effectSet.Effects[i].EffectId);
            }

            ValidateFinite(values);
            //从MutableStats转换到实际输出快照包
            WeaponStatSnapshot result = baseStats;
            result.Id = statSnapshotId;
            result.Damage = Mathf.Max(0f, values.Damage);
            result.FireRate = Mathf.Max(0.001f, values.FireRate);
            result.ReloadTime = Mathf.Max(0f, values.ReloadTime);
            result.MagSize = ToUShort(values.MagSize, 1, ushort.MaxValue);
            result.CritChance = Mathf.Clamp01(values.CritChance);
            result.CritMultiplier = Mathf.Max(0f, values.CritMultiplier);
            result.ProjectileSpeed = Mathf.Max(0.001f, values.ProjectileSpeed);
            result.ProjectileCount = ToUShort(values.ProjectileCount, 1, ushort.MaxValue);
            result.SpreadAngle = Mathf.Clamp(values.SpreadAngle, 0f, 180f);
            result.BounceCount = ToByte(values.BounceCount);
            result.PierceCount = ToByte(values.PierceCount);
            result.ProjectileSize = Mathf.Max(0f, values.ProjectileSize);
            result.ProjectileLifeTime = Mathf.Max(0.001f, values.ProjectileLifeTime);
            result.FireIntervalTicks = WeaponSystem.ToTicks(1f / result.FireRate, tickDeltaTime);
            result.ReloadTicks = WeaponSystem.ToTicks(result.ReloadTime, tickDeltaTime);

            WeaponStatSnapshotRepository.Validate(in result);
            //应用护盾到玩家，这是归属玩家的状态
            var ownerStats = new EffectOwnerStatSnapshot(Mathf.Max(0f, values.ShieldCapacity));
            return new WeaponEffectCalculation(in result, in ownerStats);
        }
        /// <summary>
        /// 根据ID找到Effect，依次触发他内部存的数值修改ID
        /// </summary>
        private void ApplyEffect(ref MutableStats values, ushort effectId)
        {
            WeaponEffectDefinition effect = _catalog.GetEffect(effectId);
            for (int i = 0; i < effect.ModifierIds.Length; i++)
            {
                WeaponModifierDefinition modifier = _catalog.GetModifier(effect.ModifierIds[i]);
                ApplyModifier(ref values, modifier);
            }
        }
        /// <summary>
        /// 自己取加和乘算对应数值，使用简单Switch完成
        /// </summary>
        private static void ApplyModifier(ref MutableStats values, WeaponModifierDefinition modifier)
        {
            float Apply(float current) => modifier.Operation == WeaponModifierOperation.Add
                ? current + modifier.BaseValue : current * modifier.BaseValue;

            switch (modifier.Stat)
            {
                case WeaponModifierStat.Damage: values.Damage = Apply(values.Damage); break;
                case WeaponModifierStat.FireRate: values.FireRate = Apply(values.FireRate); break;
                case WeaponModifierStat.ReloadTime: values.ReloadTime = Apply(values.ReloadTime); break;
                case WeaponModifierStat.MagSize: values.MagSize = Apply(values.MagSize); break;
                case WeaponModifierStat.CritChance: values.CritChance = Apply(values.CritChance); break;
                case WeaponModifierStat.CritMultiplier: values.CritMultiplier = Apply(values.CritMultiplier); break;
                case WeaponModifierStat.ProjectileSpeed: values.ProjectileSpeed = Apply(values.ProjectileSpeed); break;
                case WeaponModifierStat.ProjectileCount: values.ProjectileCount = Apply(values.ProjectileCount); break;
                case WeaponModifierStat.SpreadAngle: values.SpreadAngle = Apply(values.SpreadAngle); break;
                case WeaponModifierStat.BounceCount: values.BounceCount = Apply(values.BounceCount); break;
                case WeaponModifierStat.PierceCount: values.PierceCount = Apply(values.PierceCount); break;
                case WeaponModifierStat.ProjectileSize: values.ProjectileSize = Apply(values.ProjectileSize); break;
                case WeaponModifierStat.ProjectileLifeTime: values.ProjectileLifeTime = Apply(values.ProjectileLifeTime); break;
                case WeaponModifierStat.ShieldCapacity: values.ShieldCapacity = Apply(values.ShieldCapacity); break;
                default: throw new ArgumentOutOfRangeException(nameof(modifier.Stat), modifier.Stat, null);
            }
        }

        private static ushort ToUShort(float value, int min, int max) => (ushort)Mathf.Clamp(Mathf.RoundToInt(value), min, max);

        private static byte ToByte(float value) => (byte)Mathf.Clamp(Mathf.RoundToInt(value), 0, byte.MaxValue);

        private static void ValidateFinite(in MutableStats value)
        {
            if (!Finite(value.Damage) || !Finite(value.FireRate) || !Finite(value.ReloadTime) ||
                !Finite(value.MagSize) || !Finite(value.CritChance) || !Finite(value.CritMultiplier) ||
                !Finite(value.ProjectileSpeed) || !Finite(value.ProjectileCount) || !Finite(value.SpreadAngle) ||
                !Finite(value.BounceCount) || !Finite(value.PierceCount) || !Finite(value.ProjectileSize) ||
                !Finite(value.ProjectileLifeTime) || !Finite(value.ShieldCapacity))
                throw new InvalidOperationException("Effect Modifier 计算产生了 NaN 或 Infinity。");
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        /// <summary>
        /// 取整Stat，先全部用Float表示，算完取整
        /// </summary>
        private struct MutableStats
        {
            public float Damage;
            public float FireRate;
            public float ReloadTime;
            public float MagSize;
            public float CritChance;
            public float CritMultiplier;
            public float ProjectileSpeed;
            public float ProjectileCount;
            public float SpreadAngle;
            public float BounceCount;
            public float PierceCount;
            public float ProjectileSize;
            public float ProjectileLifeTime;
            public float ShieldCapacity;

            public MutableStats(in WeaponStatSnapshot stats, float shieldCapacity)
            {
                Damage = stats.Damage;
                FireRate = stats.FireRate;
                ReloadTime = stats.ReloadTime;
                MagSize = stats.MagSize;
                CritChance = stats.CritChance;
                CritMultiplier = stats.CritMultiplier;
                ProjectileSpeed = stats.ProjectileSpeed;
                ProjectileCount = stats.ProjectileCount;
                SpreadAngle = stats.SpreadAngle;
                BounceCount = stats.BounceCount;
                PierceCount = stats.PierceCount;
                ProjectileSize = stats.ProjectileSize;
                ProjectileLifeTime = stats.ProjectileLifeTime;
                ShieldCapacity = shieldCapacity;
            }
        }
    }
}
