using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class VFXImpactScaler : MonoBehaviour
{
    [Header("表现夸张系数 (调节爽感)")]
    [Tooltip("如果你觉得权值带来的变化不够刺激，把这个拉大！比如 1.5 或 2.0")]
    public float visualMultiplier = 1.0f;

    private ParticleSystem ps;
    private ParticleSystem.EmissionModule emission;

    // 缓存原始数据
    private ParticleSystem.Burst[] originalBursts;
    private Vector3 originalScale;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        emission = ps.emission;

        // 缓存预制体的原始 Transform 大小 (用来控制整体范围和速度)
        originalScale = transform.localScale;

        // 缓存 Burst
        originalBursts = new ParticleSystem.Burst[emission.burstCount];
        emission.GetBursts(originalBursts);
    }

    public void SetImpactWeight(float baseWeight)
    {
        float finalWeight = baseWeight * visualMultiplier;

        // 1. 【核心恢复】：修改 localScale。
        // 因为你的粒子设置了 Local 模式，这行代码会瞬间把喷射范围(Shape)、初速度(Speed)和大小(Size)全部按比例撑开！
        transform.localScale = originalScale * finalWeight;

        // 2. 动态修改 Burst 粒子爆出数量 (血滴数量变得极多)
        if (originalBursts.Length > 0)
        {
            ParticleSystem.Burst[] modifiedBursts = new ParticleSystem.Burst[originalBursts.Length];
            for (int i = 0; i < originalBursts.Length; i++)
            {
                modifiedBursts[i] = originalBursts[i];

                // 让粒子数量也随之狂增 (加上 Mathf.Min 防止单次爆出超过 500 个导致卡顿)
                short newCount = (short)Mathf.Clamp(originalBursts[i].count.constant * finalWeight, 1, 500);
                modifiedBursts[i].count = new ParticleSystem.MinMaxCurve(newCount);
            }
            emission.SetBursts(modifiedBursts);
        }
    }
}