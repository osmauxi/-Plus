using System;
using MessagePack;
using System.Collections.Generic;
using UnityEngine;

[MessagePackObject]
public class Config_MonsterAttack
{
    /// <summary> 攻击配置ID </summary>
    [Key(0)]
    public int AttackProfileId;

    /// <summary> 判定偏移X </summary>
    [Key(1)]
    public float OriginOffsetX;

    /// <summary> 判定偏移Y </summary>
    [Key(2)]
    public float OriginOffsetY;

    /// <summary> 判定偏移Z </summary>
    [Key(3)]
    public float OriginOffsetZ;

    /// <summary> 球形判定半径 </summary>
    [Key(4)]
    public float Radius;

    /// <summary> 判定距离 </summary>
    [Key(5)]
    public float Distance;

    /// <summary> 目标层掩码 </summary>
    [Key(6)]
    public int TargetLayerMask;

}
