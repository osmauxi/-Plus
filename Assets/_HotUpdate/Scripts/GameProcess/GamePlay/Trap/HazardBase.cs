using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class HazardBase : NetworkBehaviour
{
    [Header("基类设置 (Base Settings)")]
    [Tooltip("每隔多少秒触发一次持续伤害？(如果是踩地雷那种一次性的，填 0 即可)")]
    public float tickInterval = 0.5f;

    // 维护当前站在陷阱里所有“倒霉蛋”的列表
    protected List<Health> victimsInside = new List<Health>();

    private float tickTimer = 0f;

    protected virtual void Awake()
    {
        // 防呆设计：强制确保碰撞体是触发器
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        // 【核心铁律】：陷阱的计时与伤害绝对由服务器权威裁定！
        if (!IsServer) return;

        // 如果需要持续触发，且陷阱里有人
        if (tickInterval > 0f && victimsInside.Count > 0)
        {
            tickTimer += Time.deltaTime;
            if (tickTimer >= tickInterval)
            {
                tickTimer = 0f;
                ExecuteTick();
            }
        }
    }

    private void ExecuteTick()
    {
        // 清理死人：防止有怪物在陷阱里死了，尸体被销毁导致报空指针
        victimsInside.RemoveAll(h => h == null || h.isDead);

        // 遍历所有活人，触发持续判定
        foreach (var victim in victimsInside)
        {
            OnHazardTick(victim);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        Health health = other.GetComponent<Health>();
        if (health != null && !health.isDead)
        {
            if (!victimsInside.Contains(health))
            {
                victimsInside.Add(health);
                OnEntityEnter(health); // 通知子类：有人进来了！
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            if (victimsInside.Contains(health))
            {
                victimsInside.Remove(health);
                OnEntityExit(health); // 通知子类：有人出去了！
            }
        }
    }

    // ==========================================
    // 留给子类实现的多态虚方法 (Virtual Methods)
    // ==========================================

    /// <summary> 当实体踏入陷阱的瞬间调用 </summary>
    protected virtual void OnEntityEnter(Health target) { }

    /// <summary> 当实体离开陷阱的瞬间调用 </summary>
    protected virtual void OnEntityExit(Health target) { }

    /// <summary> 实体在陷阱内时，每隔 tickInterval 秒调用一次 </summary>
    protected virtual void OnHazardTick(Health target) { }
}