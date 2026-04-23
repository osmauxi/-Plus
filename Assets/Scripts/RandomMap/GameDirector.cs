using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameDirector : NetworkBehaviour
{
    public static GameDirector Instance { get; private set; }

    [Header("全局游戏状态")]
    public int currentLayer = 1;          // 当前玩家打到了第几层 (地牢层数)

    [Header("经济系统 (预算参数)")]
    public int baseBudgetPerRoom = 100;   // 第一层，一个普通房间的基础预算
    public float budgetLayerMultiplier = 1.5f; // 每下一层，预算增加 20%

    [Header("怪物商品图鉴")]
    public List<MonsterDataSO> monsterCatalog = new List<MonsterDataSO>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public override void OnNetworkSpawn()
    {
        if (!IsServer) 
            return; // 注册和发牌逻辑只在服务器执行

        foreach (var data in monsterCatalog)
        {
            SyncObjectPool.instance.RegisterDynamicPrefab(data.poolId, data.prefab.GetComponent<NetworkObject>(), data.initialPoolSize);
        }
    }
    /// <summary>
    /// 中央审批局：根据当前层数和房间权重，计算出这个房间该刷什么怪
    /// </summary>
    /// <param name="roomDifficultyWeight">房间倍率（比如普通房是1，精英房是2）</param>
    /// <returns>一份怪物对象池ID的购物清单</returns>
    public List<string> AllocateMonstersForRoom(float roomDifficultyWeight = 1f)
    {
        List<string> shoppingList = new List<string>();

        // 1. 算账：当前房间总预算 = 基础预算 * (层数加成) * 房间特殊倍率
        int budget = (int)(baseBudgetPerRoom * Mathf.Pow(budgetLayerMultiplier, currentLayer - 1) * roomDifficultyWeight);

        Debug.Log($"[发牌员] 房间预算批复：{budget} 块钱。开始智能采购...");

        // 2. 疯狂采购循环 (直到钱花光)
        int safeCounter = 0; // 防止死循环的安全锁
        while (budget > 0 && safeCounter < 1000)
        {
            safeCounter++;

            // 筛选出当前“买得起”且“层数已解锁”的怪物卡片
            List<MonsterDataSO> affordableCards = new List<MonsterDataSO>();
            foreach (var card in monsterCatalog)
            {
                if (card.cost <= budget && currentLayer >= card.minLayerToSpawn)
                {
                    affordableCards.Add(card);
                }
            }

            // 如果连最便宜的怪都买不起了，停止采购
            if (affordableCards.Count == 0) break;

            // 从买得起的列表里，随机挑一个买
            // TODO高级向：未来可以根据权重来 Roll，比如 80% 概率买小怪，20% 概率买精英
            int randomIndex = Random.Range(0, affordableCards.Count);
            MonsterDataSO selectedCard = affordableCards[randomIndex];

            // 扣钱，加入清单！
            budget -= selectedCard.cost;
            shoppingList.Add(selectedCard.poolId);
        }

        Debug.Log($"[发牌员] 采购完毕！总共买了 {shoppingList.Count} 只怪，剩余零钱 {budget} 块。");
        return shoppingList;
    }

    /// <summary>
    /// 开放给怪物外壳的接口：告诉怪物现在的全局难度倍率是多少
    /// </summary>
    public float GetCurrentDifficultyMultiplier()
    {
        // 比如每一层怪物血量增加 10%
        return 1f + (currentLayer - 1) * 0.1f;
    }
}