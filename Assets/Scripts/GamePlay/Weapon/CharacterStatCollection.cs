using System.Collections.Generic;
using UnityEngine;

//明确定义你的游戏里有哪些属性类型
public enum StatType
{
    // --- 枪械基础输出 ---
    Damage,             // 基础伤害
    FireRate,           // 射速 (发/秒)
    ReloadTime,         // 换弹时间 (秒)
    MagSize,            // 弹匣容量
    CritChance,         // 暴击率 (0~1)
    CritDamage,         // 暴击伤害倍率 (默认 1.5)

    // --- 子弹形态与物理轨迹 ---
    ProjectileSpeed,    // 子弹飞行速度
    ProjectileCount,    // 弹片数量 (例如霰弹枪默认5，拿到词条+1变6)
    SpreadAngle,        // 散布角度 (精准度，越小越准)
    BounceCount,        // 弹射次数 (撞墙反弹)
    PierceCount,        // 穿透次数 (穿透敌人)

    // --- 角色生存与机动 (商店道具) ---
    MaxHealth,          // 最大生命值
    Armor,              // 护甲减伤
    MoveSpeed,          // 移动速度
    DodgeChance,   // 闪避率

    ProjectileSize,     // 子弹体积缩放倍率 (默认 1)
}

public class CharacterStatCollection : MonoBehaviour
{
    // 使用字典显式映射
    public Dictionary<StatType, Stat> Stats { get; private set; }

    public CharacterStatCollection()
    {
        Stats = new Dictionary<StatType, Stat>
        {
            { StatType.Damage, new Stat(10f,1f) },
            { StatType.FireRate, new Stat(5f, 1f) },
            { StatType.ReloadTime, new Stat(2f,0.1f,10f) },
            { StatType.MagSize, new Stat(30f,1f) },
            { StatType.CritChance, new Stat(0.05f,0f,1f) },
            { StatType.CritDamage, new Stat(1.5f,0f) },  

            // 子弹物理默认值
            { StatType.ProjectileSpeed, new Stat(25f,25f,100f) }, // 默认子弹速度
            { StatType.ProjectileCount, new Stat(1f) },  // 默认单发
            { StatType.SpreadAngle, new Stat(0f,0f,90f) },      // 默认指哪打哪
            { StatType.BounceCount, new Stat(0f) },      // 默认不弹射
            { StatType.PierceCount, new Stat(0f) },      // 默认不穿透
            { StatType.ProjectileSize, new Stat(1f, 0.1f, 10f) },

            // 角色生存默认值
            { StatType.MaxHealth, new Stat(100f) },
            { StatType.Armor, new Stat(0f) },
            { StatType.MoveSpeed, new Stat(8f,1f,25f) },
            { StatType.DodgeChance, new Stat(0f,0f,0.75f) }
        };
    }

    public Stat GetStat(StatType type)
    {
        if (Stats.TryGetValue(type, out Stat stat))
        {
            return stat;
        }
        return null;
    }
    public float GetStatValue(StatType type)
    {
        return GetStat(type)?.Value ?? 0f;
    }
    // 快捷添加词条的方法
    public void AddModifier(StatType type, float value, StatModType modType, object source)
    {
        GetStat(type)?.AddModifier(new StatModifier(value, modType, source));
    }

    // 快捷移除某个来源的所有词条
    public void RemoveAllModifiersFromSource(object source)
    {
        foreach (var stat in Stats.Values)
        {
            stat.RemoveAllModifiersFromSource(source);
        }
    }
}