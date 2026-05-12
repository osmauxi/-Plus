using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[CreateAssetMenu(fileName = "OverloadEffect", menuName = "Roguelike/Effects/Overload")]
public class OverloadEffect : WeaponEffectSO
{
    [Header("基准与成长数值")]
    public int baseDepth = 1;
    public int bonusDepthPerStack = 1;

    public float baseDamageMultiplier = 0.5f;
    public float bonusDamagePerStack = 0.1f;

    public int chainCount = 3;
    public float searchRadius = 5f;

    public override void OnHit(ProjectileBase projectile, GameObject target, Vector3 hitPoint, CharacterStatCollection stats)
    {
        // 1. 调用基类的通用方法，极其优雅地获取当前层数！
        int currentStacks = GetCurrentStacks(stats);

        // 如果层数 <= 0，说明要么没配置 modifierId，要么是网络延迟还没同步到，直接防错退出
        if (currentStacks <= 0) return;

        // 2. 利用纯数学公式算出当前级别该有的数值！(无状态计算，绝对不会引起联机 Bug)
        int currentMaxDepth = baseDepth + (currentStacks - 1) * bonusDepthPerStack;
        float currentDamageMultiplier = baseDamageMultiplier + (currentStacks - 1) * bonusDamagePerStack;

        // 3. 频率限制 (直接算，不存私有变量)
        float currentAttackSpeed = stats.GetStatValue(StatType.FireRate);
        float frequencyLimit = Mathf.Min(5.0f, currentAttackSpeed * 0.5f);
        float minInterval = 1f / Mathf.Max(0.1f, frequencyLimit);

        // 提示：你之前代码里的 lastTriggerTime 被我删了，因为它会导致所有拿着这个武器的人共享冷却！
        // 如果你需要做严格的防刷 CD，你需要把 CD 记在 `WeaponBase` 或玩家身上，而不是记在单例 SO 里。
        // 但对于电弧来说，其实只要用下面的 visited 集合防止同一次射击无限反弹就足够了。

        // 4. 执行核心逻辑
        HashSet<GameObject> visitedTargets = new HashSet<GameObject> { target };
        ExecuteChainLightning(target.transform.position, stats, 0, currentMaxDepth, currentDamageMultiplier, visitedTargets);
    }

    private void ExecuteChainLightning(Vector3 startPos, CharacterStatCollection stats, int currentDepth, int maxDepth, float dmgMult, HashSet<GameObject> visited)
    {
        if (currentDepth >= maxDepth) return;

        Collider[] hits = Physics.OverlapSphere(startPos, searchRadius);
        int currentChainCount = 0;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy") || visited.Contains(hit.gameObject)) continue;

            Health targetHealth = hit.GetComponentInParent<Health>();
            if (targetHealth != null && !targetHealth.isDead)
            {
                currentChainCount++;
                visited.Add(hit.gameObject);

                Vector3 targetPos = hit.transform.position;

                GlobalLocalVFXPool.Instance.GetVFX("ArcEffect", targetPos);

                if (NetworkManager.Singleton.IsServer)
                {
                    float arcDamage = stats.GetStatValue(StatType.Damage) * dmgMult;
                    targetHealth.TakeDamage(arcDamage, targetPos, (targetPos - startPos).normalized, 0f);
                }

                ExecuteChainLightning(targetPos, stats, currentDepth + 1, maxDepth, dmgMult, visited);

                if (currentChainCount >= chainCount) break;
            }
        }
    }
}