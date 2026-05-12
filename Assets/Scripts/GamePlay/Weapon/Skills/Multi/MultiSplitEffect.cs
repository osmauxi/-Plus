using UnityEngine;

[CreateAssetMenu(fileName = "MultiSplitEffect", menuName = "Roguelike/Effects/MultiSplit")]
public class MultiSplitEffect : WeaponEffectSO
{
    public int baseSplitCount = 2;
    public int bonusSplitPerStack = 1;
    public int maxGeneration = 1;
    public float damageRatio = 0.6f;

    public override void OnHit(ProjectileBase projectile, GameObject target, Vector3 hitPoint, CharacterStatCollection stats)
    {
        if (projectile.generation >= maxGeneration) return;
        if (!target.CompareTag("Enemy")) return;

        int stacks = GetCurrentStacks(stats);
        if (stacks <= 0) return;

        int currentSplitCount = baseSplitCount + (stacks - 1) * bonusSplitPerStack;
        float spreadAngle = 90f;
        float startAngle = -spreadAngle / 2f;
        float angleStep = currentSplitCount > 1 ? spreadAngle / (currentSplitCount - 1) : 0f;

        float newDamage = projectile.baseDamage * damageRatio;
        int currentBounces = (int)stats.GetStatValue(StatType.BounceCount);
        int currentPierces = (int)stats.GetStatValue(StatType.PierceCount);

        for (int i = 0; i < currentSplitCount; i++)
        {
            float angle = startAngle + i * angleStep;
            Quaternion splitRotation = projectile.transform.rotation * Quaternion.Euler(0, angle, 0);

            // 确保 "Bullet" 和你 WeaponBase 里的 projectilePoolId 保持一致
            GameObject bulletObj = LocalObjectPool.instance.GetT("Bullet", hitPoint, null);
            bulletObj.transform.rotation = splitRotation;

            ProjectileBase newProj = bulletObj.GetComponent<ProjectileBase>();
            newProj.Init(
                owner: projectile.owner,
                damage: newDamage,
                speed: projectile.speed,
                bounces: currentBounces,
                pierces: currentPierces,
                effects: stats.GetComponentInChildren<WeaponBase>().activeEffects,
                stats: stats,
                inheritedVelocity: Vector3.zero,
                generation: projectile.generation + 1
            );
        }
    }
}