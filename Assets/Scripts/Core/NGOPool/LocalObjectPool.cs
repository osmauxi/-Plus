using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

[System.Serializable]
public class PoolItemConfig
{
    public string ID;//用于标识该预制体的名称
    public GameObject prefab;//预制体
    public int iniAmount;//初始生成数量
}
[System.Serializable]
public class PoolGroupConfig
{
    [Header("当前分组的类型ID")]
    public string groupName;
    [Header("该类型下的所有等级注册项")]
    public List<PoolItemConfig> Items = new List<PoolItemConfig>();
}

//IObjectPool是Unity自己封装好的对象池接口，相对自己写来说更高效更安全
public class LocalObjectPool : MonoBehaviour
{
    public static LocalObjectPool instance;

    [Header("注册配置")]
    public List<PoolGroupConfig> typeRegistries = new List<PoolGroupConfig>();

    //反向查找表,预先存储每个实例的ID和它对应的对象池，这样返还时直接查它的ID就能知道它属于哪个池，不需要知道它的type和lev
    private Dictionary<int, IObjectPool<GameObject>> _instanceIdToPoolMap = new Dictionary<int, IObjectPool<GameObject>>();

    public Dictionary<string, IObjectPool<GameObject>> pool = new Dictionary<string, IObjectPool<GameObject>>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        InitializeDic();
        //DontDestroyOnLoad(gameObject);
    }

    private void InitializeDic()
    {
        foreach (var group in typeRegistries)
        {
            foreach (var item in group.Items)
            {
                if (string.IsNullOrEmpty(item.ID))
                {
                    Debug.LogError("Prefab缺少ID，在组: " + group.groupName);
                    continue;
                }

                if (pool.ContainsKey(item.ID))
                {
                    Debug.LogError("ID重复 : " + item.ID);
                    continue;
                }

                IObjectPool<GameObject> newPool = null;
                //createFunc:怎么造？告诉Handler怎么创建该物品的实例
                //actionOnGet:取出时做什么？在这个物品返回给请求者前做什么
                //actionOnRelease:放回时做什么？在这个物品被放回池子前做什么
                //actionOnDestroy:销毁时做什么？在这个物品被销毁前做什么
                //defaultCapacity:初始容量;
                newPool = new ObjectPool<GameObject>(
                    createFunc: () =>
                    {
                        //创建时顺便登记这个实例的ID和它所属的池
                        GameObject obj = Instantiate(item.prefab);
                        _instanceIdToPoolMap[obj.GetInstanceID()] = newPool;
                        return obj;
                    },
                    actionOnGet: (obj) => obj.gameObject.SetActive(true),
                    actionOnRelease: (obj) => {
                        obj.gameObject.SetActive(false);
                        obj.transform.SetParent(transform);
                    },
                    actionOnDestroy: (obj) => Destroy(obj.gameObject),
                    defaultCapacity: item.iniAmount
                );

                pool.Add(item.ID, newPool);
            }
        }
    }

    //按名称查找
    public GameObject GetT(string keyName, Vector3 position, Transform parent)
    {
        if (pool.TryGetValue(keyName, out var targetPool))
        //确认此name对象池存在
        {
            GameObject obj = targetPool.Get();
            obj.transform.position = position;
            if (parent != null)
                obj.transform.SetParent(parent);
            return obj;
        }

        Debug.LogError($"未注册的本地单位：" + keyName);
        return null;
    }
    public void RetToPool(GameObject obj)
    {
        if (obj == null)
            return;

        int id = obj.GetInstanceID();

        if (_instanceIdToPoolMap.TryGetValue(id, out var targetPool))
        {
            targetPool.Release(obj);
        }
        else
        {
            Debug.LogWarning("未登记的非法对象");
            Destroy(obj);
        }
    }
}