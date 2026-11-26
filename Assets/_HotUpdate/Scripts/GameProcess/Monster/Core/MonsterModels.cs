using System.Collections.Generic;
using ProjectGame.Core.Monsters;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    public readonly struct MonsterRoomBeginData
    {
        /// <summary>
        /// MonsterPoseQuantizer使用的XZ量化矩形的起点和尺寸
        /// 因为房间基本一样，其实可以不传数据只传房间ID让客户端自己算
        /// </summary>
        public readonly Vector2 QuantizationOrigin;
        public readonly Vector2 QuantizationSize;
        public readonly float GroundY;
        public readonly uint MinimumAcceptedTick;

        public MonsterRoomBeginData(Vector2 quantizationOrigin, Vector2 quantizationSize,
            float groundY, uint minimumAcceptedTick)
        {
            QuantizationOrigin = quantizationOrigin;
            QuantizationSize = quantizationSize;
            GroundY = groundY;
            MinimumAcceptedTick = minimumAcceptedTick;
        }
    }

    public readonly struct MonsterPlayerTarget
    {
        public readonly Vector2 Position;

        public MonsterPlayerTarget(Vector2 position)
        {
            Position = position;
        }
    }

    /// <summary>
    /// 对原始ConfigManager数据处理过后的专用只读怪物配置数据
    /// </summary>
    public readonly struct MonsterRuntimeConfig
    {
        /// <summary>
        /// 对原始Config处理后的新ID，Monster系统内部引用是基于此新ID，而不是Config的原始ID
        /// </summary>
        public readonly int ConfigId;
        public readonly float MoveSpeed;
        public readonly float AttackRangeSqr;
        public readonly uint WindupTicks;
        public readonly uint RecoveryTicks;
        public readonly float AttackDamage;
        public readonly float MaxHealth;
        /// <summary>
        /// 处理后的对应原始Config攻击模组数据的ID
        /// </summary>
        public readonly ushort AttackProfileIndex;

        public MonsterRuntimeConfig(int configId, float moveSpeed, float attackRange,
            uint windupTicks, uint recoveryTicks, float attackDamage,
            float maxHealth, ushort attackProfileIndex)
        {
            ConfigId = configId;
            MoveSpeed = moveSpeed;
            AttackRangeSqr = attackRange * attackRange;
            WindupTicks = windupTicks;
            RecoveryTicks = recoveryTicks;
            AttackDamage = attackDamage;
            MaxHealth = maxHealth;
            AttackProfileIndex = attackProfileIndex;
        }
    }

    public sealed class MonsterRuntimeCatalog
    {
        private readonly MonsterRuntimeConfig[] _items;
        private readonly MonsterSimulationConfig[] _simulationItems;
        private readonly Dictionary<int, ushort> _indices = new Dictionary<int, ushort>();

        public int Count => _items.Length;

        public MonsterRuntimeCatalog(MonsterRuntimeConfig[] items)
        {
            _items = items;
            _simulationItems = new MonsterSimulationConfig[items.Length];
            for (int i = 0; i < _items.Length; i++)
            {
                _indices.Add(_items[i].ConfigId, (ushort)i);
                MonsterRuntimeConfig config = _items[i];
                _simulationItems[i] = new MonsterSimulationConfig(
                    config.MoveSpeed,
                    config.AttackRangeSqr,
                    config.WindupTicks,
                    config.RecoveryTicks,
                    config.AttackDamage,
                    config.AttackProfileIndex);
            }
        }

        /// <summary>
        /// 虽然本身属于数组，但内部存的也是值类型结构体，访问内部对象默认依然走值类型赋值
        /// 为了省这部分开销使用ref强制引用传递
        /// </summary>
        public ref readonly MonsterRuntimeConfig Get(ushort index)
        {
            return ref _items[index];
        }

        internal MonsterSimulationConfig[] SimulationItems => _simulationItems;

        public bool TryGetIndex(int configId, out ushort index) => _indices.TryGetValue(configId, out index);
    }

    /// <summary>
    /// 攻击检测的空间参数
    /// </summary>
    public readonly struct MonsterAttackProfile
    {
        public readonly Vector3 OriginOffset;
        public readonly float Radius;
        /// <summary>
        /// 攻击判定的检测长度
        /// </summary>
        public readonly float Distance;
        public readonly LayerMask TargetMask;

        public MonsterAttackProfile(Vector3 originOffset, float radius, float distance, LayerMask targetMask)
        {
            OriginOffset = originOffset;
            Radius = radius;
            Distance = distance;
            TargetMask = targetMask;
        }
    }

    public sealed class MonsterAttackCatalog
    {
        private readonly MonsterAttackProfile[] _items;
        public int Count => _items.Length;

        public MonsterAttackCatalog(MonsterAttackProfile[] items)
        {
            _items = items;
        }

        public ref readonly MonsterAttackProfile Get(ushort index)
        {
            return ref _items[index];
        }
    }

    /// <summary>
    /// 怪物生成配置，指定某一种怪物在波次生成系统的中的生成规则
    /// </summary>
    public readonly struct MonsterSpawnConfig
    {
        /// <summary>
        /// 这条配置指向哪一种Monster
        /// </summary>
        public readonly ushort ConfigIndex;
        public readonly int Cost;
        /// <summary>
        /// 相对权重
        /// </summary>
        public readonly float Weight;
        public readonly int MinDifficulty;
        public readonly int MaxDifficulty;
        public readonly int MaxPerWave;

        public MonsterSpawnConfig(ushort configIndex, int cost, float weight,
            int minDifficulty = 0, int maxDifficulty = int.MaxValue, int maxPerWave = int.MaxValue)
        {
            ConfigIndex = configIndex;
            Cost = cost;
            Weight = weight;
            MinDifficulty = minDifficulty;
            MaxDifficulty = maxDifficulty;
            MaxPerWave = maxPerWave;
        }

        public bool IsAvailable(int difficulty) => difficulty >= MinDifficulty && difficulty <= MaxDifficulty;
    }

    public sealed class MonsterSpawnCatalog
    {
        private readonly MonsterSpawnConfig[] _items;
        public int Count => _items.Length;

        public MonsterSpawnCatalog(MonsterSpawnConfig[] items)
        {
            _items = items;
        }

        public ref readonly MonsterSpawnConfig Get(int index)
        {
            return ref _items[index];
        }
    }

    public sealed class MonsterViewConfig
    {
        public ushort ConfigIndex { get; }
        public string LocalPoolId { get; }

        public MonsterViewConfig(ushort configIndex, string localPoolId)
        {
            ConfigIndex = configIndex;
            LocalPoolId = localPoolId;
        }
    }

    public sealed class MonsterViewCatalog
    {
        private readonly Dictionary<ushort, MonsterViewConfig> _items = new Dictionary<ushort, MonsterViewConfig>();

        public MonsterViewCatalog(IEnumerable<MonsterViewConfig> items)
        {
            foreach (MonsterViewConfig item in items)
                _items.Add(item.ConfigIndex, item);
        }

        public MonsterViewConfig Get(ushort configIndex) => _items[configIndex];
    }

    internal static class MonsterMath
    {
        internal static bool HasReached(uint tick, uint deadline) => unchecked((int)(tick - deadline)) >= 0;
        internal static bool IsNewer(uint tick, uint previous) => unchecked((int)(tick - previous)) > 0;
    }
}
