namespace ProjectGame.HotFix.Gameplay.Weapon.Effects.Special
{
    public sealed class StaticShieldEffectSystem : WeaponSpecialEffectSystem
    {
        public const ushort Id = 2003;
        public override ushort EffectId => Id;
        public StaticShieldEffectSystem(IWeaponSpecialEffectCommandSink commands) : base(commands) { }

        public override void OnEquipped(byte level, in WeaponEffectEquipContext context)
        {
            float capacity = 15f + 5f * (level - 1);
            Commands.SetShieldCapacity(new ShieldCapacityEffectCommand(
                EffectId, level, context.OwnerEntityId, capacity));
        }

        public override void OnUnequipped(ulong ownerEntityId) =>
            Commands.SetShieldCapacity(new ShieldCapacityEffectCommand(EffectId, 0, ownerEntityId, 0f));

        public override void OnProjectileHit(byte level, ref ProjectileState projectile,
            in ProjectileHitContext context)
        {
            float amount = 1f + 0.5f * (level - 1);
            Commands.AddShield(new ShieldEffectCommand(EffectId, level, projectile, context, amount));
        }
    }
}
