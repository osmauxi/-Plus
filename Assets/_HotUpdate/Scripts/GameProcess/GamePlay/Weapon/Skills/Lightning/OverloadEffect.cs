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

    // ==========================================
    // 1. 原本的子弹触发入口
    // ==========================================
    public override void OnHit(ProjectileBase projectile, GameObject target, Vector3 hitPoint, CharacterStatCollection stats)
    {
        // 子弹打中时：触发连锁，并且要求附加首目标的额外电击伤害 (true)
        TriggerChainLightning(target, stats, true);
    }

    // ==========================================
    // 2. 供其他技能(如雷云)跨界调用的联动接口！
    // ==========================================
    public void TriggerExternalChain(GameObject target, CharacterStatCollection stats)
    {
        // 外部技能触发时：只向外传播连锁闪电，不重复劈首目标 (false)，因为雷云本身已经劈过它了
        TriggerChainLightning(target, stats, false);
    }

    // ==========================================
    // 3. 核心分发逻辑
    // ==========================================
    private void TriggerChainLightning(GameObject target, CharacterStatCollection stats, bool applyInitialDamage)
    {
        int currentStacks = GetCurrentStacks(stats);
        if (currentStacks <= 0) return;

        int currentMaxDepth = baseDepth + (currentStacks - 1) * bonusDepthPerStack;
        float currentDamageMultiplier = baseDamageMultiplier + (currentStacks - 1) * bonusDamagePerStack;

        // 如果要求首目标额外伤害 (子弹命中专属)
        if (applyInitialDamage && target.CompareTag("Enemy") && target.TryGetComponent<Health>(out var initialHealth) && !initialHealth.isDead)
        {
            GlobalLocalVFXPool.Instance.GetVFX("OnHit_Lightning", target.transform.position);

            // 【新增】：连锁闪电首发命中音效
            AudioManager.instance.PlaySFXByCategory(AudioCategory.SFX_Skill_LightningHit, 0.8f);

            if (NetworkManager.Singleton.IsServer)
            {
                float initialArcDamage = stats.GetStatValue(StatType.Damage) * currentDamageMultiplier;
                initialHealth.TakeDamage(initialArcDamage, target.transform.position, Vector3.zero, 0f);
            }
        }

        HashSet<GameObject> visitedTargets = new HashSet<GameObject> { target };
        ExecuteChainLightning(target.transform.position, stats, 0, currentMaxDepth, currentDamageMultiplier, visitedTargets);
    }

    // ==========================================
    // 4. 精准连线逻辑 (保持不变)
    // ==========================================
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

                Vector3 dir = targetPos - startPos;
                float distance = dir.magnitude;
                Vector3 midPoint = startPos + dir / 2f;
                Quaternion rotation = Quaternion.LookRotation(dir);

                GameObject arc = GlobalLocalVFXPool.Instance.GetVFX("OnLightning", midPoint, rotation, 1f);

                // 【新增】：连锁闪电弹跳命中音效
                AudioManager.instance.PlaySFXByCategory(AudioCategory.SFX_Skill_LightningHit, 0.8f);

                if (arc != null)
                {
                    float visualLength = Mathf.Max(0.2f, distance - 1.0f);
                    arc.transform.localScale = new Vector3(1f, 1f, visualLength);
                }

                GlobalLocalVFXPool.Instance.GetVFX("OnHit_Lightning", targetPos);

                if (NetworkManager.Singleton.IsServer)
                {
                    float arcDamage = stats.GetStatValue(StatType.Damage) * dmgMult;
                    targetHealth.TakeDamage(arcDamage, targetPos, dir.normalized, 0f);
                }

                ExecuteChainLightning(targetPos, stats, currentDepth + 1, maxDepth, dmgMult, visited);

                if (currentChainCount >= chainCount) break;
            }
        }
    }
}