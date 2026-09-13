using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Gameplay.Combat.Health;
using ProjectGame.HotFix.Gameplay.Network;
using ProjectGame.HotFix.Gameplay.Player;
using ProjectGame.HotFix.Gameplay.Runtime;
using ProjectGame.HotFix.Gameplay.Weapon;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// GameRoot 上的 Monster 会话组合根。服务初始化时固化配置和网络协议；房间边界显式创建/销毁 World。
    /// 服务顺序必须位于 WeaponRuntimeService 与 PlayerHealthRuntimeService 之前。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MonsterRuntimeService : MonoBehaviour, IGameRuntimeService, IMonsterNetworkReceiver
    {
        [SerializeField] private WeaponRuntimeService _weaponRuntimeService;
        [SerializeField, Min(1)] private int _initialCapacity = 128;
        [SerializeField, Min(1)] private int _presentationIntervalTicks = 2;

        private GameplayNetworkRuntime _network;
        private NetworkManager _networkManager;
        private HealthRuntime _monsterHealth;
        private HealthEntityBindings<Collider> _monsterColliderBindings;
        private MonsterNetworkTransport _transport;
        private MonsterPoseQuantizer _quantizer;
        private bool _roomActive;

        public static MonsterRuntimeService Instance { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsRoomActive => IsInitialized && _roomActive;
        public MonsterCatalogBundle Catalogs { get; private set; }
        public MonsterRoomRuntime AuthorityRoom { get; private set; }
        public MonsterReplica Replica { get; private set; }
        public float RoomGroundY { get; private set; }
        public int AliveMonsterCount => AuthorityRoom?.World.AliveCount ?? 0;

        public event Action<MonsterRoomBeginData> RoomBegan;
        public event Action RoomEnded;
        public event Action<MonsterSpawnBatch> SpawnBatchReceived;
        public event Action<MonsterPresentationFrame, MonsterFrameApplyResult> PresentationFrameApplied;
        public event Action<int, HealthResult> MonsterDied;
        public event Action<MonsterDeathReaction> DeathReactionRequested;

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized) return UniTask.CompletedTask;
            cancellationToken.ThrowIfCancellationRequested();
            if (Instance != null && Instance != this)
                throw new InvalidOperationException("重复 MonsterRuntimeService。");
            if (_initialCapacity < 1 || _presentationIntervalTicks < 1)
                throw new InvalidOperationException("Monster Capacity 与 PresentationInterval 必须大于 0。");

            _network = GameNetworkRuntime.Gameplay;
            _networkManager = NetworkManager.Singleton;
            if (_network == null || !_network.IsInitialized || _networkManager == null || !_networkManager.IsListening)
                throw new InvalidOperationException("Monster 必须在 Gameplay 网络运行时后初始化。");

            Catalogs = MonsterConfigAssembler.FromConfigManager(ConfigManager.Instance, _network.Clock.TickRate);
            _transport = new MonsterNetworkTransport(_network.Transport);
            try
            {
                _transport.Initialize(this);
                Replica = new MonsterReplica();
                if (_networkManager.IsServer)
                {
                    _monsterHealth = new HealthRuntime(Catalogs.Health);
                    _monsterColliderBindings = new HealthEntityBindings<Collider>(_monsterHealth);
                    WireProjectileResolver();
                    _network.Clock.TickCompleted += TickAuthority;
                }
            }
            catch
            {
                _network.Clock.TickCompleted -= TickAuthority;
                _monsterColliderBindings?.Dispose();
                _monsterColliderBindings = null;
                _monsterHealth?.Dispose();
                _monsterHealth = null;
                _transport.Shutdown();
                _transport = null;
                Replica = null;
                Catalogs = null;
                _network = null;
                _networkManager = null;
                throw;
            }

            Instance = this;
            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        private void WireProjectileResolver()
        {
            if (_weaponRuntimeService == null) _weaponRuntimeService = GetComponent<WeaponRuntimeService>();
            if (_weaponRuntimeService == null)
                throw new InvalidOperationException("MonsterRuntimeService 缺少 WeaponRuntimeService 引用。");
            if (_weaponRuntimeService.IsInitialized)
                throw new InvalidOperationException("MonsterRuntimeService 必须在 WeaponRuntimeService 之前初始化。");
            var monsterResolver = new MonsterProjectileHealthResolver(
                _monsterColliderBindings, _monsterHealth, () => _network.Clock.CurrentTick);
            _weaponRuntimeService.SetProjectileTargetResolver(new CompositeProjectileTargetResolver(
                monsterResolver, ComponentProjectileTargetResolver.Instance));
        }

        /// <summary>兼容零高度平面房间；新房间入口应显式传入 GroundY。</summary>
        public void BeginRoom(Vector2 quantizationOrigin, Vector2 quantizationSize,
            int expectedTotalMonsterCount = 0, uint? minimumAcceptedTick = null)
            => BeginRoom(quantizationOrigin, quantizationSize, 0f,
                expectedTotalMonsterCount, minimumAcceptedTick);

        /// <summary>Server 开启房间并可靠同步量化边界与统一 GroundY。</summary>
        public void BeginRoom(Vector2 quantizationOrigin, Vector2 quantizationSize, float groundY,
            int expectedTotalMonsterCount = 0, uint? minimumAcceptedTick = null)
        {
            EnsureInitialized();
            if (!_networkManager.IsServer)
                throw new InvalidOperationException("只有 Server 可以开启 Monster 房间。");
            if (_roomActive) throw new InvalidOperationException("Monster 房间已经开始，请先 EndRoom。");
            if (expectedTotalMonsterCount < 0) throw new ArgumentOutOfRangeException(nameof(expectedTotalMonsterCount));
            uint roomTick = minimumAcceptedTick ?? _network.Clock.EstimatedServerTick;
            var data = new MonsterRoomBeginData(quantizationOrigin, quantizationSize, groundY, roomTick);
            try
            {
                BeginRoomLocal(data, expectedTotalMonsterCount);
                _transport.SendRoomBegin(_networkManager.ConnectedClientsIds, data);
            }
            catch
            {
                EndRoomLocal(false);
                throw;
            }
        }

        private void BeginRoomLocal(in MonsterRoomBeginData data, int expectedTotalMonsterCount)
        {
            if (_roomActive) EndRoomLocal(true);
            _quantizer = new MonsterPoseQuantizer(data.QuantizationOrigin, data.QuantizationSize);
            RoomGroundY = data.GroundY;
            Replica.BeginRoom(data.MinimumAcceptedTick);
            if (_networkManager.IsServer)
            {
                PlayerHealthRuntimeService playerHealth = PlayerHealthRuntimeService.For(_networkManager);
                if (playerHealth == null || !playerHealth.IsAuthority)
                    throw new InvalidOperationException("Monster BeginRoom 前 PlayerHealth 权威服务尚未初始化。");
                AuthorityRoom = MonsterRoomRuntime.CreateWithAttackResolverFactory(
                    Catalogs.Runtime, Catalogs.Attacks, _quantizer, _monsterHealth,
                    binding => new MonsterMeleePhysicsResolver(playerHealth.ColliderBindings,
                        playerHealth, playerHealth, binding),
                    _initialCapacity, stepHealthInternally: false);
                AuthorityRoom.HealthBinding.MonsterDied += OnMonsterDied;
                AuthorityRoom.HealthBinding.DeathReactionRequested += OnDeathReactionRequested;
                AuthorityRoom.BeginRoom(expectedTotalMonsterCount, data.GroundY);
            }
            _roomActive = true;
            RoomBegan?.Invoke(data);
        }

        /// <summary>Server 原子追加连续 Slot，并把静态出生数据可靠发送给远端客户端。</summary>
        public MonsterSpawnBatch Spawn(IReadOnlyList<MonsterSpawnPlan> plans)
        {
            EnsureAuthorityRoom();
            MonsterSpawnBatch batch = AuthorityRoom.Spawn(plans);
            if (batch.Count > 0)
            {
                if (_networkManager.IsClient) ApplySpawnBatchLocal(batch);
                _transport.SendSpawn(_networkManager.ConnectedClientsIds, batch);
            }
            return batch;
        }

        /// <summary>View 出生或换碰撞体时调用；热路径命中只查集中映射。</summary>
        public void BindViewColliders(int slot, IEnumerable<Collider> colliders)
        {
            EnsureAuthorityRoom();
            if (!AuthorityRoom.HealthBinding.TryGetHealth(slot, out HealthEntity entity))
                throw new ArgumentOutOfRangeException(nameof(slot), $"Monster Slot={slot} 没有 Health 绑定。");
            _monsterColliderBindings.Bind(entity, colliders);
        }

        public bool UnbindViewColliders(int slot)
        {
            EnsureAuthorityRoom();
            return AuthorityRoom.HealthBinding.TryGetHealth(slot, out HealthEntity entity) &&
                _monsterColliderBindings.Unbind(entity);
        }

        private void TickAuthority(uint tick)
        {
            if (!_roomActive || AuthorityRoom == null) return;
            // Weapon 在本回调之后模拟，因此上一 Tick 的 Projectile 命令在这里统一提交；最多延迟一个 Tick。
            _monsterHealth.Step(tick);
            PlayerManager players = PlayerManager.Instance;
            if (players == null || !players.IsInitialized) return;
            AuthorityRoom.Tick(players, tick, _network.Clock.TickDeltaTime);
            if (tick % (uint)_presentationIntervalTicks != 0) return;
            MonsterPresentationFrame frame = AuthorityRoom.CaptureFrame(tick);
            if (_networkManager.IsClient) ApplyPresentationFrameLocal(frame);
            _transport.SendPresentation(_networkManager.ConnectedClientsIds, frame);
        }

        public void ReceiveRoomBegin(MonsterRoomBeginData data)
        {
            if (!IsInitialized || _networkManager.IsServer) return;
            BeginRoomLocal(data, 0);
        }

        public void ReceiveSpawnBatch(MonsterSpawnBatch batch)
        {
            if (!_roomActive || _networkManager.IsServer || _quantizer == null) return;
            ApplySpawnBatchLocal(batch);
        }

        public void ReceivePresentationFrame(MonsterPresentationFrame frame)
        {
            if (!_roomActive || _networkManager.IsServer || _quantizer == null) return;
            ApplyPresentationFrameLocal(frame);
        }

        public void ReceiveRoomEnd()
        {
            if (!IsInitialized || _networkManager.IsServer) return;
            EndRoomLocal(true);
        }

        private void ApplySpawnBatchLocal(MonsterSpawnBatch batch)
        {
            Replica.ApplySpawnBatch(batch, _quantizer);
            SpawnBatchReceived?.Invoke(batch);
        }

        private void ApplyPresentationFrameLocal(MonsterPresentationFrame frame)
        {
            MonsterFrameApplyResult result = Replica.Apply(frame, _quantizer);
            PresentationFrameApplied?.Invoke(frame, result);
        }

        private void OnMonsterDied(int slot, HealthResult result)
        {
            _monsterColliderBindings.Unbind(result.After.Entity);
            MonsterDied?.Invoke(slot, result);
        }

        private void OnDeathReactionRequested(MonsterDeathReaction reaction) =>
            DeathReactionRequested?.Invoke(reaction);

        public void EndRoom()
        {
            if (!IsInitialized || !_roomActive) return;
            if (_networkManager.IsServer)
                _transport.SendRoomEnd(_networkManager.ConnectedClientsIds);
            EndRoomLocal(true);
        }

        private void EndRoomLocal(bool notify)
        {
            if (!_roomActive && AuthorityRoom == null && _quantizer == null) return;
            DisposeAuthorityRoom();
            Replica.BeginRoom(0);
            _quantizer = null;
            RoomGroundY = 0f;
            _roomActive = false;
            if (notify) RoomEnded?.Invoke();
        }

        private void DisposeAuthorityRoom()
        {
            if (AuthorityRoom == null) return;
            AuthorityRoom.HealthBinding.MonsterDied -= OnMonsterDied;
            AuthorityRoom.HealthBinding.DeathReactionRequested -= OnDeathReactionRequested;
            AuthorityRoom.Dispose();
            AuthorityRoom = null;
        }

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            if (!IsInitialized) return UniTask.CompletedTask;
            EndRoom();
            if (_network != null) _network.Clock.TickCompleted -= TickAuthority;
            _transport?.Shutdown();
            _transport = null;
            _monsterColliderBindings?.Dispose();
            _monsterColliderBindings = null;
            _monsterHealth?.Dispose();
            _monsterHealth = null;
            Replica = null;
            Catalogs = null;
            _network = null;
            _networkManager = null;
            SpawnBatchReceived = null;
            PresentationFrameApplied = null;
            RoomBegan = null;
            RoomEnded = null;
            MonsterDied = null;
            DeathReactionRequested = null;
            if (Instance == this) Instance = null;
            IsInitialized = false;
            return UniTask.CompletedTask;
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized) throw new InvalidOperationException("MonsterRuntimeService 尚未初始化。");
        }

        private void EnsureAuthorityRoom()
        {
            EnsureInitialized();
            if (!_networkManager.IsServer || !_roomActive || AuthorityRoom == null)
                throw new InvalidOperationException("Monster 权威房间尚未开始。");
        }

        private void OnDestroy() => ShutdownAsync(CancellationToken.None).Forget();
    }
}
