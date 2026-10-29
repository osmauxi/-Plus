using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ProjectGame.Bootstrap;
using ProjectGame.HotFix.Gameplay.Benchmarking;
using ProjectGame.HotFix.Gameplay.Monsters;
using ProjectGame.HotFix.Gameplay.Network;
using ProjectGame.HotFix.Gameplay.Player;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using ProjectGame.HotFix.Gameplay.Weapon;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace ProjectGame.HotFix.Benchmarking
{
    [Serializable]
    internal sealed class BenchmarkRoleMetrics
    {
        public int schemaVersion = 6;
        public string runId;
        public string role;
        public string focusTarget;
        public string unityVersion;
        public string platform;
        public string graphicsDevice;
        public string qualityLevel;
        public int width;
        public int height;
        public int vSyncCount;
        public int targetFrameRate;
        public int monsterCount;
        public int sampleCount;
        public int focusedFrameCount;
        public int unfocusedFrameCount;
        public int focusChangeCount;
        public double actualDurationSeconds;
        public double averageFrameMilliseconds;
        public double p50FrameMilliseconds;
        public double p90FrameMilliseconds;
        public double p95FrameMilliseconds;
        public double p99FrameMilliseconds;
        public double maximumFrameMilliseconds;
        public double averageMainThreadMilliseconds;
        public double p95MainThreadMilliseconds;
        public double p99MainThreadMilliseconds;
        public double maximumMainThreadMilliseconds;
        public double averageRenderThreadMilliseconds;
        public double p95RenderThreadMilliseconds;
        public double p99RenderThreadMilliseconds;
        public double maximumRenderThreadMilliseconds;
        public double averageGpuFrameMilliseconds;
        public double p95GpuFrameMilliseconds;
        public double p99GpuFrameMilliseconds;
        public double maximumGpuFrameMilliseconds;
        public long gcAllocatedBytes;
        public double averageGcAllocatedBytes;
        public double p95GcAllocatedBytes;
        public long maximumGcAllocatedBytes;
        public int gcCollectionCount0;
        public int gcCollectionCount1;
        public int gcCollectionCount2;
        public long monoUsedBytes;
        public long totalAllocatedMemoryBytes;
        public long sentMessageCount;
        public long sentPayloadBytes;
        public long receivedMessageCount;
        public long receivedPayloadBytes;
        public int authorityAliveMonsterCount;
        public int replicaSlotCount;
        public int replicaActiveCount;
        public int activeViewCount;
        public int viewSlotCount;
        public int sourceRebuildCount;
        public int topologyRebuildCount;
        public int costRebuildCount;
        public int movementCommands;
        public int movementWaypoints;
        public float movementDistance;
        public int spatialBuildCount;
        public int spatialQueryCount;
        public long spatialCandidateCount;
        public long spatialVisitedNodeCount;
        public double averageSpatialCandidateCount;
        public int p50SpatialCandidateCount;
        public int p95SpatialCandidateCount;
        public int maximumSpatialCandidateCount;
        public double averageSpatialVisitedNodeCount;
        public int p50SpatialVisitedNodeCount;
        public int p95SpatialVisitedNodeCount;
        public int maximumSpatialVisitedNodeCount;
        public double averageSpatialCandidateRatio;
        public double p95SpatialCandidateRatio;
        public int shotFiredCount;
        public int projectileSpawnedCount;
        public double averageActiveProjectileCount;
        public int maximumActiveProjectileCount;
        public bool workloadValid;
        public string invalidReason;
        public string captureStartedUtc;
        public string captureStoppedUtc;
        public string percentileMethod = "nearest-rank";
    }

    internal sealed class BenchmarkRuntimeRecorder : MonoBehaviour
    {
        private static readonly string[] CustomMarkerNames =
        {
            "Monster.FlowField.Rebuild.Topology",
            "Monster.FlowField.Rebuild.Source",
            "Monster.FlowField.Rebuild.Cost",
            "Monster.FlowField.Build",
            "Monster.Simulation.DensityUpdate",
            "Monster.Simulation.FlowFieldUpdate",
            "Monster.Simulation.SeparationBuild",
            "Monster.Simulation.EntityTick",
            "Monster.Simulation.EntityTick.Target",
            "Monster.Simulation.EntityTick.Attack",
            "Monster.Simulation.EntityTick.Navigation",
            "Monster.Simulation.EntityTick.Separation",
            "Monster.Simulation.EntityTick.Constraint",
            "Monster.Density.Capture",
            "Monster.Density.Kernel",
            "Monster.Separation.Capture",
            "Monster.Separation.Kernel",
            "Monster.Presentation.Capture",
            "Monster.Presentation.Apply",
            "Monster.Presentation.Serialize",
            "Monster.Presentation.Decode",
            "Monster.Replication.Publish",
            "Monster.Replication.Capture",
            "Monster.Replication.ApplyLocal",
            "Monster.Replication.SendPresentation",
            "Monster.Replication.ReceiveApply",
            "Monster.Network.SendPresentation",
            "Monster.Network.SendPresentation.Transport",
            "Monster.Network.ReceivePresentation",
            "Gameplay.TickClock.WorldSimulation",
            "Gameplay.TickClock.PhysicsSync",
            "Gameplay.TickClock.MeleeCombat",
            "Gameplay.TickClock.CombatSimulation",
            "Gameplay.TickClock.Replication",
            "Monster.View.ApplyAuthority",
            "Monster.SpatialIndex.Build",
            "Monster.SpatialIndex.Query",
            "Weapon.Projectile.MonsterNarrowPhase",
        };

        private sealed class Counter
        {
            public string Name;
            public ProfilerRecorder Recorder;
            public bool Available;
            public long MaximumValidValue = long.MaxValue;
            public long Sum;
            public long Maximum;
            public long SampleCount;
            public int NonZeroFrames;
            public int ValidFrameCount;
            public readonly List<long> FrameValues = new();

            public bool TryGetLatestValue(out long value)
            {
                value = -1L;
                if (!Available || Recorder.Count == 0)
                    return false;

                value = Recorder.GetSample(Recorder.Count - 1).Value;
                return value >= 0L && value <= MaximumValidValue;
            }

            public void Accumulate()
            {
                if (!TryGetLatestValue(out long value))
                    return;
                ProfilerRecorderSample sample = Recorder.GetSample(Recorder.Count - 1);
                Sum += value;
                Maximum = Math.Max(Maximum, value);
                SampleCount += sample.Count;
                ValidFrameCount++;
                FrameValues.Add(value);
                if (value != 0)
                    NonZeroFrames++;
            }

            public void Dispose()
            {
                if (Available)
                    Recorder.Dispose();
            }
        }

        private readonly List<double> _frameMilliseconds = new();
        private readonly List<double> _mainThreadMilliseconds = new();
        private readonly List<double> _renderThreadMilliseconds = new();
        private readonly List<double> _gpuFrameMilliseconds = new();
        private readonly List<long> _gcAllocatedBytes = new();
        private readonly List<string> _frameRows = new();
        private readonly List<Counter> _markerCounters = new();
        private Counter _mainThread;
        private Counter _renderThread;
        private Counter _gpuFrame;
        private Counter _gcAllocated;
        private BenchmarkLaunchOptions _options;
        private BenchmarkMovementDriver _movement;
        private DateTime _captureStartUtc;
        private double _captureStartRealtime;
        private int _gc0;
        private int _gc1;
        private int _gc2;
        private long _sentMessages;
        private long _sentBytes;
        private long _receivedMessages;
        private long _receivedBytes;
        private int _sourceRebuilds;
        private int _topologyRebuilds;
        private int _costRebuilds;
        private int _movementCommands;
        private int _movementWaypoints;
        private float _movementDistance;
        private MonsterSpatialIndexDiagnostics _spatialDiagnostics;
        private WeaponRuntimeService _weaponRuntime;
        private int _shotFiredCount;
        private int _projectileSpawnedCount;
        private long _activeProjectileCountSum;
        private int _activeProjectileSampleCount;
        private int _maximumActiveProjectileCount;
        private bool _focusInitialized;
        private bool _lastFocused;
        private int _focusedFrames;
        private int _unfocusedFrames;
        private int _focusChanges;
        private bool _capturing;
        private bool _profilerCapturing;

        public void Initialize(BenchmarkLaunchOptions options, BenchmarkMovementDriver movement)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _movement = movement ?? throw new ArgumentNullException(nameof(movement));
        }

        public void StartCapture()
        {
            if (_capturing)
                throw new InvalidOperationException("Benchmark 已经开始采样。");

            _frameMilliseconds.Clear();
            _mainThreadMilliseconds.Clear();
            _renderThreadMilliseconds.Clear();
            _gpuFrameMilliseconds.Clear();
            _gcAllocatedBytes.Clear();
            _frameRows.Clear();
            _markerCounters.Clear();
            _mainThread = StartCounter(ProfilerCategory.Internal, "Main Thread");
            _renderThread = StartCounter(ProfilerCategory.Render, "Render Thread");
            _gpuFrame = StartCounter(ProfilerCategory.Render, "GPU Frame Time");
            _gpuFrame.MaximumValidValue = 10_000_000_000L;
            _gcAllocated = StartCounter(ProfilerCategory.Memory, "GC Allocated In Frame");
            foreach (string markerName in CustomMarkerNames)
                _markerCounters.Add(StartCounter(ProfilerCategory.Scripts, markerName));

            _gc0 = GC.CollectionCount(0);
            _gc1 = GC.CollectionCount(1);
            _gc2 = GC.CollectionCount(2);
            NetworkTransportStats stats = GameNetworkRuntime.Gameplay?.Stats;
            _sentMessages = stats?.TotalMessageCount ?? 0L;
            _sentBytes = stats?.TotalPayloadBytes ?? 0L;
            _receivedMessages = stats?.TotalReceivedMessageCount ?? 0L;
            _receivedBytes = stats?.TotalReceivedPayloadBytes ?? 0L;
            MonsterRoomRuntime authority = MonsterRuntimeService.Instance?.AuthorityRoom;
            _sourceRebuilds = authority?.SourceRebuildCount ?? 0;
            _topologyRebuilds = authority?.TopologyRebuildCount ?? 0;
            _costRebuilds = authority?.CostRebuildCount ?? 0;
            _movementCommands = _movement.SubmittedCommandCount;
            _movementWaypoints = _movement.WaypointsVisited;
            _movementDistance = _movement.DistanceTravelled;
            _spatialDiagnostics = default;
            if (_options.Role == BenchmarkProcessRole.Host)
                authority.SpatialIndex.BeginPerformanceDiagnostics();
            _weaponRuntime = WeaponRuntimeService.Instance;
            _shotFiredCount = 0;
            _projectileSpawnedCount = 0;
            _activeProjectileCountSum = 0L;
            _activeProjectileSampleCount = 0;
            _maximumActiveProjectileCount = 0;
            _weaponRuntime.ShotFired += OnShotFired;
            _weaponRuntime.ProjectileSpawned += OnProjectileSpawned;
            _focusInitialized = false;
            _focusedFrames = 0;
            _unfocusedFrames = 0;
            _focusChanges = 0;
            _captureStartUtc = DateTime.UtcNow;
            _captureStartRealtime = Time.realtimeSinceStartupAsDouble;
            StartProfilerCapture();
            _capturing = true;
        }

        private void LateUpdate()
        {
            if (_options.Role == _options.FocusRole)
                BenchmarkProcessFocus.TryBringCurrentProcessToForeground();

            if (!_capturing)
                return;

            double frameMs = Time.unscaledDeltaTime * 1000d;
            bool focused = BenchmarkProcessFocus.IsCurrentProcessForeground;
            if (_focusInitialized && focused != _lastFocused)
                _focusChanges++;
            _focusInitialized = true;
            _lastFocused = focused;
            if (focused)
                _focusedFrames++;
            else
                _unfocusedFrames++;

            _frameMilliseconds.Add(frameMs);
            _mainThread.Accumulate();
            _renderThread.Accumulate();
            _gpuFrame.Accumulate();
            _gcAllocated.Accumulate();
            foreach (Counter marker in _markerCounters)
                marker.Accumulate();

            int activeProjectileCount = _weaponRuntime.ActiveProjectileCount;
            _activeProjectileCountSum += activeProjectileCount;
            _activeProjectileSampleCount++;
            _maximumActiveProjectileCount = Math.Max(_maximumActiveProjectileCount, activeProjectileCount);

            if (_mainThread.TryGetLatestValue(out long mainThreadValue))
                _mainThreadMilliseconds.Add(mainThreadValue / 1_000_000d);
            if (_renderThread.TryGetLatestValue(out long renderThreadValue))
                _renderThreadMilliseconds.Add(renderThreadValue / 1_000_000d);
            if (_gpuFrame.TryGetLatestValue(out long gpuFrameValue))
                _gpuFrameMilliseconds.Add(gpuFrameValue / 1_000_000d);
            if (_gcAllocated.TryGetLatestValue(out long gcAllocatedValue))
                _gcAllocatedBytes.Add(gcAllocatedValue);

            _frameRows.Add(string.Join(",",
                Time.frameCount.ToString(CultureInfo.InvariantCulture),
                Time.realtimeSinceStartupAsDouble.ToString("F6", CultureInfo.InvariantCulture),
                frameMs.ToString("F6", CultureInfo.InvariantCulture),
                ToMilliseconds(_mainThread).ToString("F6", CultureInfo.InvariantCulture),
                ToMilliseconds(_renderThread).ToString("F6", CultureInfo.InvariantCulture),
                ToMilliseconds(_gpuFrame).ToString("F6", CultureInfo.InvariantCulture),
                (gcAllocatedValue >= 0L ? gcAllocatedValue : -1L).ToString(CultureInfo.InvariantCulture),
                focused ? "true" : "false"));
        }

        public BenchmarkRoleMetrics StopAndWrite()
        {
            if (!_capturing)
                throw new InvalidOperationException("Benchmark 尚未开始采样。");

            _capturing = false;
            _weaponRuntime.ShotFired -= OnShotFired;
            _weaponRuntime.ProjectileSpawned -= OnProjectileSpawned;
            if (_options.Role == BenchmarkProcessRole.Host)
                _spatialDiagnostics = MonsterRuntimeService.Instance.AuthorityRoom.SpatialIndex.EndPerformanceDiagnostics();
            StopProfilerCapture();
            DateTime stoppedUtc = DateTime.UtcNow;
            double duration = Time.realtimeSinceStartupAsDouble - _captureStartRealtime;
            BenchmarkRoleMetrics metrics = BuildMetrics(stoppedUtc, duration);
            WriteOutputs(metrics);
            DisposeCounters();
            return metrics;
        }

        private void StartProfilerCapture()
        {
            if (!_options.CaptureProfiler || _options.Role != _options.FocusRole)
                return;

            Profiler.maxUsedMemory = 256 * 1024 * 1024;
            Profiler.logFile = Path.Combine(_options.RoleResultDirectory, "profiler");
            Profiler.enableBinaryLog = true;
            Profiler.enabled = true;
            _profilerCapturing = true;
        }

        private void StopProfilerCapture()
        {
            if (!_profilerCapturing)
                return;

            Profiler.enabled = false;
            Profiler.enableBinaryLog = false;
            Profiler.logFile = string.Empty;
            _profilerCapturing = false;
        }

        private BenchmarkRoleMetrics BuildMetrics(DateTime stoppedUtc, double duration)
        {
            double[] sorted = _frameMilliseconds.OrderBy(value => value).ToArray();
            double[] mainThreadSorted = _mainThreadMilliseconds.OrderBy(value => value).ToArray();
            double[] renderThreadSorted = _renderThreadMilliseconds.OrderBy(value => value).ToArray();
            double[] gpuFrameSorted = _gpuFrameMilliseconds.OrderBy(value => value).ToArray();
            long[] gcAllocatedSorted = _gcAllocatedBytes.OrderBy(value => value).ToArray();
            MonsterRuntimeService monsters = MonsterRuntimeService.Instance;
            MonsterReplica replica = monsters?.Replica;
            MonsterRoomRuntime authority = monsters?.AuthorityRoom;
            MonsterViewRuntimeService view = FindObjectOfType<MonsterViewRuntimeService>();
            NetworkTransportStats stats = GameNetworkRuntime.Gameplay?.Stats;
            int sourceRebuilds = (authority?.SourceRebuildCount ?? 0) - _sourceRebuilds;
            int movementCommands = _movement.SubmittedCommandCount - _movementCommands;
            int movementWaypoints = _movement.WaypointsVisited - _movementWaypoints;
            float movementDistance = _movement.DistanceTravelled - _movementDistance;

            bool movementValid = movementCommands > 0 && movementDistance >= 2f;
            bool monsterValid = _options.Role == BenchmarkProcessRole.Host
                ? monsters != null && monsters.AliveMonsterCount == _options.MonsterCount
                : replica != null && replica.HasFrame &&
                  replica.SlotCount == _options.MonsterCount &&
                  replica.ActiveCount == _options.MonsterCount &&
                  view != null && view.ActiveViewCount == _options.MonsterCount;
            bool flowValid = _options.Role != BenchmarkProcessRole.Host || sourceRebuilds > 0;
            bool spatialValid = _options.Role != BenchmarkProcessRole.Host ||
                                _spatialDiagnostics.QueryCount > 0 && _shotFiredCount > 0;
            bool environmentValid = Screen.width == 1024 &&
                                    Screen.height == 768 &&
                                    QualitySettings.names[QualitySettings.GetQualityLevel()] == "Performant" &&
                                    QualitySettings.vSyncCount == 0 &&
                                    Application.targetFrameRate == -1;
            bool durationValid = sorted.Length > 0 && duration >= _options.CaptureSeconds * 0.95d;
            int focusSampleCount = _focusedFrames + _unfocusedFrames;
            bool measuredRole = _options.Role == _options.FocusRole;
            bool focusValid = !measuredRole ||
                              focusSampleCount > 0 && (double)_focusedFrames / focusSampleCount >= 0.95d;
            string invalidReason = !environmentValid
                ? "分辨率、质量档、VSync 或目标帧率与固定基准配置不一致"
                : !durationValid
                    ? "有效采样帧数为空或采样时长不足"
                : !focusValid
                    ? "当前测量角色的 Windows 前台帧占比低于 95%"
                : !monsterValid
                ? "怪物权威/Replica/View 数量未达到预期"
                : !movementValid
                    ? "自动移动没有产生足够实际位移"
                    : !flowValid
                        ? "Host 采样期间没有发生 Source FlowField 重建"
                    : !spatialValid
                        ? "Host 采样期间没有产生射击或四叉树查询"
                        : string.Empty;

            return new BenchmarkRoleMetrics
            {
                runId = _options.RunId,
                role = _options.RoleName,
                focusTarget = _options.FocusRoleName,
                unityVersion = Application.unityVersion,
                platform = Application.platform.ToString(),
                graphicsDevice = SystemInfo.graphicsDeviceName,
                qualityLevel = QualitySettings.names[QualitySettings.GetQualityLevel()],
                width = Screen.width,
                height = Screen.height,
                vSyncCount = QualitySettings.vSyncCount,
                targetFrameRate = Application.targetFrameRate,
                monsterCount = _options.MonsterCount,
                sampleCount = sorted.Length,
                focusedFrameCount = _focusedFrames,
                unfocusedFrameCount = _unfocusedFrames,
                focusChangeCount = _focusChanges,
                actualDurationSeconds = duration,
                averageFrameMilliseconds = sorted.Length == 0 ? 0d : sorted.Average(),
                p50FrameMilliseconds = Percentile(sorted, 0.50d),
                p90FrameMilliseconds = Percentile(sorted, 0.90d),
                p95FrameMilliseconds = Percentile(sorted, 0.95d),
                p99FrameMilliseconds = Percentile(sorted, 0.99d),
                maximumFrameMilliseconds = sorted.Length == 0 ? 0d : sorted[^1],
                averageMainThreadMilliseconds = AverageMilliseconds(_mainThread),
                p95MainThreadMilliseconds = MetricPercentile(mainThreadSorted, 0.95d),
                p99MainThreadMilliseconds = MetricPercentile(mainThreadSorted, 0.99d),
                maximumMainThreadMilliseconds = MaximumMilliseconds(_mainThread),
                averageRenderThreadMilliseconds = AverageMilliseconds(_renderThread),
                p95RenderThreadMilliseconds = MetricPercentile(renderThreadSorted, 0.95d),
                p99RenderThreadMilliseconds = MetricPercentile(renderThreadSorted, 0.99d),
                maximumRenderThreadMilliseconds = MaximumMilliseconds(_renderThread),
                averageGpuFrameMilliseconds = AverageMilliseconds(_gpuFrame),
                p95GpuFrameMilliseconds = MetricPercentile(gpuFrameSorted, 0.95d),
                p99GpuFrameMilliseconds = MetricPercentile(gpuFrameSorted, 0.99d),
                maximumGpuFrameMilliseconds = MaximumMilliseconds(_gpuFrame),
                gcAllocatedBytes = _gcAllocated.Available ? _gcAllocated.Sum : -1L,
                averageGcAllocatedBytes = gcAllocatedSorted.Length > 0
                    ? gcAllocatedSorted.Average(value => (double)value)
                    : -1d,
                p95GcAllocatedBytes = gcAllocatedSorted.Length > 0
                    ? Percentile(gcAllocatedSorted.Select(value => (double)value).ToArray(), 0.95d)
                    : -1d,
                maximumGcAllocatedBytes = gcAllocatedSorted.Length > 0 ? gcAllocatedSorted[^1] : -1L,
                gcCollectionCount0 = GC.CollectionCount(0) - _gc0,
                gcCollectionCount1 = GC.CollectionCount(1) - _gc1,
                gcCollectionCount2 = GC.CollectionCount(2) - _gc2,
                monoUsedBytes = Profiler.GetMonoUsedSizeLong(),
                totalAllocatedMemoryBytes = Profiler.GetTotalAllocatedMemoryLong(),
                sentMessageCount = (stats?.TotalMessageCount ?? 0L) - _sentMessages,
                sentPayloadBytes = (stats?.TotalPayloadBytes ?? 0L) - _sentBytes,
                receivedMessageCount = (stats?.TotalReceivedMessageCount ?? 0L) - _receivedMessages,
                receivedPayloadBytes = (stats?.TotalReceivedPayloadBytes ?? 0L) - _receivedBytes,
                authorityAliveMonsterCount = monsters?.AliveMonsterCount ?? 0,
                replicaSlotCount = replica?.SlotCount ?? 0,
                replicaActiveCount = replica?.ActiveCount ?? 0,
                activeViewCount = view?.ActiveViewCount ?? 0,
                viewSlotCount = view?.ViewSlotCount ?? 0,
                sourceRebuildCount = sourceRebuilds,
                topologyRebuildCount = (authority?.TopologyRebuildCount ?? 0) - _topologyRebuilds,
                costRebuildCount = (authority?.CostRebuildCount ?? 0) - _costRebuilds,
                movementCommands = movementCommands,
                movementWaypoints = movementWaypoints,
                movementDistance = movementDistance,
                spatialBuildCount = _spatialDiagnostics.BuildCount,
                spatialQueryCount = _spatialDiagnostics.QueryCount,
                spatialCandidateCount = _spatialDiagnostics.TotalCandidateCount,
                spatialVisitedNodeCount = _spatialDiagnostics.TotalVisitedNodeCount,
                averageSpatialCandidateCount = Average(_spatialDiagnostics.TotalCandidateCount, _spatialDiagnostics.QueryCount),
                p50SpatialCandidateCount = _spatialDiagnostics.P50CandidateCount,
                p95SpatialCandidateCount = _spatialDiagnostics.P95CandidateCount,
                maximumSpatialCandidateCount = _spatialDiagnostics.MaximumCandidateCount,
                averageSpatialVisitedNodeCount = Average(_spatialDiagnostics.TotalVisitedNodeCount, _spatialDiagnostics.QueryCount),
                p50SpatialVisitedNodeCount = _spatialDiagnostics.P50VisitedNodeCount,
                p95SpatialVisitedNodeCount = _spatialDiagnostics.P95VisitedNodeCount,
                maximumSpatialVisitedNodeCount = _spatialDiagnostics.MaximumVisitedNodeCount,
                averageSpatialCandidateRatio = Ratio(
                    Average(_spatialDiagnostics.TotalCandidateCount, _spatialDiagnostics.QueryCount),
                    _options.MonsterCount),
                p95SpatialCandidateRatio = Ratio(_spatialDiagnostics.P95CandidateCount, _options.MonsterCount),
                shotFiredCount = _shotFiredCount,
                projectileSpawnedCount = _projectileSpawnedCount,
                averageActiveProjectileCount = Average(_activeProjectileCountSum, _activeProjectileSampleCount),
                maximumActiveProjectileCount = _maximumActiveProjectileCount,
                workloadValid = environmentValid && durationValid && focusValid && monsterValid && movementValid && flowValid && spatialValid,
                invalidReason = invalidReason,
                captureStartedUtc = _captureStartUtc.ToString("O"),
                captureStoppedUtc = stoppedUtc.ToString("O"),
            };
        }

        private void WriteOutputs(BenchmarkRoleMetrics metrics)
        {
            string directory = _options.RoleResultDirectory;
            Directory.CreateDirectory(directory);
            BenchmarkFileProtocol.WriteJsonAtomically(
                Path.Combine(directory, "metrics.json"),
                JsonUtility.ToJson(metrics, true));

            var frames = new StringBuilder("frame,realtime_seconds,frame_ms,main_thread_ms,render_thread_ms,gpu_frame_ms,gc_alloc_bytes,focused\n");
            foreach (string row in _frameRows)
                frames.AppendLine(row);
            File.WriteAllText(Path.Combine(directory, "frames.csv"), frames.ToString());

            var markers = new StringBuilder("marker,available,sample_count,non_zero_frames,total_ms,average_ms_per_sample,average_ms_per_frame,p95_frame_ms,max_frame_ms\n");
            foreach (Counter marker in _markerCounters)
            {
                markers.Append(EscapeCsv(marker.Name)).Append(',')
                    .Append(marker.Available ? "true" : "false").Append(',')
                    .Append(marker.SampleCount).Append(',')
                    .Append(marker.NonZeroFrames).Append(',')
                    .Append((marker.Sum / 1_000_000d).ToString("F6", CultureInfo.InvariantCulture)).Append(',')
                    .Append((marker.SampleCount > 0 ? marker.Sum / 1_000_000d / marker.SampleCount : 0d)
                        .ToString("F6", CultureInfo.InvariantCulture)).Append(',')
                    .Append((metrics.sampleCount > 0 ? marker.Sum / 1_000_000d / metrics.sampleCount : 0d)
                        .ToString("F6", CultureInfo.InvariantCulture)).Append(',')
                    .Append(MarkerPercentileMilliseconds(marker, 0.95d).ToString("F6", CultureInfo.InvariantCulture)).Append(',')
                    .Append((marker.Maximum / 1_000_000d).ToString("F6", CultureInfo.InvariantCulture))
                    .AppendLine();
            }
            File.WriteAllText(Path.Combine(directory, "markers.csv"), markers.ToString());

            var report = new StringBuilder();
            report.AppendLine($"# {(_options.Role == BenchmarkProcessRole.Host ? "Host" : "Client")} 性能基准详细报告");
            report.AppendLine();
            report.AppendLine($"- 有效：`{metrics.workloadValid}`");
            if (!metrics.workloadValid)
                report.AppendLine($"- 无效原因：{metrics.invalidReason}");
            report.AppendLine($"- 怪物数：{metrics.monsterCount}");
            report.AppendLine($"- 前台目标：{metrics.focusTarget}");
            report.AppendLine($"- 权威端存活 / Replica 槽位-激活 / View 槽位-激活：{metrics.authorityAliveMonsterCount} / {metrics.replicaSlotCount}-{metrics.replicaActiveCount} / {metrics.viewSlotCount}-{metrics.activeViewCount}");
            report.AppendLine($"- 样本数 / 实际时长：{metrics.sampleCount} / {metrics.actualDurationSeconds:F3}s");
            report.AppendLine($"- 前台/后台帧及切换次数：{metrics.focusedFrameCount}/{metrics.unfocusedFrameCount}，切换 {metrics.focusChangeCount} 次");
            report.AppendLine($"- 帧间隔 平均/P50/P90/P95/P99/最大(ms)：{metrics.averageFrameMilliseconds:F3} / {metrics.p50FrameMilliseconds:F3} / {metrics.p90FrameMilliseconds:F3} / {metrics.p95FrameMilliseconds:F3} / {metrics.p99FrameMilliseconds:F3} / {metrics.maximumFrameMilliseconds:F3}");
            report.AppendLine($"- Main Thread 平均/P95/P99/最大(ms)：{FormatOptional(metrics.averageMainThreadMilliseconds)} / {FormatOptional(metrics.p95MainThreadMilliseconds)} / {FormatOptional(metrics.p99MainThreadMilliseconds)} / {FormatOptional(metrics.maximumMainThreadMilliseconds)}");
            report.AppendLine($"- GPU Frame Time 平均/P95/P99/最大(ms)：{FormatOptional(metrics.averageGpuFrameMilliseconds)} / {FormatOptional(metrics.p95GpuFrameMilliseconds)} / {FormatOptional(metrics.p99GpuFrameMilliseconds)} / {FormatOptional(metrics.maximumGpuFrameMilliseconds)}");
            report.AppendLine($"- Render Thread：Unavailable（当前 Unity 2022.3 Player 轻量计数器不可用时不推算）");
            report.AppendLine($"- GC Alloc 总量 / 平均 / P95 / 最大(B)：{metrics.gcAllocatedBytes} / {metrics.averageGcAllocatedBytes:F0} / {metrics.p95GcAllocatedBytes:F0} / {metrics.maximumGcAllocatedBytes}");
            report.AppendLine($"- GC 次数 Gen0/1/2：{metrics.gcCollectionCount0}/{metrics.gcCollectionCount1}/{metrics.gcCollectionCount2}");
            report.AppendLine($"- Mono 已用 / 总分配内存：{metrics.monoUsedBytes} / {metrics.totalAllocatedMemoryBytes} B");
            report.AppendLine($"- 应用层网络发送/接收：{metrics.sentPayloadBytes}/{metrics.receivedPayloadBytes} B，{metrics.sentMessageCount}/{metrics.receivedMessageCount} 条消息");
            report.AppendLine($"- FlowField 重建 Topology/Source/Cost：{metrics.topologyRebuildCount} / {metrics.sourceRebuildCount} / {metrics.costRebuildCount}");
            report.AppendLine($"- 移动命令/实际距离/经过路点：{metrics.movementCommands} / {metrics.movementDistance:F2}m / {metrics.movementWaypoints}");
            report.AppendLine($"- 射击/生成弹丸/活动弹丸平均-最大：{metrics.shotFiredCount} / {metrics.projectileSpawnedCount} / {metrics.averageActiveProjectileCount:F2}-{metrics.maximumActiveProjectileCount}");
            if (_options.Role == BenchmarkProcessRole.Host)
            {
                report.AppendLine($"- 四叉树 Build/Query：{metrics.spatialBuildCount} / {metrics.spatialQueryCount} 次");
                report.AppendLine($"- Query 候选数 平均/P50/P95/最大：{metrics.averageSpatialCandidateCount:F2} / {metrics.p50SpatialCandidateCount} / {metrics.p95SpatialCandidateCount} / {metrics.maximumSpatialCandidateCount}（占怪物总数 {metrics.averageSpatialCandidateRatio:P2} / {metrics.p95SpatialCandidateRatio:P2}）");
                report.AppendLine($"- Query 访问节点 平均/P50/P95/最大：{metrics.averageSpatialVisitedNodeCount:F2} / {metrics.p50SpatialVisitedNodeCount} / {metrics.p95SpatialVisitedNodeCount} / {metrics.maximumSpatialVisitedNodeCount}");
            }
            report.AppendLine();
            report.AppendLine("## Marker 统计");
            report.AppendLine();
            foreach (Counter marker in _markerCounters
                         .Where(item => item.Available)
                         .OrderByDescending(item => item.Sum)
                         .Take(8))
            {
                report.AppendLine(
                    $"- `{marker.Name}`：总计 {marker.Sum / 1_000_000d:F3} ms，" +
                    $"调用样本 {marker.SampleCount}，" +
                    $"平均每次 {(marker.SampleCount > 0 ? marker.Sum / 1_000_000d / marker.SampleCount : 0d):F3} ms，" +
                    $"P95 单帧 {MarkerPercentileMilliseconds(marker, 0.95d):F3} ms，" +
                    $"最大单帧 {marker.Maximum / 1_000_000d:F3} ms，" +
                    $"非零帧 {marker.NonZeroFrames}");
            }
            report.AppendLine();
            report.AppendLine("## 基于证据的提示");
            report.AppendLine();
            if (!metrics.workloadValid)
                report.AppendLine($"- 工作负载验证失败：{metrics.invalidReason}。");
            if (metrics.p95FrameMilliseconds > 0d &&
                metrics.p99FrameMilliseconds >= metrics.p95FrameMilliseconds * 1.5d)
                report.AppendLine("- P99 至少比 P95 高 50%；应联查长帧逐帧记录与 Marker 峰值。");
            if (metrics.gcAllocatedBytes > 0L)
                report.AppendLine("- 采样期间存在托管分配；应把 GC Alloc 逐帧值与长帧对齐检查。");
            Counter highestMarker = _markerCounters
                .Where(item => item.Available)
                .OrderByDescending(item => item.Sum)
                .FirstOrDefault();
            if (highestMarker != null)
                report.AppendLine($"- 自定义 Marker 累计耗时最高的是 `{highestMarker.Name}`；这是排查线索，不等同于已证明根因。");
            report.AppendLine();
            report.AppendLine("不可用计数器在 CSV 中保存为 -1 或 false；最大 Payload、UTP 协议开销、RTT、丢包率和重传次数均不推算。");
            File.WriteAllText(Path.Combine(directory, "REPORT.md"), report.ToString());
        }

        private static Counter StartCounter(ProfilerCategory category, string name)
        {
            var counter = new Counter { Name = name };
            try
            {
                counter.Recorder = ProfilerRecorder.StartNew(
                    category,
                    name,
                    1,
                    ProfilerRecorderOptions.Default | ProfilerRecorderOptions.SumAllSamplesInFrame);
                counter.Available = counter.Recorder.Valid;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Benchmark] ProfilerRecorder unavailable: {name}: {exception.Message}");
            }
            return counter;
        }

        private static double ToMilliseconds(Counter counter) =>
            counter.TryGetLatestValue(out long value) ? value / 1_000_000d : -1d;

        private static double AverageMilliseconds(Counter counter) =>
            counter.Available && counter.ValidFrameCount > 0
                ? counter.Sum / 1_000_000d / counter.ValidFrameCount
                : -1d;

        private static double MaximumMilliseconds(Counter counter) =>
            counter.Available && counter.ValidFrameCount > 0
                ? counter.Maximum / 1_000_000d
                : -1d;

        private static double MarkerPercentileMilliseconds(Counter counter, double percentile)
        {
            if (!counter.Available || counter.FrameValues.Count == 0)
                return 0d;
            long[] sorted = counter.FrameValues.OrderBy(value => value).ToArray();
            int rank = Math.Max(1, (int)Math.Ceiling(percentile * sorted.Length));
            return sorted[Math.Min(rank - 1, sorted.Length - 1)] / 1_000_000d;
        }

        private static double Average(long total, int count) => count > 0 ? (double)total / count : 0d;
        private static double Ratio(double value, int total) => total > 0 ? value / total : 0d;

        private void OnShotFired(ShotContext _) => _shotFiredCount++;
        private void OnProjectileSpawned(ProjectileSpawn _) => _projectileSpawnedCount++;

        private static double MetricPercentile(double[] sorted, double percentile) =>
            sorted.Length > 0 ? Percentile(sorted, percentile) : -1d;

        private static string FormatOptional(double value) =>
            value >= 0d ? value.ToString("F3", CultureInfo.InvariantCulture) : "Unavailable";

        private static double Percentile(double[] sorted, double percentile)
        {
            if (sorted.Length == 0)
                return 0d;
            int rank = Math.Max(1, (int)Math.Ceiling(percentile * sorted.Length));
            return sorted[Math.Min(rank - 1, sorted.Length - 1)];
        }

        private static string EscapeCsv(string value) =>
            '"' + (value ?? string.Empty).Replace("\"", "\"\"") + '"';

        private void DisposeCounters()
        {
            _mainThread?.Dispose();
            _renderThread?.Dispose();
            _gpuFrame?.Dispose();
            _gcAllocated?.Dispose();
            foreach (Counter marker in _markerCounters)
                marker.Dispose();
        }

        private void OnDestroy()
        {
            if (_capturing)
                DisposeCounters();
            StopProfilerCapture();
        }
    }
}
