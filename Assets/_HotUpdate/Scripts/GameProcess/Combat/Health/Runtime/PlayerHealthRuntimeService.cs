using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Gameplay.Network;
using ProjectGame.HotFix.Gameplay.Runtime;
using ProjectGame.HotFix.Gameplay.Weapon;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Combat.Health
{
    /// <summary>
    /// GameRoot 的玩家生命组合根：服务器使用 HealthRuntime，客户端读取玩家 NGO 镜像。
    /// 放在 WeaponRuntimeService 后初始化，使本 Tick 的战斗请求在 TickCompleted 末尾结算。
    /// 此服务不改变死亡后的移动、掉落、复活或友伤规则。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerHealthRuntimeService : MonoBehaviour, IGameRuntimeService,
        IHealthStateSource, IHealthCommandSink
    {
        [SerializeField, Min(1)] private float _maxHealth = 100;
        [SerializeField, Min(0)] private float _maxShield = 50;
        private static readonly Dictionary<NetworkManager, PlayerHealthRuntimeService> Services = new();
        private readonly Dictionary<ulong, PlayerHealthNetworkState> _players = new();
        private NetworkManager _network;
        private NetworkSimulationClock _clock;
        private HealthRuntime _authority;
        private HealthEntityBindings<Collider> _colliderBindings;
        private ulong _nextEventId;

        public static PlayerHealthRuntimeService Instance => For(NetworkManager.Singleton);
        public static PlayerHealthRuntimeService For(NetworkManager network) =>
            network != null && Services.TryGetValue(network, out var service) ? service : null;
        public bool IsInitialized { get; private set; }
        public bool IsAuthority => IsInitialized && _network != null && _network.IsServer;
        public HealthEntityBindings<Collider> ColliderBindings => _colliderBindings ??
            throw new InvalidOperationException("PlayerHealth ColliderBindings 尚未初始化。");
        public event Action<HealthChange> StateChanged;

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsInitialized) return UniTask.CompletedTask;
            var gameplay = GameNetworkRuntime.Gameplay;
            if (gameplay == null || !gameplay.IsInitialized)
                throw new InvalidOperationException("PlayerHealth 必须在 Gameplay 网络运行时后初始化。");
            Initialize(NetworkManager.Singleton, gameplay.Clock);
            return UniTask.CompletedTask;
        }

        /// <summary>显式 Peer/时钟装配入口，也供多 Peer 集成测试使用。</summary>
        public void Initialize(NetworkManager network, NetworkSimulationClock clock)
        {
            if (IsInitialized) throw new InvalidOperationException("PlayerHealth 已初始化。");
            if (network == null || !network.IsListening || clock == null || For(network) != null)
                throw new InvalidOperationException("PlayerHealth 需要唯一、已启动的网络 Peer 和时钟。");
            var definition = new HealthDefinition(0, _maxHealth, _maxShield);
            _network = network;
            _clock = clock;
            _nextEventId = 1;
            _colliderBindings = new HealthEntityBindings<Collider>(this);
            if (network.IsServer)
            {
                _authority = new HealthRuntime(new HealthCatalog(new[] { definition }));
                _authority.StateChanged += OnAuthoritativeChange;
                _clock.TickCompleted += OnTick;
            }
            Services.Add(network, this);
            IsInitialized = true;
        }

        internal void Register(PlayerHealthNetworkState player)
        {
            if (!IsInitialized) throw new InvalidOperationException("PlayerHealth 未初始化。");
            _players.Add(player.NetworkObjectId, player);
            HealthEntity entity = default;
            try
            {
                if (IsAuthority) entity = _authority.Register(player.NetworkObjectId, 0, initialShield: 0);
                else if (player.TryGetHealth(out var state))
                {
                    entity = state.Entity;
                    NotifyReplica(new HealthChange(HealthChangeKind.Registered, state, state.Revision));
                }
                if (!entity.IsValid)
                    throw new InvalidOperationException($"玩家 {player.NetworkObjectId} 缺少有效 Health 快照。");
                _colliderBindings.Bind(entity, player.GetComponentsInChildren<Collider>(true));
            }
            catch
            {
                if (IsAuthority && entity.IsValid && _authority.TryGetHealth(entity, out _))
                    _authority.Unregister(entity);
                _players.Remove(player.NetworkObjectId);
                throw;
            }
        }

        internal void Unregister(PlayerHealthNetworkState player)
        {
            if (!IsInitialized || !_players.TryGetValue(player.NetworkObjectId, out var current) || current != player)
                return;
            if (IsAuthority && _authority.TryGetEntity(player.NetworkObjectId, out var entity))
                _authority.Unregister(entity);
            else if (player.TryGetHealth(out var state))
                NotifyReplica(new HealthChange(HealthChangeKind.Removed, state, state.Revision));
            _players.Remove(player.NetworkObjectId);
        }

        public bool TryGetEntity(ulong entityId, out HealthEntity entity)
        {
            entity = default;
            if (!IsInitialized) return false;
            if (_authority != null) return _authority.TryGetEntity(entityId, out entity);
            if (!_players.TryGetValue(entityId, out var player) || player == null || !player.TryGetHealth(out var state))
                return false;
            entity = state.Entity;
            return true;
        }

        public bool TryGetHealth(HealthEntity entity, out HealthSnapshot state)
        {
            state = default;
            if (!IsInitialized) return false;
            if (_authority != null) return _authority.TryGetHealth(entity, out state);
            return _players.TryGetValue(entity.EntityId, out var player) && player != null &&
                player.TryGetHealth(out state) && state.Entity == entity;
        }

        /// <summary>只接受本机服务器战斗逻辑的命令；不能被客户端当作改血 RPC 使用。</summary>
        public bool TryEnqueue(in HealthCommand command) => IsAuthority && _authority.TryEnqueue(command);
        public bool TryDamage(ulong networkObjectId, float amount) => Queue(networkObjectId, amount, HealthCommandKind.Damage);
        public bool TryHeal(ulong networkObjectId, float amount) => Queue(networkObjectId, amount, HealthCommandKind.Heal);
        public bool TryAddShield(ulong networkObjectId, float amount) => Queue(networkObjectId, amount, HealthCommandKind.AddShield);
        public bool TryRevive(ulong networkObjectId, float health) => Queue(networkObjectId, health, HealthCommandKind.Revive);
        public bool TryApplyProjectileDamage(ulong targetId, in ProjectileDamageContext context)
        {
            if (!IsAuthority || !TryGetEntity(targetId, out var target) || !HealthMath.Positive(context.Damage))
                return false;
            HealthEntity source = default;
            TryGetEntity(context.SourceEntityId, out source);
            ulong eventId = _nextEventId++;
            if (_nextEventId == 0) _nextEventId = 1;
            var flags = context.IsCritical ? HealthDamageFlags.Critical : HealthDamageFlags.None;
            return _authority.TryEnqueue(HealthCommand.Damage(eventId, _clock.CurrentTick, source, target,
                context.Damage, HealthDamageType.Physical, flags,
                impact: new HealthImpact(context.HitPoint, context.HitDirection,
                    origin: DamageOriginType.Projectile)));
        }

        private bool Queue(ulong id, float amount, HealthCommandKind kind)
        {
            if (!IsAuthority || !HealthMath.Positive(amount) || !TryGetEntity(id, out var target)) return false;
            // 便捷入口使用环境 Source，调用方需避免在该 Source/Target 范围自行重复 EventId。
            ulong eventId = _nextEventId++;
            if (_nextEventId == 0) _nextEventId = 1;
            uint tick = _clock.CurrentTick;
            HealthCommand command = kind == HealthCommandKind.Damage ? HealthCommand.Damage(eventId, tick, default, target, amount) :
                kind == HealthCommandKind.Heal ? HealthCommand.Heal(eventId, tick, default, target, amount) :
                kind == HealthCommandKind.AddShield ? HealthCommand.AddShield(eventId, tick, default, target, amount) :
                HealthCommand.Revive(eventId, tick, default, target, amount);
            return _authority.TryEnqueue(command);
        }

        private void OnTick(uint tick) => _authority.Step(tick);
        private void OnAuthoritativeChange(HealthChange change)
        {
            if (change.Kind != HealthChangeKind.Removed && _players.TryGetValue(change.State.Entity.EntityId, out var player))
                player.Publish(change.State);
            StateChanged?.Invoke(change);
        }
        internal void NotifyReplica(HealthChange change) => StateChanged?.Invoke(change);

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            if (!IsInitialized) return UniTask.CompletedTask;
            if (_clock != null) _clock.TickCompleted -= OnTick;
            _colliderBindings?.Dispose();
            _colliderBindings = null;
            _authority?.Dispose();
            _authority = null;
            _players.Clear();
            if (_network != null) Services.Remove(_network);
            _network = null;
            _clock = null;
            StateChanged = null;
            IsInitialized = false;
            return UniTask.CompletedTask;
        }

        private void OnDestroy() => ShutdownAsync(CancellationToken.None).Forget();
    }
}
