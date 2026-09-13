using System;
using MessagePack;
using System.Collections.Generic;
using UnityEngine;

[MessagePackObject]
public class Config_Modifier
{
    /// <summary> Modifier唯一ID </summary>
    [Key(0)]
    public int ModifierID;

    /// <summary> 程序调试名 </summary>
    [Key(1)]
    public string CodeName;

    /// <summary> 目标数据枚举：0伤害，1射速，2换弹，3弹匣，4暴击率，5暴击倍率，6弹速，7弹丸数，8散布，9反弹，10穿透，11尺寸，12寿命，13护盾容量 </summary>
    [Key(2)]
    public int StatType;

    /// <summary> 运算枚举：0 Add，1 Multiply；Multiply直接乘BaseValue </summary>
    [Key(3)]
    public int Operation;

    /// <summary> 基础数值；Multiply统一以1.0为不变基准 </summary>
    [Key(4)]
    public float BaseValue;

    /// <summary> 是否启用 </summary>
    [Key(5)]
    public bool Enabled;

}
