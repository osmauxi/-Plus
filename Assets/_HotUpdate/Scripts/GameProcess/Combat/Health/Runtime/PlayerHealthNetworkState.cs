using System;
using ProjectGame.HotFix.Gameplay.Weapon;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Combat.Health
{
    /// <summary>
    /// 随 PlayerRuntimeRoot Spawn/Despawn；所有客户端可读，仅服务器可写。
    /// NetworkVariable 自动覆盖初始 Spawn、晚加入和后续变化，无客户端写血 RPC。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(ProjectileHitTargetAdapter))]
    public sealed class PlayerHealthNetworkState : NetworkBehaviour, IProjectileHitTarget
    {
        private readonly NetworkVariable<PlayerHealthWireState> _state = new(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private PlayerHealthRuntimeService _service;
        private HealthSnapshot _snapshot;
        private bool _hasSnapshot;

        public bool TryGetHealth(out HealthSnapshot state)
        {
            state = _snapshot;
            return IsSpawned && _hasSnapshot;
        }
        public ulong EntityId => NetworkObjectId;
        public bool IsAlive => TryGetHealth(out var state) && state.IsAlive;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _hasSnapshot = false;
            _state.OnValueChanged += OnStateChanged;
            _service = PlayerHealthRuntimeService.For(NetworkManager);
            if (_service == null || !_service.IsInitialized)
                throw new InvalidOperationException("PlayerHealth 必须在玩家生成前完成服务初始化。");
            // 客户端初始 NetworkVariable 在 OnNetworkSpawn 之前已经接收。
            ReadSnapshot(_state.Value);
            _service.Register(this);
            GetComponent<ProjectileHitTargetAdapter>().Bind(this);
        }

        public void ApplyProjectileDamage(in ProjectileDamageContext context)
        {
            if (IsServer) _service?.TryApplyProjectileDamage(NetworkObjectId, context);
        }

        internal void Publish(in HealthSnapshot state)
        {
            if (!IsServer || !IsSpawned)
                throw new InvalidOperationException("只有已生成玩家的服务器端可以发布生命状态。");
            var wire = new PlayerHealthWireState(state);
            _state.Value = wire;
            ReadSnapshot(wire);
        }

        private void ReadSnapshot(PlayerHealthWireState state)
        {
            _hasSnapshot = state.Generation != 0;
            _snapshot = _hasSnapshot ? state.ToSnapshot(NetworkObjectId) : default;
        }

        private void OnStateChanged(PlayerHealthWireState previous, PlayerHealthWireState next)
        {
            ReadSnapshot(next);
            if (!IsServer && _hasSnapshot)
                _service?.NotifyReplica(new HealthChange(previous.Generation == next.Generation
                    ? HealthChangeKind.Updated : HealthChangeKind.Registered, _snapshot, _snapshot.Revision));
        }

        public override void OnNetworkDespawn()
        {
            var target = GetComponent<ProjectileHitTargetAdapter>();
            if (target.Target == this) target.Unbind();
            _state.OnValueChanged -= OnStateChanged;
            _service?.Unregister(this);
            _service = null;
            _hasSnapshot = false;
            _snapshot = default;
            base.OnNetworkDespawn();
        }
    }
}
