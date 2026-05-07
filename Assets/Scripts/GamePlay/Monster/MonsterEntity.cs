using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

/// <summary>
/// 怪物实体外壳 (Facade / Config)
/// 职责：存放基础固定数值，接收外部难度注入，统筹组件的初始化和重置
/// </summary>
[RequireComponent(typeof(Health), typeof(AIBlackboard))]
public class MonsterEntity : NetworkBehaviour
{
    [HideInInspector] public MonsterDataSO Config;
    private EntityFXManager fXManager;

    [Header("动态表现")]
    public float woundedSpeedMultiplier = 0.5f;

    private Health health;
    public NavMeshAgent agent;
    private AIBlackboard blackboard;
    private Animator anim;

    private void Awake()
    {
        health = GetComponent<Health>();
        agent = GetComponent<NavMeshAgent>();
        blackboard = GetComponent<AIBlackboard>();
        anim = GetComponentInChildren<Animator>();
        fXManager = GetComponent<EntityFXManager>();
        blackboard.EntityConfig = this;

        health.OnDied += HandleDeath;
        health.OnHealthChanged += HandleWoundedFeedback;
    }

    private void OnDestroy()
    {
        health.OnDied -= HandleDeath;
        health.OnHealthChanged -= HandleWoundedFeedback;
    }
    public void InitializeEntity(MonsterDataSO data)
    {
        if (!IsServer) return;
        Config = data; // 拿到自己的档案
    }

    public void ResetEntity()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = false;
        }
        fXManager.ResetAllRenderers();

        anim.speed = 1f;
        anim.Play("Idle", 0, 0f);

        agent.enabled = false;
        agent.enabled = true;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
        else
        {
            Debug.LogWarning($"[导航警告] 怪物 {gameObject.name} 的生成点不在 NavMesh 上！请检查 SpawnNode 的高度。");
        }

        blackboard.ClearBlackboard();
    }


    // ==========================================
    // 接口 2：导演(难度系统)注入
    // ==========================================
    public void SetupDifficulty(float difficultyMultiplier)
    {
        if (!IsServer || Config == null) return;

        // 1. 初始化血量上限和当前血量
        health.InitializeHealth(Config.baseMaxHealth * difficultyMultiplier);

        // 2. 初始化最终移速
        if (agent != null)
        {
            agent.speed = Config.baseSpeed * (1 + (difficultyMultiplier - 1) * 0.1f);
        }
    }

    // ==========================================
    // 表现逻辑与死亡
    // ==========================================
    private void HandleWoundedFeedback(float currentHp, float maxHp)
    {
        if (!IsServer || health.currentHealth.Value <= 0) 
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
        brain.enabled = false;
        if (agent.isActiveAndEnabled) agent.isStopped = true;
        // 回收对象
        SyncObjectPool.instance.RetToPool(GetComponent<NetworkObject>(), Config.poolId);
    }
}