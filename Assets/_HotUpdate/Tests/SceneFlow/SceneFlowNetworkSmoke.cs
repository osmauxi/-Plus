using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Core.Network;
using ProjectGame.HotFix.Gameplay.Runtime;
using ProjectGame.HotFix.Gameplay.State;
using ProjectGame.HotFix.Netcode;
using ProjectGame.HotFix.Network.Runtime;
using ProjectGame.HotFix.SceneFlow;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectGame.HotFix.Tests.SceneFlow
{
    /// <summary>独立进程联机冒烟测试。仅在测试场景/包含测试程序集的开发包中使用。</summary>
    public sealed class SceneFlowNetworkSmoke : MonoBehaviour
    {
        public string EditorRole = "host";
        public string OutputDirectory = "Temp/SceneFlowSmoke";
        public string LocalContentDirectory = "ServerData/StandaloneWindows64";
        public bool InjectClientInitializationFailure;
        private string _role;
        private string _output;
        private readonly List<string> _errors = new();
        private int _games;
        private bool _completed;
        private bool _sawExpectedFailure;

        [Serializable] private sealed class Result
        {
            public string role;
            public string stage;
            public bool passed;
            public int games;
            public bool expectedFailure;
            public string[] errors;
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;
            Application.targetFrameRate = 60;
            _role = Argument("--scene-flow-role=") ?? EditorRole;
            if (Argument("--scene-flow-failure=") == "initialize") InjectClientInitializationFailure = true;
            _output = Path.GetFullPath(Argument("--scene-flow-output=") ?? OutputDirectory);
            Directory.CreateDirectory(_output);
            string content = Path.GetFullPath(Argument("--scene-flow-content=") ?? LocalContentDirectory);
            // 测试使用本机构建内容，避免依赖开发机 HTTP 服务。生产资源地址保持不变。
            Addressables.InternalIdTransformFunc = location =>
            {
                string id = location.InternalId;
                if (Uri.TryCreate(id, UriKind.Absolute, out Uri uri) &&
                    (uri.Scheme == "http" || uri.Scheme == "https"))
                {
                    string local = Path.Combine(content, Path.GetFileName(uri.AbsolutePath));
                    if (File.Exists(local)) return local;
                }
                return id;
            };
            Application.logMessageReceived += OnLog;
            RunAsync(this.GetCancellationTokenOnDestroy()).Forget(exception => Finish(false, exception.ToString()));
        }

        private async UniTask RunAsync(CancellationToken token)
        {
            Save("boot", false);
            HotFixEntry.StartGame();
            await WaitAsync(() => NetworkSessionBootstrap.Instance != null &&
                AddressableSceneLoadService.Shared.IsLoaded("Assets/_HotUpdate/Scenes/LobbyScene.unity"), token);
            await UniTask.DelayFrame(3, cancellationToken: token);
            var session = NetworkSessionBootstrap.Instance;
            var manager = NetworkManager.Singleton;
            NetworkObject sessionSeed = NetworkRuntimeBootstrap.Instance.PrefabRegistry
                .GetPersistentSeed(NetworkPrefabId.NetworkSessionRoot);
            int sessionSeedInstanceId = sessionSeed.GetInstanceID();
            await session.PrepareConnectionAsync(token);
            manager.NetworkConfig.ConnectionData = System.Text.Encoding.UTF8.GetBytes("smoke-" + _role);
            manager.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", 17879, "127.0.0.1");
            if (_role == "host")
            {
                Require(manager.StartHost(), "StartHost failed");
                await session.WaitForLobbyReadyAsync(token);
                File.WriteAllText(Path.Combine(_output, "host-ready.flag"), "ready");
            }
            else
            {
                await WaitAsync(() => File.Exists(Path.Combine(_output, "host-ready.flag")), token);
                Require(manager.StartClient(), "StartClient failed");
            }
            await WaitAsync(() => manager.IsConnectedClient && LobbyNetworkManager.Instance != null &&
                LobbyNetworkManager.Instance.IsSpawned && LobbyNetworkManager.Instance.LobbyPlayers.Count == 2, token);
            var lobby = LobbyNetworkManager.Instance;
            Require(lobby.gameObject.scene.name == "LobbyScene", "LobbyRoot was not owned by LobbyScene");
            int lobbyInstanceId = lobby.GetInstanceID();
            await UniTask.DelayFrame(3, cancellationToken: token);
            if (InjectClientInitializationFailure && _role == "client")
                ConfigManager.Instance.GetTable<Config_RoomTemplate>().Clear();

            int rounds = InjectClientInitializationFailure ? 1 : 2;
            for (int round = 0; round < rounds; round++)
            {
                Save("ready-" + round, false);
                SubmitLocalPlayerProfile(lobby, manager.LocalClientId);
                await UniTask.DelayFrame(2, cancellationToken: token);
                lobby.ToggleReadyServerRpc();
                await WaitAsync(() => GameRuntimeBootstrap.Instance != null, token);
                if (InjectClientInitializationFailure)
                {
                    await WaitAsync(() => GameRuntimeBootstrap.Instance == null &&
                        NetworkRuntimeBootstrap.Instance.ScopeManager.ActiveSceneMask == NetworkSceneMask.Lobby &&
                        !GameSceneFlowController.Instance.IsTransitioning, token);
                    Require(_role == "host" || _sawExpectedFailure, "Client did not report injected initialization failure");
                    break;
                }
                await WaitAsync(() => GameStateController.Instance != null &&
                    GameStateController.Instance.CurrentState == GameState.GamePlaying, token);
                Require(GameRuntimeBootstrap.Instance.IsLocalPlayerRuntimeReady, "Local players were not ready at GamePlaying");
                Require(GameRuntimeBootstrap.Instance.gameObject.scene.name == "GameRunTimeScene",
                    "GameRoot was not owned by GameRunTimeScene");
                foreach (ProjectGame.HotFix.Gameplay.Player.PlayerRuntime player in
                         FindObjectsOfType<ProjectGame.HotFix.Gameplay.Player.PlayerRuntime>(true))
                    Require(player.gameObject.scene.name == "GameRunTimeScene",
                        "Player was not owned by GameRunTimeScene");
                Require(ProjectGame.HotFix.Gameplay.Player.PlayerManager.Instance.SpawnedPlayerCount == 2, "Expected two players");
                _games++;
                File.WriteAllText(Path.Combine(_output, _role + "-playing-" + round + ".flag"), "ready");
                Save("playing-" + round, false);
                if (_role == "host")
                {
                    await WaitAsync(() => File.Exists(Path.Combine(_output, "client-playing-" + round + ".flag")), token);
                    await UniTask.Delay(300, ignoreTimeScale: true, cancellationToken: token);
                    await GameSceneFlowController.Instance.TransitionToLobbySceneAsync();
                }
                else
                {
                    await WaitAsync(() => GameRuntimeBootstrap.Instance == null &&
                        NetworkRuntimeBootstrap.Instance.ScopeManager.ActiveSceneMask == NetworkSceneMask.Lobby, token);
                }
                await WaitAsync(() => !GameSceneFlowController.Instance.IsTransitioning &&
                    AddressableSceneLoadService.Shared.IsLoaded("Assets/_HotUpdate/Scenes/LobbyScene.unity") &&
                    LobbyNetworkManager.Instance != null, token);
                await UniTask.DelayFrame(5, cancellationToken: token);
                Require(lobby == null, "Previous LobbyRoot survived outside Lobby Scope");
                lobby = LobbyNetworkManager.Instance;
                Require(lobby.GetInstanceID() != lobbyInstanceId, "LobbyRoot was not recreated for Lobby Scope");
                lobbyInstanceId = lobby.GetInstanceID();
                Require(lobby.gameObject.scene.name == "LobbyScene", "Recreated LobbyRoot has wrong owner scene");
                Require(sessionSeed != null && sessionSeed.GetInstanceID() == sessionSeedInstanceId,
                    "Persistent NetworkSessionRoot was replaced");
                Require(!NetworkRuntimeBootstrap.Instance.ScopeManager.HasInstance(NetworkPrefabId.GameRuntimeNetworkRoot), "GameRoot leaked");
                File.WriteAllText(Path.Combine(_output, _role + "-lobby-" + round + ".flag"), "ready");
                if (_role == "host")
                    await WaitAsync(() => File.Exists(Path.Combine(_output, "client-lobby-" + round + ".flag")), token);
                else
                    await WaitAsync(() => File.Exists(Path.Combine(_output, "host-lobby-" + round + ".flag")), token);
            }
            Finish(_errors.Count == 0, null);
        }

        private void OnLog(string condition, string stack, LogType type)
        {
            if (_completed || (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)) return;
            // 无图形沙箱不能写 Windows PlayerPrefs；冒烟测试显式提交玩家资料，不依赖大厅 UI 的本地持久化。
            if (Application.isBatchMode && condition.StartsWith("PlayerPrefsException: Could not store preference value"))
                return;
            if (InjectClientInitializationFailure &&
                (condition.Contains("RoomTemplate") || condition.Contains("Activate 失败") ||
                 condition.Contains("运行时启动失败")))
            {
                _sawExpectedFailure = true;
                return;
            }
            _errors.Add(condition + "\n" + stack);
        }

        private void Finish(bool passed, string error)
        {
            if (_completed) return;
            if (error != null) _errors.Add(error);
            _completed = true;
            Save("complete", passed && _errors.Count == 0);
        }

        private void Save(string stage, bool passed)
        {
            File.WriteAllText(Path.Combine(_output, _role + ".json"), JsonUtility.ToJson(new Result
            {
                role = _role, stage = stage, passed = passed, games = _games,
                expectedFailure = _sawExpectedFailure, errors = _errors.ToArray()
            }, true));
        }

        private static string Argument(string prefix)
            => Environment.GetCommandLineArgs().FirstOrDefault(arg => arg.StartsWith(prefix, StringComparison.Ordinal))?.Substring(prefix.Length);
        private static void SubmitLocalPlayerProfile(LobbyNetworkManager lobby, ulong localClientId)
        {
            foreach (LobbyPlayerState player in lobby.LobbyPlayers)
            {
                if (player.ClientId != localClientId) continue;
                lobby.SubmitPlayerProfileServerRpc(
                    player.PlayerName,
                    player.CharacterId,
                    player.WeaponId,
                    player.ItemId);
                return;
            }

            throw new InvalidOperationException("Local lobby player was not registered");
        }
        private static void Require(bool condition, string error) { if (!condition) throw new InvalidOperationException(error); }
        private static UniTask WaitAsync(Func<bool> condition, CancellationToken token)
            => SceneFlowLocalOperation.WaitAsync(condition, 70f, "Smoke test wait timed out", token);
        private void OnDestroy() => Application.logMessageReceived -= OnLog;
    }
}
