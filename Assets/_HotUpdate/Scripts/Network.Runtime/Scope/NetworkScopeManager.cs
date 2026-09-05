using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectGame.HotFix.Network.Runtime
{
    /// <summary>
    /// 管理单个Client对当前目标Scope的NGO准备，如NetworkPrefab的注册，Root的获取
    /// </summary>
    public sealed class NetworkScopeManager
    {
        private readonly NetworkPrefabCatalog _catalog;
        private readonly NetworkPrefabRegistry _registry;
        private readonly NetworkManager _networkManager;
        //每台机器登记自己的本地 NGO 实例。
        private readonly Dictionary<NetworkPrefabId, NetworkObject> _instances =  new Dictionary<NetworkPrefabId, NetworkObject>();
        private readonly Dictionary<NetworkPrefabId, NetworkScopeLifecycle> _lifecycles = new Dictionary<NetworkPrefabId, NetworkScopeLifecycle>();
        private readonly Dictionary<NetworkPrefabId, Exception> _registrationFailures = new Dictionary<NetworkPrefabId, Exception>();

        private NetworkSceneMask _activeSceneMask = NetworkSceneMask.None;
        private bool _isPreparing;
        /// <summary>
        /// 当前已经Commit的网络Scope状态
        /// </summary>
        public NetworkSceneMask ActiveSceneMask => _activeSceneMask;
        public event Action<NetworkSceneMask> ScopeActivated;
        public bool IsPreparing => _isPreparing;

        public NetworkScopeManager(NetworkPrefabCatalog catalog,NetworkPrefabRegistry registry,NetworkManager networkManager)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
        }

        public void RegisterSpawnedInstance(NetworkPrefabId id,NetworkObject instance)
        {
            if (id == NetworkPrefabId.Invalid)
                throw new ArgumentException(
                    "不能登记 Invalid NetworkPrefabId",
                    nameof(id));

            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (_instances.TryGetValue(id, out NetworkObject existing))
            {
                if (existing == instance)
                    return;

                if (existing != null && existing.IsSpawned)
                {
                    throw new InvalidOperationException(
                        $"NetworkPrefab {id} 已存在存活实例，禁止重复登记");
                }
            }

            NetworkPrefabEntry entry = GetEntry(id);
            _instances[id] = instance;
            try
            {
                if (entry.Lifetime == NetworkPrefabLifetime.Persistent)
                    UnityEngine.Object.DontDestroyOnLoad(instance.gameObject);
                else
                    MoveToOwnerScene(entry, instance);
                _lifecycles[id] = new NetworkScopeLifecycle(instance);
                _registrationFailures.Remove(id);
            }
            catch (Exception exception)
            {
                _registrationFailures[id] = exception;
                throw;
            }

            Debug.Log(
                $"[NetworkScopeManager] 本地 NetworkRoot 已登记：{id}，" +
                $"ClientId={_networkManager.LocalClientId}");
        }

        public void UnregisterSpawnedInstance(NetworkPrefabId id,NetworkObject instance)
        {
            if (!_instances.TryGetValue(id, out NetworkObject existing))
                return;

            if (existing != instance)
            {
                Debug.LogWarning(
                    $"[NetworkScopeManager] 忽略不匹配的实例注销：{id}");
                return;
            }

            _instances.Remove(id);
            _lifecycles.Remove(id);
            _registrationFailures.Remove(id);
            Debug.Log($"[NetworkScopeManager] 本地 NetworkRoot 已注销：{id}");
        }

        public async UniTask<NetworkScopePrepareContext> PrepareScopeAsync(NetworkSceneMask targetMask,CancellationToken cancellationToken)
        {
            if (targetMask == NetworkSceneMask.None)
                throw new ArgumentException("TargetMask 不能为 None", nameof(targetMask));

            if (_isPreparing)
                throw new InvalidOperationException("已有 NetworkScope 正在 Prepare");

            var context = new NetworkScopePrepareContext(this,_activeSceneMask,targetMask);

            _isPreparing = true;
            try
            {
                foreach (NetworkPrefabEntry entry in GetRequiredEntries(targetMask))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (_registry.IsPrepared(entry.Id))
                        continue;

                    await _registry.PrepareAsync(entry.Id, cancellationToken);
                    context.AddNewlyPrepared(entry.Id);
                }

                cancellationToken.ThrowIfCancellationRequested();
                return context;
            }
            catch
            {
                ReleaseNewlyPreparedPrefabs(context);
                context.MarkRolledBack();
                throw;
            }
            finally
            {
                _isPreparing = false;
            }
        }

        /// <summary>
        /// Server根据已Prepare的目标Scope生成缺失的NetworkRoot。
        /// </summary>
        public void SpawnPreparedScope(NetworkScopePrepareContext context)
        {
            ValidateContext(context);
            EnsureServer("Spawn NetworkScope");

            try
            {
                foreach (NetworkPrefabEntry entry in GetRequiredEntries(context.TargetMask))
                {
                    if (HasLiveInstance(entry.Id))
                        continue;

                    if (!_registry.IsPrepared(entry.Id))
                    {
                        throw new InvalidOperationException(
                            $"NetworkPrefab 尚未 Prepare，禁止 Spawn：{entry.Id}");
                    }

                    GameObject prefab = _registry.GetPrefab(entry.Id);
                    ValidateManagedRootPrefab(entry, prefab);

                    GameObject instanceObject = UnityEngine.Object.Instantiate(prefab);
                    NetworkObject networkObject = instanceObject.GetComponent<NetworkObject>();

                    try
                    {
                        //禁止Scene销毁
                        networkObject.Spawn(destroyWithScene: false);
                    }
                    catch
                    {
                        if (networkObject.IsSpawned)
                            networkObject.Despawn();
                        UnityEngine.Object.Destroy(instanceObject);
                        throw;
                    }

                    context.AddNewlySpawned(entry.Id);
                    Debug.Log($"[NetworkScopeManager] NetworkRoot Spawn 完成：{entry.Id}");
                }
            }
            catch
            {
                DespawnPreparedScopeRoots(context);
                throw;
            }
        }
        /// <summary>
        /// 遍历确定每一个需求Root被加载
        /// </summary>
        public bool AreRequiredRootsReady(NetworkScopePrepareContext context)
        {
            ValidateContext(context);

            foreach (NetworkPrefabEntry entry in GetRequiredEntries(context.TargetMask))
            {
                if (_registrationFailures.TryGetValue(entry.Id, out Exception failure))
                    throw new InvalidOperationException($"NetworkRoot 登记失败：{entry.Id}", failure);
                if (!HasLiveInstance(entry.Id) || !_lifecycles.ContainsKey(entry.Id))
                    return false;
            }

            return true;
        }

        public void CommitPreparedScope(NetworkScopePrepareContext context)
        {
            ValidateContext(context);
            if (context.IsCommitted) return;
            if (!context.IsRuntimeReady)
                throw new InvalidOperationException("Bind/Initialize 尚未完成，禁止 Commit");

            if (_activeSceneMask != context.PreviousMask)
            {
                throw new InvalidOperationException(
                    "NetworkScope 状态已经发生变化，无法提交过期 Prepare。" +
                    $" Expected={context.PreviousMask}, Current={_activeSceneMask}");
            }

            if (!AreRequiredRootsReady(context))
                throw new InvalidOperationException("目标 NetworkScope 的 Root 尚未全部 Ready");
            //正式承认目标Scope，更新当前状态
            _activeSceneMask = context.TargetMask;
            context.MarkCommitted();
        }

        /// <summary>
        /// Commit前Rollback的第一步：Server撤销本轮新生成且旧Scope不需要的Root
        /// </summary>
        public void DespawnPreparedScopeRoots(NetworkScopePrepareContext context)
        {
            ValidateContext(context);
            if (context.IsCommitted)
                throw new InvalidOperationException("已经 Commit 的 Scope 不能回滚");
            EnsureServer("Rollback NetworkScope Root");

            IReadOnlyList<NetworkPrefabId> spawned = context.NewlySpawnedIds;
            for (int i = spawned.Count - 1; i >= 0; i--)
            {
                NetworkPrefabId id = spawned[i];
                NetworkPrefabEntry entry = GetEntry(id);

                if (entry.IsRequiredBy(context.PreviousMask))
                    continue;

                if (_instances.TryGetValue(id, out NetworkObject instance) &&
                    instance != null &&
                    instance.IsSpawned)
                {
                    instance.Despawn();
                }
            }
        }

        public bool ArePreparedScopeRootsDespawned(NetworkScopePrepareContext context)
        {
            ValidateContext(context);

            foreach (NetworkPrefabEntry entry in _catalog.Entries)
            {
                if (entry == null)
                    continue;

                bool neededByTarget = entry.IsRequiredBy(context.TargetMask);
                bool neededByPrevious = entry.IsRequiredBy(context.PreviousMask);

                if (neededByTarget && !neededByPrevious && HasLiveInstance(entry.Id))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Pre-Commit Rollback的第二步：确认新 Root 消失后，释放本轮新 Prepare 的 Prefab。
        /// </summary>
        public void ReleaseRolledBackScopePrefabs(NetworkScopePrepareContext context)
        {
            ValidateContext(context);
            if (context.IsCommitted)
                throw new InvalidOperationException("已经 Commit 的 Scope 不能回滚");

            if (!ArePreparedScopeRootsDespawned(context))
                throw new InvalidOperationException("新 NetworkRoot 尚未全部 Despawn");

            ReleaseNewlyPreparedPrefabs(context);
            context.MarkRolledBack();
        }

        /// <summary>
        /// 仅适用于本地已经没有新 Root 的同步回滚便利入口。
        /// 跨客户端流程应使用 Despawn → 等待 → Release 三段式屏障。
        /// </summary>
        public void RollbackPreparedScope(NetworkScopePrepareContext context)
        {
            ValidateContext(context);

            if (context.IsCommitted)
                throw new InvalidOperationException("已经 Commit 的 Scope 不能直接 Rollback");

            if (_networkManager.IsServer)
                DespawnPreparedScopeRoots(context);

            ReleaseRolledBackScopePrefabs(context);
        }

        public void DespawnObsoleteScopeRoots(NetworkScopePrepareContext context)
        {
            ValidateCommittedContext(context);
            EnsureServer("Despawn obsolete NetworkScope Root");

            foreach (NetworkPrefabEntry entry in _catalog.Entries
                         .Where(entry => entry != null)
                         .OrderByDescending(entry => entry.SpawnOrder))
            {
                if (entry.Lifetime == NetworkPrefabLifetime.Persistent)
                    continue;

                bool wasRequired = entry.IsRequiredBy(context.PreviousMask);
                bool stillRequired = entry.IsRequiredBy(context.TargetMask);
                if (!wasRequired || stillRequired)
                    continue;

                if (_instances.TryGetValue(entry.Id, out NetworkObject instance) &&
                    instance != null &&
                    instance.IsSpawned)
                {
                    instance.Despawn();
                }
            }
        }

        public bool AreObsoleteRootsDespawned(NetworkScopePrepareContext context)
        {
            ValidateCommittedContext(context);

            foreach (NetworkPrefabEntry entry in _catalog.Entries)
            {
                if (entry == null || entry.Lifetime == NetworkPrefabLifetime.Persistent)
                    continue;

                bool wasRequired = entry.IsRequiredBy(context.PreviousMask);
                bool stillRequired = entry.IsRequiredBy(context.TargetMask);
                //存在被之前Mask需要，但当前Msk不要的说明没有清理干净
                if (wasRequired && !stillRequired && HasLiveInstance(entry.Id))
                    return false;
            }

            return true;
        }

        public void ReleaseObsoleteScopePrefabs(NetworkScopePrepareContext context)
        {
            ValidateCommittedContext(context);

            if (!AreObsoleteRootsDespawned(context))
                throw new InvalidOperationException("旧 NetworkRoot 尚未全部 Despawn");

            foreach (NetworkPrefabEntry entry in _catalog.Entries
                         .Where(entry => entry != null)
                         .OrderByDescending(entry => entry.SpawnOrder))
            {
                if (entry.Lifetime == NetworkPrefabLifetime.Persistent)
                    continue;

                bool wasRequired = entry.IsRequiredBy(context.PreviousMask);
                bool stillRequired = entry.IsRequiredBy(context.TargetMask);
                if (wasRequired && !stillRequired && _registry.IsPrepared(entry.Id))
                    _registry.Release(entry.Id);
            }
            context.IsCleanedUp = true;
        }

        /// <summary>All Bind calls finish locally before any Initialize call. One combined ACK.</summary>
        public async UniTask RunPreCommitStagesAsync(NetworkScopePrepareContext context,
            int revision, CancellationToken cancellationToken)
        {
            ValidateContext(context);
            if (context.IsRuntimeReady) return;
            if (context.StagesStarted)
                throw new InvalidOperationException("本轮 Bind/Initialize 已启动，失败后必须回滚");
            if (!AreRequiredRootsReady(context))
                throw new InvalidOperationException("NetworkRoot 尚未 Ready");

            context.StagesStarted = true;
            context.Revision = revision;
            foreach (NetworkPrefabEntry entry in GetRequiredEntries(context.TargetMask))
            {
                NetworkScopeLifecycle lifecycle = _lifecycles[entry.Id];
                // Existing active Roots remain untouched so pre-commit rollback is local to new Roots.
                if (!lifecycle.IsActivated) context.Participants.Add(lifecycle);
            }
            var stageContext = new NetworkScopeStageContext(this, context.PreviousMask, context.TargetMask, revision);
            foreach (NetworkScopeLifecycle lifecycle in context.Participants)
            {
                foreach (IScopeBindable binder in lifecycle.Binders)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    lifecycle.RequireAlive();
                    NetworkScopeLifecycle.RequireComponent(binder);
                    await binder.BindAsync(stageContext, cancellationToken);
                }
            }
            foreach (NetworkScopeLifecycle lifecycle in context.Participants)
            {
                foreach (IScopeInitializable initializer in lifecycle.Initializers)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    lifecycle.RequireAlive();
                    NetworkScopeLifecycle.RequireComponent(initializer);
                    await initializer.InitializeAsync(stageContext, cancellationToken);
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            ValidateContext(context);
            if (!AreRequiredRootsReady(context))
                throw new InvalidOperationException("初始化期间 NetworkRoot 已消失");
            context.IsRuntimeReady = true;
        }

        public void ActivatePreparedScope(NetworkScopePrepareContext context)
        {
            ValidateCommittedContext(context);
            if (context.IsActivated) return;
            if (!context.IsRuntimeReady || !context.IsCleanedUp)
                throw new InvalidOperationException("Runtime Ready 和 Cleanup 完成后才能 Activate");
            if (context.ActivationStarted)
                throw new InvalidOperationException("Activate 失败后必须返回大厅，禁止重试部分激活的 Scope");
            context.ActivationStarted = true;
            var stageContext = new NetworkScopeStageContext(this, context.PreviousMask, context.TargetMask, context.Revision);
            foreach (NetworkScopeLifecycle lifecycle in context.Participants)
            {
                lifecycle.RequireAlive();
                foreach (IScopeActivatable activator in lifecycle.Activators)
                {
                    NetworkScopeLifecycle.RequireComponent(activator);
                    activator.Activate(stageContext);
                }
                lifecycle.IsActivated = true;
            }
            context.IsActivated = true;
            ScopeActivated?.Invoke(context.TargetMask);
        }

        public bool TryGetInstance(NetworkPrefabId id, out NetworkObject instance)
        {
            instance = null;
            return HasLiveInstance(id) && _instances.TryGetValue(id, out instance);
        }

        /// <summary>Drain business tasks while their Root and Addressables dependencies are still alive.</summary>
        public async UniTask ShutdownRootsAsync(NetworkScopePrepareContext context, bool rollback,
            bool recovery, CancellationToken cancellationToken)
        {
            foreach (NetworkPrefabEntry entry in _catalog.Entries.Where(e => e != null)
                         .OrderByDescending(e => e.SpawnOrder).ThenByDescending(e => e.Id))
            {
                bool selected = recovery
                    ? entry.Lifetime != NetworkPrefabLifetime.Persistent
                    : context != null && (rollback
                        ? entry.IsRequiredBy(context.TargetMask) && !entry.IsRequiredBy(context.PreviousMask)
                        : entry.Lifetime != NetworkPrefabLifetime.Persistent &&
                          entry.IsRequiredBy(context.PreviousMask) && !entry.IsRequiredBy(context.TargetMask));
                if (!selected || !_lifecycles.TryGetValue(entry.Id, out NetworkScopeLifecycle lifecycle)) continue;
                for (int i = lifecycle.ShutdownHandlers.Count - 1; i >= 0; i--)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await lifecycle.ShutdownHandlers[i].ShutdownScopeAsync(cancellationToken);
                }
            }
        }

        // Recovery deliberately discards all scene-scoped Roots. Persistent session Roots
        // (including the barriers) remain available to coordinate entry into Lobby.
        public void DespawnSceneScopedRootsForRecovery()
        {
            EnsureServer("Recover NetworkScope");
            foreach (NetworkPrefabEntry entry in _catalog.Entries.OrderByDescending(entry => entry.SpawnOrder))
                if (entry.Lifetime != NetworkPrefabLifetime.Persistent && TryGetInstance(entry.Id, out NetworkObject instance))
                    instance.Despawn();
        }

        public bool AreSceneScopedRootsDespawned()
        {
            return _catalog.Entries.All(entry => entry.Lifetime == NetworkPrefabLifetime.Persistent || !HasLiveInstance(entry.Id));
        }

        public void ReleaseSceneScopedPrefabsForRecovery()
        {
            if (_isPreparing || !AreSceneScopedRootsDespawned())
                throw new InvalidOperationException("在途 Prepare/Root 尚未结束，禁止恢复 Scope");
            foreach (NetworkPrefabEntry entry in _catalog.Entries.OrderByDescending(entry => entry.SpawnOrder))
                if (entry.Lifetime != NetworkPrefabLifetime.Persistent && _registry.IsPrepared(entry.Id))
                    _registry.Release(entry.Id);
            _activeSceneMask = NetworkSceneMask.None;
        }

        public bool HasInstance(NetworkPrefabId id)
        {
            return HasLiveInstance(id);
        }

        private bool HasLiveInstance(NetworkPrefabId id)
        {
            if (!_instances.TryGetValue(id, out NetworkObject instance))
                return false;

            if (instance != null && instance.IsSpawned)
                return true;

            _instances.Remove(id);
            _lifecycles.Remove(id);
            _registrationFailures.Remove(id);
            return false;
        }

        private List<NetworkPrefabEntry> GetRequiredEntries(
            NetworkSceneMask targetMask)
        {
            return _catalog.Entries
                .Where(entry => entry != null && entry.IsRequiredBy(targetMask))
                .OrderBy(entry => entry.SpawnOrder)
                .ThenBy(entry => entry.Id)
                .ToList();
        }

        private void ReleaseNewlyPreparedPrefabs(NetworkScopePrepareContext context)
        {
            IReadOnlyList<NetworkPrefabId> prepared = context.NewlyPreparedIds;
            for (int i = prepared.Count - 1; i >= 0; i--)
            {
                NetworkPrefabId id = prepared[i];
                if (HasLiveInstance(id))
                {
                    throw new InvalidOperationException(
                        $"NetworkPrefab {id} 仍存在实例，禁止 Release");
                }

                if (_registry.IsPrepared(id))
                    _registry.Release(id);
            }
        }

        private NetworkPrefabEntry GetEntry(NetworkPrefabId id)
        {
            if (!_catalog.TryGetEntry(id, out NetworkPrefabEntry entry))
                throw new KeyNotFoundException(
                    $"NetworkPrefabCatalog 中不存在 Id：{id}");

            return entry;
        }

        private static void MoveToOwnerScene(NetworkPrefabEntry entry, NetworkObject instance)
        {
            Scene ownerScene = SceneManager.GetSceneByName(entry.OwnerSceneName);
            if (!ownerScene.IsValid() || !ownerScene.isLoaded)
                throw new InvalidOperationException(
                    $"SceneScoped Root {entry.Id} 的 Owner Scene 尚未加载：{entry.OwnerSceneName}");
            if (instance.transform.parent != null)
                throw new InvalidOperationException($"SceneScoped Root {entry.Id} 必须是根 GameObject");
            if (instance.gameObject.scene != ownerScene)
                SceneManager.MoveGameObjectToScene(instance.gameObject, ownerScene);
        }

        private static void ValidateManagedRootPrefab(NetworkPrefabEntry entry,GameObject prefab)
        {
            NetworkObject networkObject = prefab.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                throw new InvalidOperationException(
                    $"NetworkPrefab 根节点缺少 NetworkObject：{entry.Id}");
            }

            NetworkScopeMember member = prefab.GetComponent<NetworkScopeMember>();
            if (member == null || member.Id != entry.Id)
            {
                throw new InvalidOperationException(
                    $"NetworkPrefab {entry.Id} 缺少匹配 Id 的 NetworkScopeMember");
            }
        }

        private void EnsureServer(string operation)
        {
            if (!_networkManager.IsServer)
                throw new InvalidOperationException($"只有 Server/Host 可以 {operation}");
        }

        private void ValidateContext(NetworkScopePrepareContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!ReferenceEquals(context.Owner, this))
                throw new InvalidOperationException("NetworkScopePrepareContext 不属于当前 Manager");

            if (context.IsRolledBack)
                throw new InvalidOperationException("该 NetworkScope Prepare 已经 Rollback");
            if (context.IsInvalidated)
                throw new InvalidOperationException("该 NetworkScope Context 已失效");
        }

        private void ValidateCommittedContext(NetworkScopePrepareContext context)
        {
            ValidateContext(context);
            if (!context.IsCommitted)
                throw new InvalidOperationException("NetworkScope 尚未 Commit");
        }
    }

    public sealed class NetworkScopePrepareContext
    {
        private readonly List<NetworkPrefabId> _newlyPreparedIds = new List<NetworkPrefabId>();
        private readonly List<NetworkPrefabId> _newlySpawnedIds = new List<NetworkPrefabId>();

        internal NetworkScopeManager Owner { get; }
        public NetworkSceneMask PreviousMask { get; }
        public NetworkSceneMask TargetMask { get; }
        public IReadOnlyList<NetworkPrefabId> NewlyPreparedIds => _newlyPreparedIds;
        public IReadOnlyList<NetworkPrefabId> NewlySpawnedIds => _newlySpawnedIds;
        public bool IsCommitted { get; private set; }
        public bool IsRolledBack { get; private set; }
        public bool IsRuntimeReady { get; internal set; }
        public bool IsCleanedUp { get; internal set; }
        public bool IsActivated { get; internal set; }
        public bool IsInvalidated { get; private set; }
        internal bool StagesStarted;
        internal bool ActivationStarted;
        internal int Revision;
        internal readonly List<NetworkScopeLifecycle> Participants = new List<NetworkScopeLifecycle>();

        public void Invalidate() => IsInvalidated = true;

        internal NetworkScopePrepareContext(NetworkScopeManager owner,NetworkSceneMask previousMask,NetworkSceneMask targetMask)
        {
            Owner = owner;
            PreviousMask = previousMask;
            TargetMask = targetMask;
        }

        internal void AddNewlyPrepared(NetworkPrefabId id)
        {
            _newlyPreparedIds.Add(id);
        }

        internal void AddNewlySpawned(NetworkPrefabId id)
        {
            _newlySpawnedIds.Add(id);
        }

        internal void MarkCommitted()
        {
            IsCommitted = true;
        }

        internal void MarkRolledBack()
        {
            IsRolledBack = true;
        }
    }
}
