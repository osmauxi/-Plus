using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Weapon.Effects.Special
{
    public sealed class KineticBoostEffectSystem : WeaponSpecialEffectSystem
    {
        public const ushort Id = 2007;
        public override ushort EffectId => Id;

        public KineticBoostEffectSystem(IWeaponSpecialEffectCommandSink commands) : base(commands) { }

        public override void OnProjectileHit(byte level, ref ProjectileState projectile,
            in ProjectileHitContext context)
        {
            float tierOne = Mathf.Max(0f, 3f - 0.5f * (level - 1));
            float tierTwo = Mathf.Max(tierOne, 5f - 0.5f * (level - 1));
            float fireRate = context.Stats.FireRate;
            if (fireRate < tierOne) return;

            float duration = fireRate >= tierTwo
                ? 1f + 0.5f * (level - 1) + (fireRate - tierTwo) * 0.2f
                : 0.5f;
            Commands.ApplyCrowdControl(new CrowdControlEffectCommand(context, duration));
        }
    }
}
