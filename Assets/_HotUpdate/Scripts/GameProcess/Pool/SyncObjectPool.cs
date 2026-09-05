using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Gameplay.Runtime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace ProjectGame.HotFix.Gameplay.Pooling
{
    /// <summary>
    /// GameRuntime内的网络对象池 
    /// </summary>
    //三个对象池的资源链路和控制逻辑其实基本相同，
    //网络对象池多了一个NetworkPrefab注册，但我们没有使用常规的提前在NetworkConfig里注册Prefab的方式，而是运行时动态注册/卸载Prefab，
    //在热更情景下，如果我们更新了一个新的模型，这个包被Client拉下来时，模型是不可能被注册进NetworkPrefabList的，他没法被NGO识别 
    //所以我们在运行时动态注册Prefab，使得NGO识别这个Prefab，来实现他的同步功能，作为统一，这个pool被卸载时我们也需要明确告知NGO这个Prefab不再被使用了
    //所以在卸载时会将对应prefab从NetworkPrefabList中移除，以及顺带删除他的handler 
    //注册NetworkPrefab，就是在Client与Server之间建立对一个对象的统一映射，他们的PrefabIdHash是一致的，
    //C/S之间通过这个hash来识别同一个对象类型，Spawn后再通过NetworkObjectId来识别实例
    //这也是为什么ForceSamePrefabs=true时NetworkList如果hash不一致是不允许你联机的，因为资源的映射关系乱了
    //而我们这里动态加载的原因，我们需要取消NetworkConfig.ForceSamePrefabs,使NGO容许短暂的List不一致 
    //所以，对于所有需要注册进NetworkPrefabList的Prefab，我们约定不提前在NetworkPrefabList中注册，全部交由对象池手动注册和卸载 
    public sealed class SyncObjectPool : MonoBehaviour, IGameRuntimeService
    {
        public static SyncObjectPool Instance { get; private set; }

        [Tooltip("未使用网络对象的父节点 ")]
        [SerializeField] private Transform _inactiveRoot;

        private readonly Dictionary<string, PoolEntry> _entriesById = new(StringComparer.Ordinal);

        private readonly Dictionary<int, PoolEntry> _entryByInstanceId = new();

        private readonly Dictionary<int, NetworkObject> _instances = new();

        private readonly Dictionary<int, IPoolable[]> _poolableCallbacks = new();

        private readonly HashSet<int> _rentedInstanceIds = new();

        private readonly HashSet<string> _configuredPrefabAddresses = new(StringComparer.Ordinal);

        private NetworkManager _networkManager;
        private CancellationTokenSource _lifetimeCts;

        public bool IsInitialized { get; private set; }

        public int PoolCount => _entriesById.Count;
        public int RentedCount => _rentedInstanceIds.Count;

        private Transform InactiveRoot => _inactiveRoot != null ? _inactiveRoot : transform;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError($"[{nameof(SyncObjectPool)}] 场景中存在重复实例 ");
                Destroy(this);
                return;
            }

            Instance = this;
        }

        /// <summary>
        /// 初始化阶段只登记池定义，不加载 NetworkPrefab 
        /// </summary>
        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return UniTask.CompletedTask;

            cancellationToken.ThrowIfCancellationRequested();

            _networkManager = NetworkManager.Singleton;

            if (_networkManager == null || !_networkManager.IsListening)
                throw new InvalidOperationException("NGO 尚未启动，无法初始化网络对象池 ");

            //当前架构是在 NGO 已启动后动态注册 NetworkPrefab 
            //ForceSamePrefabs 开启时不允许这样使用 
            if (_networkManager.NetworkConfig.ForceSamePrefabs)
                throw new InvalidOperationException("SyncObjectPool 使用运行时动态 NetworkPrefab，请关闭 NetworkConfig.ForceSamePrefabs ");

            try
            {
                _lifetimeCts = new CancellationTokenSource();

                RegisterConfiguredDefinitions(cancellationToken);

                IsInitialized = true;

                Debug.Log($"[{nameof(SyncObjectPool)}] 初始化完成，已登记 {_entriesById.Count} 个网络对象池定义 ");

                return UniTask.CompletedTask;
            }
            catch
            {
                ShutdownInternal();
                throw;
            }
        }

        /// <summary>
        /// 准备一个网络对象池 
        ///
        /// 完成后保证：
        /// Prefab 已加载
        /// NGO已识别该NetworkPrefab
        /// PrefabHandler已注册
        /// ObjectPool已创建并完成Prewarm
        /// </summary>
        public async UniTask PreparePoolAsync(string poolId, CancellationToken cancellationToken)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(poolId))
                throw new ArgumentException("PoolId 不能为空 ", nameof(poolId));

            if (!_entriesById.TryGetValue(poolId, out PoolEntry entry))
                throw new KeyNotFoundException($"没有登记网络对象池：{poolId}");

            if (entry.IsPrepared)
                return;

            if (!entry.IsPreparing)
            {
                entry.IsPreparing = true;
                entry.PrepareCompletion = new UniTaskCompletionSource();

                PrepareEntryAsync(entry, _lifetimeCts.Token).Forget();
            }

            //调用方取消只取消自己的等待 
            //不因为某一个调用方离开，就破坏其他调用方正在等待的共享 Prepare 
            await entry.PrepareCompletion.Task.AttachExternalCancellation(cancellationToken);
        }

        /// <summary>
        /// 并行准备多个网络对象池 
        /// </summary>
        public async UniTask PreparePoolsAsync(IEnumerable<string> poolIds, CancellationToken cancellationToken)
        {
            EnsureInitialized();

            if (poolIds == null)
                throw new ArgumentNullException(nameof(poolIds));

            HashSet<string> uniqueIds = new(StringComparer.Ordinal);

            foreach (string poolId in poolIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!string.IsNullOrWhiteSpace(poolId))
                    uniqueIds.Add(poolId);
            }

            if (uniqueIds.Count == 0)
                return;

            List<UniTask> tasks = new(uniqueIds.Count);

            foreach (string poolId in uniqueIds)
                tasks.Add(PreparePoolAsync(poolId, cancellationToken));

            await UniTask.WhenAll(tasks);
        }

        /// <summary>
        /// 准备当前配置中的全部网络对象池 
        /// </summary>
        public UniTask PrepareAllPoolsAsync(CancellationToken cancellationToken)
        {
            return PreparePoolsAsync(_entriesById.Keys, cancellationToken);
        }

        public bool ContainsPool(string poolId)
        {
            return !string.IsNullOrWhiteSpace(poolId) && _entriesById.ContainsKey(poolId);
        }

        public bool IsPoolPrepared(string poolId)
        {
            return !string.IsNullOrWhiteSpace(poolId) &&
                   _entriesById.TryGetValue(poolId, out PoolEntry entry) &&
                   entry.IsPrepared;
        }

        /// <summary>
        /// 只能由服务器触发
        /// 生成服务器所有的NetworkObject
        /// 用于所有由服务器完全控制的公共对象
        /// </summary>
        public NetworkObject Spawn(string poolId, Vector3 position, Quaternion rotation, bool destroyWithScene = true)
        {
            return SpawnInternal(poolId, position, rotation, null, destroyWithScene);
        }

        /// <summary>
        /// 只能由服务器触发
        /// 生成客户端所有的网络对象
        /// 适合客户端控制的对象，如玩家及附属物
        /// </summary>
        public NetworkObject SpawnWithOwnership(string poolId, ulong ownerClientId, Vector3 position, Quaternion rotation, bool destroyWithScene = true)
        {
            return SpawnInternal(poolId, position, rotation, ownerClientId, destroyWithScene);
        }

        /// <summary>
        /// Server Despawn网络对象并返还对象池 
        /// </summary>
        public void DespawnAndReturn(NetworkObject instance)
        {
            if (instance == null)
                return;

            EnsureInitialized();

            if (!_networkManager.IsServer)
            {
                Debug.LogWarning($"[{nameof(SyncObjectPool)}] 只有 Server 可以主动回收网络对象 ");
                return;
            }

            int instanceId = instance.GetInstanceID();

            if (!_entryByInstanceId.TryGetValue(instanceId, out PoolEntry entry))
            {
                Debug.LogError($"[{nameof(SyncObjectPool)}] 对象不属于当前网络池：{instance.name}");
                return;
            }

            if (!_rentedInstanceIds.Contains(instanceId))
            {
                Debug.LogWarning($"[{nameof(SyncObjectPool)}] 对象可能已经被返还：{instance.name}");
                return;
            }

            
            //false表示NGO Despawn后不Destroy GameObject 
            //Client通过PrefabHandler.Destroy返回自己的本地池 
            if (instance.IsSpawned)
                instance.Despawn(false);

            //Server 主动取得的实例需要自己返还 
            //Host的本地Handler在某些流程中可能已经处理，所以再次检查 
            if (_rentedInstanceIds.Contains(instanceId))
                ReturnInstance(entry, instance);
        }

        public async UniTask ShutdownAsync(CancellationToken cancellationToken)
        {   
            //Shutdown不接受外部取消 
            //先结束当前正在进行的Addressables Prepare 
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
                    // Prepare 自己负责回滚 NGO 注册和 Addressables Handle 
                }
            }

            // 各端 Cleanup 同时开始。Client 必须等 Server 的 Despawn 到达，
            // 才能注销 Handler/释放 Prefab，不能直接 Destroy 仍在联网的池对象。
            while (_networkManager != null && _networkManager.IsListening && !_networkManager.IsServer &&
                   HasSpawnedInstances())
                await UniTask.Yield(PlayerLoopTiming.Update);

            ShutdownInternal();
        }

        private bool HasSpawnedInstances()
        {
            foreach (NetworkObject instance in _instances.Values)
                if (instance != null && instance.IsSpawned) return true;
            return false;
        }

        public bool ReleasePool(string poolId)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(poolId))
                throw new ArgumentException("PoolId 不能为空 ", nameof(poolId));

            if (!_entriesById.TryGetValue(poolId, out PoolEntry entry))
                throw new KeyNotFoundException($"没有登记网络对象池：{poolId}");

            if (!CanReleaseEntry(entry, true))
                return false;

            ReleasePreparedEntry(entry);

            Debug.Log($"[{nameof(SyncObjectPool)}] 网络对象池已释放：{poolId}");

            return true;

        }

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

            //先保证整批对象都能释放 
            foreach (string poolId in uniquePoolIds)
            {
                if (!_entriesById.TryGetValue(poolId, out PoolEntry entry))
                    throw new KeyNotFoundException($"没有登记网络对象池：{poolId}");

                if (!CanReleaseEntry(entry, true))
                    return false;
            }

            //所有Entry都安全后再真正修改状态 
            foreach (string poolId in uniquePoolIds)
                ReleasePreparedEntry(_entriesById[poolId]);

            Debug.Log($"[{nameof(SyncObjectPool)}] 批量释放网络对象池完成，PoolCount={uniquePoolIds.Count}");

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

            Debug.Log($"[{nameof(SyncObjectPool)}] 所有已准备网络对象池均已释放 ");

            return true;
        }

        public int GetPoolRentedCount(string poolId)
        {
            EnsureInitialized();

            if (!_entriesById.TryGetValue(poolId, out PoolEntry entry))
                throw new KeyNotFoundException($"没有登记网络对象池：{poolId}");

            return entry.RentedCount;
        }

        private async UniTaskVoid PrepareEntryAsync(PoolEntry entry, CancellationToken cancellationToken)
        {
            AsyncOperationHandle<GameObject> handle = default;
            bool hasHandle = false;

            try
            {  
                //每个Pool只发起一次Addressables Load 
                handle = Addressables.LoadAssetAsync<GameObject>(entry.Config.PrefabAddress);
                hasHandle = true;

                GameObject prefabObject =  await handle.ToUniTask(cancellationToken: cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                if (handle.Status != AsyncOperationStatus.Succeeded || prefabObject == null)
                    throw new InvalidOperationException($"Addressable NetworkPrefab 加载失败：Pool={entry.Config.Id}");

                if (!prefabObject.TryGetComponent(out NetworkObject networkPrefab))
                    throw new InvalidOperationException($"Addressable Prefab 缺少 NetworkObject：Pool={entry.Config.Id}");

                //PrefabIdHash标明一个对象类型，比如一个prefab的PrefabIdHash是固定的
                //PrefabIdHash是一个uint类型的哈希值，NGO用它来识别不同的网络对象类型 
                //NGO的整个对象同步，本质上就是先靠PrefabIdHash找类型，Spawn后再靠NetworkObjectId找实例
                uint prefabHash = networkPrefab.PrefabIdHash;

                entry.PrefabObject = prefabObject;
                entry.NetworkPrefab = networkPrefab;

                entry.PrefabHandle = handle;
                entry.HasPrefabHandle = true;

                //将Prefab注册进NetworkPrefabList，这样NGO就认识这个prefab了
                _networkManager.PrefabHandler.AddNetworkPrefab(prefabObject);
                entry.IsNetworkPrefabRegistered = true;

                CreateRuntimePool(entry);

                entry.Handler = new PooledPrefabInstanceHandler(this, entry);

                //客户端NGO认识这个prefab后，AddHandler让我们自定义的PooledPrefabInstanceHandler来处理客户端的Instantiate和Destroy
                //而不是默认的强制Instantiate和Destroy，这里是把对象池逻辑强制绑定到客户端的PrefabHandler上
                if (!_networkManager.PrefabHandler.AddHandler(prefabObject, entry.Handler))
                    throw new InvalidOperationException($"PrefabHandler 注册失败：Pool={entry.Config.Id}");

                entry.IsHandlerRegistered = true;

                PrewarmPool(entry.Pool, entry.Config.InitialCapacity);

                entry.IsPrepared = true;

                entry.PrepareCompletion.TrySetResult();

                Debug.Log($"[{nameof(SyncObjectPool)}] Pool 准备完成：{entry.Config.Id}，Initial={entry.Config.InitialCapacity}，Max={entry.Config.MaxSize}");
            }
            catch (Exception exception)
            {
                RollbackPrepare(entry);

                if (hasHandle && !entry.HasPrefabHandle && handle.IsValid())
                    Addressables.Release(handle);

                entry.PrepareCompletion?.TrySetException(exception);
            }
            finally
            {
                entry.IsPreparing = false;
            }
        }

        /// <summary>
        /// Initialize时只登记数据定义 
        /// </summary>
        private void RegisterConfiguredDefinitions(CancellationToken cancellationToken)
        {
            Dictionary<int, Config_SyncObjectPool> table = ConfigManager.Instance.GetTable<Config_SyncObjectPool>();

            if (table == null)
                throw new InvalidOperationException("未加载 SyncObjectPool 配置表 ");

            var configIds = new List<int>(table.Keys);
            configIds.Sort();

            for (int index = 0; index < configIds.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int configId = configIds[index];
                Config_SyncObjectPool row = table[configId];

                if (row == null)
                    throw new InvalidOperationException($"SyncObjectPool 配置为空，ConfigId={configId}");

                if (row.ConfigId != configId)
                    throw new InvalidOperationException(
                        $"SyncObjectPool 配置主键不一致：DictionaryKey={configId}，ConfigId={row.ConfigId}");

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
                throw new InvalidOperationException($"Network PoolId 重复：{config.Id}");

            if (!_configuredPrefabAddresses.Add(config.PrefabAddress))
                throw new InvalidOperationException($"多个 Network Pool 指向同一个 PrefabAddress：{config.PrefabAddress}");

            _entriesById.Add(config.Id, new PoolEntry
            {
                Config = config
            });
        }

        private void CreateRuntimePool(PoolEntry entry)
        {
            if (entry.NetworkPrefab == null)
                throw new InvalidOperationException($"Pool={entry.Config.Id} 尚未加载 NetworkPrefab ");

            entry.Pool = new ObjectPool<NetworkObject>(
                createFunc: () => CreateInstance(entry),
                actionOnGet: null,
                actionOnRelease: PrepareInactiveInstance,
                actionOnDestroy: DestroyPooledInstance,
                collectionCheck: true,
                defaultCapacity: entry.Config.InitialCapacity,
                maxSize: entry.Config.MaxSize);
        }

        private NetworkObject SpawnInternal(string poolId, Vector3 position, Quaternion rotation, ulong? ownerClientId, bool destroyWithScene)
        {
            EnsureInitialized();

            if (!_networkManager.IsServer)
            {
                Debug.LogWarning($"[{nameof(SyncObjectPool)}] 只有 Server 可以 Spawn 网络对象 ");
                return null;
            }

            if (!_entriesById.TryGetValue(poolId, out PoolEntry entry))
                throw new KeyNotFoundException($"没有登记网络对象池：{poolId}");

            if (!entry.IsPrepared || entry.Pool == null)
                throw new InvalidOperationException($"网络对象池尚未 Prepare：{poolId}");

            NetworkObject instance = RentInstance(entry, position, rotation);

            try
            {
                //指定了ownerClientId就把所有权给他，没有的话默认归属服务器
                if (ownerClientId.HasValue)
                    instance.SpawnWithOwnership(ownerClientId.Value, destroyWithScene);
                else
                    instance.Spawn(destroyWithScene);

                return instance;
            }
            catch
            {
                if (!instance.IsSpawned && _rentedInstanceIds.Contains(instance.GetInstanceID()))
                    ReturnInstance(entry, instance);

                throw;
            }
        }

        private NetworkObject CreateInstance(PoolEntry entry)
        {
            // 不要直接带 Parent Instantiate NetworkObject 
            // InactiveRoot 位于 GameRoot.NetworkObject 之下，Unity 在克隆组件尚未全部就绪时
            // 可能先触发父级相关查询，使 NetworkBehaviour 临时绑定到外层 GameRoot，
            // NGO 随后会把空的 ChildNetworkBehaviours 缓存下来，导致 OnNetworkSpawn 不执行 
            // 先在根层级完整实例化并停用，再临时关闭 NGO 自动父级同步后归档到池根节点 
            NetworkObject instance = Instantiate(entry.NetworkPrefab);
            instance.name = entry.NetworkPrefab.name;
            instance.gameObject.SetActive(false);

            Scene ownerScene = gameObject.scene;
            if (ownerScene.IsValid() && ownerScene.isLoaded && instance.gameObject.scene != ownerScene)
                SceneManager.MoveGameObjectToScene(instance.gameObject, ownerScene);

            bool autoObjectParentSync = instance.AutoObjectParentSync;
            instance.AutoObjectParentSync = false;
            instance.transform.SetParent(InactiveRoot, false);
            instance.AutoObjectParentSync = autoObjectParentSync;

            int instanceId = instance.GetInstanceID();

            _entryByInstanceId.Add(instanceId, entry);
            _instances.Add(instanceId, instance);
            _poolableCallbacks.Add(instanceId, CollectPoolableCallbacks(instance.gameObject));

            return instance;
        }

        /// <summary>
        /// Server主动Spawn后，客户端触发PrefabHandler调用Instantiate，也会走RentInstance
        /// </summary>
        private NetworkObject RentInstance(PoolEntry entry, Vector3 position, Quaternion rotation)
        {
            if (!entry.IsPrepared || entry.Pool == null)
                throw new InvalidOperationException($"网络对象池尚未准备完成：{entry.Config.Id}");

            NetworkObject instance = entry.Pool.Get();

            if (instance == null)
                throw new InvalidOperationException($"网络对象池返回空实例：{entry.Config.Id}");

            int instanceId = instance.GetInstanceID();

            if (!_rentedInstanceIds.Add(instanceId))
                throw new InvalidOperationException($"网络对象被重复租出：{instance.name} ({instanceId})");

            entry.RentedCount++;
            Transform instanceTransform = instance.transform;
            //NGO OnNetworkSpawn发生前先恢复业务状态 
            SetUnspawnedParent(instance, null);
            instanceTransform.SetPositionAndRotation(position, rotation);
            instanceTransform.localScale = Vector3.one;
            
            InvokeRentCallbacks(instanceId);

            instance.gameObject.SetActive(true);

            return instance;
        }

        private void ReturnInstance(PoolEntry entry, NetworkObject instance)
        {
            if (instance == null)
                return;

            int instanceId = instance.GetInstanceID();

            if (!_entryByInstanceId.TryGetValue(instanceId, out PoolEntry actualEntry) || actualEntry != entry)
            {
                Debug.LogError($"[{nameof(SyncObjectPool)}] 实例与对象池不匹配：{instance.name}");
                return;
            }

            if (!_rentedInstanceIds.Remove(instanceId))
            {
                Debug.LogWarning($"[{nameof(SyncObjectPool)}] 对象被重复返还：{instance.name}");
                return;
            }

            entry.RentedCount--;

            if (entry.RentedCount < 0)
            {
                entry.RentedCount = 0;
                Debug.LogError($"[{nameof(SyncObjectPool)}] Pool={entry.Config.Id} 的 RentedCount 出现异常 ");
            }

            if (instance.IsSpawned)
            {
                Debug.LogError($"[{nameof(SyncObjectPool)}] 不能返还仍处于 Spawn 状态的 NetworkObject：{instance.name}");
                //前面已经判定--，这里发现删不了，加回来
                entry.RentedCount++;
                _rentedInstanceIds.Add(instanceId);
                return;
            }

            InvokeReturnCallbacks(instanceId);
            entry.Pool.Release(instance);
        }

        private void PrepareInactiveInstance(NetworkObject instance)
        {
            if (instance == null)
                return;

            instance.gameObject.SetActive(false);

            Transform instanceTransform = instance.transform;

            SetUnspawnedParent(instance, InactiveRoot);
            instanceTransform.localPosition = Vector3.zero;
            instanceTransform.localRotation = Quaternion.identity;
            instanceTransform.localScale = Vector3.one;
        }

        private static void SetUnspawnedParent(NetworkObject instance, Transform parent)
        {
            bool synchronizeParent = instance.AutoObjectParentSync;
            instance.AutoObjectParentSync = false;
            try { instance.transform.SetParent(parent, false); }
            finally { instance.AutoObjectParentSync = synchronizeParent; }
        }

        private void DestroyPooledInstance(NetworkObject instance)
        {
            if (instance == null)
                return;

            int instanceId = instance.GetInstanceID();

            _rentedInstanceIds.Remove(instanceId);
            _entryByInstanceId.Remove(instanceId);
            _instances.Remove(instanceId);
            _poolableCallbacks.Remove(instanceId);

            Destroy(instance.gameObject);
        }

        private static void PrewarmPool(IObjectPool<NetworkObject> targetPool, int amount)
        {
            if (amount <= 0)
                return;

            List<NetworkObject> instances = new(amount);

            for (int i = 0; i < amount; i++)
                instances.Add(targetPool.Get());

            for (int i = 0; i < instances.Count; i++)
                targetPool.Release(instances[i]);
        }

        private void ReleasePreparedEntry(PoolEntry entry)
        {
            if (!entry.IsPrepared)
                return;

           //此时必须保证：
           //所有NetworkObject已经Despawn；
           //所有实例已经返回Pool 
            RemoveNetworkBindings(entry);
  
            //Handler已经移除，不会再有新的NGO Instantiate / Destroy
            //进入这个ObjectPool 
            if (entry.Pool != null)
            {
                entry.Pool.Clear();
                entry.Pool = null;
            }

            entry.Handler = null;
            entry.NetworkPrefab = null;
            entry.PrefabObject = null;

            if (entry.HasPrefabHandle && entry.PrefabHandle.IsValid())
                Addressables.Release(entry.PrefabHandle);

            entry.PrefabHandle = default;
            entry.HasPrefabHandle = false;

            entry.RentedCount = 0;
            entry.IsPrepared = false;
        }

        private bool CanReleaseEntry(PoolEntry entry, bool logWarning)
        {
            if (!entry.IsPrepared && !entry.IsPreparing)
                return true;

            if (entry.IsPreparing)
            {
                if (logWarning)
                    Debug.LogWarning($"[{nameof(SyncObjectPool)}] Pool 正在 Prepare，暂时不能 Release：{entry.Config.Id}");

                return false;
            }

            if (entry.RentedCount > 0)
            {
                if (logWarning)
                    Debug.LogWarning($"[{nameof(SyncObjectPool)}] Pool={entry.Config.Id} 仍有 {entry.RentedCount} 个 NetworkObject 在使用，不能 Release ");

                return false;
            }

            return true;
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
                    Debug.LogError($"[{nameof(SyncObjectPool)}] OnRentFromPool 执行失败：\n{exception}");
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
                    Debug.LogError($"[{nameof(SyncObjectPool)}] OnReturnToPool 执行失败：\n{exception}");
                }
            }
        }

        /// <summary>
        /// Prepare 中途失败时，只回滚当前Entry 
        /// </summary>
        private void RollbackPrepare(PoolEntry entry)
        {
            if (entry.IsHandlerRegistered && entry.PrefabObject != null)
            {
                try
                {
                    _networkManager?.PrefabHandler.RemoveHandler(entry.PrefabObject);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[{nameof(SyncObjectPool)}] Prepare 回滚 Handler 失败：\n{exception}");
                }

                entry.IsHandlerRegistered = false;
            }

            if (entry.IsNetworkPrefabRegistered && entry.PrefabObject != null)
            {
                try
                {
                    _networkManager?.PrefabHandler.RemoveNetworkPrefab(entry.PrefabObject);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[{nameof(SyncObjectPool)}] Prepare 回滚 NetworkPrefab 失败：\n{exception}");
                }

                entry.IsNetworkPrefabRegistered = false;
            }

            if (entry.Pool != null)
            {
                entry.Pool.Clear();
                entry.Pool = null;
            }

            entry.NetworkPrefab = null;
            entry.PrefabObject = null;
            entry.IsPrepared = false;

            if (entry.HasPrefabHandle && entry.PrefabHandle.IsValid())
                Addressables.Release(entry.PrefabHandle);

            entry.PrefabHandle = default;
            entry.HasPrefabHandle = false;
        }

        private void ShutdownInternal()
        {
            if (!IsInitialized && _entriesById.Count == 0 && _instances.Count == 0)
            {
                DisposeLifetimeToken();
                return;
            }

            DestroyRemainingRentedInstances();
       
            //所有运行时 NetworkObject 都已经处理完毕，
            //可以逐个释放 Prepared Pool 
            foreach (PoolEntry entry in _entriesById.Values)
                ReleasePreparedEntry(entry);

            _rentedInstanceIds.Clear();
            _poolableCallbacks.Clear();
            _entryByInstanceId.Clear();
            _instances.Clear();

            //ReleasePool不删除定义；
            //Shutdown才删除全部定义  
            _entriesById.Clear();

            _networkManager = null;
            IsInitialized = false;
            _configuredPrefabAddresses.Clear();

            DisposeLifetimeToken();

            Debug.Log($"[{nameof(SyncObjectPool)}] 已关闭，网络注册、实例和 Addressables Handle 已全部释放 ");
        }

        private void DestroyRemainingRentedInstances()
        {
            if (_rentedInstanceIds.Count == 0)
                return;

            Debug.LogWarning($"[{nameof(SyncObjectPool)}] Shutdown 时仍有 {_rentedInstanceIds.Count} 个网络对象未归还 ");

            List<int> rentedIds = new(_rentedInstanceIds);

            for (int i = 0; i < rentedIds.Count; i++)
            {
                int instanceId = rentedIds[i];

                if (!_instances.TryGetValue(instanceId, out NetworkObject instance) || instance == null)
                    continue;

                //Server必须先完成网络Despawn 
                //Client理论上应该由Server的Despawn消息正常回池 
                if (instance.IsSpawned && _networkManager != null && _networkManager.IsServer)
                    instance.Despawn(false);

                //Host可能在Despawn中已经通过Handler Return 
                if (!_rentedInstanceIds.Contains(instanceId))
                    continue;

                if (_entryByInstanceId.TryGetValue(instanceId, out PoolEntry entry))
                    entry.RentedCount = Mathf.Max(0, entry.RentedCount - 1);

                InvokeReturnCallbacks(instanceId);
                DestroyPooledInstance(instance);
            }
        }

        private void RemoveNetworkBindings(PoolEntry entry)
        {
            if (_networkManager == null || entry.PrefabObject == null)
                return;

            if (entry.IsHandlerRegistered)
            {
                try
                {
                    _networkManager.PrefabHandler.RemoveHandler(entry.PrefabObject);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[{nameof(SyncObjectPool)}] RemoveHandler 失败：Pool={entry.Config.Id}\n{exception}");
                }

                entry.IsHandlerRegistered = false;
            }

            if (entry.IsNetworkPrefabRegistered)
            {
                try
                {
                    _networkManager.PrefabHandler.RemoveNetworkPrefab(entry.PrefabObject);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[{nameof(SyncObjectPool)}] RemoveNetworkPrefab 失败：Pool={entry.Config.Id}\n{exception}");
                }

                entry.IsNetworkPrefabRegistered = false;
            }
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException($"{nameof(SyncObjectPool)} 尚未初始化，请确认它已加入 GameRuntimeBootstrap ");
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

            _lifetimeCts?.Cancel();
            ShutdownInternal();

            Instance = null;
        }

        /// <summary>
        /// 一个网络 Pool 的完整运行时状态 
        /// </summary>
        private sealed class PoolEntry
        {
            public PoolItemConfig Config;

            public GameObject PrefabObject;
            public NetworkObject NetworkPrefab;

            public AsyncOperationHandle<GameObject> PrefabHandle;
            public bool HasPrefabHandle;

            public IObjectPool<NetworkObject> Pool;
            public PooledPrefabInstanceHandler Handler;

            public int RentedCount;

            public bool IsNetworkPrefabRegistered;
            public bool IsHandlerRegistered;

            public bool IsPrepared;
            public bool IsPreparing;

            public UniTaskCompletionSource PrepareCompletion;
        }

        /// <summary>
        /// NGO在Client生成 / 销毁网络对象时调用 
        /// </summary>
        private sealed class PooledPrefabInstanceHandler : INetworkPrefabInstanceHandler
        {
            private readonly SyncObjectPool _owner;
            private readonly PoolEntry _entry;

            public PooledPrefabInstanceHandler(SyncObjectPool owner, PoolEntry entry)
            {
                _owner = owner;
                _entry = entry;
            }

            public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
            {
                return _owner.RentInstance(_entry, position, rotation);
            }

            public void Destroy(NetworkObject networkObject)
            {
                _owner.ReturnInstance(_entry, networkObject);
            }
        }
    }
}
