using System;
using MessagePack;
using System.Collections.Generic;
using UnityEngine;

[MessagePackObject]
public class Config_Effect
{
    /// <summary> 效果唯一ID </summary>
    [Key(0)]
    public int EffectID;

    /// <summary> 程序调试名，不用于UI </summary>
    [Key(1)]
    public string CodeName;

    /// <summary> 关联Modifier ID列表，逗号分隔；按填写顺序应用 </summary>
    [Key(2)]
    public int[] ModifierIDs;

    /// <summary> 是否允许重复选择 </summary>
    [Key(3)]
    public bool Repeatable;

    /// <summary> 最高等级；不可重复时必须为1 </summary>
    [Key(4)]
    public int MaxLevel;

    /// <summary> 效果类型：0普通数值，1特殊脚本或混合效果 </summary>
    [Key(5)]
    public int EffectType;

    /// <summary> 抽取池：0普通，1异变 </summary>
    [Key(6)]
    public int RollPool;

    /// <summary> 所属流派ID列表，逗号分隔 </summary>
    [Key(7)]
    public int[] SchoolIDs;

    /// <summary> 冲突流派ID列表，逗号分隔 </summary>
    [Key(8)]
    public int[] ConflictSchoolIDs;

    /// <summary> 基础抽取权重 </summary>
    [Key(9)]
    public float BaseWeight;

    /// <summary> 每个已拥有同流派增加的权重 </summary>
    [Key(10)]
    public float SchoolWeightBonus;

    /// <summary> 是否启用 </summary>
    [Key(11)]
    public bool Enabled;

}
