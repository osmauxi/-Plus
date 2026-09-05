namespace ProjectGame.HotFix.Gameplay.Weapon
{
    public readonly struct EffectSnapshot
    {
        public readonly ushort EffectId;
        public readonly byte Stack;

        public EffectSnapshot(ushort effectId, byte stack)
        {
            EffectId = effectId;
            Stack = stack;
        }
    }
}