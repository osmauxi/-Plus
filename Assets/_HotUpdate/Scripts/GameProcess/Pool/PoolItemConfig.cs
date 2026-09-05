using System;

namespace ProjectGame.HotFix.Gameplay.Pooling
{
    /// <summary>
    /// 单个对象池的运行时配置，由 Excel 配表转换而来 
    /// </summary>
    public sealed class PoolItemConfig
    {
        public PoolItemConfig(string id, string prefabAddress, int initialCapacity, int maxSize)
        {
            Id = id;
            PrefabAddress = prefabAddress;
            InitialCapacity = initialCapacity;
            MaxSize = maxSize;
        }

        public string Id { get; }
        public string PrefabAddress { get; }
        public int InitialCapacity { get; }
        public int MaxSize { get; }

        public void Validate(string groupName)
        {
            if (string.IsNullOrWhiteSpace(Id))
                throw new InvalidOperationException($"对象池配置存在空 PoolId，Group={groupName}");

            if (string.IsNullOrWhiteSpace(PrefabAddress))
                throw new InvalidOperationException($"Pool={Id} 没有配置有效的 Addressable Prefab，Group={groupName}");

            if (InitialCapacity < 0)
                throw new InvalidOperationException($"Pool={Id} 的 InitialCapacity 不能小于 0 ");

            if (MaxSize < 1)
                throw new InvalidOperationException($"Pool={Id} 的 MaxSize 必须大于 0 ");

            if (InitialCapacity > MaxSize)
                throw new InvalidOperationException($"Pool={Id} 的 InitialCapacity 不能大于 MaxSize ");
        }
    }
}
