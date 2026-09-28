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
            float threshold = GetThreshold(level);
            Commands.TryExecute(new ExecuteEffectCommand(context, threshold));
        }

        public static float GetThreshold(EffectSet effects) =>
            effects.TryGetLevel(Id, out byte level) ? GetThreshold(level) : 0f;

        private static float GetThreshold(byte level) =>
            Mathf.Clamp01(0.3f + 0.05f * (level - 1));
    }
}
