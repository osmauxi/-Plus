using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // 引入寻路命名空间

public class HitStopManager : MonoBehaviour
{
    public static HitStopManager Instance { get; private set; }

    // 用一个内部类来保存被冻结对象的所有状态
    private class FrozenData
    {
        public float unfreezeTime;
        public Animator animator;
        public Rigidbody rb;
        public Vector3 savedVelocity; // 保存冻结前的惯性
        public NavMeshAgent agent;
        public bool wasAgentStopped;
    }

    public float debug = 1;
    private Dictionary<GameObject, FrozenData> frozenEntities = new Dictionary<GameObject, FrozenData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (frozenEntities.Count == 0) return;

        List<GameObject> toUnfreeze = new List<GameObject>();
        float currentTime = Time.unscaledTime;

        foreach (var kvp in frozenEntities)
        {
            if (currentTime >= kvp.Value.unfreezeTime)
            {
                toUnfreeze.Add(kvp.Key);
            }
        }

        foreach (var entity in toUnfreeze)
        {
            Unfreeze(entity);
        }
    }

    /// <summary>
    /// 全面冻结实体（动画、物理、寻路）
    /// </summary>
    public void Freeze(GameObject target, float duration)
    {
        if (target == null) return;


        float targetUnfreezeTime = Time.unscaledTime + duration * debug;

        if (frozenEntities.TryGetValue(target, out FrozenData data))
        {
            // 防连击 Bug：刷新最长的冻结时间
            if (targetUnfreezeTime > data.unfreezeTime)
            {
                data.unfreezeTime = targetUnfreezeTime;
            }
        }
        else
        {
            // 第一次冻结，抓取所有组件并保存状态
            FrozenData newData = new FrozenData();
            newData.unfreezeTime = targetUnfreezeTime;

            // ==========================================
            // 【核心架构升级】：优先走 Facade 获取 O(1) 缓存组件
            // ==========================================
            if (target.TryGetComponent<MonsterEntity>(out var monster))
            {
                newData.animator = monster.Anim;
                newData.rb = monster.Rb;
                newData.agent = monster.agent;
            }
            else if (target.TryGetComponent<PlayerController>(out var player))
            {
                newData.animator = player.Anim;
                newData.rb = player.Rb;
                newData.agent = null; // 玩家没有 NavMeshAgent，直接置空
            }
            else
            {
                // 【兜底方案】：打中的是没重构的旧物体、或者可破坏的场景物件
                newData.animator = target.GetComponentInChildren<Animator>();
                newData.rb = target.GetComponent<Rigidbody>();
                newData.agent = target.GetComponent<NavMeshAgent>();
            }
            // ==========================================

            // 1. 冻结动画
            if (newData.animator != null) newData.animator.speed = 0f;

            // 2. 冻结物理（保存速度，并暂时剥夺物理控制权）
            if (newData.rb != null)
            {
                newData.savedVelocity = newData.rb.velocity;
                newData.rb.velocity = Vector3.zero;
                newData.rb.isKinematic = true; // 开启运动学，像钉子一样钉在原地
            }

            // 3. 冻结寻路（如果是怪物）
            if (newData.agent != null && newData.agent.isActiveAndEnabled)
            {
                newData.wasAgentStopped = newData.agent.isStopped;
                newData.agent.isStopped = true;
            }

            frozenEntities.Add(target, newData);
        }
    }

    private void Unfreeze(GameObject target)
    {
        if (frozenEntities.TryGetValue(target, out FrozenData data))
        {
            // 1. 恢复动画
            if (data.animator != null) data.animator.speed = 1f;

            // 2. 恢复物理
            if (data.rb != null)
            {
                data.rb.isKinematic = false;
                data.rb.velocity = data.savedVelocity; // 把冻结前的惯性还给它
            }

            // 3. 恢复寻路
            if (data.agent != null && data.agent.isActiveAndEnabled)
            {
                data.agent.isStopped = data.wasAgentStopped;
            }

            frozenEntities.Remove(target);
        }
    }
}