using System;
using MessagePack;
using System.Collections.Generic;
using UnityEngine;

[MessagePackObject]
public class Config_Health
{
    /// <summary> 生命配置ID </summary>
    [Key(0)]
    public int ProfileId;

    /// <summary> 最大生命 </summary>
    [Key(1)]
    public float MaxHealth;

    /// <summary> 最大护盾 </summary>
    [Key(2)]
    public float MaxShield;

    /// <summary> 防御 </summary>
    [Key(3)]
    public float Defense;

    /// <summary> 受击保护秒数 </summary>
    [Key(4)]
    public float DamageGateSeconds;

}
