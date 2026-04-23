using Unity.Netcode;
using UnityEngine;
public class RoomData
{
    public Vector2Int GridPos;   // 逻辑坐标，比如 (0,0), (1,0)
    public int RoomType;         // 房间类型: -1起点, -2Boss, 1普通, 2商店, 3特殊
    public bool IsCleared;       // 是否已通关
    public bool IsDiscovered;    // 是否已经在小地图上探开
    public string PoolId;
    public RoomData(Vector2Int pos, int type,string poolid)
    {
        GridPos = pos;
        RoomType = type;
        // 起始房( -1)、商店(2) 和 特殊房(3) 默认没有战斗，直接视为已清理
        IsCleared = (type == -1 || type == 1 || type == 3);
        IsDiscovered = false;
        PoolId = poolid;
    }
}

public class RoomVisualCache
{
    public GameObject RoomObj;
    public GameObject RightObj;
    public GameObject LeftObj;
    public GameObject UpObj;
    public GameObject DownObj;
}

public class RoomNodeData : MonoBehaviour
{
    public Transform[] SpawnNodes;
    public Transform[] TreasurePos;
}