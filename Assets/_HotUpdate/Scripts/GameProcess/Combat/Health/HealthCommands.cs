using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Combat.Health
{
    public enum HealthCommandKind : byte { Damage, Heal, AddShield, Revive, SetInvulnerable, Reconfigure }
    public enum HealthDamageType : byte { Physical, True }

    public enum DamageOriginType : byte
    {
        Unknown,
        Projectile,
        Melee,
        Effect,
        Environment,
    }

    [Flags]
    public enum HealthDamageFlags : byte
    {
        None = 0,
        BypassShield = 1,
        BypassInvulnerability = 2,
        BypassDamageGate = 4,
        Critical = 8,
        SuppressReaction = 16,
    }

    /// <summary>反应的输入上下文。力度独立于伤害；模块不执行位移、冻结动画或播放特效。</summary>
    public readonly struct HealthImpact
    {
        public readonly Vector3 Point;
        public readonly Vector3 Direction;
        public readonly float Strength;
        public readonly int ReactionProfileId;
        public readonly DamageOriginType Origin;
        public HealthImpact(Vector3 point, Vector3 direction, float strength = 0, int reactionProfileId = 0,
            DamageOriginType origin = DamageOriginType.Unknown)
        {
            Point = point;
            Direction = direction;
            Strength = strength;
            ReactionProfileId = reactionProfileId;
            Origin = origin;
        }

        internal bool IsValid => Finite(Point) && Finite(Direction) && HealthMath.NonNegative(Strength) &&
            ReactionProfileId >= 0 && Origin <= DamageOriginType.Environment;
        private static bool Finite(Vector3 value) =>
            !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
            !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
            !float.IsNaN(value.z) && !float.IsInfinity(value.z);
    }

    /// <summary>
    /// 仅由权威战斗逻辑创建。EventId 在 (Source, Target) 范围内跨命令类型唯一；重试保留原 Tick。
    /// Source 可为空（环境伤害）；GateChannel=0 关闭频率门控，步枪/霰弹丸默认使用 0。
    /// </summary>
    public readonly struct HealthCommand
    {
        public readonly ulong EventId;
        public readonly uint Tick;
        public readonly HealthEntity Source;
        public readonly HealthEntity Target;
        public readonly HealthCommandKind Kind;
        public readonly float Amount;
        public readonly HealthDamageType DamageType;
        public readonly HealthDamageFlags Flags;
        public readonly ushort GateChannel;
        public readonly HealthImpact Impact;
        public readonly HealthDefinition Definition;

        private HealthCommand(ulong eventId, uint tick, HealthEntity source, HealthEntity target,
            HealthCommandKind kind, float amount, HealthDamageType damageType = HealthDamageType.Physical,
            HealthDamageFlags flags = HealthDamageFlags.None, ushort gateChannel = 0,
            HealthImpact impact = default, HealthDefinition definition = null)
        {
            EventId = eventId; Tick = tick; Source = source; Target = target; Kind = kind; Amount = amount;
            DamageType = damageType; Flags = flags; GateChannel = gateChannel; Impact = impact; Definition = definition;
        }

        public static HealthCommand Damage(ulong eventId, uint tick, HealthEntity source, HealthEntity target,
            float amount, HealthDamageType damageType = HealthDamageType.Physical,
            HealthDamageFlags flags = HealthDamageFlags.None, ushort gateChannel = 0, HealthImpact impact = default) =>
            new HealthCommand(eventId, tick, source, target, HealthCommandKind.Damage, amount, damageType, flags, gateChannel, impact);

        public static HealthCommand Heal(ulong eventId, uint tick, HealthEntity source, HealthEntity target, float amount) =>
            new HealthCommand(eventId, tick, source, target, HealthCommandKind.Heal, amount);
        public static HealthCommand AddShield(ulong eventId, uint tick, HealthEntity source, HealthEntity target, float amount) =>
            new HealthCommand(eventId, tick, source, target, HealthCommandKind.AddShield, amount);
        /// <summary>显式复活；清空护盾、无敌标记和伤害门控，恢复指定 HP（受上限约束）。</summary>
        public static HealthCommand Revive(ulong eventId, uint tick, HealthEntity source, HealthEntity target, float health) =>
            new HealthCommand(eventId, tick, source, target, HealthCommandKind.Revive, health);
        public static HealthCommand SetInvulnerable(ulong eventId, uint tick, HealthEntity target, bool enabled) =>
            new HealthCommand(eventId, tick, default, target, HealthCommandKind.SetInvulnerable, enabled ? 1 : 0);
        /// <summary>更换属性快照，仅裁剪到新上限，不自动补血、不复活。</summary>
        public static HealthCommand Reconfigure(ulong eventId, uint tick, HealthEntity target, HealthDefinition definition) =>
            new HealthCommand(eventId, tick, default, target, HealthCommandKind.Reconfigure, 0, definition: definition);

        internal bool IsValid => EventId != 0 && Target.IsValid &&
            (Source.IsValid || Source.EntityId == 0) && Impact.IsValid &&
            DamageType <= HealthDamageType.True && (Flags & ~(HealthDamageFlags)31) == 0 &&
            (Kind == HealthCommandKind.Reconfigure ? Definition != null :
                Kind == HealthCommandKind.SetInvulnerable ? Amount == 0 || Amount == 1 :
                Kind <= HealthCommandKind.Revive && HealthMath.Positive(Amount));
    }

    public enum HealthResultCode : byte
    {
        Applied, NoChange, InvalidCommand, UnknownTarget, Duplicate, Expired, FutureTick,
        Dead, AlreadyAlive, Invulnerable, DamageGated,
    }

    /// <summary>用于伤害数字、仇恨、统计和反应路由；实际扣除量不含过量伤害。</summary>
    public readonly struct HealthResult
    {
        public readonly HealthCommand Command;
        public readonly uint AppliedTick;
        public readonly HealthResultCode Code;
        public readonly HealthSnapshot Before;
        public readonly HealthSnapshot After;
        public readonly float MitigatedDamage;
        public float HealthLost => Math.Max(0, Before.CurrentHealth - After.CurrentHealth);
        public float ShieldLost => Math.Max(0, Before.CurrentShield - After.CurrentShield);
        public bool WasKilled => Code == HealthResultCode.Applied && Before.IsAlive && !After.IsAlive;
        public bool WasRevived => Code == HealthResultCode.Applied && !Before.IsAlive && After.IsAlive;
        public bool ShieldBroken => Code == HealthResultCode.Applied && Before.CurrentShield > 0 && After.CurrentShield == 0;
        public bool ShouldReact => Code == HealthResultCode.Applied && Command.Kind == HealthCommandKind.Damage &&
            HealthLost + ShieldLost > 0 && (Command.Flags & HealthDamageFlags.SuppressReaction) == 0;

        internal HealthResult(in HealthCommand command, uint tick, HealthResultCode code,
            in HealthSnapshot before, in HealthSnapshot after, float mitigatedDamage = 0)
        { Command = command; AppliedTick = tick; Code = code; Before = before; After = after; MitigatedDamage = mitigatedDamage; }
    }

    public interface IHealthCommandSink
    {
        /// <summary>队列满时返回 false；不覆盖旧请求。结算成功与否通过 CommandResolved 获得。</summary>
        bool TryEnqueue(in HealthCommand command);
    }

    public interface IHealthEventSource
    {
        event Action<HealthResult> CommandResolved;
        event Action<HealthResult> Died;
        event Action<HealthResult> Revived;
        event Action<HealthResult> ReactionRequested;
    }
}
