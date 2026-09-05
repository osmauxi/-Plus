using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Weapon.Network
{
    public struct FireCommand : INetworkSerializable
    {
        public uint OwnerEntityId;

        public uint Tick;
        public uint ShotSequence;

        public Vector3 AimDirection;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref OwnerEntityId);
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref ShotSequence);
            serializer.SerializeValue(ref AimDirection);
        }
    }

    public static class WeaponMessageNames
    {
        public const string FireCommand = "PG.Weapon.FireCommand";
    }
}