using System;
using Unity.Netcode;

namespace ProjectGame.HotFix.Gameplay.Combat.Health
{
    /// <summary>NGO 的完整玩家生命快照。Generation=0 表示尚未注册，不伪造满血。</summary>
    public struct PlayerHealthWireState : INetworkSerializable, IEquatable<PlayerHealthWireState>
    {
        public uint Generation;
        public ulong Revision;
        public int ProfileId;
        public float Health, Shield, MaxHealth, MaxShield, Defense;
        public uint DamageGateTicks, LastDamageTick;
        public bool IsInvulnerable, HasTakenDamage;

        public PlayerHealthWireState(in HealthSnapshot state)
        {
            Generation = state.Entity.Generation;
            Revision = state.Revision;
            ProfileId = state.Definition.ProfileId;
            Health = state.CurrentHealth;
            Shield = state.CurrentShield;
            MaxHealth = state.Definition.MaxHealth;
            MaxShield = state.Definition.MaxShield;
            Defense = state.Definition.Defense;
            DamageGateTicks = state.Definition.DamageGateTicks;
            LastDamageTick = state.LastDamageTick;
            IsInvulnerable = state.IsInvulnerable;
            HasTakenDamage = state.HasTakenDamage;
        }

        public HealthSnapshot ToSnapshot(ulong networkObjectId) => new HealthSnapshot(
            new HealthEntity(networkObjectId, Generation),
            new HealthDefinition(ProfileId, MaxHealth, MaxShield, Defense, DamageGateTicks),
            Health, Shield, IsInvulnerable, LastDamageTick, HasTakenDamage, Revision);

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Generation);
            serializer.SerializeValue(ref Revision);
            serializer.SerializeValue(ref ProfileId);
            serializer.SerializeValue(ref Health);
            serializer.SerializeValue(ref Shield);
            serializer.SerializeValue(ref MaxHealth);
            serializer.SerializeValue(ref MaxShield);
            serializer.SerializeValue(ref Defense);
            serializer.SerializeValue(ref DamageGateTicks);
            serializer.SerializeValue(ref LastDamageTick);
            serializer.SerializeValue(ref IsInvulnerable);
            serializer.SerializeValue(ref HasTakenDamage);
        }

        public bool Equals(PlayerHealthWireState other) => Generation == other.Generation &&
            Revision == other.Revision && ProfileId == other.ProfileId && Health == other.Health &&
            Shield == other.Shield && MaxHealth == other.MaxHealth && MaxShield == other.MaxShield &&
            Defense == other.Defense && DamageGateTicks == other.DamageGateTicks &&
            LastDamageTick == other.LastDamageTick && IsInvulnerable == other.IsInvulnerable &&
            HasTakenDamage == other.HasTakenDamage;
    }
}
