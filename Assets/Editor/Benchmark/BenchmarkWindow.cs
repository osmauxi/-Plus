using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        private bool _captureProfiler;

        [MenuItem("Tools/性能基准/双端性能测试")]
        public static void Open()
        {
            BenchmarkWindow window = GetWindow<BenchmarkWindow>();
            window.titleContent = new GUIContent("双端性能基准");
            window.minSize = new Vector2(460f, 430f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("主机 + 客户端双端性能基准测试", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "固定用例：1024 / 2048 / 5012。每次重复会依次执行 Host 前台轮和 Client 前台轮，每轮各启动一对进程。",
                MessageType.Info);
            EditorGUILayout.HelpBox(
                "每轮只采信前台目标角色的性能数据，另一端仅提供网络与工作负载。目标角色前台帧占比低于 95% 时该轮无效；总运行时长约为原模式的两倍。",
                MessageType.Warning);

            using (new EditorGUI.DisabledScope(BenchmarkProcessOrchestrator.IsRunning))
            {
                _seed = EditorGUILayout.IntField(
                    new GUIContent("随机种子", "主机端和客户端使用相同种子，确保每轮压力场景可复现。"),
                    _seed);
                _repeatCount = EditorGUILayout.IntSlider(
                    new GUIContent("每档重复次数", "每个怪物数量分别执行的次数，用于降低单次采样波动。"),
                    _repeatCount,
                    1,
                    10);
                _warmupSeconds = EditorGUILayout.Slider(
                    new GUIContent("预热时长（秒）", "正式记录前等待的时间，用于避开场景初始化和缓存预热。"),
                    _warmupSeconds,
                    0f,
                    60f);
                _captureSeconds = EditorGUILayout.Slider(
                    new GUIContent("采样时长（秒）", "每轮正式记录帧时间与工作负载计数器的持续时间。"),
                    _captureSeconds,
                    5f,
                    300f);
                _stageTimeoutSeconds = EditorGUILayout.Slider(
                    new GUIContent("阶段超时（秒）", "主机端或客户端在单个启动阶段允许等待的最长时间。"),
                    _stageTimeoutSeconds,
                    30f,
                    600f);
                _gamePort = EditorGUILayout.IntField(
                    new GUIContent("起始游戏端口", "不同怪物档位会从该端口开始依次使用端口。"),
                    _gamePort);
                _captureProfiler = EditorGUILayout.Toggle(
                    new GUIContent(
                        "保存完整 Profiler 原始数据",
                        "仅在每轮被测前台角色的正式采样窗口内生成 profiler.raw；会产生额外开销，不能与普通轻量基线混用。"),
                    _captureProfiler);

                GUILayout.Space(8f);
                EditorGUILayout.HelpBox(
                    "首次运行请选择“构建并运行”。该操作会重新生成 HybridCLR、构建 Addressables 和 Development Player，然后自动启动双进程测试。",
                    MessageType.Warning);
                if (GUILayout.Button(
                        new GUIContent(
                            "构建并运行双端基准测试",
                            "重新生成 HybridCLR、构建 Addressables 和 Development Player，再依次执行 Host/Client 前台双轮测试。"),
                        GUILayout.Height(34f)))
                    BuildAndRun();
                if (GUILayout.Button(
                        new GUIContent(
                            "使用现有构建运行",
                            "选择已有 Player；仅当构建指纹与当前 Catalog 一致时才会启动测试。"),
                        GUILayout.Height(30f)))
                    RunExistingBuild();
                if (GUILayout.Button(
                        new GUIContent(
                            "选择历史基线",
                            "选择包含 SUMMARY.md 的历史 RunId 目录，用于后续兼容环境下的性能对比。"),
                        GUILayout.Height(26f)))
                    SelectBaseline();
            }

            using (new EditorGUI.DisabledScope(!BenchmarkProcessOrchestrator.IsRunning))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "停止当前基准测试",
                            "只终止本轮记录且启动时间匹配的 Host/Client 进程，并保留已经生成的结果。"),
                        GUILayout.Height(28f)))
                    BenchmarkProcessOrchestrator.Stop("用户从 EditorWindow 停止测试");
            }

            if (GUILayout.Button(
                    new GUIContent(
                        "打开测试结果目录",
                        "在文件管理器中打开 BenchmarkResults。"),
                    GUILayout.Height(26f)))
                EditorUtility.RevealInFinder(BenchmarkPaths.ResultsRoot);

            GUILayout.Space(10f);
            EditorGUILayout.LabelField("当前状态", EditorStyles.boldLabel);
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
                EditorUtility.DisplayDialog("性能基准构建失败", exception.Message, "确定");
            }
        }

        private void RunExistingBuild()
        {
            string initial = EditorPrefs.GetString(LastExecutableKey, string.Empty);
            string directory = string.IsNullOrEmpty(initial) ? string.Empty : Path.GetDirectoryName(initial);
            string executable = EditorUtility.OpenFilePanel("选择性能基准 Player", directory, "exe");
            if (string.IsNullOrEmpty(executable))
                return;
            if (!TryValidateBuildManifest(executable, out string validationError))
            {
                EditorUtility.DisplayDialog("性能基准构建无效", validationError, "确定");
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
                    CaptureProfiler = _captureProfiler,
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
                error = $"缺少 {BuildManifestFileName}；请先使用“构建并运行双端基准测试”生成可追溯构建。";
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
        public bool CaptureProfiler;
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
        private static readonly BenchmarkProcessRole[] FocusTargets =
        {
            BenchmarkProcessRole.Host,
            BenchmarkProcessRole.Client,
        };
        private const int SwRestore = 9;

        private delegate bool EnumWindowsCallback(IntPtr window, IntPtr parameter);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsCallback callback, IntPtr parameter);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr window);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr window);

        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr window);

        [DllImport("user32.dll")]
        private static extern void SwitchToThisWindow(IntPtr window, bool altTab);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BringWindowToTop(IntPtr window);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindowAsync(IntPtr window, int command);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AttachThreadInput(uint attachThread, uint attachToThread, bool attach);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

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
        private static int _focusTargetIndex;
        private static bool _captureCommandWritten;
        private static DateTime _nextFocusRefreshUtc;
        private static DateTime _stageDeadlineUtc;

        public static bool IsRunning { get; private set; }
        public static string Status { get; private set; } = "空闲";

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
            _focusTargetIndex = 0;
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
            Status = "已停止：" + reason;
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

                if (_captureCommandWritten)
                    EnsureTargetForeground();

                if (!_captureCommandWritten &&
                    hostStatus?.stage == "workload-ready" &&
                    clientStatus?.stage == "workload-ready")
                {
                    if (!TryFocusProcess(CurrentFocusProcess) || !IsProcessForeground(CurrentFocusProcess))
                    {
                        Status = $"正在切换到 {CurrentPassLabel}：{Cases[_caseIndex]} 个怪物，第 {_repeatIndex} 次";
                        return;
                    }

                    WriteCaptureCommand();
                    _captureCommandWritten = true;
                    Status = $"正在采样 {CurrentPassLabel}：{Cases[_caseIndex]} 个怪物，第 {_repeatIndex} 次";
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
            _nextFocusRefreshUtc = DateTime.MinValue;
            int monsterCount = Cases[_caseIndex];
            ushort port = checked((ushort)(_settings.BaseGamePort + _caseIndex));
            _host = Launch("host", monsterCount, port, out _hostStartTime);
            _client = null;
            Status = $"正在等待主机端（{CurrentPassLabel}）：{monsterCount} 个怪物，第 {_repeatIndex} 次";
            _stageDeadlineUtc = DateTime.UtcNow.AddSeconds(_settings.StageTimeoutSeconds);
        }

        private static void LaunchCurrentClient()
        {
            int monsterCount = Cases[_caseIndex];
            ushort port = checked((ushort)(_settings.BaseGamePort + _caseIndex));
            _client = Launch("client", monsterCount, port, out _clientStartTime);
            Status = $"正在等待客户端与压力场景（{CurrentPassLabel}）：{monsterCount} 个怪物，第 {_repeatIndex} 次";
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
                $"-benchmark-focus={CurrentFocusName}",
                $"-benchmark-run-id={_runId}",
                $"-benchmark-monsters={monsterCount}",
                $"-benchmark-seed={_settings.Seed}",
                $"-benchmark-repeat={_repeatIndex}",
                $"-benchmark-warmup={Format(_settings.WarmupSeconds)}",
                $"-benchmark-duration={Format(_settings.CaptureSeconds)}",
                $"-benchmark-timeout={Format(_settings.StageTimeoutSeconds)}",
                "-benchmark-host=127.0.0.1",
                $"-benchmark-port={port}",
                $"-benchmark-profiler={(_settings.CaptureProfiler ? 1 : 0)}",
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

            _focusTargetIndex++;
            if (_focusTargetIndex < FocusTargets.Length)
            {
                LaunchCurrentHost();
                return;
            }

            _focusTargetIndex = 0;
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
            Status = "已完成";
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
                return JsonUtility.FromJson<BenchmarkProcessStatus>(
                    BenchmarkFileProtocol.ReadAllTextShared(path));
            }
            catch (IOException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static string RoleDirectory(string role) => Path.Combine(
            _runDirectory,
            $"monsters-{Cases[_caseIndex]}",
            $"repeat-{_repeatIndex:D2}",
            $"focus-{CurrentFocusName}",
            role);

        private static BenchmarkProcessRole CurrentFocusTarget => FocusTargets[_focusTargetIndex];
        private static string CurrentFocusName =>
            CurrentFocusTarget == BenchmarkProcessRole.Host ? "host" : "client";
        private static string CurrentPassLabel =>
            CurrentFocusTarget == BenchmarkProcessRole.Host ? "Host 前台轮" : "Client 前台轮";
        private static Process CurrentFocusProcess =>
            CurrentFocusTarget == BenchmarkProcessRole.Host ? _host : _client;

        private static void EnsureTargetForeground()
        {
            Process process = CurrentFocusProcess;
            if (process == null || process.HasExited)
                return;

            DateTime now = DateTime.UtcNow;
            if (!IsProcessForeground(process) || now >= _nextFocusRefreshUtc)
            {
                TryFocusProcess(process);
                _nextFocusRefreshUtc = now.AddSeconds(1d);
            }
        }

        private static bool IsProcessForeground(Process process)
        {
            if (process == null || process.HasExited)
                return false;
            IntPtr foreground = GetForegroundWindow();
            if (foreground == IntPtr.Zero)
                return false;
            GetWindowThreadProcessId(foreground, out uint processId);
            return processId == process.Id;
        }

        private static bool TryFocusProcess(Process process)
        {
            if (process == null || process.HasExited)
                return false;

            IntPtr targetWindow = FindVisibleWindow(process.Id);
            if (targetWindow == IntPtr.Zero)
                return false;

            uint currentThread = GetCurrentThreadId();
            uint targetThread = GetWindowThreadProcessId(targetWindow, out _);
            IntPtr foregroundWindow = GetForegroundWindow();
            uint foregroundThread = foregroundWindow == IntPtr.Zero
                ? 0
                : GetWindowThreadProcessId(foregroundWindow, out _);
            bool attachedForeground = foregroundThread != 0 && foregroundThread != currentThread &&
                                      AttachThreadInput(currentThread, foregroundThread, true);
            bool attachedTarget = targetThread != 0 && targetThread != currentThread &&
                                  AttachThreadInput(currentThread, targetThread, true);
            try
            {
                ShowWindowAsync(targetWindow, SwRestore);
                BringWindowToTop(targetWindow);
                SwitchToThisWindow(targetWindow, true);
                SetForegroundWindow(targetWindow);
                SetFocus(targetWindow);
            }
            finally
            {
                if (attachedTarget)
                    AttachThreadInput(currentThread, targetThread, false);
                if (attachedForeground)
                    AttachThreadInput(currentThread, foregroundThread, false);
            }

            return IsProcessForeground(process);
        }

        private static IntPtr FindVisibleWindow(int processId)
        {
            IntPtr result = IntPtr.Zero;
            EnumWindows((window, _) =>
            {
                GetWindowThreadProcessId(window, out uint windowProcessId);
                if (windowProcessId != processId || !IsWindowVisible(window))
                    return true;
                result = window;
                return false;
            }, IntPtr.Zero);
            return result;
        }

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
            string buildManifestPath = Path.Combine(
                Path.GetDirectoryName(_executable) ?? string.Empty,
                "benchmark-build.json");
            BenchmarkBuildManifest buildManifest = JsonUtility.FromJson<BenchmarkBuildManifest>(
                File.ReadAllText(buildManifestPath));
            var metadata = new BenchmarkRunMetadata
            {
                schemaVersion = 3,
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
                executableSha256 = buildManifest.executableSha256,
                contentCatalogHash = buildManifest.contentCatalogHash,
                buildCreatedUtc = buildManifest.createdUtc,
                samplingMode = _settings.CaptureProfiler
                    ? "dual-pass-foreground-profiler-v1"
                    : "dual-pass-foreground-v1",
                captureProfiler = _settings.CaptureProfiler,
                frameTimeDefinition = "Time.unscaledDeltaTime（相邻渲染帧间隔）",
                cacheMode = "isolated-per-player",
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
        public string executableSha256;
        public string contentCatalogHash;
        public string buildCreatedUtc;
        public string samplingMode;
        public bool captureProfiler;
        public string frameTimeDefinition;
        public string cacheMode;
        public string routeVersion;
    }

    [Serializable]
    internal sealed class BenchmarkMetricsDto
    {
        public bool workloadValid;
        public string focusTarget;
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
        public string captureStartedUtc;
        public string captureStoppedUtc;
        public long sentMessageCount;
        public long sentPayloadBytes;
        public long receivedMessageCount;
        public long receivedPayloadBytes;
        public int authorityAliveMonsterCount;
        public int replicaSlotCount;
        public int replicaActiveCount;
        public int activeViewCount;
        public int viewSlotCount;
        public int topologyRebuildCount;
        public int sourceRebuildCount;
        public int costRebuildCount;
        public int movementCommands;
        public int movementWaypoints;
        public float movementDistance;
    }

    internal sealed class BenchmarkMarkerDto
    {
        public string Name;
        public long SampleCount;
        public int NonZeroFrames;
        public double TotalMilliseconds;
        public double MaximumFrameMilliseconds;
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
        private const double PairAlignmentToleranceMilliseconds = 100d;

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
            BenchmarkRunMetadata metadata = ReadMetadata(runDirectory);
            string baselineDirectory = ReadBaselineDirectory();
            BenchmarkRunMetadata baselineMetadata = null;
            string baselineCompatibility = "未选择基线";
            if (!string.IsNullOrEmpty(baselineDirectory))
            {
                baselineMetadata = ReadMetadata(baselineDirectory);
                if (AreRunsCompatible(runDirectory, baselineDirectory, out string incompatibility))
                    baselineCompatibility = "兼容，可计算同口径差值";
                else
                {
                    baselineCompatibility = "不兼容：" + incompatibility;
                    baselineDirectory = string.Empty;
                }
            }

            var summary = new StringBuilder();
            summary.AppendLine("# 双端性能基准汇总");
            summary.AppendLine();
            summary.AppendLine("## 1. 测试身份与有效性");
            summary.AppendLine();
            summary.AppendLine($"- RunId：`{metadata?.runId ?? Path.GetFileName(runDirectory)}`");
            summary.AppendLine($"- 测试完成：`{FormatBool(completed)}`");
            if (!completed)
                summary.AppendLine($"- 失败或停止原因：{failure}");
            summary.AppendLine($"- Git：`{FormatGit(metadata)}`");
            summary.AppendLine($"- 构建：`{metadata?.buildType ?? "不可用"}` / `{metadata?.target ?? "不可用"}` / Unity `{metadata?.unityVersion ?? "不可用"}`");
            summary.AppendLine($"- Player SHA-256：`{metadata?.executableSha256 ?? "不可用"}`");
            summary.AppendLine($"- Catalog 指纹：`{metadata?.contentCatalogHash ?? "不可用"}`");
            summary.AppendLine($"- 固定环境：{metadata?.resolution ?? "不可用"}，质量档 `{metadata?.quality ?? "不可用"}`，VSync `{metadata?.vSync.ToString(CultureInfo.InvariantCulture) ?? "不可用"}`，目标帧率 `{metadata?.targetFrameRate.ToString(CultureInfo.InvariantCulture) ?? "不可用"}`");
            summary.AppendLine($"- 压测配置：怪物 {string.Join(" / ", cases)}，每档 {settings?.RepeatCount ?? 0} 次，Seed `{metadata?.seed.ToString(CultureInfo.InvariantCulture) ?? "不可用"}`，预热 {settings?.WarmupSeconds ?? 0:F1}s，计划采样 {settings?.CaptureSeconds ?? 0:F1}s");
            summary.AppendLine($"- 采样模式：`{metadata?.samplingMode ?? "不可用"}`；完整 Profiler：`{FormatBool(metadata?.captureProfiler == true)}`；缓存：`{metadata?.cacheMode ?? "不可用"}`；路线：`{metadata?.routeVersion ?? "不可用"}`");
            summary.AppendLine($"- “帧时”统计口径：{metadata?.frameTimeDefinition ?? "不可用"}；Main Thread 与 GPU Frame Time 是独立维度，不能相加为总帧耗时。");
            summary.AppendLine($"- 双端采样对齐口径：开始/结束时间偏差均不超过 {PairAlignmentToleranceMilliseconds:F0} ms（30 秒窗口的 0.33%）。");
            if (metadata?.captureProfiler == true)
                summary.AppendLine("- 数据质量提示：完整 Profiler 模式的开头 1～2 帧包含二进制采集启动开销；最大帧需结合 `frames.csv` 中的位置以及 Main/GPU 时间判断，P95/P99 不受单个启动帧影响。");
            summary.AppendLine($"- 有效性能样本：{FormatValidRepeatCounts(runDirectory, cases, settings?.RepeatCount ?? 0)}");
            summary.AppendLine($"- 基线 RunId：`{baselineMetadata?.runId ?? "不可用"}`");
            summary.AppendLine($"- 基线兼容性：{baselineCompatibility}");
            summary.AppendLine();

            summary.AppendLine("## 2. Host / Client 性能概览");
            summary.AppendLine();
            summary.AppendLine("### 2.1 帧间隔");
            summary.AppendLine();
            summary.AppendLine("| 怪物数 | 重复 | 角色 | 双端有效 | 对齐与开始/结束偏差 | 实际时长(s) | 前台占比 | 样本数 | 平均(ms) | P50 | P90 | P95 | P99 | 最大 | 相对基线 |");
            summary.AppendLine("|---:|---:|---|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|");

            foreach (int monsterCount in cases)
            {
                for (int repeat = 1; repeat <= (settings?.RepeatCount ?? 0); repeat++)
                {
                    foreach (string focusTarget in new[] { "host", "client" })
                    {
                        string passDirectory = Path.Combine(
                            runDirectory,
                            $"monsters-{monsterCount}",
                            $"repeat-{repeat:D2}",
                            $"focus-{focusTarget}");
                        BenchmarkMetricsDto hostMetrics = ReadMetrics(
                            Path.Combine(passDirectory, "host", "metrics.json"));
                        BenchmarkMetricsDto clientMetrics = ReadMetrics(
                            Path.Combine(passDirectory, "client", "metrics.json"));
                        double startSkewMilliseconds = CalculateStartSkewMilliseconds(
                            hostMetrics,
                            clientMetrics);
                        double stopSkewMilliseconds = CalculateStopSkewMilliseconds(
                            hostMetrics,
                            clientMetrics);
                        bool pairAligned = !double.IsNaN(startSkewMilliseconds) &&
                                           !double.IsNaN(stopSkewMilliseconds) &&
                                           startSkewMilliseconds <= PairAlignmentToleranceMilliseconds &&
                                           stopSkewMilliseconds <= PairAlignmentToleranceMilliseconds;

                        string role = focusTarget;
                        BenchmarkMetricsDto metrics = role == "host" ? hostMetrics : clientMetrics;
                        BenchmarkMetricsDto companion = role == "host" ? clientMetrics : hostMetrics;
                        string relative = Path.Combine(
                            $"monsters-{monsterCount}",
                            $"repeat-{repeat:D2}",
                            $"focus-{focusTarget}",
                            role,
                            "metrics.json");
                        BenchmarkMetricsDto baseline = string.IsNullOrEmpty(baselineDirectory)
                            ? null
                            : ReadMetrics(Path.Combine(baselineDirectory, relative));
                        if (metrics == null)
                        {
                            summary.AppendLine($"| {monsterCount} | {repeat} | {FormatRole(role)} | 缺失 | 否 / 不可用 | - | - | 0 | - | - | - | - | - | - | - |");
                            continue;
                        }

                        bool pairValid = metrics.workloadValid && companion?.workloadValid == true && pairAligned;
                        string delta = pairAligned
                            ? FormatDelta(metrics, baseline)
                            : "不可用（采样偏差过大）";
                        summary.AppendLine(
                            $"| {monsterCount} | {repeat} | {FormatRole(role)} | {FormatBool(pairValid)} | " +
                            $"{FormatBool(pairAligned)} / {FormatSkew(startSkewMilliseconds, stopSkewMilliseconds)} | " +
                            $"{metrics.actualDurationSeconds:F3} | {FormatFocusedFrames(metrics)} | " +
                            $"{metrics.sampleCount} | {metrics.averageFrameMilliseconds:F3} | " +
                            $"{metrics.p50FrameMilliseconds:F3} | {metrics.p90FrameMilliseconds:F3} | " +
                            $"{metrics.p95FrameMilliseconds:F3} | {metrics.p99FrameMilliseconds:F3} | " +
                            $"{metrics.maximumFrameMilliseconds:F3} | {delta} |");
                    }
                }
            }

            summary.AppendLine();
            summary.AppendLine("### 2.2 Main Thread、GPU 与 GC");
            summary.AppendLine();
            summary.AppendLine("| 怪物数 | 重复 | 角色 | Main Thread 平均/P95/P99/最大(ms) | GPU 平均/P95/P99/最大(ms) | GC Alloc 平均/P95/最大(B) | GC 次数 Gen0/1/2 |");
            summary.AppendLine("|---:|---:|---|---|---|---|---|");
            foreach (int monsterCount in cases)
            {
                for (int repeat = 1; repeat <= (settings?.RepeatCount ?? 0); repeat++)
                {
                    foreach (string role in new[] { "host", "client" })
                    {
                        BenchmarkMetricsDto metrics = ReadMeasuredMetrics(runDirectory, monsterCount, repeat, role);
                        if (metrics == null)
                            continue;
                        summary.AppendLine(
                            $"| {monsterCount} | {repeat} | {FormatRole(role)} | " +
                            $"{FormatTiming(metrics.averageMainThreadMilliseconds, metrics.p95MainThreadMilliseconds, metrics.p99MainThreadMilliseconds, metrics.maximumMainThreadMilliseconds)} | " +
                            $"{FormatTiming(metrics.averageGpuFrameMilliseconds, metrics.p95GpuFrameMilliseconds, metrics.p99GpuFrameMilliseconds, metrics.maximumGpuFrameMilliseconds)} | " +
                            $"{FormatAllocation(metrics.averageGcAllocatedBytes, metrics.p95GcAllocatedBytes, metrics.maximumGcAllocatedBytes)} | " +
                            $"{metrics.gcCollectionCount0}/{metrics.gcCollectionCount1}/{metrics.gcCollectionCount2} |");
                    }
                }
            }
            summary.AppendLine();
            summary.AppendLine("Render Thread 在当前 Unity 2022.3 Player 轻量计数器中按 `Unavailable` 处理；完整调用关系请查看对应 `profiler.raw`。Main Thread、GPU 与帧间隔互为诊断维度，不能相加。");

            summary.AppendLine();
            summary.AppendLine("## 3. 工作负载与网络");
            summary.AppendLine();
            summary.AppendLine("### 3.1 工作负载一致性");
            summary.AppendLine();
            summary.AppendLine("| 怪物数 | 重复 | 前台轮 | Host 存活 | Client Replica | Client View 槽/激活 | Host 移动 m/命令/路点 | Client 移动 m/命令/路点 | Host 重建 Topology/Source/Cost（Hz） |");
            summary.AppendLine("|---:|---:|---|---:|---:|---:|---|---|---|");
            AppendWorkloadRows(summary, runDirectory, cases, settings?.RepeatCount ?? 0);
            summary.AppendLine();
            summary.AppendLine("移动距离表示采样窗口内角色实际走过的路径长度，不等同于路线完成率；路线版本、命令数和路点数用于判断各重复的负载是否相近。");
            summary.AppendLine();
            summary.AppendLine("### 3.2 应用层网络速率");
            summary.AppendLine();
            summary.AppendLine("| 怪物数 | 重复 | 角色 | 发送 KiB/s | 接收 KiB/s | 发送消息/s | 接收消息/s | 平均发送/接收 Payload(B) |");
            summary.AppendLine("|---:|---:|---|---:|---:|---:|---:|---|");
            AppendNetworkRows(summary, runDirectory, cases, settings?.RepeatCount ?? 0);
            summary.AppendLine();
            summary.AppendLine("最大 Payload、UTP 协议开销、RTT、丢包率和重传次数：`Unavailable`，不从累计字节数反推。");

            summary.AppendLine();
            summary.AppendLine("## 4. 关键热点与性能回归");
            summary.AppendLine();
            summary.AppendLine("以下 Top 5 来自轻量 Marker，按相同档位和角色的重复累计总耗时排序；完整调用树以 `profiler.raw` 为准。");
            summary.AppendLine();
            summary.AppendLine("| 怪物数 | 角色 | Marker | 总耗时(ms) | 调用样本数 | 平均每次(ms) | 平均每帧(ms) | 最大单帧(ms) |");
            summary.AppendLine("|---:|---|---|---:|---:|---:|---:|---:|");
            AppendHotspotRows(summary, runDirectory, cases, settings?.RepeatCount ?? 0);
            summary.AppendLine();
            summary.AppendLine($"基线判断：{baselineCompatibility}。不兼容或缺失时不对小幅变化作提升/退化结论。");

            summary.AppendLine();
            summary.AppendLine("## 5. 详细数据入口");
            summary.AppendLine();
            summary.AppendLine("- [运行元数据](run-metadata.json)");
            summary.AppendLine();
            summary.AppendLine("| 怪物数 | 重复 | 角色 | 详细报告 | 指标 | 逐帧 | Marker | 完整 Profiler |");
            summary.AppendLine("|---:|---:|---|---|---|---|---|---|");
            AppendEvidenceLinks(summary, runDirectory, cases, settings?.RepeatCount ?? 0, metadata?.captureProfiler == true);
            summary.AppendLine();
            summary.AppendLine($"每次重复会运行两对进程：Host 前台轮只采信 Host，Client 前台轮只采信 Client；另一端仅提供网络与工作负载。前台占比低于 95%，或双端采样开始/结束偏差任一超过 {PairAlignmentToleranceMilliseconds:F0} ms，均不作为有效性能样本。");
            File.WriteAllText(Path.Combine(runDirectory, "SUMMARY.md"), summary.ToString());
        }

        private static string FormatGit(BenchmarkRunMetadata metadata)
        {
            if (metadata == null || !metadata.gitStatusKnown)
                return "不可用";
            string commit = string.IsNullOrEmpty(metadata.gitCommit)
                ? "不可用"
                : metadata.gitCommit.Substring(0, Math.Min(12, metadata.gitCommit.Length));
            return commit + (metadata.gitDirty ? "（工作区有未提交修改）" : "（工作区干净）");
        }

        private static string FormatValidRepeatCounts(
            string runDirectory,
            IReadOnlyList<int> cases,
            int repeatCount)
        {
            var parts = new List<string>();
            foreach (int monsterCount in cases)
            {
                foreach (string role in new[] { "host", "client" })
                {
                    int valid = 0;
                    for (int repeat = 1; repeat <= repeatCount; repeat++)
                    {
                        string passDirectory = Path.Combine(
                            runDirectory,
                            $"monsters-{monsterCount}",
                            $"repeat-{repeat:D2}",
                            $"focus-{role}");
                        BenchmarkMetricsDto host = ReadMetrics(Path.Combine(passDirectory, "host", "metrics.json"));
                        BenchmarkMetricsDto client = ReadMetrics(Path.Combine(passDirectory, "client", "metrics.json"));
                        BenchmarkMetricsDto measured = role == "host" ? host : client;
                        if (measured?.workloadValid == true &&
                            host?.workloadValid == true &&
                            client?.workloadValid == true &&
                            CalculateStartSkewMilliseconds(host, client) <= PairAlignmentToleranceMilliseconds &&
                            CalculateStopSkewMilliseconds(host, client) <= PairAlignmentToleranceMilliseconds)
                            valid++;
                    }
                    parts.Add($"{monsterCount} {FormatRole(role)} {valid}/{repeatCount}");
                }
            }
            return string.Join("；", parts);
        }

        private static BenchmarkMetricsDto ReadMeasuredMetrics(
            string runDirectory,
            int monsterCount,
            int repeat,
            string role)
        {
            return ReadMetrics(Path.Combine(
                runDirectory,
                $"monsters-{monsterCount}",
                $"repeat-{repeat:D2}",
                $"focus-{role}",
                role,
                "metrics.json"));
        }

        private static string FormatTiming(double average, double p95, double p99, double maximum)
        {
            return average < 0d || p95 < 0d || p99 < 0d || maximum < 0d
                ? "Unavailable"
                : $"{average:F3}/{p95:F3}/{p99:F3}/{maximum:F3}";
        }

        private static string FormatAllocation(double average, double p95, long maximum)
        {
            return average < 0d || p95 < 0d || maximum < 0L
                ? "Unavailable"
                : $"{average:F0}/{p95:F0}/{maximum}";
        }

        private static void AppendWorkloadRows(
            StringBuilder summary,
            string runDirectory,
            IReadOnlyList<int> cases,
            int repeatCount)
        {
            foreach (int monsterCount in cases)
            {
                for (int repeat = 1; repeat <= repeatCount; repeat++)
                {
                    foreach (string focusTarget in new[] { "host", "client" })
                    {
                        string passDirectory = Path.Combine(
                            runDirectory,
                            $"monsters-{monsterCount}",
                            $"repeat-{repeat:D2}",
                            $"focus-{focusTarget}");
                        BenchmarkMetricsDto host = ReadMetrics(Path.Combine(passDirectory, "host", "metrics.json"));
                        BenchmarkMetricsDto client = ReadMetrics(Path.Combine(passDirectory, "client", "metrics.json"));
                        if (host == null || client == null)
                            continue;
                        summary.AppendLine(
                            $"| {monsterCount} | {repeat} | {FormatRole(focusTarget)} | {host.authorityAliveMonsterCount} | " +
                            $"{client.replicaSlotCount}/{client.replicaActiveCount} | {client.viewSlotCount}/{client.activeViewCount} | " +
                            $"{FormatMovement(host)} | {FormatMovement(client)} | {FormatRebuilds(host)} |");
                    }
                }
            }
        }

        private static string FormatMovement(BenchmarkMetricsDto metrics) =>
            $"{metrics.movementDistance:F2}/{metrics.movementCommands}/{metrics.movementWaypoints}";

        private static string FormatRebuilds(BenchmarkMetricsDto metrics)
        {
            double duration = metrics.actualDurationSeconds;
            return $"{metrics.topologyRebuildCount}/{metrics.sourceRebuildCount}/{metrics.costRebuildCount} " +
                   $"({Rate(metrics.topologyRebuildCount, duration):F2}/{Rate(metrics.sourceRebuildCount, duration):F2}/{Rate(metrics.costRebuildCount, duration):F2})";
        }

        private static void AppendNetworkRows(
            StringBuilder summary,
            string runDirectory,
            IReadOnlyList<int> cases,
            int repeatCount)
        {
            foreach (int monsterCount in cases)
            {
                for (int repeat = 1; repeat <= repeatCount; repeat++)
                {
                    foreach (string role in new[] { "host", "client" })
                    {
                        BenchmarkMetricsDto metrics = ReadMeasuredMetrics(runDirectory, monsterCount, repeat, role);
                        if (metrics == null)
                            continue;
                        double duration = metrics.actualDurationSeconds;
                        summary.AppendLine(
                            $"| {monsterCount} | {repeat} | {FormatRole(role)} | " +
                            $"{Rate(metrics.sentPayloadBytes, duration) / 1024d:F2} | " +
                            $"{Rate(metrics.receivedPayloadBytes, duration) / 1024d:F2} | " +
                            $"{Rate(metrics.sentMessageCount, duration):F2} | " +
                            $"{Rate(metrics.receivedMessageCount, duration):F2} | " +
                            $"{AveragePayload(metrics.sentPayloadBytes, metrics.sentMessageCount):F1}/" +
                            $"{AveragePayload(metrics.receivedPayloadBytes, metrics.receivedMessageCount):F1} |");
                    }
                }
            }
        }

        private static double Rate(long value, double duration) => duration > 0d ? value / duration : 0d;

        private static double AveragePayload(long bytes, long messages) =>
            messages > 0L ? (double)bytes / messages : 0d;

        private static void AppendHotspotRows(
            StringBuilder summary,
            string runDirectory,
            IReadOnlyList<int> cases,
            int repeatCount)
        {
            foreach (int monsterCount in cases)
            {
                foreach (string role in new[] { "host", "client" })
                {
                    var aggregate = new Dictionary<string, BenchmarkMarkerDto>(StringComparer.Ordinal);
                    int totalFrames = 0;
                    for (int repeat = 1; repeat <= repeatCount; repeat++)
                    {
                        BenchmarkMetricsDto metrics = ReadMeasuredMetrics(runDirectory, monsterCount, repeat, role);
                        if (metrics == null)
                            continue;
                        totalFrames += metrics.sampleCount;
                        string path = Path.Combine(
                            runDirectory,
                            $"monsters-{monsterCount}",
                            $"repeat-{repeat:D2}",
                            $"focus-{role}",
                            role,
                            "markers.csv");
                        foreach (BenchmarkMarkerDto marker in ReadMarkers(path))
                        {
                            if (!aggregate.TryGetValue(marker.Name, out BenchmarkMarkerDto current))
                            {
                                current = new BenchmarkMarkerDto { Name = marker.Name };
                                aggregate.Add(marker.Name, current);
                            }
                            current.SampleCount += marker.SampleCount;
                            current.NonZeroFrames += marker.NonZeroFrames;
                            current.TotalMilliseconds += marker.TotalMilliseconds;
                            current.MaximumFrameMilliseconds = Math.Max(
                                current.MaximumFrameMilliseconds,
                                marker.MaximumFrameMilliseconds);
                        }
                    }

                    foreach (BenchmarkMarkerDto marker in aggregate.Values
                                 .Where(item => item.TotalMilliseconds > 0d)
                                 .OrderByDescending(item => item.TotalMilliseconds)
                                 .Take(5))
                    {
                        summary.AppendLine(
                            $"| {monsterCount} | {FormatRole(role)} | `{marker.Name}` | " +
                            $"{marker.TotalMilliseconds:F3} | {marker.SampleCount} | " +
                            $"{(marker.SampleCount > 0 ? marker.TotalMilliseconds / marker.SampleCount : 0d):F3} | " +
                            $"{(totalFrames > 0 ? marker.TotalMilliseconds / totalFrames : 0d):F3} | " +
                            $"{marker.MaximumFrameMilliseconds:F3} |");
                    }
                }
            }
        }

        private static IEnumerable<BenchmarkMarkerDto> ReadMarkers(string path)
        {
            if (!File.Exists(path))
                yield break;
            foreach (string line in File.ReadLines(path).Skip(1))
            {
                int nameEnd = line.IndexOf("\",", 1, StringComparison.Ordinal);
                if (nameEnd < 0)
                    continue;
                string name = line.Substring(1, nameEnd - 1).Replace("\"\"", "\"");
                string[] values = line.Substring(nameEnd + 2).Split(',');
                if (values.Length < 7 || !bool.TryParse(values[0], out bool available) || !available)
                    continue;
                yield return new BenchmarkMarkerDto
                {
                    Name = name,
                    SampleCount = long.Parse(values[1], CultureInfo.InvariantCulture),
                    NonZeroFrames = int.Parse(values[2], CultureInfo.InvariantCulture),
                    TotalMilliseconds = double.Parse(values[3], CultureInfo.InvariantCulture),
                    MaximumFrameMilliseconds = double.Parse(values[6], CultureInfo.InvariantCulture),
                };
            }
        }

        private static void AppendEvidenceLinks(
            StringBuilder summary,
            string runDirectory,
            IReadOnlyList<int> cases,
            int repeatCount,
            bool captureProfiler)
        {
            foreach (int monsterCount in cases)
            {
                for (int repeat = 1; repeat <= repeatCount; repeat++)
                {
                    foreach (string role in new[] { "host", "client" })
                    {
                        string relativeDirectory = $"monsters-{monsterCount}/repeat-{repeat:D2}/focus-{role}/{role}";
                        string rawPath = Path.Combine(runDirectory, relativeDirectory.Replace('/', Path.DirectorySeparatorChar), "profiler.raw");
                        string profiler = captureProfiler && File.Exists(rawPath)
                            ? $"[profiler.raw]({relativeDirectory}/profiler.raw)"
                            : "Unavailable";
                        summary.AppendLine(
                            $"| {monsterCount} | {repeat} | {FormatRole(role)} | " +
                            $"[REPORT]({relativeDirectory}/REPORT.md) | " +
                            $"[metrics]({relativeDirectory}/metrics.json) | " +
                            $"[frames]({relativeDirectory}/frames.csv) | " +
                            $"[markers]({relativeDirectory}/markers.csv) | {profiler} |");
                    }
                }
            }
        }

        private static string FormatDelta(BenchmarkMetricsDto current, BenchmarkMetricsDto baseline)
        {
            if (baseline == null || !baseline.workloadValid || !current.workloadValid)
                return "不可用";
            if (!TryGetFocusedRatio(current, out double currentFocused))
                return "不可用（当前前台占比未知）";
            if (!TryGetFocusedRatio(baseline, out double baselineFocused))
                return "不可用（基线前台占比未知）";
            if (Math.Abs(currentFocused - baselineFocused) > 0.05d)
                return "不可用（前台条件不一致）";

            double avg = PercentChange(current.averageFrameMilliseconds, baseline.averageFrameMilliseconds);
            double p95 = PercentChange(current.p95FrameMilliseconds, baseline.p95FrameMilliseconds);
            double p99 = PercentChange(current.p99FrameMilliseconds, baseline.p99FrameMilliseconds);
            string warning = avg > 5d || p95 > 5d || p99 > 10d ? " 性能回退" : string.Empty;
            return $"平均 {avg:+0.0;-0.0;0.0}%，P95 {p95:+0.0;-0.0;0.0}%，P99 {p99:+0.0;-0.0;0.0}%{warning}";
        }

        private static string FormatRole(string role) => role == "host" ? "Host" : "Client";

        private static string FormatBool(bool value) => value ? "是" : "否";

        private static string FormatFocusedFrames(BenchmarkMetricsDto metrics)
        {
            return TryGetFocusedRatio(metrics, out double ratio) ? $"{ratio * 100d:F1}%" : "不可用";
        }

        private static bool TryGetFocusedRatio(BenchmarkMetricsDto metrics, out double ratio)
        {
            int total = metrics.focusedFrameCount + metrics.unfocusedFrameCount;
            ratio = total == 0 ? 0d : (double)metrics.focusedFrameCount / total;
            return total > 0;
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
                return "不可用";
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
            if (current == null)
            {
                reason = $"本轮缺少 {Path.Combine(currentDirectory, "run-metadata.json")}";
                return false;
            }
            if (baseline == null)
            {
                reason = $"基线缺少 {Path.Combine(baselineDirectory, "run-metadata.json")}";
                return false;
            }

            var mismatches = new List<string>();
            if (current.schemaVersion != baseline.schemaVersion) mismatches.Add("数据格式版本");
            if (current.buildType != baseline.buildType) mismatches.Add("构建类型");
            if (current.target != baseline.target) mismatches.Add("目标平台");
            if (current.resolution != baseline.resolution) mismatches.Add("分辨率");
            if (current.quality != baseline.quality) mismatches.Add("质量档");
            if (current.vSync != baseline.vSync) mismatches.Add("VSync");
            if (current.targetFrameRate != baseline.targetFrameRate) mismatches.Add("目标帧率");
            if (current.seed != baseline.seed) mismatches.Add("随机种子");
            if (current.captureSeconds != baseline.captureSeconds) mismatches.Add("采样时长");
            if (current.monsterCounts != baseline.monsterCounts) mismatches.Add("怪物档位");
            if (current.executableSha256 != baseline.executableSha256) mismatches.Add("Player 构建指纹");
            if (current.contentCatalogHash != baseline.contentCatalogHash) mismatches.Add("Catalog 指纹");
            if (current.samplingMode != baseline.samplingMode) mismatches.Add("采样模式");
            if (current.frameTimeDefinition != baseline.frameTimeDefinition) mismatches.Add("帧时统计口径");
            if (current.cacheMode != baseline.cacheMode) mismatches.Add("AssetBundle 缓存模式");
            if (current.routeVersion != baseline.routeVersion) mismatches.Add("路线版本");
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
