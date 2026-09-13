using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    public enum MonsterTargetModule : byte { NearestTarget }
    public enum MonsterMoveModule : byte { DirectChase }
    public enum MonsterAttackModule : byte { MeleeAttack }

    public enum MonsterAttackPhase : byte
    {
        Ready,
        Windup,
        Recovery,
    }

    public struct MonsterMetaData
    {
        public ushort ConfigIndex;
        public bool IsActive;
    }

    public struct MonsterMotionData
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Yaw;
    }

    public struct MonsterTargetData
    {
        public ulong ClientId;
        public Vector2 Position;
        public float DistanceSqr;
        public bool HasTarget;
    }

    public struct MonsterMoveData
    {
        public Vector2 DesiredVelocity;
    }

    public struct MonsterAttackData
    {
        public MonsterAttackPhase Phase;
        public uint PhaseEndTick;
    }

    public struct MonsterPresentationData
    {
        public byte AttackSequence;
    }

    public readonly struct MonsterPlayerTarget
    {
        public readonly ulong ClientId;
        public readonly Vector2 Position;

        public MonsterPlayerTarget(ulong clientId, Vector2 position)
        {
            if (!MonsterMath.IsFinite(position))
                throw new ArgumentOutOfRangeException(nameof(position));
            ClientId = clientId;
            Position = position;
        }
    }

    /// <summary>配置层转换后的 Tick 热路径数据；距离平方在构造阶段预计算。</summary>
    public readonly struct MonsterRuntimeConfig
    {
        public readonly int ConfigId;
        public readonly float MoveSpeed;
        public readonly MonsterTargetModule TargetModule;
        public readonly MonsterMoveModule MoveModule;
        public readonly MonsterAttackModule AttackModule;
        public readonly float AttackRangeSqr;
        public readonly uint WindupTicks;
        public readonly uint RecoveryTicks;
        public readonly float AttackDamage;
        public readonly int HealthProfileId;
        public readonly ushort AttackProfileIndex;

        public MonsterRuntimeConfig(int configId, float moveSpeed,
            MonsterTargetModule targetModule, MonsterMoveModule moveModule,
            MonsterAttackModule attackModule, float attackRange,
            uint windupTicks, uint recoveryTicks, float attackDamage,
            int healthProfileId, ushort attackProfileIndex)
        {
            if (configId < 0 || !MonsterMath.IsNonNegativeFinite(moveSpeed) ||
                !MonsterMath.IsPositiveFinite(attackRange) ||
                windupTicks == 0 || windupTicks > int.MaxValue ||
                recoveryTicks == 0 || recoveryTicks > int.MaxValue ||
                !MonsterMath.IsPositiveFinite(attackDamage) || healthProfileId < 0 ||
                targetModule != MonsterTargetModule.NearestTarget ||
                moveModule != MonsterMoveModule.DirectChase ||
                attackModule != MonsterAttackModule.MeleeAttack)
                throw new ArgumentOutOfRangeException(nameof(configId), "Monster Runtime 配置包含非法字段。");
            double attackRangeSqr = (double)attackRange * attackRange;
            if (attackRangeSqr > float.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(attackRange), "Monster AttackRange 平方后超出 float 范围。");

            ConfigId = configId;
            MoveSpeed = moveSpeed;
            TargetModule = targetModule;
            MoveModule = moveModule;
            AttackModule = attackModule;
            AttackRangeSqr = (float)attackRangeSqr;
            WindupTicks = windupTicks;
            RecoveryTicks = recoveryTicks;
            AttackDamage = attackDamage;
            HealthProfileId = healthProfileId;
            AttackProfileIndex = attackProfileIndex;
        }
    }

    public sealed class MonsterRuntimeCatalog
    {
        private readonly MonsterRuntimeConfig[] _items;
        private readonly Dictionary<int, ushort> _indices = new Dictionary<int, ushort>();

        public int Count => _items.Length;

        public MonsterRuntimeCatalog(IEnumerable<MonsterRuntimeConfig> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            var list = new List<MonsterRuntimeConfig>(items);
            if (list.Count > ushort.MaxValue + 1)
                throw new ArgumentException("Monster Runtime 配置超过 ushort 索引容量。", nameof(items));
            _items = list.ToArray();
            for (int i = 0; i < _items.Length; i++)
            {
                if (_indices.ContainsKey(_items[i].ConfigId))
                    throw new ArgumentException($"重复 Monster ConfigId={_items[i].ConfigId}。", nameof(items));
                _indices.Add(_items[i].ConfigId, checked((ushort)i));
            }
        }

        public ref readonly MonsterRuntimeConfig Get(ushort index)
        {
            if (index >= _items.Length) throw new IndexOutOfRangeException($"Monster ConfigIndex={index} 未注册。");
            return ref _items[index];
        }

        public bool TryGetIndex(int configId, out ushort index) => _indices.TryGetValue(configId, out index);
    }

    public readonly struct MonsterAttackProfile
    {
        public readonly Vector3 OriginOffset;
        public readonly float Radius;
        public readonly float Distance;
        public readonly LayerMask TargetMask;

        public MonsterAttackProfile(Vector3 originOffset, float radius, float distance, LayerMask targetMask)
        {
            if (!MonsterMath.IsFinite(originOffset) || !MonsterMath.IsPositiveFinite(radius) ||
                !MonsterMath.IsNonNegativeFinite(distance))
                throw new ArgumentOutOfRangeException(nameof(radius), "Monster AttackProfile 包含非法空间参数。");
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

        public MonsterAttackCatalog(IEnumerable<MonsterAttackProfile> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            _items = new List<MonsterAttackProfile>(items).ToArray();
            if (_items.Length > ushort.MaxValue + 1)
                throw new ArgumentException("Monster AttackProfile 超过 ushort 索引容量。", nameof(items));
        }

        public ref readonly MonsterAttackProfile Get(ushort index)
        {
            if (index >= _items.Length) throw new IndexOutOfRangeException($"Monster AttackProfileIndex={index} 未注册。");
            return ref _items[index];
        }
    }

    public readonly struct MonsterSpawnConfig
    {
        public readonly ushort ConfigIndex;
        public readonly int Cost;
        public readonly float Weight;
        public readonly int MinDifficulty;
        public readonly int MaxDifficulty;
        public readonly int MaxPerWave;

        public MonsterSpawnConfig(ushort configIndex, int cost, float weight,
            int minDifficulty = 0, int maxDifficulty = int.MaxValue, int maxPerWave = int.MaxValue)
        {
            if (cost <= 0 || !MonsterMath.IsPositiveFinite(weight) || minDifficulty < 0 ||
                maxDifficulty < minDifficulty || maxPerWave <= 0)
                throw new ArgumentOutOfRangeException(nameof(cost), "Monster Spawn 配置包含非法字段。");
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

        public MonsterSpawnCatalog(IEnumerable<MonsterSpawnConfig> items, MonsterRuntimeCatalog runtimeCatalog)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (runtimeCatalog == null) throw new ArgumentNullException(nameof(runtimeCatalog));
            _items = new List<MonsterSpawnConfig>(items).ToArray();
            for (int i = 0; i < _items.Length; i++)
                runtimeCatalog.Get(_items[i].ConfigIndex);
        }

        public ref readonly MonsterSpawnConfig Get(int index)
        {
            if ((uint)index >= (uint)_items.Length) throw new IndexOutOfRangeException();
            return ref _items[index];
        }
    }

    public sealed class MonsterViewConfig
    {
        public ushort ConfigIndex { get; }
        public string LocalPoolId { get; }

        public MonsterViewConfig(ushort configIndex, string localPoolId)
        {
            if (string.IsNullOrWhiteSpace(localPoolId)) throw new ArgumentException("Monster View PoolId 不能为空。", nameof(localPoolId));
            ConfigIndex = configIndex;
            LocalPoolId = localPoolId;
        }
    }

    public sealed class MonsterViewCatalog
    {
        private readonly Dictionary<ushort, MonsterViewConfig> _items = new Dictionary<ushort, MonsterViewConfig>();

        public MonsterViewCatalog(IEnumerable<MonsterViewConfig> items, MonsterRuntimeCatalog runtimeCatalog)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (runtimeCatalog == null) throw new ArgumentNullException(nameof(runtimeCatalog));
            foreach (MonsterViewConfig item in items)
            {
                if (item == null) throw new ArgumentException("Monster View 配置不能为空。", nameof(items));
                runtimeCatalog.Get(item.ConfigIndex);
                _items.Add(item.ConfigIndex, item);
            }
        }

        public MonsterViewConfig Get(ushort configIndex) => _items.TryGetValue(configIndex, out var item)
            ? item : throw new KeyNotFoundException($"Monster View ConfigIndex={configIndex} 未注册。");
    }

    internal static class MonsterMath
    {
        internal static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        internal static bool IsNonNegativeFinite(float value) => value >= 0 && IsFinite(value);
        internal static bool IsPositiveFinite(float value) => value > 0 && IsFinite(value);
        internal static bool IsFinite(Vector2 value) => IsFinite(value.x) && IsFinite(value.y);
        internal static bool IsFinite(Vector3 value) => IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        internal static bool HasReached(uint tick, uint deadline) => unchecked((int)(tick - deadline)) >= 0;
        internal static bool IsNewer(uint tick, uint previous) => unchecked((int)(tick - previous)) > 0;
    }
}
