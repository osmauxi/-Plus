using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;

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
                    //var autoReturn = obj.GetComponent<VFXAutoReturn>();
                    //if (autoReturn == null) Debug.LogError($"特效 {reg.id} 缺少 VFXAutoReturn 脚本！");
                    //else autoReturn.vfxId = reg.id;
                    return obj;
                },
                actionOnGet: (obj) => {
                    obj.transform.SetParent(null);
                },
                actionOnRelease: (obj) => {
                    obj.SetActive(false);
                    obj.transform.SetParent(this.transform);
                },
                actionOnDestroy: (obj) => Destroy(obj),
                defaultCapacity: reg.defaultCapacity,
                maxSize: 100
            );

            vfxPools.Add(reg.id, pool);

            var prewarmList = new List<GameObject>();
            for (int i = 0; i < reg.defaultCapacity; i++)
            {
                prewarmList.Add(pool.Get());
            }
            foreach (var obj in prewarmList)
            {
                pool.Release(obj);
            }
        }
    }

    /// <summary>
    /// 【升级】：现在返回 GameObject，方便外部进行非等比拉伸等高级操作！
    /// </summary>
    public GameObject GetVFX(string id, Vector3 position, Quaternion rotation = default, float weight = 1f)
    {
        if (vfxPools.TryGetValue(id, out var pool))
        {
            GameObject vfxObj = pool.Get();

            vfxObj.transform.position = position;
            if (rotation != default) vfxObj.transform.rotation = rotation;

            if (vfxObj.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            vfxObj.SetActive(true);

            var trails = vfxObj.GetComponentsInChildren<TrailRenderer>();
            foreach (var trail in trails) trail.Clear();

            var pss = vfxObj.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in pss)
            {
                if (!ps.main.playOnAwake) ps.Play();
            }

            var vfxGraphs = vfxObj.GetComponentsInChildren<VisualEffect>();
            foreach (var vfx in vfxGraphs)
            {
                vfx.Reinit();
                vfx.SendEvent(Shader.PropertyToID(id));
            }

            // 每次从池子里拿出来时，默认恢复等比缩放 (不用担心被之前的拉伸污染)
            //if (vfxObj.TryGetComponent<VFXImp actScaler>(out var scaler))
            //{
            //    scaler.SetImpactWeight(weight);
            //}
            //else
            //{
            //    vfxObj.transform.localScale = Vector3.one * weight;
            //}

            return vfxObj; // 返回生成的特效物体
        }
        else
        {
            Debug.LogWarning($"[VFX Pool] 找不到 ID 为 {id} 的特效！");
            return null;
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