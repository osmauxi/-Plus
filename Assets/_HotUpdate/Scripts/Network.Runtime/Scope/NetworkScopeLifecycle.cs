using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Network.Runtime
{
    // Only implement the stages a component needs. No default no-op methods.
    public interface IScopeBindable
    {
        UniTask BindAsync(NetworkScopeStageContext context, CancellationToken cancellationToken);
    }

    public interface IScopeInitializable
    {
        UniTask InitializeAsync(NetworkScopeStageContext context, CancellationToken cancellationToken);
    }

    public interface IScopeActivatable
    {
        void Activate(NetworkScopeStageContext context);
    }

    /// <summary>Optional asynchronous teardown. Cleanup waits on every peer before Server Despawn.</summary>
    public interface IScopeShutdown
    {
        UniTask ShutdownScopeAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// Bind/Initialize only prepare the new Root's state and must honor cancellation.
    /// Do not start gameplay or mutate a previously active Root before Activate.
    /// Each Root runs this lifecycle once per Spawn; retained Roots keep their runtime.
    /// </summary>
    public readonly struct NetworkScopeStageContext
    {
        private readonly NetworkScopeManager _manager;
        public NetworkSceneMask PreviousMask { get; }
        public NetworkSceneMask TargetMask { get; }
        public int Revision { get; }

        internal NetworkScopeStageContext(NetworkScopeManager manager,
            NetworkSceneMask previousMask, NetworkSceneMask targetMask, int revision)
        {
            _manager = manager;
            PreviousMask = previousMask;
            TargetMask = targetMask;
            Revision = revision;
        }

        public bool TryGetRoot(NetworkPrefabId id, out NetworkObject root)
        {
            root = null;
            return _manager != null && _manager.TryGetInstance(id, out root);
        }
    }

    /// <summary>Cached at local NGO Spawn registration, released at Despawn.</summary>
    internal sealed class NetworkScopeLifecycle
    {
        public readonly NetworkObject Root;
        public readonly List<IScopeBindable> Binders = new List<IScopeBindable>();
        public readonly List<IScopeInitializable> Initializers = new List<IScopeInitializable>();
        public readonly List<IScopeActivatable> Activators = new List<IScopeActivatable>();
        public readonly List<IScopeShutdown> ShutdownHandlers = new List<IScopeShutdown>();
        public bool IsActivated;

        public NetworkScopeLifecycle(NetworkObject root)
        {
            Root = root;
            foreach (MonoBehaviour component in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component == null || component.GetComponentInParent<NetworkObject>(true) != root)
                    continue;
                if (component is IScopeBindable binder) Binders.Add(binder);
                if (component is IScopeInitializable initializer) Initializers.Add(initializer);
                if (component is IScopeActivatable activator) Activators.Add(activator);
                if (component is IScopeShutdown shutdown) ShutdownHandlers.Add(shutdown);
            }
        }

        public void RequireAlive()
        {
            if (Root == null || !Root.IsSpawned)
                throw new InvalidOperationException("阶段执行期间 NetworkRoot 已 Despawn");
        }

        public static void RequireComponent(object component)
        {
            if (component is UnityEngine.Object unityObject && unityObject == null)
                throw new InvalidOperationException("阶段执行期间参与组件已销毁");
        }
    }
}
