using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Config;

namespace ProjectGame.HotFix.Gameplay.Weapon.Effects
{
    /// <summary>
    /// buff类型，普通修改数值的，特殊会增加新能力
    /// </summary>
    public enum WeaponEffectType : byte 
    {
        Numeric = 0, 
        Special = 1 
    }
    /// <summary>
    /// 区分普通和特殊抽取池
    /// </summary>
    public enum WeaponEffectRollPool : byte 
    {
        Standard = 0, 
        Mutation = 1 
    }
    /// <summary>
    /// 配表数值调整选择项，加或乘运算
    /// </summary>
    public enum WeaponModifierOperation : byte 
    {
        Add = 0, 
        Multiply = 1 
    }
    /// <summary>
    /// 配表可修改的数据枚举
    /// </summary>
    public enum WeaponModifierStat : byte
    {
        Damage = 0,
        FireRate = 1,
        ReloadTime = 2,
        MagSize = 3,
        CritChance = 4,
        CritMultiplier = 5,
        ProjectileSpeed = 6,
        ProjectileCount = 7,
        SpreadAngle = 8,
        BounceCount = 9,
        PierceCount = 10,
        ProjectileSize = 11,
        ProjectileLifeTime = 12,
        ShieldCapacity = 13,
    }
    /// <summary>
    /// 一行Modifier数据的定义，跟配表结构对应
    /// 本质上为配置对象，所有人读取时都使用同一份，所以更贴合类的性质
    /// </summary>
    public sealed class WeaponModifierDefinition
    {
        public ushort Id { get; }
        public string CodeName { get; }
        public WeaponModifierStat Stat { get; }
        public WeaponModifierOperation Operation { get; }
        public float BaseValue { get; }

        internal WeaponModifierDefinition(Config_Modifier row)
        {
            Id = checked((ushort)row.ModifierID);
            CodeName = row.CodeName;
            Stat = (WeaponModifierStat)row.StatType;
            Operation = (WeaponModifierOperation)row.Operation;
            BaseValue = row.BaseValue;
        }
    }
    /// <summary>
    /// 一行具体Effect效果的定义，与配表强对应
    /// </summary>
    public sealed class WeaponEffectDefinition
    {
        public ushort Id { get; }
        public string CodeName { get; }
        public ushort[] ModifierIds { get; }
        public bool Repeatable { get; }
        public byte MaxLevel { get; }
        public WeaponEffectType Type { get; }
        public WeaponEffectRollPool RollPool { get; }
        public ushort[] SchoolIds { get; }
        public ushort[] ConflictSchoolIds { get; }
        public float BaseWeight { get; }
        public float SchoolWeightBonus { get; }

        internal WeaponEffectDefinition(Config_Effect row)
        {
            Id = checked((ushort)row.EffectID);
            CodeName = row.CodeName;
            ModifierIds = CopyIds(row.ModifierIDs);
            Repeatable = row.Repeatable;
            MaxLevel = checked((byte)row.MaxLevel);
            Type = (WeaponEffectType)row.EffectType;
            RollPool = (WeaponEffectRollPool)row.RollPool;
            SchoolIds = CopyIds(row.SchoolIDs);
            ConflictSchoolIds = CopyIds(row.ConflictSchoolIDs);
            BaseWeight = row.BaseWeight;
            SchoolWeightBonus = row.SchoolWeightBonus;
        }

        private static ushort[] CopyIds(int[] values)
        {
            if (values == null || values.Length == 0) 
                return Array.Empty<ushort>();
            var result = new ushort[values.Length];
            for (int i = 0; i < values.Length; i++) 
                result[i] = checked((ushort)values[i]);
            return result;
        }
    }
    /// <summary>
    /// 一行Effect的UI展示数据定义，与配表强对应
    /// </summary>
    public sealed class WeaponEffectRollDefinition
    {
        public ushort EffectId { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public string UpgradeDescription { get; }
        public string IconAddress { get; }

        internal WeaponEffectRollDefinition(Config_EffectRoll row)
        {
            EffectId = checked((ushort)row.EffectID);
            DisplayName = row.DisplayName;
            Description = row.Description;
            UpgradeDescription = row.UpgradeDescription;
            IconAddress = row.IconAddress;
        }
    }

    /// <summary>
    /// 在这里定死ConfigManager中拿到的Effect信息，业务层不持有可变配置行
    /// 业务层通过四个Get方法获取数据
    /// </summary>
    public sealed class WeaponEffectCatalog
    {
        private readonly Dictionary<ushort, WeaponEffectDefinition> _effects = new();
        private readonly Dictionary<ushort, WeaponModifierDefinition> _modifiers = new();
        private readonly Dictionary<ushort, WeaponEffectRollDefinition> _rolls = new();

        public IEnumerable<WeaponEffectDefinition> Effects => _effects.Values;

        public WeaponEffectCatalog(
            IDictionary<int, Config_Effect> effects,
            IDictionary<int, Config_Modifier> modifiers,
            IDictionary<int, Config_EffectRoll> rolls)
        {
            EffectConfigRules.ValidateAll(effects, modifiers, rolls);

            foreach (KeyValuePair<int, Config_Modifier> pair in modifiers)
                if (pair.Value.Enabled)
                    _modifiers.Add(checked((ushort)pair.Key), new WeaponModifierDefinition(pair.Value));

            foreach (KeyValuePair<int, Config_EffectRoll> pair in rolls)
                if (pair.Value.Enabled)
                    _rolls.Add(checked((ushort)pair.Key), new WeaponEffectRollDefinition(pair.Value));

            foreach (KeyValuePair<int, Config_Effect> pair in effects)
                if (pair.Value.Enabled)
                    _effects.Add(checked((ushort)pair.Key), new WeaponEffectDefinition(pair.Value));

            if (_effects.Count == 0) 
                throw new InvalidOperationException("Effect 表没有启用项。");
        }

        public bool TryGetEffect(ushort id, out WeaponEffectDefinition definition) =>
            _effects.TryGetValue(id, out definition);

        public WeaponEffectDefinition GetEffect(ushort id) => _effects.TryGetValue(id, out var definition)
            ? definition : throw new KeyNotFoundException($"EffectID={id} 未配置或未启用。");

        public WeaponModifierDefinition GetModifier(ushort id) => _modifiers.TryGetValue(id, out var definition)
            ? definition : throw new KeyNotFoundException($"ModifierID={id} 未配置或未启用。");

        public WeaponEffectRollDefinition GetRoll(ushort effectId) => _rolls.TryGetValue(effectId, out var definition)
            ? definition : throw new KeyNotFoundException($"EffectRoll EffectID={effectId} 未配置或未启用。");
    }
}
