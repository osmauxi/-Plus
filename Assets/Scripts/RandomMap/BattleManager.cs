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

        currentBattleRoomGrid = roomGrid;
        currentSpawnNodes = roomData.SpawnNodes;
        isBattleActive = true;
        aliveMonsterCount = 0;

        // 1. 向发牌员申请兵力！
        pendingMonsters = GameDirector.Instance.AllocateMonstersForRoom();

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

        while (pendingMonsters.Count > 0)
        {
            // 决定这一波刷多少只 (比如最多同屏 10 只，或者按剩余数量的一半刷)
            int spawnCountThisWave = Mathf.Min(pendingMonsters.Count, 8);

            for (int i = 0; i < spawnCountThisWave; i++)
            {
                string monsterPoolId = pendingMonsters[0];
                pendingMonsters.RemoveAt(0);

                // 随机找一个生成点
                Transform spawnPoint = currentSpawnNodes[Random.Range(0, currentSpawnNodes.Length)];

                // 从对象池生成，并同步到所有客户端
                GameObject monsterObj = LocalObjectPool.instance.GetT(monsterPoolId, spawnPoint.position, null);

                // 重置状态与注入难度
                MonsterEntity monsterBase = monsterObj.GetComponent<MonsterEntity>();
                monsterBase.ResetEntity();
                monsterBase.SetupDifficulty(GameDirector.Instance.GetCurrentDifficultyMultiplier());

                // 监听这只怪物的死亡
                Health monsterHealth = monsterObj.GetComponent<Health>();
                monsterHealth.OnDied += HandleMonsterDied;

                aliveMonsterCount++;

                // 每一只怪生成间隔 0.2 秒，防止瞬间卡顿，也更有“接踵而至”的视觉效果
                yield return new WaitForSeconds(0.2f);
            }

            // 等待这一波的怪物死得差不多了（比如场上少于 3 只），再刷下一波
            yield return new WaitUntil(() => aliveMonsterCount <= 3);
            yield return new WaitForSeconds(1.5f); // 波次之间的喘息时间
        }

        // 所有波次都刷完了，等待场上最后几只怪死光
        yield return new WaitUntil(() => aliveMonsterCount <= 0);

        // ==========================================
        // 战斗胜利！结算管线
        // ==========================================
        isBattleActive = false;
        RoomManager.Instance.NotifyRoomCleared(); // 通知开门

        // TODO: 通知掉落系统在这个房间生成战利品/词条三选一
        Debug.Log($"[BattleManager] 房间 {currentBattleRoomGrid} 战斗结束，已通关！");
    }

    private void HandleMonsterDied()
    {
        aliveMonsterCount--;

        // 可以在这里统一处理怪物死亡的额外逻辑，比如掉落金币等
    }
}