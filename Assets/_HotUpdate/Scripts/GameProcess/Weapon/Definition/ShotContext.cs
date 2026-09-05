using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Weapon
{
    /// <summary>
    /// 代表一次服务器确认过的射击
    /// </summary>
    public struct ShotContext
    {
        public ulong ShotId;

        public uint FireTick;
        public uint OwnerEntityId;

        public ushort WeaponId;

        public uint RandomSeed;

        public Vector3 Origin;
        public Vector3 AimDirection;

        public ushort StatSnapshotId;
        public ushort EffectSetId;
    }
}