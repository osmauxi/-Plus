using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Weapon.Effects.Special
{
    public sealed class NuclearFissionEffectSystem : WeaponSpecialEffectSystem
    {
        public const ushort Id = 2009;
        public override ushort EffectId => Id;

        public NuclearFissionEffectSystem(IWeaponSpecialEffectCommandSink commands) : base(commands) { }

        public override void OnProjectileHit(byte level, ref ProjectileState projectile,
            in ProjectileHitContext context)
        {
            float ratio = 0.2f + 0.1f * (level - 1);
            float radius = 4f + level - 1;
            float slow = Mathf.Clamp(0.3f + 0.1f * (level - 1), 0f, 0.9f);
            Commands.UpsertDamageArea(new PersistentAreaEffectCommand(
                EffectId, level, projectile, context, radius, 5f, 1f,
                context.Stats.Damage * ratio, slow, true));
        }
    }
}
