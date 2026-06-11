using UnityEngine;
using Unity.Netcode;

[CreateAssetMenu(fileName = "NuclearFissionEffect", menuName = "Roguelike/Effects/NuclearFission")]
public class NuclearFissionEffect : WeaponEffectSO
{
    [Header("基础伤害比率 (大幅加强)")]
    public float baseDamageRatio = 0.5f;   // 1层：每秒造成 50% 面板的真实伤害
    public float bonusRatioPerStack = 0.3f;// 每多1层，加 30%

    [Header("辐射范围成长")]
    public float baseRadius = 4f;          // 初始 4 米
    public float bonusRadiusPerStack = 1f; // 每多1层，加 1 米

    [Header("减速幅度 (0~1)")]
    public float baseSlow = 0.3f;          // 初始：减速 30%
    public float bonusSlowPerStack = 0.1f; // 每多1层，多减速 10% (5层时可减速 70%！)

    // 【重要修改】：改为击中敌人才触发
    public override void OnHit(ProjectileBase projectile, GameObject target, Vector3 hitPoint, CharacterStatCollection stats)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (!target.CompareTag("Enemy")) return;

        int stacks = GetCurrentStacks(stats);
        if (stacks <= 0) return;

        // 纯数学推算当前满配属性
        float currentRatio = baseDamageRatio + (stacks - 1) * bonusRatioPerStack;
        float tickDamage = stats.GetStatValue(StatType.Damage) * currentRatio;

        float currentRadius = baseRadius + (stacks - 1) * bonusRadiusPerStack;
        float currentSlow = baseSlow + (stacks - 1) * bonusSlowPerStack;

        // 检查旧毒圈并刷新
        Collider[] existingAreas = Physics.OverlapSphere(hitPoint, currentRadius);
        foreach (var col in existingAreas)
        {
            if (col.TryGetComponent<DamageArea>(out var area))
            {
                // 把最新的范围和减速比例传进去
                area.InitOrRefresh(5.0f, tickDamage, true, stats, currentRadius, currentSlow);
                return;
            }
        }

        // 生成新毒圈
        GameObject areaObj = GlobalLocalVFXPool.Instance.GetVFX("OnRadiation", hitPoint);
        if (areaObj != null)
        {
            if (areaObj.TryGetComponent<MonoBehaviour>(out var autoReturn))
                autoReturn.enabled = false;

            // 视觉特效动态放大
            areaObj.transform.localScale = new Vector3(currentRadius * 2f, 1f, currentRadius * 2f);
            areaObj.transform.position = new Vector3(hitPoint.x, 0.2f, hitPoint.z); 
            if (!areaObj.TryGetComponent<DamageArea>(out var damageArea))
            {
                damageArea = areaObj.AddComponent<DamageArea>();
            }

            damageArea.InitOrRefresh(5.0f, tickDamage, true, stats, currentRadius, currentSlow);
        }
    }
}