using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatModType
{
    Flat = 100,         // 固定值加成 (比如: 基础伤害 +5)
    PercentAdd = 200,   // 百分比加法 (比如: 射速 +10%, 多个同类相加后计算)
    PercentMult = 300   // 百分比乘法 (比如: 最终伤害 x1.5倍)
}

public class StatModifier
{
    public readonly float Value;
    public readonly StatModType Type;
    public readonly object Source; //谁提供了这个Buff？(芯片实例、武器本身等)

    public StatModifier(float value, StatModType type, object source = null)
    {
        Value = value;
        Type = type;
        Source = source;
    }
}

public class Stat
{
    public float BaseValue; // 武器或角色的天生白字面板

    protected bool isDirty = true; // 是否需要重新计算
    protected float _value;        // 缓存的最终计算结果
    protected float lastBaseValue; // 用于检测BaseValue是否被直接修改

    // 存放所有施加在这个属性上的词条修饰器
    protected readonly List<StatModifier> statModifiers;

    public Stat(float baseValue = 0f)
    {
        BaseValue = baseValue;
        statModifiers = new List<StatModifier>();
    }

    // 获取最终绿字面板
    public float Value
    {
        get
        {
            if (isDirty || BaseValue != lastBaseValue)
            {
                lastBaseValue = BaseValue;
                _value = CalculateFinalValue();
                isDirty = false;
            }
            return _value;
        }
    }

    public void AddModifier(StatModifier mod)
    {
        isDirty = true;
        statModifiers.Add(mod);
        // 按 Type 排序，确保先算加法，再算乘法
        statModifiers.Sort(CompareModifierOrder);
    }

    public bool RemoveModifier(StatModifier mod)
    {
        if (statModifiers.Remove(mod))
        {
            isDirty = true;
            return true;
        }
        return false;
    }

    // 当玩家丢弃某个芯片时，调用此方法移除该芯片带来的所有加成
    public bool RemoveAllModifiersFromSource(object source)
    {
        bool didRemove = false;
        for (int i = statModifiers.Count - 1; i >= 0; i--)
        {
            if (statModifiers[i].Source == source)
            {
                isDirty = true;
                didRemove = true;
                statModifiers.RemoveAt(i);
            }
        }
        return didRemove;
    }

    // 核心数学计算逻辑
    protected virtual float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0; // 累计的百分比加法

        for (int i = 0; i < statModifiers.Count; i++)
        {
            StatModifier mod = statModifiers[i];

            if (mod.Type == StatModType.Flat)
            {
                finalValue += mod.Value;
            }
            else if (mod.Type == StatModType.PercentAdd)
            {
                sumPercentAdd += mod.Value;

                // 如果到了列表末尾，或者下一个不是 PercentAdd，则结算一次加法百分比
                if (i + 1 >= statModifiers.Count || statModifiers[i + 1].Type != StatModType.PercentAdd)
                {
                    finalValue *= (1.0f + sumPercentAdd);
                    sumPercentAdd = 0; // 清零以防逻辑错误
                }
            }
            else if (mod.Type == StatModType.PercentMult)
            {
                finalValue *= (1.0f + mod.Value); // 独立乘区
            }
        }

        // 保证属性不为负数（比如装弹时间不能是负数）
        return (float)Math.Round(Math.Max(0, finalValue), 4);
    }

    private int CompareModifierOrder(StatModifier a, StatModifier b)
    {
        if (a.Type < b.Type) return -1;
        if (a.Type > b.Type) return 1;
        return 0;
    }
}
