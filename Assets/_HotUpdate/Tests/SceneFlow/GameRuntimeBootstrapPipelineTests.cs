#if UNITY_EDITOR
using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using ProjectGame.HotFix.Core.Session;
using ProjectGame.HotFix.Gameplay.Runtime;
using ProjectGame.HotFix.Gameplay.State;
using ProjectGame.HotFix.Network.Runtime;
using ProjectGame.HotFix.SceneFlow;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectGame.HotFix.Tests.SceneFlow
{
    public sealed partial class SceneFlowPipelineTests
    {
        public static void CreateBootstrapFixture()
        {
            string path = Fixtures + "BootstrapGame.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
            Scene preview = EditorSceneManager.NewPreviewScene();
            try
            {
                var root = new GameObject("BootstrapGame");
                SceneManager.MoveGameObjectToScene(root, preview);
                root.AddComponent<NetworkObject>();
                Set(root.AddComponent<NetworkScopeMember>(), "_id", NetworkPrefabId.GameRuntimeNetworkRoot);
                root.AddComponent<GameStateController>();
                var bootstrap = root.AddComponent<GameRuntimeBootstrap>();
                var a = root.AddComponent<BootstrapServiceProbe>(); a.ServiceName = "A";
                var b = root.AddComponent<BootstrapServiceProbe>(); b.ServiceName = "B";
                var level = root.AddComponent<GameLevelFlowController>();
                Set(bootstrap, "_runtimeServiceComponents", new MonoBehaviour[] { a, b, level });
                Set(bootstrap, "_levelFlowController", level);
                Set(bootstrap, "_requiredSceneNames", Array.Empty<string>());
                Set(bootstrap, "_runtimeReadyTimeoutSeconds", 0.15f);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { EditorSceneManager.ClosePreviewScene(preview); }
        }

        private void UseBootstrapGame()
        {
            BootstrapServiceProbe.Calls.Clear();
            BootstrapServiceProbe.HoldFirst = true;
            BootstrapServiceProbe.FailSecond = true;
            BootstrapServiceProbe.StoppedWhileSpawned = false;
            string path = Fixtures + "BootstrapGame.prefab";
            string guid = AssetDatabase.AssetPathToGUID(path);
            _locator.Add(guid, new ResourceLocationBase(path, path, _provider.ProviderId, typeof(GameObject)));
            Assert.IsTrue(_catalog.TryGetEntry(NetworkPrefabId.GameRuntimeNetworkRoot, out NetworkPrefabEntry entry));
            Set(entry, "_prefab", new AssetReferenceGameObject(guid));
            GameSessionContext.Configure(GameSessionMode.SinglePlayer,
                new[] { new PlayerSessionData(0, "probe", "probe", 1, 1, 0) });
        }

        [UnityTest]
        public IEnumerator Bootstrap_PreCommitDoesNotStartServices_AndCleanupDrains() => Run(async () =>
        {
            UseBootstrapGame();
            try
            {
                await AddressableSceneLoadService.Shared.LoadSceneAsync(
                    Fixtures + "PipelineGame.unity", UnityEngine.SceneManagement.LoadSceneMode.Additive,
                    CancellationToken.None);
                await _scope.PrepareForAllClientsAsync(NetworkSceneMask.GameRuntime | NetworkSceneMask.GameUI, Timeout, CancellationToken.None);
                _scope.SpawnPreparedScope();
                await _scope.WaitForRootsReadyForAllClientsAsync(Timeout, CancellationToken.None);
                await UniTask.Yield();
                Assert.IsEmpty(BootstrapServiceProbe.Calls, "Spawn must not start RunRuntimeAsync");
                await _scope.RunPreCommitStagesForAllClientsAsync(Timeout, CancellationToken.None);
                Assert.IsEmpty(BootstrapServiceProbe.Calls, "Scope Initialize must not initialize gameplay services");
                await _scope.CommitForAllClientsAsync(Timeout, CancellationToken.None);
                await _scope.CleanupObsoleteScopeForAllClientsAsync(Timeout, CancellationToken.None);
                _scope.ActivateForAllClients();
                _scope.ActivateForAllClients();
                await SceneFlowLocalOperation.WaitAsync(() => BootstrapServiceProbe.Calls.Count > 0,
                    Timeout, "RunRuntimeAsync start", CancellationToken.None);
                CollectionAssert.AreEqual(new[] { "A:Initialize" }, BootstrapServiceProbe.Calls);
                await _flow.TransitionToLobbySceneAsync();
                CollectionAssert.AreEqual(new[] { "A:Initialize", "A:Shutdown" }, BootstrapServiceProbe.Calls);
                Assert.IsTrue(BootstrapServiceProbe.StoppedWhileSpawned);
            }
            finally { GameSessionContext.Clear(); BootstrapServiceProbe.HoldFirst = false; }
        });

        [UnityTest]
        public IEnumerator Bootstrap_AsyncFailure_RecoversAndShutsDownInReverse() => Run(async () =>
        {
            UseBootstrapGame();
            BootstrapServiceProbe.HoldFirst = false;
            bool old = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
            try
            {
                await _flow.TransitionToGameSceneAsync();
                await SceneFlowLocalOperation.WaitAsync(() => !_flow.IsTransitioning &&
                    _runtime.ScopeManager.ActiveSceneMask == NetworkSceneMask.Lobby,
                    Timeout, "asynchronous recovery", CancellationToken.None);
                CollectionAssert.AreEqual(new[] { "A:Initialize", "B:Initialize", "B:Shutdown", "A:Shutdown" }, BootstrapServiceProbe.Calls);
                Assert.IsTrue(BootstrapServiceProbe.StoppedWhileSpawned);
                Assert.IsFalse(_runtime.ScopeManager.HasInstance(NetworkPrefabId.GameRuntimeNetworkRoot));
            }
            finally { LogAssert.ignoreFailingMessages = old; GameSessionContext.Clear(); }
        });

        [UnityTest]
        public IEnumerator Bootstrap_LocalStartupTimeout_ReturnsToLobby() => Run(async () =>
        {
            UseBootstrapGame();
            bool old = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
            try
            {
                await _flow.TransitionToGameSceneAsync();
                await SceneFlowLocalOperation.WaitAsync(() => !_flow.IsTransitioning &&
                    _runtime.ScopeManager.ActiveSceneMask == NetworkSceneMask.Lobby,
                    Timeout, "startup timeout recovery", CancellationToken.None);
                CollectionAssert.AreEqual(new[] { "A:Initialize", "A:Shutdown" }, BootstrapServiceProbe.Calls);
                Assert.IsTrue(BootstrapServiceProbe.StoppedWhileSpawned);
            }
            finally { LogAssert.ignoreFailingMessages = old; GameSessionContext.Clear(); BootstrapServiceProbe.HoldFirst = false; }
        });
    }
}
#endif
