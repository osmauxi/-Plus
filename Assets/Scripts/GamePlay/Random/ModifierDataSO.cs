using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StatModConfig
{
    public StatType statType;
    public float value;
    public StatModType modType;
}

[CreateAssetMenu(fileName = "NewModifier", menuName = "Data/Modifier")]
public class ModifierDataSO : ScriptableObject
{
    [Header("基础信息")]
    public string modifierId;       // 全局唯一ID，网络同步就靠它 (如 "FireRate_Up")
    public string modifierName;     // UI 显示名称
    public Sprite icon;             // UI 图标
    [TextArea] public string description;

    [Header("堆叠与互斥")]
    [Tooltip("允许获取的最大次数。1为不可重复拿。")]
    public int maxStacks = 1;

    [Tooltip("该词条自身的标签 (如 'Fire', 'Projectile')")]
    public List<string> tags = new List<string>();

    [Tooltip("只要玩家身上有这些标签之一，这个词条就不会出现")]
    public List<string> conflictTags = new List<string>();

    [Header("数值修饰 (挂在 Stat 系统上)")]
    public List<StatModConfig> statModifiers = new List<StatModConfig>();

    [Header("机制注入 (挂在 Weapon 系统上)")]
    [SerializeReference]
    public WeaponEffectSO specialEffect;
}