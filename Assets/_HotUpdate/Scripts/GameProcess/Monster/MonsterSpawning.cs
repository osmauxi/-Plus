using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Spawning;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>低频、配置驱动的预算选择器；固定 seed 可用于权威回放与测试。</summary>
    public sealed class MonsterSpawnSelector
    {
        private readonly MonsterSpawnCatalog _catalog;
        private readonly System.Random _random;

        public MonsterSpawnSelector(MonsterSpawnCatalog catalog, int? seed = null)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        }

        /// <returns>无法继续购买后的剩余预算。</returns>
        public int Select(int budget, int difficulty, int maxCount, List<ushort> destination)
        {
            if (budget < 0) throw new ArgumentOutOfRangeException(nameof(budget));
            if (difficulty < 0) throw new ArgumentOutOfRangeException(nameof(difficulty));
            if (maxCount < 0) throw new ArgumentOutOfRangeException(nameof(maxCount));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            destination.Clear();
            var selectedPerEntry = new int[_catalog.Count];

            while (budget > 0 && destination.Count < maxCount)
            {
                double totalWeight = 0;
                for (int i = 0; i < _catalog.Count; i++)
                {
                    ref readonly MonsterSpawnConfig item = ref _catalog.Get(i);
                    if (item.Cost <= budget && item.IsAvailable(difficulty) && selectedPerEntry[i] < item.MaxPerWave)
                        totalWeight += item.Weight;
                }
                if (totalWeight <= 0) break;

                double choice = _random.NextDouble() * totalWeight;
                int selectedIndex = -1;
                for (int i = 0; i < _catalog.Count; i++)
                {
                    ref readonly MonsterSpawnConfig item = ref _catalog.Get(i);
                    if (item.Cost > budget || !item.IsAvailable(difficulty) || selectedPerEntry[i] >= item.MaxPerWave)
                        continue;
                    choice -= item.Weight;
                    if (choice > 0) continue;
                    selectedIndex = i;
                    break;
                }
                // 浮点尾差兜底：寻找最后一个有效候选。
                if (selectedIndex < 0)
                    for (int i = _catalog.Count - 1; i >= 0; i--)
                    {
                        ref readonly MonsterSpawnConfig item = ref _catalog.Get(i);
                        if (item.Cost <= budget && item.IsAvailable(difficulty) && selectedPerEntry[i] < item.MaxPerWave)
                        { selectedIndex = i; break; }
                    }
                if (selectedIndex < 0) break;

                ref readonly MonsterSpawnConfig selected = ref _catalog.Get(selectedIndex);
                destination.Add(selected.ConfigIndex);
                selectedPerEntry[selectedIndex]++;
                budget -= selected.Cost;
            }
            return budget;
        }
    }

    public readonly struct MonsterSpawnPlan
    {
        public readonly ushort ConfigIndex;
        public readonly Vector2 Position;
        public readonly float Yaw;

        public MonsterSpawnPlan(ushort configIndex, Vector2 position, float yaw)
        {
            if (!MonsterMath.IsFinite(position) || !MonsterMath.IsFinite(yaw))
                throw new ArgumentOutOfRangeException(nameof(position));
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
            _sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));
            if (!MonsterMath.IsNonNegativeFinite(minimumSpacing))
                throw new ArgumentOutOfRangeException(nameof(minimumSpacing));
            if (!MonsterMath.IsFinite(groundY) || !MonsterMath.IsNonNegativeFinite(groundHeightTolerance))
                throw new ArgumentOutOfRangeException(nameof(groundY));
            if (attemptsPerPoint < 1) throw new ArgumentOutOfRangeException(nameof(attemptsPerPoint));
            _random = new System.Random(seed);
            _minimumSpacing = minimumSpacing;
            _groundY = groundY;
            _groundHeightTolerance = groundHeightTolerance;
            _attemptsPerPoint = attemptsPerPoint;
        }

        public bool TryBuildPlans(IReadOnlyList<ushort> configIndices, MonsterPlayerTargetBuffer players,
            List<MonsterSpawnPlan> destination)
        {
            if (configIndices == null) throw new ArgumentNullException(nameof(configIndices));
            if (players == null) throw new ArgumentNullException(nameof(players));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
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
        public readonly ushort ConfigIndex;
        public readonly uint PackedPose;
        public MonsterSpawnData(ushort configIndex, uint packedPose)
        { ConfigIndex = configIndex; PackedPose = packedPose; }
    }

    /// <summary>一波可靠静态出生数据，数组顺序隐式映射连续 Slot。</summary>
    public sealed class MonsterSpawnBatch
    {
        private readonly MonsterSpawnData[] _items;
        private readonly IReadOnlyList<MonsterSpawnData> _readOnlyItems;
        public int StartSlot { get; }
        public int Count => _items.Length;
        public IReadOnlyList<MonsterSpawnData> Items => _readOnlyItems;

        public MonsterSpawnBatch(int startSlot, IEnumerable<MonsterSpawnData> items)
        {
            if (startSlot < 0) throw new ArgumentOutOfRangeException(nameof(startSlot));
            if (items == null) throw new ArgumentNullException(nameof(items));
            _items = new List<MonsterSpawnData>(items).ToArray();
            if ((long)startSlot + _items.Length > int.MaxValue)
                throw new ArgumentException("Monster SpawnBatch Slot 范围溢出。", nameof(items));
            _readOnlyItems = Array.AsReadOnly(_items);
            StartSlot = startSlot;
        }

        public static MonsterSpawnBatch FromPlans(int startSlot, IReadOnlyList<MonsterSpawnPlan> plans,
            MonsterPoseQuantizer quantizer)
        {
            if (plans == null) throw new ArgumentNullException(nameof(plans));
            if (quantizer == null) throw new ArgumentNullException(nameof(quantizer));
            var items = new MonsterSpawnData[plans.Count];
            for (int i = 0; i < plans.Count; i++)
                items[i] = new MonsterSpawnData(plans[i].ConfigIndex,
                    quantizer.Pack(plans[i].Position, plans[i].Yaw));
            return new MonsterSpawnBatch(startSlot, items);
        }
    }
}
