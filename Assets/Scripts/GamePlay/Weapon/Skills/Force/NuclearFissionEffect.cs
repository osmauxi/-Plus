using UnityEngine;
using Unity.Netcode;

[CreateAssetMenu(fileName = "NuclearFissionEffect", menuName = "Roguelike/Effects/NuclearFission")]
public class NuclearFissionEffect : WeaponEffectSO
{
    public float baseDamageRatio = 0.2f;
    public float bonusRatioPerStack = 0.1f;

    public override void OnProjectileDestroyed(ProjectileBase projectile, Vector3 pos, CharacterStatCollection stats)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        int stacks = GetCurrentStacks(stats);
        if (stacks <= 0) return;

        float currentRatio = baseDamageRatio + (stacks - 1) * bonusRatioPerStack;
        float tickDamage = stats.GetStatValue(StatType.Damage) * currentRatio;

        Collider[] existingAreas = Physics.OverlapSphere(pos, 2f);
        foreach (var col in existingAreas)
        {
            if (col.TryGetComponent<DamageArea>(out var area))
            {
                area.InitOrRefresh(5.0f, tickDamage, true);
                return;
            }
        }
        // GameObject areaObj = LocalObjectPool.instance.GetT("RadiationZone", pos, null);
        // areaObj.GetComponent<DamageArea>().InitOrRefresh(5.0f, tickDamage, true);
    }
}