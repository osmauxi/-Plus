using System;
using System.Collections.Generic;

namespace ProjectGame.HotFix.Gameplay.Combat.Health
{
    /// <summary>EntityId 可为 0；Generation=0 表示无实体。每次注册都分配新代次。</summary>
    public readonly struct HealthEntity : IEquatable<HealthEntity>
    {
        public readonly ulong EntityId;
        public readonly uint Generation;
        public bool IsValid => Generation != 0;

        public HealthEntity(ulong entityId, uint generation)
        {
            EntityId = entityId;
            Generation = generation;
        }

        public bool Equals(HealthEntity other) => EntityId == other.EntityId && Generation == other.Generation;
        public override bool Equals(object obj) => obj is HealthEntity other && Equals(other);
        public override int GetHashCode() => unchecked((EntityId.GetHashCode() * 397) ^ (int)Generation);
        public override string ToString() => $"{EntityId}:{Generation}";
        public static bool operator ==(HealthEntity a, HealthEntity b) => a.Equals(b);
        public static bool operator !=(HealthEntity a, HealthEntity b) => !a.Equals(b);
    }

    /// <summary>玩家、怪物、可破坏物共享定义；配置适配器将读表结果复制为不可变定义。</summary>
    public sealed class HealthDefinition
    {
        public int ProfileId { get; }
        public float MaxHealth { get; }
        public float MaxShield { get; }
        public float Defense { get; }
        public uint DamageGateTicks { get; }

        public HealthDefinition(int profileId, float maxHealth, float maxShield = 0,
            float defense = 0, uint damageGateTicks = 0)
        {
            if (profileId < 0 || !HealthMath.Positive(maxHealth) ||
                !HealthMath.NonNegative(maxShield) || !HealthMath.NonNegative(defense) ||
                damageGateTicks > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(maxHealth), "Health 配置包含非法 ID、数值或 Tick 时长。");
            ProfileId = profileId;
            MaxHealth = maxHealth;
            MaxShield = maxShield;
            Defense = defense;
            DamageGateTicks = damageGateTicks;
        }
    }

    public interface IHealthDefinitionSource
    {
        HealthDefinition GetDefinition(int profileId);
    }

    /// <summary>独立于 ConfigManager 的参数目录；装配层负责转换配置，无内置玩家/怪物特判。</summary>
    public sealed class HealthCatalog : IHealthDefinitionSource
    {
        private readonly Dictionary<int, HealthDefinition> _definitions = new Dictionary<int, HealthDefinition>();

        public HealthCatalog(IEnumerable<HealthDefinition> definitions)
        {
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));
            foreach (var definition in definitions)
            {
                if (definition == null) throw new ArgumentException("Health 定义不能为空。", nameof(definitions));
                _definitions.Add(definition.ProfileId, definition);
            }
        }

        public HealthDefinition GetDefinition(int profileId) => _definitions.TryGetValue(profileId, out var definition)
            ? definition : throw new KeyNotFoundException($"HealthProfile={profileId} 未注册。");
    }

    /// <summary>只读值副本；死亡由 Health==0 唯一确定，不能独立写入第二份生命状态。</summary>
    public readonly struct HealthSnapshot
    {
        public readonly HealthEntity Entity;
        public readonly HealthDefinition Definition;
        public readonly float CurrentHealth;
        public readonly float CurrentShield;
        public readonly bool IsInvulnerable;
        public readonly uint LastDamageTick;
        public readonly bool HasTakenDamage;
        public readonly ulong Revision;
        public bool IsAlive => CurrentHealth > 0;

        public HealthSnapshot(HealthEntity entity, HealthDefinition definition, float currentHealth,
            float currentShield, bool isInvulnerable, uint lastDamageTick, bool hasTakenDamage, ulong revision)
        {
            if (!entity.IsValid || definition == null || revision == 0 ||
                !HealthMath.NonNegative(currentHealth) || currentHealth > definition.MaxHealth ||
                !HealthMath.NonNegative(currentShield) || currentShield > definition.MaxShield)
                throw new ArgumentException("非法 Health 快照。");
            Entity = entity;
            Definition = definition;
            CurrentHealth = currentHealth;
            CurrentShield = currentShield;
            IsInvulnerable = isInvulnerable;
            LastDamageTick = lastDamageTick;
            HasTakenDamage = hasTakenDamage;
            Revision = revision;
        }
    }

    public enum HealthChangeKind : byte { Registered, Updated, Removed }

    public readonly struct HealthChange
    {
        public readonly HealthChangeKind Kind;
        // Removed 时 State 是移除前的最终副本；移除版本单独记录。
        public readonly HealthSnapshot State;
        public readonly ulong Revision;
        public HealthChange(HealthChangeKind kind, in HealthSnapshot state, ulong revision)
        { Kind = kind; State = state; Revision = revision; }
    }

    /// <summary>UI 可同时读取权威端和客户端镜像；不暴露结算或可写容器。</summary>
    public interface IHealthStateSource
    {
        bool TryGetHealth(HealthEntity entity, out HealthSnapshot state);
        bool TryGetEntity(ulong entityId, out HealthEntity entity);
        event Action<HealthChange> StateChanged;
    }

    internal static class HealthMath
    {
        internal static bool NonNegative(float value) => value >= 0 && !float.IsInfinity(value);
        internal static bool Positive(float value) => value > 0 && !float.IsInfinity(value);
        internal static bool IsNewer(uint tick, uint previous) => unchecked((int)(tick - previous)) > 0;
    }

    /// <summary>通知发生在提交之后；单个订阅者异常不能跳过其他订阅者或撤销已经提交的结算。</summary>
    internal static class HealthNotifications
    {
        internal static void Send<T>(Action<T> handlers, T value, ref List<Exception> errors)
        {
            if (handlers == null) return;
            foreach (Action<T> handler in handlers.GetInvocationList())
            {
                try { handler(value); }
                catch (Exception ex)
                {
                    if (errors == null) errors = new List<Exception>();
                    errors.Add(ex);
                }
            }
        }

        internal static void ThrowIfAny(List<Exception> errors)
        {
            if (errors != null) throw new AggregateException("Health 状态已提交；部分通知订阅者执行失败，请勿重放该 Tick。", errors);
        }
    }
}
