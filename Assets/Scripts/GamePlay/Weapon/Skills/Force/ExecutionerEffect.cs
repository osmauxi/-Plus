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
        int stacks = GetCurrentStacks(stats);
        if (stacks <= 0) return;

        float currentThreshold = baseThreshold + (stacks - 1) * bonusThresholdPerStack;

        if (target.CompareTag("Enemy") && !target.CompareTag("Boss"))
        {
            if (target.transform.root.TryGetComponent<Health>(out var targetHealth) && !targetHealth.isDead)
            {
                float hpPercent = targetHealth.currentHealth.Value / targetHealth.maxHealth.Value;
                if (hpPercent <= currentThreshold)
                {
                    targetHealth.TakeDamage(999999f, hitPoint, projectile.transform.forward, 3.0f);
                }
            }
        }
    }
}