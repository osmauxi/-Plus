using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Spawning;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 低频、配置驱动的预算选择器；固定 seed 可用于权威回放与测试。
    /// </summary>
    public sealed class MonsterSpawnSelector
    {
        private readonly MonsterSpawnCatalog _catalog;
        private readonly System.Random _random;
        private readonly int[] _selectedPerEntry;

        public MonsterSpawnSelector(MonsterSpawnCatalog catalog, int? seed = null)
        {
            _catalog = catalog;
            _random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
            _selectedPerEntry = new int[_catalog.Count];
        }

        /// <returns>无法继续购买后的剩余预算。</returns>
        public int Select(int budget, int difficulty, int maxCount, List<ushort> destination)
        {
            destination.Clear();
            Array.Clear(_selectedPerEntry, 0, _selectedPerEntry.Length);

            while (budget > 0 && destination.Count < maxCount)
            {
                double totalWeight = 0;
                for (int i = 0; i < _catalog.Count; i++)
                {
                    ref readonly MonsterSpawnConfig item = ref _catalog.Get(i);
                    if (item.Cost <= budget && item.IsAvailable(difficulty) && _selectedPerEntry[i] < item.MaxPerWave)
                        totalWeight += item.Weight;
                }
                if (totalWeight <= 0) break;

                double choice = _random.NextDouble() * totalWeight;
                int selectedIndex = -1;
                int lastValidIndex = -1;
                for (int i = 0; i < _catalog.Count; i++)
                {
                    ref readonly MonsterSpawnConfig item = ref _catalog.Get(i);
                    if (item.Cost > budget || !item.IsAvailable(difficulty) || _selectedPerEntry[i] >= item.MaxPerWave)
                        continue;
                    lastValidIndex = i;
                    choice -= item.Weight;
                    if (choice > 0) continue;
                    selectedIndex = i;
                    break;
                }
                if (selectedIndex < 0)
                    selectedIndex = lastValidIndex;

                ref readonly MonsterSpawnConfig selected = ref _catalog.Get(selectedIndex);
                destination.Add(selected.ConfigIndex);
                _selectedPerEntry[selectedIndex]++;
                budget -= selected.Cost;
            }
            return budget;
        }
    }

    /// <summary>
    /// 一个已经规划好的怪物生成信息
    /// </summary>
    public readonly struct MonsterSpawnPlan
    {
        public readonly ushort ConfigIndex;
        public readonly Vector2 Position;
        public readonly float Yaw;

        public MonsterSpawnPlan(ushort configIndex, Vector2 position, float yaw)
        {
            ConfigIndex = configIndex;
            Position = position;
            Yaw = yaw;
        }
    }

    /// <summary>房间空间算法的稳定边界；具体 SpawnArea/Ground/Clearance 策略留给房间模块。</summary>
    public interface IMonsterSpawnPlanner
    {
        bool TryBuildPlans(IReadOnlyList<ushort> configIndices, MonsterPlayerTargetBuffer players,
            List<MonsterSpawnPlan> destination);
    }

    /// <summary>
    /// 当前波次一次性随机采样全部落点。只有完整波次规划成功才向 Manager 返回，避免发布半个 SpawnBatch。
    /// MonsterWorld 仍是二维 XZ，因此同一房间的可用地面必须接近统一 GroundY。
    /// </summary>
    public sealed class MonsterRandomGroundSpawnPlanner : IMonsterSpawnPlanner
    {
        private readonly RoomSpawnRegion _region;
        private readonly RoomGroundPointSampler _sampler;
        private readonly System.Random _random;
        private readonly float _minimumSpacing;
        private readonly float _groundY;
        private readonly float _groundHeightTolerance;
        private readonly int _attemptsPerPoint;
        private readonly List<Vector3> _positions = new();

        public MonsterRandomGroundSpawnPlanner(in RoomSpawnRegion region, RoomGroundPointSampler sampler,
            int seed, float minimumSpacing, float groundY, float groundHeightTolerance,
            int attemptsPerPoint = 48)
        {
            _region = region;
            _sampler = sampler;
            _random = new System.Random(seed);
            _minimumSpacing = minimumSpacing;
            _groundY = groundY;
            _groundHeightTolerance = groundHeightTolerance;
            _attemptsPerPoint = attemptsPerPoint;
        }

        public bool TryBuildPlans(IReadOnlyList<ushort> configIndices, MonsterPlayerTargetBuffer players,
            List<MonsterSpawnPlan> destination)
        {
            destination.Clear();
            if (!_sampler.TrySampleMany(_region, configIndices.Count, 1f, _minimumSpacing,
                    _random, _positions, _attemptsPerPoint))
                return false;
            for (int i = 0; i < _positions.Count; i++)
            {
                Vector3 position = _positions[i];
                if (Mathf.Abs(position.y - _groundY) > _groundHeightTolerance)
                {
                    destination.Clear();
                    return false;
                }
                destination.Add(new MonsterSpawnPlan(configIndices[i],
                    new Vector2(position.x, position.z), (float)_random.NextDouble() * 360f));
            }
            return true;
        }
    }

    public readonly struct MonsterSpawnData
    {
        public readonly ulong EntityId;
        public readonly ushort ConfigIndex;
        public readonly uint PackedPose;
        public MonsterSpawnData(ulong entityId, ushort configIndex, uint packedPose)
        {
            EntityId = entityId;
            ConfigIndex = configIndex;
            PackedPose = packedPose;
        }
    }

    /// <summary>
    /// 一波可靠静态出生数据。数组顺序只表示本批次的写入顺序，网络身份由 EntityId 显式携带。
    /// </summary>
    public sealed class MonsterSpawnBatch
    {
        private readonly MonsterSpawnData[] _items;
        public int Count => _items.Length;
        public IReadOnlyList<MonsterSpawnData> Items => _items;

        public MonsterSpawnBatch(MonsterSpawnData[] items)
        {
            _items = items;
        }

        public static MonsterSpawnBatch FromPlans(
            int startStorageIndex,
            IReadOnlyList<MonsterSpawnPlan> plans,
            MonsterPoseQuantizer quantizer,
            MonsterWorld world)
        {
            var items = new MonsterSpawnData[plans.Count];
            for (int i = 0; i < plans.Count; i++)
                items[i] = new MonsterSpawnData(world.GetEntityId(startStorageIndex + i), plans[i].ConfigIndex,
                    quantizer.Pack(plans[i].Position, plans[i].Yaw));
            return new MonsterSpawnBatch(items);
        }
    }
}
