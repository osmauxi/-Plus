using UnityEngine;
using Unity.Netcode;

[CreateAssetMenu(fileName = "StaticShieldEffect", menuName = "Roguelike/Effects/StaticShield")]
public class StaticShieldEffect : WeaponEffectSO
{
    [Header("击中加盾数值")]
    public float baseShieldPerHit = 1f;       // 1层时，每打中一枪加 1 点盾
    public float bonusShieldPerStack = 0.5f;  // 每多 1 层，多加 0.5 点盾

    [Header("护盾绝对上限")]
    public float baseMaxShieldLimit = 15f;    // 1层时，该词条最多提供 15 点盾
    public float bonusMaxShieldPerStack = 5f; // 每多 1 层，上限提升 5 点

    // ==========================================
    // 核心钩子：改为击中敌人才叠加护盾
    // ==========================================
    public override void OnHit(ProjectileBase projectile, GameObject target, Vector3 hitPoint, CharacterStatCollection stats)
    {
        // 防网络风暴与作弊：只能由服务器来计算并派发护盾
        if (!NetworkManager.Singleton.IsServer) return;

        // 只能是打中敌人才算数 (打墙不加盾)
        if (!target.CompareTag("Enemy")) return;

        int stacks = GetCurrentStacks(stats);
        if (stacks <= 0) return;

        // projectile.owner 就是玩家
        if (projectile.owner.transform.root.TryGetComponent<Health>(out var playerHealth))
        {
            float currentShieldGain = baseShieldPerHit + (stacks - 1) * bonusShieldPerStack;
            float currentEffectCap = baseMaxShieldLimit + (stacks - 1) * bonusMaxShieldPerStack;

            // 最终上限 = 词条上限 + 玩家商店买的基础上限
            float finalShieldLimit = currentEffectCap + stats.GetStatValue(StatType.MaxShield);

            playerHealth.AddShieldServerRpc(currentShieldGain, finalShieldLimit);
        }
    }
}