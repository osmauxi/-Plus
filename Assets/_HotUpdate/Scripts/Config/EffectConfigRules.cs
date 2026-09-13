using System;
using System.Collections.Generic;

namespace ProjectGame.HotFix.Config
{
    /// <summary>Effect 三表的字段和外键约束。配置导出做逐行校验，运行时目录构建再做跨表校验。</summary>
    public static class EffectConfigRules
    {
        public const int MaxStatType = 13;
        public const int MaxEffectType = 1;
        public const int MaxRollPool = 1;
        public const int MaxOperation = 1;

        public static void ValidateEffect(Config_Effect row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            ValidateRuntimeId(row.EffectID, nameof(row.EffectID));
            if (string.IsNullOrWhiteSpace(row.CodeName))
                throw new ArgumentException($"EffectID={row.EffectID} 缺少 CodeName。");
            if (row.MaxLevel < 1 || row.MaxLevel > byte.MaxValue)
                throw new ArgumentException($"EffectID={row.EffectID} MaxLevel 必须位于 1~{byte.MaxValue}。");
            if (!row.Repeatable && row.MaxLevel != 1)
                throw new ArgumentException($"EffectID={row.EffectID} 不可重复时 MaxLevel 必须为 1。");
            if (row.EffectType < 0 || row.EffectType > MaxEffectType)
                throw new ArgumentException($"EffectID={row.EffectID} EffectType 非法。");
            if (row.RollPool < 0 || row.RollPool > MaxRollPool)
                throw new ArgumentException($"EffectID={row.EffectID} RollPool 非法。");
            if (!FiniteNonNegative(row.BaseWeight) || !FiniteNonNegative(row.SchoolWeightBonus))
                throw new ArgumentException($"EffectID={row.EffectID} 抽取权重必须是非负有限值。");
            if (row.Enabled && row.BaseWeight <= 0f)
                throw new ArgumentException($"EffectID={row.EffectID} 已启用但 BaseWeight 不大于 0。");

            ValidateIdArray(row.ModifierIDs, "ModifierIDs", row.EffectID);
            ValidateIdArray(row.SchoolIDs, "SchoolIDs", row.EffectID);
            ValidateIdArray(row.ConflictSchoolIDs, "ConflictSchoolIDs", row.EffectID);
            if (row.Enabled && row.EffectType == 0 && (row.ModifierIDs == null || row.ModifierIDs.Length == 0))
                throw new ArgumentException($"EffectID={row.EffectID} 是普通数值效果但没有 ModifierIDs。");
            if (Intersects(row.SchoolIDs, row.ConflictSchoolIDs))
                throw new ArgumentException($"EffectID={row.EffectID} 所属流派不能同时出现在冲突流派中。");
        }

        public static void ValidateModifier(Config_Modifier row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            ValidateRuntimeId(row.ModifierID, nameof(row.ModifierID));
            if (string.IsNullOrWhiteSpace(row.CodeName))
                throw new ArgumentException($"ModifierID={row.ModifierID} 缺少 CodeName。");
            if (row.StatType < 0 || row.StatType > MaxStatType)
                throw new ArgumentException($"ModifierID={row.ModifierID} StatType 非法。");
            if (row.Operation < 0 || row.Operation > MaxOperation)
                throw new ArgumentException($"ModifierID={row.ModifierID} Operation 非法。");
            if (!Finite(row.BaseValue) || (row.Operation == 1 && row.BaseValue <= 0f))
                throw new ArgumentException($"ModifierID={row.ModifierID} BaseValue 非法；Multiply 必须大于 0。");
        }

        public static void ValidateRoll(Config_EffectRoll row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            ValidateRuntimeId(row.EffectID, nameof(row.EffectID));
            if (string.IsNullOrWhiteSpace(row.DisplayName) || string.IsNullOrWhiteSpace(row.Description) ||
                string.IsNullOrWhiteSpace(row.UpgradeDescription) || string.IsNullOrWhiteSpace(row.IconAddress))
                throw new ArgumentException($"EffectRoll EffectID={row.EffectID} 存在空显示字段或图标地址。");
        }

        public static void ValidateAll(
            IDictionary<int, Config_Effect> effects,
            IDictionary<int, Config_Modifier> modifiers,
            IDictionary<int, Config_EffectRoll> rolls)
        {
            if (effects == null || modifiers == null || rolls == null)
                throw new InvalidOperationException("Effect、Modifier、EffectRoll 必须全部加载后才能构建 Effect 目录。");

            var effectNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (KeyValuePair<int, Config_Effect> pair in effects)
            {
                ValidateEffect(pair.Value);
                if (pair.Key != pair.Value.EffectID)
                    throw new InvalidOperationException($"Effect 表键 {pair.Key} 与 EffectID={pair.Value.EffectID} 不一致。");
                if (!effectNames.Add(pair.Value.CodeName))
                    throw new InvalidOperationException($"Effect CodeName 重复：{pair.Value.CodeName}。");

                if (!pair.Value.Enabled) continue;
                if (!rolls.TryGetValue(pair.Key, out Config_EffectRoll roll) || !roll.Enabled)
                    throw new InvalidOperationException($"启用的 EffectID={pair.Key} 缺少启用的 EffectRoll 行。");
                if (pair.Value.ModifierIDs == null) continue;
                foreach (int modifierId in pair.Value.ModifierIDs)
                {
                    if (!modifiers.TryGetValue(modifierId, out Config_Modifier modifier) || !modifier.Enabled)
                        throw new InvalidOperationException($"EffectID={pair.Key} 引用了缺失或禁用的 ModifierID={modifierId}。");
                }
            }

            var modifierNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (KeyValuePair<int, Config_Modifier> pair in modifiers)
            {
                ValidateModifier(pair.Value);
                if (pair.Key != pair.Value.ModifierID)
                    throw new InvalidOperationException($"Modifier 表键 {pair.Key} 与 ModifierID={pair.Value.ModifierID} 不一致。");
                if (!modifierNames.Add(pair.Value.CodeName))
                    throw new InvalidOperationException($"Modifier CodeName 重复：{pair.Value.CodeName}。");
            }

            foreach (KeyValuePair<int, Config_EffectRoll> pair in rolls)
            {
                ValidateRoll(pair.Value);
                if (pair.Key != pair.Value.EffectID)
                    throw new InvalidOperationException($"EffectRoll 表键 {pair.Key} 与 EffectID={pair.Value.EffectID} 不一致。");
                if (!effects.ContainsKey(pair.Key))
                    throw new InvalidOperationException($"EffectRoll EffectID={pair.Key} 在 Effect 表中不存在。");
            }
        }

        private static void ValidateRuntimeId(int id, string field)
        {
            if (id <= 0 || id > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(field, $"{field} 必须位于 1~{ushort.MaxValue}。");
        }

        private static void ValidateIdArray(int[] values, string field, int ownerId)
        {
            if (values == null) return;
            var unique = new HashSet<int>();
            foreach (int value in values)
            {
                ValidateRuntimeId(value, field);
                if (!unique.Add(value))
                    throw new ArgumentException($"EffectID={ownerId} 的 {field} 包含重复 ID={value}。");
            }
        }

        private static bool Intersects(int[] left, int[] right)
        {
            if (left == null || right == null || left.Length == 0 || right.Length == 0) return false;
            var values = new HashSet<int>(left);
            foreach (int value in right)
                if (values.Contains(value)) return true;
            return false;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static bool FiniteNonNegative(float value) => value >= 0f && Finite(value);
    }
}
