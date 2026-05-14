using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MapGenerator : NetworkBehaviour
{
    public static MapGenerator instance;


    // 在 Inspector 里配置，比如 Type: 2, Count: 3 (代表普通房有 3 种样式)
    [System.Serializable]
    public struct VariantConfig { public int roomType; public int variantCount; }
    public List<VariantConfig> RoomVariantCounts = new List<VariantConfig>();
    // 地图种子
    private NetworkVariable<int> mapSeed = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private System.Random mapPRNG;

    public bool isServerGenerated = false;

    public struct Room
    {
        public int roomID;
        public float roomWeight;
    };

    [SerializeField] private static int RoomTypeNum = 2;
    [SerializeField] private static int ArraySize = 100;
    [SerializeField] private Room[] rooms = new Room[2];

    [Header("房间设置")]
    [SerializeField] private int BossRoomDis = 5;
    [SerializeField] private int NormalRoomDis = 5;
    [SerializeField] private int MinBossRoomDistance = 2;

    private int InitializedRoomNum = 0;
    private int MinIniRoomNum = 12;
    private bool bossRoomGenerated = false;
    private int coroCount = 0;
    private bool GenerateOver = false;

    public bool IsAnimating; // 强烈建议联机时设为 false，确保 PRNG 序列在全端绝对一致
    public Vector2Int initRoomGridPos = new Vector2Int(0, 0);

    int[,] map = new int[ArraySize, ArraySize];
    float[] dirWeight = new float[4];
    int up = 0, down = 0, right = 0, left = 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            if (IsServer) Destroy(gameObject);
            else
            {
                var oldInstance = instance;
                instance = this;
                DontDestroyOnLoad(gameObject);
                if (oldInstance != null) oldInstance.gameObject.SetActive(false);
            }
            return;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsClient) mapSeed.OnValueChanged -= OnMapSeedReceived;
        if (instance == this) instance = null;
    }

    public IEnumerator PreGenerateMap()
    {
        if (IsServer)
        {
            int randomSeed = UnityEngine.Random.Range(1000, 9999);
            mapSeed.Value = randomSeed;
            mapPRNG = new System.Random(randomSeed);
            yield return StartCoroutine(StartGenerateMap());
        }
        else
        {
            if (mapSeed.Value != 0)
            {
                // 如果客户端进得慢，发现种子已经有值了，就不等事件了，直接开始生成！
                Debug.Log($"[地图系统] 发现服务器已分发种子 {mapSeed.Value}，立刻追赶进度！");
                OnMapSeedReceived(0, mapSeed.Value);
            }
            else
            {
                // 如果大家同步得很好，正常监听
                mapSeed.OnValueChanged += OnMapSeedReceived;
            }
        }
    }

    private void OnMapSeedReceived(int oldSeed, int newSeed)
    {
        if (newSeed != 0 && mapPRNG == null)
        {
            mapPRNG = new System.Random(newSeed);
            StartCoroutine(StartGenerateMap());
            mapSeed.OnValueChanged -= OnMapSeedReceived;
        }
    }

    private void MapInitialize()
    {
        for (int i = 0; i < ArraySize; i++)
            for (int j = 0; j < ArraySize; j++)
                map[i, j] = 0;

        int gridToIndexOffset = ArraySize / 2;
        int initArrayX = initRoomGridPos.x + gridToIndexOffset;
        int initArrayY = initRoomGridPos.y + gridToIndexOffset;
        map[initArrayX, initArrayY] = -1;
    }

    #region 辅助函数 (保持不变)
    int RandomDir()
    {
        if (mapPRNG == null) return 1;
        int dir = 0;
        int seed = mapPRNG.Next(1, 101);
        int index = 0;
        for (int i = 0; i < 4; i++)
        {
            int l = (int)(dirWeight[i] * 100f);
            if ((seed > index) && (seed <= (index + l))) dir = i + 1;
            index += l;
        }
        return dir;
    }

    int RandomRoom()
    {
        if (mapPRNG == null) return 1;
        int type = 0;
        int seed = mapPRNG.Next(1, 101);
        int index = 0;
        for (int i = 0; i < RoomTypeNum; i++)
        {
            int l = (int)(rooms[i].roomWeight * 100f);
            if ((seed >= index) && (seed <= (index + l))) type = rooms[i].roomID;
            index += l;
        }
        return type;
    }

    Vector2Int Dir(int x, int y, int d)
    {
        int p = x, q = y;
        bool flag = false;
        int t = (int)((float)BossRoomDis / 3f + 0.5f);
        if ((right > t) && (left > t) && (up > t) && (down > t)) flag = true;
        if (d == 1 && ((right <= t) || flag)) { p++; right++; }
        if (d == 2 && ((left <= t) || flag)) { p--; left++; }
        if (d == 3 && ((up <= t) || flag)) { q++; up++; }
        if (d == 4 && ((down <= t) || flag)) { q--; down++; }
        return new Vector2Int(p, q);
    }
    #endregion

    public IEnumerator StartGenerateMap()
    {
        while (mapPRNG == null) yield return null;
        GenerateOver = false;
        BeginGen();
        while (!GenerateOver) yield return null;

        if (IsServer) isServerGenerated = true;
    }

    void BeginGen()
    {
        // 清理 RoomManager 中的旧数据
        RoomManager.Instance.AllRoomsData.Clear();

        bossRoomGenerated = false;
        InitializedRoomNum = 0;
        MapInitialize();
        up = 0; down = 0; right = 0; left = 0;

        rooms[0].roomID = 1; rooms[0].roomWeight = 0.8f; 
        rooms[1].roomID = 2; rooms[1].roomWeight = 0.2f; 

        for (int i = 0; i < 4; i++) dirWeight[i] = 0;
        int s = mapPRNG.Next(0, 4);
        dirWeight[s] = (float)mapPRNG.NextDouble() * (0.35f - 0.29f) + 0.29f;
        float res = 1f - dirWeight[s];
        for (int i = 0; i < 4; i++)
        {
            if (dirWeight[i] == 0)
            {
                float randomVal = (float)mapPRNG.NextDouble() * (0.3f - 0.26f) + 0.26f;
                dirWeight[i] = Mathf.Min(randomVal, res);
                res -= dirWeight[i];
            }
        }

        coroCount = 0;

        int startType = -1;
        string startPoolId = $"Room_{startType}_{GetRandomVariant(startType)}";
        RoomManager.Instance.RegisterRoomData(initRoomGridPos.x, initRoomGridPos.y, startType, startPoolId,mapPRNG.Next(0, 4));
        InitializedRoomNum++;

        StartCoroutine(GenRoom(initRoomGridPos.x, initRoomGridPos.y, 1, true, IsAnimating, () => coroCount--));
        StartCoroutine(GenRoom(initRoomGridPos.x, initRoomGridPos.y, 1, false, IsAnimating, () => coroCount--));
        StartCoroutine(GenRoom(initRoomGridPos.x, initRoomGridPos.y, 1, false, IsAnimating, () => coroCount--));
    }

    IEnumerator GenRoom(int x, int y, int n, bool boss, bool isAnim, System.Action onComplete)
    {
        try
        {
            coroCount++;

            if (boss && n > BossRoomDis)
            {
                if (isAnim) yield return new WaitForSecondsRealtime(0.25f);
                int gridDistance = Mathf.Abs(x - initRoomGridPos.x) + Mathf.Abs(y - initRoomGridPos.y);

                if (gridDistance < MinBossRoomDistance)
                {
                    int a = 0;
                    Vector2Int newBossPos = Dir(x, y, RandomDir());
                    while ((Mathf.Abs(x - initRoomGridPos.x) + Mathf.Abs(y - initRoomGridPos.y) <= MinBossRoomDistance))
                    {
                        newBossPos = Dir(x, y, RandomDir());
                        a++;
                        if (a >= 15) break;
                    }
                    if (a < 15) StartCoroutine(GenRoom(newBossPos.x, newBossPos.y, n + 1, boss, isAnim, () => coroCount--));
                    yield break;
                }

                string bossPoolId = $"Room_{-2}_{GetRandomVariant(-2)}";
                RoomManager.Instance.RegisterRoomData(x, y, -2, bossPoolId, mapPRNG.Next(0, 4));

                InitializedRoomNum++;
                bossRoomGenerated = true;
                yield break;
            }

            if (!boss && n > NormalRoomDis) yield break;
            if (isAnim) yield return new WaitForSecondsRealtime(0.25f);

            int gridToIndexOffset = ArraySize / 2;
            if (boss || x != 0 && !boss || y != 0 && !boss)
            {
                int arrayX = x + gridToIndexOffset;
                int arrayY = y + gridToIndexOffset;
                int currentType = map[arrayX, arrayY];

                string currentPoolId = $"Room_{currentType}_{GetRandomVariant(currentType)}";
                RoomManager.Instance.RegisterRoomData(x, y, currentType, currentPoolId, mapPRNG.Next(0, 4));
                InitializedRoomNum++;
            }

            Vector2Int nextGridPos = Dir(x, y, RandomDir());
            int c = 0;
            int nextArrayX = nextGridPos.x + gridToIndexOffset;
            int nextArrayY = nextGridPos.y + gridToIndexOffset;

            while (nextArrayX < 0 || nextArrayX >= ArraySize || nextArrayY < 0 || nextArrayY >= ArraySize ||
                   map[nextArrayX, nextArrayY] != 0 || map[nextArrayX, nextArrayY] == -1)
            {
                nextGridPos = Dir(x, y, RandomDir());
                nextArrayX = nextGridPos.x + gridToIndexOffset;
                nextArrayY = nextGridPos.y + gridToIndexOffset;
                c++;
                if (c >= 20)
                {
                    nextGridPos = new Vector2Int(x, y);
                    break;
                }
            }

            if (c < 20)
            {
                map[nextArrayX, nextArrayY] = RandomRoom();
                StartCoroutine(GenRoom(nextGridPos.x, nextGridPos.y, n + 1, boss, isAnim, () => coroCount--));
            }
        }
        finally
        {
            onComplete?.Invoke();
            CheckAllCoroutinesEnded();
        }
    }

    private void CheckAllCoroutinesEnded()
    {
        if (coroCount == 0) CheckRoomNum(InitializedRoomNum);
    }
    private int GetRandomVariant(int roomType)
    {
        if (mapPRNG == null) return 0;
        foreach (var config in RoomVariantCounts)
        {
            if (config.roomType == roomType)
            {
                // 用全端同步的伪随机数种子去 Roll 样式
                return mapPRNG.Next(0, config.variantCount);
            }
        }
        return 0; // 默认只有一种样式 (即 0)
    }
    private void CheckRoomNum(int n)
    {
        if (n <= MinIniRoomNum || bossRoomGenerated == false)
        {
            BeginGen(); // 直接重新生成数据即可
        }
        else
        {
            GenerateOver = true;
            if (IsServer && GameStateController.instance != null)
            {
                GameStateController.instance.ChangeState(GameState.GamePlaying);
            }

            // 【新增】生成完成后，强制触发一次玩家房间检测，生成初始场景的视觉
            if (IsClient || IsServer)
            {
                RoomManager.Instance.ForceInitVisuals();
            }
        }
    }
}