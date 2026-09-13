using System;
using System.Linq;

namespace ProjectGame.HotFix.Config
{
    /// <summary>
    /// 配置层公共规则,Lobby无需反向依赖Gameplay
    /// Config层的武器配置合法性规则
    /// </summary>
    public static class WeaponConfigRules
    {
        /// <summary>
        /// 确认当前武器在Lobby中存在ID对应
        /// </summary>
        public static bool IsSelectable(int id)
        {
            var weapons = ConfigManager.Instance.GetTable<Config_Weapon>();
            var lobby = ConfigManager.Instance.GetTable<Config_Lobby_Weapons>();
            return weapons != null && lobby != null && lobby.ContainsKey(id) &&
                   weapons.TryGetValue(id, out var row) && row.Enabled;
        }

        public static int DefaultWeaponId()
        {
            var table = ConfigManager.Instance.GetTable<Config_Weapon>();
            if (table == null) throw new InvalidOperationException("Weapon 配置尚未加载。");

            for (int id = 0; id < table.Count; id++)
                if (IsSelectable(id)) return id;

            throw new InvalidOperationException("没有已启用且具有 Lobby 资源记录的武器。");
        }

        public static void Validate(Config_Weapon row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            if (row.WeaponID < 0 || row.WeaponID >= ushort.MaxValue)
                throw new ArgumentException("WeaponID 必须在 0~65534；快照 ID=WeaponID+1，0 留给未装备。");
            if (!row.Enabled) 
                return;
            bool finite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
            bool positive(float v) => finite(v) && v > 0;
            bool nonnegative(float v) => finite(v) && v >= 0;
            if (!positive(row.Damage) || !positive(row.FireRate) || !positive(row.ReloadTime) ||
                row.MagSize < 1 || row.MagSize > ushort.MaxValue || row.ReserveAmmo < 0 ||
                row.ReserveAmmo > ushort.MaxValue || !positive(row.ProjectileSpeed) ||
                !positive(row.ProjectileLifeTime) || !positive(row.ProjectileSize) ||
                row.ProjectileCount < 1 || row.ProjectileCount > 64 ||
                !nonnegative(row.CritChance) || row.CritChance > 1 ||
                !finite(row.CritMultiplier) || row.CritMultiplier < 1 ||
                !nonnegative(row.SpreadAngle) || row.SpreadAngle > 180 ||
                row.BounceCount < 0 || row.BounceCount > byte.MaxValue ||
                row.PierceCount < 0 || row.PierceCount > byte.MaxValue)
                throw new ArgumentException($"WeaponID={row.WeaponID} 参数非法，请检查伤害、射速、弹药、散布和弹丸字段。");
        }
    }
}
