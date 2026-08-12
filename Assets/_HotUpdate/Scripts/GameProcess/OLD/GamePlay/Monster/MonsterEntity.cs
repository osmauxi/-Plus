using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

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

    public Animator Anim => anim;
    public Rigidbody Rb { get; private set; }

    // ==========================================
    // 移速状态机 (彻底修复了原本难度移速丢失的Bug)
    // ==========================================
    private float baseDifficultySpeed = 0f; // 缓存加入难度加成后的基准速度
    private bool isWounded = false;         // 是否处于半血蹒跚状态
    private float slowMultiplier = 1f;      // 外部塞入的减速倍率
    private float slowTimer = 0f;           // 减速计时器
    public float maxMoveSpeedLimit = 8f;
    private void Awake()
    {
        health = GetComponent<Health>();
        agent = GetComponent<NavMeshAgent>();
        blackboard = GetComponent<AIBlackboard>();
        anim = GetComponentInChildren<Animator>();
        fXManager = GetComponent<EntityFXManager>();
        Rb = GetComponent<Rigidbody>();
        blackboard.EntityConfig = this;

        health.OnDied += HandleDeath;
        health.OnHealthChanged += HandleWoundedFeedback;
    }

    private void OnDestroy()
    {
        health.OnDied -= HandleDeath;
        health.OnHealthChanged -= HandleWoundedFeedback;
    }

    private void Update()
    {
        if (!IsServer) return;

        // 处理减速倒计时
        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                slowMultiplier = 1f; // 减速时间到，恢复
                UpdateSpeed();
            }
        }
    }

    public void InitializeEntity(MonsterDataSO data)
    {
        if (!IsServer) return;
        Config = data;
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

        // 清空状态机
        isWounded = false;
        slowTimer = 0f;
        slowMultiplier = 1f;

        if (agent.isActiveAndEnabled) agent.isStopped = true;
        agent.enabled = true;
        agent.Warp(transform.position);

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }
        blackboard.ClearBlackboard();
        if (UnityEngine.Random.value <= 0.3f)
        {
            AudioManager.instance.PlaySFXByCategory(AudioCategory.SFX_Monster_Roar, 0.6f);
        }
    }

    public void SetupDifficulty(float difficultyMultiplier)
    {
        if (!IsServer || Config == null) return;

        // 1. 最大生命值：无上限爆发成长！(吃满难度系数)
        health.InitializeHealth(Config.baseMaxHealth * difficultyMultiplier);

        // 2. 移动速度：增加侵略性，但加上绝对限速锁！
        // 成长率从原先的 0.1f 提到了 0.2f，让前期怪物加速明显一点，给玩家压力
        float calculatedSpeed = Config.baseSpeed * (1f + (difficultyMultiplier - 1f) * 0.2f);

        // 使用 Mathf.Min 强行给移速盖上天花板
        baseDifficultySpeed = Mathf.Min(calculatedSpeed, maxMoveSpeedLimit);

        UpdateSpeed();
    }

    // ==========================================
    // 开放给毒沼/冰霜的减速接口
    // ==========================================
    public void ApplySlow(float multiplier, float duration)
    {
        if (!IsServer) return;

        // 如果有多个减速源，取最强的减速效果 (multiplier 越小越慢)
        if (slowTimer <= 0 || multiplier < slowMultiplier)
        {
            slowMultiplier = multiplier;
        }

        // 刷新持续时间
        slowTimer = Mathf.Max(slowTimer, duration);
        UpdateSpeed();
    }

    private void HandleWoundedFeedback(float currentHp, float maxHp)
    {
        if (!IsServer || health.currentHealth.Value <= 0) return;

        // 仅标记状态，将计算权利交给统一结算中心
        isWounded = (currentHp / maxHp <= 0.4f);
        UpdateSpeed();
    }

    /// <summary>
    /// 唯一合法的移速计算中心，杜绝冲突
    /// </summary>
    private void UpdateSpeed()
    {
        if (agent == null || Config == null || health.currentHealth.Value <= 0) return;

        float finalSpeed = baseDifficultySpeed > 0f ? baseDifficultySpeed : Config.baseSpeed;

        if (isWounded) finalSpeed *= woundedSpeedMultiplier; // 先算残血蹒跚
        finalSpeed *= slowMultiplier;                        // 再算外力减速

        agent.speed = finalSpeed;
    }

    private void HandleDeath()
    {
        if (!IsServer) return;
        var brain = GetComponent<MonsterBrain>();
        if (brain != null) brain.enabled = false;
        if (agent.isActiveAndEnabled) agent.isStopped = true;

        SyncObjectPool.instance.RetToPool(GetComponent<NetworkObject>(), Config.poolId);
    }
}