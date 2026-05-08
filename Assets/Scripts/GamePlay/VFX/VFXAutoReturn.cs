using UnityEngine;

/// <summary>
/// 挂在特效 Prefab 上的自动回收器
/// 职责：监听特效播放结束，自动把自己还给全局特效池
/// </summary>
public class VFXAutoReturn : MonoBehaviour
{
    [Tooltip("特效池中的唯一 ID，必须与 GlobalLocalVFXPool 中的注册一致")]
    public string vfxId;

    [Tooltip("特效持续时间。如果是粒子，建议设为粒子的 Duration")]
    public float lifetime = 1.0f;

    private float timer;

    // 当特效从对象池里被拿出来（SetActive(true)）时，会自动触发 OnEnable
    private void OnEnable()
    {
        timer = lifetime;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (GlobalLocalVFXPool.Instance != null)
        {
            GlobalLocalVFXPool.Instance.ReturnVFX(vfxId, this.gameObject);
        }
        else
        {
            // 兜底：如果池子被毁了（比如切场景），就直接销毁
            Destroy(gameObject);
        }
    }
}