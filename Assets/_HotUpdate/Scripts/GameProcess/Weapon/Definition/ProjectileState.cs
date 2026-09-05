using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Weapon
{
    [Flags]
    public enum ProjectileFlags : ushort
    {
        None = 0,
        Critical = 1 << 0,
    }

    public struct ProjectileState
    {
        public uint ProjectileId;
        public ulong ShotId;

        public Vector3 Position;
        public Vector3 Velocity;

        public float RemainingLifeTime;

        public float DamageMultiplier;
        public float SizeMultiplier;

        public byte PierceRemaining;
        public byte BounceRemaining;

        public byte Generation;
        public ushort HitCount;

        public ProjectileFlags Flags;
    }
}