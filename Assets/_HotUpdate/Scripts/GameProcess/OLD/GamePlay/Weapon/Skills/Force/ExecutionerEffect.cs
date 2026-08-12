using UnityEngine;
using Unity.Netcode;

[CreateAssetMenu(fileName = "ExecutionerEffect", menuName = "Roguelike/Effects/Executioner")]
public class ExecutionerEffect : WeaponEffectSO
{
    public float baseThreshold = 0.3f;
    public float bonusThresholdPerStack = 0.05f;

    public override void OnHit(ProjectileBase projectile, GameObject target, Vector3 hitPoint, CharacterStatCollection stats)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (target.CompareTag("Enemy") && target.transform.root.TryGetComponent<Health>(out var targetHealth) && !targetHealth.isDead)
        {
            // 子弹击中触发斩杀
            TryExecute(targetHealth, stats, hitPoint, projectile.transform.forward);
        }
    }

    public bool TryExecute(Health targetHealth, CharacterStatCollection stats, Vector3 hitPoint, Vector3 hitDirection)
    {
        int stacks = GetCurrentStacks(stats);
        if (stacks <= 0) return false;

        float currentThreshold = baseThreshold + (stacks - 1) * bonusThresholdPerStack;

        // 注意防错：标签判断挂在 Health 所在的物体上
        if (!targetHealth.isDead)
        {
            float hpPercent = targetHealth.currentHealth.Value / targetHealth.maxHealth.Value;
            if (hpPercent <= currentThreshold)
            {
                // 触发 999999 斩杀伤害，并给一个巨大的击飞权重 (3.0f) 展现处决的暴力感
                targetHealth.TakeDamage(999999f, hitPoint, hitDirection, 3.0f);
                return true;
            }
        }
        return false;
    }
}