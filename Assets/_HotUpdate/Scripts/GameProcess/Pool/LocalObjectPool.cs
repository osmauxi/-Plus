using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Gameplay.Runtime;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectGame.HotFix.Gameplay.Pooling
{
    /// <summary>
    /// GameRuntime范围内的本地对象池。
    ///
    /// 生命周期：
    /// 1. Initialize登记池配置，但不加载Prefab
    /// 2. 通过Addressables加载Prefab，创建并预热池
    /// 3. Rent / Return：同步租借和返还实例
    /// 4. Shutdown：销毁全部实例，再释放 Addressables Handle。
    /// </summary>
    public sealed class LocalObjectPool : MonoBehaviour, IGameRuntimeService
    {
        public static LocalObjectPool Instance { get; private set; }

        [Tooltip("池内闲置对象的父节点。")]
        [SerializeField] private Transform _inactiveRoot;

        /// <summary>
        /// PoolId → 池的完整运行时信息。
        /// 相比于普通对象池额外保存了配置、Prefab、Addressables Handle和准备状态。
        /// </summary>
        private readonly Dictionary<string, PoolEntry> _entriesById = new(StringComparer.Ordinal);

        /// <summary>
        /// 实例ID → 所属池。
        /// Return 时不需要调用方再次传入PoolId。
        /// </summary>
        private readonly Dictionary<int, PoolEntry> _entryByInstanceId = new();

        /// <summary>
        /// 所有由当前对象池创建的实例。
        /// 包括闲置实例和当前租出实例。
        /// </summary>
        private readonly Dictionary<int, GameObject> _instances = new();

        /// <summary>
        /// 创建实例时缓存IPoolable，避免每次租借时扫描组件。
        /// </summary>
        private readonly Dictionary<int, IPoolable[]> _poolableCallbacks = new();

        /// <summary>
        /// 当前处于池外使用状态的实例。
        /// 同时用于阻止重复 Rent 和重复 Return。
        /// </summary>
        private readonly HashSet<int> _rentedInstanceIds = new();

        /// <summary>
        /// Pool自身的生命周期Token。
        /// Shutdown时取消所有仍在进行的Addressables Prepare。
        /// </summary>
        private CancellationTokenSource _lifetimeCts;

        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 已登记配置的池数量，不代表这些池已经完成资源加载。
        /// </summary>
        public int PoolCount => _entriesById.Count;

        public int RentedCount => _rentedInstanceIds.Count;

        private Transform InactiveRoot => _inactiveRoot != null ? _inactiveRoot : transform;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError($"[{nameof(LocalObjectPool)}] 场景中存在重复实例。");
                Destroy(this);
                return;
            }

            Instance = this;
        }

        /// <summary>
        /// 只登记池定义，不加载Addressable Prefab。
        /// </summary>
        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return UniTask.CompletedTask;

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                _lifetimeCts = new CancellationTokenSource();

                RegisterConfiguredDefinitions(cancellationToken);

                IsInitialized = true;

                Debug.Log($"[{nameof(LocalObjectPool)}] 初始化完成，已登记 {_entriesById.Count} 个对象池定义。");
                return UniTask.CompletedTask;
            }
            catch
            {
                ShutdownInternal();
                throw;
            }
        }

        public async UniTask PreparePoolAsync(string poolId, CancellationToken cancellationToken)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(poolId))
                throw new ArgumentException("PoolId 不能为空。", nameof(poolId));

            if (!_entriesById.TryGetValue(poolId, out PoolEntry entry))
                throw new KeyNotFoundException($"没有登记对象池定义：{poolId}");

            if (entry.IsPrepared)
                return;

            //已经有调用方在准备该池时，后续调用方只等待同一个结果，
            //不重复创建 Handle 和 ObjectPool。
            if (!entry.IsPreparing)
            {
                entry.IsPreparing = true;
                entry.PrepareCompletion = new UniTaskCompletionSource();

                //真正的加载只绑定对象池生命周期。
                PrepareEntryAsync(entry, _lifetimeCts.Token).Forget();
            }
            //等待先进行的任务完成，把自己的CancellationToken也绑定上去。
            //当前调用方取消，只停止自己的等待。
            await entry.PrepareCompletion.Task.AttachExternalCancellation(cancellationToken);
        }
        private async UniTaskVoid PrepareEntryAsync(PoolEntry entry, CancellationToken cancellationToken)
        {
            AsyncOperationHandle<GameObject> handle = default;
            bool hasHandle = false;

            try
            {
                //AssetReference作为Addressables.LoadAssetAsync的Key直接指定加载对象。
                //Pool保存返回的Handle，直到Shutdown时释放。
                handle = Addressables.LoadAssetAsync<GameObject>(entry.Config.PrefabAddress);
                hasHandle = true;

                GameObject prefab = await handle.ToUniTask(cancellationToken: cancellationToken);

                if (handle.Status != AsyncOperationStatus.Succeeded || prefab == null)
                    throw new InvalidOperationException($"Addressable Prefab 加载失败：Pool={entry.Config.Id}");

                entry.Prefab = prefab;
                entry.PrefabHandle = handle;
                entry.HasPrefabHandle = true;
                //这个资源预制件被加载出来之后，进行这个池子的注册，创建和初始化，之后就可以正常使用了。
                CreateRuntimePool(entry);
                PrewarmPool(entry.Pool, entry.Config.InitialCapacity);

                entry.IsPrepared = true;

                entry.PrepareCompletion.TrySetResult();

                Debug.Log($"[{nameof(LocalObjectPool)}] Pool 准备完成：{entry.Config.Id}，Initial={entry.Config.InitialCapacity}，Max={entry.Config.MaxSize}");
            }
            catch (Exception exception)
            {
                //创建池或Prewarm中途失败时，回收已经产生的实例。
                RollbackPrepare(entry);
                
                //如果Handle还没有转移给 entry，
                //在这里释放当前局部 Handle。
                if (hasHandle && !entry.HasPrefabHandle && handle.IsValid())
                    Addressables.Release(handle);

                entry.PrepareCompletion?.TrySetException(exception);
            }
            finally
            {
                entry.IsPreparing = false;
            }
        }
        private void RollbackPrepare(PoolEntry entry)
        {
            if (entry.Pool != null)
            {
                entry.Pool.Clear();
                entry.Pool = null;
            }

            entry.Prefab = null;
            entry.IsPrepared = false;
            entry.RentedCount = 0;

            if (entry.HasPrefabHandle && entry.PrefabHandle.IsValid())
                Addressables.Release(entry.PrefabHandle);

            entry.PrefabHandle = default;
            entry.HasPrefabHandle = false;
        }

        /// <summary>
        /// 批量准备一组池。
        /// 重复PoolId会被去重。
        /// </summary>
        public async UniTask PreparePoolsAsync(IEnumerable<string> poolIds, CancellationToken cancellationToken)
        {
            EnsureInitialized();

            if (poolIds == null)
                throw new ArgumentNullException(nameof(poolIds));

            HashSet<string> uniquePoolIds = new(StringComparer.Ordinal);

            foreach (string poolId in poolIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!string.IsNullOrWhiteSpace(poolId))
                    uniquePoolIds.Add(poolId);
            }

            if (uniquePoolIds.Count == 0)
                return;

            List<UniTask> prepareTasks = new(uniquePoolIds.Count);

            foreach (string poolId in uniquePoolIds)
                prepareTasks.Add(PreparePoolAsync(poolId, cancellationToken));

            await UniTask.WhenAll(prepareTasks);
        }

        /// <summary>
        /// 准备配置中登记的全部池。
        /// 一般只用于明确需要全量预加载的场景。
        /// </summary>
        public UniTask PrepareAllPoolsAsync(CancellationToken cancellationToken)
        {
            return PreparePoolsAsync(_entriesById.Keys, cancellationToken);
        }

        /// <summary>
        /// 从已经完成 Prepare 的池中同步租借对象。
        /// Rent 不负责触发 Addressables 异步加载。
        /// </summary>
        public GameObject Rent(string poolId, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(poolId))
                throw new ArgumentException("PoolId 不能为空。", nameof(poolId));

            if (!_entriesById.TryGetValue(poolId, out PoolEntry entry))
                throw new KeyNotFoundException($"没有登记对象池定义：{poolId}");

            if (!entry.IsPrepared || entry.Pool == null)
                throw new InvalidOperationException($"对象池尚未 Prepare，不能 Rent：{poolId}");

            GameObject instance = entry.Pool.Get();

            if (instance == null)
                throw new InvalidOperationException($"对象池返回了空实例：{poolId}");

            int instanceId = instance.GetInstanceID();

            if (!_rentedInstanceIds.Add(instanceId))
                throw new InvalidOperationException($"实例被重复租出：{instance.name} ({instanceId})");

            entry.RentedCount++;

            Transform instanceTransform = instance.transform;
            //先重置业务状态，再激活对象。
            //这样 OnEnable 看到的是已经清理过的实例。
            instanceTransform.SetParent(parent, false);
            instanceTransform.SetPositionAndRotation(position, rotation);
            instanceTransform.localScale = Vector3.one;

            InvokeRentCallbacks(instanceId);
            instance.SetActive(true);

            return instance;
        }

        public GameObject Rent(string poolId, Vector3 position, Transform parent = null)
        {
            return Rent(poolId, position, Quaternion.identity, parent);
        }

        /// <summary>
        /// 将实例返还给其所属对象池。
        /// </summary>
        public void Return(GameObject instance)
        {
            if (instance == null)
                return;

            if (!IsInitialized)
            {
                Debug.LogWarning($"[{nameof(LocalObjectPool)}] 对象池已经关闭，直接销毁对象：{instance.name}");
                Destroy(instance);
                return;
            }

            int instanceId = instance.GetInstanceID();

            if (!_entryByInstanceId.TryGetValue(instanceId, out PoolEntry entry))
            {
                Debug.LogWarning($"[{nameof(LocalObjectPool)}] 对象不属于当前对象池，直接销毁：{instance.name}");
                Destroy(instance);
                return;
            }

            if (!_rentedInstanceIds.Remove(instanceId))
            {
                Debug.LogWarning($"[{nameof(LocalObjectPool)}] 对象可能已经被返还：{instance.name} ({instanceId})");
                return;
            }

            entry.RentedCount--;
            if (entry.RentedCount < 0)
            {
                entry.RentedCount = 0;
                Debug.LogError($"[{nameof(LocalObjectPool)}] Pool={entry.Config.Id} 的 RentedCount 出现异常。");
            }


            InvokeReturnCallbacks(instanceId);
            entry.Pool.Release(instance);
        }

        /// <summary>
        /// 卸载一个已经Prepare的对象池。
        /// 要求当前池没有任何租出实例。
        /// 成功后Poo配置仍然存在，可以再次 Prepare。
        /// </summary>
        public bool ReleasePool(string poolId)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(poolId))
                throw new ArgumentException("PoolId 不能为空。", nameof(poolId));

            if (!_entriesById.TryGetValue(poolId, out PoolEntry entry))
                throw new KeyNotFoundException($"没有登记对象池定义：{poolId}");

            if (!CanReleaseEntry(entry, true))
                return false;

            ReleasePreparedEntry(entry);

            Debug.Log($"[{nameof(LocalObjectPool)}] Pool 已释放：{poolId}");

            return true;
        }
        /// <summary>
        /// 批量卸载已经Prepare的对象池
        /// </summary>
        public bool ReleasePools(IEnumerable<string> poolIds)
        {
            EnsureInitialized();

            if (poolIds == null)
                throw new ArgumentNullException(nameof(poolIds));

            HashSet<string> uniquePoolIds = new(StringComparer.Ordinal);

            foreach (string poolId in poolIds)
            {
                if (!string.IsNullOrWhiteSpace(poolId))
                    uniquePoolIds.Add(poolId);
            }

            if (uniquePoolIds.Count == 0)
                return true;

            //这里先遍历保证能够释放所有池，如果有一个池不满足条件，就不释放任何池。
            foreach (string poolId in uniquePoolIds)
            {
                if (!_entriesById.TryGetValue(poolId, out PoolEntry entry))
                    throw new KeyNotFoundException($"没有登记对象池定义：{poolId}");

                if (!CanReleaseEntry(entry, true))
                    return false;
            }

            //统一释放所有池
            foreach (string poolId in uniquePoolIds)
            {
                PoolEntry entry = _entriesById[poolId];
                ReleasePreparedEntry(entry);
            }

            Debug.Log($"[{nameof(LocalObjectPool)}] 批量释放完成，PoolCount={uniquePoolIds.Count}");

            return true;
        }

        public bool ReleaseAllPreparedPools()
        {
            EnsureInitialized();

            foreach (PoolEntry entry in _entriesById.Values)
            {
                if (!CanReleaseEntry(entry, true))
                    return false;
            }

            foreach (PoolEntry entry in _entriesById.Values)
                ReleasePreparedEntry(entry);

            Debug.Log($"[{nameof(LocalObjectPool)}] 所有已准备 Pool 均已释放。");

            return true;
        }

        private void ReleasePreparedEntry(PoolEntry entry)
        {
            if (!entry.IsPrepared)
                return;

            //此时RentedCount必须为 0。
            //所有实例都在池内，所以Clear能销毁全部实例。
            if (entry.Pool != null)
            {
                entry.Pool.Clear();
                entry.Pool = null;
            }

            //Clear会通过DestroyPooledInstance删除所有实例缓存。
            //实例已经不存在后，再释放Addressables Handle。
            entry.Prefab = null;

            if (entry.HasPrefabHandle && entry.PrefabHandle.IsValid())
                Addressables.Release(entry.PrefabHandle);

            entry.PrefabHandle = default;
            entry.HasPrefabHandle = false;

            entry.IsPrepared = false;
            entry.RentedCount = 0;
        }

        private bool CanReleaseEntry(PoolEntry entry, bool logError)
        {
            //没加载，本身就已经是Released状态。
            if (!entry.IsPrepared && !entry.IsPreparing)
                return true;

            if (entry.IsPreparing)
            {
                if (logError)
                    Debug.LogWarning($"[{nameof(LocalObjectPool)}] Pool 正在 Prepare，暂时不能 Release：{entry.Config.Id}");

                return false;
            }

            if (entry.RentedCount > 0)
            {
                if (logError)
                    Debug.LogWarning($"[{nameof(LocalObjectPool)}] Pool={entry.Config.Id} 仍有 {entry.RentedCount} 个实例在使用，不能 Release。");

                return false;
            }

            return true;
        }

        public int GetPoolRentedCount(string poolId)
        {
            EnsureInitialized();

            if (!_entriesById.TryGetValue(poolId, out PoolEntry entry))
                throw new KeyNotFoundException($"没有登记对象池定义：{poolId}");

            return entry.RentedCount;
        }

        /// <summary>
        /// 是否存在该PoolId的配置定义。
        /// 不代表该池已经完成Addressables加载。
        /// </summary>
        public bool ContainsPool(string poolId)
        {
            return !string.IsNullOrWhiteSpace(poolId) && _entriesById.ContainsKey(poolId);
        }

        /// <summary>
        /// 是否已经完成Prefab加载、对象池创建和Prewarm。
        /// </summary>
        public bool IsPoolPrepared(string poolId)
        {
            return !string.IsNullOrWhiteSpace(poolId) &&
                   _entriesById.TryGetValue(poolId, out PoolEntry entry) &&
                   entry.IsPrepared;
        }

        public async UniTask ShutdownAsync(CancellationToken cancellationToken)
        {    
            //Shutdown不能被外部Token中途打断。
            //先取消池自身的加载任务，再等待它们结束。
            _lifetimeCts?.Cancel();

            List<UniTask> pendingTasks = new();

            foreach (PoolEntry entry in _entriesById.Values)
            {
                if (entry.IsPreparing && entry.PrepareCompletion != null)
                    pendingTasks.Add(entry.PrepareCompletion.Task);
            }

            if (pendingTasks.Count > 0)
            {
                try
                {
                    await UniTask.WhenAll(pendingTasks);
                }
                catch
                {
                    // Prepare 自己负责 Handle 回滚。
                }
            }

            ShutdownInternal();
        }

        /// <summary>
        /// 初始化阶段只登记配置，不建立 ObjectPool。
        /// </summary>
        private void RegisterConfiguredDefinitions(CancellationToken cancellationToken)
        {
            Dictionary<int, Config_LocalObjectPool> table = ConfigManager.Instance.GetTable<Config_LocalObjectPool>();

            if (table == null)
                throw new InvalidOperationException("未加载 LocalObjectPool 配置表。");

            var configIds = new List<int>(table.Keys);
            configIds.Sort();

            for (int index = 0; index < configIds.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int configId = configIds[index];
                Config_LocalObjectPool row = table[configId];

                if (row == null)
                    throw new InvalidOperationException($"LocalObjectPool 配置为空，ConfigId={configId}");

                if (row.ConfigId != configId)
                    throw new InvalidOperationException(
                        $"LocalObjectPool 配置主键不一致：DictionaryKey={configId}，ConfigId={row.ConfigId}");

                var config = new PoolItemConfig(
                    row.PoolId,
                    row.PrefabAddress,
                    row.InitialCapacity,
                    row.MaxSize);

                RegisterDefinition(row.GroupName, config);
            }
        }

        private void RegisterDefinition(string groupName, PoolItemConfig config)
        {
            config.Validate(groupName);

            if (_entriesById.ContainsKey(config.Id))
                throw new InvalidOperationException($"PoolId 重复：{config.Id}");

            _entriesById.Add(config.Id, new PoolEntry
            {
                Config = config
            });
        }

        /// <summary>
        /// Prefab 完成加载后才创建真正的 ObjectPool。
        /// </summary>
        private void CreateRuntimePool(PoolEntry entry)
        {
            if (entry.Prefab == null)
                throw new InvalidOperationException($"Pool={entry.Config.Id} 尚未取得 Prefab。");

            entry.Pool = new ObjectPool<GameObject>(
                createFunc: () => CreateInstance(entry),
                actionOnGet: null,
                actionOnRelease: PrepareInactiveInstance,
                actionOnDestroy: DestroyPooledInstance,
                collectionCheck: true,
                defaultCapacity: entry.Config.InitialCapacity,
                maxSize: entry.Config.MaxSize);
        }

        private GameObject CreateInstance(PoolEntry entry)
        {
            GameObject instance = Instantiate(entry.Prefab, InactiveRoot);

            instance.name = entry.Prefab.name;
            instance.SetActive(false);

            int instanceId = instance.GetInstanceID();

            _entryByInstanceId.Add(instanceId, entry);
            _instances.Add(instanceId, instance);
            _poolableCallbacks.Add(instanceId, CollectPoolableCallbacks(instance));

            return instance;
        }

        private void PrepareInactiveInstance(GameObject instance)
        {
            if (instance == null)
                return;

            instance.SetActive(false);

            Transform instanceTransform = instance.transform;

            instanceTransform.SetParent(InactiveRoot, false);
            instanceTransform.localPosition = Vector3.zero;
            instanceTransform.localRotation = Quaternion.identity;
            instanceTransform.localScale = Vector3.one;
        }

        private void DestroyPooledInstance(GameObject instance)
        {
            if (instance == null)
                return;

            int instanceId = instance.GetInstanceID();

            _rentedInstanceIds.Remove(instanceId);
            _entryByInstanceId.Remove(instanceId);
            _instances.Remove(instanceId);
            _poolableCallbacks.Remove(instanceId);

            Destroy(instance);
        }

        /// <summary>
        /// 主动调用Get创建InitialCapacity个实例，再全部放回池。
        /// </summary>
        private static void PrewarmPool(IObjectPool<GameObject> targetPool, int amount)
        {
            if (amount <= 0)
                return;

            List<GameObject> instances = new(amount);

            for (int i = 0; i < amount; i++)
                instances.Add(targetPool.Get());

            for (int i = 0; i < instances.Count; i++)
                targetPool.Release(instances[i]);
        }

        private static IPoolable[] CollectPoolableCallbacks(GameObject instance)
        {
            MonoBehaviour[] behaviours = instance.GetComponentsInChildren<MonoBehaviour>(true);

            if (behaviours == null || behaviours.Length == 0)
                return Array.Empty<IPoolable>();

            List<IPoolable> callbacks = new();

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IPoolable poolable)
                    callbacks.Add(poolable);
            }

            return callbacks.ToArray();
        }

        private void InvokeRentCallbacks(int instanceId)
        {
            if (!_poolableCallbacks.TryGetValue(instanceId, out IPoolable[] callbacks))
                return;

            for (int i = 0; i < callbacks.Length; i++)
            {
                try
                {
                    callbacks[i].OnRentFromPool();
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[{nameof(LocalObjectPool)}] OnRentFromPool 执行失败：\n{exception}");
                }
            }
        }

        private void InvokeReturnCallbacks(int instanceId)
        {
            if (!_poolableCallbacks.TryGetValue(instanceId, out IPoolable[] callbacks))
                return;

            for (int i = callbacks.Length - 1; i >= 0; i--)
            {
                try
                {
                    callbacks[i].OnReturnToPool();
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[{nameof(LocalObjectPool)}] OnReturnToPool 执行失败：\n{exception}");
                }
            }
        }

        private void ShutdownInternal()
        {
            if (!IsInitialized && _entriesById.Count == 0 && _instances.Count == 0)
            {
                DisposeLifetimeToken();
                return;
            }

            DestroyRemainingRentedInstances();
  
            //现在所有池的 RentedCount 都应该为 0。
            //逐池释放对象、Prefab 和 Addressables Handle。
            foreach (PoolEntry entry in _entriesById.Values)
                ReleasePreparedEntry(entry);

            _rentedInstanceIds.Clear();
            _poolableCallbacks.Clear();
            _entryByInstanceId.Clear();
            _instances.Clear();

            //Shutdown才真正删除Pool定义。
            _entriesById.Clear();

            IsInitialized = false;

            DisposeLifetimeToken();

            Debug.Log($"[{nameof(LocalObjectPool)}] 已关闭，所有对象池实例和 Addressables Handle 已释放。");
        }

        private void DestroyRemainingRentedInstances()
        {
            if (_rentedInstanceIds.Count == 0)
                return;

            Debug.LogWarning($"[{nameof(LocalObjectPool)}] Shutdown 时仍有 {_rentedInstanceIds.Count} 个对象未归还。");

            List<int> rentedIds = new(_rentedInstanceIds);

            for (int i = 0; i < rentedIds.Count; i++)
            {
                int instanceId = rentedIds[i];

                if (!_instances.TryGetValue(instanceId, out GameObject instance) || instance == null)
                    continue;

                if (_entryByInstanceId.TryGetValue(instanceId, out PoolEntry entry))
                {
                    entry.RentedCount = Mathf.Max(0, entry.RentedCount - 1);
                    InvokeReturnCallbacks(instanceId);
                }

                DestroyPooledInstance(instance);
            }
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException($"{nameof(LocalObjectPool)} 尚未初始化，请确认它已加入 GameRuntimeBootstrap。");
        }

        private void DisposeLifetimeToken()
        {
            if (_lifetimeCts == null)
                return;

            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();
            _lifetimeCts = null;
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;
    
           //正常流程应通过 Bootstrap 调用 ShutdownAsync。
           //OnDestroy 只作为场景被直接销毁时的最终兜底。
            _lifetimeCts?.Cancel();
            ShutdownInternal();

            Instance = null;
        }

        /// <summary>
        /// 单个对象池的完整运行时状态。
        /// </summary>
        private sealed class PoolEntry
        {
            public PoolItemConfig Config;

            public GameObject Prefab;
            public AsyncOperationHandle<GameObject> PrefabHandle;
            public bool HasPrefabHandle;

            public IObjectPool<GameObject> Pool;

            public bool IsPrepared;
            public bool IsPreparing;

            // 当前该池有多少实例正在外部使用。
            // ReleasePool 时必须为 0。
            public int RentedCount;

            public UniTaskCompletionSource PrepareCompletion;
        }
    }
}
