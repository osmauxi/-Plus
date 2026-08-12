using UnityEngine;
using Unity.Netcode;

[CreateAssetMenu(fileName = "KineticBoostEffect", menuName = "Roguelike/Effects/KineticBoost")]
public class KineticBoostEffect : WeaponEffectSO
{
    [Header("阈值与成长")]
    public float baseTier1Threshold = 3.0f;
    public float baseTier2Threshold = 5.0f;
    public float thresholdReducePerStack = 0.5f; // 每多一层，门槛降0.5

    public float baseStunDuration = 1.0f;
    public float bonusStunPerStack = 0.5f;       // 每多一层，多晕0.5秒

    public override void OnHit(ProjectileBase projectile, GameObject target, Vector3 hitPoint, CharacterStatCollection stats)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        int stacks = GetCurrentStacks(stats);
        if (stacks <= 0) return;

        // 动态计算当前需要的门槛和眩晕时间
        float currentTier1 = baseTier1Threshold - (stacks - 1) * thresholdReducePerStack;
        float currentTier2 = baseTier2Threshold - (stacks - 1) * thresholdReducePerStack;
        float currentStunDur = baseStunDuration + (stacks - 1) * bonusStunPerStack;

        float currentFireRate = stats.GetStatValue(StatType.FireRate);
        if (currentFireRate < currentTier1) return;

        if (target.CompareTag("Enemy") && target.transform.root.TryGetComponent<Health>(out var targetHealth) && !targetHealth.isDead)
        {
            float targetHitStopTime = currentFireRate >= currentTier2
                ? currentStunDur + (currentFireRate - currentTier2) * 0.2f
                : 0.5f;

            if (targetHitStopTime > 0f && target.transform.root.TryGetComponent<MonsterBrain>(out var brain))
            {
                if (brain.ApplyHitStop(targetHitStopTime))
                    targetHealth.TriggerHitStopClientRpc(targetHitStopTime);
            }
        }
    }
}