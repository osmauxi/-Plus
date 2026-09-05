#if UNITY_EDITOR
using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using ProjectGame.HotFix.Gameplay.Runtime;
using ProjectGame.HotFix.Gameplay.Player;
using ProjectGame.HotFix.Gameplay.State;
using ProjectGame.HotFix.Netcode;
using ProjectGame.HotFix.Network.Runtime;
using ProjectGame.HotFix.SceneFlow;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;
using System.Text;
using Object = UnityEngine.Object;

namespace ProjectGame.HotFix.Tests.SceneFlow
{
    public sealed class ProductionSceneFlowTests
    {
        [UnityTest]
        public IEnumerator ProductionHost_GameRoot_RoundTripAndReenter() => Run(async () =>
        {
            Assert.IsNull(NetworkManager.Singleton);
            HotFixEntry.StartGame();
            await WaitAsync(() => NetworkSessionBootstrap.Instance != null &&
                AddressableSceneLoadService.Shared.IsLoaded("Assets/_HotUpdate/Scenes/LobbyScene.unity"));
            NetworkSessionBootstrap session = NetworkSessionBootstrap.Instance;
            await session.PrepareConnectionAsync(CancellationToken.None);
            NetworkManager manager = NetworkManager.Singleton;
            manager.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes("production-test");
            NetworkObject sessionSeed = NetworkRuntimeBootstrap.Instance.PrefabRegistry
                .GetPersistentSeed(NetworkPrefabId.NetworkSessionRoot);
            Assert.IsFalse(sessionSeed.IsSpawned);
            Assert.IsTrue(manager.StartHost());
            await session.WaitForLobbyReadyAsync(CancellationToken.None);
            LobbyNetworkManager lobby = LobbyNetworkManager.Instance;
            Assert.IsNotNull(lobby);
            Assert.IsTrue(lobby.IsSpawned);
            Assert.AreEqual("LobbyScene", lobby.gameObject.scene.name);
            Assert.AreNotEqual("DontDestroyOnLoad", lobby.gameObject.scene.name);
            lobby.StartSinglePlayerAndEnterGame();
            await WaitAsync(() => GameStateController.Instance != null &&
                GameStateController.Instance.CurrentState == GameState.GamePlaying);
            Assert.IsTrue(GameRuntimeBootstrap.Instance.IsLocalPlayerRuntimeReady);
            NetworkObject game = GameRuntimeBootstrap.Instance.NetworkObject;
            Assert.AreEqual("GameRunTimeScene", game.gameObject.scene.name);
            Assert.AreNotEqual("DontDestroyOnLoad", game.gameObject.scene.name);
            foreach (PlayerRuntime player in Object.FindObjectsOfType<PlayerRuntime>(true))
                Assert.AreEqual("GameRunTimeScene", player.gameObject.scene.name);
            await GameSceneFlowController.Instance.TransitionToLobbySceneAsync();
            Assert.IsTrue(game == null || !game.IsSpawned);
            Assert.IsTrue(lobby == null);
            lobby = LobbyNetworkManager.Instance;
            Assert.IsNotNull(lobby);
            Assert.AreEqual("LobbyScene", lobby.gameObject.scene.name);
            await UniTask.DelayFrame(2);
            lobby.StartSinglePlayerAndEnterGame();
            await WaitAsync(() => GameStateController.Instance != null &&
                GameStateController.Instance.CurrentState == GameState.GamePlaying);
            Assert.IsTrue(GameRuntimeBootstrap.Instance.IsLocalPlayerRuntimeReady);
            await GameSceneFlowController.Instance.TransitionToLobbySceneAsync();
            lobby = LobbyNetworkManager.Instance;
            // 只有会话 Root 跨完整 NGO 关闭 / 重开复用；Lobby Root 随 Lobby Scope 重建。
            manager.Shutdown();
            await WaitAsync(() => !manager.IsListening && !manager.ShutdownInProgress);
            await UniTask.DelayFrame(3);
            await session.PrepareConnectionAsync(CancellationToken.None);
            Assert.IsTrue(lobby == null);
            Assert.IsNull(LobbyNetworkManager.Instance);
            Assert.IsFalse(sessionSeed.IsSpawned);
            Assert.IsTrue(manager.StartHost());
            await session.WaitForLobbyReadyAsync(CancellationToken.None);
            Assert.IsTrue(sessionSeed.IsSpawned);
            Assert.IsNotNull(LobbyNetworkManager.Instance);
            Assert.AreEqual("LobbyScene", LobbyNetworkManager.Instance.gameObject.scene.name);
            Assert.AreEqual(NetworkSceneMask.Lobby, NetworkRuntimeBootstrap.Instance.ScopeManager.ActiveSceneMask);
        });

        [UnityTearDown]
        public IEnumerator TearDown() => Run(async () =>
        {
            NetworkManager manager = NetworkManager.Singleton;
            if (GameRuntimeBootstrap.Instance != null)
                await GameRuntimeBootstrap.Instance.ShutdownScopeAsync(CancellationToken.None);
            if (GameSceneFlowController.Instance != null)
                SceneFlowPipelineTests.Set(GameSceneFlowController.Instance, "_commitReached", false);
            if (manager != null)
            {
                manager.Shutdown();
                await WaitAsync(() => !manager.IsListening && !manager.ShutdownInProgress);
                NetworkRuntimeBootstrap.Instance?.ResetAfterShutdown();
            }
            UnityEngine.SceneManagement.SceneManager.CreateScene("ProductionTestCleanup");
            foreach (string name in new[] { "UIGameUIScene", "GameRunTimeScene", "LobbyScene" })
                await AddressableSceneLoadService.Shared.UnloadSceneAsync("Assets/_HotUpdate/Scenes/" + name + ".unity", CancellationToken.None);
            if (manager != null) Object.Destroy(manager.gameObject);
            await UniTask.DelayFrame(2);
        });

        private static UniTask WaitAsync(Func<bool> condition)
            => SceneFlowLocalOperation.WaitAsync(condition, 60f, "Production SceneFlow timed out", CancellationToken.None);
        private static IEnumerator Run(Func<UniTask> action) { yield return action().ToCoroutine(); }
    }
}
#endif
