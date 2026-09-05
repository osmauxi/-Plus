using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Network.Runtime
{
    /// <summary>
    /// 上层网络预制件静态目录,存放所有注册进上层网络的NetworkPrefab的定义
    /// 二轮可以直接挪到Excel表中
    /// </summary>
    [CreateAssetMenu(fileName = "NetworkPrefabCatalog",menuName = "ProjectGame/Network/Network Prefab Catalog")]
    public sealed class NetworkPrefabCatalog : ScriptableObject
    {
        [SerializeField] private List<NetworkPrefabEntry> _entries = new List<NetworkPrefabEntry>();

        public IReadOnlyList<NetworkPrefabEntry> Entries => _entries;

        /// <summary>
        /// 根据稳定Id查询配置，当前数量很少，直接线性查找
        /// </summary>
        public bool TryGetEntry(NetworkPrefabId id, out NetworkPrefabEntry entry)
        {
            foreach (NetworkPrefabEntry candidate in _entries)
            {
                if (candidate != null && candidate.Id == id)
                {
                    entry = candidate;
                    return true;
                }
            }

            entry = null;
            return false;
        }

        /// <summary>
        /// Runtime初始化时执行一次，暴露配置错误。
        /// </summary>
        public void ValidateOrThrow()
        {
            if (_entries == null)
                throw new InvalidOperationException("NetworkPrefabCatalog Entries 为空");

            var registeredIds = new HashSet<NetworkPrefabId>();

            for (int i = 0; i < _entries.Count; i++)
            {
                NetworkPrefabEntry entry = _entries[i];
                if (entry == null)
                    throw new InvalidOperationException($"NetworkPrefabCatalog Entries[{i}] 为空");

                if (entry.Id == NetworkPrefabId.Invalid)
                    throw new InvalidOperationException($"NetworkPrefabCatalog Entries[{i}] 未配置有效 NetworkPrefabId");

                if (!registeredIds.Add(entry.Id))
                    throw new InvalidOperationException($"NetworkPrefabCatalog 存在重复 NetworkPrefabId：{entry.Id}");

                if (entry.Prefab == null || !entry.Prefab.RuntimeKeyIsValid())
                    throw new InvalidOperationException($"NetworkPrefabCatalog {entry.Id} 未配置有效 Addressable Prefab");

                if (entry.SceneMask == NetworkSceneMask.None)
                    throw new InvalidOperationException($"NetworkPrefabCatalog {entry.Id} 的 SceneMask 不能为 None");

                if (entry.Lifetime == NetworkPrefabLifetime.SceneScoped &&
                    string.IsNullOrWhiteSpace(entry.OwnerSceneName))
                    throw new InvalidOperationException(
                        $"NetworkPrefabCatalog {entry.Id} 的 SceneScoped Root 未配置 OwnerSceneName");
            }
        }
    }
}
