using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Weapon.Effects.Special
{
    public sealed class ExecutionerEffectSystem : WeaponSpecialEffectSystem
    {
        public const ushort Id = 2006;
        public override ushort EffectId => Id;

        public ExecutionerEffectSystem(IWeaponSpecialEffectCommandSink commands) : base(commands) { }

        public override void OnProjectileHit(byte level, ref ProjectileState projectile,
            in ProjectileHitContext context)
        {
            float threshold = Mathf.Clamp01(0.2f + 0.05f * (level - 1));
            Commands.TryExecute(new ExecuteEffectCommand(
                EffectId, level, projectile, context, threshold));
        }
    }
}
