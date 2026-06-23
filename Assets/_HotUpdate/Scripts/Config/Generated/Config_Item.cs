using System;
using MessagePack;
using System.Collections.Generic;

[MessagePackObject]
public class Config_Item
{
    /// <summary> 物品ID </summary>
    [Key(0)]
    public int ItemID;

    /// <summary> 物品名称 </summary>
    [Key(1)]
    public string Name;

    /// <summary> 最大堆叠数量 </summary>
    [Key(2)]
    public int MaxStackSize;

    /// <summary> 图标路径 </summary>
    [Key(3)]
    public string IconPath;

    /// <summary> 物品描述 </summary>
    [Key(4)]
    public string Description;

}
