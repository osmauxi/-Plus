using System;
using MessagePack;
using System.Collections.Generic;
using UnityEngine;

[MessagePackObject]
public class Config_Weapon
{
    /// <summary> 武器ID </summary>
    [Key(0)]
    public int WeaponID;

    /// <summary> 武器名称 </summary>
    [Key(1)]
    public string Name;

    /// <summary> 武器模型地址（历史字段名保留） </summary>
    [Key(2)]
    public string ModleName;

    /// <summary> 图标名称 </summary>
    [Key(3)]
    public string IconName;

    /// <summary> 武器描述 </summary>
    [Key(4)]
    public string Description;

    /// <summary> 武器生成锚点 </summary>
    [Key(5)]
    public int WeaponSpawnSlot;

    /// <summary> 武器装备动画 </summary>
    [Key(6)]
    public int WeaponEquipAnim;

    /// <summary> 启用 </summary>
    [Key(7)]
    public bool Enabled;

    /// <summary> 单发基础伤害 </summary>
    [Key(8)]
    public float Damage;

    /// <summary> 每秒发数 </summary>
    [Key(9)]
    public float FireRate;

    /// <summary> 换弹秒数 </summary>
    [Key(10)]
    public float ReloadTime;

    /// <summary> 弹匣容量 </summary>
    [Key(11)]
    public int MagSize;

    /// <summary> 初始备弹 </summary>
    [Key(12)]
    public int ReserveAmmo;

    /// <summary> 空仓自动换弹 </summary>
    [Key(13)]
    public bool AutoReload;

    /// <summary> 暴击概率0~1 </summary>
    [Key(14)]
    public float CritChance;

    /// <summary> 暴击倍率 </summary>
    [Key(15)]
    public float CritMultiplier;

    /// <summary> 弹速米每秒 </summary>
    [Key(16)]
    public float ProjectileSpeed;

    /// <summary> 每发弹丸数 </summary>
    [Key(17)]
    public int ProjectileCount;

    /// <summary> 水平总散布角度 </summary>
    [Key(18)]
    public float SpreadAngle;

    /// <summary> 反弹次数 </summary>
    [Key(19)]
    public int BounceCount;

    /// <summary> 穿透次数 </summary>
    [Key(20)]
    public int PierceCount;

    /// <summary> 弹丸直径米 </summary>
    [Key(21)]
    public float ProjectileSize;

    /// <summary> 存活秒数 </summary>
    [Key(22)]
    public float ProjectileLifeTime;

}
