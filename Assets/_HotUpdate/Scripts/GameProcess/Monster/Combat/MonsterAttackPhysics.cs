using ProjectGame.HotFix.Gameplay.Player;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>服务器怪物近战检测：取最近玩家并直接扣除玩家生命。</summary>
    public sealed class MonsterMeleePhysicsResolver
    {
        private readonly RaycastHit[] _hits = new RaycastHit[8];

        public void Execute(Vector3 origin, Vector3 direction, in MonsterAttackProfile profile, float damage)
        {
            int hitCount = Physics.SphereCastNonAlloc(origin, profile.Radius, direction,
                _hits, profile.Distance, profile.TargetMask, QueryTriggerInteraction.Collide);
            float nearestDistance = float.MaxValue;
            PlayerHealthNetworkState target = null;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = _hits[i];
                if (hit.collider == null || hit.distance >= nearestDistance)
                    continue;
                PlayerHealthNetworkState candidate = hit.collider.GetComponentInParent<PlayerHealthNetworkState>();
                if (candidate == null || !candidate.IsAlive)
                    continue;
                nearestDistance = hit.distance;
                target = candidate;
            }

            target?.Damage(damage);
        }
    }
}
