using UnityEngine;

[CreateAssetMenu(fileName = "PhotonMomentumEffect", menuName = "Roguelike/Effects/PhotonMomentum")]
public class PhotonMomentumEffect : WeaponEffectSO
{
    public float baseDamageBoost = 1.10f;
    public float bonusBoostPerStack = 0.05f;
    public float sizeReduce = 0.95f;

    public override void OnHit(ProjectileBase projectile, GameObject target, Vector3 hitPoint, CharacterStatCollection stats)
    {
        int stacks = GetCurrentStacks(stats);
        if (stacks <= 0) return;

        float currentBoost = baseDamageBoost + (stacks - 1) * bonusBoostPerStack;

        // 动态修改物理实体的当前属性，越穿透伤害越高，体积越小！
        projectile.baseDamage *= currentBoost;
        projectile.transform.localScale *= sizeReduce;
    }
}