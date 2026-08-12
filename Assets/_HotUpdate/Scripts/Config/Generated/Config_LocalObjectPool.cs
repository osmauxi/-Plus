using System;
using MessagePack;
using System.Collections.Generic;

[MessagePackObject]
public class Config_LocalObjectPool
{
    /// <summary> 配置ID </summary>
    [Key(0)]
    public int ConfigId;

    /// <summary> 对象池ID </summary>
    [Key(1)]
    public string PoolId;

    /// <summary> 配置分组 </summary>
    [Key(2)]
    public string GroupName;

    /// <summary> 预制体Addressable地址 </summary>
    [Key(3)]
    public string PrefabAddress;

    /// <summary> 初始容量 </summary>
    [Key(4)]
    public int InitialCapacity;

    /// <summary> 最大容量 </summary>
    [Key(5)]
    public int MaxSize;

}
