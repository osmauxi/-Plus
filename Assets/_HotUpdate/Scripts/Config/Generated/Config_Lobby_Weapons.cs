using System;
using MessagePack;
using System.Collections.Generic;

[MessagePackObject]
public class Config_Lobby_Weapons
{
    /// <summary> 武器ID </summary>
    [Key(0)]
    public int WeaponID;

    /// <summary> 武器名称 </summary>
    [Key(1)]
    public string Name;

    /// <summary> 模型名称 </summary>
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

}
