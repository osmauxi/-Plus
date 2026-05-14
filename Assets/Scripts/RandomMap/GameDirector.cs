using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameDirector : NetworkBehaviour
{
    public static GameDirector Instance { get; private set; }

    [Header("经济与难度")]
    public int baseBudgetPerRoom = 100;
    public float budgetLayerMultiplier = 1.5f;

    [Header("异变系统设置")]
    [Tooltip("基础异变概率 (0.0 ~ 1.0)")]
    public float baseMutationChance = 0.1f;
    [Tooltip("每个已清理房间增加的额外异变概率")]
    public float chanceAddPerClearedRoom = 0.05f;
    [Tooltip("异变房间的预算倍率")]
    public float mutationBudgetMultiplier = 2.5f;

    // 内部状态：本层已清理的房间数
    private int clearedRoomsInCurrentLayer = 0;

    [Header("AI 导演智能调控")]
    [Tooltip("价格高于此值的怪物被视为精英怪")]
    public int eliteCostThreshold = 80;
    [Tooltip("价格低于此值的怪物被视为炮灰")]
    public int fodderCostThreshold = 25;
    [Tooltip("精英怪最多能占用多少总预算比例 (0.0 ~ 1.0)")]
    public float maxEliteBudgetRatio = 0.4f;
    [Header("动态调控")]
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

        // Boss 精英房：强行异变，且难度极高
        if (roomType == -2)
        {
            isMutated = true;
            return mutationBudgetMultiplier * 1.5f;
        }

        // 普通精英房
        if (roomType == 2)
        {
            return 1.5f;
        }

        // 普通房异变检测
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
        if (cost <= fodderCostThreshold) return 100f; // 炮灰，极度容易被抽中
        if (cost <= eliteCostThreshold) return 50f;   // 基础怪，正常概率
        return 10f;                                   // 精英怪，概率极低
    }

    // ==========================================
    // 核心采购算法 (AI Director 2.0)
    // ==========================================
    public List<string> AllocateMonstersForRoom(float roomDifficultyWeight = 1f)
    {
        List<string> shoppingList = new List<string>();
        //动态预算 = 基础预算 * 层数倍率 * 房间系数 * 清房狂暴系数
        float clearRampUp = 1f + (clearedRoomsInCurrentLayer * budgetIncreasePerClearedRoom);
        int totalBudget = (int)(baseBudgetPerRoom * Mathf.Pow(budgetLayerMultiplier, GameStateController.instance.CurrentLevel.Value - 1) * roomDifficultyWeight * clearRampUp); 
        int currentBudget = totalBudget;

        // 【核心优化】：精英预算上限熔断
        int maxEliteBudget = (int)(totalBudget * maxEliteBudgetRatio);
        int spentOnElites = 0;

        Debug.Log($"[发牌员] 批复预算：{totalBudget}。精英预算额度：{maxEliteBudget}。开始智能采购...");

        int safeCounter = 0;
        while (currentBudget > 0 && safeCounter < 1000)
        {
            safeCounter++;

            // 2. 筛选当前合法的商品
            List<MonsterDataSO> validCandidates = new List<MonsterDataSO>();
            float totalWeightForRoll = 0f;

            foreach (var card in monsterCatalog)
            {
                // 买得起，且层数够
                if (card.cost <= currentBudget && GameStateController.instance.CurrentLevel.Value >= card.minLayerToSpawn)
                {
                    // 【防沉迷拦截】：如果它是精英怪，且买了它就会超出精英预算上限，则直接把它踢出候选名单！
                    if (IsElite(card.cost) && (spentOnElites + card.cost > maxEliteBudget))
                    {
                        continue;
                    }

                    validCandidates.Add(card);
                    totalWeightForRoll += GetSpawnWeight(card.cost);
                }
            }

            // 如果连最便宜的怪都买不起（或者被拦截了），提前结束采购
            if (validCandidates.Count == 0) break;

            // 3. 权重轮盘赌 (Weighted Random)
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

            // 兜底（以防浮点数精度问题）
            if (selectedCard == null) selectedCard = validCandidates[validCandidates.Count - 1];

            // 4. 买定离手，结账扣款！
            currentBudget -= selectedCard.cost;
            shoppingList.Add(selectedCard.poolId);

            // 记账：如果买了精英，把花费算进精英总额度里
            if (IsElite(selectedCard.cost))
            {
                spentOnElites += selectedCard.cost;
            }
        }

        Debug.Log($"[发牌员] 采购完毕！买了 {shoppingList.Count} 只怪。精英消耗: {spentOnElites}。剩余零钱: {currentBudget}。");
        return shoppingList;
    }

    public float GetCurrentDifficultyMultiplier()
    {
        return 1f + (GameStateController.instance.CurrentLevel.Value - 1) * 0.1f + (clearedRoomsInCurrentLayer * 0.05f);
    }

}