using System.Collections.Generic;
using UnityEngine;

public class ModifierPoolManager : MonoBehaviour
{
    public static ModifierPoolManager Instance;

    [Header("卡池总览")]
    [Tooltip("常规枪械增强与生存词条")]
    public List<ModifierDataSO> allModifiers;

    [Tooltip("异变与机制质变词条")]
    public List<ModifierDataSO> mutationModifiers; // 新增：专属异变池

    // 高速检索字典 (用于客户端收到 ID 后快速找到对应 SO)
    private Dictionary<string, ModifierDataSO> modifierDict = new Dictionary<string, ModifierDataSO>();
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 初始化字典，把常规池和异变池都装进字典，方便通过全局 ID 统一检索
        RegisterModifiersToDict(allModifiers);
        RegisterModifiersToDict(mutationModifiers);
    }

    private void RegisterModifiersToDict(List<ModifierDataSO> modifiers)
    {
        foreach (var mod in modifiers)
        {
            if (modifierDict.ContainsKey(mod.modifierId))
            {
                Debug.LogError($"[词条系统] 发现重复的词条 ID: {mod.modifierId}");
                continue;
            }
            modifierDict.Add(mod.modifierId, mod);
        }
    }

    public ModifierDataSO GetModifierById(string id)
    {
        modifierDict.TryGetValue(id, out var mod);
        return mod;
    }

    // ======================================================================
    // 带权重/互斥的常规增强词条 Roll (适用于普通战斗房奖励)
    // ======================================================================
    public List<ModifierDataSO> RollStandardModifiersWithWeight(int amount, Dictionary<string, int> stackCounts, HashSet<string> playerTags)
    {
        // 开启冲突检测，开启流派权重加成
        List<ModifierDataSO> results = GenerateCandidatesAndRoll(amount, allModifiers, stackCounts, playerTags, checkConflicts: true, applyWeightBonus: true);

        // ==========================================
        // 【新增】：卡池数量保底机制
        // ==========================================
        if (results.Count < amount)
        {
            Debug.Log($"[词条系统] 常规卡池冲突过多 (仅抽到 {results.Count} 张)，触发全随机保底重抽！");
            // 抛弃残缺结果，直接无视冲突全随机重抽！
            results = RollStandardModifiersChaos(amount, stackCounts, playerTags);
        }

        return results;
    }
    // ======================================================================
    // 纯随机/无视互斥的常规增强词条 Roll (适用于特殊/献祭房间，打造无敌 Combo)
    // ======================================================================
    public List<ModifierDataSO> RollStandardModifiersChaos(int amount, Dictionary<string, int> stackCounts, HashSet<string> playerTags)
    {
        // 关闭冲突检测，关闭权重加成 (纯随机)。但注意，MaxStacks 的底线限制依然生效，防止游戏崩溃！
        return GenerateCandidatesAndRoll(amount, allModifiers, stackCounts, playerTags, checkConflicts: false, applyWeightBonus: false);
    }

    // ======================================================================
    // 带严格互斥与权重的异变词条 Roll (适用于 Boss 房 / 异变精英房)
    // ======================================================================
    public List<ModifierDataSO> RollMutationModifiers(int amount, Dictionary<string, int> stackCounts, HashSet<string> playerTags)
    {
        // 异变词条极度危险，必须严格开启冲突检测，并应用流派权重
        List<ModifierDataSO> results = GenerateCandidatesAndRoll(amount, mutationModifiers, stackCounts, playerTags, checkConflicts: true, applyWeightBonus: true);

        // ==========================================
        // 【新增】：卡池数量保底机制
        // ==========================================
        if (results.Count < amount)
        {
            Debug.Log($"[词条系统] 异变卡池冲突过多 (仅抽到 {results.Count} 张)，触发全随机保底重抽！");
            // 抛弃残缺结果，直接无视冲突全随机重抽！
            results = RollMutationModifiersChaos(amount, stackCounts, playerTags);
        }

        return results;
    }

    // ======================================================================
    // 【新增】：纯随机/无视互斥的异变词条 Roll (适用于异变宝箱触发了 15% 混沌升级)
    // ======================================================================
    public List<ModifierDataSO> RollMutationModifiersChaos(int amount, Dictionary<string, int> stackCounts, HashSet<string> playerTags)
    {
        // 关闭冲突检测，关闭权重倾向！放手让玩家组建逆天 Combo！
        return GenerateCandidatesAndRoll(amount, mutationModifiers, stackCounts, playerTags, checkConflicts: false, applyWeightBonus: false);
    }
    // ======================================================================
    // 内部通用漏斗与轮盘赌算法核心
    // ======================================================================
    private List<ModifierDataSO> GenerateCandidatesAndRoll(
          int amount,
          List<ModifierDataSO> sourcePool,
          Dictionary<string, int> stackCounts,
          HashSet<string> playerTags,
          bool checkConflicts,
          bool applyWeightBonus)
    {
        List<ModifierDataSO> candidates = new List<ModifierDataSO>();
        List<float> weights = new List<float>();
        float totalWeight = 0f;

        // 1. 候选池过滤 (漏斗机制)
        foreach (var mod in sourcePool)
        {
            // [绝对过滤]：是否已达到最大层数？
            if (stackCounts.TryGetValue(mod.modifierId, out int currentStacks))
            {
                if (currentStacks >= mod.maxStacks) continue;
            }

            // [可选过滤]：互斥标签检测
            if (checkConflicts)
            {
                bool isConflict = false;
                foreach (var conflictTag in mod.conflictTags)
                {
                    if (playerTags.Contains(conflictTag))
                    {
                        isConflict = true;
                        break;
                    }
                }
                if (isConflict) continue;
            }

            // 2. 计算权重
            float weight = 100f; // 基础权重

            // [可选加成]：流派倾向检测
            if (applyWeightBonus)
            {
                foreach (var tag in mod.tags)
                {
                    if (playerTags.Contains(tag))
                    {
                        weight += 50f;
                    }
                }
            }

            candidates.Add(mod);
            weights.Add(weight);
            totalWeight += weight;
        } 

        // 3. 轮盘赌抽卡 (此时所有的候选者都已经准备完毕)
        List<ModifierDataSO> results = new List<ModifierDataSO>();
        int rollCount = Mathf.Min(amount, candidates.Count);

        for (int i = 0; i < rollCount; i++)
        {
            float randomVal = Random.Range(0f, totalWeight);
            float weightAccumulator = 0f;

            for (int j = 0; j < candidates.Count; j++)
            {
                weightAccumulator += weights[j];
                if (randomVal <= weightAccumulator)
                {
                    results.Add(candidates[j]);

                    // 抽中后，需要将其从候选池中移除，避免重复抽取
                    totalWeight -= weights[j];
                    candidates.RemoveAt(j);
                    weights.RemoveAt(j);

                    break; // 跳出内层 j 循环，进行下一次抽取 (下一次 i)
                }
            }
        }

        return results;
    }
}