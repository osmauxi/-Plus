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

public class RoomNodeData : MonoBehaviour
{
    [Header("战斗生成数据")]
    public Transform[] SpawnNodes;
    public Transform[] ChestPos;
    public Transform[] NextLevelPos;
    public Transform[] PlayerSpawnPos;



    [Header("运行时表现缓存 (程序自动分配)")]
    [HideInInspector] public GameObject RightDoor;
    [HideInInspector] public GameObject LeftDoor;
    [HideInInspector] public GameObject UpDoor;
    [HideInInspector] public GameObject DownDoor;

    /// <summary>
    /// 自治方法：打开并回收所有的门
    /// </summary>
    public void OpenDoors()
    {
        if (RightDoor != null) { LocalObjectPool.instance.RetToPool(RightDoor); RightDoor = null; }
        if (LeftDoor != null) { LocalObjectPool.instance.RetToPool(LeftDoor); LeftDoor = null; }
        if (UpDoor != null) { LocalObjectPool.instance.RetToPool(UpDoor); UpDoor = null; }
        if (DownDoor != null) { LocalObjectPool.instance.RetToPool(DownDoor); DownDoor = null; }
    }

    /// <summary>
    /// 自治方法：清空关卡时的彻底回收
    /// </summary>
    public void RecycleAll()
    {
        OpenDoors(); // 必须先还门
        LocalObjectPool.instance.RetToPool(this.gameObject); // 再把自己还给池子
    }
}