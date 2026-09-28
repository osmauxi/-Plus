using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Core.Events;
using ProjectGame.HotFix.Gameplay.Map.Flow;
using ProjectGame.HotFix.Gameplay.Map.Generation;
using ProjectGame.HotFix.Gameplay.Map.View;
using ProjectGame.HotFix.Gameplay.Monsters;
using ProjectGame.HotFix.Gameplay.Player;
using UnityEngine;

namespace ProjectGame.HotFix.UI.Gameplay.Minimap
{
    [DisallowMultipleComponent]
    public sealed class MinimapPresenter : MonoBehaviour
    {
        [SerializeField] private MinimapView _view;
        [SerializeField, Min(1f)] private float _overviewCellPixels = 44f;
        [SerializeField, Min(1f)] private float _overviewBaseNodePixels = 34f;
        [SerializeField, Range(0.1f, 1f)] private float _overviewFill = 0.82f;
        [SerializeField, Range(0.1f, 1f)] private float _tacticalFill = 0.9f;
        [SerializeField, Range(0.05f, 1f)] private float _revealedRoomAlpha = 0.32f;

        private readonly List<IDisposable> _subscriptions = new();
        private readonly Dictionary<int, Vector2> _overviewPositions = new();
        private readonly Dictionary<int, float> _overviewSides = new();

        private MapGenerationController _mapGeneration;
        private MapVisualBuilder _mapBuilder;
        private RoomFlowController _roomFlow;
        private MonsterRuntimeService _monsterRuntime;
        private MapLayout _layout;
        private MapBuildPlan _buildPlan;
        private MinimapDisplayMode _mode;
        private int _tacticalRoomId = -1;
        private RoomViewRuntime _tacticalRoom;
        private RoomMinimapDefinition _tacticalDefinition;

        private void OnEnable()
        {
            if (_view == null)
                _view = GetComponent<MinimapView>();

            _subscriptions.Add(LocalEvents.Subscribe<MapRuntimeBuiltEvent>(HandleMapBuilt));
            _subscriptions.Add(LocalEvents.Subscribe<MapRuntimeClearingEvent>(_ => Clear()));
            _subscriptions.Add(LocalEvents.Subscribe<RoomFlowSnapshotAppliedEvent>(_ => RefreshState()));
            _subscriptions.Add(LocalEvents.Subscribe<RoomStateChangedEvent>(_ => RefreshState()));
            _subscriptions.Add(LocalEvents.Subscribe<RoomFogChangedEvent>(_ => RefreshState()));
            _subscriptions.Add(LocalEvents.Subscribe<PlayerRoomChangedEvent>(_ => RefreshState()));

            ResolveRuntimeReferences();
            if (_mapGeneration != null && _mapGeneration.CurrentLayout != null)
                Build(_mapGeneration.CurrentLayout, _mapGeneration.CurrentBuildPlan);
        }

        private void OnDisable()
        {
            for (int i = 0; i < _subscriptions.Count; i++)
                _subscriptions[i].Dispose();
            _subscriptions.Clear();
            Clear();
        }

        private void LateUpdate()
        {
            if (_mode != MinimapDisplayMode.Tactical)
                return;

            UpdateTacticalPlayer();
            UpdateTacticalMonsters();
        }

        private void HandleMapBuilt(MapRuntimeBuiltEvent mapEvent)
        {
            ResolveRuntimeReferences();
            Build(mapEvent.Layout, mapEvent.BuildPlan);
        }

        private void Build(MapLayout layout, MapBuildPlan buildPlan)
        {
            Clear();
            _layout = layout;
            _buildPlan = buildPlan;

            float gridSpacing = FindGridSpacing();
            float baseFootprint = FindBaseFootprint();

            for (int i = 0; i < layout.Rooms.Count; i++)
            {
                MapRoomDefinition room = layout.Rooms[i];
                RoomViewRuntime runtime = _mapBuilder.Rooms[room.RoomId];
                RoomMinimapDefinition minimap = runtime.MinimapDefinition;
                Vector3 scale = runtime.View.transform.lossyScale;
                float footprint = Mathf.Max(minimap.LocalSize.x * Mathf.Abs(scale.x),
                    minimap.LocalSize.y * Mathf.Abs(scale.z));
                int sizeBucket = Mathf.Clamp(Mathf.RoundToInt(footprint / baseFootprint), 1, 3);
                float side = _overviewBaseNodePixels * sizeBucket;
                Vector2 position = new Vector2(
                    Mathf.Round(room.WorldPosition.x / gridSpacing),
                    Mathf.Round(room.WorldPosition.z / gridSpacing)) * _overviewCellPixels;

                _overviewPositions.Add(room.RoomId, position);
                _overviewSides.Add(room.RoomId, side);
                _view.CreateOverviewRoom(room.RoomId, room.RoomType, position, side);
            }

            if (buildPlan.ConnectionMode == ConnectionPresentationMode.ConnectionView)
            {
                for (int i = 0; i < layout.Connections.Count; i++)
                {
                    MapConnectionDefinition connection = layout.Connections[i];
                    if (_mapBuilder.TryGetConnection(connection.ConnectionId, out ConnectionViewRuntime runtime) &&
                        runtime.UsesConnectionView)
                    {
                        _view.CreateOverviewConnection(connection.ConnectionId,
                            _overviewPositions[connection.RoomAId], _overviewPositions[connection.RoomBId]);
                    }
                }
            }

            RefreshState();
        }

        private void RefreshState()
        {
            if (_layout == null)
                return;

            ResolveRuntimeReferences();
            int currentRoomId = GetLocalPlayerRoom();

            for (int i = 0; i < _layout.Rooms.Count; i++)
            {
                int roomId = _layout.Rooms[i].RoomId;
                _view.SetOverviewRoomState(roomId, _roomFlow.GetFogState(roomId),
                    roomId == currentRoomId, _revealedRoomAlpha);
            }

            if (_buildPlan.ConnectionMode == ConnectionPresentationMode.ConnectionView)
            {
                for (int i = 0; i < _layout.Connections.Count; i++)
                {
                    MapConnectionDefinition connection = _layout.Connections[i];
                    bool visible = _roomFlow.GetFogState(connection.RoomAId) != RoomFogState.Hidden &&
                                   _roomFlow.GetFogState(connection.RoomBId) != RoomFogState.Hidden;
                    _view.SetOverviewConnectionVisible(connection.ConnectionId, visible);
                }
            }

            FitOverview();
            RefreshMode(currentRoomId);
        }

        private void RefreshMode(int currentRoomId)
        {
            bool inCombat = currentRoomId >= 0 &&
                            _roomFlow.TryGetRoomState(currentRoomId, out RoomGameplayState state) &&
                            state == RoomGameplayState.Combat;

            if (!inCombat)
            {
                _mode = MinimapDisplayMode.Overview;
                _tacticalRoomId = -1;
                _tacticalRoom = null;
                _tacticalDefinition = null;
                _view.SetMode(_mode);
                return;
            }

            if (_tacticalRoomId != currentRoomId)
                EnterTactical(currentRoomId);
        }

        private void EnterTactical(int roomId)
        {
            _tacticalRoom = _mapBuilder.Rooms[roomId];
            _tacticalDefinition = _tacticalRoom.MinimapDefinition;
            if (_tacticalDefinition.LayoutSprite == null)
                throw new InvalidOperationException($"Room {roomId} 缺少烘焙小地图。 ");

            _tacticalRoomId = roomId;
            _mode = MinimapDisplayMode.Tactical;
            _view.ShowTacticalRoom(_tacticalDefinition.LayoutSprite, _tacticalDefinition.LocalSize, _tacticalFill);
            _view.SetMode(_mode);
        }

        private void FitOverview()
        {
            bool found = false;
            Vector2 minimum = new(float.MaxValue, float.MaxValue);
            Vector2 maximum = new(float.MinValue, float.MinValue);

            for (int i = 0; i < _layout.Rooms.Count; i++)
            {
                int roomId = _layout.Rooms[i].RoomId;
                if (_roomFlow.GetFogState(roomId) == RoomFogState.Hidden)
                    continue;

                float halfSide = _overviewSides[roomId] * 0.5f;
                Vector2 extents = new(halfSide, halfSide);
                minimum = Vector2.Min(minimum, _overviewPositions[roomId] - extents);
                maximum = Vector2.Max(maximum, _overviewPositions[roomId] + extents);
                found = true;
            }

            if (!found)
                return;

            Canvas.ForceUpdateCanvases();
            Vector2 boundsSize = maximum - minimum;
            Vector2 viewportSize = _view.Viewport.rect.size;
            float scale = Mathf.Min(viewportSize.x / boundsSize.x, viewportSize.y / boundsSize.y) * _overviewFill;
            Vector2 center = (minimum + maximum) * 0.5f;
            _view.SetOverviewTransform(-center * scale, scale);
        }

        private void UpdateTacticalPlayer()
        {
            PlayerRuntime player = PlayerManager.Instance.LocalPlayer;
            Vector2 position = WorldToTactical(player.transform.position);
            float localYaw = Mathf.DeltaAngle(_tacticalRoom.View.transform.eulerAngles.y,
                player.transform.eulerAngles.y);
            _view.SetTacticalPlayer(position, localYaw);
        }

        private void UpdateTacticalMonsters()
        {
            _view.HideAllEnemyMarkers();
            MonsterReplica replica = _monsterRuntime.Replica;
            bool boss = _tacticalRoom.RoomType == RoomType.Boss;
            for (int slot = 0; slot < replica.SlotCount; slot++)
            {
                replica.TryGetState(slot, 1f, out MonsterReplicaState monster);
                if (!monster.IsActive)
                    continue;

                Vector3 world = new(monster.Position.x, _monsterRuntime.RoomGroundY, monster.Position.y);
                _view.SetEnemyMarker(slot, WorldToTactical(world), boss);
            }
        }

        private Vector2 WorldToTactical(Vector3 worldPosition)
        {
            Vector3 local = _tacticalRoom.View.transform.InverseTransformPoint(worldPosition);
            Vector2 normalized = new(
                (local.x - _tacticalDefinition.LocalCenter.x) / _tacticalDefinition.LocalSize.x,
                (local.z - _tacticalDefinition.LocalCenter.y) / _tacticalDefinition.LocalSize.y);
            Vector2 roomSize = _view.TacticalRoomSize;
            return new Vector2(normalized.x * roomSize.x, normalized.y * roomSize.y);
        }

        private int GetLocalPlayerRoom()
        {
            PlayerRuntime player = PlayerManager.Instance != null ? PlayerManager.Instance.LocalPlayer : null;
            return player != null && _roomFlow.TryGetPlayerRoom(player.ClientId, out int roomId) ? roomId : -1;
        }

        private float FindGridSpacing()
        {
            float spacing = float.MaxValue;
            for (int i = 0; i < _layout.Connections.Count; i++)
            {
                MapConnectionDefinition connection = _layout.Connections[i];
                _layout.TryGetRoom(connection.RoomAId, out MapRoomDefinition roomA);
                _layout.TryGetRoom(connection.RoomBId, out MapRoomDefinition roomB);
                Vector3 delta = roomA.WorldPosition - roomB.WorldPosition;
                float axisDistance = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.z));
                if (axisDistance > 0.01f)
                    spacing = Mathf.Min(spacing, axisDistance);
            }

            return spacing;
        }

        private float FindBaseFootprint()
        {
            float footprint = float.MaxValue;
            foreach (RoomViewRuntime room in _mapBuilder.Rooms.Values)
            {
                Vector3 scale = room.View.transform.lossyScale;
                RoomMinimapDefinition minimap = room.MinimapDefinition;
                float roomFootprint = Mathf.Max(minimap.LocalSize.x * Mathf.Abs(scale.x),
                    minimap.LocalSize.y * Mathf.Abs(scale.z));
                footprint = Mathf.Min(footprint, roomFootprint);
            }

            return footprint;
        }

        private void ResolveRuntimeReferences()
        {
            if (_mapGeneration == null)
                _mapGeneration = FindObjectOfType<MapGenerationController>(true);
            if (_mapBuilder == null)
                _mapBuilder = FindObjectOfType<MapVisualBuilder>(true);
            if (_roomFlow == null)
                _roomFlow = FindObjectOfType<RoomFlowController>(true);
            if (_monsterRuntime == null)
                _monsterRuntime = FindObjectOfType<MonsterRuntimeService>(true);
        }

        private void Clear()
        {
            if (_view != null)
                _view.Clear();

            _layout = null;
            _buildPlan = default;
            _mode = MinimapDisplayMode.Overview;
            _tacticalRoomId = -1;
            _tacticalRoom = null;
            _tacticalDefinition = null;
            _overviewPositions.Clear();
            _overviewSides.Clear();
        }
    }
}
