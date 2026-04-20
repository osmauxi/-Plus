using UnityEngine;
using Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance { get; private set; }

    [Header("震动源槽位 (拖入对应的子物体)")]
    [Tooltip("方案 A：短促、干脆的后坐力震动 (使用 Bump 或 Custom 波形)")]
    public CinemachineImpulseSource recoilSource;

    [Tooltip("方案 B：混沌、持续的爆炸/全屏震动 (使用 Legacy 和 6D Noise)")]
    public CinemachineImpulseSource explosionSource;

    [Tooltip("备用：轻微的受击或交互反馈震动")]
    public CinemachineImpulseSource lightHitSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 【接口 1】：触发定向后坐力 (方案 A)
    /// </summary>
    /// <param name="worldDirection">枪口的反方向</param>
    /// <param name="force">力度倍率</param>
    public void ShakeRecoil(Vector3 worldDirection, float force = 1f)
    {
        if (recoilSource == null) return;

        // 俯视角映射：把世界空间的 X和Z 映射到屏幕的 X和Y，屏蔽缩放(Z)
        Vector3 screenShakeDir = new Vector3(worldDirection.x, worldDirection.z, 0f);
        recoilSource.GenerateImpulseWithVelocity(screenShakeDir.normalized * force);
    }

    /// <summary>
    /// 【接口 2】：触发无方向的全局爆炸震动 (方案 B)
    /// </summary>
    /// <param name="force">力度倍率</param>
    public void ShakeExplosion(float force = 1f)
    {
        if (explosionSource == null) return;

        // 爆炸不需要方向，直接全屏混沌震动
        explosionSource.GenerateImpulseWithForce(force);
    }

    /// <summary>
    /// 【接口 3】：触发轻微震动 (通用)
    /// </summary>
    public void ShakeLight()
    {
        if (lightHitSource == null) return;
        lightHitSource.GenerateImpulseWithForce(1f);
    }
}