using System;
using MessagePack;
using System.Collections.Generic;
using UnityEngine;

[MessagePackObject]
public class Config_EffectRoll
{
    /// <summary> Effect表外键 </summary>
    [Key(0)]
    public int EffectID;

    /// <summary> Roll卡片显示名 </summary>
    [Key(1)]
    public string DisplayName;

    /// <summary> 当前等级效果描述 </summary>
    [Key(2)]
    public string Description;

    /// <summary> 再次选择后的升级描述 </summary>
    [Key(3)]
    public string UpgradeDescription;

    /// <summary> Addressables图标Key </summary>
    [Key(4)]
    public string IconAddress;

    /// <summary> 是否启用 </summary>
    [Key(5)]
    public bool Enabled;

}
