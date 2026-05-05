using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

public class SyncObjectPool : NetworkBehaviour
{
    public static SyncObjectPool instance;
    //networkPrefabRegistries中方便注册进行分组，便于查找和注册，但代码实际使用的pool中是乱序的，只给代码引用所以无所谓
    [Header("配置的所有预制体注册信息")]
    public List<PoolGroupConfig> networkPrefabRegistries = new List<PoolGroupConfig>();
    public Dictionary<string, IObjectPool<NetworkObject>> pool = new Dictionary<string, IObjectPool<NetworkObject>>();
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    public override void OnNetworkSpawn()
    {
        InitializeDic();
    }
    private void InitializeDic()
    {
        //遍历所有类型分组，注册每组的预制体
        foreach (var group in networkPrefabRegistries)
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
                //尝试获取NetworkObject组件，确保该预制体可以用于网络同步，有的话写入netPrefab变量给下方使用
                if (!item.prefab.TryGetComponent(out NetworkObject netPrefab))
                {
                    Debug.LogError("预制体缺少NetworkObject组件，ID ： " + item.ID);
                    continue;
                }
                //createFunc:怎么造？告诉Handler怎么创建该物品的实例
                //actionOnGet:取出时做什么？在这个物品返回给请求者前做什么
                //actionOnRelease:放回时做什么？在这个物品被放回池子前做什么
                //actionOnDestroy:销毁时做什么？在这个物品被销毁前做什么
                //defaultCapacity:初始容量;
                var newPool = new ObjectPool<NetworkObject>(
                    createFunc: () => Instantiate(netPrefab),
                    actionOnGet: (obj) => { obj.gameObject.SetActive(true);obj.transform.SetParent(null); },
                    actionOnRelease: (obj) => { obj.gameObject.SetActive(false); obj.transform.SetParent(this.transform); },
                    actionOnDestroy: (obj) => Destroy(obj.gameObject),
                    defaultCapacity: item.iniAmount
                );

                //注册这种物体，之后只要有生成这个Prefab的需求，都会转到handler这边进行处理
                PooledPrefabInstanceHandler handler = new PooledPrefabInstanceHandler(netPrefab, newPool);
                NetworkManager.Singleton.PrefabHandler.AddHandler(item.prefab, handler);

                pool.Add(item.ID, newPool);
            }
        }
    }
    public void RegisterDynamicPrefab(string id, NetworkObject netPrefab, int iniAmount)
    {
        if (string.IsNullOrEmpty(id) || netPrefab == null) return;

        if (pool.ContainsKey(id))
        {
            Debug.LogWarning($"[对象池] ID {id} 已存在，跳过动态注册。");
            return;
        }

        var newPool = new ObjectPool<NetworkObject>(
            createFunc: () => Instantiate(netPrefab),
            actionOnGet: (obj) => obj.gameObject.SetActive(true),
            actionOnRelease: (obj) => obj.gameObject.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj.gameObject),
            defaultCapacity: iniAmount
        );

        PooledPrefabInstanceHandler handler = new PooledPrefabInstanceHandler(netPrefab, newPool);
        NetworkManager.Singleton.PrefabHandler.AddHandler(netPrefab.gameObject, handler);

        pool.Add(id, newPool);
        Debug.Log($"[对象池] 成功动态注册外部管线对象: {id}");
    }

    public NetworkObject GetT(string id, Vector3 pos, Quaternion rot)
    {
        if (!IsServer)
            return null;

        if (pool.TryGetValue(id, out var _pool))
        {
            var obj = _pool.Get();
            obj.transform.position = pos;
            obj.transform.rotation = rot;
            obj.Spawn(true);
            return obj;
        }

        Debug.LogError($"找不到 ID 为 '{id}' 的对象池！请检查 Inspector 配置。");
        return null;
    }
    public void RetToPool(NetworkObject obj, string poolId)
    {
        if (!IsServer)
            return;

        if (obj.IsSpawned)
        {
            obj.Despawn(false);
        }

        // 2. 【核心修复】：Server 必须手动将物体放回自己的池子！
        if (pool.TryGetValue(poolId, out var _pool))
        {
            _pool.Release(obj); // 这会触发 actionOnRelease，执行 SetActive(false)
        }
        else
        {
            Debug.LogError($"[对象池错误] Server 找不到 ID 为 '{poolId}' 的对象池，只能强制销毁！");
            Destroy(obj.gameObject); // 兜底处理
        }
    }
}

public class PooledPrefabInstanceHandler : INetworkPrefabInstanceHandler
{
    private NetworkObject _prefab;
    private IObjectPool<NetworkObject> _pool;

    public PooledPrefabInstanceHandler(NetworkObject prefab, IObjectPool<NetworkObject> pool)
    {
        _prefab = prefab;
        _pool = pool;
    }

    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        //客户端收到生成消息时，会跑这里，直接从池里拿
        var obj = _pool.Get();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        return obj;
    }

    public void Destroy(NetworkObject networkObject)
    {
        //客户端收到销毁消息时，会跑这里，放回池子
        _pool.Release(networkObject);
    }
}
