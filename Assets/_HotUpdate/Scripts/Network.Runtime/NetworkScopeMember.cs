using System;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Network.Runtime
{
    /// <summary>
    /// Managed NetworkRoot 的身份组件，只负责在 NGO Spawn/Despawn 时向本机 ScopeManager 报到。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    public sealed class NetworkScopeMember : NetworkBehaviour
    {
        [SerializeField] private NetworkPrefabId _id = NetworkPrefabId.Invalid;

        public NetworkPrefabId Id => _id;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (_id == NetworkPrefabId.Invalid)
                throw new InvalidOperationException(
                    $"{name} 的 NetworkScopeMember 未配置有效 NetworkPrefabId");

            NetworkRuntimeBootstrap bootstrap = NetworkRuntimeBootstrap.Instance;
            if (bootstrap == null || !bootstrap.IsInitialized)
                throw new InvalidOperationException(
                    $"{name} Spawn 时 NetworkRuntimeBootstrap 尚未初始化");

            bootstrap.ScopeManager.RegisterSpawnedInstance(_id, NetworkObject);
        }

        public override void OnNetworkDespawn()
        {
            NetworkRuntimeBootstrap bootstrap = NetworkRuntimeBootstrap.Instance;
            if (bootstrap != null && bootstrap.IsInitialized)
            {
                bootstrap.ScopeManager.UnregisterSpawnedInstance(
                    _id,
                    NetworkObject);
            }

            base.OnNetworkDespawn();
        }
    }
}
