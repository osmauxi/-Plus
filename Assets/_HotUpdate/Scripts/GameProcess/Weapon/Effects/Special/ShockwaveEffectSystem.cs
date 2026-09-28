namespace ProjectGame.HotFix.Gameplay.Weapon.Effects.Special
{
    public sealed class ShockwaveEffectSystem : WeaponSpecialEffectSystem
    {
        public const ushort Id = 2011;
        public override ushort EffectId => Id;

        public ShockwaveEffectSystem(IWeaponSpecialEffectCommandSink commands) : base(commands) { }

        public override void OnProjectileDestroyed(byte level, ref ProjectileState projectile,
            in ProjectileDestroyedEffectContext context)
        {
            float radius = (5f + 1.5f * (level - 1)) *
                           context.Stats.ProjectileSize * projectile.SizeMultiplier;
            float force = 25f + 10f * (level - 1);
            Commands.EmitRadialImpact(new RadialImpactEffectCommand(
                context, projectile, radius,
                context.Stats.Damage * projectile.DamageMultiplier * 0.2f, force));
        }
    }
}
