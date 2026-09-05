namespace ProjectGame.HotFix.Gameplay.Weapon
{
    public struct WeaponStatSnapshot
    {
        public ushort Id;

        public float Damage;

        public float FireRate;
        public float ReloadTime;
        public ushort MagSize;

        public float CritChance;
        public float CritMultiplier;

        public float ProjectileSpeed;
        public ushort ProjectileCount;
        public float SpreadAngle;

        public byte BounceCount;
        public byte PierceCount;

        public float ProjectileSize;
        public float ProjectileLifeTime;
    }
}