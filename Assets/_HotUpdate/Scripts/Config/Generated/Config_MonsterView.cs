using System;
using MessagePack;
using System.Collections.Generic;
using UnityEngine;

[MessagePackObject]
public class Config_MonsterView
{
    /// <summary> 怪物配置ID </summary>
    [Key(0)]
    public int ConfigId;

    /// <summary> 本地表现池ID </summary>
    [Key(1)]
    public string LocalPoolId;

}
