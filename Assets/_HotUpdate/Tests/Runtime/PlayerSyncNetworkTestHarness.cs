using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Gameplay.Network;
using ProjectGame.HotFix.Gameplay.Player.Movement;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using Unity.Multiplayer.Tools.NetworkSimulator.Runtime;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Profiling;
using UnityEngine;

namespace ProjectGame.HotFix.Tests.Runtime
{
    /// <summary>
    /// PlayerLocomotionTest 场景的跨进程同步实测入口。
    /// 仅在存在测试配置文件或显式传入 --sync-test-config 时启动，不影响平时离线手感测试。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerSyncNetworkTestHarness : MonoBehaviour
    {
        private const string DefaultConfigRelativePath = "Temp/PlayerSyncNetworkTest/config.json";
        private const string PlayerPrefabResourcePath = "PlayerSyncNetworkTestPlayer";

        private readonly List<float> _frameTimesMs = new();
        private readonly List<long> _mainThreadTimesNs = new();
        private readonly List<long> _gcAllocatedBytes = new();
        private readonly List<ulong> _rttSamplesMs = new();

        private PlayerSyncNetworkTestConfig _config;
        private NetworkManager _networkManager;
        private UnityTransport _transport;
        private NetworkSimulator _networkSimulator;
        private GameNetworkRuntime _gameNetworkRuntime;
        private NetworkPrefabsList _runtimePrefabList;
        private GameObject _playerPrefab;
        private ProfilerRecorder _mainThreadRecorder;
        private ProfilerRecorder _gcRecorder;
        private string _role;
        private string _runToken;
        private string _hostReadyPath;
        private string _failure;
        private float _connectionDeadline;
        private float _measurementStartedAt = -1f;
        private bool _remoteClientConnected;
        private bool _playersSpawned;
        private bool _simulatorAvailableDuringMeasurement;
        private int _maxActorCount;
        private int _maxSpawnedNetworkObjectCount;
        private bool _finished;

        private void Start()
        {
            string configPath = ResolveConfigPath();
            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
            {
                enabled = false;
                return;
            }

            try
            {
                _config = JsonUtility.FromJson<PlayerSyncNetworkTestConfig>(File.ReadAllText(configPath));
                _role = ResolveArgument("--sync-test-role=") ?? (Application.isEditor ? "host" : "client");
                _runToken = File.GetLastWriteTimeUtc(configPath).Ticks.ToString();
                _hostReadyPath = Path.Combine(Path.GetDirectoryName(configPath) ?? string.Empty, "host-ready.flag");
                ValidateConfig();
                DisableOfflineTestDriver();
                StartCoroutine(RunTest());
            }
            catch (Exception exception)
            {
                _failure = $"测试启动失败：{exception}";
                StartCoroutine(FinishAndExit());
            }
        }

        private IEnumerator RunTest()
        {
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = _config.TargetFrameRate;

            _networkSimulator = FindObjectOfType<NetworkSimulator>();
            if (_networkSimulator == null)
            {
                _failure = "当前场景未找到 NetworkSimulator。";
                yield return FinishAndExit();
                yield break;
            }

            _playerPrefab = Resources.Load<GameObject>(PlayerPrefabResourcePath);
            if (_playerPrefab == null)
            {
                _failure = $"找不到 Resources/{PlayerPrefabResourcePath}.prefab。";
                yield return FinishAndExit();
                yield break;
            }

            CreateNetworkManager();
            ConfigureNetworkSimulator();

            bool started = string.Equals(_role, "host", StringComparison.OrdinalIgnoreCase)
                ? _networkManager.StartHost()
                : _networkManager.StartClient();
            if (!started)
            {
                _failure = $"NetworkManager.Start{_role} 返回 false。";
                yield return FinishAndExit();
                yield break;
            }

            _gameNetworkRuntime = gameObject.AddComponent<GameNetworkRuntime>();
            _gameNetworkRuntime.InitializeAsync(CancellationToken.None).Forget();

            if (string.Equals(_role, "host", StringComparison.OrdinalIgnoreCase))
                File.WriteAllText(_hostReadyPath, _runToken);

            _connectionDeadline = Time.realtimeSinceStartup + _config.ConnectionTimeoutSeconds;
            StartProfilers();

            while (!_finished)
            {
                if (_networkManager.IsServer && _remoteClientConnected && !_playersSpawned)
                {
                    // 等连接回调完成并进入下一帧后再生成，避免把动态对象插进
                    // NGO 正在处理的连接同步回调。
                    _playersSpawned = true;
                    SpawnPlayerForClient(NetworkManager.ServerClientId, new Vector3(-2f, 0f, 0f));
                    SpawnPlayerForClient(_networkManager.ConnectedClientsIds.First(id =>
                        id != NetworkManager.ServerClientId), new Vector3(2f, 0f, 0f));
                }

                SampleFrameMetrics();

                PlayerSyncNetworkTestActor[] actors = FindObjectsOfType<PlayerSyncNetworkTestActor>();
                _maxActorCount = Mathf.Max(_maxActorCount, actors.Length);
                if (_networkManager.SpawnManager != null)
                    _maxSpawnedNetworkObjectCount = Mathf.Max(
                        _maxSpawnedNetworkObjectCount,
                        _networkManager.SpawnManager.SpawnedObjects.Count);

                // 双网络 Actor 本身就是连接和动态 Spawn 都已完成的强证据，
                // 不再让计时依赖 NGO 连接回调与协程首帧之间的执行先后。
                if (actors.Length >= 2)
                    _remoteClientConnected = true;

                if (_remoteClientConnected && actors.Length >= 2 && _measurementStartedAt < 0f)
                    _measurementStartedAt = Time.realtimeSinceStartup;

                if (_measurementStartedAt >= 0f &&
                    Time.realtimeSinceStartup - _measurementStartedAt >= _config.DurationSeconds)
                {
                    yield return FinishAndExit();
                    yield break;
                }

                if (_measurementStartedAt < 0f && Time.realtimeSinceStartup >= _connectionDeadline)
                {
                    _failure = $"连接或双玩家生成超时（{_config.ConnectionTimeoutSeconds:F1}s）。";
                    yield return FinishAndExit();
                    yield break;
                }

                yield return null;
            }
        }

        private void CreateNetworkManager()
        {
            GameObject managerObject = new("PlayerSyncNetworkTest_NetworkManager");
            _networkManager = managerObject.AddComponent<NetworkManager>();
            _transport = managerObject.AddComponent<UnityTransport>();
            _transport.SetConnectionData("127.0.0.1", (ushort)_config.Port, "0.0.0.0");

            _runtimePrefabList = ScriptableObject.CreateInstance<NetworkPrefabsList>();
            _runtimePrefabList.Add(new NetworkPrefab { Prefab = _playerPrefab });

            NetworkConfig networkConfig = new()
            {
                NetworkTransport = _transport,
                TickRate = 30,
                EnableSceneManagement = false,
                ForceSamePrefabs = true,
                ConnectionApproval = false,
                ClientConnectionBufferTimeout = Mathf.CeilToInt(_config.ConnectionTimeoutSeconds),
                EnableNetworkLogs = true,
            };
            networkConfig.Prefabs.NetworkPrefabsLists.Add(_runtimePrefabList);
            _networkManager.NetworkConfig = networkConfig;

            _networkManager.OnClientConnectedCallback += HandleClientConnected;
            _networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        private void ConfigureNetworkSimulator()
        {
            bool applyImpairment = string.Equals(_role, "client", StringComparison.OrdinalIgnoreCase);
            _networkSimulator.ConnectionPreset = NetworkSimulatorPreset.Create(
                _config.Profile,
                "Automated player sync verification",
                applyImpairment ? _config.DelayMs : 0,
                applyImpairment ? _config.JitterMs : 0,
                0,
                applyImpairment ? _config.PacketLossPercent : 0);
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (clientId != NetworkManager.ServerClientId)
                _remoteClientConnected = true;

            if (_networkManager.IsClient && !_networkManager.IsServer &&
                clientId == _networkManager.LocalClientId)
                _remoteClientConnected = true;

        }

        private void HandleClientDisconnected(ulong clientId)
        {
            if (_finished)
                return;

            if (clientId == _networkManager.LocalClientId || clientId != NetworkManager.ServerClientId)
                _failure = $"测试期间连接断开：ClientId={clientId}。";
        }

        private void SpawnPlayerForClient(ulong clientId, Vector3 position)
        {
            GameObject instance = Instantiate(_playerPrefab, position, Quaternion.identity);
            instance.name = $"PlayerSyncNetworkTestPlayer_{clientId}";
            NetworkObject networkObject = instance.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(clientId, true);
            Debug.Log($"[PlayerSyncNetworkTest] Server spawned NetworkObjectId={networkObject.NetworkObjectId}, " +
                $"OwnerClientId={clientId}, SpawnWithObservers={networkObject.SpawnWithObservers}");
        }

        private void SampleFrameMetrics()
        {
            _simulatorAvailableDuringMeasurement |= _networkSimulator != null && _networkSimulator.IsAvailable;

            if (_measurementStartedAt < 0f)
                return;

            _frameTimesMs.Add(Time.unscaledDeltaTime * 1000f);

            if (_mainThreadRecorder.Valid && _mainThreadRecorder.Count > 0)
                _mainThreadTimesNs.Add(_mainThreadRecorder.LastValue);
            if (_gcRecorder.Valid && _gcRecorder.Count > 0)
                _gcAllocatedBytes.Add(_gcRecorder.LastValue);

            if (_networkManager != null && _networkManager.IsClient && _transport != null)
                _rttSamplesMs.Add(_transport.GetCurrentRtt(NetworkManager.ServerClientId));
        }

        private void StartProfilers()
        {
            _mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1);
            _gcRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1);
        }

        private IEnumerator FinishAndExit()
        {
            if (_finished)
                yield break;
            _finished = true;

            PlayerSyncNetworkTestResult result = BuildResult();
            string outputDirectory = Path.GetFullPath(_config?.OutputDirectory ?? "Temp/PlayerSyncNetworkTest");
            Directory.CreateDirectory(outputDirectory);
            string outputPath = Path.Combine(outputDirectory, $"{_config?.Profile ?? "startup"}-{_role ?? "unknown"}.json");
            File.WriteAllText(outputPath, JsonUtility.ToJson(result, true));
            Debug.Log($"[PlayerSyncNetworkTest] Result={outputPath}, Passed={result.Passed}");

            _mainThreadRecorder.Dispose();
            _gcRecorder.Dispose();

            // Host 先写结果但暂不立刻断开，让 Client 有时间完成相同采样窗口并落盘。
            if (string.Equals(_role, "host", StringComparison.OrdinalIgnoreCase))
                yield return new WaitForSecondsRealtime(5f);
            else if (Application.isEditor)
                yield return new WaitForSecondsRealtime(6f);
            else if (!Application.isEditor)
                yield return new WaitForSecondsRealtime(1.5f);

            if (_gameNetworkRuntime != null && _gameNetworkRuntime.IsInitialized)
                _gameNetworkRuntime.ShutdownAsync(CancellationToken.None).Forget();
            if (_networkManager != null && _networkManager.IsListening)
                _networkManager.Shutdown();

            if (string.Equals(_role, "host", StringComparison.OrdinalIgnoreCase) &&
                File.Exists(_hostReadyPath) && File.ReadAllText(_hostReadyPath) == _runToken)
                File.Delete(_hostReadyPath);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(result.Passed ? 0 : 2);
#endif
        }

        private PlayerSyncNetworkTestResult BuildResult()
        {
            PlayerSyncNetworkTestActor[] actors = FindObjectsOfType<PlayerSyncNetworkTestActor>();
            PlayerSyncNetworkTestActor owner = actors.FirstOrDefault(actor => actor.IsOwner);
            PlayerSyncNetworkTestActor observer = actors.FirstOrDefault(actor => !actor.IsOwner && !actor.IsServer);
            PlayerSyncNetworkTestActor remoteOwnedOnServer = actors.FirstOrDefault(actor =>
                actor.IsServer && actor.OwnerClientId != NetworkManager.ServerClientId);

            PlayerSyncController ownerSync = owner?.SyncController;
            PlayerSyncController observerSync = observer?.SyncController;
            PlayerSyncController serverSync = remoteOwnedOnServer?.SyncController;
            PlayerSyncTransport syncTransport = GameNetworkRuntime.PlayerSync;

            bool isClientRole = string.Equals(_role, "client", StringComparison.OrdinalIgnoreCase);
            bool coreEvidence = _remoteClientConnected && actors.Length >= 2 && _measurementStartedAt >= 0f;
            if (isClientRole)
            {
                coreEvidence &= ownerSync != null && ownerSync.InputPacketSendCount > 0 && ownerSync.SnapshotReceiveCount > 0;
                coreEvidence &= observerSync != null && observerSync.SnapshotReceiveCount > 0;
            }
            else
            {
                coreEvidence &= serverSync != null && serverSync.ServerAcceptedInputCount > 0 && serverSync.SnapshotSendCount > 0;
                int simulatedInputTicks = (serverSync?.ServerExactInputTickCount ?? 0) +
                    (serverSync?.ServerRetimedLateInputTickCount ?? 0) +
                    (serverSync?.ServerReusedInputTickCount ?? 0) +
                    (serverSync?.ServerNeutralInputTickCount ?? 0);
                coreEvidence &= simulatedInputTicks > 0 &&
                    (serverSync?.ServerNeutralInputTickCount ?? simulatedInputTicks) <=
                    Mathf.CeilToInt(simulatedInputTicks * 0.35f);
            }

            if (isClientRole)
                coreEvidence &= (ownerSync?.HardResyncCount ?? 1) == 0;

            return new PlayerSyncNetworkTestResult
            {
                Profile = _config?.Profile,
                Role = _role,
                Passed = string.IsNullOrEmpty(_failure) && coreEvidence,
                Failure = _failure ?? (coreEvidence ? string.Empty : "缺少输入、快照或双玩家链路证据。"),
                DurationSeconds = _measurementStartedAt < 0f ? 0f : Time.realtimeSinceStartup - _measurementStartedAt,
                ConfiguredDelayMs = isClientRole ? _config.DelayMs : 0,
                ConfiguredJitterMs = isClientRole ? _config.JitterMs : 0,
                ConfiguredPacketLossPercent = isClientRole ? _config.PacketLossPercent : 0,
                SimulatorAvailable = _simulatorAvailableDuringMeasurement ||
                    (_networkSimulator != null && _networkSimulator.IsAvailable),
                ActorCount = actors.Length,
                MaxActorCount = _maxActorCount,
                MaxSpawnedNetworkObjectCount = _maxSpawnedNetworkObjectCount,
                InputPacketsSent = ownerSync?.InputPacketSendCount ?? 0,
                SnapshotsReceivedByOwner = ownerSync?.SnapshotReceiveCount ?? 0,
                SnapshotsReceivedByObserver = observerSync?.SnapshotReceiveCount ?? 0,
                SnapshotsSentByServerPlayer = serverSync?.SnapshotSendCount ?? 0,
                RollbackCount = ownerSync?.RollbackCount ?? 0,
                HardResyncCount = ownerSync?.HardResyncCount ?? 0,
                LastReplayTickCount = ownerSync?.LastReplayTickCount ?? 0,
                LastPositionError = ownerSync?.LastPositionError ?? 0f,
                LastRotationError = ownerSync?.LastRotationError ?? 0f,
                LastVelocityError = ownerSync?.LastVelocityError ?? 0f,
                ServerAcceptedInputs = serverSync?.ServerAcceptedInputCount ?? 0,
                ServerDuplicateInputs = serverSync?.ServerDuplicateInputCount ?? 0,
                ServerOutdatedInputs = serverSync?.ServerOutdatedInputCount ?? 0,
                ServerInvalidInputs = serverSync?.ServerInvalidInputCount ?? 0,
                ServerInvalidFutureInputs = serverSync?.ServerInvalidFutureInputCount ?? 0,
                ServerExactInputTicks = serverSync?.ServerExactInputTickCount ?? 0,
                ServerRetimedLateInputsAccepted = serverSync?.ServerRetimedLateAcceptedInputCount ?? 0,
                ServerRetimedLateInputTicks = serverSync?.ServerRetimedLateInputTickCount ?? 0,
                ServerReusedInputTicks = serverSync?.ServerReusedInputTickCount ?? 0,
                ServerNeutralInputTicks = serverSync?.ServerNeutralInputTickCount ?? 0,
                InputPayloadBytesSent = syncTransport?.InputPayloadBytesSent ?? 0,
                SnapshotPayloadBytesSent = syncTransport?.SnapshotPayloadBytesSent ?? 0,
                DroppedDeltaWithoutBaseline = syncTransport?.DroppedDeltaWithoutBaselineCount ?? 0,
                MeanRttMs = Mean(_rttSamplesMs),
                P95RttMs = Percentile(_rttSamplesMs, 0.95f),
                MaxRttMs = Max(_rttSamplesMs),
                MeanFrameTimeMs = Mean(_frameTimesMs),
                P95FrameTimeMs = Percentile(_frameTimesMs, 0.95f),
                P99FrameTimeMs = Percentile(_frameTimesMs, 0.99f),
                MaxFrameTimeMs = Max(_frameTimesMs),
                MeanMainThreadTimeMs = Mean(_mainThreadTimesNs) / 1_000_000f,
                P95MainThreadTimeMs = Percentile(_mainThreadTimesNs, 0.95f) / 1_000_000f,
                MeanGcAllocatedBytesPerFrame = Mean(_gcAllocatedBytes),
                P95GcAllocatedBytesPerFrame = Percentile(_gcAllocatedBytes, 0.95f),
                RemotePresentationSamples = observer?.PresentationSampleCount ?? 0,
                RemotePresentationStallSamples = observer?.PresentationStallSampleCount ?? 0,
                RemotePresentationMeanStep = observer == null || observer.PresentationSampleCount == 0
                    ? 0f
                    : observer.PresentationStepSum / observer.PresentationSampleCount,
                RemotePresentationMaxStep = observer?.PresentationMaxStep ?? 0f,
            };
        }

        private void ValidateConfig()
        {
            if (_config == null)
                throw new InvalidDataException("测试配置为空。 ");
            if (_config.DurationSeconds <= 0f)
                throw new InvalidDataException("DurationSeconds 必须大于 0。 ");
            if (_config.Port <= 0 || _config.Port > ushort.MaxValue)
                throw new InvalidDataException("Port 超出范围。 ");
            if (_config.TargetFrameRate <= 0)
                _config.TargetFrameRate = 120;
            if (string.IsNullOrWhiteSpace(_config.OutputDirectory))
                _config.OutputDirectory = "Temp/PlayerSyncNetworkTest";
        }

        private static void DisableOfflineTestDriver()
        {
            foreach (PlayerLocomotionTestDriver driver in FindObjectsOfType<PlayerLocomotionTestDriver>())
                driver.enabled = false;
        }

        private static string ResolveConfigPath()
        {
            string explicitPath = ResolveArgument("--sync-test-config=");
            if (!string.IsNullOrWhiteSpace(explicitPath))
                return Path.GetFullPath(explicitPath.Trim('"'));

            if (!Application.isEditor)
                return null;

            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", DefaultConfigRelativePath));
        }

        private static string ResolveArgument(string prefix)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return arguments[i].Substring(prefix.Length);
            }
            return null;
        }

        private static float Mean(IReadOnlyList<float> values) => values.Count == 0 ? 0f : values.Average();
        private static float Max(IReadOnlyList<float> values) => values.Count == 0 ? 0f : values.Max();
        private static float Percentile(IReadOnlyList<float> values, float percentile)
        {
            if (values.Count == 0)
                return 0f;
            float[] sorted = values.OrderBy(value => value).ToArray();
            int index = Mathf.Clamp(Mathf.CeilToInt(sorted.Length * percentile) - 1, 0, sorted.Length - 1);
            return sorted[index];
        }
        private static float Mean(IReadOnlyList<long> values) => values.Count == 0 ? 0f : (float)values.Average();
        private static float Percentile(IReadOnlyList<long> values, float percentile)
        {
            if (values.Count == 0)
                return 0f;
            long[] sorted = values.OrderBy(value => value).ToArray();
            int index = Mathf.Clamp(Mathf.CeilToInt(sorted.Length * percentile) - 1, 0, sorted.Length - 1);
            return sorted[index];
        }
        private static float Mean(IReadOnlyList<ulong> values) => values.Count == 0 ? 0f : (float)values.Average(value => (double)value);
        private static float Max(IReadOnlyList<ulong> values) => values.Count == 0 ? 0f : values.Max();
        private static float Percentile(IReadOnlyList<ulong> values, float percentile)
        {
            if (values.Count == 0)
                return 0f;
            ulong[] sorted = values.OrderBy(value => value).ToArray();
            int index = Mathf.Clamp(Mathf.CeilToInt(sorted.Length * percentile) - 1, 0, sorted.Length - 1);
            return sorted[index];
        }
    }

    [Serializable]
    public sealed class PlayerSyncNetworkTestConfig
    {
        public string Profile = "Baseline";
        public float DurationSeconds = 15f;
        public float ConnectionTimeoutSeconds = 15f;
        public int DelayMs;
        public int JitterMs;
        public int PacketLossPercent;
        public int Port = 7979;
        public int TargetFrameRate = 120;
        public string OutputDirectory = "Temp/PlayerSyncNetworkTest";
    }

    [Serializable]
    public sealed class PlayerSyncNetworkTestResult
    {
        public string Profile;
        public string Role;
        public bool Passed;
        public string Failure;
        public float DurationSeconds;
        public int ConfiguredDelayMs;
        public int ConfiguredJitterMs;
        public int ConfiguredPacketLossPercent;
        public bool SimulatorAvailable;
        public int ActorCount;
        public int MaxActorCount;
        public int MaxSpawnedNetworkObjectCount;
        public int InputPacketsSent;
        public int SnapshotsReceivedByOwner;
        public int SnapshotsReceivedByObserver;
        public int SnapshotsSentByServerPlayer;
        public int RollbackCount;
        public int HardResyncCount;
        public int LastReplayTickCount;
        public float LastPositionError;
        public float LastRotationError;
        public float LastVelocityError;
        public int ServerAcceptedInputs;
        public int ServerDuplicateInputs;
        public int ServerOutdatedInputs;
        public int ServerInvalidInputs;
        public int ServerInvalidFutureInputs;
        public int ServerExactInputTicks;
        public int ServerRetimedLateInputsAccepted;
        public int ServerRetimedLateInputTicks;
        public int ServerReusedInputTicks;
        public int ServerNeutralInputTicks;
        public long InputPayloadBytesSent;
        public long SnapshotPayloadBytesSent;
        public int DroppedDeltaWithoutBaseline;
        public float MeanRttMs;
        public float P95RttMs;
        public float MaxRttMs;
        public float MeanFrameTimeMs;
        public float P95FrameTimeMs;
        public float P99FrameTimeMs;
        public float MaxFrameTimeMs;
        public float MeanMainThreadTimeMs;
        public float P95MainThreadTimeMs;
        public float MeanGcAllocatedBytesPerFrame;
        public float P95GcAllocatedBytesPerFrame;
        public int RemotePresentationSamples;
        public int RemotePresentationStallSamples;
        public float RemotePresentationMeanStep;
        public float RemotePresentationMaxStep;
    }
}
