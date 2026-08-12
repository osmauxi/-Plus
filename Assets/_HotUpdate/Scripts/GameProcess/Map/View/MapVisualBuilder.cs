using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Gameplay.Map.Generation;
using ProjectGame.HotFix.Gameplay.Runtime;
using UnityEngine;
using ProjectGame.HotFix.Gameplay.Pooling;

namespace ProjectGame.HotFix.Gameplay.Map.View
{
    /// <summary>
    /// 地图本地视觉的统一管理器。
    ///
    /// RoomView、RoomConnectorSlot 和可选的 ConnectionView 只提供静态资源数据；
    /// 所有初始化、墙体切换、连接登记和战斗门操作都集中在这里。
    /// </summary>
    public sealed class MapVisualBuilder : MonoBehaviour, IGameRuntimeService
    {
        [Header("物体实例堆放")]
        [SerializeField] private Transform _roomRoot;
        [SerializeField] private Transform _connectionRoot;

        private readonly Dictionary<int, RoomViewRuntime> _roomsById = new();
        private readonly Dictionary<int, ConnectionViewRuntime> _connectionsById = new();
        private readonly Dictionary<int, List<int>> _connectionIdsByRoomId = new();

        [SerializeField] private RoomTemplateCatalog _templateCatalog;

        private readonly Dictionary<int, GameObject> _roomObjectsById = new Dictionary<int, GameObject>();
        private readonly Dictionary<int, GameObject> _connectionObjectsById = new Dictionary<int, GameObject>();

        /// <summary>
        /// 当前由地图系统持有的 Pool。
        ///
        /// 目前采用“每层地图 Old-New 释放”的简单策略。
        /// 后续做阶段式地图时，可以改成整个 Stage 共用一份 Pool Set，
        /// 不需要修改房间和连接的 Rent / Return 流程。
        /// </summary>
        private readonly HashSet<string> _heldPoolIds = new(StringComparer.Ordinal);

        [Header("Grid Connection")]
        [Tooltip("GridGraph 使用的固定短通道对象池 ID。")]
        [SerializeField] private string _gridConnectionPoolId = "Connection_Grid";

        [Tooltip("Connector.forward 与连接方向的最小点积，用于检查锚点朝向是否正确。")]
        [SerializeField, Range(0f, 1f)] private float _minimumAnchorFacingDot = 0.8f;

        [Tooltip("网格连接两端允许的最大高度差。")]
        [SerializeField, Min(0f)] private float _maximumAnchorHeightDifference = 0.2f;

        [Tooltip("Seamless 模式下，两端 Connector 世界坐标允许的最大误差（按房间 Scale 同比缩放）。")]
        [SerializeField, Min(0f)] private float _seamPositionTolerance = 0.02f;

        public bool IsInitialized { get; private set; }

        public Transform RoomRoot => _roomRoot != null ? _roomRoot : transform;
        public Transform ConnectionRoot => _connectionRoot != null ? _connectionRoot : transform;

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return UniTask.CompletedTask;

            cancellationToken.ThrowIfCancellationRequested();

            if (Pooling.LocalObjectPool.Instance == null || !Pooling.LocalObjectPool.Instance.IsInitialized)
                throw new InvalidOperationException("LocalObjectPool 尚未初始化。");

            if (_templateCatalog == null || !_templateCatalog.IsInitialized)
                throw new InvalidOperationException("RoomTemplateCatalog 尚未初始化。");

            _roomsById.Clear();
            _connectionsById.Clear();
            _connectionIdsByRoomId.Clear();

            _roomObjectsById.Clear();
            _connectionObjectsById.Clear();

            _heldPoolIds.Clear();

            IsInitialized = true;

            Debug.Log($"[{nameof(MapVisualBuilder)}] 初始化完成。");

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 房间从对象池取出并完成位置设置后，由构建流程调用一次。
        /// </summary>
        public RoomViewRuntime RegisterRoom(int roomId, RoomView view)
        {
            EnsureInitialized();

            if (view == null)
                throw new ArgumentNullException(nameof(view));

            if (_roomsById.ContainsKey(roomId))
                throw new InvalidOperationException($"Room {roomId} 已经注册。");

            RoomViewRuntime runtime = new RoomViewRuntime(roomId, view);

            _roomsById.Add(roomId, runtime);
            _connectionIdsByRoomId.Add(roomId, new List<int>());

            // 房间首次注册时，所有插槽默认保持封闭。
            for (int i = 0; i < runtime.Connectors.Count; i++)
                ApplyConnectorState(runtime.Connectors[i], false);

            return runtime;
        }

        /// <summary>
        /// 根据视觉方案构建完整本地地图。
        /// 构建顺序：
        /// 1. 生成所有房间；
        /// 2. 打开房间所需插槽；
        /// 3. 生成每条唯一连接。
        /// </summary>
        public async UniTask BuildAsync(MapBuildPlan buildPlan, CancellationToken cancellationToken)
        {
            EnsureInitialized();

            buildPlan.Validate();

            if (buildPlan.ConnectionMode == ConnectionPresentationMode.ConnectionView &&
                string.IsNullOrWhiteSpace(_gridConnectionPoolId))
            {
                throw new InvalidOperationException("没有配置 Grid Connection Pool ID。");
            }

            HashSet<string> requiredPoolIds = CollectRequiredPoolIds(buildPlan);

            // 先归还上一张地图全部实例，让旧 Pool 的 RentedCount 归零。
            ClearVisualsInternal();

            // 当前先采用每层地图 Old-New 的释放方式。
            ReleaseUnusedPools(requiredPoolIds);

            // 先登记资源所有权。
            // 即使等待 Prepare 时外部取消，底层共享 Prepare 仍可能继续，
            // 后面的 Build / Clear 仍知道这个资源由地图系统申请过。
            foreach (string poolId in requiredPoolIds)
                _heldPoolIds.Add(poolId);

            try
            {
                await Pooling.LocalObjectPool.Instance.PreparePoolsAsync(requiredPoolIds, cancellationToken);

                BuildRoomsInternal(buildPlan, cancellationToken);

                if (buildPlan.ConnectionMode == ConnectionPresentationMode.Seamless)
                    AlignSeamlessRoomsToConnectorAnchors(buildPlan);

                BuildConnectionsInternal(buildPlan, cancellationToken);

                Debug.Log(
                    $"[{nameof(MapVisualBuilder)}] 地图视觉构建完成，Rooms={_roomObjectsById.Count}，" +
                    $"LogicalConnections={_connectionsById.Count}，ConnectionViews={_connectionObjectsById.Count}，" +
                    $"Mode={buildPlan.ConnectionMode}，Scale={buildPlan.RoomScale}，Pools={requiredPoolIds.Count}");
            }
            catch
            {
                // 只回收已经实际 Rent 出来的地图实例。
                // Prepare 出来的资源暂时保留，下一次 Build 可以直接复用。
                ClearVisualsInternal();
                throw;
            }
        }

        private HashSet<string> CollectRequiredPoolIds(MapBuildPlan buildPlan)
        {
            HashSet<string> requiredPoolIds = new(StringComparer.Ordinal);

            for (int i = 0; i < buildPlan.Rooms.Count; i++)
            {
                MapRoomBuildDefinition definition = buildPlan.Rooms[i];
                RoomTemplateConfig template = _templateCatalog.GetTemplate(definition.TemplateId);

                if (string.IsNullOrWhiteSpace(template.PoolId))
                    throw new InvalidOperationException($"房间模板 {template.TemplateId} 没有配置 PoolId。");

                if (!Pooling.LocalObjectPool.Instance.ContainsPool(template.PoolId))
                    throw new InvalidOperationException($"LocalObjectPool 没有登记房间 Pool：Template={template.TemplateId}，Pool={template.PoolId}");

                requiredPoolIds.Add(template.PoolId);
            }

            if (buildPlan.ConnectionMode == ConnectionPresentationMode.ConnectionView &&
                buildPlan.Connections.Count > 0)
            {
                if (!Pooling.LocalObjectPool.Instance.ContainsPool(_gridConnectionPoolId))
                    throw new InvalidOperationException($"LocalObjectPool 没有登记连接 Pool：{_gridConnectionPoolId}");

                requiredPoolIds.Add(_gridConnectionPoolId);
            }

            return requiredPoolIds;
        }

        /// <summary>
        /// 当前采用层级粒度资源策略：
        /// 只释放上一层持有、但新一层不再需要的 Pool。
        ///
        /// 后续做阶段式地图时，只需要替换这里的策略。
        /// </summary>
        private void ReleaseUnusedPools(HashSet<string> requiredPoolIds)
        {
            if (_heldPoolIds.Count == 0)
                return;

            List<string> releasePoolIds = new();

            foreach (string poolId in _heldPoolIds)
            {
                if (!requiredPoolIds.Contains(poolId))
                    releasePoolIds.Add(poolId);
            }

            if (releasePoolIds.Count == 0)
                return;

            if (!Pooling.LocalObjectPool.Instance.ReleasePools(releasePoolIds))
            {
                Debug.LogWarning($"[{nameof(MapVisualBuilder)}] 部分旧地图 Pool 暂时不能释放，将继续保留到后续 Build / Clear。");
                return;
            }

            for (int i = 0; i < releasePoolIds.Count; i++)
                _heldPoolIds.Remove(releasePoolIds[i]);
        }

        private void BuildRoomsInternal(MapBuildPlan buildPlan, CancellationToken cancellationToken)
        {
            for (int i = 0; i < buildPlan.Rooms.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                MapRoomBuildDefinition definition = buildPlan.Rooms[i];
                RoomTemplateConfig template = _templateCatalog.GetTemplate(definition.TemplateId);

                GameObject roomObject = Pooling.LocalObjectPool.Instance.Rent(template.PoolId, definition.WorldPosition, definition.WorldRotation, RoomRoot);
                roomObject.transform.localScale = Vector3.one * buildPlan.RoomScale;

                if (!roomObject.TryGetComponent(out RoomView roomView))
                {
                    Pooling.LocalObjectPool.Instance.Return(roomObject);
                    throw new InvalidOperationException($"房间模板 {template.TemplateId} 的预制体缺少 RoomView：{template.PoolId}");
                }

                
                //先记录对象，确保后续 Register 或插槽配置失败时，
                //ClearVisualsInternal 仍然能够统一回收它。
                _roomObjectsById.Add(definition.RoomId, roomObject);

                RoomViewRuntime runtime = RegisterRoom(definition.RoomId, roomView);
                ApplyConnectorMask(runtime, definition.RequiredLocalConnectors);
            }
        }

        /// <summary>
        /// Grid 拓扑中的 WorldPosition 只负责给出房间的大致方位。
        /// Seamless 模式再以预制体实际 Connector 为准逐房间吸附，
        /// 因此门锚点可以位于有厚度的墙体内部，不需要强制落在理论 Plane 边缘。
        /// 当前 GridGraph 是树结构，从 StartRoom 开始遍历可以得到唯一且可复现的摆放结果。
        /// </summary>
        private void AlignSeamlessRoomsToConnectorAnchors(MapBuildPlan buildPlan)
        {
            if (buildPlan.Rooms.Count <= 1 || buildPlan.Connections.Count == 0)
                return;

            Dictionary<int, MapRoomBuildDefinition> definitionsById = new(buildPlan.Rooms.Count);
            Dictionary<int, List<MapConnectionDefinition>> connectionsByRoomId = new(buildPlan.Rooms.Count);

            for (int i = 0; i < buildPlan.Rooms.Count; i++)
            {
                MapRoomBuildDefinition definition = buildPlan.Rooms[i];
                definitionsById.Add(definition.RoomId, definition);
                connectionsByRoomId.Add(definition.RoomId, new List<MapConnectionDefinition>(4));
            }

            for (int i = 0; i < buildPlan.Connections.Count; i++)
            {
                MapConnectionDefinition connection = buildPlan.Connections[i];

                if (!connectionsByRoomId.TryGetValue(connection.RoomAId, out List<MapConnectionDefinition> roomAConnections) ||
                    !connectionsByRoomId.TryGetValue(connection.RoomBId, out List<MapConnectionDefinition> roomBConnections))
                {
                    throw new InvalidOperationException(
                        $"Connection {connection.ConnectionId} 引用了不存在的房间：A={connection.RoomAId}，B={connection.RoomBId}");
                }

                roomAConnections.Add(connection);
                roomBConnections.Add(connection);
            }

            if (!_roomsById.ContainsKey(buildPlan.StartRoomId))
                throw new InvalidOperationException($"找不到起始房间运行时实例：{buildPlan.StartRoomId}");

            HashSet<int> alignedRoomIds = new() { buildPlan.StartRoomId };
            Queue<int> pendingRoomIds = new();
            pendingRoomIds.Enqueue(buildPlan.StartRoomId);

            while (pendingRoomIds.Count > 0)
            {
                int currentRoomId = pendingRoomIds.Dequeue();
                RoomViewRuntime currentRuntime = _roomsById[currentRoomId];
                MapRoomBuildDefinition currentDefinition = definitionsById[currentRoomId];
                List<MapConnectionDefinition> roomConnections = connectionsByRoomId[currentRoomId];

                for (int i = 0; i < roomConnections.Count; i++)
                {
                    MapConnectionDefinition connection = roomConnections[i];
                    int neighborRoomId = connection.RoomAId == currentRoomId
                        ? connection.RoomBId
                        : connection.RoomAId;

                    if (alignedRoomIds.Contains(neighborRoomId))
                        continue;

                    RoomViewRuntime neighborRuntime = _roomsById[neighborRoomId];
                    MapRoomBuildDefinition neighborDefinition = definitionsById[neighborRoomId];
                    Vector3 nominalDirection = neighborDefinition.WorldPosition - currentDefinition.WorldPosition;

                    if (nominalDirection.sqrMagnitude <= 0.001f)
                    {
                        throw new InvalidOperationException(
                            $"Connection {connection.ConnectionId} 的两个房间没有有效的布局方向。");
                    }

                    Transform currentTransform = currentRuntime.View.transform;
                    Transform neighborTransform = neighborRuntime.View.transform;

                    RoomConnectorSlot currentConnector = ResolveRequiredConnector(
                        currentRuntime,
                        currentTransform.position + nominalDirection);
                    RoomConnectorSlot neighborConnector = ResolveRequiredConnector(
                        neighborRuntime,
                        neighborTransform.position - nominalDirection);

                    neighborTransform.position += currentConnector.Anchor.position - neighborConnector.Anchor.position;

                    alignedRoomIds.Add(neighborRoomId);
                    pendingRoomIds.Enqueue(neighborRoomId);
                }
            }

            if (alignedRoomIds.Count != buildPlan.Rooms.Count)
            {
                throw new InvalidOperationException(
                    $"Seamless 房间图不连通：Aligned={alignedRoomIds.Count}，Rooms={buildPlan.Rooms.Count}");
            }
        }

        /// <summary>
        /// 为每条 MapConnectionDefinition 建立运行时连接。
        /// Seamless 模式只校验并登记房间插槽，不实例化 ConnectionView。
        /// </summary>
        private void BuildConnectionsInternal(MapBuildPlan buildPlan, CancellationToken cancellationToken)
        {
            for (int i = 0; i < buildPlan.Connections.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                MapConnectionDefinition definition = buildPlan.Connections[i];

                if (!_roomsById.TryGetValue(definition.RoomAId, out RoomViewRuntime roomARuntime))
                    throw new InvalidOperationException($"Room {definition.RoomAId} 尚未生成。");

                if (!_roomsById.TryGetValue(definition.RoomBId, out RoomViewRuntime roomBRuntime))
                    throw new InvalidOperationException($"Room {definition.RoomBId} 尚未生成。");

                RoomConnectorSlot connectorA = ResolveRequiredConnector(roomARuntime, roomBRuntime.View.transform.position);
                RoomConnectorSlot connectorB = ResolveRequiredConnector(roomBRuntime, roomARuntime.View.transform.position);

                if (buildPlan.ConnectionMode == ConnectionPresentationMode.Seamless)
                {
                    ValidateSeamlessConnectorPair(
                        definition.ConnectionId,
                        connectorA,
                        connectorB,
                        buildPlan.RoomScale);

                    ConnectionViewRuntime seamlessRuntime = ConnectionViewRuntime.CreateSeamless(
                        definition.ConnectionId,
                        definition.RoomAId,
                        definition.RoomBId,
                        connectorA,
                        connectorB,
                        buildPlan.RoomScale);

                    RegisterConnectionRuntime(seamlessRuntime);
                    seamlessRuntime.SetLocked(false);
                    continue;
                }

                if (buildPlan.ConnectionMode != ConnectionPresentationMode.ConnectionView)
                    throw new InvalidOperationException($"不支持的连接呈现模式：{buildPlan.ConnectionMode}");

                ValidateConnectorPair(definition.ConnectionId, connectorA, connectorB);

                GameObject connectionObject = Pooling.LocalObjectPool.Instance.Rent(
                    _gridConnectionPoolId,
                    Vector3.zero,
                    Quaternion.identity,
                    ConnectionRoot);

                if (!connectionObject.TryGetComponent(out ConnectionView connectionView))
                {
                    Pooling.LocalObjectPool.Instance.Return(connectionObject);
                    throw new InvalidOperationException($"连接预制体 {_gridConnectionPoolId} 缺少 ConnectionView。");
                }

                _connectionObjectsById.Add(definition.ConnectionId, connectionObject);

               
                //Runtime 必须在拉伸前创建，
                //这样才能缓存StretchRoot的原始缩放。
                ConnectionViewRuntime runtime = new ConnectionViewRuntime(
                    definition.ConnectionId,
                    definition.RoomAId,
                    definition.RoomBId,
                    connectorA,
                    connectorB,
                    connectionView,
                    buildPlan.RoomScale);

                RegisterConnectionRuntime(runtime);

                ConfigureConnectionTransform(runtime);
                ValidateConfiguredPortals(runtime);
            }
        }

        /// <summary>
        /// 根据另一个房间的位置，找到当前房间对应的局部方向插槽。
        /// </summary>
        private static RoomConnectorSlot ResolveRequiredConnector(RoomViewRuntime roomRuntime, Vector3 neighborWorldPosition)
        {
            ConnectorDirection localDirection = ResolveLocalDirection(roomRuntime.View.transform, neighborWorldPosition);

            if (!roomRuntime.TryGetDirectionalConnector(localDirection, out RoomConnectorSlot connector))
                throw new InvalidOperationException($"Room {roomRuntime.RoomId} 缺少必要插槽：{localDirection}");

            return connector;
        }

        /// <summary>
        /// 检查两端锚点是否面向彼此，并且高度基本一致。
        /// 这类错误通常来自预制体 Connector 的位置或 forward 配错。
        /// </summary>
        private void ValidateConnectorPair(int connectionId, RoomConnectorSlot connectorA, RoomConnectorSlot connectorB)
        {
            Vector3 anchorA = connectorA.Anchor.position;
            Vector3 anchorB = connectorB.Anchor.position;
            Vector3 direction = anchorB - anchorA;

            if (direction.sqrMagnitude < 0.01f)
                throw new InvalidOperationException($"Connection {connectionId} 的两个 Connector 几乎位于同一点。");

            if (Mathf.Abs(anchorA.y - anchorB.y) > _maximumAnchorHeightDifference)
                throw new InvalidOperationException($"Connection {connectionId} 两端高度不一致：A={anchorA.y:F2}，B={anchorB.y:F2}");

            Vector3 normalizedDirection = direction.normalized;
            float facingA = Vector3.Dot(connectorA.Anchor.forward, normalizedDirection);
            float facingB = Vector3.Dot(connectorB.Anchor.forward, -normalizedDirection);

            if (facingA < _minimumAnchorFacingDot)
                throw new InvalidOperationException($"Connection {connectionId} 的 Connector A 没有朝向 Connector B，Dot={facingA:F2}");

            if (facingB < _minimumAnchorFacingDot)
                throw new InvalidOperationException($"Connection {connectionId} 的 Connector B 没有朝向 Connector A，Dot={facingB:F2}");
        }

        /// <summary>
        /// Grid 房间没有中间连接实体；两个开口必须位于同一世界坐标且朝向相反。
        /// </summary>
        private void ValidateSeamlessConnectorPair(
            int connectionId,
            RoomConnectorSlot connectorA,
            RoomConnectorSlot connectorB,
            float roomScale)
        {
            Vector3 anchorA = connectorA.Anchor.position;
            Vector3 anchorB = connectorB.Anchor.position;
            float scaledTolerance = Mathf.Max(0.001f, _seamPositionTolerance * roomScale);
            float distance = Vector3.Distance(anchorA, anchorB);

            if (distance > scaledTolerance)
            {
                throw new InvalidOperationException(
                    $"Connection {connectionId} 的 Seamless Connector 没有重合：" +
                    $"Distance={distance:F4}，Tolerance={scaledTolerance:F4}，A={anchorA}，B={anchorB}");
            }

            if (Mathf.Abs(anchorA.y - anchorB.y) > _maximumAnchorHeightDifference * roomScale)
            {
                throw new InvalidOperationException(
                    $"Connection {connectionId} 两端高度不一致：A={anchorA.y:F2}，B={anchorB.y:F2}");
            }

            float oppositeFacing = Vector3.Dot(connectorA.Anchor.forward, connectorB.Anchor.forward);

            if (oppositeFacing > -_minimumAnchorFacingDot)
            {
                throw new InvalidOperationException(
                    $"Connection {connectionId} 的 Seamless Connector 朝向应相反，Dot={oppositeFacing:F2}");
            }
        }

        /// <summary>
        /// 将 ConnectionView 放在两端 Connector 中间，并沿本地 Z 轴拉伸。
        /// </summary>
        private static void ConfigureConnectionTransform(ConnectionViewRuntime runtime)
        {
            ConnectionView view = runtime.View;
            Vector3 anchorA = runtime.ConnectorA.Anchor.position;
            Vector3 anchorB = runtime.ConnectorB.Anchor.position;
            Vector3 connectionVector = anchorB - anchorA;
            float connectionLength = connectionVector.magnitude;

            if (connectionLength <= 0.01f)
                throw new InvalidOperationException($"Connection {runtime.ConnectionId} 的长度无效。");

            if (view.BaseLength <= 0f)
                throw new InvalidOperationException($"Connection {runtime.ConnectionId} 的 BaseLength 必须大于 0。");

            Vector3 midpoint = (anchorA + anchorB) * 0.5f;
            Quaternion rotation = Quaternion.LookRotation(connectionVector.normalized, Vector3.up);

            view.transform.SetPositionAndRotation(midpoint, rotation);

            /*
             * BaseLength 表示预制体处于原始缩放时的通道长度。
             * 只调整本地 Z，不改变宽度和高度。
             */
            float lengthScale = connectionLength / view.BaseLength;
            Vector3 stretchScale = runtime.OriginalStretchScale;
            stretchScale.z *= lengthScale;

            view.StretchRoot.localScale = stretchScale;

            // 地图刚生成时，所有战斗门默认开启。
            ApplyBattleGateState(view, false);
        }

        /// <summary>
        /// 检查 PortalA 是否靠近 Room A，PortalB 是否靠近 Room B，且 forward 朝向正确。
        /// </summary>
        private static void ValidateConfiguredPortals(ConnectionViewRuntime runtime)
        {
            Vector3 anchorA = runtime.ConnectorA.Anchor.position;
            Vector3 anchorB = runtime.ConnectorB.Anchor.position;

            float normalDistance = (runtime.View.PortalA.position - anchorA).sqrMagnitude + (runtime.View.PortalB.position - anchorB).sqrMagnitude;
            float swappedDistance = (runtime.View.PortalA.position - anchorB).sqrMagnitude + (runtime.View.PortalB.position - anchorA).sqrMagnitude;

            if (swappedDistance + 0.01f < normalDistance)
                throw new InvalidOperationException($"Connection {runtime.ConnectionId} 的 PortalA 与 PortalB 可能配置反了。");

            Vector3 directionAToB = (anchorB - anchorA).normalized;
            float portalAFacing = Vector3.Dot(runtime.View.PortalA.forward, -directionAToB);
            float portalBFacing = Vector3.Dot(runtime.View.PortalB.forward, directionAToB);

            if (portalAFacing < 0.5f)
                throw new InvalidOperationException($"Connection {runtime.ConnectionId} 的 PortalA.forward 应朝向 Room A。");

            if (portalBFacing < 0.5f)
                throw new InvalidOperationException($"Connection {runtime.ConnectionId} 的 PortalB.forward 应朝向 Room B。");
        }

        private void RegisterConnectionRuntime(ConnectionViewRuntime runtime)
        {
            if (runtime == null)
                throw new ArgumentNullException(nameof(runtime));

            if (_connectionsById.ContainsKey(runtime.ConnectionId))
                throw new InvalidOperationException($"Connection {runtime.ConnectionId} 已经注册。");

            if (!_roomsById.ContainsKey(runtime.RoomAId) || !_roomsById.ContainsKey(runtime.RoomBId))
                throw new InvalidOperationException($"Connection {runtime.ConnectionId} 引用了尚未注册的房间。");

            _connectionsById.Add(runtime.ConnectionId, runtime);
            _connectionIdsByRoomId[runtime.RoomAId].Add(runtime.ConnectionId);
            _connectionIdsByRoomId[runtime.RoomBId].Add(runtime.ConnectionId);
        }

        /// <summary>
        /// 获取从当前房间可以进入其他房间的全部 Portal。
        /// 返回结果由调用方立即使用，不应长期保存。
        /// </summary>
        public void CollectOutgoingPortals(int currentRoomId, List<RoomPortalRuntime> results)
        {
            EnsureInitialized();

            if (results == null)
                throw new ArgumentNullException(nameof(results));

            results.Clear();

            if (!_connectionIdsByRoomId.TryGetValue(currentRoomId, out List<int> connectionIds))
                return;

            for (int i = 0; i < connectionIds.Count; i++)
            {
                if (_connectionsById.TryGetValue(connectionIds[i], out ConnectionViewRuntime connection))
                    results.Add(connection.GetPortalLeavingRoom(currentRoomId));
            }
        }

        /// <summary>
        /// 根据房间局部连接掩码，统一切换墙体和门框。
        /// </summary>
        private static void ApplyConnectorMask(RoomViewRuntime roomRuntime, ConnectorMask requiredMask)
        {
            for (int i = 0; i < roomRuntime.Connectors.Count; i++)
                ApplyConnectorState(roomRuntime.Connectors[i], false);

            ConnectorDirection[] directions =
            {
             ConnectorDirection.North,
             ConnectorDirection.East,
             ConnectorDirection.South,
             ConnectorDirection.West
             };

            for (int i = 0; i < directions.Length; i++)
            {
                ConnectorDirection direction = directions[i];
                ConnectorMask directionMask = ConnectorMaskUtility.FromDirection(direction);

                if ((requiredMask & directionMask) == 0)
                    continue;

                if (!roomRuntime.TryGetDirectionalConnector(direction, out RoomConnectorSlot connector))
                    throw new InvalidOperationException($"Room {roomRuntime.RoomId} 缺少必要插槽：{direction}");

                ApplyConnectorState(connector, true);
            }
        }

        public UniTask ClearMapVisualsAsync(CancellationToken cancellationToken)
        {
            EnsureInitialized();

            // 清理不能做到一半，所以这里不使用外部 Token 中断。
            ClearVisualsInternal();
            ReleaseHeldPools();

            return UniTask.CompletedTask;
        }

        private void ReleaseHeldPools()
        {
            if (_heldPoolIds.Count == 0)
                return;

            if (Pooling.LocalObjectPool.Instance == null || !Pooling.LocalObjectPool.Instance.IsInitialized)
            {
                _heldPoolIds.Clear();
                return;
            }

            if (!Pooling.LocalObjectPool.Instance.ReleasePools(_heldPoolIds))
            {
                Debug.LogWarning($"[{nameof(MapVisualBuilder)}] 仍有地图 Pool 暂时不能释放，交由后续 Pool Shutdown 兜底。");
                return;
            }

            _heldPoolIds.Clear();
        }

        private void ClearVisualsInternal()
        {
           
           //先恢复ConnectionView的拉伸和门状态，
           //再把实例放回 LocalObjectPool。
            foreach (KeyValuePair<int, ConnectionViewRuntime> pair in _connectionsById)
            {
                ConnectionViewRuntime runtime = pair.Value;

                runtime.SetLocked(false);

                if (runtime.View != null)
                {
                    runtime.View.StretchRoot.localScale = runtime.OriginalStretchScale;
                }
            }

            foreach (GameObject connectionObject in _connectionObjectsById.Values)
            {
                if (connectionObject != null)
                    ReturnOrDestroy(connectionObject);
            }

            /*
             * 房间回池前恢复为全封闭状态，
             * 防止下一次复用时短暂显示上一张地图的开口。
             */
            foreach (RoomViewRuntime runtime in _roomsById.Values)
            {
                for (int i = 0; i < runtime.Connectors.Count; i++)
                    ApplyConnectorState(runtime.Connectors[i], false);
            }

            foreach (GameObject roomObject in _roomObjectsById.Values)
            {
                if (roomObject != null)
                    ReturnOrDestroy(roomObject);
            }

            _connectionObjectsById.Clear();
            _roomObjectsById.Clear();

            _connectionsById.Clear();
            _connectionIdsByRoomId.Clear();
            _roomsById.Clear();
        }

        private static void ReturnOrDestroy(GameObject instance)
        {
            if (Pooling.LocalObjectPool.Instance != null && Pooling.LocalObjectPool.Instance.IsInitialized)
            {
                Pooling.LocalObjectPool.Instance.Return(instance);
                return;
            }

            Destroy(instance);
        }


        /// <summary>
        /// 锁定或打开一个房间关联的全部战斗门。
        /// </summary>
        public void SetRoomConnectionsLocked(int roomId, bool locked)
        {
            EnsureInitialized();

            if (!_connectionIdsByRoomId.TryGetValue(roomId, out List<int> connectionIds))
            {
                Debug.LogWarning($"[{nameof(MapVisualBuilder)}] 找不到 Room {roomId} 的连接数据。");
                return;
            }

            for (int i = 0; i < connectionIds.Count; i++)
            {
                if (_connectionsById.TryGetValue(connectionIds[i], out ConnectionViewRuntime connection))
                    connection.SetLocked(locked);
            }
        }

        public bool TryGetRoom(int roomId, out RoomViewRuntime room)
        {
            return _roomsById.TryGetValue(roomId, out room);
        }

        public bool TryGetConnection(int connectionId, out ConnectionViewRuntime connection)
        {
            return _connectionsById.TryGetValue(connectionId, out connection);
        }

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            if (!IsInitialized)
                return UniTask.CompletedTask;

            // 必须先正常把地图对象还给 LocalObjectPool。
            ClearVisualsInternal();

            // 再释放地图阶段持有的资源。
            ReleaseHeldPools();

            _heldPoolIds.Clear();

            IsInitialized = false;

            Debug.Log($"[{nameof(MapVisualBuilder)}] 已关闭并归还全部地图实例。");

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 墙体和门框的实际切换只允许在 Builder 内执行。
        /// RoomConnectorSlot 自身不包含任何行为方法。
        /// </summary>
        private static void ApplyConnectorState(RoomConnectorSlot connector, bool connected)
        {
            if (connector.ClosedWallRoot != null)
                connector.ClosedWallRoot.SetActive(!connected);

            if (connector.OpenFrameRoot != null)
                connector.OpenFrameRoot.SetActive(connected);
        }

        private static void ApplyBattleGateState(ConnectionView connectionView, bool locked)
        {
            if (connectionView != null && connectionView.BattleGateRoot != null)
                connectionView.BattleGateRoot.SetActive(locked);
        }

        /// <summary>
        /// 将相邻房间的世界方向转换为当前房间的局部方向。
        /// 因此即使整个房间发生旋转，也仍能匹配正确的本地插槽。
        /// </summary>
        private static ConnectorDirection ResolveLocalDirection(Transform roomTransform, Vector3 neighborWorldPosition)
        {
            Vector3 worldDirection = neighborWorldPosition - roomTransform.position;
            Vector3 localDirection = roomTransform.InverseTransformDirection(worldDirection.normalized);

            if (Mathf.Abs(localDirection.x) > Mathf.Abs(localDirection.z))
                return localDirection.x >= 0f ? ConnectorDirection.East : ConnectorDirection.West;

            return localDirection.z >= 0f ? ConnectorDirection.North : ConnectorDirection.South;
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException($"{nameof(MapVisualBuilder)} 尚未初始化。");
        }
    }
}
