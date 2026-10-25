using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.Bootstrap;
using ProjectGame.HotFix.Core.Network;
using ProjectGame.HotFix.Gameplay.Benchmarking;
using ProjectGame.HotFix.Gameplay.Monsters;
using ProjectGame.HotFix.Gameplay.Player;
using ProjectGame.HotFix.Gameplay.Runtime;
using ProjectGame.HotFix.Gameplay.State;
using ProjectGame.HotFix.Netcode;
using ProjectGame.HotFix.SceneFlow;
using ProjectGame.HotFix.UI.Lobby;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Benchmarking
{
    [DisallowMultipleComponent]
    public sealed class BenchmarkRuntimeCoordinator : MonoBehaviour
    {
        public static void StartIfEnabled()
        {
            BenchmarkLaunchOptions options = BenchmarkLaunchOptions.Current;
            if (!options.Enabled)
                return;
            if (FindObjectOfType<BenchmarkRuntimeCoordinator>() != null)
                return;

            var root = new GameObject("BenchmarkRuntimeCoordinator");
            DontDestroyOnLoad(root);
            root.AddComponent<BenchmarkRuntimeCoordinator>();
        }

        private void Start() => RunAsync(this.GetCancellationTokenOnDestroy()).Forget();

        private async UniTaskVoid RunAsync(CancellationToken cancellationToken)
        {
            BenchmarkLaunchOptions options = BenchmarkLaunchOptions.Current;
            try
            {
                MonsterNavigationStressTestManager.ConfigureBenchmark(
                    options.MonsterCount,
                    options.RandomSeed);

                await WaitUntilAsync(
                    () => LobbyUIManager.Instance != null &&
                          LobbyUIManager.Instance.OverviewCoordinator != null,
                    "Lobby UI 初始化",
                    options,
                    cancellationToken);

                if (options.Role == BenchmarkProcessRole.Host)
                {
                    await RunStageAsync(
                        token => LobbyConnectionService.StartHostAsync(
                            options.PlayerId,
                            options.HostAddress,
                            options.GamePort,
                            token),
                        "启动 Host",
                        options,
                        cancellationToken);
                    await RunStageAsync(
                        token => NetworkSessionBootstrap.Instance.WaitForLobbyReadyAsync(token),
                        "等待 Host Lobby Root",
                        options,
                        cancellationToken);
                    BenchmarkFileProtocol.WriteStatus(options, "host-ready", "Host 已监听且 Lobby Root Ready");
                }
                else
                {
                    await RunStageAsync(
                        token => LobbyConnectionService.StartClientAsync(
                            options.PlayerId,
                            options.HostAddress,
                            options.GamePort,
                            token),
                        "启动 Client",
                        options,
                        cancellationToken);
                    await WaitUntilAsync(
                        () => NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient,
                        "Client 连接 Host",
                        options,
                        cancellationToken);
                    BenchmarkFileProtocol.WriteStatus(options, "client-connected", "Client 已连接 Host");
                }

                await WaitUntilAsync(
                    () => LobbyNetworkManager.Instance != null &&
                          LobbyNetworkManager.Instance.IsSpawned &&
                          LobbyNetworkManager.Instance.LobbyPlayers.Count == 2,
                    "等待双人 Lobby 名单",
                    options,
                    cancellationToken);

                if (options.Role == BenchmarkProcessRole.Host)
                {
                    await WaitUntilAsync(
                        () => LobbyNetworkManager.Instance != null &&
                              LobbyNetworkManager.Instance.AllPlayerProfilesSubmitted,
                        "等待双端 Profile",
                        options,
                        cancellationToken);
                }

                ToggleLocalReadyIfNeeded();
                BenchmarkFileProtocol.WriteStatus(options, "lobby-ready", "本地玩家已提交 Ready");

                await WaitUntilAsync(
                    () => GameRuntimeBootstrap.Instance != null &&
                          GameRuntimeBootstrap.Instance.IsLocalRuntimeReady &&
                          GameRuntimeBootstrap.Instance.IsLocalPlayerRuntimeReady &&
                          GameStateController.Instance != null &&
                          GameStateController.Instance.IsPlaying,
                    "等待 GamePlaying 与本地 Player Runtime",
                    options,
                    cancellationToken);

                await WaitUntilAsync(
                    () => IsMonsterWorkloadReady(options),
                    "等待怪物权威/Replica/View 达到目标数量",
                    options,
                    cancellationToken);

                PlayerRuntime localPlayer = PlayerManager.Instance?.LocalPlayer;
                if (localPlayer == null)
                    throw new InvalidOperationException("找不到本地 PlayerRuntime。");
                BenchmarkMovementDriver movement =
                    localPlayer.GetComponent<BenchmarkMovementDriver>() ??
                    localPlayer.gameObject.AddComponent<BenchmarkMovementDriver>();
                movement.Configure(options.RandomSeed + (int)options.Role);

                if (options.Role == BenchmarkProcessRole.Host)
                {
                    int initialSourceRebuilds =
                        MonsterRuntimeService.Instance?.AuthorityRoom?.SourceRebuildCount ?? 0;
                    await WaitUntilAsync(
                        () => movement.DistanceTravelled >= 2f &&
                              (MonsterRuntimeService.Instance?.AuthorityRoom?.SourceRebuildCount ?? 0) > initialSourceRebuilds,
                        "验证移动触发 Source FlowField 重建",
                        options,
                        cancellationToken);
                }
                else
                {
                    await WaitUntilAsync(
                        () => movement.DistanceTravelled >= 2f,
                        "验证 Client 自动移动",
                        options,
                        cancellationToken);
                }

                if (options.Role == options.FocusRole)
                {
                    await WaitUntilAsync(
                        BenchmarkProcessFocus.TryBringCurrentProcessToForeground,
                        "切换目标 Player 到 Windows 前台",
                        options,
                        cancellationToken);
                }

                BenchmarkFileProtocol.WriteStatus(options, "workload-ready", "工作负载有效，等待统一采样命令");
                BenchmarkCaptureCommand command = await WaitForCaptureCommandAsync(options, cancellationToken);
                await WaitUntilUtcAsync(command.startUtcTicks, cancellationToken);

                BenchmarkRuntimeRecorder recorder = gameObject.AddComponent<BenchmarkRuntimeRecorder>();
                recorder.Initialize(options, movement);
                recorder.StartCapture();
                BenchmarkFileProtocol.WriteStatus(options, "capturing", "已开始正式采样");

                await WaitUntilUtcAsync(command.stopUtcTicks, cancellationToken);
                BenchmarkRoleMetrics metrics = recorder.StopAndWrite();
                if (!metrics.workloadValid)
                    throw new InvalidOperationException($"采样完成但有效性检查失败：{metrics.invalidReason}");

                BenchmarkFileProtocol.WriteStatus(
                    options,
                    "completed",
                    "报告已写入",
                    completed: true);
                Application.Quit(0);
            }
            catch (OperationCanceledException)
            {
                BenchmarkFileProtocol.WriteStatus(options, "cancelled", "Benchmark 被取消", failed: true);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Benchmark] 运行失败：{exception}");
                BenchmarkFileProtocol.WriteStatus(
                    options,
                    "failed",
                    exception.ToString(),
                    failed: true);
                Application.Quit(2);
            }
        }

        private static void ToggleLocalReadyIfNeeded()
        {
            LobbyNetworkManager lobby = LobbyNetworkManager.Instance;
            ulong localClientId = NetworkManager.Singleton.LocalClientId;
            foreach (LobbyPlayerState player in lobby.LobbyPlayers)
            {
                if (player.ClientId != localClientId)
                    continue;
                if (!player.IsReady)
                    lobby.ToggleReadyServerRpc();
                return;
            }
            throw new InvalidOperationException("Lobby 名单中找不到本地玩家。");
        }

        private static bool IsMonsterWorkloadReady(BenchmarkLaunchOptions options)
        {
            MonsterRuntimeService monsters = MonsterRuntimeService.Instance;
            if (monsters == null || !monsters.IsInitialized || !monsters.IsRoomActive)
                return false;

            if (options.Role == BenchmarkProcessRole.Host)
                return monsters.AliveMonsterCount == options.MonsterCount &&
                       monsters.MonsterSlotCount == options.MonsterCount;

            MonsterReplica replica = monsters.Replica;
            if (replica == null || !replica.HasFrame ||
                replica.SlotCount != options.MonsterCount ||
                replica.ActiveCount != options.MonsterCount)
                return false;

            MonsterViewRuntimeService view = FindObjectOfType<MonsterViewRuntimeService>();
            return view != null &&
                   view.ViewSlotCount == options.MonsterCount &&
                   view.ActiveViewCount == options.MonsterCount;
        }

        private static async UniTask WaitUntilAsync(
            Func<bool> predicate,
            string description,
            BenchmarkLaunchOptions options,
            CancellationToken cancellationToken)
        {
            double deadline = Time.realtimeSinceStartupAsDouble + options.StageTimeoutSeconds;
            while (!predicate())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (Time.realtimeSinceStartupAsDouble >= deadline)
                    throw new TimeoutException($"Benchmark 阶段超时：{description}");
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        private static async UniTask RunStageAsync(
            Func<CancellationToken, UniTask> action,
            string description,
            BenchmarkLaunchOptions options,
            CancellationToken cancellationToken)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(options.StageTimeoutSeconds));
            try
            {
                await action(timeout.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException($"Benchmark 阶段超时：{description}");
            }
        }

        private static async UniTask<BenchmarkCaptureCommand> WaitForCaptureCommandAsync(
            BenchmarkLaunchOptions options,
            CancellationToken cancellationToken)
        {
            double deadline = Time.realtimeSinceStartupAsDouble + options.StageTimeoutSeconds;
            BenchmarkCaptureCommand command;
            while (!BenchmarkFileProtocol.TryReadCaptureCommand(options, out command))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (Time.realtimeSinceStartupAsDouble >= deadline)
                    throw new TimeoutException("等待 Editor 统一采样命令超时。");
                await UniTask.Delay(100, ignoreTimeScale: true, cancellationToken: cancellationToken);
            }
            return command;
        }

        private static async UniTask WaitUntilUtcAsync(long utcTicks, CancellationToken cancellationToken)
        {
            while (DateTime.UtcNow.Ticks < utcTicks)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await UniTask.Delay(10, ignoreTimeScale: true, cancellationToken: cancellationToken);
            }
        }
    }
}
