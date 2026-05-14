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
        //CurrentActiveRoom.OnValueChanged += OnRoomChanged;
    }

    public override void OnNetworkDespawn()
    {
        //CurrentActiveRoom.OnValueChanged -= OnRoomChanged;
    }

    // 给 MapGenerator 调用的接口，用来注册算好的房间
    public void RegisterRoomData(int x, int y, int type, string poolId,int rotIndex)
    {
        Vector2Int gridPos = new Vector2Int(x, y);
        if (!AllRoomsData.ContainsKey(gridPos))
        {
            AllRoomsData.Add(gridPos, new RoomData(gridPos, type, poolId,rotIndex));
        }
    }

    private void FixedUpdate()
    {
        // 只有服务器且在游玩状态下才进行房间判定
        // if (!IsServer || GameStateController.instance.currentNetState.Value != GameState.GamePlaying) return;
        UpdateLocalPlayerVisibility();
        if (!IsServer) return; // 暂且简写

        CheckPlayerPositions();
    }
    private float lastRoomChangeTime = 0f;
    private void CheckPlayerPositions()
    {
        if (Time.time - lastRoomChangeTime < 1.0f) 
            return;
        if (AllRoomsData.TryGetValue(CurrentActiveRoom.Value, out RoomData activeRoomData))
        {
            // 只要没打完，锁死房间判定逻辑
            if (!activeRoomData.IsCleared) return;
        }
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
                    Debug.Log("ENTER");
                    HandlePlayerEnterNewRoom(player, playerGrid, roomData);
                    lastRoomChangeTime = Time.time;
                    break;
                }
            }
        }
    }
    private Vector2Int localLastGrid = new Vector2Int(-999, -999);
    private void UpdateLocalPlayerVisibility()
    {
        // 1. 找到本地玩家
        PlayerController localPlayer = null;
        foreach (var p in PlayerManager.Instance.AllPlayers)
        {
            if (p.IsOwner) { localPlayer = p; break; }
        }

        if (localPlayer == null) return;

        // 2. 计算本地玩家所在的网格
        int gridX = Mathf.RoundToInt(localPlayer.transform.position.x / roomSize);
        int gridY = Mathf.RoundToInt(localPlayer.transform.position.z / roomSize);
        Vector2Int currentGrid = new Vector2Int(gridX, gridY);

        // 3. 只有格位变化时才更新渲染，节省性能
        if (currentGrid != localLastGrid)
        {
            localLastGrid = currentGrid;
            UpdateLocalVisuals(currentGrid); // 这里的 UpdateLocalVisuals 保持你原有的 HashSet 逻辑即可
            UpdateMinimapFog(currentGrid);   // 迷雾也改为本地触发
        }
    }
    private void HandlePlayerEnterNewRoom(PlayerController enteringPlayer, Vector2Int newRoomGrid, RoomData roomData)
    {
        CurrentActiveRoom.Value = newRoomGrid;

        if (!roomData.IsCleared)
        {
            Vector3 enterPos = enteringPlayer.transform.position; 
            Vector3 safeTeleportPos = enterPos;
            if (SpawnedRooms.TryGetValue(newRoomGrid, out RoomNodeData nData) && nData.PlayerSpawnPos != null && nData.PlayerSpawnPos.Length > 0)
            {
                // 【核心优化】：极其自然的传送落点
                Vector3 centerPos = nData.PlayerSpawnPos[0].position;

                // 1. 算出从中心指向玩家大门方向的单位向量
                Vector3 dirToPlayer = (enterPos - centerPos).normalized;

                // 2. 在进门玩家的脚下，顺着向内（中心）的方向回退 3 米，加上随机散布
                safeTeleportPos = enterPos - dirToPlayer * 3.0f;
            }

            PullOtherPlayers(enteringPlayer, safeTeleportPos); // 传过去

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

    private void PullOtherPlayers(PlayerController enteringPlayer, Vector3 basePos)
    {
        foreach (var player in PlayerManager.Instance.AllPlayers)
        {
            if (player != enteringPlayer)
            {
                Debug.Log($"[传送] 准备拉取玩家 ID: {player.OwnerClientId}");

                // 在安全的中心点附近加一点小偏移
                Vector3 offset = new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), 0, UnityEngine.Random.Range(-1.5f, 1.5f));

                // 【核心修复】：放弃 ClientRpcParams！直接全网广播，把需要传送的玩家 ID 传过去
                ForceTeleportClientRpc(player.OwnerClientId, basePos + offset);
            }
        }
    }

    [ClientRpc]
    private void ForceTeleportClientRpc(ulong targetClientId, Vector3 targetPos)
    {
        // 【核心修复】：所有客户端都会收到广播，但如果是叫别人，直接无视！
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        // 走到这里，说明服务器叫的就是我本地这个玩家
        foreach (var player in PlayerManager.Instance.AllPlayers)
        {
            if (player.IsOwner)
            {
                var rb = player.GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.interpolation = RigidbodyInterpolation.None;

                player.transform.position = targetPos;
                rb.position = targetPos;

                StartCoroutine(RestoreInterpolation(rb));
                Debug.Log("【系统】已响应服务器召唤，强行突入战场！");
                break;
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

        roomObj.transform.rotation = Quaternion.Euler(0, data.RoomRotationIndex * 90f, 0);
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
                nodeData.RegisterSpawnedObject(wallObj);
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
                   if (data.RoomType == -1) 
                        targetColor = StartRoomColor;       // 优先级 1：起点永远是起点色
                    else if (data.IsCleared) 
                        targetColor = FinishedColor;        // 优先级 2：只要通关了，统统变绿（包括精英房！）
                    else if (data.RoomType == -2) 
                        targetColor = BossRoomColor;        // 优先级 3：没打的 Boss 房
                    else if (data.RoomType == 2) 
                        targetColor = TreasureRoomColor;    // 优先级 4：没打的精英房 (金色)
                    else 
                        targetColor = MonsterRoomColor;     // 优先级 5：没打的普通房

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