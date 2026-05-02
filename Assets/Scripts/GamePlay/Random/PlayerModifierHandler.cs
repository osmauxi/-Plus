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
    // 交互入口：玩家靠近不同类型的宝箱/祭坛，按下 F 键后触发
    // ======================================================================

    /// <summary>
    /// 1. 打开普通怪物房通关宝箱 (带流派倾向，防冲突)
    /// </summary>
    public void OpenStandardChest()
    {
        if (!IsOwner) return; 
        List<ModifierDataSO> choices = ModifierPoolManager.Instance.RollStandardModifiersWithWeight(3, cachedStackCounts, cachedPlayerTags);
        ShowHextechSelectionUI(choices, "常规武装箱");
    }

    /// <summary>
    /// 2. 打开鲜血祭坛 / 隐藏房间 (无视流派冲突，纯随机，可能构建神仙Combo)
    /// </summary>
    public void OpenChaosChest()
    {
        if (!IsOwner) return;
        List<ModifierDataSO> choices = ModifierPoolManager.Instance.RollStandardModifiersChaos(3, cachedStackCounts, cachedPlayerTags);
        ShowHextechSelectionUI(choices, "混沌赐福");
    }

    /// <summary>
    /// 3. 打开 Boss房 / 异变精英房 (抽取机制质变的异变词条)
    /// </summary>
    public void OpenMutationChest()
    {
        if (!IsOwner) return;
        List<ModifierDataSO> choices = ModifierPoolManager.Instance.RollMutationModifiers(3, cachedStackCounts, cachedPlayerTags);
        ShowHextechSelectionUI(choices, "异变核心提取");
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
            currentWeapon.AddOrUpgradeEffect(modData.specialEffect);
        }

        if (magSizeChanged)
        {
            currentWeapon.ForceInstantReload();
        }
        Debug.Log($"[系统广播] 玩家 {OwnerClientId} 获得了强化：{modData.modifierName}");
    }
}