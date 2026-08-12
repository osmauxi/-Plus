using System;
using MessagePack;
using System.Collections.Generic;

[MessagePackObject]
public class Config_RoomTemplate
{
    /// <summary> 模板ID </summary>
    [Key(0)]
    public int TemplateId;

    /// <summary> 房间玩法类型 </summary>
    [Key(1)]
    public int RoomType;

    /// <summary> 允许生成策略掩码 </summary>
    [Key(2)]
    public int AllowedStrategyMask;

    /// <summary> 本地对象池ID </summary>
    [Key(3)]
    public string PoolId;

    /// <summary> 预制体局部连接方向掩码 </summary>
    [Key(4)]
    public int SupportedConnectorMask;

    /// <summary> 是否允许未使用连接口 </summary>
    [Key(5)]
    public bool AllowUnusedConnectors;

    /// <summary> 允许旋转角度掩码 </summary>
    [Key(6)]
    public int AllowedRotations;

    /// <summary> 选择优先级 </summary>
    [Key(7)]
    public int Priority;

    /// <summary> 同优先级随机权重 </summary>
    [Key(8)]
    public float Weight;

}
