using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// 挂在特效预制体根节点，播完自动还给对象池。完美兼容 ParticleSystem 和 VFX Graph！
/// </summary>
public class VFXAutoReturn : MonoBehaviour
{
    [HideInInspector] public string vfxId;

    [Tooltip("如果特效是无限循环的，强制多少秒后回收？(0表示必须等特效自然播完)")]
    public float forceLifeTime = 0f;

    private ParticleSystem[] pss;
    private VisualEffect[] vfxs;
    private float timer;

    private void Awake()
    {
        pss = GetComponentsInChildren<ParticleSystem>();
        vfxs = GetComponentsInChildren<VisualEffect>();
    }

    private void OnEnable()
    {
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // 1. 强制生命周期兜底 (针对循环特效，如燃烧的火焰)
        if (forceLifeTime > 0f && timer >= forceLifeTime)
        {
            ReturnToPool();
            return;
        }

        // 2. 如果强制周期为 0，则智能检测是否所有粒子都死光了
        if (forceLifeTime <= 0f)
        {
            bool isStillPlaying = false;

            // 检查老粒子系统
            foreach (var ps in pss)
            {
                if (ps.IsAlive(true)) { isStillPlaying = true; break; }
            }

            // 检查 VFX Graph
            if (!isStillPlaying)
            {
                foreach (var vfx in vfxs)
                {
                    // aliveCount 只要大于 0，就说明还有粒子在屏幕上
                    if (vfx.aliveParticleCount > 0) { isStillPlaying = true; break; }
                }
            }

            // 如果全死光了，安全回收
            if (!isStillPlaying && timer > 0.1f) // 给0.1秒的启动缓冲期
            {
                ReturnToPool();
            }
        }
    }

    private void ReturnToPool()
    {
        // 在彻底关闭前，让 VFX 停止生成新粒子
        foreach (var vfx in vfxs) vfx.Stop();

        if (GlobalLocalVFXPool.Instance != null && !string.IsNullOrEmpty(vfxId))
        {
            GlobalLocalVFXPool.Instance.ReturnVFX(vfxId, this.gameObject);
        }
        else
        {
            gameObject.SetActive(false); // 兜底：如果没有池子，直接隐藏
        }
    }
}