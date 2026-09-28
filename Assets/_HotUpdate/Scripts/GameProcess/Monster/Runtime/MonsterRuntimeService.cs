using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Gameplay.Map;
using ProjectGame.HotFix.Gameplay.Network;
using ProjectGame.HotFix.Gameplay.Player;
using ProjectGame.HotFix.Gameplay.Runtime;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// Monster会话组合根，服务初始化时固化配置和网络协议，房间边界显式创建/销毁World。
    /// 怪物生命只存在于服务器 MonsterWorld，网络层只广播活跃状态和表现数据。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MonsterRuntimeService : MonoBehaviour,IGameRuntimeService,IMonsterNetworkReceiver
    {
        public const ulong MonsterEntityIdBase = 1UL << 63;
        /// <summary>
        /// 房间World/Replica等运行时容器预留容量
        /// </summary>
        [SerializeField, Min(1)] private int _initialCapacity = 128;
        /// <summary>
        /// 限制怪物presentation发送频率
        /// </summary>
        [SerializeField, Min(1)] private int _presentationIntervalTicks = 2;
        //网络依赖
        private GameplayNetworkRuntime _network;
        private NetworkManager _networkManager;
        //网络同步组件
        private MonsterNetworkTransport _transport;
        private MonsterPoseQuantizer _quantizer;
        //房间生命状态
        private bool _roomActive;

        public static MonsterRuntimeService Instance { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsRoomActive => IsInitialized && _roomActive;
        public MonsterCatalogBundle Catalogs { get; private set; }
        public MonsterRoomRuntime AuthorityRoom { get; private set; }
        public MonsterReplica Replica { get; private set; }
        public float RoomGroundY { get; private set; }
        public int PresentationIntervalTicks => _presentationIntervalTicks;
        public int AliveMonsterCount => AuthorityRoom?.World.AliveCount ?? 0;
        public int MonsterSlotCount => AuthorityRoom?.World.SlotCount ?? 0;

        //模块对外的事件
        public event Action<MonsterRoomBeginData> RoomBegan;
        public event Action RoomEnded;
        public event Action<MonsterSpawnBatch> SpawnBatchReceived;
        public event Action<MonsterPresentationFrame, MonsterFrameApplyResult> PresentationFrameApplied;
        public event Action AuthorityTickCompleted;

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized) 
                return UniTask.CompletedTask;
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
                    _network.Clock.TickCompleted += TickAuthority;
            }
            catch
            {
                _network.Clock.TickCompleted -= TickAuthority;
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

        /// <summary>
        /// Server开启房间并可靠同步量化边界与统一GroundY
        /// </summary>
        public void BeginRoom(Vector2 quantizationOrigin, Vector2 quantizationSize, float groundY,
            RoomSpatialData spatialData, Transform roomRoot,
            int expectedTotalMonsterCount = 0, uint? minimumAcceptedTick = null)
        {
            EnsureInitialized();
            if (!_networkManager.IsServer)
                throw new InvalidOperationException("只有 Server 可以开启 Monster 房间。");
            if (_roomActive) 
                throw new InvalidOperationException("Monster 房间已经开始，请先 EndRoom。");
            if (expectedTotalMonsterCount < 0) 
                throw new ArgumentOutOfRangeException(nameof(expectedTotalMonsterCount));
            uint roomTick = minimumAcceptedTick ?? _network.Clock.EstimatedServerTick;
            var data = new MonsterRoomBeginData(quantizationOrigin, quantizationSize, groundY, roomTick);
            try
            {
                BeginRoomLocal(data, expectedTotalMonsterCount, spatialData, roomRoot);
                _transport.SendRoomBegin(data);
            }
            catch
            {
                EndRoomLocal(false);
                throw;
            }
        }

        private void BeginRoomLocal(in MonsterRoomBeginData data, int expectedTotalMonsterCount,RoomSpatialData spatialData = null, Transform roomRoot = null)
        {
            if (_roomActive) 
                EndRoomLocal(true);
            _quantizer = new MonsterPoseQuantizer(data.QuantizationOrigin, data.QuantizationSize);
            RoomGroundY = data.GroundY;
            Replica.BeginRoom(data.MinimumAcceptedTick);
            if (_networkManager.IsServer)
            {
                AuthorityRoom = new MonsterRoomRuntime(
                    Catalogs.Runtime, Catalogs.Attacks, _quantizer, spatialData,roomRoot, _initialCapacity);
                AuthorityRoom.BeginRoom(expectedTotalMonsterCount, data.GroundY);
            }
            _roomActive = true;
            RoomBegan?.Invoke(data);
        }

        /// <summary>
        /// Server生成新的Slot，并把静态出生数据可靠发送给远端客户端
        /// </summary>
        public MonsterSpawnBatch Spawn(IReadOnlyList<MonsterSpawnPlan> plans)
        {
            EnsureAuthorityRoom();
            MonsterSpawnBatch batch = AuthorityRoom.Spawn(plans);
            if (batch.Count > 0)
            {
                // 权威端也建立 Replica/View：专用服务器需要表现预制件上的命中碰撞体。
                ApplySpawnBatchLocal(batch);
                _transport.SendSpawn(batch);
            }
            return batch;
        }

        public bool IsMonsterAlive(int slot) =>
            AuthorityRoom != null && AuthorityRoom.World.IsActive(slot);

        public bool DamageMonster(int slot, float damage) =>
            AuthorityRoom != null && AuthorityRoom.World.Damage(slot, damage);

        public bool TryGetMonsterPosition(int slot, out Vector3 position)
        {
            if (AuthorityRoom != null && AuthorityRoom.World.TryGetPosition(slot, out Vector2 flat))
            {
                position = new Vector3(flat.x, RoomGroundY, flat.y);
                return true;
            }
            position = default;
            return false;
        }

        public bool TryGetMonsterHealthRatio(int slot, out float ratio)
        {
            if (AuthorityRoom == null || !AuthorityRoom.World.IsActive(slot))
            {
                ratio = 0f;
                return false;
            }
            ushort configIndex = AuthorityRoom.World.Meta[slot].ConfigIndex;
            return AuthorityRoom.World.TryGetHealthRatio(
                slot, Catalogs.Runtime.Get(configIndex).MaxHealth, out ratio);
        }

        public bool SlowMonster(int slot, float speedMultiplier, float duration, uint currentTick)
        {
            uint durationTicks = (uint)Mathf.Max(1, Mathf.CeilToInt(duration / _network.Clock.TickDeltaTime));
            return AuthorityRoom != null && AuthorityRoom.World.ApplySlow(
                slot, speedMultiplier, currentTick, currentTick + durationTicks);
        }

        public bool StunMonster(int slot, float duration, uint currentTick)
        {
            uint durationTicks = (uint)Mathf.Max(1, Mathf.CeilToInt(duration / _network.Clock.TickDeltaTime));
            return AuthorityRoom != null && AuthorityRoom.World.ApplyStun(
                slot, currentTick, currentTick + durationTicks);
        }

        public bool PushMonster(int slot, Vector2 offset) =>
            AuthorityRoom != null && AuthorityRoom.World.Push(slot, offset);
        /// <summary>
        /// Monster主循环
        /// </summary>
        private void TickAuthority(uint tick)
        {
            if (!_roomActive || AuthorityRoom == null) 
                return;
            PlayerManager players = PlayerManager.Instance;
            if (players == null || !players.IsInitialized) 
                return;
            AuthorityRoom.Tick(players, tick, _network.Clock.TickDeltaTime);
            AuthorityTickCompleted?.Invoke();
            if (tick % (uint)_presentationIntervalTicks != 0) 
                return;
            MonsterPresentationFrame frame = AuthorityRoom.CaptureFrame(tick);
            ApplyPresentationFrameLocal(frame);
            _transport.SendPresentation(frame);
        }

        public void ReceiveRoomBegin(MonsterRoomBeginData data)
        {
            if (!IsInitialized || _networkManager.IsServer) 
                return;
            BeginRoomLocal(data, 0);
        }

        public void ReceiveSpawnBatch(MonsterSpawnBatch batch)
        {
            if (!_roomActive || _networkManager.IsServer || _quantizer == null) 
                return;
            ApplySpawnBatchLocal(batch);
        }

        public void ReceivePresentationFrame(MonsterPresentationFrame frame)
        {
            if (!_roomActive || _networkManager.IsServer || _quantizer == null) 
                return;
            ApplyPresentationFrameLocal(frame);
        }

        public void ReceiveRoomEnd()
        {
            if (!IsInitialized || _networkManager.IsServer) 
                return;
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

        public void EndRoom()
        {
            if (!IsInitialized || !_roomActive) return;
            if (_networkManager.IsServer)
                _transport.SendRoomEnd();
            EndRoomLocal(true);
        }

        private void EndRoomLocal(bool notify)
        {
            if (!_roomActive && AuthorityRoom == null && _quantizer == null) 
                return;
            DisposeAuthorityRoom();
            Replica.BeginRoom(0);
            _quantizer = null;
            RoomGroundY = 0f;
            _roomActive = false;
            if (notify)
               RoomEnded?.Invoke();
        }

        private void DisposeAuthorityRoom()
        {
            if (AuthorityRoom == null) return;
            AuthorityRoom.EndRoom();
            AuthorityRoom = null;
        }

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            if (!IsInitialized) return UniTask.CompletedTask;
            EndRoom();
            if (_network != null) _network.Clock.TickCompleted -= TickAuthority;
            _transport?.Shutdown();
            _transport = null;
            Replica = null;
            Catalogs = null;
            _network = null;
            _networkManager = null;
            SpawnBatchReceived = null;
            PresentationFrameApplied = null;
            AuthorityTickCompleted = null;
            RoomBegan = null;
            RoomEnded = null;
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
