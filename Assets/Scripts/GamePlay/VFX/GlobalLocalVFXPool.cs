using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 全局本地特效池 (纯客户端视觉，不参与网络同步)
/// </summary>
public class GlobalLocalVFXPool : MonoBehaviour
{
    public static GlobalLocalVFXPool Instance { get; private set; }

    [System.Serializable]
    public struct VFXRegistry
    {
        public string id;             // 比如 "BloodSplash", "BulletHitMetal"
        public GameObject prefab;     // 注意：必须挂载了 VFXAutoReturn 脚本！
        public int defaultCapacity;   // 默认池子大小，比如子弹火花建议 20，爆血建议 10
    }

    [Header("本地特效注册表")]
    public List<VFXRegistry> registries = new List<VFXRegistry>();

    // 核心数据结构：一个大字典，里面装满了各种特效的独立对象池
    private Dictionary<string, ObjectPool<GameObject>> vfxPools = new Dictionary<string, ObjectPool<GameObject>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var reg in registries)
        {
            if (string.IsNullOrEmpty(reg.id) || reg.prefab == null) continue;

            // 创建 Unity 原生的 ObjectPool
            var pool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject obj = Instantiate(reg.prefab, this.transform);
                    // 确保预制件上挂了回收脚本，并且 ID 填对了
                    var autoReturn = obj.GetComponent<VFXAutoReturn>();
                    if (autoReturn == null) Debug.LogError($"特效 {reg.id} 缺少 VFXAutoReturn 脚本！");
                    else autoReturn.vfxId = reg.id;
                    return obj;
                },
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                defaultCapacity: reg.defaultCapacity,
                maxSize: 100 // 防止池子无限膨胀
            );

            vfxPools.Add(reg.id, pool);
        }
    }

    /// <summary>
    /// 【极度高频被调用的接口】在指定位置播放特效
    /// </summary>
    public void GetVFX(string id, Vector3 position, Quaternion rotation = default)
    {
        if (vfxPools.TryGetValue(id, out var pool))
        {
            GameObject vfxObj = pool.Get();
            vfxObj.transform.position = position;

            // 如果传入了旋转就用传入的，否则保持预制件的默认旋转
            if (rotation != default) vfxObj.transform.rotation = rotation;

            // 特效的粒子播放 (如果有 ParticleSystem，通常在 OnEnable 时会自动 Play，
            // 如果你的特效没勾选 Play On Awake，可以在这里 GetComponent<ParticleSystem>().Play())
        }
        else
        {
            Debug.LogWarning($"[VFX Pool] 找不到 ID 为 {id} 的特效！");
        }
    }

    /// <summary>
    /// 回收接口：通常由特效身上的 VFXAutoReturn 自动调用
    /// </summary>
    public void ReturnVFX(string id, GameObject vfxObj)
    {
        if (vfxPools.TryGetValue(id, out var pool))
        {
            pool.Release(vfxObj);
        }
    }
}