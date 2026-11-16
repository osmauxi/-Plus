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
using Unity.Profiling;
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
        private static readonly ProfilerMarker PublishMarker =
            new("Monster.Replication.Publish");
        private static readonly ProfilerMarker CaptureMarker =
            new("Monster.Replication.Capture");
        private static readonly ProfilerMarker ApplyLocalMarker =
            new("Monster.Replication.ApplyLocal");
        private static readonly ProfilerMarker PublishGroupsMarker =
            new("Monster.Replication.PublishGroups");
        private static readonly ProfilerMarker GroupScheduleMarker =
            new("Monster.Network.GroupSchedule");
        private static readonly ProfilerMarker ReceiveApplyMarker =
            new("Monster.Replication.ReceiveApply");
        private const int GroupPoseBudgetPerTick = 32 * 1024;
        private const int GroupSnapshotBatchMaxBytes = 1024;

        public const ulong MonsterEntityIdBase = 1UL << 63;
        /// <summary>
        /// 房间World/Replica等运行时容器预留容量
        /// </summary>
        [SerializeField, Min(1)] private int _initialCapacity = 128;
        /// <summary>
        /// Host 本地整房间表现采样间隔。
        /// </summary>
        [SerializeField, Min(1)] private int _presentationIntervalTicks = 2;
        [SerializeField, Min(0.01f)] private float _monsterHitRadius = 0.73f;
        //网络依赖
        private GameplayNetworkRuntime _network;
        private NetworkManager _networkManager;
        //网络同步组件
        private MonsterNetworkTransport _transport;
        private MonsterPoseQuantizer _quantizer;
        private MonsterPresentationFrame _authorityWriteFrame;
        private MonsterPresentationFrame _authorityPublishedFrame;
        private readonly List<MonsterNetworkGroupIndex.Group> _duePoseGroups = new();
        private readonly GroupSnapshotSendBatch _initialSnapshotBatch = new();
        private readonly GroupSnapshotSendBatch _periodicSnapshotBatch = new();

        private sealed class GroupSnapshotSendBatch
        {
            private readonly List<MonsterGroupSnapshot> _snapshotPool = new();
            internal readonly List<MonsterNetworkGroupIndex.Group> Groups = new();
            internal readonly List<MonsterGroupSnapshot> Frames = new();
            internal readonly List<byte> Intervals = new();
            internal int PayloadBytes { get; private set; }
            internal int Count => Frames.Count;

            internal bool CanFit(int length) =>
                GroupSnapshotBatchHeaderSize + PayloadBytes + length <= GroupSnapshotBatchMaxBytes;

            internal MonsterGroupSnapshot Add(
                MonsterNetworkGroupIndex.Group group, byte intervalTicks, int length)
            {
                int index = Frames.Count;
                while (_snapshotPool.Count <= index)
                    _snapshotPool.Add(new MonsterGroupSnapshot(MonsterNetworkGroupIndex.GroupCapacity));

                Groups.Add(group);
                Frames.Add(_snapshotPool[index]);
                Intervals.Add(intervalTicks);
                PayloadBytes += length;
                return _snapshotPool[index];
            }

            internal void Clear()
            {
                Groups.Clear();
                Frames.Clear();
                Intervals.Clear();
                PayloadBytes = 0;
            }
        }

        private const int GroupSnapshotBatchHeaderSize = 2;
        //房间生命状态
        private bool _roomActive;

        public static MonsterRuntimeService Instance { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsRoomActive => IsInitialized && _roomActive;
        public MonsterCatalogBundle Catalogs { get; private set; }
        public MonsterRoomRuntime AuthorityRoom { get; private set; }
        public MonsterReplica Replica { get; private set; }
        public float RoomGroundY { get; private set; }
        public int AliveMonsterCount => AuthorityRoom?.World.AliveCount ?? 0;
        public int MonsterSlotCount => AuthorityRoom?.World.SlotCount ?? 0;
        public float MonsterHitRadius => AuthorityRoom?.MonsterHitRadius ?? _monsterHitRadius;

        //模块对外的事件
        public event Action<MonsterRoomBeginData> RoomBegan;
        public event Action RoomEnded;
        public event Action<MonsterSpawnBatch> SpawnBatchReceived;
        public event Action<MonsterPresentationFrame, MonsterFrameApplyResult> PresentationFrameApplied;

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized) 
                return UniTask.CompletedTask;
            cancellationToken.ThrowIfCancellationRequested();
            if (Instance != null && Instance != this)
                throw new InvalidOperationException("重复 MonsterRuntimeService。");
            if (_initialCapacity < 1 || _presentationIntervalTicks < 1 ||
                _monsterHitRadius <= 0f)
                throw new InvalidOperationException(
                    "Monster Capacity、PresentationInterval 与 HitRadius 必须大于 0。");

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
                    _authorityWriteFrame = new MonsterPresentationFrame(_initialCapacity);
                    _authorityPublishedFrame = new MonsterPresentationFrame(_initialCapacity);
                    _network.Clock.Subscribe(GameplayTickPhase.WorldSimulation, TickAuthoritySimulation);
                    _network.Clock.Subscribe(GameplayTickPhase.MeleeCombat, ResolveAuthorityCombat);
                    _network.Clock.Subscribe(GameplayTickPhase.Replication, PublishAuthorityState);
                }
            }
            catch
            {
                _network.Clock.Unsubscribe(GameplayTickPhase.WorldSimulation, TickAuthoritySimulation);
                _network.Clock.Unsubscribe(GameplayTickPhase.MeleeCombat, ResolveAuthorityCombat);
                _network.Clock.Unsubscribe(GameplayTickPhase.Replication, PublishAuthorityState);
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
                    Catalogs.Runtime, Catalogs.Attacks, _quantizer, spatialData, roomRoot,
                    _monsterHitRadius, _initialCapacity);
                _authorityWriteFrame.EnsureCapacity(expectedTotalMonsterCount, expectedTotalMonsterCount);
                _authorityPublishedFrame.EnsureCapacity(expectedTotalMonsterCount, expectedTotalMonsterCount);
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
                int slotCount = AuthorityRoom.World.SlotCount;
                _authorityWriteFrame.EnsureCapacity(slotCount, slotCount);
                _authorityPublishedFrame.EnsureCapacity(slotCount, slotCount);
                // 权威端也建立 Replica/View，供 Host 表现；命中判定只读取 MonsterWorld。
                ApplySpawnBatchLocal(batch);
                _transport.SendSpawn(batch);
            }
            return batch;
        }

        public bool IsMonsterAlive(int slot) =>
            AuthorityRoom != null && AuthorityRoom.World.IsActive(slot);

        public bool DamageMonster(int slot, float damage)
        {
            if (AuthorityRoom == null || !AuthorityRoom.DamageMonster(slot, damage))
                return false;

            if (!AuthorityRoom.World.IsActive(slot))
                _transport.SendDespawn(slot);
            return true;
        }

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

        public int QueryMonsterCircleCandidates(
            Vector2 center,
            float radius,
            int[] results)
        {
            if (AuthorityRoom == null)
                return 0;

            return AuthorityRoom.QueryMonsterCircleCandidates(center, radius, results);
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
            AuthorityRoom != null && AuthorityRoom.PushMonster(slot, offset);
        /// <summary>
        /// Monster 权威模拟；在玩家完成本 Tick 后、弹丸碰撞前推进。
        /// </summary>
        private void TickAuthoritySimulation(uint tick)
        {
            if (!_roomActive || AuthorityRoom == null) 
                return;
            PlayerManager players = PlayerManager.Instance;
            if (players == null || !players.IsInitialized) 
                return;
            AuthorityRoom.Tick(players, tick, _network.Clock.TickDeltaTime);
        }

        private void ResolveAuthorityCombat(uint tick)
        {
            if (!_roomActive || AuthorityRoom == null)
                return;

            AuthorityRoom.ResolveCombat();
        }

        /// <summary>
        /// 战斗结算完成后推进 Host 本地表现和远端群组调度。
        /// </summary>
        private void PublishAuthorityState(uint tick)
        {
            if (!_roomActive)
                return;

            using var publishScope = PublishMarker.Auto();
            if (tick % (uint)_presentationIntervalTicks == 0)
            {
                using (CaptureMarker.Auto())
                    AuthorityRoom.CaptureFrame(tick, _authorityWriteFrame);
                MonsterPresentationFrame publishedFrame = _authorityWriteFrame;
                _authorityWriteFrame = _authorityPublishedFrame;
                _authorityPublishedFrame = publishedFrame;
                using (ApplyLocalMarker.Auto())
                    ApplyPresentationFrameLocal(_authorityPublishedFrame);
            }

            if (_network.HasRemoteClients)
                PublishRemoteGroups(tick, _network.RemoteClientIds);
        }

        /// <summary>
        /// 全队共享群组发布计划。过期组优先，同一到期 Tick 优先发内层。
        /// </summary>
        private void PublishRemoteGroups(uint tick, IReadOnlyList<ulong> clients)
        {
            using var publishGroupsScope = PublishGroupsMarker.Auto();
            IReadOnlyList<MonsterNetworkGroupIndex.Group> groups = AuthorityRoom.RefreshNetworkGroups(tick);
            using (GroupScheduleMarker.Auto())
            {
                _duePoseGroups.Clear();
                for (int i = 0; i < groups.Count; i++)
                {
                    MonsterNetworkGroupIndex.Group group = groups[i];
                    if (group.Count == 0)
                    {
                        if (group.PublishedMembershipRevision != 0 && group.PublishedMembershipRevision != group.MembershipRevision)
                        {
                            _transport.SendGroupMembership(clients, group);
                            group.PublishedMembershipRevision = group.MembershipRevision;
                        }

                        continue;
                    }

                    if (MonsterMath.HasReached(tick, group.NextPoseTick))
                        _duePoseGroups.Add(group);
                }

                _duePoseGroups.Sort(static (left, right) =>
                {
                    int dueOrder = unchecked((int)(left.NextPoseTick - right.NextPoseTick));
                    if (dueOrder != 0)
                        return dueOrder;
                    int intervalOrder = left.IntervalTicks.CompareTo(right.IntervalTicks);
                    return intervalOrder != 0 ? intervalOrder : left.Id.CompareTo(right.Id);
                });
            }

            int remainingBytes = GroupPoseBudgetPerTick;
            _initialSnapshotBatch.Clear();
            _periodicSnapshotBatch.Clear();
            foreach (MonsterNetworkGroupIndex.Group group in _duePoseGroups)
            {
                int length = MonsterNetworkCodec.GetGroupSnapshotByteCount(group.Count);
                if (length > remainingBytes)
                    break;

                bool membershipChanged = group.PublishedMembershipRevision != group.MembershipRevision;
                GroupSnapshotSendBatch batch = membershipChanged ? _initialSnapshotBatch : _periodicSnapshotBatch;
                if (batch.Count != 0 && !batch.CanFit(length))
                    FlushGroupSnapshotBatch(clients, batch, membershipChanged, tick);

                MonsterGroupSnapshot frame = batch.Add(group, group.IntervalTicks, length);
                AuthorityRoom.CaptureGroupFrame(group, tick, frame);
                if (membershipChanged)
                    _transport.SendGroupMembership(clients, group);

                remainingBytes -= length;
            }

            FlushGroupSnapshotBatch(clients, _initialSnapshotBatch, true, tick);
            FlushGroupSnapshotBatch(clients, _periodicSnapshotBatch, false, tick);
        }

        private void FlushGroupSnapshotBatch(
            IReadOnlyList<ulong> clients,
            GroupSnapshotSendBatch batch,
            bool isInitialSnapshot,
            uint tick)
        {
            if (batch.Count == 0)
                return;

            _transport.SendGroupSnapshotBatch(clients, batch.Frames, batch.Intervals, isInitialSnapshot);
            for (int i = 0; i < batch.Count; i++)
            {
                MonsterNetworkGroupIndex.Group group = batch.Groups[i];
                group.PublishedMembershipRevision = group.MembershipRevision;
                group.NextPoseTick = unchecked(tick + group.IntervalTicks);
            }

            batch.Clear();
        }
        public void ReceiveGroupMembership(ref FastBufferReader reader)
        {
            if (!_roomActive || _networkManager.IsServer)
                return;

            int count = MonsterNetworkCodec.ReadGroupMembershipHeader(ref reader, out uint groupId, out uint revision);
            Span<int> slots = Replica.BeginGroupMembershipWrite(groupId, count, out int offset);
            MonsterNetworkCodec.ReadGroupMembershipSlots(ref reader, slots);
            Replica.CommitGroupMembership(groupId, offset, count, revision);
        }

        public void ReceiveGroupSnapshot(MonsterGroupSnapshot frame, byte intervalTicks)
        {
            if (!_roomActive || _networkManager.IsServer)
                return;

            float duration = intervalTicks * _network.Clock.TickDeltaTime;
            Replica.ApplyGroupSnapshot(frame, _quantizer, Time.unscaledTime, duration);
        }

        public void ReceiveRoomBegin(MonsterRoomBeginData data)
        {
            if (!IsInitialized || _networkManager.IsServer) 
                return;
            BeginRoomLocal(data, 0);
        }

        public void ReceiveSpawnBatch(MonsterSpawnBatch batch)
        {
            if (!_roomActive || _networkManager.IsServer)
                return;
            ApplySpawnBatchLocal(batch);
        }

        public void ReceiveDespawn(int slot)
        {
            if (!_roomActive || _networkManager.IsServer)
                return;

            Replica.Despawn(slot);
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
            using var applyScope = ReceiveApplyMarker.Auto();
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
            _duePoseGroups.Clear();
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
            if (_network != null)
            {
                _network.Clock.Unsubscribe(GameplayTickPhase.WorldSimulation, TickAuthoritySimulation);
                _network.Clock.Unsubscribe(GameplayTickPhase.MeleeCombat, ResolveAuthorityCombat);
                _network.Clock.Unsubscribe(GameplayTickPhase.Replication, PublishAuthorityState);
            }
            _transport?.Shutdown();
            _transport = null;
            _authorityWriteFrame = null;
            _authorityPublishedFrame = null;
            _initialSnapshotBatch.Clear();
            _periodicSnapshotBatch.Clear();
            Replica = null;
            Catalogs = null;
            _network = null;
            _networkManager = null;
            SpawnBatchReceived = null;
            PresentationFrameApplied = null;
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
