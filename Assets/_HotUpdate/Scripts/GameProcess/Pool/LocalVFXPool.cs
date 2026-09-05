using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Gameplay.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

namespace ProjectGame.HotFix.Gameplay.Pooling
{
    public sealed class LocalVFXPool : MonoBehaviour, IGameRuntimeService
    {
        public static LocalVFXPool Instance { get; private set; }

        [Tooltip("未使用特效的父节点 ")]
        [SerializeField] private Transform _inactiveRoot;

        /// <summary>
        /// PoolId → 完整运行时状态 
        /// </summary>
        private readonly Dictionary<string, PoolEntry> _entriesById = new(StringComparer.Ordinal);

        /// <summary>
        /// 实例ID → 所属池 
        /// Return 时不需要调用方保存 PoolId 
        /// </summary>
        private readonly Dictionary<int, PoolEntry> _entryByInstanceId = new();

        /// <summary>
        /// 当前Pool创建的全部实例 
        /// </summary>
        private readonly Dictionary<int, GameObject> _instances = new();

        /// <summary>
        /// 粒子、Trail、VFX Graph、Rigidbody等表现组件缓存 
        /// </summary>
        private readonly Dictionary<int, VFXRuntimeCache> _runtimeCaches = new();

        /// <summary>
        /// IPoolable生命周期缓存 
        /// </summary>
        private readonly Dictionary<int, IPoolable[]> _poolableCallbacks = new();

        /// <summary>
        /// 当前正在外部播放的实例 
        /// </summary>
        private readonly HashSet<int> _rentedInstanceIds = new();

        private CancellationTokenSource _lifetimeCts;

        public bool IsInitialized { get; private set; }

        public int PoolCount => _entriesById.Count;

        public int PlayingCount => _rentedInstanceIds.Count;

        private Transform InactiveRoot => _inactiveRoot != null ? _inactiveRoot : transform;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError($"[{nameof(LocalVFXPool)}] 场景中存在重复实例 ");
                Destroy(this);
                return;
            }

            Instance = this;
        }

        /// <summary>
        /// 初始化时只登记配置，不加载任何 Addressable VFX Prefab 
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

                Debug.Log($"[{nameof(LocalVFXPool)}] 初始化完成，已登记 {_entriesById.Count} 个特效池定义 ");

                return UniTask.CompletedTask;
            }
            catch
            {
                ShutdownInternal();
                throw;
            }
        }

        /// <summary>
        /// 准备一个特效池 
        ///
        /// 真正的加载只绑定 Pool 生命周期 
        /// 调用者取消时只停止自己的等待，不破坏其他调用者共享的 Prepare 
        /// </summary>
        public async UniTask PreparePoolAsync(string poolId, CancellationToken cancellationToken)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(poolId))
                throw new ArgumentException("PoolId 不能为空 ", nameof(poolId));

            if (!_entriesById.TryGetValue(poolId, out PoolEntry entry))
                throw new KeyNotFoundException($"没有登记特效池定义：{poolId}");

            if (entry.IsPrepared)
                return;

            if (!entry.IsPreparing)
            {
                entry.IsPreparing = true;
                entry.PrepareCompletion = new UniTaskCompletionSource();

                PrepareEntryAsync(entry, _lifetimeCts.Token).Forget();
            }

            await entry.PrepareCompletion.Task.AttachExternalCancellation(cancellationToken);
        }

        /// <summary>
        /// 批量并行准备特效池 
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

            List<UniTask> tasks = new(uniquePoolIds.Count);

            foreach (string poolId in uniquePoolIds)
                tasks.Add(PreparePoolAsync(poolId, cancellationToken));

            await UniTask.WhenAll(tasks);
        }

        public UniTask PrepareAllPoolsAsync(CancellationToken cancellationToken)
        {
            return PreparePoolsAsync(_entriesById.Keys, cancellationToken);
        }

        /// <summary>
        /// 使用单位旋转播放特效 
        /// </summary>
        public GameObject Play(string poolId, Vector3 position, float weight = 1f, Transform parent = null)
        {
            return Play(poolId, position, Quaternion.identity, weight, parent);
        }

        /// <summary>
        /// 从已经完成 Prepare 的池中同步取得并播放特效 
        ///
        /// Play 不负责触发 Addressables 加载 
        /// </summary>
        public GameObject Play(string poolId, Vector3 position, Quaternion rotation, float weight = 1f, Transform parent = null)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(poolId))
                throw new ArgumentException("PoolId 不能为空 ", nameof(poolId));

            if (!_entriesById.TryGetValue(poolId, out PoolEntry entry))
                throw new KeyNotFoundException($"没有登记特效池定义：{poolId}");

            if (!entry.IsPrepared || entry.Pool == null)
                throw new InvalidOperationException($"特效池尚未 Prepare，不能 Play：{poolId}");

            GameObject instance = entry.Pool.Get();

            if (instance == null)
                throw new InvalidOperationException($"特效池返回了空实例：{poolId}");

            int instanceId = instance.GetInstanceID();

            if (!_rentedInstanceIds.Add(instanceId))
                throw new InvalidOperationException($"特效被重复租出：{instance.name} ({instanceId})");

            entry.PlayingCount++;

            Transform instanceTransform = instance.transform;

            instanceTransform.SetParent(parent, false);
            instanceTransform.SetPositionAndRotation(position, rotation);

            
            //先清除上一次播放的数据，再调用业务复用回调 
            //此时GameObject仍然处于关闭状态 
            VFXRuntimeCache cache = _runtimeCaches[instanceId];

            cache.PrepareForPlay(weight);
            InvokeRentCallbacks(instanceId);

            instance.SetActive(true);
            
            //ParticleSystem / VisualEffect 必须在激活之后播放   
            cache.Play(entry.EffectEventId);

            return instance;
        }

        /// <summary>
        /// 停止并返还特效 
        /// </summary>
        public void Return(GameObject instance)
        {
            if (instance == null)
                return;

            if (!IsInitialized)
            {
                Debug.LogWarning($"[{nameof(LocalVFXPool)}] 对象池已经关闭，直接销毁特效：{instance.name}");
                Destroy(instance);
                return;
            }

            int instanceId = instance.GetInstanceID();

            if (!_entryByInstanceId.TryGetValue(instanceId, out PoolEntry entry))
            {
                Debug.LogWarning($"[{nameof(LocalVFXPool)}] 特效不属于当前对象池，直接销毁：{instance.name}");
                Destroy(instance);
                return;
            }

            if (!_rentedInstanceIds.Remove(instanceId))
            {
                Debug.LogWarning($"[{nameof(LocalVFXPool)}] 特效可能已经被返还：{instance.name} ({instanceId})");
                return;
            }

            entry.PlayingCount--;

            if (entry.PlayingCount < 0)
            {
                entry.PlayingCount = 0;
                Debug.LogError($"[{nameof(LocalVFXPool)}] Pool={entry.Config.Id} 的 PlayingCount 出现异常 ");
            }
  
            //先停止表现，再清理业务状态，最后放回ObjectPool  
            _runtimeCaches[instanceId].PrepareForReturn();
            InvokeReturnCallbacks(instanceId);

            entry.Pool.Release(instance);
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

        public int GetPoolPlayingCount(string poolId)
        {
            EnsureInitialized();

            if (!_entriesById.TryGetValue(poolId, out PoolEntry entry))
                throw new KeyNotFoundException($"没有登记特效池定义：{poolId}");

            return entry.PlayingCount;
        }

        /// <summary>
        /// 释放一个特效池 
        ///
        /// 只有不存在正在播放的实例时才允许释放 
        /// Pool配置不会删除，因此之后仍可以再次 Prepare 
        /// </summary>
        public bool ReleasePool(string poolId)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(poolId))
                throw new ArgumentException("PoolId 不能为空 ", nameof(poolId));

            if (!_entriesById.TryGetValue(poolId, out PoolEntry entry))
                throw new KeyNotFoundException($"没有登记特效池定义：{poolId}");

            if (!CanReleaseEntry(entry, true))
                return false;

            ReleasePreparedEntry(entry);

            Debug.Log($"[{nameof(LocalVFXPool)}] 特效池已释放：{poolId}");

            return true;
        }

        /// <summary>
        /// 批量释放特效池 
        ///
        /// 先检查整个集合，只有全部满足释放条件后才真正执行，
        /// 防止出现只释放一半的状态 
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

            foreach (string poolId in uniquePoolIds)
            {
                if (!_entriesById.TryGetValue(poolId, out PoolEntry entry))
                    throw new KeyNotFoundException($"没有登记特效池定义：{poolId}");

                if (!CanReleaseEntry(entry, true))
                    return false;
            }

            foreach (string poolId in uniquePoolIds)
                ReleasePreparedEntry(_entriesById[poolId]);

            Debug.Log($"[{nameof(LocalVFXPool)}] 批量释放完成，PoolCount={uniquePoolIds.Count}");

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

            Debug.Log($"[{nameof(LocalVFXPool)}] 所有已准备特效池均已释放 ");

            return true;
        }

        public async UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            //Shutdown不使用外部Token中断 
            //先通知全部后台Prepare生命周期结束 
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
                    // Prepare 自己负责资源回滚 
                }
            }

            ShutdownInternal();
        }

        /// <summary>
        /// 真正执行单个Entry的Addressables Prepare 
        /// </summary>
        private async UniTaskVoid PrepareEntryAsync(PoolEntry entry, CancellationToken cancellationToken)
        {
            AsyncOperationHandle<GameObject> handle = default;
            bool hasHandle = false;

            try
            {
                handle = Addressables.LoadAssetAsync<GameObject>(entry.Config.PrefabAddress);
                hasHandle = true;
                GameObject prefab = await handle.ToUniTask(cancellationToken: cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                if (handle.Status != AsyncOperationStatus.Succeeded || prefab == null)
                    throw new InvalidOperationException($"Addressable VFX Prefab 加载失败：Pool={entry.Config.Id}");

                entry.Prefab = prefab;
                entry.PrefabHandle = handle;
                entry.HasPrefabHandle = true;

                CreateRuntimePool(entry);
                PrewarmPool(entry.Pool, entry.Config.InitialCapacity);

                entry.IsPrepared = true;

                entry.PrepareCompletion.TrySetResult();

                Debug.Log($"[{nameof(LocalVFXPool)}] Pool 准备完成：{entry.Config.Id}，Initial={entry.Config.InitialCapacity}，Max={entry.Config.MaxSize}");
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
        /// Initialize 阶段只登记配置 
        /// </summary>
        private void RegisterConfiguredDefinitions(CancellationToken cancellationToken)
        {
            Dictionary<int, Config_LocalVFXPool> table =
                ConfigManager.Instance.GetTable<Config_LocalVFXPool>();

            if (table == null)
                throw new InvalidOperationException("未加载 LocalVFXPool 配置表 ");

            var configIds = new List<int>(table.Keys);
            configIds.Sort();

            for (int index = 0; index < configIds.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int configId = configIds[index];
                Config_LocalVFXPool row = table[configId];

                if (row == null)
                    throw new InvalidOperationException($"LocalVFXPool 配置为空，ConfigId={configId}");

                if (row.ConfigId != configId)
                    throw new InvalidOperationException(
                        $"LocalVFXPool 配置主键不一致：DictionaryKey={configId}，ConfigId={row.ConfigId}");

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
                throw new InvalidOperationException($"VFX PoolId 重复：{config.Id}");

            _entriesById.Add(config.Id, new PoolEntry
            {
                Config = config,

                //PoolId同时作为VFX Graph Event名称 
                EffectEventId = Shader.PropertyToID(config.Id)
            });
        }

        /// <summary>
        /// Addressable Prefab 已加载完成后才创建真正 ObjectPool 
        /// </summary>
        private void CreateRuntimePool(PoolEntry entry)
        {
            if (entry.Prefab == null)
                throw new InvalidOperationException($"Pool={entry.Config.Id} 尚未取得 VFX Prefab ");

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
            GameObject instance = Instantiate(entry.Prefab);
            Scene ownerScene = gameObject.scene;
            if (ownerScene.IsValid() && ownerScene.isLoaded && instance.scene != ownerScene)
                SceneManager.MoveGameObjectToScene(instance, ownerScene);
            instance.transform.SetParent(InactiveRoot, false);

            instance.name = entry.Prefab.name;
            instance.SetActive(false);

            int instanceId = instance.GetInstanceID();

            _entryByInstanceId.Add(instanceId, entry);
            _instances.Add(instanceId, instance);
  
            //这些扫描只在实例真正创建时执行一次 
            _runtimeCaches.Add(instanceId, new VFXRuntimeCache(instance));
            _poolableCallbacks.Add(instanceId, CollectPoolableCallbacks(instance));

            return instance;
        }

        private void PrepareInactiveInstance(GameObject instance)
        {
            if (instance == null)
                return;

            instance.SetActive(false);

            int instanceId = instance.GetInstanceID();

            if (_runtimeCaches.TryGetValue(instanceId, out VFXRuntimeCache cache))
                cache.RestoreOriginalScale();

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
            _runtimeCaches.Remove(instanceId);
            _poolableCallbacks.Remove(instanceId);

            Destroy(instance);
        }

        private bool CanReleaseEntry(PoolEntry entry, bool logWarning)
        {
            if (!entry.IsPrepared && !entry.IsPreparing)
                return true;

            if (entry.IsPreparing)
            {
                if (logWarning)
                    Debug.LogWarning($"[{nameof(LocalVFXPool)}] Pool 正在 Prepare，暂时不能 Release：{entry.Config.Id}");

                return false;
            }

            if (entry.PlayingCount > 0)
            {
                if (logWarning)
                    Debug.LogWarning($"[{nameof(LocalVFXPool)}] Pool={entry.Config.Id} 仍有 {entry.PlayingCount} 个特效正在播放，不能 Release ");

                return false;
            }

            return true;
        }

        /// <summary>
        /// 真正销毁单个Pool的全部实例并释放Addressables Handle 
        /// Pool Definition本身仍然保留 
        /// </summary>
        private void ReleasePreparedEntry(PoolEntry entry)
        {
            if (!entry.IsPrepared)
                return;

            if (entry.Pool != null)
            {
                entry.Pool.Clear();
                entry.Pool = null;
            }

            entry.Prefab = null;

            if (entry.HasPrefabHandle && entry.PrefabHandle.IsValid())
                Addressables.Release(entry.PrefabHandle);

            entry.PrefabHandle = default;
            entry.HasPrefabHandle = false;

            entry.PlayingCount = 0;
            entry.IsPrepared = false;
        }

        private void RollbackPrepare(PoolEntry entry)
        {
            if (entry.Pool != null)
            {
                entry.Pool.Clear();
                entry.Pool = null;
            }

            entry.Prefab = null;
            entry.PlayingCount = 0;
            entry.IsPrepared = false;

            if (entry.HasPrefabHandle && entry.PrefabHandle.IsValid())
                Addressables.Release(entry.PrefabHandle);

            entry.PrefabHandle = default;
            entry.HasPrefabHandle = false;
        }

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
                    Debug.LogError($"[{nameof(LocalVFXPool)}] OnRentFromPool 执行失败：\n{exception}");
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
                    Debug.LogError($"[{nameof(LocalVFXPool)}] OnReturnToPool 执行失败：\n{exception}");
                }
            }
        }

        private void DestroyRemainingPlayingInstances()
        {
            if (_rentedInstanceIds.Count == 0)
                return;

            Debug.LogWarning($"[{nameof(LocalVFXPool)}] Shutdown 时仍有 {_rentedInstanceIds.Count} 个特效正在播放 ");

            List<int> rentedIds = new(_rentedInstanceIds);

            for (int i = 0; i < rentedIds.Count; i++)
            {
                int instanceId = rentedIds[i];

                if (!_instances.TryGetValue(instanceId, out GameObject instance) || instance == null)
                    continue;

                if (_entryByInstanceId.TryGetValue(instanceId, out PoolEntry entry))
                    entry.PlayingCount = Mathf.Max(0, entry.PlayingCount - 1);

                if (_runtimeCaches.TryGetValue(instanceId, out VFXRuntimeCache cache))
                    cache.PrepareForReturn();

                InvokeReturnCallbacks(instanceId);
                DestroyPooledInstance(instance);
            }
        }

        private void ShutdownInternal()
        {
            if (!IsInitialized && _entriesById.Count == 0 && _instances.Count == 0)
            {
                DisposeLifetimeToken();
                return;
            }

            DestroyRemainingPlayingInstances();

            //此时所有外部实例已经销毁，
            //可以安全Clear各池并Release Handle 
            foreach (PoolEntry entry in _entriesById.Values)
                ReleasePreparedEntry(entry);

            _rentedInstanceIds.Clear();
            _poolableCallbacks.Clear();
            _runtimeCaches.Clear();
            _entryByInstanceId.Clear();
            _instances.Clear();
            //只有ShutDown才删除Entry定义
            _entriesById.Clear();

            IsInitialized = false;

            DisposeLifetimeToken();

            Debug.Log($"[{nameof(LocalVFXPool)}] 已关闭，全部实例和 Addressables Handle 已释放 ");
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException($"{nameof(LocalVFXPool)} 尚未初始化，请确认它已加入 GameRuntimeBootstrap ");
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
        /// 一个 VFX Pool 的完整运行时状态 
        /// </summary>
        private sealed class PoolEntry
        {
            public PoolItemConfig Config;

            public GameObject Prefab;
            public AsyncOperationHandle<GameObject> PrefabHandle;
            public bool HasPrefabHandle;

            public IObjectPool<GameObject> Pool;

            public int EffectEventId;
            public int PlayingCount;

            public bool IsPrepared;
            public bool IsPreparing;

            public UniTaskCompletionSource PrepareCompletion;
        }

        /// <summary>
        /// 单个特效实例的运行时表现缓存 
        /// </summary>
        private sealed class VFXRuntimeCache
        {
            private readonly Transform _root;
            private readonly Vector3 _originalRootScale;

            private readonly Rigidbody[] _rigidbodies;
            private readonly TrailRenderer[] _trails;
            private readonly ParticleSystem[] _particleSystems;
            private readonly VisualEffect[] _visualEffects;
            private readonly VFXImpactScaler[] _impactScalers;

            public VFXRuntimeCache(GameObject instance)
            {
                _root = instance.transform;
                _originalRootScale = _root.localScale;

                _rigidbodies = instance.GetComponentsInChildren<Rigidbody>(true);
                _trails = instance.GetComponentsInChildren<TrailRenderer>(true);
                _particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
                _visualEffects = instance.GetComponentsInChildren<VisualEffect>(true);
                _impactScalers = instance.GetComponentsInChildren<VFXImpactScaler>(true);
            }

            public void PrepareForPlay(float weight)
            {
                ResetRigidbodies();
                ClearTrails();
                RestoreOriginalScale();

                float safeWeight = Mathf.Max(0f, weight);

                if (_impactScalers.Length == 0)
                {
                    _root.localScale = _originalRootScale * safeWeight;
                    return;
                }

                for (int i = 0; i < _impactScalers.Length; i++)
                    _impactScalers[i].ApplyWeight(safeWeight);
            }

            public void Play(int effectEventId)
            {
                for (int i = 0; i < _particleSystems.Length; i++)
                {
                    ParticleSystem particleSystem = _particleSystems[i];

                    if (!particleSystem.main.playOnAwake)
                        particleSystem.Play(true);
                }

                for (int i = 0; i < _visualEffects.Length; i++)
                {
                    VisualEffect visualEffect = _visualEffects[i];

                    visualEffect.Reinit();
                    visualEffect.SendEvent(effectEventId);
                }
            }

            public void PrepareForReturn()
            {
                for (int i = 0; i < _particleSystems.Length; i++)
                    _particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                for (int i = 0; i < _visualEffects.Length; i++)
                    _visualEffects[i].Stop();

                ResetRigidbodies();
                ClearTrails();
                RestoreOriginalScale();
            }

            public void RestoreOriginalScale()
            {
                _root.localScale = _originalRootScale;

                for (int i = 0; i < _impactScalers.Length; i++)
                    _impactScalers[i].ResetToOriginal();
            }

            private void ResetRigidbodies()
            {
                for (int i = 0; i < _rigidbodies.Length; i++)
                {
                    _rigidbodies[i].velocity = Vector3.zero;
                    _rigidbodies[i].angularVelocity = Vector3.zero;
                }
            }

            private void ClearTrails()
            {
                for (int i = 0; i < _trails.Length; i++)
                    _trails[i].Clear();
            }
        }
    }
}
