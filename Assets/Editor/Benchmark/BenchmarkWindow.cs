using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ProjectGame.Bootstrap;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ProjectGame.Editor.Benchmarking
{
    public sealed class BenchmarkWindow : EditorWindow
    {
        private const string LastExecutableKey = "ProjectGame.Benchmark.LastExecutable";
        private const string BuildManifestFileName = "benchmark-build.json";
        private int _seed = 12345;
        private int _repeatCount = 3;
        private float _warmupSeconds = 5f;
        private float _captureSeconds = 30f;
        private float _stageTimeoutSeconds = 180f;
        private int _gamePort = LanEndpoint.DefaultGamePort;

        [MenuItem("Tools/Benchmark/Dual Player Benchmark")]
        public static void Open()
        {
            BenchmarkWindow window = GetWindow<BenchmarkWindow>();
            window.titleContent = new GUIContent("Dual Benchmark");
            window.minSize = new Vector2(460f, 430f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Dual Player Performance Benchmark", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "固定用例：1024 / 2048 / 5012。每个用例重启一对 Host + Client，构建只执行一次。",
                MessageType.Info);

            using (new EditorGUI.DisabledScope(BenchmarkProcessOrchestrator.IsRunning))
            {
                _seed = EditorGUILayout.IntField("Random Seed", _seed);
                _repeatCount = EditorGUILayout.IntSlider("Repeat Count", _repeatCount, 1, 10);
                _warmupSeconds = EditorGUILayout.Slider("Warmup Seconds", _warmupSeconds, 0f, 60f);
                _captureSeconds = EditorGUILayout.Slider("Capture Seconds", _captureSeconds, 5f, 300f);
                _stageTimeoutSeconds = EditorGUILayout.Slider("Stage Timeout", _stageTimeoutSeconds, 30f, 600f);
                _gamePort = EditorGUILayout.IntField("Base Game Port", _gamePort);

                GUILayout.Space(8f);
                if (GUILayout.Button("Build & Run Dual Benchmark", GUILayout.Height(34f)))
                    BuildAndRun();
                if (GUILayout.Button("Run Existing Build", GUILayout.Height(30f)))
                    RunExistingBuild();
                if (GUILayout.Button("Select Baseline", GUILayout.Height(26f)))
                    SelectBaseline();
            }

            using (new EditorGUI.DisabledScope(!BenchmarkProcessOrchestrator.IsRunning))
            {
                if (GUILayout.Button("Stop Benchmark", GUILayout.Height(28f)))
                    BenchmarkProcessOrchestrator.Stop("用户从 EditorWindow 停止测试");
            }

            if (GUILayout.Button("Open Benchmark Results", GUILayout.Height(26f)))
                EditorUtility.RevealInFinder(BenchmarkPaths.ResultsRoot);

            GUILayout.Space(10f);
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(BenchmarkProcessOrchestrator.Status, MessageType.None);
            Repaint();
        }

        private void BuildAndRun()
        {
            try
            {
                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
                    throw new InvalidOperationException(
                        "首版 Benchmark 只支持 StandaloneWindows64；请先切换 Active Build Target。");

                HotUpdateBuilderTool.GenerateAllAndSyncDlls();
                AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings == null)
                    throw new InvalidOperationException("找不到 AddressableAssetSettings。");
                AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult contentResult);
                if (!string.IsNullOrWhiteSpace(contentResult.Error))
                    throw new InvalidOperationException($"Addressables 构建失败：{contentResult.Error}");

                string executable = Path.GetFullPath(AddressablesContentBuildTool.BuildBenchmarkPlayer());
                WriteBuildManifest(executable);
                EditorPrefs.SetString(LastExecutableKey, executable);
                StartRun(executable);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Benchmark] Build & Run 失败：{exception}");
                EditorUtility.DisplayDialog("Benchmark 构建失败", exception.Message, "确定");
            }
        }

        private void RunExistingBuild()
        {
            string initial = EditorPrefs.GetString(LastExecutableKey, string.Empty);
            string directory = string.IsNullOrEmpty(initial) ? string.Empty : Path.GetDirectoryName(initial);
            string executable = EditorUtility.OpenFilePanel("选择 Benchmark Player", directory, "exe");
            if (string.IsNullOrEmpty(executable))
                return;
            if (!TryValidateBuildManifest(executable, out string validationError))
            {
                EditorUtility.DisplayDialog("Benchmark Build 无效", validationError, "确定");
                return;
            }
            EditorPrefs.SetString(LastExecutableKey, executable);
            StartRun(executable);
        }

        private void StartRun(string executable)
        {
            if (_gamePort < 1 || _gamePort > ushort.MaxValue - 2)
                throw new ArgumentOutOfRangeException(nameof(_gamePort));
            BenchmarkProcessOrchestrator.Start(
                executable,
                new BenchmarkRunSettings
                {
                    Seed = _seed,
                    RepeatCount = _repeatCount,
                    WarmupSeconds = _warmupSeconds,
                    CaptureSeconds = _captureSeconds,
                    StageTimeoutSeconds = _stageTimeoutSeconds,
                    BaseGamePort = (ushort)_gamePort,
                });
        }

        private static void SelectBaseline()
        {
            string selected = EditorUtility.OpenFolderPanel(
                "选择 runs/<RunId> 目录作为基线",
                Path.Combine(BenchmarkPaths.ResultsRoot, "runs"),
                string.Empty);
            if (string.IsNullOrEmpty(selected))
                return;
            if (!File.Exists(Path.Combine(selected, "SUMMARY.md")))
            {
                EditorUtility.DisplayDialog("无效基线", "所选目录没有 SUMMARY.md。", "确定");
                return;
            }
            BenchmarkSummaryWriter.WriteBaseline(selected);
            Debug.Log($"[Benchmark] 已选择基线：{selected}");
        }

        private static void WriteBuildManifest(string executable)
        {
            var manifest = new BenchmarkBuildManifest
            {
                schemaVersion = 1,
                unityVersion = Application.unityVersion,
                target = EditorUserBuildSettings.activeBuildTarget.ToString(),
                executableName = Path.GetFileName(executable),
                executableSha256 = ComputeSha256(executable),
                contentCatalogHash = ReadLatestCatalogHash(),
                createdUtc = DateTime.UtcNow.ToString("O"),
            };
            BenchmarkFileProtocol.WriteJsonAtomically(
                Path.Combine(Path.GetDirectoryName(executable) ?? string.Empty, BuildManifestFileName),
                JsonUtility.ToJson(manifest, true));
        }

        private static bool TryValidateBuildManifest(string executable, out string error)
        {
            string manifestPath = Path.Combine(
                Path.GetDirectoryName(executable) ?? string.Empty,
                BuildManifestFileName);
            if (!File.Exists(manifestPath))
            {
                error = $"缺少 {BuildManifestFileName}；请先使用 Build & Run 生成可追溯构建。";
                return false;
            }

            try
            {
                BenchmarkBuildManifest manifest =
                    JsonUtility.FromJson<BenchmarkBuildManifest>(File.ReadAllText(manifestPath));
                if (manifest == null || manifest.schemaVersion != 1)
                {
                    error = "Benchmark build manifest 版本不兼容。";
                    return false;
                }
                if (manifest.executableName != Path.GetFileName(executable) ||
                    manifest.executableSha256 != ComputeSha256(executable))
                {
                    error = "Player 文件与 build manifest 指纹不一致。";
                    return false;
                }
                if (manifest.contentCatalogHash != ReadLatestCatalogHash())
                {
                    error = "当前 ServerData Catalog 与 Player 构建时不一致，请重新 Build & Run。";
                    return false;
                }

                error = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                error = "读取 Benchmark build manifest 失败：" + exception.Message;
                return false;
            }
        }

        private static string ComputeSha256(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }

        private static string ReadLatestCatalogHash()
        {
            if (!Directory.Exists(BenchmarkPaths.ContentRoot))
                return string.Empty;
            string path = Directory.GetFiles(
                    BenchmarkPaths.ContentRoot,
                    "catalog*.hash",
                    SearchOption.TopDirectoryOnly)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
            return path == null ? string.Empty : File.ReadAllText(path).Trim();
        }
    }

    [Serializable]
    internal sealed class BenchmarkBuildManifest
    {
        public int schemaVersion;
        public string unityVersion;
        public string target;
        public string executableName;
        public string executableSha256;
        public string contentCatalogHash;
        public string createdUtc;
    }

    [Serializable]
    internal sealed class BenchmarkRunSettings
    {
        public int Seed;
        public int RepeatCount;
        public float WarmupSeconds;
        public float CaptureSeconds;
        public float StageTimeoutSeconds;
        public ushort BaseGamePort;
    }

    internal static class BenchmarkPaths
    {
        public static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        public static string ResultsRoot => Path.Combine(ProjectRoot, "BenchmarkResults");
        public static string ContentRoot => Path.Combine(ProjectRoot, "ServerData", "StandaloneWindows64");
    }

    [InitializeOnLoad]
    internal static class BenchmarkProcessOrchestrator
    {
        private static readonly int[] Cases = BenchmarkLaunchOptions.DefaultMonsterCounts;
        private static Process _host;
        private static Process _client;
        private static DateTime _hostStartTime;
        private static DateTime _clientStartTime;
        private static BenchmarkRunSettings _settings;
        private static string _executable;
        private static string _runId;
        private static string _runDirectory;
        private static int _caseIndex;
        private static int _repeatIndex;
        private static bool _captureCommandWritten;
        private static DateTime _stageDeadlineUtc;

        public static bool IsRunning { get; private set; }
        public static string Status { get; private set; } = "Idle";

        static BenchmarkProcessOrchestrator()
        {
            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += () =>
            {
                if (IsRunning)
                    Stop("脚本域即将重载，已停止当前 Benchmark");
            };
            EditorApplication.quitting += () =>
            {
                if (IsRunning)
                    Stop("Unity Editor 正在退出");
            };
        }

        public static void Start(string executable, BenchmarkRunSettings settings)
        {
            if (IsRunning)
                throw new InvalidOperationException("已经有 Benchmark 正在运行。");
            if (!File.Exists(executable))
                throw new FileNotFoundException("找不到 Benchmark Player。", executable);
            if (!Directory.Exists(BenchmarkPaths.ContentRoot))
                throw new DirectoryNotFoundException(
                    $"找不到 Addressables 内容目录：{BenchmarkPaths.ContentRoot}");

            _executable = Path.GetFullPath(executable);
            _settings = settings;
            _runId = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) +
                     "-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            _runDirectory = Path.Combine(BenchmarkPaths.ResultsRoot, "runs", _runId);
            Directory.CreateDirectory(_runDirectory);
            _caseIndex = 0;
            _repeatIndex = 1;
            IsRunning = true;
            WriteRunMetadata();
            LaunchCurrentHost();
        }

        public static void Stop(string reason)
        {
            StopProcess(_host, _hostStartTime);
            StopProcess(_client, _clientStartTime);
            _host = null;
            _client = null;
            IsRunning = false;
            Status = "Stopped: " + reason;
            if (!string.IsNullOrEmpty(_runDirectory))
                BenchmarkSummaryWriter.WriteSummary(_runDirectory, Cases, _settings, false, reason);
        }

        private static void Update()
        {
            if (!IsRunning)
                return;

            try
            {
                if (DateTime.UtcNow >= _stageDeadlineUtc)
                    throw new TimeoutException("Editor 等待 Player 阶段超时。");

                ThrowIfFailed(_host, "Host");
                ThrowIfFailed(_client, "Client");

                BenchmarkProcessStatus hostStatus = ReadStatus("host");
                BenchmarkProcessStatus clientStatus = ReadStatus("client");
                if (hostStatus?.failed == true)
                    throw new InvalidOperationException("Host 报告失败：" + hostStatus.detail);
                if (clientStatus?.failed == true)
                    throw new InvalidOperationException("Client 报告失败：" + clientStatus.detail);

                if (_client == null && hostStatus?.stage == "host-ready")
                {
                    LaunchCurrentClient();
                    return;
                }

                if (!_captureCommandWritten &&
                    hostStatus?.stage == "workload-ready" &&
                    clientStatus?.stage == "workload-ready")
                {
                    WriteCaptureCommand();
                    _captureCommandWritten = true;
                    Status = $"Capturing {Cases[_caseIndex]} monsters, repeat {_repeatIndex}";
                    _stageDeadlineUtc = DateTime.UtcNow.AddSeconds(
                        _settings.CaptureSeconds + _settings.StageTimeoutSeconds);
                    return;
                }

                if (hostStatus?.completed == true &&
                    clientStatus?.completed == true &&
                    _host != null && _host.HasExited &&
                    _client != null && _client.HasExited)
                    AdvanceOrComplete();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Benchmark] Orchestrator failed: {exception}");
                Stop(exception.Message);
            }
        }

        private static void LaunchCurrentHost()
        {
            _captureCommandWritten = false;
            int monsterCount = Cases[_caseIndex];
            ushort port = checked((ushort)(_settings.BaseGamePort + _caseIndex));
            _host = Launch("host", monsterCount, port, out _hostStartTime);
            _client = null;
            Status = $"Waiting Host: {monsterCount} monsters, repeat {_repeatIndex}";
            _stageDeadlineUtc = DateTime.UtcNow.AddSeconds(_settings.StageTimeoutSeconds);
        }

        private static void LaunchCurrentClient()
        {
            int monsterCount = Cases[_caseIndex];
            ushort port = checked((ushort)(_settings.BaseGamePort + _caseIndex));
            _client = Launch("client", monsterCount, port, out _clientStartTime);
            Status = $"Waiting Client/workload: {monsterCount} monsters, repeat {_repeatIndex}";
            _stageDeadlineUtc = DateTime.UtcNow.AddSeconds(_settings.StageTimeoutSeconds);
        }

        private static Process Launch(
            string role,
            int monsterCount,
            ushort port,
            out DateTime startTime)
        {
            string roleDirectory = RoleDirectory(role);
            Directory.CreateDirectory(roleDirectory);
            string logPath = Path.Combine(roleDirectory, "player.log");
            string arguments = string.Join(" ", new[]
            {
                "-benchmark",
                $"-benchmark-role={role}",
                $"-benchmark-run-id={_runId}",
                $"-benchmark-monsters={monsterCount}",
                $"-benchmark-seed={_settings.Seed}",
                $"-benchmark-repeat={_repeatIndex}",
                $"-benchmark-warmup={Format(_settings.WarmupSeconds)}",
                $"-benchmark-duration={Format(_settings.CaptureSeconds)}",
                $"-benchmark-timeout={Format(_settings.StageTimeoutSeconds)}",
                "-benchmark-host=127.0.0.1",
                $"-benchmark-port={port}",
                $"-benchmark-result-root={Quote(BenchmarkPaths.ResultsRoot)}",
                $"-benchmark-content-root={Quote(BenchmarkPaths.ContentRoot)}",
                $"-logFile {Quote(logPath)}",
            });
            var info = new ProcessStartInfo
            {
                FileName = _executable,
                Arguments = arguments,
                WorkingDirectory = Path.GetDirectoryName(_executable) ?? BenchmarkPaths.ProjectRoot,
                UseShellExecute = false,
                CreateNoWindow = false,
            };
            Process process = Process.Start(info) ??
                              throw new InvalidOperationException($"无法启动 {role} Player。");
            startTime = process.StartTime.ToUniversalTime();
            return process;
        }

        private static void WriteCaptureCommand()
        {
            DateTime start = DateTime.UtcNow.AddSeconds(Math.Max(2f, _settings.WarmupSeconds));
            DateTime stop = start.AddSeconds(_settings.CaptureSeconds);
            var command = new BenchmarkCaptureCommand
            {
                runId = _runId,
                startUtcTicks = start.Ticks,
                stopUtcTicks = stop.Ticks,
            };
            string caseDirectory = Path.GetDirectoryName(RoleDirectory("host"));
            BenchmarkFileProtocol.WriteJsonAtomically(
                Path.Combine(caseDirectory ?? _runDirectory, BenchmarkFileProtocol.CaptureCommandFileName),
                JsonUtility.ToJson(command, true));
        }

        private static void AdvanceOrComplete()
        {
            _host?.Dispose();
            _client?.Dispose();
            _host = null;
            _client = null;

            _repeatIndex++;
            if (_repeatIndex <= _settings.RepeatCount)
            {
                LaunchCurrentHost();
                return;
            }

            _repeatIndex = 1;
            _caseIndex++;
            if (_caseIndex < Cases.Length)
            {
                LaunchCurrentHost();
                return;
            }

            IsRunning = false;
            Status = "Completed";
            BenchmarkSummaryWriter.WriteSummary(_runDirectory, Cases, _settings, true, string.Empty);
            BenchmarkSummaryWriter.WriteLatest(_runDirectory, _runId);
            AssetDatabase.Refresh();
            Debug.Log($"[Benchmark] 完成：{_runDirectory}");
        }

        private static BenchmarkProcessStatus ReadStatus(string role)
        {
            string path = Path.Combine(RoleDirectory(role), BenchmarkFileProtocol.StatusFileName);
            if (!File.Exists(path))
                return null;
            try
            {
                return JsonUtility.FromJson<BenchmarkProcessStatus>(File.ReadAllText(path));
            }
            catch (IOException)
            {
                return null;
            }
        }

        private static string RoleDirectory(string role) => Path.Combine(
            _runDirectory,
            $"monsters-{Cases[_caseIndex]}",
            $"repeat-{_repeatIndex:D2}",
            role);

        private static void ThrowIfFailed(Process process, string role)
        {
            if (process != null && process.HasExited && process.ExitCode != 0)
                throw new InvalidOperationException($"{role} Player 异常退出，ExitCode={process.ExitCode}");
        }

        private static void StopProcess(Process process, DateTime expectedStartTime)
        {
            if (process == null)
                return;
            try
            {
                if (!process.HasExited && process.StartTime.ToUniversalTime() == expectedStartTime)
                {
                    process.Kill();
                    process.WaitForExit(5000);
                }
            }
            catch (InvalidOperationException)
            {
                // 进程已经退出。
            }
            finally
            {
                process.Dispose();
            }
        }

        private static void WriteRunMetadata()
        {
            string gitCommit = ReadGit("rev-parse HEAD", out bool commitKnown);
            string gitStatus = ReadGit("status --porcelain", out bool statusKnown);
            var metadata = new BenchmarkRunMetadata
            {
                schemaVersion = 1,
                runId = _runId,
                createdUtc = DateTime.UtcNow.ToString("O"),
                unityVersion = Application.unityVersion,
                gitCommit = gitCommit,
                gitStatusKnown = commitKnown && statusKnown,
                gitDirty = statusKnown && !string.IsNullOrWhiteSpace(gitStatus),
                buildType = "Development",
                target = EditorUserBuildSettings.activeBuildTarget.ToString(),
                resolution = "1024x768",
                quality = "Performant",
                vSync = 0,
                targetFrameRate = -1,
                seed = _settings.Seed,
                repeatCount = _settings.RepeatCount,
                warmupSeconds = _settings.WarmupSeconds,
                captureSeconds = _settings.CaptureSeconds,
                monsterCounts = string.Join(",", Cases),
                cacheMode = "shared-warm-cache-host-first",
                routeVersion = "square-12m-v1",
            };
            BenchmarkFileProtocol.WriteJsonAtomically(
                Path.Combine(_runDirectory, "run-metadata.json"),
                JsonUtility.ToJson(metadata, true));
        }

        private static string ReadGit(string arguments, out bool succeeded)
        {
            succeeded = false;
            try
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    WorkingDirectory = BenchmarkPaths.ProjectRoot,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                });
                if (process == null)
                    return string.Empty;
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(3000);
                succeeded = process.ExitCode == 0;
                return succeeded ? output : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string Quote(string value) => '"' + value.Replace("\"", "\\\"") + '"';
        private static string Format(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    [Serializable]
    internal sealed class BenchmarkRunMetadata
    {
        public int schemaVersion;
        public string runId;
        public string createdUtc;
        public string unityVersion;
        public string gitCommit;
        public bool gitStatusKnown;
        public bool gitDirty;
        public string buildType;
        public string target;
        public string resolution;
        public string quality;
        public int vSync;
        public int targetFrameRate;
        public int seed;
        public int repeatCount;
        public float warmupSeconds;
        public float captureSeconds;
        public string monsterCounts;
        public string cacheMode;
        public string routeVersion;
    }

    [Serializable]
    internal sealed class BenchmarkMetricsDto
    {
        public bool workloadValid;
        public int sampleCount;
        public double actualDurationSeconds;
        public double averageFrameMilliseconds;
        public double p95FrameMilliseconds;
        public double p99FrameMilliseconds;
        public string captureStartedUtc;
        public string captureStoppedUtc;
        public long sentPayloadBytes;
        public long receivedPayloadBytes;
        public int authorityAliveMonsterCount;
        public int replicaActiveCount;
        public int activeViewCount;
        public int viewSlotCount;
        public int sourceRebuildCount;
        public float movementDistance;
    }

    [Serializable]
    internal sealed class BenchmarkBaselinePointer
    {
        public string runDirectory;
    }

    [Serializable]
    internal sealed class BenchmarkLatestPointer
    {
        public string runId;
        public string runDirectory;
    }

    internal static class BenchmarkSummaryWriter
    {
        public static void WriteBaseline(string runDirectory)
        {
            Directory.CreateDirectory(BenchmarkPaths.ResultsRoot);
            BenchmarkFileProtocol.WriteJsonAtomically(
                Path.Combine(BenchmarkPaths.ResultsRoot, "baseline.json"),
                JsonUtility.ToJson(new BenchmarkBaselinePointer { runDirectory = runDirectory }, true));
        }

        public static void WriteLatest(string runDirectory, string runId)
        {
            BenchmarkFileProtocol.WriteJsonAtomically(
                Path.Combine(BenchmarkPaths.ResultsRoot, "latest.json"),
                JsonUtility.ToJson(new BenchmarkLatestPointer
                {
                    runId = runId,
                    runDirectory = runDirectory,
                }, true));
        }

        public static void WriteSummary(
            string runDirectory,
            IReadOnlyList<int> cases,
            BenchmarkRunSettings settings,
            bool completed,
            string failure)
        {
            string baselineDirectory = ReadBaselineDirectory();
            string baselineNote = "Unavailable";
            if (!string.IsNullOrEmpty(baselineDirectory))
            {
                if (AreRunsCompatible(runDirectory, baselineDirectory, out string incompatibility))
                    baselineNote = baselineDirectory;
                else
                {
                    baselineNote = "Incompatible: " + incompatibility;
                    baselineDirectory = string.Empty;
                }
            }
            var summary = new StringBuilder();
            summary.AppendLine("# Dual Player Benchmark Summary");
            summary.AppendLine();
            summary.AppendLine($"- Completed: `{completed}`");
            if (!completed)
                summary.AppendLine($"- Failure/stop reason: {failure}");
            summary.AppendLine($"- Cases: {string.Join(", ", cases)}");
            summary.AppendLine($"- Repeats: {settings?.RepeatCount ?? 0}");
            summary.AppendLine($"- Baseline: {baselineNote}");
            summary.AppendLine();
            summary.AppendLine("| Monsters | Repeat | Role | Role valid | Pair aligned | Pair start/end skew | Samples | Avg ms | P95 ms | P99 ms | Baseline delta |");
            summary.AppendLine("|---:|---:|---|---|---|---:|---:|---:|---:|---:|---|");

            foreach (int monsterCount in cases)
            {
                for (int repeat = 1; repeat <= (settings?.RepeatCount ?? 0); repeat++)
                {
                    string repeatDirectory = Path.Combine(
                        runDirectory,
                        $"monsters-{monsterCount}",
                        $"repeat-{repeat:D2}");
                    BenchmarkMetricsDto hostMetrics = ReadMetrics(
                        Path.Combine(repeatDirectory, "host", "metrics.json"));
                    BenchmarkMetricsDto clientMetrics = ReadMetrics(
                        Path.Combine(repeatDirectory, "client", "metrics.json"));
                    double startSkewMilliseconds = CalculateStartSkewMilliseconds(
                        hostMetrics,
                        clientMetrics);
                    double stopSkewMilliseconds = CalculateStopSkewMilliseconds(
                        hostMetrics,
                        clientMetrics);
                    bool pairAligned = !double.IsNaN(startSkewMilliseconds) &&
                                       !double.IsNaN(stopSkewMilliseconds) &&
                                       startSkewMilliseconds <= 50d &&
                                       stopSkewMilliseconds <= 50d;

                    foreach (string role in new[] { "host", "client" })
                    {
                        string relative = Path.Combine(
                            $"monsters-{monsterCount}",
                            $"repeat-{repeat:D2}",
                            role,
                            "metrics.json");
                        BenchmarkMetricsDto metrics = role == "host" ? hostMetrics : clientMetrics;
                        BenchmarkMetricsDto baseline = string.IsNullOrEmpty(baselineDirectory)
                            ? null
                            : ReadMetrics(Path.Combine(baselineDirectory, relative));
                        if (metrics == null)
                        {
                            summary.AppendLine($"| {monsterCount} | {repeat} | {role} | missing | false | - | 0 | - | - | - | - |");
                            continue;
                        }

                        summary.AppendLine(
                            $"| {monsterCount} | {repeat} | {role} | {metrics.workloadValid} | {pairAligned} | " +
                            $"{FormatSkew(startSkewMilliseconds, stopSkewMilliseconds)} | " +
                            $"{metrics.sampleCount} | {metrics.averageFrameMilliseconds:F3} | " +
                            $"{metrics.p95FrameMilliseconds:F3} | {metrics.p99FrameMilliseconds:F3} | " +
                            $"{(pairAligned ? FormatDelta(metrics, baseline) : "N/A (capture skew)")} |");
                    }
                }
            }

            summary.AppendLine();
            summary.AppendLine("## Workload counters");
            summary.AppendLine();
            summary.AppendLine("| Monsters | Repeat | Role | Authority alive | Replica active | View slots/active | Source rebuilds | Movement m | Sent bytes | Received bytes |");
            summary.AppendLine("|---:|---:|---|---:|---:|---:|---:|---:|---:|---:|");
            foreach (int monsterCount in cases)
            {
                for (int repeat = 1; repeat <= (settings?.RepeatCount ?? 0); repeat++)
                {
                    foreach (string role in new[] { "host", "client" })
                    {
                        string path = Path.Combine(
                            runDirectory,
                            $"monsters-{monsterCount}",
                            $"repeat-{repeat:D2}",
                            role,
                            "metrics.json");
                        BenchmarkMetricsDto metrics = ReadMetrics(path);
                        if (metrics == null)
                            continue;
                        summary.AppendLine(
                            $"| {monsterCount} | {repeat} | {role} | {metrics.authorityAliveMonsterCount} | " +
                            $"{metrics.replicaActiveCount} | {metrics.viewSlotCount}/{metrics.activeViewCount} | " +
                            $"{metrics.sourceRebuildCount} | {metrics.movementDistance:F2} | " +
                            $"{metrics.sentPayloadBytes} | {metrics.receivedPayloadBytes} |");
                    }
                }
            }

            summary.AppendLine();
            summary.AppendLine("Host and Client frame times are reported separately and are never added together.");
            summary.AppendLine("Pair skew is shown as start/end; direct comparison requires both <= 50 ms.");
            summary.AppendLine("UTP protocol overhead, RTT, loss and retransmission remain unavailable unless a later runtime integration exposes them.");
            File.WriteAllText(Path.Combine(runDirectory, "SUMMARY.md"), summary.ToString());
        }

        private static string FormatDelta(BenchmarkMetricsDto current, BenchmarkMetricsDto baseline)
        {
            if (baseline == null || !baseline.workloadValid || !current.workloadValid)
                return "N/A";
            double avg = PercentChange(current.averageFrameMilliseconds, baseline.averageFrameMilliseconds);
            double p95 = PercentChange(current.p95FrameMilliseconds, baseline.p95FrameMilliseconds);
            double p99 = PercentChange(current.p99FrameMilliseconds, baseline.p99FrameMilliseconds);
            string warning = avg > 5d || p95 > 5d || p99 > 10d ? " REGRESSION" : string.Empty;
            return $"avg {avg:+0.0;-0.0;0.0}%, p95 {p95:+0.0;-0.0;0.0}%, p99 {p99:+0.0;-0.0;0.0}%{warning}";
        }

        private static double PercentChange(double current, double baseline) =>
            baseline <= 0d ? 0d : (current - baseline) / baseline * 100d;

        private static double CalculateStartSkewMilliseconds(
            BenchmarkMetricsDto host,
            BenchmarkMetricsDto client)
        {
            if (host == null || client == null ||
                !DateTime.TryParse(host.captureStartedUtc, CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out DateTime hostStart) ||
                !DateTime.TryParse(client.captureStartedUtc, CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out DateTime clientStart))
                return double.NaN;
            return Math.Abs((hostStart - clientStart).TotalMilliseconds);
        }

        private static double CalculateStopSkewMilliseconds(
            BenchmarkMetricsDto host,
            BenchmarkMetricsDto client)
        {
            if (host == null || client == null ||
                !DateTime.TryParse(host.captureStoppedUtc, CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out DateTime hostStop) ||
                !DateTime.TryParse(client.captureStoppedUtc, CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out DateTime clientStop))
                return double.NaN;
            return Math.Abs((hostStop - clientStop).TotalMilliseconds);
        }

        private static string FormatSkew(double startMilliseconds, double stopMilliseconds)
        {
            if (double.IsNaN(startMilliseconds) || double.IsNaN(stopMilliseconds))
                return "N/A";
            return startMilliseconds.ToString("F1", CultureInfo.InvariantCulture) + "/" +
                   stopMilliseconds.ToString("F1", CultureInfo.InvariantCulture) + " ms";
        }

        private static BenchmarkMetricsDto ReadMetrics(string path)
        {
            try
            {
                return File.Exists(path)
                    ? JsonUtility.FromJson<BenchmarkMetricsDto>(File.ReadAllText(path))
                    : null;
            }
            catch (IOException)
            {
                return null;
            }
        }

        private static string ReadBaselineDirectory()
        {
            string path = Path.Combine(BenchmarkPaths.ResultsRoot, "baseline.json");
            if (!File.Exists(path))
                return string.Empty;
            try
            {
                return JsonUtility.FromJson<BenchmarkBaselinePointer>(File.ReadAllText(path))?.runDirectory ?? string.Empty;
            }
            catch (IOException)
            {
                return string.Empty;
            }
        }

        private static bool AreRunsCompatible(
            string currentDirectory,
            string baselineDirectory,
            out string reason)
        {
            BenchmarkRunMetadata current = ReadMetadata(currentDirectory);
            BenchmarkRunMetadata baseline = ReadMetadata(baselineDirectory);
            if (current == null || baseline == null)
            {
                reason = "missing run-metadata.json";
                return false;
            }

            var mismatches = new List<string>();
            if (current.schemaVersion != baseline.schemaVersion) mismatches.Add("schema");
            if (current.buildType != baseline.buildType) mismatches.Add("buildType");
            if (current.target != baseline.target) mismatches.Add("target");
            if (current.resolution != baseline.resolution) mismatches.Add("resolution");
            if (current.quality != baseline.quality) mismatches.Add("quality");
            if (current.vSync != baseline.vSync) mismatches.Add("vSync");
            if (current.targetFrameRate != baseline.targetFrameRate) mismatches.Add("targetFrameRate");
            if (current.seed != baseline.seed) mismatches.Add("seed");
            if (current.captureSeconds != baseline.captureSeconds) mismatches.Add("captureSeconds");
            if (current.monsterCounts != baseline.monsterCounts) mismatches.Add("monsterCounts");
            if (current.routeVersion != baseline.routeVersion) mismatches.Add("routeVersion");
            reason = string.Join(", ", mismatches);
            return mismatches.Count == 0;
        }

        private static BenchmarkRunMetadata ReadMetadata(string runDirectory)
        {
            string path = Path.Combine(runDirectory, "run-metadata.json");
            try
            {
                return File.Exists(path)
                    ? JsonUtility.FromJson<BenchmarkRunMetadata>(File.ReadAllText(path))
                    : null;
            }
            catch (IOException)
            {
                return null;
            }
        }
    }
}
