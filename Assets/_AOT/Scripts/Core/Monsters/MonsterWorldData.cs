using UnityEngine;

namespace ProjectGame.Core.Monsters
{
    public struct MonsterMetaData
    {
        public ushort ConfigIndex;
        public bool IsActive;
    }

    public struct MonsterMotionData
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Yaw;
    }
}
