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

    public NetworkVariable<float> currentShield = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // ==========================================
    // 提供给 UI 和 其他脚本监听的事件 (C# 委托)
    // ==========================================
    /// <summary> 当血量变化时触发 (参数：当前血量，最大血量) </summary>
    public event Action<float, float> OnHealthChanged;

    public event Action<float, float> OnShieldChanged;

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
        currentShield.OnValueChanged += HandleShieldChange;
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= HandleHealthChange;
        currentShield.OnValueChanged -= HandleShieldChange;
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
    /// <param name="hitPoint">受击点</param>
    /// <param name="hitDirection">攻击打来的方向</param>
    /// <param name="hitWeight">击退/顿帧权重</param>
    /// <param name="attacker">谁打的我？</param>
    public void TakeDamage(float rawDamage, Vector3 hitPoint, Vector3 hitDirection, float hitWeight = 1f, Transform attacker = null, bool isTrueDamage = false)
    {
        if (!IsServer || isDead) return;

        if (Time.time < lastHitTime + iFrameDuration)
            return;
        // 这里可以做减伤计算，比如读取 CharacterStatCollection 里的护甲值

        lastHitTime = Time.time;
        float defense = 0;
        if (monsterEntity != null && monsterEntity.Config != null)
        {
            defense = monsterEntity.Config.baseDefense * GameDirector.Instance.GetCurrentDifficultyMultiplier();
        }
        else if (TryGetComponent<CharacterStatCollection>(out var stats))
        {
            // 如果是玩家，从玩家的属性字典里读取护甲值！
            defense = stats.GetStatValue(StatType.Armor);
        }

        float damageReduction = 100f / (100f + defense);
        float finalDamage = rawDamage;
        if (!isTrueDamage)
        {
            finalDamage = rawDamage * damageReduction;
        }
        finalDamage = Mathf.Max(1f, finalDamage);

        if (attacker != null && monsterEntity != null)
        {
            if (monsterEntity.TryGetComponent<AIBlackboard>(out var bb))
            {
                // 仇恨值与最终造成的真实伤害挂钩，打得越痛仇恨越高
                bb.AddThreat(attacker, finalDamage);
            }
        }

        if (currentShield.Value > 0f)
        {
            if (currentShield.Value >= finalDamage)
            {
                currentShield.Value -= finalDamage;
                finalDamage = 0f;
            }
            else
            {
                finalDamage -= currentShield.Value;
                currentShield.Value = 0f;
            }
        }

        // 如果护盾碎了还有真实伤害溢出，再扣血
        if (finalDamage > 0f)
        {
            currentHealth.Value -= finalDamage;
        }

        TriggerHitFeedbackClientRpc(hitPoint, hitDirection);
        ///击退逻辑：伤害越高、权重越大，击退越狠；怪越肉，击退越弱。并且只有横向击退，没有竖向（起飞）效果。
        if (hitWeight > 0f)
        {
            // 1. 提取纯横向方向，拒绝起飞
            Vector3 flatDir = new Vector3(hitDirection.x, 0, hitDirection.z).normalized;

            //击退公式：(基础伤害 * 击退权重 * 全局倍率) / (护甲/重量 + 10)
            //伤害越高、权重越大，击退越狠；怪越肉，击退越弱。10f 是倍率常数，可凭手感微调。
            float knockbackMagnitude = (rawDamage * hitWeight * 10f) / (defense + 10f);

            // 3. 施加小门槛，过滤掉机枪刮痧那种微不可察的抖动，节省性能
            if (knockbackMagnitude > 1.0f)
            {
                Vector3 knockbackForce = flatDir * knockbackMagnitude;

                // 呼叫接口：不关心你是玩家还是怪物，只要实现了接口就击退
                IKnockbackable kb = GetComponent<IKnockbackable>();
                if (kb != null)
                {
                    kb.ApplyKnockback(knockbackForce);
                }
            }
        }
        ///顿帧
        float calculatedStopDuration = (rawDamage * hitWeight) / (defense + 10f);
        float finalStopDuration = Mathf.Clamp(calculatedStopDuration, 0.05f, 0.3f);
        bool shouldVisualFreeze = true;

        if (monsterEntity != null && monsterEntity.TryGetComponent<MonsterBrain>(out var brain))
        {
            shouldVisualFreeze = brain.ApplyHitStop(finalStopDuration);
        }
        else if (TryGetComponent<PlayerController>(out var player))
        {
            // 预留接口：如果以后你想让玩家挨打时也被打断换弹/开枪，可以在 PlayerController 里加个类似的方法
            // player.ApplyHitStop(finalStopDuration);
        }
        if (shouldVisualFreeze)
        {
            TriggerHitStopClientRpc(finalStopDuration);
        }

        if (currentHealth.Value <= 0f)
        {
            currentHealth.Value = 0f;
            isDead = true;
            TriggerBloodBurstClientRpc(hitPoint, hitDirection,hitWeight);
            OnDied?.Invoke(); // 通知同物体上的其他脚本 (比如 AI 脚本准备播死亡动画)
        }
    }
    [ClientRpc]
    public void TriggerHitStopClientRpc(float duration)
    {
        // 收到服务器指令后，全网所有客户端呼叫自己本地的全局顿帧管理器！
        HitStopManager.Instance.Freeze(this.gameObject, duration);
    }
    [ServerRpc(RequireOwnership = false)]
    public void AddShieldServerRpc(float amount, float maxShieldLimit)
    {
        if (isDead) return;
        currentShield.Value += amount;
        if (currentShield.Value > maxShieldLimit)
        {
            currentShield.Value = maxShieldLimit;
        }
    }
    [ClientRpc]
    private void TriggerHitFeedbackClientRpc(Vector3 pos, Vector3 dir)
    {
        fXManager.PlayHitFlash();
        if (gameObject.CompareTag("Player"))
        {
            AudioManager.instance.PlaySFXByCategory(AudioCategory.SFX_Player_Hurt, 0.8f);
        }
    }

    [ClientRpc]
    private void TriggerBloodBurstClientRpc(Vector3 pos, Vector3 dir, float hitWeight)
    {
        float randomX = UnityEngine.Random.Range(-15f, 15f);
        float randomY = UnityEngine.Random.Range(-15f, 15f);
        float randomZ = UnityEngine.Random.Range(-10f, 10f);
        Vector3 randomizedDir = Quaternion.Euler(randomX, randomY, randomZ) * dir;

        // 兜底保护
        if (randomizedDir == Vector3.zero) randomizedDir = Vector3.forward;

        float dampedWeight = 1f;
        if (hitWeight > 1f)
        {
            dampedWeight = 1f + (hitWeight - 1f) * 0.3f; // 软衰减
        }
        else
        {
            dampedWeight = hitWeight; // 小子弹正常缩小
        }

        // 强行锁死上限，比如血花最大只允许是默认的 1.8 倍
        float finalSafeWeight = Mathf.Clamp(dampedWeight, 0.5f, 10f);

        GlobalLocalVFXPool.Instance.GetVFX("BloodBurst", pos, Quaternion.LookRotation(randomizedDir), finalSafeWeight);
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

    private void HandleShieldChange(float oldShield, float newShield)
    {
        OnShieldChanged?.Invoke(newShield, maxHealth.Value);
    }
}