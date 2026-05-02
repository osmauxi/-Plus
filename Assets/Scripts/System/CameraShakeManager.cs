using UnityEngine;
using Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance { get; private set; }

    [Header("震动源槽位")]
    public CinemachineImpulseSource recoilSource; 
    public CinemachineImpulseSource explosionSource; 
    public CinemachineImpulseSource lightHitSource; 

    [Header("防抽搐冷却 (秒)")]
    public float recoilCooldown = 0.05f; // 限制最多每秒震 20 次
    private float lastRecoilTime;

    private void Awake()
    {
        if (Instance == null) Instance = this; 
        else Destroy(gameObject); 
    }

    public void ShakeRecoil(Vector3 worldDirection, float force = 1f)
    {
        if (recoilSource == null) return; 

        // 冷却拦截，防止霰弹枪瞬间多次调用导致震动风暴
        if (Time.time - lastRecoilTime < recoilCooldown) return;
        lastRecoilTime = Time.time;

        Vector3 screenShakeDir = new Vector3(worldDirection.x, worldDirection.z, 0f);
        recoilSource.GenerateImpulseWithVelocity(screenShakeDir.normalized * force); 
    }

    public void ShakeExplosion(float force = 1f)
    {
        if (explosionSource == null) return; 
        explosionSource.GenerateImpulseWithForce(force);
    }
}