using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameDirector : NetworkBehaviour
{
    public static GameDirector Instance { get; private set; }

    [Header("经济与难度")]
    public int baseBudgetPerRoom = 100;
    public float budgetLayerMultiplier = 1.5f;

    // 【新增】：前期放水，后期狂暴的基础参数
    [Tooltip("游戏初始的基础难度，设为 0.8 让第一关更轻松")]
    public float baseDifficultyStart = 0.8f;

    [Header("异变系统设置")]
    [Tooltip("基础异变概率 (0.0 ~ 1.0)")]
    public float baseMutationChance = 0.1f;
    [Tooltip("每个已清理房间增加的额外异变概率")]
    public float chanceAddPerClearedRoom = 0.05f;
    [Tooltip("异变房间的预算倍率")]
    public float mutationBudgetMultiplier = 2.5f;

    private int clearedRoomsInCurrentLayer = 0;

    [Header("AI 导演智能调控")]
    public int eliteCostThreshold = 80;
    public int fodderCostThreshold = 25;
    public float maxEliteBudgetRatio = 0.4f;
    public float budgetIncreasePerClearedRoom = 0.15f;

    [Header("怪物商品图鉴")]
    public List<MonsterDataSO> monsterCatalog = new List<MonsterDataSO>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        foreach (var data in monsterCatalog)
        {
            SyncObjectPool.instance.RegisterDynamicPrefab(data.poolId, data.prefab.GetComponent<NetworkObject>(), data.initialPoolSize);
        }
    }
    public void OnRoomCleared()
    {
        if (!IsServer) return;
        clearedRoomsInCurrentLayer++;
    }
    public void ResetTension()
    {
        clearedRoomsInCurrentLayer = 0;
    }
    public float GetRoomDifficultyFactor(int roomType, out bool isMutated)
    {
        isMutated = false;
        if (roomType == -1) return 1f;

        if (roomType == -2)
        {
            isMutated = true;
            return mutationBudgetMultiplier * 1.5f;
        }

        if (roomType == 2)
        {
            return 1.5f;
        }

        float currentChance = baseMutationChance + (clearedRoomsInCurrentLayer * chanceAddPerClearedRoom);
        if (Random.value < currentChance)
        {
            isMutated = true;
            return mutationBudgetMultiplier;
        }

        return 1f;
    }
    private bool IsElite(int cost) => cost > eliteCostThreshold;

    private float GetSpawnWeight(int cost)
    {
        if (cost <= fodderCostThreshold) return 100f;
        if (cost <= eliteCostThreshold) return 50f;
        return 10f;
    }

    // ==========================================
    // 【核心新增】：动态评估全队战力 (根据持有词条数)
    // ==========================================
    private float GetPlayerPowerMultiplier()
    {
        //if (PlayerManager.Instance == null || PlayerManager.Instance.AllPlayers.Count == 0) return 1f;

        //int totalModifiers = 0;
        //foreach (var player in PlayerManager.Instance.AllPlayers)
        //{
        //    // 获取玩家手里枪械的词条数量
        //    var weapon = player.GetComponentInChildren<WeaponBase>();
        //    if (weapon != null)
        //    {
        //        totalModifiers += weapon.activeEffects.Count;
        //    }
        //}

        //// 算出平均没人带了几个词条
        //float avgModifiers = (float)totalModifiers / PlayerManager.Instance.AllPlayers.Count;

        //// 0 词条时是 1.0 倍
        //// 5 个词条时是 1 + 5*0.15 = 1.75 倍
        //// 10 个词条时是 1 + 10*0.15 = 2.5 倍！
        //return 1f + (avgModifiers * 0.15f);
        return 1;
    }

    // ==========================================
    // 核心采购算法 (AI Director 3.0)
    // ==========================================
    public List<string> AllocateMonstersForRoom(float roomDifficultyWeight = 1f)
    {
        List<string> shoppingList = new List<string>();
        float clearRampUp = 1f + (clearedRoomsInCurrentLayer * budgetIncreasePerClearedRoom);

        // 【核心修改 1】：将玩家的“战力倍率”无情地乘以总预算，玩家越强，导演越有钱刷怪！
        float powerMult = GetPlayerPowerMultiplier();
        //int totalBudget = (int)(baseBudgetPerRoom * Mathf.Pow(budgetLayerMultiplier, GameStateController.instance.CurrentLevel.Value - 1) * roomDifficultyWeight * clearRampUp * powerMult);
        int totalBudget = 1;
        int currentBudget = totalBudget;

        int maxEliteBudget = (int)(totalBudget * maxEliteBudgetRatio);
        int spentOnElites = 0;

        Debug.Log($"[发牌员] 批复预算：{totalBudget} (战力膨胀: {powerMult}x) 开始智能采购...");

        int safeCounter = 0;
        while (currentBudget > 0 && safeCounter < 1000)
        {
            safeCounter++;

            List<MonsterDataSO> validCandidates = new List<MonsterDataSO>();
            float totalWeightForRoll = 0f;

            foreach (var card in monsterCatalog)
            {
                //if (card.cost <= currentBudget && GameStateController.instance.CurrentLevel.Value >= card.minLayerToSpawn)
                //{
                //    if (IsElite(card.cost) && (spentOnElites + card.cost > maxEliteBudget)) continue;

                //    validCandidates.Add(card);
                //    totalWeightForRoll += GetSpawnWeight(card.cost);
                //}
            }

            if (validCandidates.Count == 0) break;

            float randomVal = Random.Range(0f, totalWeightForRoll);
            float weightAccumulator = 0f;
            MonsterDataSO selectedCard = null;

            foreach (var card in validCandidates)
            {
                weightAccumulator += GetSpawnWeight(card.cost);
                if (randomVal <= weightAccumulator)
                {
                    selectedCard = card;
                    break;
                }
            }

            if (selectedCard == null) selectedCard = validCandidates[validCandidates.Count - 1];

            currentBudget -= selectedCard.cost;
            shoppingList.Add(selectedCard.poolId);

            if (IsElite(selectedCard.cost)) spentOnElites += selectedCard.cost;
        }

        Debug.Log($"[发牌员] 买了 {shoppingList.Count} 只怪 剩余零钱: {currentBudget} ");
        return shoppingList;
    }

    // ==========================================
    // 【核心修改 2】：重塑怪物血量和移速的难度公式
    // ==========================================
    public float GetCurrentDifficultyMultiplier()
    {
        // 基础层数成长 (每层 +0.15)
        float levelScale = 1;
        float roomScale = clearedRoomsInCurrentLayer * 0.05f;
        // 【让怪变肉】：从词条倍率里抽取一半，加给怪物的血量和速度！
        float powerScale = (GetPlayerPowerMultiplier() - 1f) * 0.5f;

        // 结果：前期白板时 0.8 倍数值 (变脆)；大后期神装时，怪物体力呈指数级爆发！
        return baseDifficultyStart + levelScale + roomScale + powerScale;
    }
}