using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 攻击状态机三阶段，准备，攻击前摇和攻击后摇
    /// </summary>
    public enum MonsterAttackPhase : byte
    {
        Ready,
        Windup,
        Recovery,
    }
    #region 网络同步数据
    //SoA,将不同模块数据进行拆分，而不是面向对象中一个结构放一起，这里进行简单的SoA拆分
    //优化结构的同时不提高太多复杂度
    public struct MonsterMetaData
    {
        /// <summary>
        /// 怪物对应MonsterRuntimeCatalog中的基础数据ID
        /// </summary>
        public ushort ConfigIndex;
        /// <summary>
        /// 是否存活，非存活不会进行更新
        /// </summary>
        public bool IsActive;
    }
    /// <summary>
    /// 权威移动状态数据
    /// </summary>
    public struct MonsterMotionData
    {
        //因为游戏特性，基本能视为X_Z平面的二维游戏，所以数据均被进行压缩
        public Vector2 Position;
        public Vector2 Velocity;
        public float Yaw;
    }
    /// <summary>只存在于服务器 MonsterWorld，不进入网络快照。</summary>
    public struct MonsterHealthData
    {
        public float CurrentHealth;
    }

    /// <summary>服务器内部的减速与停顿状态，不参与网络同步。</summary>
    public struct MonsterStatusData
    {
        public float SpeedMultiplier;
        public uint SlowEndTick;
        public uint StunEndTick;
    }

    public struct MonsterAttackData
    {
        /// <summary>
        /// 当前处在的攻击状态
        /// </summary>
        public MonsterAttackPhase Phase;
        /// <summary>
        /// 此状态多少Tick后结束
        /// </summary>
        public uint PhaseEndTick;
    }

    public struct MonsterPresentationData
    {
        /// <summary>
        /// 使用检测序列号变化的方式进行攻击行为判定
        /// </summary>
        public byte AttackSequence;
        /// <summary>
        /// 使用序列号变化驱动一次受击表现；只属于表现同步，不参与伤害判定。
        /// </summary>
        public byte HitSequence;
    }

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
            if (!MonsterMath.IsFinite(quantizationOrigin) ||
                !MonsterMath.IsPositiveFinite(quantizationSize.x) ||
                !MonsterMath.IsPositiveFinite(quantizationSize.y) ||
                !MonsterMath.IsFinite(groundY))
                throw new ArgumentOutOfRangeException(nameof(quantizationSize));
            QuantizationOrigin = quantizationOrigin;
            QuantizationSize = quantizationSize;
            GroundY = groundY;
            MinimumAcceptedTick = minimumAcceptedTick;
        }
    }
    #endregion
    public readonly struct MonsterPlayerTarget
    {
        public readonly Vector2 Position;

        public MonsterPlayerTarget(Vector2 position)
        {
            if (!MonsterMath.IsFinite(position))
                throw new ArgumentOutOfRangeException(nameof(position));
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
            if (configId < 0 || !MonsterMath.IsNonNegativeFinite(moveSpeed) ||
                !MonsterMath.IsPositiveFinite(attackRange) ||
                windupTicks == 0 || windupTicks > int.MaxValue ||
                recoveryTicks == 0 || recoveryTicks > int.MaxValue ||
                !MonsterMath.IsPositiveFinite(attackDamage) ||
                !MonsterMath.IsPositiveFinite(maxHealth))
                throw new ArgumentOutOfRangeException(nameof(configId), "Monster Runtime 配置包含非法字段。");
            double attackRangeSqr = (double)attackRange * attackRange;
            if (attackRangeSqr > float.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(attackRange), "Monster AttackRange 平方后超出 float 范围。");

            ConfigId = configId;
            MoveSpeed = moveSpeed;
            AttackRangeSqr = (float)attackRangeSqr;
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

        /// <summary>
        /// 虽然本身属于数组，但内部存的也是值类型结构体，访问内部对象默认依然走值类型赋值
        /// 为了省这部分开销使用ref强制引用传递
        /// </summary>
        public ref readonly MonsterRuntimeConfig Get(ushort index)
        {
            if (index >= _items.Length) 
                throw new IndexOutOfRangeException($"Monster ConfigIndex={index} 未注册。");
            return ref _items[index];
        }

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
            if (items == null) 
                throw new ArgumentNullException(nameof(items));
            _items = new List<MonsterAttackProfile>(items).ToArray();
            if (_items.Length > ushort.MaxValue + 1)
                throw new ArgumentException("Monster AttackProfile 超过 ushort 索引容量。", nameof(items));
        }

        public ref readonly MonsterAttackProfile Get(ushort index)
        {
            if (index >= _items.Length) 
                throw new IndexOutOfRangeException($"Monster AttackProfileIndex={index} 未注册。");
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
            if (items == null) 
                throw new ArgumentNullException(nameof(items));
            if (runtimeCatalog == null) 
                throw new ArgumentNullException(nameof(runtimeCatalog));
            _items = new List<MonsterSpawnConfig>(items).ToArray();
            //这里做Catalog的外键校验，保证其数据是有对应的，因为这些数据分了两个表
            for (int i = 0; i < _items.Length; i++)
                runtimeCatalog.Get(_items[i].ConfigIndex);
        }

        public ref readonly MonsterSpawnConfig Get(int index)
        {
            if ((uint)index >= (uint)_items.Length) 
                throw new IndexOutOfRangeException();
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
        private readonly string[] _localPoolIds;

        public IReadOnlyList<string> LocalPoolIds => _localPoolIds;

        public MonsterViewCatalog(IEnumerable<MonsterViewConfig> items, MonsterRuntimeCatalog runtimeCatalog)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (runtimeCatalog == null) throw new ArgumentNullException(nameof(runtimeCatalog));
            var poolIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (MonsterViewConfig item in items)
            {
                if (item == null) 
                    throw new ArgumentException("Monster View 配置不能为空。", nameof(items));
                //外键检查，这也是单独一个表
                runtimeCatalog.Get(item.ConfigIndex);
                _items.Add(item.ConfigIndex, item);
                poolIds.Add(item.LocalPoolId);
            }
            _localPoolIds = new string[poolIds.Count];
            poolIds.CopyTo(_localPoolIds);
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
