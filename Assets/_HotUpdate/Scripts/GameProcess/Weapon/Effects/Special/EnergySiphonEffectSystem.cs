namespace ProjectGame.HotFix.Gameplay.Weapon.Effects.Special
{
    public sealed class EnergySiphonEffectSystem : WeaponSpecialEffectSystem
    {
        public const ushort Id = 2005;
        public override ushort EffectId => Id;

        public EnergySiphonEffectSystem(IWeaponSpecialEffectCommandSink commands) : base(commands) { }

        public override void OnEquipped(byte level, in WeaponEffectEquipContext context)
        {
            Commands.SetShieldCapacity(new ShieldCapacityEffectCommand(
                EffectId, context.OwnerEntityId, context.Stats.Damage * 1.5f));
        }

        public override void OnUnequipped(ulong ownerEntityId) =>
            Commands.SetShieldCapacity(new ShieldCapacityEffectCommand(EffectId, ownerEntityId, 0f));

        public override void OnProjectileHit(byte level, ref ProjectileState projectile,
            in ProjectileHitContext context)
        {
            float conversionRate = 0.05f + 0.02f * (level - 1);
            Commands.AddShield(new ShieldEffectCommand(
                context, context.Stats.Damage * conversionRate));
        }
    }
}
