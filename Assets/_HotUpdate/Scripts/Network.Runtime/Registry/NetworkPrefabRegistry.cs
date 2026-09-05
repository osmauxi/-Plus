using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectGame.HotFix.Network.Runtime
{
    /// <summary>
    /// 负责上层NetworkPrefab的Addressables加载与NGO运行时注册
    /// 主要任务为注册这个网络组件进NGO，以及从Addressable加载Prefab
    /// </summary>
    public sealed class NetworkPrefabRegistry
    {
        private readonly NetworkPrefabCatalog _catalog;
        private readonly NetworkManager _networkManager;
        //运行时状态，包含Addressables加载Handle、Prefab引用、是否已注册进NGO等
        private readonly Dictionary<NetworkPrefabId, NetworkPrefabRuntimeState> _states = new Dictionary<NetworkPrefabId, NetworkPrefabRuntimeState>();
        private readonly Dictionary<NetworkPrefabId, PersistentSeedHandler> _seeds = new();
      
        public NetworkPrefabRegistry(NetworkPrefabCatalog catalog, NetworkManager networkManager)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));

            _catalog.ValidateOrThrow();

            if (_networkManager.NetworkConfig.ForceSamePrefabs)
                throw new InvalidOperationException("动态NetworkPrefab管线要求NetworkConfig.ForceSamePrefabs = false");

            foreach (NetworkPrefabEntry entry in _catalog.Entries)
                _states.Add(entry.Id, new NetworkPrefabRuntimeState(entry));
        }

        /// <summary>
        /// 确保目标Prefab已通过Addressables加载并注册进本机NGO
        /// 重复调用已准备完成的Prefab不会重复加载或注册
        /// </summary>
        public async UniTask<GameObject> PrepareAsync(NetworkPrefabId id, CancellationToken cancellationToken)
        {
            NetworkPrefabRuntimeState state = GetState(id);
            if (state.IsRegistered)
                return state.Prefab;

            if (state.Handle.IsValid())
                throw new InvalidOperationException($"NetworkPrefab {id} 正处于未完成的加载状态，禁止重复 Prepare");

            AsyncOperationHandle<GameObject> handle = state.Entry.Prefab.LoadAssetAsync<GameObject>();

            state.Handle = handle;

            try
            {
                //等待加载完成，期间允许取消
                await WaitForCompletionAsync(handle, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
                    throw handle.OperationException ?? new InvalidOperationException($"Addressables 加载 NetworkPrefab 失败：{id}");

                GameObject prefab = handle.Result;
                ValidateNetworkPrefab(id, prefab);
                //注册进NGO的NetwrokPrefabList中，NGO在同步时能获取此网络对象的信息
                _networkManager.AddNetworkPrefab(prefab);

                state.Prefab = prefab;
                state.IsRegistered = true;

                Debug.Log($"[NetworkPrefabRegistry] NetworkPrefab 准备完成：{id}");
                return prefab;
            }
            catch
            {
                if (state.IsRegistered && state.Prefab != null)
                    _networkManager.RemoveNetworkPrefab(state.Prefab);

                if (state.Handle.IsValid())
                    Addressables.Release(state.Handle);

                state.Reset();
                throw;
            }
        }
        /// <summary>
        /// 获取已经完成 Prepare 的 NetworkPrefab。
        /// </summary>
        public GameObject GetPrefab(NetworkPrefabId id)
        {
            NetworkPrefabRuntimeState state = GetState(id);
            if (!state.IsRegistered || state.Prefab == null)
                throw new InvalidOperationException($"NetworkPrefab 尚未准备完成：{id}");

            return state.Prefab;
        }

        public bool IsPrepared(NetworkPrefabId id)
        {
            NetworkPrefabRuntimeState state = GetState(id);
            return state.IsRegistered && state.Prefab != null;
        }

        /// <summary>
        /// 注销 NGO Prefab 并释放 Addressables 引用。
        /// 调用方必须保证该 Prefab 创建出的 NetworkObject 已全部 Despawn。
        /// </summary>
        public void Release(NetworkPrefabId id)
        {
            NetworkPrefabRuntimeState state = GetState(id);

            if (state.IsRegistered && state.Prefab != null)
                _networkManager.RemoveNetworkPrefab(state.Prefab);

            if (state.Handle.IsValid())
                Addressables.Release(state.Handle);

            state.Reset();
            Debug.Log($"[NetworkPrefabRegistry] NetworkPrefab 已释放：{id}");
        }

        public void ReleaseAll(bool retainSessionSeeds = false)
        {
            foreach (NetworkPrefabRuntimeState state in _states.Values)
            {
                if (retainSessionSeeds && _seeds.ContainsKey(state.Entry.Id)) continue;
                if (_seeds.TryGetValue(state.Entry.Id, out PersistentSeedHandler seed))
                {
                    _networkManager.PrefabHandler.RemoveHandler(state.Prefab);
                    if (seed.Instance != null) UnityEngine.Object.Destroy(seed.Instance.gameObject);
                    _seeds.Remove(state.Entry.Id);
                }
                if (state.IsRegistered && state.Prefab != null)
                    _networkManager.RemoveNetworkPrefab(state.Prefab);

                if (state.Handle.IsValid())
                    Addressables.Release(state.Handle);

                state.Reset();
            }

            Debug.Log("[NetworkPrefabRegistry] 已释放全部 NetworkPrefab");
        }

        private NetworkPrefabRuntimeState GetState(NetworkPrefabId id)
        {
            if (!_states.TryGetValue(id, out NetworkPrefabRuntimeState state))
                throw new KeyNotFoundException($"NetworkPrefabCatalog 中不存在 Id：{id}");

            return state;
        }

        private static void ValidateNetworkPrefab(NetworkPrefabId id, GameObject prefab)
        {
            if (!prefab.TryGetComponent<NetworkObject>(out _))
                throw new InvalidOperationException($"NetworkPrefab {id} 的根节点没有 NetworkObject：{prefab.name}");
        }

        private static async UniTask WaitForCompletionAsync(AsyncOperationHandle<GameObject> handle, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            while (!handle.IsDone)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>Connection infrastructure exists locally before NGO starts; its prefab handler reuses it.</summary>
        public async UniTask<NetworkObject> PreparePersistentSeedAsync(NetworkPrefabId id, CancellationToken cancellationToken)
        {
            if (_seeds.TryGetValue(id, out PersistentSeedHandler existing)) return existing.Instance;
            if (_networkManager.IsListening) throw new InvalidOperationException("会话种子必须在联网前准备");
            if (GetState(id).Entry.Lifetime != NetworkPrefabLifetime.Persistent)
                throw new InvalidOperationException("只有 Persistent Root 可以作为会话种子");
            GameObject prefab = await PrepareAsync(id, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            NetworkObject instance = UnityEngine.Object.Instantiate(prefab).GetComponent<NetworkObject>();
            UnityEngine.Object.DontDestroyOnLoad(instance.gameObject);
            // NGO 启动时会扫描所有 active NetworkObject。种子保持 inactive，启动回调中显式 Spawn。
            instance.gameObject.SetActive(false);
            var handler = new PersistentSeedHandler(instance);
            if (!_networkManager.PrefabHandler.AddHandler(prefab, handler))
            {
                UnityEngine.Object.Destroy(instance.gameObject);
                throw new InvalidOperationException($"{id} 已存在 PrefabHandler");
            }
            _seeds.Add(id, handler);
            return instance;
        }

        public NetworkObject GetPersistentSeed(NetworkPrefabId id)
            => _seeds.TryGetValue(id, out PersistentSeedHandler handler) ? handler.Instance
                : throw new InvalidOperationException($"尚未准备会话种子：{id}");

        private sealed class PersistentSeedHandler : INetworkPrefabInstanceHandler
        {
            public NetworkObject Instance { get; }
            public PersistentSeedHandler(NetworkObject instance) => Instance = instance;
            public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
            {
                if (Instance == null || Instance.IsSpawned)
                    throw new InvalidOperationException("会话种子失效或重复 Spawn");
                Instance.transform.SetPositionAndRotation(position, rotation);
                Instance.gameObject.SetActive(true);
                return Instance;
            }
            public void Destroy(NetworkObject networkObject)
            {
                // 保留离线 UI/连接审批使用的对象；下一次会话仍复用它。
                if (networkObject != null) networkObject.gameObject.SetActive(false);
            }
        }

    }
}
