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
        public string id;
        public GameObject prefab;
        public int defaultCapacity;
    }

    [Header("本地特效注册表")]
    public List<VFXRegistry> registries = new List<VFXRegistry>();

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

            var pool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject obj = Instantiate(reg.prefab, this.transform);
                    var autoReturn = obj.GetComponent<VFXAutoReturn>();
                    if (autoReturn == null) Debug.LogError($"特效 {reg.id} 缺少 VFXAutoReturn 脚本！");
                    else autoReturn.vfxId = reg.id;
                    return obj;
                },
                actionOnGet: (obj) => {
                    obj.transform.SetParent(null);
                },
                actionOnRelease: (obj) => {
                    obj.SetActive(false);
                    // 【修复 1】：放回池子时统一挂回父节点，保证 Hierarchy 面板整洁
                    obj.transform.SetParent(this.transform);
                },
                actionOnDestroy: (obj) => Destroy(obj),
                defaultCapacity: reg.defaultCapacity,
                maxSize: 100
            );

            vfxPools.Add(reg.id, pool);

            // ==========================================
            // 【核心修复 2】：强行预热池子 (Pre-warm)
            // ==========================================
            var prewarmList = new List<GameObject>();
            for (int i = 0; i < reg.defaultCapacity; i++)
            {
                prewarmList.Add(pool.Get()); // 强行生成
            }
            foreach (var obj in prewarmList)
            {
                pool.Release(obj); // 立刻塞回去
            }
        }
    }

    /// <summary>
    /// 【极度高频被调用的接口】在指定位置播放特效
    /// </summary>
    public void GetVFX(string id, Vector3 position, Quaternion rotation = default, float weight = 1f)
    {
        if (vfxPools.TryGetValue(id, out var pool))
        {
            GameObject vfxObj = pool.Get();

            // ==========================================
            // 【核心修复 3】：状态清洗与严格的生命周期顺序
            // ==========================================

            // 第一步：先摆正位置和朝向（绝对不能先 SetActive）
            vfxObj.transform.position = position;
            if (rotation != default) vfxObj.transform.rotation = rotation;

            // 第二步：清洗残留的物理状态（防止掉入地下！）
            if (vfxObj.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // 第三步：激活物体
            vfxObj.SetActive(true);

            // 第四步：清理拖尾和粒子残影（防止空间瞬移产生一条长长的尾迹）
            var trails = vfxObj.GetComponentsInChildren<TrailRenderer>();
            foreach (var trail in trails)
            {
                trail.Clear();
            }

            var pss = vfxObj.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in pss)
            {
                // 如果没有开启 PlayOnAwake，就手动播一下
                if (!ps.main.playOnAwake) ps.Play();
            }

            // 第五步：处理尺寸和权重
            if (vfxObj.TryGetComponent<VFXImpactScaler>(out var scaler))
            {
                scaler.SetImpactWeight(weight);
            }
            else
            {
                vfxObj.transform.localScale = Vector3.one * weight;
            }
        }
        else
        {
            Debug.LogWarning($"[VFX Pool] 找不到 ID 为 {id} 的特效！");
        }
    }

    public void ReturnVFX(string id, GameObject vfxObj)
    {
        if (vfxPools.TryGetValue(id, out var pool))
        {
            vfxObj.transform.localScale = Vector3.one;
            pool.Release(vfxObj);
        }
    }
}