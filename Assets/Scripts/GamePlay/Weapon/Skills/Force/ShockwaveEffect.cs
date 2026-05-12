using UnityEngine;
using Unity.Netcode;

[CreateAssetMenu(fileName = "ShockwaveEffect", menuName = "Roguelike/Effects/Shockwave")]
public class ShockwaveEffect : WeaponEffectSO
{
    public float baseRadius = 5f;
    public float bonusRadius = 1.5f;
    public float baseForce = 25f;
    public float bonusForce = 10f;

    public override void OnProjectileDestroyed(ProjectileBase projectile, Vector3 pos, CharacterStatCollection stats)
    {
        int stacks = GetCurrentStacks(stats);
        if (stacks <= 0) return;

        float sizeMod = stats.GetStatValue(StatType.ProjectileSize);
        float currentRadius = (baseRadius + (stacks - 1) * bonusRadius) * sizeMod;
        float currentForce = baseForce + (stacks - 1) * bonusForce;

        GlobalLocalVFXPool.Instance.GetVFX("ExplosionVFX", pos, Quaternion.identity, sizeMod);

        if (!NetworkManager.Singleton.IsServer) return;

        Collider[] hits = Physics.OverlapSphere(pos, currentRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector3 closestPoint = hit.ClosestPoint(pos);
                float distance = Vector3.Distance(pos, closestPoint);
                float forceRatio = 1f - Mathf.Clamp01(distance / currentRadius);

                if (forceRatio > 0.05f)
                {
                    Vector3 pushDir = (hit.transform.position - pos).normalized;
                    pushDir.y = 0; pushDir.Normalize();

                    if (hit.transform.root.TryGetComponent<IKnockbackable>(out var kb))
                        kb.ApplyKnockback(pushDir * currentForce * forceRatio);

                    if (hit.transform.root.TryGetComponent<Health>(out var health))
                        health.TakeDamage(stats.GetStatValue(StatType.Damage) * 0.2f, closestPoint, pushDir, 0.5f);
                }
            }
        }
    }
}