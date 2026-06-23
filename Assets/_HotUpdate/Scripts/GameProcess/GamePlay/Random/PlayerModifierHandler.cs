using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterStatCollection))]
public class PlayerModifierHandler : NetworkBehaviour
{
    private CharacterStatCollection statCollection;
    private WeaponBase currentWeapon;

    public List<ModifierDataSO> ownedModifiers = new List<ModifierDataSO>();

    // 状态缓存字典，空间换时间
    public Dictionary<string, int> cachedStackCounts = new Dictionary<string, int>();
    public HashSet<string> cachedPlayerTags = new HashSet<string>();

    private void Awake()
    {
        statCollection = GetComponent<CharacterStatCollection>();
        currentWeapon = GetComponentInChildren<WeaponBase>();
    }

    // ======================================================================
    // 交互入口：统一接收 TreasureChest 发来的开箱指令
    // ======================================================================
    public void OpenChestFromTrigger(TreasureChest.ChestType chestType, bool isChaosUpgrade)
    {
        if (!IsOwner) return;

        int mappedChestId = 0;

        // 根据宝箱类型和是否触发 15% 升级，映射到 0~3 的内部 ID
        if (chestType == TreasureChest.ChestType.Standard)
            mappedChestId = isChaosUpgrade ? 1 : 0;
        else if (chestType == TreasureChest.ChestType.ChaosAltar)
            mappedChestId = 1;
        else if (chestType == TreasureChest.ChestType.Mutation)
            mappedChestId = isChaosUpgrade ? 3 : 2;

        NotifyChestOpenedServerRpc(mappedChestId);
    }

    [ServerRpc]
    private void NotifyChestOpenedServerRpc(int chestType)
    {
        TriggerChestUIClientRpc(chestType);
    }

    [ClientRpc]
    private void TriggerChestUIClientRpc(int chestType)
    {
        if (!IsOwner) return;

        List<ModifierDataSO> choices = null;
        string title = "";

        // 根据映射的 ID Roll 不同的池子
        switch (chestType)
        {
            case 0:
                choices = ModifierPoolManager.Instance.RollStandardModifiersWithWeight(3, cachedStackCounts, cachedPlayerTags);
                title = "常规武装箱";
                break;
            case 1:
                choices = ModifierPoolManager.Instance.RollStandardModifiersChaos(3, cachedStackCounts, cachedPlayerTags);
                title = "✨ 混沌武装赐福 ✨";
                break;
            case 2:
                choices = ModifierPoolManager.Instance.RollMutationModifiers(3, cachedStackCounts, cachedPlayerTags);
                title = "异变核心提取";
                break;
            case 3:
                choices = ModifierPoolManager.Instance.RollMutationModifiersChaos(3, cachedStackCounts, cachedPlayerTags);
                title = "✨ 混沌异变核心 ✨";
                break;
        }

        ShowHextechSelectionUI(choices, title);
    }

    // ======================================================================
    // UI 表现层预留接口 (金铲铲海克斯风格)
    // ======================================================================

    private void ShowHextechSelectionUI(List<ModifierDataSO> choices, string title)
    {
        // 兜底校验：如果卡池已经被玩家抽空了
        if (choices == null || choices.Count == 0)
        {
            Debug.LogWarning("[词条UI] 没有抽到任何可用的词条（卡池已空），应当转化为金币/血量补偿。");
            return;
        }

        ModifierSelectionUI.Instance.ShowPanel(choices, (selectedId) =>
        {
            SelectModifierFromUI(selectedId);
        });
    }

    // ======================================================================
    // 网络同步装配核心 (保持不变)
    // ======================================================================

    /// <summary>
    /// 当玩家在 UI 上点击了某一张海克斯卡牌时调用
    /// </summary>
    public void SelectModifierFromUI(string modifierId)
    {
        if (!IsOwner) return;
        ApplyModifierServerRpc(modifierId);
    }

    [ServerRpc]
    private void ApplyModifierServerRpc(string modifierId)
    {
        ApplyModifierClientRpc(modifierId);
    }

    [ClientRpc]
    private void ApplyModifierClientRpc(string modifierId)
    {
        ModifierDataSO modData = ModifierPoolManager.Instance.GetModifierById(modifierId);
        if (modData == null) return;

        ownedModifiers.Add(modData);

        // 刷新缓存字典
        if (cachedStackCounts.ContainsKey(modifierId))
            cachedStackCounts[modifierId]++;
        else
            cachedStackCounts[modifierId] = 1;

        foreach (var tag in modData.tags)
        {
            cachedPlayerTags.Add(tag);
        }
        bool magSizeChanged = false;

        // 注入属性与机制
        foreach (var statMod in modData.statModifiers)
        {
            statCollection.AddModifier(statMod.statType, statMod.value, statMod.modType, modData);
            if (statMod.statType == StatType.MagSize)
            {
                magSizeChanged = true;
            }
        }

        if (modData.specialEffect != null && currentWeapon != null)
        {
            // 将 SO 里的特技逻辑挂载到武器上
            currentWeapon.AddOrUpgradeEffect(modData.specialEffect);
        }

        if (magSizeChanged)
        {
            currentWeapon.ForceInstantReload();
        }

        if (IsServer)
        {
            var healthComp = GetComponent<Health>();
            if (healthComp != null)
            {
                // 直接拿到经过词条乘区计算后的最新血量上限
                float newMaxHp = statCollection.GetStatValue(StatType.MaxHealth);

                float hpIncrease = newMaxHp - healthComp.maxHealth.Value;

                if (hpIncrease > 0)
                {
                    healthComp.maxHealth.Value = newMaxHp;
                    healthComp.currentHealth.Value += hpIncrease; // 同步拔高当前血量
                }
            }
        }
        Debug.Log($"[系统广播] 玩家 {OwnerClientId} 获得了强化：{modData.modifierName}");
    }
}