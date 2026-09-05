using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectGame.HotFix.Network.Runtime
{
    /// <summary>
    /// 单个注册进上层网络的NetworkPrefab的定义
    /// 这里只保存配置，不保存Addressables Handle、运行时实例等动态状态
    /// </summary>
    [Serializable]
    public sealed class NetworkPrefabEntry 
    {
        [SerializeField] private NetworkPrefabId _id = NetworkPrefabId.Invalid;
        [SerializeField] private AssetReferenceGameObject _prefab;
        [SerializeField] private NetworkSceneMask _sceneMask = NetworkSceneMask.None;
        [SerializeField] private NetworkPrefabLifetime _lifetime = NetworkPrefabLifetime.SceneScoped;
        [SerializeField] private string _ownerSceneName;
        [SerializeField] private int _spawnOrder = 0;

        public NetworkPrefabId Id => _id;
        public AssetReferenceGameObject Prefab => _prefab;
        public NetworkSceneMask SceneMask => _sceneMask;
        public NetworkPrefabLifetime Lifetime => _lifetime;
        public string OwnerSceneName => _ownerSceneName;
        public int SpawnOrder => _spawnOrder;

        /// <summary>
        /// 当前激活的Scene Scope中，是否至少有一个Scope需要该对象。
        /// </summary>
        public bool IsRequiredBy(NetworkSceneMask activeSceneMask) => (_sceneMask & activeSceneMask) != 0;
    }

}
