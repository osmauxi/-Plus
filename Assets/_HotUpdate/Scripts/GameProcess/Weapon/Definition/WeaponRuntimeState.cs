using System;

namespace ProjectGame.HotFix.Gameplay.Weapon
{
    [Flags]
    public enum WeaponRuntimeFlags : byte
    {
        None = 0,
        Reloading = 1 << 0,
        Disabled = 1 << 1,
    }

    public struct WeaponRuntimeState
    {
        public ushort WeaponId;
        public ushort SnapshotVersion;

        public ushort CurrentAmmo;

        public uint ShotSequence;
        public uint NextFireTick;
        public uint ReloadEndTick;

        public ushort StatSnapshotId;
        public ushort EffectSetId;

        public WeaponRuntimeFlags Flags;

        public bool IsReloading => (Flags & WeaponRuntimeFlags.Reloading) != 0;
        public bool IsDisabled => (Flags & WeaponRuntimeFlags.Disabled) != 0;
    }
}