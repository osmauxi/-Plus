using System;
using System.Collections;
using System.Collections.Generic;
using ProjectGame.HotFix.Core.Events;
using ProjectGame.HotFix.Gameplay.Map.Generation;
using ProjectGame.HotFix.Gameplay.Map.View;
using ProjectGame.HotFix.Gameplay.Player;
using ProjectGame.HotFix.Gameplay.Spawning;
using ProjectGame.HotFix.Gameplay.State;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    public enum MonsterStressSpawnCenter : byte
    {
        RoomCenter = 0,
        FirstPlayer = 1
    }

    public enum MonsterStressSpawnArea : byte
    {
        Ring = 0,
        Disc = 1,
        WholeRoomSpawnRegion = 2
    }

    /// <summary>
    /// StressArena 专用的服务器怪物导航压测管理器。
    /// 地图完成并进入 GamePlaying 后开启一个 Monster 房间，按 Inspector 参数分批生成怪物。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MonsterNavigationStressTestManager : MonoBehaviour
    {
        [Header("运行依赖")]
        [Tooltip("用于从压测布局的 StartRoomId 获取已经实例化的 RoomView。")]
        [SerializeField] private MapVisualBuilder _mapVisualBuilder;

        [Tooltip("复用正式怪物权威模拟、Flow Field、网络同步与表现链路。")]
        [SerializeField] private MonsterRuntimeService _monsterRuntimeService;

        [Header("启动条件")]
        [Tooltip("关闭后仍可生成 StressArena 地图，但不会自动开启怪物压测。")]
        [SerializeField] private bool _autoStart = true;

        [Tooltip("GamePlaying 且玩家准备完成后，再额外等待多久开始生成。使用非缩放时间。")]
        [SerializeField, Min(0f)] private float _startDelaySeconds = 1f;

        [Tooltip("叠加到地图 Seed 上的偏移。保持地图 Seed 和该值不变即可复现生成位置与朝向。")]
        [SerializeField] private int _seedOffset = 7919;

        [Header("怪物种类")]
        [Tooltip("启用时使用 MonsterSpawn 配表中的第一条有效配置，忽略下面的 ConfigId。")]
        [SerializeField] private bool _useFirstSpawnCatalogEntry = true;

        [Tooltip("关闭“使用第一条配置”时生效。这里填写 MonsterRuntime 配表的 ConfigId，不是运行时 ushort 索引。")]
        [SerializeField, Min(0)] private int _monsterConfigId;

        [Header("压力规模")]
        [Tooltip("首次生成或持续补充时希望保持的存活怪物数量。")]
        [SerializeField, Min(1)] private int _targetAliveMonsterCount = 256;

        [Tooltip("本次房间允许创建的 Slot 总数上限。死亡 Slot 在房间结束前不会复用，因此该值必须有限。")]
        [SerializeField, Min(1)] private int _maximumTotalSpawnCount = 256;

        [Tooltip("启用后怪物死亡时会补充到目标存活数，直到达到总生成上限；关闭时只生成初始目标数量。")]
        [SerializeField] private bool _maintainTargetAliveCount;

        [Tooltip("每个可靠 SpawnBatch 的怪物数量。较小值可降低单帧生成和单包压力。")]
        [SerializeField, Min(1)] private int _spawnBatchSize = 64;

        [Tooltip("初始批次之间的非缩放时间间隔。设为 0 时每帧生成一个批次。")]
        [SerializeField, Min(0f)] private float _batchIntervalSeconds = 0.05f;

        [Tooltip("持续补充模式下检查存活数量的非缩放时间间隔。")]
        [SerializeField, Min(0.01f)] private float _refillCheckIntervalSeconds = 0.5f;

        [Header("生成中心")]
        [Tooltip("RoomCenter 使用房间 SpawnRegion 中心；FirstPlayer 使用服务器列表中的第一名玩家当前位置。")]
        [SerializeField] private MonsterStressSpawnCenter _spawnCenter = MonsterStressSpawnCenter.FirstPlayer;

        [Tooltip("在房间本地 XZ 平面上的额外中心偏移。偏移会随房间旋转和 Scale 变换。")]
        [SerializeField] private Vector2 _roomLocalCenterOffset;

        [Header("生成区域")]
        [Tooltip("Ring：固定半径圆环；Disc：圆盘；WholeRoomSpawnRegion：整个 RoomView SpawnRegion。")]
        [SerializeField] private MonsterStressSpawnArea _spawnArea = MonsterStressSpawnArea.Ring;

        [Tooltip("Ring/Disc 的世界单位半径。使用大 RoomScale 时，该值不会自动随 Scale 增长。")]
        [SerializeField, Min(0.1f)] private float _spawnRadius = 25f;

        [Tooltip("Ring 的世界单位厚度。0 表示严格固定半径；大于 0 时在半径两侧形成环带。")]
        [SerializeField, Min(0f)] private float _ringThickness = 4f;

        [Tooltip("同一批次内两个出生点的最小世界距离。设为 0 可故意制造高密度重叠压力。")]
        [SerializeField, Min(0f)] private float _minimumSpawnSpacing = 0.5f;

        [Tooltip("每只怪物寻找合法 Ground 点时的最大随机尝试次数。")]
        [SerializeField, Min(1)] private int _spawnAttemptsPerMonster = 48;

        [Header("地面检测")]
        [Tooltip("所有怪物使用同一个二维模拟 GroundY；采样点与首个地面点的高度差不能超过此值。")]
        [SerializeField, Min(0f)] private float _groundHeightTolerance = 0.25f;

        [Tooltip("Ground 向下射线在 SpawnRegion 顶部额外抬高和底部额外延长的距离。")]
        [SerializeField, Min(0f)] private float _raycastPadding = 2f;

        [Tooltip("启用时怪物出生朝向生成中心；关闭时使用随机 Yaw。")]
        [SerializeField] private bool _faceSpawnCenter = true;

        [Header("诊断日志")]
        [Tooltip("每个 SpawnBatch 完成后输出当前 Alive、Slot 和累计生成数。大量小批次时建议关闭。")]
        [SerializeField] private bool _logEachBatch;

        [Tooltip("开始、达到目标或达到总上限时输出压测摘要。")]
        [SerializeField] private bool _logSummary = true;

        private readonly List<MonsterSpawnPlan> _spawnPlans = new();
        private readonly List<Vector3> _batchGroundPositions = new();

        private IDisposable _mapBuiltSubscription;
        private IDisposable _mapClearingSubscription;
        private Coroutine _spawnRoutine;
        private System.Random _random;
        private RoomView _roomView;
        private int _generationId = -1;
        private int _totalSpawned;
        private ushort _resolvedMonsterConfigIndex;
        private bool _ownsMonsterRoom;

        public bool IsRunning => _ownsMonsterRoom;
        public int TotalSpawned => _totalSpawned;

        private void OnEnable()
        {
            _mapBuiltSubscription = LocalEvents.Subscribe<MapRuntimeBuiltEvent>(HandleMapBuilt);
            _mapClearingSubscription = LocalEvents.Subscribe<MapRuntimeClearingEvent>(HandleMapClearing);
        }

        private void OnDisable()
        {
            _mapBuiltSubscription?.Dispose();
            _mapClearingSubscription?.Dispose();
            _mapBuiltSubscription = null;
            _mapClearingSubscription = null;
            StopStressRun(true);
        }

        private void HandleMapBuilt(MapRuntimeBuiltEvent mapEvent)
        {
            StopStressRun(true);

            if (!_autoStart || mapEvent.Layout.StrategyType != MapLayoutStrategyType.StressArena || !IsServer())
                return;

            ValidateSettings();

            if (_mapVisualBuilder == null)
                throw new InvalidOperationException("压测管理器没有绑定 MapVisualBuilder。");

            if (_monsterRuntimeService == null)
                throw new InvalidOperationException("压测管理器没有绑定 MonsterRuntimeService。");

            if (!_mapVisualBuilder.TryGetRoom(mapEvent.Layout.StartRoomId, out RoomViewRuntime roomRuntime))
                throw new InvalidOperationException($"找不到压测房间：RoomId={mapEvent.Layout.StartRoomId}");

            _generationId = mapEvent.GenerationId;
            _roomView = roomRuntime.View;
            _random = new System.Random(unchecked(mapEvent.Layout.Seed * 486187739 + _seedOffset));
            _spawnRoutine = StartCoroutine(RunStressTest(mapEvent.GenerationId));
        }

        private void HandleMapClearing(MapRuntimeClearingEvent mapEvent)
        {
            if (mapEvent.GenerationId == _generationId)
                StopStressRun(true);
        }

        private IEnumerator RunStressTest(int generationId)
        {
            while (IsCurrentRun(generationId) && !IsGameplayReady())
                yield return null;

            if (!IsCurrentRun(generationId))
                yield break;

            if (_startDelaySeconds > 0f)
                yield return new WaitForSecondsRealtime(_startDelaySeconds);

            if (!IsCurrentRun(generationId))
                yield break;

            BeginMonsterRoom();

            WaitForSecondsRealtime batchDelay = _batchIntervalSeconds > 0f
                ? new WaitForSecondsRealtime(_batchIntervalSeconds)
                : null;
            WaitForSecondsRealtime refillDelay = new WaitForSecondsRealtime(_refillCheckIntervalSeconds);

            if (_logSummary)
            {
                Debug.Log(
                    $"[{nameof(MonsterNavigationStressTestManager)}] 压测开始，Generation={generationId}，" +
                    $"TargetAlive={_targetAliveMonsterCount}，MaxTotal={_maximumTotalSpawnCount}，" +
                    $"Batch={_spawnBatchSize}，Area={_spawnArea}");
            }

            while (IsCurrentRun(generationId) && _totalSpawned < _maximumTotalSpawnCount)
            {
                int missingCount = _maintainTargetAliveCount
                    ? _targetAliveMonsterCount - _monsterRuntimeService.AliveMonsterCount
                    : _targetAliveMonsterCount - _totalSpawned;

                if (missingCount <= 0)
                {
                    if (!_maintainTargetAliveCount)
                        break;

                    yield return refillDelay;
                    continue;
                }

                int batchCount = Math.Min(
                    _spawnBatchSize,
                    Math.Min(missingCount, _maximumTotalSpawnCount - _totalSpawned));

                if (!TryBuildSpawnPlans(batchCount))
                {
                    Debug.LogError(
                        $"[{nameof(MonsterNavigationStressTestManager)}] 无法为完整批次找到合法出生点，" +
                        $"Batch={batchCount}，AttemptsPerMonster={_spawnAttemptsPerMonster}。");
                    break;
                }

                _monsterRuntimeService.Spawn(_spawnPlans);
                _totalSpawned += batchCount;

                if (_logEachBatch)
                {
                    Debug.Log(
                        $"[{nameof(MonsterNavigationStressTestManager)}] 已生成批次，" +
                        $"Batch={batchCount}，Alive={_monsterRuntimeService.AliveMonsterCount}，" +
                        $"Slots={_monsterRuntimeService.MonsterSlotCount}，Total={_totalSpawned}");
                }

                if (batchDelay != null)
                    yield return batchDelay;
                else
                    yield return null;
            }

            if (_logSummary && IsCurrentRun(generationId))
            {
                string reason = _totalSpawned >= _maximumTotalSpawnCount
                    ? "达到总生成上限"
                    : "达到目标数量";
                Debug.Log(
                    $"[{nameof(MonsterNavigationStressTestManager)}] {reason}，" +
                    $"Alive={_monsterRuntimeService.AliveMonsterCount}，" +
                    $"Slots={_monsterRuntimeService.MonsterSlotCount}，Total={_totalSpawned}");
            }

            _spawnRoutine = null;
        }

        private void BeginMonsterRoom()
        {
            if (!_monsterRuntimeService.IsInitialized)
                throw new InvalidOperationException("MonsterRuntimeService 尚未初始化。");

            if (_monsterRuntimeService.IsRoomActive)
                throw new InvalidOperationException("MonsterRuntimeService 已有活动房间，不能启动导航压测。");

            RoomSpawnRegion region = _roomView.SpawnRegion;
            if (!TrySampleAnyGround(region, out Vector3 ground))
                throw new InvalidOperationException("压测房间 SpawnRegion 内找不到 Ground。");

            ushort configIndex = ResolveMonsterConfigIndex();
            region.GetWorldXZBounds(out Vector2 quantizationOrigin, out Vector2 quantizationSize);
            _monsterRuntimeService.BeginRoom(
                quantizationOrigin,
                quantizationSize,
                ground.y,
                _roomView.SpatialData,
                _roomView.transform,
                _maximumTotalSpawnCount);

            _ownsMonsterRoom = true;
            _resolvedMonsterConfigIndex = configIndex;
            _spawnPlans.Capacity = Math.Max(_spawnPlans.Capacity, _spawnBatchSize);
            _batchGroundPositions.Capacity = Math.Max(_batchGroundPositions.Capacity, _spawnBatchSize);
            _totalSpawned = 0;
        }

        private bool TryBuildSpawnPlans(int count)
        {
            _spawnPlans.Clear();
            _batchGroundPositions.Clear();

            Vector3 center = ResolveSpawnCenter();
            float groundY = _monsterRuntimeService.RoomGroundY;
            RoomSpawnRegion region = _roomView.SpawnRegion;
            float spacingSqr = _minimumSpawnSpacing * _minimumSpawnSpacing;

            for (int monsterIndex = 0; monsterIndex < count; monsterIndex++)
            {
                bool accepted = false;

                for (int attempt = 0; attempt < _spawnAttemptsPerMonster; attempt++)
                {
                    Vector3 candidate = SampleHorizontalPosition(region, center);

                    if (!TryRaycastGround(region, candidate.x, candidate.z, out Vector3 ground) ||
                        Mathf.Abs(ground.y - groundY) > _groundHeightTolerance ||
                        !HasMinimumSpacing(ground, spacingSqr))
                    {
                        continue;
                    }

                    _batchGroundPositions.Add(ground);
                    float yaw = _faceSpawnCenter
                        ? CalculateFacingYaw(ground, center)
                        : (float)_random.NextDouble() * 360f;
                    _spawnPlans.Add(new MonsterSpawnPlan(
                        _resolvedMonsterConfigIndex,
                        new Vector2(ground.x, ground.z),
                        yaw));
                    accepted = true;
                    break;
                }

                if (accepted)
                    continue;

                _spawnPlans.Clear();
                _batchGroundPositions.Clear();
                return false;
            }

            return true;
        }

        private Vector3 ResolveSpawnCenter()
        {
            Vector3 center;

            if (_spawnCenter == MonsterStressSpawnCenter.FirstPlayer)
            {
                IReadOnlyList<PlayerRuntime> players = PlayerManager.Instance.RuntimePlayers;
                if (players.Count == 0)
                    throw new InvalidOperationException("压测生成时已经没有可用玩家。");
                center = players[0].transform.position;
            }
            else
            {
                RoomSpawnRegion region = _roomView.SpawnRegion;
                center = region.Space.TransformPoint(region.LocalCenter);
            }

            Vector3 offset = _roomView.transform.TransformVector(
                new Vector3(_roomLocalCenterOffset.x, 0f, _roomLocalCenterOffset.y));
            return center + offset;
        }

        private Vector3 SampleHorizontalPosition(in RoomSpawnRegion region, Vector3 center)
        {
            if (_spawnArea == MonsterStressSpawnArea.WholeRoomSpawnRegion)
            {
                Vector3 half = region.LocalSize * 0.5f;
                float localX = region.LocalCenter.x + Mathf.Lerp(-half.x, half.x, (float)_random.NextDouble());
                float localZ = region.LocalCenter.z + Mathf.Lerp(-half.z, half.z, (float)_random.NextDouble());
                return region.Space.TransformPoint(new Vector3(localX, region.LocalCenter.y, localZ));
            }

            float radius;
            if (_spawnArea == MonsterStressSpawnArea.Disc)
            {
                radius = Mathf.Sqrt((float)_random.NextDouble()) * _spawnRadius;
            }
            else if (_ringThickness > 0f)
            {
                float halfThickness = _ringThickness * 0.5f;
                float inner = Mathf.Max(0f, _spawnRadius - halfThickness);
                float outer = _spawnRadius + halfThickness;
                radius = Mathf.Sqrt(Mathf.Lerp(
                    inner * inner,
                    outer * outer,
                    (float)_random.NextDouble()));
            }
            else
            {
                radius = _spawnRadius;
            }

            float angle = (float)_random.NextDouble() * Mathf.PI * 2f;
            return new Vector3(
                center.x + Mathf.Cos(angle) * radius,
                center.y,
                center.z + Mathf.Sin(angle) * radius);
        }

        private bool TrySampleAnyGround(in RoomSpawnRegion region, out Vector3 ground)
        {
            Vector3 half = region.LocalSize * 0.5f;

            for (int attempt = 0; attempt < _spawnAttemptsPerMonster; attempt++)
            {
                float localX = region.LocalCenter.x + Mathf.Lerp(-half.x, half.x, (float)_random.NextDouble());
                float localZ = region.LocalCenter.z + Mathf.Lerp(-half.z, half.z, (float)_random.NextDouble());
                Vector3 world = region.Space.TransformPoint(new Vector3(localX, region.LocalCenter.y, localZ));

                if (TryRaycastGround(region, world.x, world.z, out ground))
                    return true;
            }

            ground = default;
            return false;
        }

        private bool TryRaycastGround(in RoomSpawnRegion region, float worldX, float worldZ, out Vector3 ground)
        {
            Vector3 reference = region.Space.TransformPoint(region.LocalCenter);
            Vector3 local = region.Space.InverseTransformPoint(new Vector3(worldX, reference.y, worldZ));
            Vector3 half = region.LocalSize * 0.5f;

            if (local.x < region.LocalCenter.x - half.x || local.x > region.LocalCenter.x + half.x ||
                local.z < region.LocalCenter.z - half.z || local.z > region.LocalCenter.z + half.z)
            {
                ground = default;
                return false;
            }

            float localTop = region.LocalCenter.y + half.y;
            float localBottom = region.LocalCenter.y - half.y;
            Vector3 top = region.Space.TransformPoint(new Vector3(local.x, localTop, local.z));
            Vector3 bottom = region.Space.TransformPoint(new Vector3(local.x, localBottom, local.z));
            Vector3 origin = top + Vector3.up * _raycastPadding;
            float distance = Mathf.Abs(origin.y - bottom.y) + _raycastPadding;

            if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, distance,
                    region.ProbeMask, QueryTriggerInteraction.Ignore) ||
                (region.GroundMask.value & (1 << hit.collider.gameObject.layer)) == 0)
            {
                ground = default;
                return false;
            }

            ground = hit.point;
            return true;
        }

        private bool HasMinimumSpacing(Vector3 candidate, float spacingSqr)
        {
            if (spacingSqr <= 0f)
                return true;

            for (int i = 0; i < _batchGroundPositions.Count; i++)
            {
                Vector3 existing = _batchGroundPositions[i];
                float x = candidate.x - existing.x;
                float z = candidate.z - existing.z;

                if (x * x + z * z < spacingSqr)
                    return false;
            }

            return true;
        }

        private ushort ResolveMonsterConfigIndex()
        {
            MonsterCatalogBundle catalogs = _monsterRuntimeService.Catalogs;

            if (_useFirstSpawnCatalogEntry)
            {
                if (catalogs.Spawns.Count == 0)
                    throw new InvalidOperationException("MonsterSpawn 配表没有可用于压测的配置。");

                return catalogs.Spawns.Get(0).ConfigIndex;
            }

            if (!catalogs.Runtime.TryGetIndex(_monsterConfigId, out ushort index))
                throw new InvalidOperationException($"找不到压测怪物 ConfigId={_monsterConfigId}。");

            return index;
        }

        private bool IsGameplayReady()
        {
            return _monsterRuntimeService != null &&
                   _monsterRuntimeService.IsInitialized &&
                   GameStateController.Instance != null &&
                   GameStateController.Instance.IsPlaying &&
                   PlayerManager.Instance != null &&
                   PlayerManager.Instance.IsInitialized &&
                   PlayerManager.Instance.SpawnedPlayerCount > 0;
        }

        private bool IsCurrentRun(int generationId) =>
            generationId == _generationId && _roomView != null && IsServer();

        private void StopStressRun(bool endMonsterRoom)
        {
            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
                _spawnRoutine = null;
            }

            if (endMonsterRoom && _ownsMonsterRoom && IsServer() &&
                _monsterRuntimeService != null && _monsterRuntimeService.IsInitialized &&
                _monsterRuntimeService.IsRoomActive)
            {
                _monsterRuntimeService.EndRoom();
            }

            _ownsMonsterRoom = false;
            _generationId = -1;
            _roomView = null;
            _random = null;
            _totalSpawned = 0;
            _resolvedMonsterConfigIndex = 0;
            _spawnPlans.Clear();
            _batchGroundPositions.Clear();
        }

        private void ValidateSettings()
        {
            if (_targetAliveMonsterCount < 1)
                throw new InvalidOperationException("压测目标存活数量必须大于 0。");
            if (_maximumTotalSpawnCount < _targetAliveMonsterCount)
                throw new InvalidOperationException("压测总生成上限不能小于目标存活数量。");
            if (_spawnBatchSize < 1 || _spawnAttemptsPerMonster < 1)
                throw new InvalidOperationException("压测批次和采样尝试次数必须大于 0。");
            if (!IsNonNegativeFinite(_startDelaySeconds) ||
                !IsNonNegativeFinite(_batchIntervalSeconds) ||
                !IsPositiveFinite(_refillCheckIntervalSeconds) ||
                !IsPositiveFinite(_spawnRadius) ||
                !IsNonNegativeFinite(_ringThickness) ||
                !IsNonNegativeFinite(_minimumSpawnSpacing) ||
                !IsNonNegativeFinite(_groundHeightTolerance) ||
                !IsNonNegativeFinite(_raycastPadding))
            {
                throw new InvalidOperationException("压测管理器包含非法的时间、半径、间距或地面检测参数。");
            }
            if (!Enum.IsDefined(typeof(MonsterStressSpawnCenter), _spawnCenter) ||
                !Enum.IsDefined(typeof(MonsterStressSpawnArea), _spawnArea))
            {
                throw new InvalidOperationException("压测生成中心或生成区域枚举非法。");
            }
        }

        private static float CalculateFacingYaw(Vector3 position, Vector3 center)
        {
            Vector2 direction = new Vector2(center.x - position.x, center.z - position.z);
            return direction.sqrMagnitude <= 0.000001f
                ? 0f
                : Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        }

        private static bool IsServer()
        {
            NetworkManager manager = NetworkManager.Singleton;
            return manager != null && manager.IsListening && manager.IsServer;
        }

        private static bool IsNonNegativeFinite(float value) =>
            value >= 0f && !float.IsNaN(value) && !float.IsInfinity(value);

        private static bool IsPositiveFinite(float value) =>
            value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
