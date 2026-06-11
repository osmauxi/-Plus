using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 全局战斗统筹中心 (仅在 Server 端运行)
/// 职责：接管当前激活房间的战斗逻辑、波次控制、怪物计数
/// </summary>
public class BattleManager : NetworkBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("战斗状态")]
    public bool isBattleActive = false;
    public int aliveMonsterCount = 0;

    // 内部缓存
    private Vector2Int currentBattleRoomGrid;
    private List<string> pendingMonsters = new List<string>();
    private Transform[] currentSpawnNodes;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 开启一场新战斗 (由 RoomManager 触发)
    /// </summary>
    public void StartRoomBattle(Vector2Int roomGrid, RoomNodeData roomData)
    {
        if (!IsServer || isBattleActive) return;
        int roomType = RoomManager.Instance.AllRoomsData[roomGrid].RoomType;
        bool isMutated;
        float difficultyFactor = GameDirector.Instance.GetRoomDifficultyFactor(roomType, out isMutated);
        if (isMutated)
        {
            Debug.LogWarning($"<color=red>[警告] 房间 {roomGrid} 发生异变！难度倍率：{difficultyFactor}</color>");
            // TODO: 这里可以触发一个 ClientRpc 播放异变音效或改变房间灯光
        }
        currentBattleRoomGrid = roomGrid;
        currentSpawnNodes = roomData.SpawnNodes;
        isBattleActive = true;
        aliveMonsterCount = 0;

        // 1. 向发牌员申请兵力！
        pendingMonsters = GameDirector.Instance.AllocateMonstersForRoom(difficultyFactor);

        // 2. 开始波次生成流程
        StartCoroutine(WaveSpawnRoutine());
    }

    /// <summary>
    /// 波次生成协程：不要把怪一次性全吐出来
    /// </summary>
    private IEnumerator WaveSpawnRoutine()
    {
        // 留给玩家 2 秒钟的进门准备时间
        yield return new WaitForSeconds(2.0f);

        float diffMult = GameDirector.Instance.GetCurrentDifficultyMultiplier();

        // ==========================================
        // 【核心修改 3】：同屏怪海与波次刷新极速狂暴化！
        // ==========================================
        // 同屏上限：前期十几只；大后期可以轻松达到 30~50 只同屏，全屏轰炸！
        int maxActiveMonsters = 12 + (int)(diffMult * 8);

        // 什么时候刷下一波？当场上怪死得剩下 一半 的时候就赶紧补充兵力，绝不让场子冷下来！
        int waveWaitThreshold = maxActiveMonsters / 2;

        while (pendingMonsters.Count > 0)
        {
            if (aliveMonsterCount >= maxActiveMonsters)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            int spawnCountThisWave = Mathf.Min(pendingMonsters.Count, maxActiveMonsters - aliveMonsterCount);

            for (int i = 0; i < spawnCountThisWave; i++)
            {
                string monsterPoolId = pendingMonsters[0];
                pendingMonsters.RemoveAt(0);

                Transform spawnPoint = currentSpawnNodes[Random.Range(0, currentSpawnNodes.Length)];

                NetworkObject monsterObj = SyncObjectPool.instance.GetT(monsterPoolId, spawnPoint.position, Quaternion.identity);
                MonsterEntity monsterBase = monsterObj.GetComponent<MonsterEntity>();
                MonsterDataSO dataSO = GameDirector.Instance.monsterCatalog.Find(x => x.poolId == monsterPoolId);
                monsterBase.InitializeEntity(dataSO);

                monsterBase.ResetEntity();
                // 注入被强化过的难度倍率
                monsterBase.SetupDifficulty(diffMult);
                monsterBase.GetComponent<MonsterBrain>().enabled = true;

                Health monsterHealth = monsterObj.GetComponent<Health>();
                monsterHealth.OnDied -= HandleMonsterDied;
                monsterHealth.OnDied += HandleMonsterDied;

                aliveMonsterCount++;

                // 生成间隔缩短到 0.1 秒，像虫群一样涌出
                yield return new WaitForSeconds(0.1f);
            }

            // 等场上怪剩下不到一半时...
            yield return new WaitUntil(() => aliveMonsterCount <= waveWaitThreshold);

            // 如果发牌员手里还有超过 20 只怪没刷，说明预算爆炸了，连喘息时间都不给，0.5秒后直接空投下一波！
            float nextWaveDelay = pendingMonsters.Count > 20 ? 0.5f : 1.5f;
            yield return new WaitForSeconds(nextWaveDelay);
        }

        yield return new WaitUntil(() => aliveMonsterCount <= 0);

        // ==========================================
        // 战斗胜利！结算管线
        // ==========================================
        isBattleActive = false;
        RoomManager.Instance.NotifyRoomCleared();

        SpawnRoomRewards();
        Debug.Log($"[BattleManager] 房间 {currentBattleRoomGrid} 战斗结束，已通关！");
    }
    private void SpawnRoomRewards()
    {
        if (!IsServer) return; // 生成逻辑只在 Server 执行

        // 1. 拿到当前房间的数据
        int roomType = RoomManager.Instance.AllRoomsData[currentBattleRoomGrid].RoomType;

        // 2. 找到当前房间预留的战利品生成点 (我们在 RoomNodeData 里预留了 TreasurePos 数组)
        if (RoomManager.Instance.SpawnedRooms.TryGetValue(currentBattleRoomGrid, out RoomNodeData nodeData))
        {
            if (nodeData.ChestPos.Length > 0)
            {
                // 默认拿第一个点生成
                Transform spawnPoint = nodeData.ChestPos[0];

                // 3. 决定生成哪种箱子 (根据异变状态和房间类型)
                bool isMutated;
                GameDirector.Instance.GetRoomDifficultyFactor(roomType, out isMutated);

                string chestPrefabId = "Chest_Standard";
                TreasureChest.ChestType expectedType = TreasureChest.ChestType.Standard;

                if (roomType == 2) // 普通精英房 -> 给混沌赐福 (多选池)
                {
                    expectedType = TreasureChest.ChestType.Mutation;

                }
                else if (roomType == -2) // 终极精英房 -> 异变核心 + 传送门
                {
                    expectedType = TreasureChest.ChestType.Mutation;

                    // 生成通关传送门！
                    Transform portalNode = nodeData.NextLevelPos.Length > 0 ? nodeData.NextLevelPos[0] : spawnPoint;
                    GameObject portalObj = SyncObjectPool.instance.GetT("LevelPortal", portalNode.position, Quaternion.identity).gameObject;
                    nodeData.RegisterSpawnedObject(portalObj); 
                }
                else if (isMutated) // 普通异变房
                {
                    expectedType = TreasureChest.ChestType.Mutation;

                }

                NetworkObject chestObj = SyncObjectPool.instance.GetT(chestPrefabId, spawnPoint.position, spawnPoint.rotation);

                if (chestObj != null && chestObj.TryGetComponent<TreasureChest>(out var chestComp))
                {
                    chestComp.currentChestType = expectedType;
                    nodeData.RegisterSpawnedObject(chestObj.gameObject); // 扔进垃圾袋！
                }
            }
            else
            {
                Debug.LogWarning($"[战利品] 房间 {currentBattleRoomGrid} 没有配置 TreasurePos 生成点！");
            }
        }
    }
    private void HandleMonsterDied()
    {
        aliveMonsterCount--;
        // 可以在这里统一处理怪物死亡的额外逻辑，比如掉落金币等
    }
}