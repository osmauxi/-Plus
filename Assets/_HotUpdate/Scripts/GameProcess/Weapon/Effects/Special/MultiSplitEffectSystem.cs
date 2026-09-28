using System;

namespace ProjectGame.HotFix.Gameplay.Weapon.Effects.Special
{
    public sealed class MultiSplitEffectSystem : WeaponSpecialEffectSystem
    {
        public const ushort Id = 2008;
        public override ushort EffectId => Id;

        public MultiSplitEffectSystem(IWeaponSpecialEffectCommandSink commands) : base(commands) { }

        public override void OnProjectileHit(byte level, ref ProjectileState projectile,
            in ProjectileHitContext context)
        {
            if (projectile.Generation >= 1) return;
            ushort count = checked((ushort)(2 + level - 1));
            Commands.SpawnProjectiles(new SplitProjectileEffectCommand(
                projectile, context, count, 90f, 0.6f));
        }
    }
}
