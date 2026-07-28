using System;
using MessagePack;
using System.Collections.Generic;

[MessagePackObject]
public class Config_Lobby_Skins
{
    /// <summary> 皮肤ID </summary>
    [Key(0)]
    public int SkinID;

    /// <summary> 皮肤名称 </summary>
    [Key(1)]
    public string Name;

    /// <summary> 模型名称 </summary>
    [Key(2)]
    public string ModleName;

    /// <summary> 图标名称 </summary>
    [Key(3)]
    public string IconName;

    /// <summary> 物品描述 </summary>
    [Key(4)]
    public string Description;

}
