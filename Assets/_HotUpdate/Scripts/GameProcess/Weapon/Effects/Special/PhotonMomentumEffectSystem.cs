using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Weapon.Effects.Special
{
    public sealed class PhotonMomentumEffectSystem : WeaponSpecialEffectSystem
    {
        public const ushort Id = 2004;
        public override ushort EffectId => Id;
        public PhotonMomentumEffectSystem(IWeaponSpecialEffectCommandSink commands) : base(commands) { }

        public override void OnProjectileHit(byte level, ref ProjectileState projectile,
            in ProjectileHitContext context)
        {
            projectile.DamageMultiplier *= 1f + 0.05f + 0.05f * level;
            projectile.SizeMultiplier = Mathf.Max(0.2f, projectile.SizeMultiplier * 0.95f);
        }
    }
}
