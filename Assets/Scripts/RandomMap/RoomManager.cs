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

    private Dictionary<Vector2Int, RoomVisualCache> AllVisuals = new Dictionary<Vector2Int, RoomVisualCache>();

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

    private void CheckPlayerPositions()
    {
        // 遍历你的全局玩家列表
        foreach (var player in GameDebugManager.Instance.AllPlayers)
        {
            if (player == null) continue;

            // 核心奥义：不走物理碰撞，用纯数学瞬间算出玩家所在的逻辑网格坐标！
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
                }
            }
        }
    }

    private void HandlePlayerEnterNewRoom(PlayerController enteringPlayer, Vector2Int newRoomGrid, RoomData roomData)
    {
        // 1. 更新当前全局房间
        CurrentActiveRoom.Value = newRoomGrid;

        // 2. 如果这是个未清理的怪物房或Boss房
        if (!roomData.IsCleared)
        {
            PullOtherPlayers(enteringPlayer);

            // TODO: 服务器开始在这个新房间里生成怪物
        }
    }

    private void PullOtherPlayers(PlayerController enteringPlayer)
    {
        Vector3 enterPos = enteringPlayer.transform.position;

        foreach (var player in GameDebugManager.Instance.AllPlayers)
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
        foreach (var player in GameDebugManager.Instance.AllPlayers)
        {
            if (player.IsOwner)
            {
                var rb = player.GetComponent<Rigidbody>();
                if (rb != null) rb.velocity = Vector3.zero;

                player.transform.position = targetPos;
                Debug.Log("【系统】已传送到队友所在的房间！");
            }
        }
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
        // 1. 确定视野范围（当前 + 上下左右）
        HashSet<Vector2Int> activeGrids = new HashSet<Vector2Int>
        {
            centerRoom,
            centerRoom + Vector2Int.up,
            centerRoom + Vector2Int.down,
            centerRoom + Vector2Int.left,
            centerRoom + Vector2Int.right
        };

        // 2. 遍历账本，不在视野的直接 SetActive(false)
        foreach (var kvp in AllVisuals)
        {
            bool shouldBeActive = activeGrids.Contains(kvp.Key);

            // 因为门和墙都是房间的子物体，隐藏父物体就能全部隐藏，极度省性能
            if (kvp.Value.RoomObj.activeSelf != shouldBeActive)
            {
                kvp.Value.RoomObj.SetActive(shouldBeActive);
            }
        }

        // 3. 生成还没建档的房间
        foreach (var grid in activeGrids)
        {
            if (AllRoomsData.TryGetValue(grid, out RoomData data) && !AllVisuals.ContainsKey(grid))
            {
                SpawnLocalRoomVisual(data);
            }
        }
    }

    private void SpawnLocalRoomVisual(RoomData data)
    {
        Vector3 worldPos = new Vector3(data.GridPos.x * roomSize, 0, data.GridPos.y * roomSize);
        string poolId = "Room_" + data.RoomType;

        // 1. 获取房间实体
        GameObject roomObj = LocalObjectPool.instance.GetT(data.PoolId, worldPos, this.transform);

        // 2. 创建账本
        RoomVisualCache cache = new RoomVisualCache { RoomObj = roomObj };

        // 3. 数学计算门和墙
        var directions = new (Vector2Int gridOffset, Vector3 localPos, float yRotation, System.Action<GameObject> AssignCache)[]
        {
            (Vector2Int.right, new Vector3(roomSize / 2f, 0, 0),   90f, (obj) => cache.RightObj = obj),
            (Vector2Int.left,  new Vector3(-roomSize / 2f, 0, 0), -90f, (obj) => cache.LeftObj = obj),
            (Vector2Int.up,    new Vector3(0, 0, roomSize / 2f),    0f, (obj) => cache.UpObj = obj),
            (Vector2Int.down,  new Vector3(0, 0, -roomSize / 2f), 180f, (obj) => cache.DownObj = obj)
        };

        foreach (var dir in directions)
        {
            Vector2Int neighborGrid = data.GridPos + dir.gridOffset;
            bool hasNeighbor = AllRoomsData.ContainsKey(neighborGrid);

            // 如果这个方向有相连的房间且当前房间未通关，则生成门；否则生成墙
            string borderPoolId = "Door";

            // 绝妙细节：如果是已通关的房间（例如初始房），且有邻居，我们连门都不生成，直接畅通无阻！
            if (hasNeighbor && data.IsCleared) continue;

            Vector3 spawnPos = roomObj.transform.position + dir.localPos;
            Quaternion spawnRot = Quaternion.Euler(0, dir.yRotation, 0);

            GameObject borderObj = LocalObjectPool.instance.GetT(borderPoolId, spawnPos, roomObj.transform);
            borderObj.transform.rotation = spawnRot;

            // 记入账本
            dir.AssignCache(borderObj);
        }

        // 4. 存入全局字典
        AllVisuals.Add(data.GridPos, cache);
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
        if (AllVisuals.TryGetValue(grid, out RoomVisualCache cache))
        {
            // 精准回收：如果是门（对面有房间），并且在账本里有记录，直接还给池子！
            if (AllRoomsData.ContainsKey(grid + Vector2Int.right) && cache.RightObj != null)
            {
                LocalObjectPool.instance.RetToPool(cache.RightObj);
                cache.RightObj = null;
            }
            if (AllRoomsData.ContainsKey(grid + Vector2Int.left) && cache.LeftObj != null)
            {
                LocalObjectPool.instance.RetToPool(cache.LeftObj);
                cache.LeftObj = null;
            }
            if (AllRoomsData.ContainsKey(grid + Vector2Int.up) && cache.UpObj != null)
            {
                LocalObjectPool.instance.RetToPool(cache.UpObj);
                cache.UpObj = null;
            }
            if (AllRoomsData.ContainsKey(grid + Vector2Int.down) && cache.DownObj != null)
            {
                LocalObjectPool.instance.RetToPool(cache.DownObj);
                cache.DownObj = null;
            }
        }
    }

    // 加载下一关卡时调用，一键清空所有数据！
    public void ClearAllLevelVisuals()
    {
        foreach (var kvp in AllVisuals)
        {
            var cache = kvp.Value;
            // 先还门和墙
            if (cache.RightObj != null) LocalObjectPool.instance.RetToPool(cache.RightObj);
            if (cache.LeftObj != null) LocalObjectPool.instance.RetToPool(cache.LeftObj);
            if (cache.UpObj != null) LocalObjectPool.instance.RetToPool(cache.UpObj);
            if (cache.DownObj != null) LocalObjectPool.instance.RetToPool(cache.DownObj);

            // 最后还房间本体
            if (cache.RoomObj != null) LocalObjectPool.instance.RetToPool(cache.RoomObj);
        }

        AllVisuals.Clear();
        AllRoomsData.Clear();
    }

    public void ForceInitVisuals()
    {
        UpdateLocalVisuals(CurrentActiveRoom.Value);

        // 1. 生成整张小地图的节点（纯本地生成，没有网络同步开销）
        GenerateMinimapIcons();

        // 2. 更新视野迷雾
        UpdateMinimapFog(CurrentActiveRoom.Value);

        // 3. 让小地图摄像机瞬间归位
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