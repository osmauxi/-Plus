namespace ProjectGame.HotFix.Gameplay.Weapon
{
    public sealed class EffectSet
    {
        public ushort Id { get; }
        public ulong Mask { get; }
        public EffectSnapshot[] Effects { get; }

        public EffectSet(ushort id, ulong mask, EffectSnapshot[] effects)
        {
            Id = id;
            Mask = mask;
            Effects = effects ?? System.Array.Empty<EffectSnapshot>();
        }
    }
}