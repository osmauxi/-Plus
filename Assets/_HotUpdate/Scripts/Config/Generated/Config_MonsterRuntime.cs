using System;
using MessagePack;
using System.Collections.Generic;
using UnityEngine;

[MessagePackObject]
public class Config_MonsterRuntime
{
    /// <summary> 怪物配置ID </summary>
    [Key(0)]
    public int ConfigId;

    /// <summary> 名称 </summary>
    [Key(1)]
    public string Name;

    /// <summary> 启用 </summary>
    [Key(2)]
    public bool Enabled;

    /// <summary> 移动速度 </summary>
    [Key(3)]
    public float MoveSpeed;

    /// <summary> 攻击触发距离 </summary>
    [Key(4)]
    public float AttackRange;

    /// <summary> 前摇秒数 </summary>
    [Key(5)]
    public float WindupSeconds;

    /// <summary> 后摇秒数 </summary>
    [Key(6)]
    public float RecoverySeconds;

    /// <summary> 基础攻击伤害 </summary>
    [Key(7)]
    public float AttackDamage;

    /// <summary> 最大生命 </summary>
    [Key(8)]
    public float MaxHealth;

    /// <summary> 攻击配置ID </summary>
    [Key(9)]
    public int AttackProfileId;

}
