using UnityEngine;
using Unity.Netcode;

[CreateAssetMenu(fileName = "EnergySiphonEffect", menuName = "Roguelike/Effects/EnergySiphon")]
public class EnergySiphonEffect : WeaponEffectSO
{
    [Header("伤害转化率 (吸血吸盾)")]
    public float baseConversionRate = 0.05f;      // 1层时，将本次伤害的 5% 转化为护盾
    public float bonusConversionPerStack = 0.02f; // 每多 1 层，转化率 +2%

    [Header("护盾上限系数")]
    public float capDamageMultiplier = 1.5f;      // 护盾上限最高为玩家当前基础伤害面板的 1.5 倍

    public override void OnHit(ProjectileBase projectile, GameObject target, Vector3 hitPoint, CharacterStatCollection stats)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (!target.CompareTag("Enemy")) return;

        int stacks = GetCurrentStacks(stats);
        if (stacks <= 0) return;

        if (projectile.owner.transform.root.TryGetComponent<Health>(out var playerHealth))
        {
            GameObject vfx = GlobalLocalVFXPool.Instance.GetVFX("Shield_Lightning", target.transform.position);
            vfx.transform.SetParent(projectile.owner.transform);
            vfx.transform.localPosition = new Vector3(0, 1f, 0); // 可以根据需要调整特效位置
            // 1. 获取玩家当前的真实伤害面板
            float currentDamage = stats.GetStatValue(StatType.Damage);

            // 2. 计算转化率 (比如 1层5%，满层5层就是 13%)
            float currentRate = baseConversionRate + (stacks - 1) * bonusConversionPerStack;

            // 3. 计算本次获得的护盾量
            float shieldGain = currentDamage * currentRate;

            // 4. 计算动态上限：伤害越高，不仅吸得越快，能攒的护盾池子也越深！
            float effectCap = currentDamage * capDamageMultiplier;
            float finalShieldLimit = effectCap + stats.GetStatValue(StatType.MaxShield);
            playerHealth.AddShieldServerRpc(shieldGain, finalShieldLimit);
        }
    }
}