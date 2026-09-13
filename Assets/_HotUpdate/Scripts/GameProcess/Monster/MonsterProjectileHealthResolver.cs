using System;
using ProjectGame.HotFix.Gameplay.Combat.Health;
using ProjectGame.HotFix.Gameplay.Weapon;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// ProjectileSimulation 的 Monster 命中适配器。Collider 仅存在于 View，身份和伤害均由集中表解析。
    /// </summary>
    public sealed class MonsterProjectileHealthResolver : IProjectileTargetResolver
    {
        private readonly HealthEntityBindings<Collider> _bindings;
        private readonly HealthRuntime _health;
        private readonly Func<uint> _tick;
        private ulong _nextEventId = 1;

        public MonsterProjectileHealthResolver(HealthEntityBindings<Collider> bindings,
            HealthRuntime health, Func<uint> tick)
        {
            _bindings = bindings ?? throw new ArgumentNullException(nameof(bindings));
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _tick = tick ?? throw new ArgumentNullException(nameof(tick));
        }

        public bool TryResolve(Collider collider, out ProjectileResolvedTarget target)
        {
            target = default;
            if (!_bindings.TryResolve(collider, out HealthEntity entity) ||
                !_health.TryGetHealth(entity, out HealthSnapshot state))
                return false;
            target = new ProjectileResolvedTarget(entity.EntityId, entity.Generation, state.IsAlive);
            return true;
        }

        public void ApplyDamage(in ProjectileResolvedTarget target, in ProjectileDamageContext context)
        {
            var entity = new HealthEntity(target.EntityId, target.Generation);
            if (!_health.TryGetHealth(entity, out HealthSnapshot state) || !state.IsAlive ||
                !MonsterMath.IsPositiveFinite(context.Damage))
                return;
            ulong eventId = _nextEventId++;
            if (_nextEventId == 0) _nextEventId = 1;
            HealthDamageFlags flags = context.IsCritical ? HealthDamageFlags.Critical : HealthDamageFlags.None;
            _health.TryEnqueue(HealthCommand.Damage(eventId, _tick(), default, entity,
                context.Damage, HealthDamageType.Physical, flags,
                impact: new HealthImpact(context.HitPoint, context.HitDirection,
                    origin: DamageOriginType.Projectile)));
        }
    }
}
