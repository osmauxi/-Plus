using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoomManager : NetworkBehaviour
{
    public static RoomManager Instance { get; private set; }

    [Header("基础设置")]
    public float roomSize = 70f; // 极其重要，用于坐标换算

    // 全场唯一需要联网的地图变量！记录当前大家在哪个房间打架
    public NetworkVariable<Vector2Int> CurrentActiveRoom = new NetworkVariable<Vector2Int>(
        new Vector2Int(0, 0),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // 本地维护的纯数据字典（服务器和客户端各自算完地图后都会塞满这个字典）
    public Dictionary<Vector2Int, RoomData> AllRoomsData = new Dictionary<Vector2Int, RoomData>();

    public Dictionary<Vector2Int, RoomNodeData> SpawnedRooms = new Dictionary<Vector2Int, RoomNodeData>(); 
    
    [Header("小地图配置")]
    public Transform MiniNodeParent; // 挂载的父节点

    [Header("小地图颜色")]
    public Color FrameColor = Color.white;
    public Color UnKnownColor = Color.grey;
    public Color ActiveColor = Color.green;
    public Color FinishedColor = Color.white;
    public Color BossRoomColor = Color.red;
    public Color StartRoomColor = Color.yellow;
    public Color ShopRoomColor = Color.cyan;       // 商店房 (Type 1)
    public Color TreasureRoomColor = Color.magenta;// 特殊/宝箱房 (Type 3)
    public Color MonsterRoomColor = Color.gray;

    // 缓存所有生成的小地图图标，方便变色
    private Dictionary<Vector2Int, GameObject> MinimapIcons = new Dictionary<Vector2Int, GameObject>();
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        CurrentActiveRoom.OnValueChanged += OnRoomChanged;
    }

    public override void OnNetworkDespawn()
    {
        CurrentActiveRoom.OnValueChanged -= OnRoomChanged;
    }

    // 给 MapGenerator 调用的接口，用来注册算好的房间
    public void RegisterRoomData(int x, int y, int type, string poolId)
    {
        Vector2Int gridPos = new Vector2Int(x, y);
        if (!AllRoomsData.ContainsKey(gridPos))
        {
            AllRoomsData.Add(gridPos, new RoomData(gridPos, type, poolId));
        }
    }

    private void FixedUpdate()
    {
        // 只有服务器且在游玩状态下才进行房间判定
        // if (!IsServer || GameStateController.instance.currentNetState.Value != GameState.GamePlaying) return;
        if (!IsServer) return; // 暂且简写

        CheckPlayerPositions();
    }
    private float lastRoomChangeTime = 0f;
    private void CheckPlayerPositions()
    {
        if (Time.time - lastRoomChangeTime < 1.0f) 
            return;
        // 遍历你的全局玩家列表
        foreach (var player in PlayerManager.Instance.AllPlayers)
        {
            if (player == null) continue;

            // 不走物理碰撞，用纯数学瞬间算出玩家所在的逻辑网格坐标！
            int gridX = Mathf.RoundToInt(player.transform.position.x / roomSize);
            int gridY = Mathf.RoundToInt(player.transform.position.z / roomSize);
            Vector2Int playerGrid = new Vector2Int(gridX, gridY);

            // 如果玩家踩进的这个网格确实是个合法房间
            if (AllRoomsData.TryGetValue(playerGrid, out RoomData roomData))
            {
                // 如果这不是当前的战斗房间
                if (playerGrid != CurrentActiveRoom.Value)
                {

                    HandlePlayerEnterNewRoom(player, playerGrid, roomData);
                    lastRoomChangeTime = Time.time;
                    break;
                }
            }
        }
    }

    private void HandlePlayerEnterNewRoom(PlayerController enteringPlayer, Vector2Int newRoomGrid, RoomData roomData)
    {
        CurrentActiveRoom.Value = newRoomGrid;

        if (!roomData.IsCleared)
        {
            PullOtherPlayers(enteringPlayer);

            //通知所有客户端立刻生成门，锁死这间房
            LockDoorsClientRpc(newRoomGrid);

            if (SpawnedRooms.TryGetValue(newRoomGrid, out RoomNodeData nodeData))
            {
                BattleManager.Instance.StartRoomBattle(newRoomGrid, nodeData);
            }
        }
    }
    [ClientRpc]
    private void LockDoorsClientRpc(Vector2Int grid)
    {
        if (!SpawnedRooms.TryGetValue(grid, out RoomNodeData nodeData)) return;
        if (!AllRoomsData.TryGetValue(grid, out RoomData data)) return;

        // 兜底安全校验：如果意外被清空了，不锁门
        if (data.IsCleared) return;

        // 先确保旧门被清理（防止因为各种诡异重入导致的门重叠叠加）
        nodeData.OpenDoors();

        var directions = new (Vector2Int gridOffset, Vector3 localPos, float yRot, System.Action<GameObject> AssignDoor)[]
        {
            (Vector2Int.right, new Vector3(roomSize / 2f, 0, 0),   90f, (obj) => nodeData.RightDoor = obj),
            (Vector2Int.left,  new Vector3(-roomSize / 2f, 0, 0), -90f, (obj) => nodeData.LeftDoor = obj),
            (Vector2Int.up,    new Vector3(0, 0, roomSize / 2f),    0f, (obj) => nodeData.UpDoor = obj),
            (Vector2Int.down,  new Vector3(0, 0, -roomSize / 2f), 180f, (obj) => nodeData.DownDoor = obj)
        };

        foreach (var dir in directions)
        {
            Vector2Int neighborGrid = grid + dir.gridOffset;

            // 只要外部有连接的邻居房间，就在这个通道口生成一扇门锁死！
            if (AllRoomsData.ContainsKey(neighborGrid))
            {
                Vector3 spawnPos = nodeData.transform.position + dir.localPos;
                Quaternion spawnRot = Quaternion.Euler(0, dir.yRot, 0);

                // 从池子里拔出一扇门
                GameObject doorObj = LocalObjectPool.instance.GetT("Door", spawnPos, nodeData.transform);
                doorObj.transform.rotation = spawnRot;

                // 录入管家记录册，打完怪后由管家回收
                dir.AssignDoor(doorObj);
            }
        }
    }

    private void PullOtherPlayers(PlayerController enteringPlayer)
    {
        Vector3 enterPos = enteringPlayer.transform.position;

        foreach (var player in PlayerManager.Instance.AllPlayers)
        {
            // 找到没进门的倒霉蛋队友
            if (player != enteringPlayer)
            {
                // 在进门玩家的坐标附近加个随机小偏移，防止两人模型重叠卡死
                Vector3 offset = new Vector3(UnityEngine.Random.Range(-2f, 2f), 0, UnityEngine.Random.Range(-2f, 2f));

                // 因为我们之前把玩家移动改成了“客户端权威”，服务器不能直接改位置。
                // 必须发 RPC 告诉那个客户端：“你被拉扯了，自己改下坐标！”
                ForceTeleportClientRpc(enterPos + offset, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { player.OwnerClientId } }
                });
            }
        }
    }

    [ClientRpc]
    private void ForceTeleportClientRpc(Vector3 targetPos, ClientRpcParams rpcParams = default)
    {
        // 收到指令的本地玩家执行强制位移
        foreach (var player in PlayerManager.Instance.AllPlayers)
        {
            if (player.IsOwner)
            {
                var rb = player.GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.interpolation = RigidbodyInterpolation.None;

                rb.position = targetPos;
                StartCoroutine(RestoreInterpolation(rb));
                Debug.Log("【系统】已传送到队友所在的房间！");
            }
        }
    }
    private IEnumerator RestoreInterpolation(Rigidbody rb)
    {
        yield return new WaitForFixedUpdate(); // 等待物理引擎结算完毕
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
    // ==================== 客户端/全端逻辑：视觉更新与对象池 ====================

    private void OnRoomChanged(Vector2Int oldRoom, Vector2Int newRoom)
    {
        // 只要房间变量发生变化，大家各自在本地更新视觉表现
        UpdateLocalVisuals(newRoom);
        UpdateMinimapFog(newRoom); // 每次移动房间，刷新迷雾颜色
    }

    private void UpdateLocalVisuals(Vector2Int centerRoom)
    {
        HashSet<Vector2Int> activeGrids = new HashSet<Vector2Int>
        {
            centerRoom, centerRoom + Vector2Int.up, centerRoom + Vector2Int.down,
            centerRoom + Vector2Int.left, centerRoom + Vector2Int.right
        };

        foreach (var kvp in SpawnedRooms)
        {
            bool shouldBeActive = activeGrids.Contains(kvp.Key);
            if (kvp.Value.gameObject.activeSelf != shouldBeActive)
            {
                kvp.Value.gameObject.SetActive(shouldBeActive);
            }
        }
    }

    private void SpawnLocalRoomVisual(RoomData data)
    {
        Vector3 worldPos = new Vector3(data.GridPos.x * roomSize, 0, data.GridPos.y * roomSize);

        GameObject roomObj = LocalObjectPool.instance.GetT(data.PoolId, worldPos, this.transform);

        // 核心改变：直接抓取房间自带的大管家！
        RoomNodeData nodeData = roomObj.GetComponent<RoomNodeData>();
        if (nodeData == null)
        {
            Debug.LogError($"[架构警告] 预制件 {data.PoolId} 根节点缺失 RoomNodeData 组件！");
            return;
        }

        var directions = new (Vector2Int gridOffset, Vector3 localPos, float yRot, System.Action<GameObject> AssignDoor)[]
        {
            (Vector2Int.right, new Vector3(roomSize / 2f, 0, 0),   90f, (obj) => nodeData.RightDoor = obj),
            (Vector2Int.left,  new Vector3(-roomSize / 2f, 0, 0), -90f, (obj) => nodeData.LeftDoor = obj),
            (Vector2Int.up,    new Vector3(0, 0, roomSize / 2f),    0f, (obj) => nodeData.UpDoor = obj),
            (Vector2Int.down,  new Vector3(0, 0, -roomSize / 2f), 180f, (obj) => nodeData.DownDoor = obj)
        };

        foreach (var dir in directions)
        {
            Vector2Int neighborGrid = data.GridPos + dir.gridOffset;
            bool hasNeighbor = AllRoomsData.ContainsKey(neighborGrid);

            Vector3 spawnPos = roomObj.transform.position + dir.localPos;
            Quaternion spawnRot = Quaternion.Euler(0, dir.yRot, 0);

            // 情况 A：如果没有邻居，生成实心墙。
            if (!hasNeighbor)
            {
                GameObject wallObj = LocalObjectPool.instance.GetT("Door", spawnPos, roomObj.transform);
                wallObj.transform.rotation = spawnRot;
            }

        }

        // 存入全新的强类型字典
        SpawnedRooms.Add(data.GridPos, nodeData);
        roomObj.SetActive(false);
    }

    public void NotifyRoomCleared()
    {
        if (!IsServer) return;

        Vector2Int currentGrid = CurrentActiveRoom.Value;
        if (AllRoomsData.TryGetValue(currentGrid, out RoomData data))
        {
            data.IsCleared = true;
            // 通知所有客户端开门
            UnlockDoorsClientRpc(currentGrid);
        }
    }

    [ClientRpc]
    private void UnlockDoorsClientRpc(Vector2Int grid)
    {
        if (SpawnedRooms.TryGetValue(grid, out RoomNodeData nodeData))
        {
            nodeData.OpenDoors();
        }
    }

    // 加载下一关卡时调用，一键清空所有数据！
    public void ClearAllLevelVisuals()
    {
        foreach (var kvp in SpawnedRooms)
        {
            kvp.Value.RecycleAll(); // 一键彻底回收（包含门和房间本体）
        }
        SpawnedRooms.Clear();
        AllRoomsData.Clear();
    }

    public void ForceInitVisuals()
    {
        foreach (var kvp in AllRoomsData)
        {
            if (!SpawnedRooms.ContainsKey(kvp.Key))
            {
                SpawnLocalRoomVisual(kvp.Value);
            }
        }

        //根据视野逻辑，只激活当前房间和相邻房间 (SetActive true)
        UpdateLocalVisuals(CurrentActiveRoom.Value);

        //生成整张小地图的节点
        GenerateMinimapIcons();

        //更新视野迷雾
        UpdateMinimapFog(CurrentActiveRoom.Value);

        if (MinimapCamera.Instance != null)
        {
            MinimapCamera.Instance.SnapToRoom(CurrentActiveRoom.Value);
        }
    }
    private void UpdateMinimapFog(Vector2Int centerRoom)
    {
        // 当前房间及周围四格视为“已探索”
        Vector2Int[] visibleGrids = new Vector2Int[]
        {
            centerRoom,
            centerRoom + Vector2Int.up,
            centerRoom + Vector2Int.down,
            centerRoom + Vector2Int.left,
            centerRoom + Vector2Int.right
        };

        foreach (var grid in visibleGrids)
        {
            if (AllRoomsData.TryGetValue(grid, out RoomData data))
            {
                data.IsDiscovered = true;

                if (MinimapIcons.TryGetValue(grid, out GameObject iconObj))
                {
                    var sr = iconObj.transform.GetChild(1).GetComponent<SpriteRenderer>();
                    var frameSr = iconObj.transform.GetChild(0).GetComponent<SpriteRenderer>();

                    // 1. 设置边框颜色（当前房间高亮，其他恢复原色）
                    frameSr.color = (grid == centerRoom) ? ActiveColor : FrameColor;
                    Color targetColor = UnKnownColor;
                    if (data.RoomType == -2) targetColor = BossRoomColor;
                    else if (data.RoomType == -1) targetColor = StartRoomColor;
                    else if (data.RoomType == 1) targetColor = ShopRoomColor;       // 商店
                    else if (data.RoomType == 3) targetColor = TreasureRoomColor;   // 宝箱/特殊
                    else if (data.IsCleared) targetColor = FinishedColor;
                    else targetColor = MonsterRoomColor; // 普通怪物房 (Type 2)

                    sr.color = targetColor;
                }
            }
        }
    }
    private void GenerateMinimapIcons()
    {
        // 如果有旧的，先清理掉
        foreach (var icon in MinimapIcons.Values) Destroy(icon);
        MinimapIcons.Clear();

        foreach (var kvp in AllRoomsData)
        {
            Vector2Int gridPos = kvp.Key;
            RoomData data = kvp.Value;

            // 生成在 (x, y, 0)
            Vector3 iconPos = new Vector3(gridPos.x, gridPos.y, 0);
            GameObject iconObj = LocalObjectPool.instance.GetT("Minimap", iconPos, MiniNodeParent);
            iconObj.transform.rotation = Quaternion.identity;

            // 默认全部设为未知颜色
            var renderers = iconObj.GetComponentsInChildren<SpriteRenderer>();
            if (renderers.Length >= 2)
            {
                renderers[0].color = FrameColor; // 边框
                renderers[1].color = UnKnownColor; // 内部
            }

            MinimapIcons.Add(gridPos, iconObj);
        }
    }
}