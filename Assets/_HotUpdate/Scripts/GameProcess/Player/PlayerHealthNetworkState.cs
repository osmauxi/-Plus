using System;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using ProjectGame.HotFix.Gameplay.Player.State;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player
{
    /// <summary>
    /// 玩家自己持有生命状态。服务器直接改值，NetworkVariable 负责初始同步、晚加入与后续变化。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(PlayerSyncController))]
    public sealed class PlayerHealthNetworkState : NetworkBehaviour
    {
        [SerializeField, Min(1f)] private float _maxHealth = 100f;
        [SerializeField, Min(0f)] private float _maxShield = 50f;

        private readonly NetworkVariable<PlayerHealthState> _state = new(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private PlayerSyncController _sync;

        public bool IsAlive => _state.Value.CurrentHealth > 0f;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _sync = GetComponent<PlayerSyncController>();
            if (IsServer)
                _state.Value = new PlayerHealthState(_maxHealth, 0f, _maxHealth, _maxShield);
        }

        public bool TryGetHealth(out PlayerHealthState state)
        {
            state = _state.Value;
            return IsSpawned && state.MaxHealth > 0f;
        }

        public bool Damage(float amount)
        {
            if (!IsServer || !IsAlive)
                return false;

            PlayerHealthState next = _state.Value;
            float absorbed = Mathf.Min(next.CurrentShield, amount);
            next.CurrentShield -= absorbed;
            next.CurrentHealth = Mathf.Max(0f, next.CurrentHealth - (amount - absorbed));
            _state.Value = next;

            _sync.ApplyHit();
            if (next.CurrentHealth <= 0f)
                _sync.SetLifeState(PlayerLifeState.Dead);
            return true;
        }

        public void AddShield(float amount)
        {
            PlayerHealthState next = _state.Value;
            next.CurrentShield = Mathf.Min(next.MaxShield, next.CurrentShield + amount);
            _state.Value = next;
        }

        public void SetBonusShieldCapacity(float bonusCapacity)
        {
            PlayerHealthState next = _state.Value;
            next.MaxShield = _maxShield + bonusCapacity;
            next.CurrentShield = Mathf.Min(next.CurrentShield, next.MaxShield);
            _state.Value = next;
        }
    }

    public struct PlayerHealthState : INetworkSerializable, IEquatable<PlayerHealthState>
    {
        public float CurrentHealth;
        public float CurrentShield;
        public float MaxHealth;
        public float MaxShield;

        public PlayerHealthState(float currentHealth, float currentShield, float maxHealth, float maxShield)
        {
            CurrentHealth = currentHealth;
            CurrentShield = currentShield;
            MaxHealth = maxHealth;
            MaxShield = maxShield;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref CurrentHealth);
            serializer.SerializeValue(ref CurrentShield);
            serializer.SerializeValue(ref MaxHealth);
            serializer.SerializeValue(ref MaxShield);
        }

        public bool Equals(PlayerHealthState other) =>
            CurrentHealth == other.CurrentHealth && CurrentShield == other.CurrentShield &&
            MaxHealth == other.MaxHealth && MaxShield == other.MaxShield;
    }
}
