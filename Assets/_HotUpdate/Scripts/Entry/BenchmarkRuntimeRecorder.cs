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
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace ProjectGame.HotFix.Benchmarking
{
    [Serializable]
    internal sealed class BenchmarkRoleMetrics
    {
        public int schemaVersion = 1;
        public string runId;
        public string role;
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
        public double actualDurationSeconds;
        public double averageFrameMilliseconds;
        public double p50FrameMilliseconds;
        public double p90FrameMilliseconds;
        public double p95FrameMilliseconds;
        public double p99FrameMilliseconds;
        public double maximumFrameMilliseconds;
        public double averageMainThreadMilliseconds;
        public double maximumMainThreadMilliseconds;
        public double averageRenderThreadMilliseconds;
        public double maximumRenderThreadMilliseconds;
        public double averageGpuFrameMilliseconds;
        public double maximumGpuFrameMilliseconds;
        public long gcAllocatedBytes;
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
            "Monster.Density.Capture",
            "Monster.Density.Kernel",
            "Monster.Separation.Capture",
            "Monster.Separation.Kernel",
            "Monster.Presentation.Capture",
            "Monster.Presentation.Apply",
            "Monster.Presentation.Serialize",
            "Monster.Presentation.Decode",
            "Monster.View.ApplyAuthority",
        };

        private sealed class Counter
        {
            public string Name;
            public ProfilerRecorder Recorder;
            public bool Available;
            public long Sum;
            public long Maximum;
            public int NonZeroFrames;

            public void Accumulate()
            {
                if (!Available)
                    return;
                long value = Recorder.LastValue;
                Sum += value;
                Maximum = Math.Max(Maximum, value);
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
        private bool _capturing;

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
            _frameRows.Clear();
            _markerCounters.Clear();
            _mainThread = StartCounter(ProfilerCategory.Internal, "Main Thread");
            _renderThread = StartCounter(ProfilerCategory.Render, "Render Thread");
            _gpuFrame = StartCounter(ProfilerCategory.Render, "GPU Frame Time");
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
            _captureStartUtc = DateTime.UtcNow;
            _captureStartRealtime = Time.realtimeSinceStartupAsDouble;
            _capturing = true;
        }

        private void LateUpdate()
        {
            if (!_capturing)
                return;

            double frameMs = Time.unscaledDeltaTime * 1000d;
            _frameMilliseconds.Add(frameMs);
            _mainThread.Accumulate();
            _renderThread.Accumulate();
            _gpuFrame.Accumulate();
            _gcAllocated.Accumulate();
            foreach (Counter marker in _markerCounters)
                marker.Accumulate();

            _frameRows.Add(string.Join(",",
                Time.frameCount.ToString(CultureInfo.InvariantCulture),
                Time.realtimeSinceStartupAsDouble.ToString("F6", CultureInfo.InvariantCulture),
                frameMs.ToString("F6", CultureInfo.InvariantCulture),
                ToMilliseconds(_mainThread).ToString("F6", CultureInfo.InvariantCulture),
                ToMilliseconds(_renderThread).ToString("F6", CultureInfo.InvariantCulture),
                ToMilliseconds(_gpuFrame).ToString("F6", CultureInfo.InvariantCulture),
                (_gcAllocated.Available ? _gcAllocated.Recorder.LastValue : -1L)
                    .ToString(CultureInfo.InvariantCulture)));
        }

        public BenchmarkRoleMetrics StopAndWrite()
        {
            if (!_capturing)
                throw new InvalidOperationException("Benchmark 尚未开始采样。");

            _capturing = false;
            DateTime stoppedUtc = DateTime.UtcNow;
            double duration = Time.realtimeSinceStartupAsDouble - _captureStartRealtime;
            BenchmarkRoleMetrics metrics = BuildMetrics(stoppedUtc, duration);
            WriteOutputs(metrics);
            DisposeCounters();
            return metrics;
        }

        private BenchmarkRoleMetrics BuildMetrics(DateTime stoppedUtc, double duration)
        {
            double[] sorted = _frameMilliseconds.OrderBy(value => value).ToArray();
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
            bool environmentValid = Screen.width == 1024 &&
                                    Screen.height == 768 &&
                                    QualitySettings.names[QualitySettings.GetQualityLevel()] == "Performant" &&
                                    QualitySettings.vSyncCount == 0 &&
                                    Application.targetFrameRate == -1;
            bool durationValid = sorted.Length > 0 && duration >= _options.CaptureSeconds * 0.95d;
            string invalidReason = !environmentValid
                ? "分辨率、质量档、VSync 或目标帧率与固定基准配置不一致"
                : !durationValid
                    ? "有效采样帧数为空或采样时长不足"
                : !monsterValid
                ? "怪物权威/Replica/View 数量未达到预期"
                : !movementValid
                    ? "自动移动没有产生足够实际位移"
                    : !flowValid
                        ? "Host 采样期间没有发生 Source FlowField 重建"
                        : string.Empty;

            return new BenchmarkRoleMetrics
            {
                runId = _options.RunId,
                role = _options.RoleName,
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
                actualDurationSeconds = duration,
                averageFrameMilliseconds = sorted.Length == 0 ? 0d : sorted.Average(),
                p50FrameMilliseconds = Percentile(sorted, 0.50d),
                p90FrameMilliseconds = Percentile(sorted, 0.90d),
                p95FrameMilliseconds = Percentile(sorted, 0.95d),
                p99FrameMilliseconds = Percentile(sorted, 0.99d),
                maximumFrameMilliseconds = sorted.Length == 0 ? 0d : sorted[^1],
                averageMainThreadMilliseconds = AverageMilliseconds(_mainThread, sorted.Length),
                maximumMainThreadMilliseconds = MaximumMilliseconds(_mainThread),
                averageRenderThreadMilliseconds = AverageMilliseconds(_renderThread, sorted.Length),
                maximumRenderThreadMilliseconds = MaximumMilliseconds(_renderThread),
                averageGpuFrameMilliseconds = AverageMilliseconds(_gpuFrame, sorted.Length),
                maximumGpuFrameMilliseconds = MaximumMilliseconds(_gpuFrame),
                gcAllocatedBytes = _gcAllocated.Available ? _gcAllocated.Sum : -1L,
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
                workloadValid = environmentValid && durationValid && monsterValid && movementValid && flowValid,
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

            var frames = new StringBuilder("frame,realtime_seconds,frame_ms,main_thread_ms,render_thread_ms,gpu_frame_ms,gc_alloc_bytes\n");
            foreach (string row in _frameRows)
                frames.AppendLine(row);
            File.WriteAllText(Path.Combine(directory, "frames.csv"), frames.ToString());

            var markers = new StringBuilder("marker,available,non_zero_frames,total_ms,max_frame_ms\n");
            foreach (Counter marker in _markerCounters)
            {
                markers.Append(EscapeCsv(marker.Name)).Append(',')
                    .Append(marker.Available ? "true" : "false").Append(',')
                    .Append(marker.NonZeroFrames).Append(',')
                    .Append((marker.Sum / 1_000_000d).ToString("F6", CultureInfo.InvariantCulture)).Append(',')
                    .Append((marker.Maximum / 1_000_000d).ToString("F6", CultureInfo.InvariantCulture))
                    .AppendLine();
            }
            File.WriteAllText(Path.Combine(directory, "markers.csv"), markers.ToString());

            var report = new StringBuilder();
            report.AppendLine($"# {_options.RoleName.ToUpperInvariant()} Benchmark Report");
            report.AppendLine();
            report.AppendLine($"- Valid: `{metrics.workloadValid}`");
            if (!metrics.workloadValid)
                report.AppendLine($"- Invalid reason: {metrics.invalidReason}");
            report.AppendLine($"- Monsters: {metrics.monsterCount}");
            report.AppendLine($"- Authority alive / Replica slots-active / View slots-active: {metrics.authorityAliveMonsterCount} / {metrics.replicaSlotCount}-{metrics.replicaActiveCount} / {metrics.viewSlotCount}-{metrics.activeViewCount}");
            report.AppendLine($"- Samples / duration: {metrics.sampleCount} / {metrics.actualDurationSeconds:F3}s");
            report.AppendLine($"- Frame ms avg/P50/P90/P95/P99/max: {metrics.averageFrameMilliseconds:F3} / {metrics.p50FrameMilliseconds:F3} / {metrics.p90FrameMilliseconds:F3} / {metrics.p95FrameMilliseconds:F3} / {metrics.p99FrameMilliseconds:F3} / {metrics.maximumFrameMilliseconds:F3}");
            report.AppendLine($"- Main/Render/GPU average ms: {metrics.averageMainThreadMilliseconds:F3} / {metrics.averageRenderThreadMilliseconds:F3} / {metrics.averageGpuFrameMilliseconds:F3}");
            report.AppendLine($"- GC allocated bytes / collections gen0-gen1-gen2: {metrics.gcAllocatedBytes} / {metrics.gcCollectionCount0}-{metrics.gcCollectionCount1}-{metrics.gcCollectionCount2}");
            report.AppendLine($"- Mono used / total allocated memory: {metrics.monoUsedBytes} / {metrics.totalAllocatedMemoryBytes} bytes");
            report.AppendLine($"- Network application payload sent/received: {metrics.sentPayloadBytes} / {metrics.receivedPayloadBytes} bytes");
            report.AppendLine($"- FlowField rebuild topology/source/cost: {metrics.topologyRebuildCount} / {metrics.sourceRebuildCount} / {metrics.costRebuildCount}");
            report.AppendLine($"- Movement commands/distance/waypoints: {metrics.movementCommands} / {metrics.movementDistance:F2} / {metrics.movementWaypoints}");
            report.AppendLine();
            report.AppendLine("## Marker totals");
            report.AppendLine();
            foreach (Counter marker in _markerCounters
                         .Where(item => item.Available)
                         .OrderByDescending(item => item.Sum)
                         .Take(8))
            {
                report.AppendLine(
                    $"- `{marker.Name}`: total {marker.Sum / 1_000_000d:F3} ms, " +
                    $"max frame {marker.Maximum / 1_000_000d:F3} ms, " +
                    $"non-zero frames {marker.NonZeroFrames}");
            }
            report.AppendLine();
            report.AppendLine("## Evidence-based observations");
            report.AppendLine();
            if (!metrics.workloadValid)
                report.AppendLine($"- Fact: workload validation failed: {metrics.invalidReason}.");
            if (metrics.p95FrameMilliseconds > 0d &&
                metrics.p99FrameMilliseconds >= metrics.p95FrameMilliseconds * 1.5d)
                report.AppendLine("- Fact: P99 is at least 50% higher than P95; inspect long-frame rows and marker peaks.");
            if (metrics.gcAllocatedBytes > 0L)
                report.AppendLine("- Fact: managed allocations occurred during capture; correlate GC allocation rows with long frames.");
            Counter highestMarker = _markerCounters
                .Where(item => item.Available)
                .OrderByDescending(item => item.Sum)
                .FirstOrDefault();
            if (highestMarker != null)
                report.AppendLine($"- Fact: the largest recorded custom-marker total is `{highestMarker.Name}`; this is a lead, not a proven root cause.");
            report.AppendLine();
            report.AppendLine("Unavailable counters are stored as -1 or marked false in CSV; UTP headers, RTT, loss and retransmission are not inferred.");
            File.WriteAllText(Path.Combine(directory, "REPORT.md"), report.ToString());
        }

        private static Counter StartCounter(ProfilerCategory category, string name)
        {
            var counter = new Counter { Name = name };
            try
            {
                counter.Recorder = ProfilerRecorder.StartNew(category, name, 1);
                counter.Available = counter.Recorder.Valid;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Benchmark] ProfilerRecorder unavailable: {name}: {exception.Message}");
            }
            return counter;
        }

        private static double ToMilliseconds(Counter counter) =>
            counter.Available ? counter.Recorder.LastValue / 1_000_000d : -1d;

        private static double AverageMilliseconds(Counter counter, int sampleCount) =>
            counter.Available && sampleCount > 0
                ? counter.Sum / 1_000_000d / sampleCount
                : -1d;

        private static double MaximumMilliseconds(Counter counter) =>
            counter.Available ? counter.Maximum / 1_000_000d : -1d;

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
        }
    }
}
