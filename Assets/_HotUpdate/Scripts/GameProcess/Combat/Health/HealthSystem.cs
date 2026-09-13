using System;

namespace ProjectGame.HotFix.Gameplay.Combat.Health
{
    /// <summary>只计算 HP/护盾/生死状态。调用方已完成目标、Tick 与事件去重校验。</summary>
    internal static class HealthSystem
    {
        internal static HealthResult Apply(HealthWorld world, HealthWorld.Entry entry, in HealthCommand command, uint tick)
        {
            var before = entry.State;
            var definition = before.Definition;
            float hp = before.CurrentHealth;
            float shield = before.CurrentShield;
            bool invulnerable = before.IsInvulnerable;
            bool hasDamage = before.HasTakenDamage;
            uint lastDamageTick = before.LastDamageTick;
            float mitigated = 0;
            bool clearGates = false;
            var gateKey = new HealthWorld.GateKey(command.Source, command.GateChannel);
            bool usesGate = command.GateChannel != 0 && definition.DamageGateTicks > 0 &&
                (command.Flags & HealthDamageFlags.BypassDamageGate) == 0;

            switch (command.Kind)
            {
                case HealthCommandKind.Damage:
                    if (!before.IsAlive) return Reject(command, tick, HealthResultCode.Dead, before);
                    if (invulnerable && (command.Flags & HealthDamageFlags.BypassInvulnerability) == 0)
                        return Reject(command, tick, HealthResultCode.Invulnerable, before);
                    if (usesGate && entry.Gates.TryGetValue(gateKey, out uint gateTick) &&
                        unchecked(tick - gateTick) < definition.DamageGateTicks)
                        return Reject(command, tick, HealthResultCode.DamageGated, before);

                    // double 中间值防止极大但有限的输入在加法/乘法中溢出；不强制最低 1 点伤害。
                    double remaining = command.DamageType == HealthDamageType.True ? command.Amount :
                        command.Amount * (100.0 / (100.0 + definition.Defense));
                    mitigated = (float)remaining;
                    if ((command.Flags & HealthDamageFlags.BypassShield) == 0)
                    {
                        double absorbed = Math.Min(shield, remaining);
                        shield = (float)Math.Max(0, shield - absorbed);
                        remaining -= absorbed;
                    }
                    hp = (float)Math.Max(0, hp - remaining);
                    if (hp == before.CurrentHealth && shield == before.CurrentShield)
                        return new HealthResult(command, tick, HealthResultCode.NoChange, before, before, mitigated);
                    hasDamage = true;
                    lastDamageTick = tick;
                    break;
                case HealthCommandKind.Heal:
                    if (!before.IsAlive) return Reject(command, tick, HealthResultCode.Dead, before);
                    hp = (float)Math.Min(definition.MaxHealth, (double)hp + command.Amount);
                    break;
                case HealthCommandKind.AddShield:
                    if (!before.IsAlive) return Reject(command, tick, HealthResultCode.Dead, before);
                    shield = (float)Math.Min(definition.MaxShield, (double)shield + command.Amount);
                    break;
                case HealthCommandKind.Revive:
                    if (before.IsAlive) return Reject(command, tick, HealthResultCode.AlreadyAlive, before);
                    hp = Math.Min(definition.MaxHealth, command.Amount);
                    shield = 0;
                    invulnerable = false;
                    hasDamage = false;
                    lastDamageTick = 0;
                    clearGates = true;
                    break;
                case HealthCommandKind.SetInvulnerable:
                    invulnerable = command.Amount != 0;
                    break;
                case HealthCommandKind.Reconfigure:
                    definition = command.Definition;
                    hp = Math.Min(hp, definition.MaxHealth);
                    shield = Math.Min(shield, definition.MaxShield);
                    clearGates = true;
                    break;
                default:
                    return Reject(command, tick, HealthResultCode.InvalidCommand, before);
            }

            if (hp == before.CurrentHealth && shield == before.CurrentShield && invulnerable == before.IsInvulnerable &&
                ReferenceEquals(definition, before.Definition) && hasDamage == before.HasTakenDamage &&
                lastDamageTick == before.LastDamageTick)
                return Reject(command, tick, HealthResultCode.NoChange, before);

            var after = new HealthSnapshot(before.Entity, definition, hp, shield, invulnerable,
                lastDamageTick, hasDamage, world.NextRevision());
            entry.State = after;
            if (clearGates) entry.Gates.Clear();
            if (command.Kind == HealthCommandKind.Damage && usesGate) entry.Gates[gateKey] = tick;
            return new HealthResult(command, tick, HealthResultCode.Applied, before, after, mitigated);
        }

        internal static HealthResult Reject(in HealthCommand command, uint tick, HealthResultCode code,
            in HealthSnapshot state = default) => new HealthResult(command, tick, code, state, state);
    }
}
