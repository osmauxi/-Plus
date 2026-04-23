using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 怪物实体外壳 (Facade / Config)
/// 职责：存放基础固定数值，接收外部难度注入，统筹组件的初始化和重置
/// </summary>
[RequireComponent(typeof(Health), typeof(AIBlackboard))]
public class MonsterEntity : NetworkBehaviour
{
    [HideInInspector] public MonsterDataSO Config;

    [Header("动态表现")]
    public float woundedSpeedMultiplier = 0.5f;

    private Health health;
    private NavMeshAgent agent;
    private AIBlackboard blackboard;
    private Animator anim;

    private void Awake()
    {
        health = GetComponent<Health>();
        agent = GetComponent<NavMeshAgent>();
        blackboard = GetComponent<AIBlackboard>();
        anim = GetComponentInChildren<Animator>();

        blackboard.EntityConfig = this;

        health.OnDied += HandleDeath;
        health.OnHealthChanged += HandleWoundedFeedback;
    }

    private void OnDestroy()
    {
        health.OnDied -= HandleDeath;
        health.OnHealthChanged -= HandleWoundedFeedback;
    }
    public void InitializeEntity(MonsterDataSO data, float difficultyMultiplier)
    {
        if (!IsServer) return;

        Config = data; // 拿到自己的档案

        // 1. 初始化血量
        health.InitializeHealth(Config.baseMaxHealth * difficultyMultiplier);

        // 2. 初始化速度 (物理/AI重置时也会用到这个速度)
        if (agent != null)
        {
            agent.speed = Config.baseSpeed * (1 + (difficultyMultiplier - 1) * 0.1f);
        }
    }

    // ResetEntity() 方法基本不变，但如果需要恢复基础速度，请使用 Config.baseSpeed
    public void ResetEntity()
    {
        var rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero; 
        rb.isKinematic = false; 

        anim.speed = 1f; anim.Play("Idle", 0, 0f);
        agent.enabled = true; agent.isStopped = false;

        //只修改自己的碰撞体，不包括子物体的伤害判定
        GetComponent<Collider>().enabled = true;

        // 速度恢复为档案里的速度
        agent.speed = Config.baseSpeed;

        blackboard.ClearBlackboard();
    }


    // ==========================================
    // 接口 2：导演(难度系统)注入
    // ==========================================
    public void SetupDifficulty(float difficultyMultiplier)
    {
        if (!IsServer) return;

        // 【关键】：在这里把基础数值派发给干活的组件！
        health.InitializeHealth(Config.baseMaxHealth * difficultyMultiplier);

        if (agent != null)
            agent.speed = Config.baseSpeed * (1 + (difficultyMultiplier - 1) * 0.1f);
    }

    // ==========================================
    // 表现逻辑与死亡
    // ==========================================
    private void HandleWoundedFeedback(float currentHp, float maxHp)
    {
        if (health.currentHealth.Value <= 0) 
            return; // 死了就不管蹒跚了

        if (currentHp / maxHp <= 0.4f && agent != null)
            agent.speed = Config.baseSpeed * woundedSpeedMultiplier;
        else if (agent != null)
            agent.speed = Config.baseSpeed;
    }

    private void HandleDeath()
    {
        if (!IsServer) return;

        // 告诉大脑停工（让模块不再执行）
        var brain = GetComponent<MonsterBrain>();
        if (brain != null) brain.enabled = false;
        if (agent != null && agent.isActiveAndEnabled) agent.isStopped = true;

        // 回收对象
        SyncObjectPool.instance.RetToPool(GetComponent<NetworkObject>());
    }
}