using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectGame.HotFix.Network.Runtime
{
    /// <summary>
    /// NetworkPrefabEntry对应的运行时状态。
    /// </summary>
    internal sealed class NetworkPrefabRuntimeState
    {
        public NetworkPrefabEntry Entry { get; }
        public AsyncOperationHandle<GameObject> Handle;
        public GameObject Prefab;
        public bool IsRegistered;

        public NetworkPrefabRuntimeState(NetworkPrefabEntry entry)
        {
            Entry = entry;
        }

        public void Reset()
        {
            Handle = default;
            Prefab = null;
            IsRegistered = false;
        }
    }
}