using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Combat.Health;
using ProjectGame.HotFix.Gameplay.Weapon.Effects.Special;

namespace ProjectGame.HotFix.Gameplay.Weapon.Effects
{
    /// <summary>
    /// Weapon Effect 与 Health 的窄桥接：只处理护盾容量重配置和加盾命令。
    /// 闪电、风暴云由其他战斗命令适配器实现，不在这里伪造成直接伤害。
    /// </summary>
    public sealed class HealthShieldEffectAdapter : IEffectOwnerStatSink, IWeaponSpecialEffectCommandSink
    {
        private readonly IHealthStateSource _health;
        private readonly IHealthCommandSink _commands;
        private readonly Func<uint> _tick;
        private readonly Dictionary<ulong, float> _baseCapacityByOwner = new();
        private readonly Dictionary<OwnerEffectKey, float> _specialCapacityByOwnerEffect = new();
        private ulong _nextEventId = 1;

        private readonly struct OwnerEffectKey : IEquatable<OwnerEffectKey>
        {
            public readonly ulong OwnerEntityId;
            public readonly ushort EffectId;

            public OwnerEffectKey(ulong ownerEntityId, ushort effectId)
            {
                OwnerEntityId = ownerEntityId;
                EffectId = effectId;
            }

            public bool Equals(OwnerEffectKey other) =>
                OwnerEntityId == other.OwnerEntityId && EffectId == other.EffectId;
            public override bool Equals(object obj) => obj is OwnerEffectKey other && Equals(other);
            public override int GetHashCode() =>
                unchecked((OwnerEntityId.GetHashCode() * 397) ^ EffectId.GetHashCode());
        }

        public HealthShieldEffectAdapter(IHealthStateSource health, IHealthCommandSink commands, Func<uint> tick)
        {
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _tick = tick ?? throw new ArgumentNullException(nameof(tick));
        }

        public void Apply(ulong ownerEntityId, in EffectOwnerStatSnapshot stats)
        {
            _baseCapacityByOwner[ownerEntityId] = stats.ShieldCapacity;
            ReconfigureShieldCapacity(ownerEntityId);
        }

        public void AddShield(in ShieldEffectCommand command)
        {
            if (!_health.TryGetEntity(command.OwnerEntityId, out HealthEntity entity)) return;
            _commands.TryEnqueue(HealthCommand.AddShield(
                NextEventId(), _tick(), entity, entity, command.Amount));
        }

        public void EmitLightning(in LightningEffectCommand command)
        {
            // 由范围/连锁伤害模块的组合根接管；Health 适配器不猜测目标选择规则。
        }

        public void SpawnStormCloud(in StormCloudEffectCommand command)
        {
            // 由对象池和持续伤害模块的组合根接管。
        }

        public void RemoveOwnerEffect(in OwnerEffectRemovalCommand command)
        {
            var key = new OwnerEffectKey(command.OwnerEntityId, command.EffectId);
            if (_specialCapacityByOwnerEffect.Remove(key))
                ReconfigureShieldCapacity(command.OwnerEntityId);
        }

        public void SetShieldCapacity(in ShieldCapacityEffectCommand command)
        {
            var key = new OwnerEffectKey(command.OwnerEntityId, command.EffectId);
            if (command.CapacityContribution > 0f)
                _specialCapacityByOwnerEffect[key] = command.CapacityContribution;
            else
                _specialCapacityByOwnerEffect.Remove(key);

            // 装备事件先于数值快照发布；等待 Apply 提供基础容量后再提交一次完整配置。
            if (_baseCapacityByOwner.ContainsKey(command.OwnerEntityId))
                ReconfigureShieldCapacity(command.OwnerEntityId);
        }

        public void TryExecute(in ExecuteEffectCommand command) { }
        public void ApplyCrowdControl(in CrowdControlEffectCommand command) { }
        public void SpawnProjectiles(in SplitProjectileEffectCommand command) { }
        public void EmitRadialImpact(in RadialImpactEffectCommand command) { }
        public void UpsertDamageArea(in PersistentAreaEffectCommand command) { }

        private void ReconfigureShieldCapacity(ulong ownerEntityId)
        {
            if (!_baseCapacityByOwner.TryGetValue(ownerEntityId, out float capacity) ||
                !_health.TryGetEntity(ownerEntityId, out HealthEntity entity) ||
                !_health.TryGetHealth(entity, out HealthSnapshot current))
                return;

            foreach (KeyValuePair<OwnerEffectKey, float> pair in _specialCapacityByOwnerEffect)
                if (pair.Key.OwnerEntityId == ownerEntityId)
                    capacity += pair.Value;

            if (Math.Abs(current.Definition.MaxShield - capacity) <= 0.0001f)
                return;

            var definition = new HealthDefinition(
                current.Definition.ProfileId,
                current.Definition.MaxHealth,
                capacity,
                current.Definition.Defense,
                current.Definition.DamageGateTicks);
            _commands.TryEnqueue(HealthCommand.Reconfigure(NextEventId(), _tick(), entity, definition));
        }

        private ulong NextEventId()
        {
            ulong id = _nextEventId++;
            if (_nextEventId == 0) _nextEventId = 1;
            return id;
        }
    }
}
