using System;
using MessagePack;
using System.Collections.Generic;
using UnityEngine;

[MessagePackObject]
public class Config_MonsterSpawn
{
    /// <summary> 怪物配置ID </summary>
    [Key(0)]
    public int ConfigId;

    /// <summary> 预算消耗 </summary>
    [Key(1)]
    public int Cost;

    /// <summary> 选择权重 </summary>
    [Key(2)]
    public float Weight;

    /// <summary> 最低难度 </summary>
    [Key(3)]
    public int MinDifficulty;

    /// <summary> 最高难度 </summary>
    [Key(4)]
    public int MaxDifficulty;

    /// <summary> 单波上限 </summary>
    [Key(5)]
    public int MaxPerWave;

}
