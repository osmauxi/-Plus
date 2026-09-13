using System;

namespace ProjectGame.HotFix.Gameplay.Weapon.Effects.Special
{
    public sealed class StormCloudEffectSystem : WeaponSpecialEffectSystem
    {
        public const ushort Id = 2002;
        public override ushort EffectId => Id;
        public StormCloudEffectSystem(IWeaponSpecialEffectCommandSink commands) : base(commands) { }

        public override void OnEquipped(byte level, in WeaponEffectEquipContext context)
        {
            float radius = 8f + 2f * (level - 1);
            float damage = context.Stats.Damage * 1.2f;
            float interval = Math.Max(0.2f, 1f - 0.15f * (level - 1));
            context.Effects.TryGetLevel(OverloadEffectSystem.Id, out byte overloadLevel);
            Commands.SpawnStormCloud(new StormCloudEffectCommand(
                EffectId, level, context, radius, damage, interval, overloadLevel));
        }

        public override void OnUnequipped(ulong ownerEntityId) =>
            Commands.RemoveOwnerEffect(new OwnerEffectRemovalCommand(EffectId, ownerEntityId));
    }
}
