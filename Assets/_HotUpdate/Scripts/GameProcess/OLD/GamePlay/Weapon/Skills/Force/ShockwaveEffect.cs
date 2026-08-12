using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "ShockwaveEffect", menuName = "Roguelike/Effects/Shockwave")]
public class ShockwaveEffect : WeaponEffectSO
{
    public float baseRadius = 5f;
    public float bonusRadius = 1.5f;
    public float baseForce = 25f;
    public float bonusForce = 10f;

    public override void OnProjectileSpawn(ProjectileBase projectile, CharacterStatCollection stats)
    {
        base.OnProjectileSpawn(projectile, stats);
    }

    public override void OnProjectileDestroyed(ProjectileBase projectile, Vector3 pos, CharacterStatCollection stats)
    {
        int stacks = GetCurrentStacks(stats);
        if (stacks <= 0) return;

        float sizeMod = stats.GetStatValue(StatType.ProjectileSize);
        float currentRadius = (baseRadius + (stacks - 1) * bonusRadius) * sizeMod;
        float currentForce = baseForce + (stacks - 1) * bonusForce;

        // ==========================================
        // 【核心修复】：解决贴墙爆炸特效方向乱飞的问题
        // 用 FromToRotation 强制让特效模型的默认喷发方向（Vector3.up/Y轴），
        // 旋转去对准子弹反弹道方向，让爆炸笔直向外喷射！
        // ==========================================
        Quaternion exploRotation = Quaternion.identity;
        if (projectile != null && projectile.transform.forward.sqrMagnitude > 0.01f)
        {
            exploRotation = Quaternion.FromToRotation(Vector3.up, -projectile.transform.forward);
        }

        GlobalLocalVFXPool.Instance.GetVFX("Explo_Fire", pos, exploRotation, sizeMod);

        // 【此时由于 AudioManager 底层已重构，这个音效绝不会被吃掉】
        AudioManager.instance.PlaySFXByCategory(AudioCategory.SFX_Skill_Explosion, 1f);

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