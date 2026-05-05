using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 全局通用的网络生命值组件 (挂载在玩家、怪物、可破坏物体的根节点)
/// </summary>
public class Health : NetworkBehaviour
{
    // 当前血量：只允许服务器修改，客户端只能读取
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(
        100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // 最大血量上限 (同样需要同步，因为客户端画血条需要知道分母)
    public NetworkVariable<float> maxHealth = new NetworkVariable<float>(
        100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // ==========================================
    // 提供给 UI 和 其他脚本监听的事件 (C# 委托)
    // ==========================================
    /// <summary> 当血量变化时触发 (参数：当前血量，最大血量) </summary>
    public event Action<float, float> OnHealthChanged;

    /// <summary> 当死亡时触发 </summary>
    public event Action OnDied;

    [Header("战斗手感设置")]
    [Tooltip("受击后的无敌帧时长 (秒)")]
    public float iFrameDuration = 0.2f;
    private float lastHitTime = -999f;

    public bool isDead = false;

    public EntityFXManager fXManager;
    private MonsterEntity monsterEntity;

    public override void OnNetworkSpawn()
    {
        // 核心：不论是服务器还是客户端，只要 NetworkVariable 的值变了，就会自动触发此回调
        currentHealth.OnValueChanged += HandleHealthChange;
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= HandleHealthChange;
    }
    private void Awake()
    {
        monsterEntity = GetComponent<MonsterEntity>();
        fXManager = GetComponent<EntityFXManager>();
    }
    // ==========================================
    // 服务器专用的初始化与扣血逻辑
    // ==========================================

    /// <summary>
    /// 初始化血量 (仅限服务器调用，通常由 Director 或 玩家生成器 调用)
    /// </summary>
    public void InitializeHealth(float maxHp)
    {
        if (!IsServer) return;
        maxHealth.Value = maxHp;
        currentHealth.Value = maxHp;
        isDead = false;
        lastHitTime = -999f;
    }

    /// <param name="rawDamage">伤害量</param>
    /// <param name="hitPoint">受击点的精确三维坐标</param>
    /// <param name="hitDirection">攻击打来的方向 (用于特效旋转和击退计算)</param>
    public void TakeDamage(float rawDamage, Vector3 hitPoint, Vector3 hitDirection, float hitWeight = 1f)
    {
        if (!IsServer || isDead) return;

        if (Time.time < lastHitTime + iFrameDuration)
            return;
        // 这里可以做减伤计算，比如读取 CharacterStatCollection 里的护甲值

        lastHitTime = Time.time;
        float defense = 0;
        if (monsterEntity != null && monsterEntity.Config != null)
        {
            // 怪物读取 SO 里的基础防御并应用倍率
            defense = monsterEntity.Config.baseDefense * GameDirector.Instance.GetCurrentDifficultyMultiplier();
        }


        float damageReduction = 100f / (100f + defense);
        float finalDamage = rawDamage * damageReduction;
        finalDamage = Mathf.Max(1f, finalDamage);

        currentHealth.Value -= finalDamage;

        TriggerHitFeedbackClientRpc(hitPoint, hitDirection);


        if (currentHealth.Value <= 0f)
        {
            currentHealth.Value = 0f;
            isDead = true;
            TriggerBloodBurstClientRpc(hitPoint, hitDirection,hitWeight);
            OnDied?.Invoke(); // 通知同物体上的其他脚本 (比如 AI 脚本准备播死亡动画)
        }
    }

    [ClientRpc]
    private void TriggerHitFeedbackClientRpc(Vector3 pos, Vector3 dir)
    {
        fXManager.PlayHitFlash();
    }

    [ClientRpc]
    private void TriggerBloodBurstClientRpc(Vector3 pos, Vector3 dir, float hitWeight)
    {
        GlobalLocalVFXPool.Instance.GetVFX("BloodBurst", pos, Quaternion.LookRotation(dir), hitWeight);
    }

    // ==========================================
    // 客户端/表现层逻辑
    // ==========================================
    private void HandleHealthChange(float oldHealth, float newHealth)
    {
        // 触发本地事件，UI 脚本只要订阅了这个事件，就会自动更新血条！
        OnHealthChanged?.Invoke(newHealth, maxHealth.Value);

        // 也可以在这里加一些通用的表现，比如发现 newHealth < oldHealth，就触发全屏红底之类的
    }
}