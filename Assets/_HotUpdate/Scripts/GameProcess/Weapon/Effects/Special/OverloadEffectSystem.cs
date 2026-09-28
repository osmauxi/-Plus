namespace ProjectGame.HotFix.Gameplay.Weapon.Effects.Special
{
    public sealed class OverloadEffectSystem : WeaponSpecialEffectSystem
    {
        public const ushort Id = 2001;
        public override ushort EffectId => Id;
        public OverloadEffectSystem(IWeaponSpecialEffectCommandSink commands) : base(commands) { }

        public override void OnProjectileHit(byte level, ref ProjectileState projectile,
            in ProjectileHitContext context)
        {
            byte jumpCount = checked((byte)(1 + level - 1));
            float damage = context.Stats.Damage * (0.5f + 0.1f * (level - 1));
            Commands.EmitLightning(new LightningEffectCommand(
                context, damage, jumpCount,
                branchCount: 3, searchRadius: 5f, applyInitialDamage: true));
        }
    }
}
