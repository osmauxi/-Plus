using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Core.Events;
using ProjectGame.HotFix.Gameplay.Map.Generation;
using ProjectGame.HotFix.Gameplay.Map.View;
using ProjectGame.HotFix.Gameplay.Monsters;
using ProjectGame.HotFix.Gameplay.Network;
using ProjectGame.HotFix.Gameplay.Player;
using ProjectGame.HotFix.Gameplay.Runtime;
using ProjectGame.HotFix.Gameplay.Spawning;
using ProjectGame.HotFix.Gameplay.State;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map.Flow
{
    /// <summary>
    /// 服务端权威的房间玩法编排器。地图生成器只负责静态地图，怪物管理器只负责战斗；
    /// 本类负责穿门、集合、封门、战斗生命周期以及供小地图消费的同步状态。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoomFlowController : NetworkBehaviour, IGameRuntimeService
    {
        [Header("Dependencies")]
        [SerializeField] private MapGenerationController _mapGenerationController;
        [SerializeField] private MapVisualBuilder _mapVisualBuilder;
        [SerializeField] private PlayerSpawnController _playerSpawnController;
        [SerializeField] private MonsterBattleManager _battleManager;

        [Header("Gathering")]
        [SerializeField, Min(0.1f)] private float _gatherTimeoutSeconds = 10f;
        [SerializeField, Min(1)] private int _immediateWarpGraphDistance = 2;
        [SerializeField, Min(0.5f)] private float _gatherWarpRadius = 4f;
        [SerializeField, Min(0f)] private float _gatherWarpMinimumSpacing = 1.5f;
        [SerializeField, Min(1)] private int _gatherSpawnAttemptsPerPoint = 48;
        [SerializeField, Min(0f)] private float _playerGroundOffset = 0.1f;

        [Header("Encounter")]
        [SerializeField, Min(0)] private int _baseCombatBudget = 100;
        [SerializeField, Min(0)] private int _budgetPerLevel = 30;
        [SerializeField, Min(1f)] private float _bossBudgetMultiplier = 2f;
        [SerializeField, Min(1)] private int _baseMaxActiveMonsters = 12;
        [SerializeField, Min(0)] private int _extraActivePerAdditionalPlayer = 4;

        private readonly Dictionary<int, RoomGameplayState> _roomStates = new();
        private readonly Dictionary<ulong, int> _playerRooms = new();
        private readonly Dictionary<ulong, Vector3> _lastPlayerPositions = new();
        private readonly HashSet<int> _visitedRooms = new();
        private readonly List<RoomPortalRuntime> _portalBuffer = new();
        private readonly List<PlayerRuntime> _warpPlayers = new();
        private readonly List<Vector3> _warpPositions = new();
        private readonly List<int> _roomIdBuffer = new();
        private readonly Queue<int> _distanceQueue = new();
        private readonly Dictionary<int, int> _distanceByRoom = new();
        private readonly RoomGatheringSpawnSampler _gatheringSampler = new();
        private readonly RoomGroundPointSampler _fallbackSampler = new();

        private IDisposable _mapBuiltSubscription;
        private IDisposable _mapClearingSubscription;
        private NetworkManager _networkManager;
        private PlayerManager _playerManager;
        private NetworkSimulationClock _clock;
        private MapLayout _layout;
        private int _generationId;
        private int _stateRevision;
        private int _lastAppliedRevision;
        private int _activeEncounterRoomId = -1;
        private float _gatherStartedAt;
        private Vector3 _gatherAnchor;

        public bool IsInitialized { get; private set; }
        public int GenerationId => _generationId;
        public int ActiveEncounterRoomId => _activeEncounterRoomId;
        public IReadOnlyDictionary<int, RoomGameplayState> RoomStates => _roomStates;
        public IReadOnlyDictionary<ulong, int> PlayerRooms => _playerRooms;

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized) return;

            await UniTask.WaitUntil(() => IsSpawned, cancellationToken: cancellationToken);
            ResolveDependencies();

            _networkManager = NetworkManager;
            _playerManager = PlayerManager.Instance;

            if (_networkManager == null || !_networkManager.IsListening)
                throw new InvalidOperationException("RoomFlow 初始化时网络尚未启动。");
            if (_playerManager == null || !_playerManager.IsInitialized)
                throw new InvalidOperationException("RoomFlow 必须在 PlayerManager 后初始化。");
            if (GameNetworkRuntime.Gameplay == null || !GameNetworkRuntime.Gameplay.IsInitialized)
                throw new InvalidOperationException("RoomFlow 必须在 GameNetworkRuntime 后初始化。");

            ValidateSettings();
            _clock = GameNetworkRuntime.Gameplay.Clock;
            _mapBuiltSubscription = LocalEvents.Subscribe<MapRuntimeBuiltEvent>(HandleMapBuilt);
            _mapClearingSubscription = LocalEvents.Subscribe<MapRuntimeClearingEvent>(HandleMapClearing);

            if (_networkManager.IsServer)
            {
                _clock.TickCompleted += HandleTickCompleted;
                _playerManager.PlayerRegistered += HandlePlayerRegistered;
                _playerManager.PlayerUnregistered += HandlePlayerUnregistered;
                _battleManager.RoomBattleCleared += HandleBattleCleared;
                _battleManager.RoomBattleFailed += HandleBattleFailed;
            }

            IsInitialized = true;

            if (_mapGenerationController.CurrentLayout != null && _mapGenerationController.CurrentBuildPlan.IsValid)
                HandleMapBuilt(new MapRuntimeBuiltEvent(_mapGenerationController.CurrentGenerationId,
                    _mapGenerationController.CurrentLayout, _mapGenerationController.CurrentBuildPlan));
        }

        public bool TryGetRoomState(int roomId, out RoomGameplayState state) =>
            _roomStates.TryGetValue(roomId, out state);

        public bool TryGetPlayerRoom(ulong clientId, out int roomId) =>
            _playerRooms.TryGetValue(clientId, out roomId);

        public RoomFogState GetFogState(int roomId)
        {
            if (_visitedRooms.Contains(roomId))
                return RoomFogState.Visited;

            foreach (int visitedRoomId in _visitedRooms)
            {
                IReadOnlyList<int> neighbors = _layout?.GetNeighborRoomIds(visitedRoomId);
                if (neighbors == null) continue;
                for (int i = 0; i < neighbors.Count; i++)
                    if (neighbors[i] == roomId)
                        return RoomFogState.Revealed;
            }

            return RoomFogState.Hidden;
        }

        private void ResolveDependencies()
        {
            if (_mapGenerationController == null) _mapGenerationController = GetComponent<MapGenerationController>();
            if (_mapVisualBuilder == null) _mapVisualBuilder = GetComponent<MapVisualBuilder>();
            if (_playerSpawnController == null) _playerSpawnController = GetComponent<PlayerSpawnController>();
            if (_battleManager == null) _battleManager = GetComponent<MonsterBattleManager>();

            if (_mapGenerationController == null || !_mapGenerationController.IsInitialized)
                throw new InvalidOperationException("RoomFlow 必须在 MapGenerationController 后初始化。");
            if (_mapVisualBuilder == null || !_mapVisualBuilder.IsInitialized)
                throw new InvalidOperationException("RoomFlow 必须在 MapVisualBuilder 后初始化。");
            if (_playerSpawnController == null || !_playerSpawnController.IsInitialized)
                throw new InvalidOperationException("RoomFlow 必须在 PlayerSpawnController 后初始化。");
            if (_battleManager == null || !_battleManager.IsInitialized)
                throw new InvalidOperationException("RoomFlow 必须在 MonsterBattleManager 后初始化。");
        }

        private void HandleMapBuilt(MapRuntimeBuiltEvent mapEvent)
        {
            ResetMapState(false);
            _generationId = mapEvent.GenerationId;
            _layout = mapEvent.Layout;

            for (int i = 0; i < _layout.Rooms.Count; i++)
            {
                MapRoomDefinition room = _layout.Rooms[i];
                _roomStates[room.RoomId] = IsEncounterRoom(room.RoomType)
                    ? RoomGameplayState.Dormant
                    : RoomGameplayState.Cleared;
            }

            if (IsServer)
            {
                IReadOnlyList<PlayerRuntime> players = _playerManager.RuntimePlayers;
                for (int i = 0; i < players.Count; i++)
                    HandlePlayerRegistered(players[i]);
                BroadcastSnapshot();
            }
            else if (IsClient)
                RequestRoomFlowSnapshotServerRpc(_generationId);

            LocalEvents.Publish(new RoomFlowSnapshotAppliedEvent(_generationId));
        }

        private void HandleMapClearing(MapRuntimeClearingEvent mapEvent)
        {
            if (_layout == null || mapEvent.GenerationId != _generationId)
                return;
            ResetMapState(true);
        }

        private void HandlePlayerRegistered(PlayerRuntime player)
        {
            if (!IsServer || _layout == null || player == null || !player.IsSpawned)
                return;

            _lastPlayerPositions[player.ClientId] = player.transform.position;
            SetPlayerRoomServer(player.ClientId, _layout.StartRoomId, false);
        }

        private void HandlePlayerUnregistered(PlayerRuntime player)
        {
            if (!IsServer || player == null)
                return;

            ulong clientId = player.ClientId;
            int previousRoomId = _playerRooms.TryGetValue(clientId, out int roomId) ? roomId : -1;
            _playerRooms.Remove(clientId);
            _lastPlayerPositions.Remove(clientId);
            PublishPlayerRoomDelta(clientId, previousRoomId, -1);
            EvaluateGathering();
        }

        private void HandleTickCompleted(uint _)
        {
            if (!IsInitialized || !IsServer || _layout == null)
                return;

            IReadOnlyList<PlayerRuntime> players = _playerManager.RuntimePlayers;
            for (int i = 0; i < players.Count; i++)
            {
                PlayerRuntime player = players[i];
                if (!IsActivePlayer(player)) continue;

                ulong clientId = player.ClientId;
                Vector3 currentPosition = player.transform.position;
                if (!_playerRooms.TryGetValue(clientId, out int currentRoomId))
                {
                    _lastPlayerPositions[clientId] = currentPosition;
                    SetPlayerRoomServer(clientId, _layout.StartRoomId, false);
                    continue;
                }

                if (_lastPlayerPositions.TryGetValue(clientId, out Vector3 previousPosition) &&
                    TryFindPortalCrossing(currentRoomId, previousPosition, currentPosition, out int targetRoomId))
                    SetPlayerRoomServer(clientId, targetRoomId, true);

                _lastPlayerPositions[clientId] = player.transform.position;
            }

            EvaluateGathering();
        }

        private bool TryFindPortalCrossing(int currentRoomId, Vector3 start, Vector3 end, out int targetRoomId)
        {
            targetRoomId = -1;
            _mapVisualBuilder.CollectOutgoingPortals(currentRoomId, _portalBuffer);
            float bestT = float.MaxValue;

            for (int i = 0; i < _portalBuffer.Count; i++)
            {
                RoomPortalRuntime portal = _portalBuffer[i];
                Vector3 origin = portal.PortalTransform.position;
                float startDistance = Vector3.Dot(start - origin, portal.InwardNormal);
                float endDistance = Vector3.Dot(end - origin, portal.InwardNormal);
                if (startDistance > 0.05f || endDistance <= 0.05f)
                    continue;

                float denominator = endDistance - startDistance;
                if (denominator <= 0.0001f) continue;
                float t = -startDistance / denominator;
                if (t < 0f || t > 1f || t >= bestT) continue;

                Vector3 intersection = Vector3.LerpUnclamped(start, end, t);
                Vector3 offset = intersection - origin;
                if (Mathf.Abs(Vector3.Dot(offset, portal.PortalTransform.right)) > portal.HalfWidth + 0.15f ||
                    Mathf.Abs(Vector3.Dot(offset, portal.PortalTransform.up)) > portal.HalfHeight + 0.15f)
                    continue;

                bestT = t;
                targetRoomId = portal.TargetRoomId;
            }

            return targetRoomId >= 0;
        }

        private void SetPlayerRoomServer(ulong clientId, int roomId, bool allowEncounter)
        {
            if (!IsServer || _layout == null || !_roomStates.ContainsKey(roomId))
                return;

            int previousRoomId = _playerRooms.TryGetValue(clientId, out int previous) ? previous : -1;
            if (previousRoomId != roomId)
            {
                _playerRooms[clientId] = roomId;
                PublishPlayerRoomDelta(clientId, previousRoomId, roomId);
            }

            VisitRoomServer(roomId);
            if (!allowEncounter || !_layout.TryGetRoom(roomId, out MapRoomDefinition room) ||
                !IsEncounterRoom(room.RoomType) || _roomStates[roomId] == RoomGameplayState.Cleared)
                return;

            if (_activeEncounterRoomId < 0)
            {
                Vector3 anchor = _playerManager.TryGetRuntimePlayer(clientId, out PlayerRuntime trigger)
                    ? trigger.transform.position
                    : _mapVisualBuilder.TryGetRoom(roomId, out RoomViewRuntime runtime)
                        ? runtime.View.transform.position
                        : Vector3.zero;
                BeginGathering(roomId, anchor);
            }
            else if (_activeEncounterRoomId != roomId)
                PullMissingPlayers(false);
        }

        private void BeginGathering(int roomId, Vector3 anchor)
        {
            if (_activeEncounterRoomId >= 0 || _roomStates[roomId] != RoomGameplayState.Dormant)
                return;

            _activeEncounterRoomId = roomId;
            _gatherAnchor = anchor;
            _gatherStartedAt = Time.unscaledTime;
            SetRoomStateServer(roomId, RoomGameplayState.Gathering);
            PullMissingPlayers(false);
            EvaluateGathering();
        }

        private void EvaluateGathering()
        {
            if (_activeEncounterRoomId < 0 ||
                !_roomStates.TryGetValue(_activeEncounterRoomId, out RoomGameplayState state) ||
                state != RoomGameplayState.Gathering)
                return;

            PullMissingPlayers(false);
            if (AreAllActivePlayersInside(_activeEncounterRoomId))
            {
                StartEncounter(_activeEncounterRoomId);
                return;
            }

            if (Time.unscaledTime - _gatherStartedAt < _gatherTimeoutSeconds)
                return;

            PullMissingPlayers(true);
            if (AreAllActivePlayersInside(_activeEncounterRoomId))
                StartEncounter(_activeEncounterRoomId);
        }

        private void PullMissingPlayers(bool includeNearby)
        {
            if (_activeEncounterRoomId < 0 ||
                !_mapVisualBuilder.TryGetRoom(_activeEncounterRoomId, out RoomViewRuntime targetRoom))
                return;

            _warpPlayers.Clear();
            IReadOnlyList<PlayerRuntime> players = _playerManager.RuntimePlayers;
            for (int i = 0; i < players.Count; i++)
            {
                PlayerRuntime player = players[i];
                if (!IsActivePlayer(player)) continue;
                if (_playerRooms.TryGetValue(player.ClientId, out int roomId) && roomId == _activeEncounterRoomId)
                    continue;
                if (!includeNearby && GetGraphDistance(roomId, _activeEncounterRoomId) < _immediateWarpGraphDistance)
                    continue;
                _warpPlayers.Add(player);
            }

            if (_warpPlayers.Count == 0) return;

            int seed = unchecked((_layout.Seed * 486187739) ^ (_generationId * 16777619) ^
                                 (_activeEncounterRoomId * 31) ^ _stateRevision);
            var random = new System.Random(seed);
            RoomSpawnRegion region = targetRoom.View.SpawnRegion;
            bool sampled = _gatheringSampler.TrySampleMany(region, _gatherAnchor, _gatherWarpRadius,
                _warpPlayers.Count, _gatherWarpMinimumSpacing, random, _warpPositions,
                _gatherSpawnAttemptsPerPoint);
            if (!sampled)
                sampled = _fallbackSampler.TrySampleMany(region, _warpPlayers.Count, 0.25f,
                    _gatherWarpMinimumSpacing, random, _warpPositions, _gatherSpawnAttemptsPerPoint);
            if (!sampled)
            {
                Debug.LogError($"[{nameof(RoomFlowController)}] Room {_activeEncounterRoomId} 无法找到集合落点。");
                return;
            }

            for (int i = 0; i < _warpPlayers.Count; i++)
            {
                PlayerRuntime player = _warpPlayers[i];
                var pose = new SpawnPose(_warpPositions[i] + Vector3.up * _playerGroundOffset,
                    targetRoom.View.transform.rotation);
                _playerSpawnController.WarpPlayer(player, pose);
                _lastPlayerPositions[player.ClientId] = pose.Position;
                SetPlayerRoomServer(player.ClientId, _activeEncounterRoomId, false);
            }
        }

        private void StartEncounter(int roomId)
        {
            if (!_mapVisualBuilder.TryGetRoom(roomId, out RoomViewRuntime roomRuntime) ||
                !_layout.TryGetRoom(roomId, out MapRoomDefinition roomDefinition))
            {
                FailEncounter(roomId, "找不到战斗房间运行时数据。");
                return;
            }

            SetRoomStateServer(roomId, RoomGameplayState.Combat);
            _mapVisualBuilder.SetRoomConnectionsLocked(roomId, true);

            int level = GameStateController.Instance != null ? GameStateController.Instance.CurrentLevel : 0;
            int playerCount = CountActivePlayers();
            int budget = _baseCombatBudget + Mathf.Max(0, level) * _budgetPerLevel;
            if (roomDefinition.RoomType == RoomType.Boss)
                budget = Mathf.CeilToInt(budget * _bossBudgetMultiplier);
            int maxActive = _baseMaxActiveMonsters +
                            Mathf.Max(0, playerCount - 1) * _extraActivePerAdditionalPlayer;
            int seed = unchecked(_layout.Seed * 486187739 + roomId * 16777619 + _stateRevision);
            var request = new MonsterRoomBattleRequest(roomId, roomRuntime.View, budget,
                Mathf.Max(0, level), Mathf.Max(1, maxActive), seed);

            if (!_battleManager.TryStartRoomBattle(request) && _activeEncounterRoomId == roomId)
                FailEncounter(roomId, "MonsterBattleManager 拒绝启动房间战斗。");
        }

        private void HandleBattleCleared(int roomId)
        {
            if (!IsServer || roomId != _activeEncounterRoomId) return;
            _mapVisualBuilder.SetRoomConnectionsLocked(roomId, false);
            SetRoomStateServer(roomId, RoomGameplayState.Cleared);
            _activeEncounterRoomId = -1;

            if (_layout.TryGetRoom(roomId, out MapRoomDefinition room) && room.RoomType == RoomType.Boss)
                LocalEvents.Publish(new BossRoomClearedEvent(_generationId, roomId));
        }

        private void HandleBattleFailed(int roomId, string reason)
        {
            if (!IsServer || roomId != _activeEncounterRoomId) return;
            FailEncounter(roomId, reason);
        }

        private void FailEncounter(int roomId, string reason)
        {
            _mapVisualBuilder.SetRoomConnectionsLocked(roomId, false);
            if (_roomStates.ContainsKey(roomId))
                SetRoomStateServer(roomId, RoomGameplayState.Dormant);
            _activeEncounterRoomId = -1;
            LocalEvents.Publish(new RoomEncounterFailedEvent(_generationId, roomId, reason));
            Debug.LogError($"[{nameof(RoomFlowController)}] Room {roomId} 战斗失败：{reason}");
        }

        private bool AreAllActivePlayersInside(int roomId)
        {
            int activeCount = 0;
            IReadOnlyList<PlayerRuntime> players = _playerManager.RuntimePlayers;
            for (int i = 0; i < players.Count; i++)
            {
                PlayerRuntime player = players[i];
                if (!IsActivePlayer(player)) continue;
                activeCount++;
                if (!_playerRooms.TryGetValue(player.ClientId, out int playerRoomId) || playerRoomId != roomId)
                    return false;
            }
            return activeCount > 0;
        }

        private int CountActivePlayers()
        {
            int count = 0;
            IReadOnlyList<PlayerRuntime> players = _playerManager.RuntimePlayers;
            for (int i = 0; i < players.Count; i++)
                if (IsActivePlayer(players[i])) count++;
            return count;
        }

        private int GetGraphDistance(int startRoomId, int targetRoomId)
        {
            if (startRoomId == targetRoomId) return 0;
            if (_layout == null || !_roomStates.ContainsKey(startRoomId)) return int.MaxValue;

            _distanceQueue.Clear();
            _distanceByRoom.Clear();
            _distanceQueue.Enqueue(startRoomId);
            _distanceByRoom[startRoomId] = 0;

            while (_distanceQueue.Count > 0)
            {
                int roomId = _distanceQueue.Dequeue();
                int nextDistance = _distanceByRoom[roomId] + 1;
                IReadOnlyList<int> neighbors = _layout.GetNeighborRoomIds(roomId);
                for (int i = 0; i < neighbors.Count; i++)
                {
                    int neighbor = neighbors[i];
                    if (_distanceByRoom.ContainsKey(neighbor)) continue;
                    if (neighbor == targetRoomId) return nextDistance;
                    _distanceByRoom[neighbor] = nextDistance;
                    _distanceQueue.Enqueue(neighbor);
                }
            }

            return int.MaxValue;
        }

        private void VisitRoomServer(int roomId)
        {
            if (!_visitedRooms.Add(roomId)) return;
            int revision = NextRevision();
            PublishFogNeighborhood(roomId);
            ApplyVisitedClientRpc(_generationId, revision, roomId);
        }

        private void SetRoomStateServer(int roomId, RoomGameplayState state)
        {
            if (_roomStates.TryGetValue(roomId, out RoomGameplayState current) && current == state)
                return;
            _roomStates[roomId] = state;
            int revision = NextRevision();
            LocalEvents.Publish(new RoomStateChangedEvent(_generationId, roomId, state));
            ApplyRoomStateClientRpc(_generationId, revision, roomId, (byte)state);
        }

        private void PublishPlayerRoomDelta(ulong clientId, int previousRoomId, int roomId)
        {
            int revision = NextRevision();
            LocalEvents.Publish(new PlayerRoomChangedEvent(_generationId, clientId, previousRoomId, roomId));
            ApplyPlayerRoomClientRpc(_generationId, revision, clientId, previousRoomId, roomId);
        }

        private void PublishFogNeighborhood(int visitedRoomId)
        {
            LocalEvents.Publish(new RoomFogChangedEvent(_generationId, visitedRoomId, RoomFogState.Visited));
            IReadOnlyList<int> neighbors = _layout.GetNeighborRoomIds(visitedRoomId);
            for (int i = 0; i < neighbors.Count; i++)
            {
                int neighbor = neighbors[i];
                LocalEvents.Publish(new RoomFogChangedEvent(_generationId, neighbor, GetFogState(neighbor)));
            }
        }

        private int NextRevision() => ++_stateRevision;

        [ServerRpc(RequireOwnership = false)]
        private void RequestRoomFlowSnapshotServerRpc(int generationId, ServerRpcParams rpcParams = default)
        {
            if (!IsInitialized || !IsServer || generationId != _generationId || _layout == null)
                return;
            SendSnapshotToClient(rpcParams.Receive.SenderClientId);
        }

        private void BroadcastSnapshot()
        {
            if (!IsServer || _layout == null) return;
            ApplySnapshotClientRpc(_generationId, _stateRevision, BuildRoomIdArray(), BuildRoomStateArray(),
                BuildVisitedArray(), BuildPlayerIdArray(), BuildPlayerRoomArray());
        }

        private void SendSnapshotToClient(ulong clientId)
        {
            ClientRpcParams rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
            };
            ApplySnapshotClientRpc(_generationId, _stateRevision, BuildRoomIdArray(), BuildRoomStateArray(),
                BuildVisitedArray(), BuildPlayerIdArray(), BuildPlayerRoomArray(), rpcParams);
        }

        private int[] BuildRoomIdArray()
        {
            _roomIdBuffer.Clear();
            _roomIdBuffer.AddRange(_roomStates.Keys);
            _roomIdBuffer.Sort();
            return _roomIdBuffer.ToArray();
        }

        private byte[] BuildRoomStateArray()
        {
            byte[] states = new byte[_roomIdBuffer.Count];
            for (int i = 0; i < _roomIdBuffer.Count; i++)
                states[i] = (byte)_roomStates[_roomIdBuffer[i]];
            return states;
        }

        private int[] BuildVisitedArray()
        {
            int[] rooms = new int[_visitedRooms.Count];
            _visitedRooms.CopyTo(rooms);
            Array.Sort(rooms);
            return rooms;
        }

        private ulong[] BuildPlayerIdArray()
        {
            ulong[] clients = new ulong[_playerRooms.Count];
            _playerRooms.Keys.CopyTo(clients, 0);
            Array.Sort(clients);
            return clients;
        }

        private int[] BuildPlayerRoomArray()
        {
            ulong[] clients = BuildPlayerIdArray();
            int[] rooms = new int[clients.Length];
            for (int i = 0; i < clients.Length; i++) rooms[i] = _playerRooms[clients[i]];
            return rooms;
        }

        [ClientRpc]
        private void ApplySnapshotClientRpc(int generationId, int revision, int[] roomIds, byte[] states,
            int[] visitedRoomIds, ulong[] playerIds, int[] playerRoomIds, ClientRpcParams rpcParams = default)
        {
            if (IsServer || _layout == null || generationId != _generationId || revision < _lastAppliedRevision)
                return;
            if (roomIds.Length != states.Length || playerIds.Length != playerRoomIds.Length)
                return;

            _lastAppliedRevision = revision;
            _roomStates.Clear();
            _visitedRooms.Clear();
            _playerRooms.Clear();
            for (int i = 0; i < roomIds.Length; i++) _roomStates[roomIds[i]] = (RoomGameplayState)states[i];
            for (int i = 0; i < visitedRoomIds.Length; i++) _visitedRooms.Add(visitedRoomIds[i]);
            for (int i = 0; i < playerIds.Length; i++) _playerRooms[playerIds[i]] = playerRoomIds[i];
            RefreshActiveEncounterFromStates();
            LocalEvents.Publish(new RoomFlowSnapshotAppliedEvent(_generationId));
        }

        [ClientRpc]
        private void ApplyRoomStateClientRpc(int generationId, int revision, int roomId, byte state)
        {
            if (!AcceptDelta(generationId, revision)) return;
            RoomGameplayState gameplayState = (RoomGameplayState)state;
            _roomStates[roomId] = gameplayState;
            if (gameplayState == RoomGameplayState.Gathering || gameplayState == RoomGameplayState.Combat)
                _activeEncounterRoomId = roomId;
            else if (_activeEncounterRoomId == roomId)
                _activeEncounterRoomId = -1;
            LocalEvents.Publish(new RoomStateChangedEvent(generationId, roomId, gameplayState));
        }

        [ClientRpc]
        private void ApplyPlayerRoomClientRpc(int generationId, int revision, ulong clientId,
            int previousRoomId, int roomId)
        {
            if (!AcceptDelta(generationId, revision)) return;
            if (roomId >= 0) _playerRooms[clientId] = roomId;
            else _playerRooms.Remove(clientId);
            LocalEvents.Publish(new PlayerRoomChangedEvent(generationId, clientId, previousRoomId, roomId));
        }

        [ClientRpc]
        private void ApplyVisitedClientRpc(int generationId, int revision, int roomId)
        {
            if (!AcceptDelta(generationId, revision)) return;
            _visitedRooms.Add(roomId);
            PublishFogNeighborhood(roomId);
        }

        private bool AcceptDelta(int generationId, int revision)
        {
            if (IsServer || _layout == null || generationId != _generationId || revision <= _lastAppliedRevision)
                return false;
            _lastAppliedRevision = revision;
            return true;
        }

        private void RefreshActiveEncounterFromStates()
        {
            _activeEncounterRoomId = -1;
            foreach (KeyValuePair<int, RoomGameplayState> pair in _roomStates)
            {
                if (pair.Value != RoomGameplayState.Gathering && pair.Value != RoomGameplayState.Combat)
                    continue;
                _activeEncounterRoomId = pair.Key;
                break;
            }
        }

        private void ResetMapState(bool cancelBattle)
        {
            if (cancelBattle && IsServer && _battleManager != null && _battleManager.IsBattleActive)
                _battleManager.CancelRoomBattle();
            if (IsServer && _activeEncounterRoomId >= 0 && _mapVisualBuilder != null && _mapVisualBuilder.IsInitialized)
                _mapVisualBuilder.SetRoomConnectionsLocked(_activeEncounterRoomId, false);

            _roomStates.Clear();
            _playerRooms.Clear();
            _lastPlayerPositions.Clear();
            _visitedRooms.Clear();
            _portalBuffer.Clear();
            _warpPlayers.Clear();
            _warpPositions.Clear();
            _layout = null;
            _activeEncounterRoomId = -1;
            _stateRevision = 0;
            _lastAppliedRevision = 0;
        }

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            if (!IsInitialized) return UniTask.CompletedTask;

            _mapBuiltSubscription?.Dispose();
            _mapClearingSubscription?.Dispose();
            _mapBuiltSubscription = null;
            _mapClearingSubscription = null;

            if (_networkManager != null && _networkManager.IsServer)
            {
                if (_clock != null) _clock.TickCompleted -= HandleTickCompleted;
                if (_playerManager != null)
                {
                    _playerManager.PlayerRegistered -= HandlePlayerRegistered;
                    _playerManager.PlayerUnregistered -= HandlePlayerUnregistered;
                }
                if (_battleManager != null)
                {
                    _battleManager.RoomBattleCleared -= HandleBattleCleared;
                    _battleManager.RoomBattleFailed -= HandleBattleFailed;
                }
            }

            ResetMapState(true);
            _networkManager = null;
            _playerManager = null;
            _clock = null;
            IsInitialized = false;
            return UniTask.CompletedTask;
        }

        private void ValidateSettings()
        {
            if (_gatherTimeoutSeconds <= 0f || _immediateWarpGraphDistance < 1 ||
                _gatherWarpRadius < 0.5f || _gatherWarpMinimumSpacing < 0f ||
                _gatherSpawnAttemptsPerPoint < 1 || _playerGroundOffset < 0f ||
                _baseCombatBudget < 0 || _budgetPerLevel < 0 || _bossBudgetMultiplier < 1f ||
                _baseMaxActiveMonsters < 1 || _extraActivePerAdditionalPlayer < 0)
                throw new InvalidOperationException("RoomFlow 配置非法。");
        }

        private static bool IsEncounterRoom(RoomType roomType) =>
            roomType == RoomType.Combat || roomType == RoomType.Elite || roomType == RoomType.Boss;

        private static bool IsActivePlayer(PlayerRuntime player) =>
            player != null && player.isActiveAndEnabled && player.IsSpawned;

        private void OnDestroy()
        {
            if (IsInitialized) ShutdownAsync(CancellationToken.None).Forget();
        }
    }
}
