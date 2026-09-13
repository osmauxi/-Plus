using System;
using ProjectGame.HotFix.Gameplay.Combat.Health;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// Melee 状态机的 Unity Physics 边界。只使用 NonAlloc Query 和集中 Collider→HealthEntity 映射，
    /// 不在热路径调用 GetComponent/GetComponentInParent。
    /// </summary>
    public sealed class MonsterMeleePhysicsResolver : IMonsterAttackResolver
    {
        private readonly HealthEntityBindings<Collider> _targetBindings;
        private readonly IHealthStateSource _health;
        private readonly IHealthCommandSink _commands;
        private readonly MonsterHealthBinding _monsters;
        private readonly RaycastHit[] _hits;
        private ulong _nextEventId = 1;

        public MonsterMeleePhysicsResolver(HealthEntityBindings<Collider> targetBindings,
            IHealthStateSource health, IHealthCommandSink commands, MonsterHealthBinding monsters,
            int queryCapacity = 8)
        {
            _targetBindings = targetBindings ?? throw new ArgumentNullException(nameof(targetBindings));
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _monsters = monsters ?? throw new ArgumentNullException(nameof(monsters));
            if (queryCapacity < 1) throw new ArgumentOutOfRangeException(nameof(queryCapacity));
            _hits = new RaycastHit[queryCapacity];
        }

        public void Execute(in MonsterAttackRequest request)
        {
            int hitCount = Physics.SphereCastNonAlloc(request.Origin, request.Radius, request.Direction,
                _hits, request.Distance, request.TargetMask, QueryTriggerInteraction.Collide);
            float nearestDistance = float.MaxValue;
            HealthEntity target = default;
            Vector3 hitPoint = default;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = _hits[i];
                if (hit.collider == null || hit.distance >= nearestDistance ||
                    !_targetBindings.TryResolve(hit.collider, out HealthEntity candidate) ||
                    !_health.TryGetHealth(candidate, out HealthSnapshot state) || !state.IsAlive)
                    continue;
                nearestDistance = hit.distance;
                target = candidate;
                hitPoint = hit.point;
            }
            if (!target.IsValid) return;

            HealthEntity source = default;
            _monsters.TryGetHealth(request.Slot, out source);
            ulong eventId = _nextEventId++;
            if (_nextEventId == 0) _nextEventId = 1;
            _commands.TryEnqueue(HealthCommand.Damage(eventId, request.Tick, source, target,
                request.Damage, HealthDamageType.Physical,
                impact: new HealthImpact(hitPoint, request.Direction, origin: DamageOriginType.Melee)));
        }
    }
}
