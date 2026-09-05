#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using ProjectGame.HotFix.Network.Runtime;
using ProjectGame.HotFix.SceneFlow;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectGame.HotFix.Tests.SceneFlow
{
    [TestFixture(true)]
    [TestFixture(false)]
    public sealed partial class SceneFlowPipelineTests
    {
        public const string Fixtures = "Assets/_HotUpdate/Tests/SceneFlow/Fixtures/";
        private readonly bool _host;
        private NetworkManager _network;
        private NetworkRuntimeBootstrap _runtime;
        private NetworkPrefabCatalog _catalog;
        private GameSceneFlowController _flow;
        private NetworkScopeBarrier _scope;
        private ResourceLocationMap _locator;
        private TestAssetProvider _provider;
        private readonly List<GameObject> _created = new List<GameObject>();
        private const float Timeout = 4f;

        public SceneFlowPipelineTests(bool host) { _host = host; }
        private sealed class TestAssetProvider : AssetDatabaseProvider { public TestAssetProvider() : base(0.01f) { } }

        [UnitySetUp]
        public IEnumerator SetUp() => Run(async () =>
        {
            ScopeLifecycleProbe.Calls.Clear();
            ScopeLifecycleProbe.Failure = null;
            ScopeLifecycleProbe.ActivatedBeforeCleanup = false;
            Assert.IsNull(NetworkManager.Singleton, "Tests require the Test Runner's empty scene");
            await Addressables.InitializeAsync().ToUniTask();
            _provider = new TestAssetProvider();
            Addressables.ResourceManager.ResourceProviders.Add(_provider);
            _locator = new ResourceLocationMap("SceneFlowPipelineTests");
            foreach (string name in new[] { "Session", "Lobby", "Game" })
            {
                string path = Fixtures + name + ".prefab";
                var location = new ResourceLocationBase(path, path, _provider.ProviderId, typeof(GameObject));
                _locator.Add(AssetDatabase.AssetPathToGUID(path), location);
            }
            foreach (string name in new[] { "PipelineLobby", "PipelineGame", "PipelineUI" })
            {
                string path = Fixtures + name + ".unity";
                _locator.Add(path, new ResourceLocationBase(path, path, typeof(SceneProvider).FullName, typeof(SceneInstance)));
            }
            Addressables.AddResourceLocator(_locator);

            _catalog = Object.Instantiate(AssetDatabase.LoadAssetAtPath<NetworkPrefabCatalog>(Fixtures + "Catalog.asset"));
            var networkObject = new GameObject("PipelineTestNetwork");
            _created.Add(networkObject);
            _network = networkObject.AddComponent<NetworkManager>();
            var transport = networkObject.AddComponent<UnityTransport>();
            transport.SetConnectionData("127.0.0.1", 0, "127.0.0.1");
            _network.NetworkConfig = new NetworkConfig { NetworkTransport = transport };
            _runtime = networkObject.AddComponent<NetworkRuntimeBootstrap>();
            Set(_runtime, "_catalog", _catalog);
            _runtime.Initialize();
            GameObject sessionPrefab = await _runtime.PrefabRegistry.PrepareAsync(NetworkPrefabId.NetworkSessionRoot, CancellationToken.None);
            Assert.IsTrue(_host ? _network.StartHost() : _network.StartServer());
            var session = Object.Instantiate(sessionPrefab);
            _created.Add(session);
            _flow = session.GetComponent<GameSceneFlowController>();
            _scope = session.GetComponent<NetworkScopeBarrier>();
            session.GetComponent<NetworkObject>().Spawn(false);
            await _flow.TransitionToLobbySceneAsync();
            Assert.AreEqual(NetworkSceneMask.Lobby, _runtime.ScopeManager.ActiveSceneMask);
            Assert.IsTrue(_runtime.ScopeManager.TryGetInstance(NetworkPrefabId.LobbyNetworkRoot, out NetworkObject lobbyRoot));
            Assert.AreEqual("PipelineLobby", lobbyRoot.gameObject.scene.name);
            Assert.AreNotEqual("DontDestroyOnLoad", lobbyRoot.gameObject.scene.name);
            ScopeLifecycleProbe.Calls.Clear();
        });

        [UnityTearDown]
        public IEnumerator TearDown() => Run(async () =>
        {
            ScopeLifecycleProbe.Failure = null;
            if (_flow != null) Set(_flow, "_commitReached", false);
            if (_network != null)
            {
                _network.Shutdown();
                await SceneFlowLocalOperation.WaitAsync(() => !_network.ShutdownInProgress, Timeout, "shutdown", CancellationToken.None);
                if (_runtime != null && _runtime.IsInitialized) _runtime.ResetAfterShutdown();
            }
            using var cancellation = new CancellationTokenSource();
            using var timeout = cancellation.CancelAfterSlim(TimeSpan.FromSeconds(Timeout), DelayType.Realtime);
            foreach (string name in new[] { "PipelineUI", "PipelineGame", "PipelineLobby" })
                await AddressableSceneLoadService.Shared.UnloadSceneAsync(Fixtures + name + ".unity", cancellation.Token);
            foreach (GameObject go in _created) if (go != null) Object.Destroy(go);
            _created.Clear();
            if (_catalog != null) Object.Destroy(_catalog);
            if (_locator != null) Addressables.RemoveResourceLocator(_locator);
            if (_provider != null) Addressables.ResourceManager.ResourceProviders.Remove(_provider);
            await UniTask.Yield();
        });

        [UnityTest]
        public IEnumerator RoundTrip_StagesAndPhysicalCleanup() => Run(async () =>
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByPath(Fixtures + "PipelineLobby.unity"));
            await _flow.TransitionToGameSceneAsync();
            Assert.IsTrue(_runtime.ScopeManager.TryGetInstance(NetworkPrefabId.GameRuntimeNetworkRoot, out NetworkObject gameRoot));
            Assert.AreEqual("PipelineGame", gameRoot.gameObject.scene.name);
            Assert.AreNotEqual("DontDestroyOnLoad", gameRoot.gameObject.scene.name);
            CollectionAssert.AreEqual(new[] { "GameRuntimeNetworkRoot:Bind", "BindOnly", "GameRuntimeNetworkRoot:Initialize", "GameRuntimeNetworkRoot:Activate" }, ScopeLifecycleProbe.Calls);
            Assert.IsFalse(ScopeLifecycleProbe.ActivatedBeforeCleanup);
            Assert.IsFalse(_runtime.ScopeManager.HasInstance(NetworkPrefabId.LobbyNetworkRoot));
            Assert.IsFalse(_runtime.PrefabRegistry.IsPrepared(NetworkPrefabId.LobbyNetworkRoot));
            Assert.IsFalse(_flow.IsTransitioning);
            await _flow.TransitionToLobbySceneAsync();
            Assert.IsFalse(_runtime.ScopeManager.HasInstance(NetworkPrefabId.GameRuntimeNetworkRoot));
            Assert.IsFalse(_runtime.PrefabRegistry.IsPrepared(NetworkPrefabId.GameRuntimeNetworkRoot));
            await _flow.TransitionToGameSceneAsync();
            Assert.AreEqual(2, ScopeLifecycleProbe.Calls.Count(x => x == "GameRuntimeNetworkRoot:Activate"));
        });

        [UnityTest]
        public IEnumerator BindFailure_RollsBackAndCanRetry() => Run(() => FailureAndRetry("Bind"));

        [UnityTest]
        public IEnumerator InitializeFailure_RollsBackAndCanRetry() => Run(() => FailureAndRetry("Initialize"));

        [UnityTest]
        public IEnumerator InitializeTimeout_RollsBackAndCanRetry() => Run(() => FailureAndRetry("Timeout"));

        private async UniTask FailureAndRetry(string stage)
        {
            NetworkObject oldLobby;
            Assert.IsTrue(_runtime.ScopeManager.TryGetInstance(NetworkPrefabId.LobbyNetworkRoot, out oldLobby));
            ScopeLifecycleProbe.Failure = stage;
            Set(_flow, "_operationTimeoutSeconds", stage == "Timeout" ? 0.15f : Timeout);
            Exception error = await CaptureAsync(_flow.TransitionToGameSceneAsync());
            Assert.IsNotNull(error);
            Assert.AreEqual(NetworkSceneMask.Lobby, _runtime.ScopeManager.ActiveSceneMask);
            Assert.IsTrue(oldLobby != null && oldLobby.IsSpawned, "Rollback must preserve the old Root");
            Assert.IsFalse(_runtime.PrefabRegistry.IsPrepared(NetworkPrefabId.GameRuntimeNetworkRoot));
            Assert.IsFalse(AddressableSceneLoadService.Shared.IsLoaded(Fixtures + "PipelineGame.unity"));
            Assert.IsFalse(ScopeLifecycleProbe.Calls.Contains("GameRuntimeNetworkRoot:Activate"));
            ScopeLifecycleProbe.Failure = null;
            Set(_flow, "_operationTimeoutSeconds", Timeout);
            await _flow.TransitionToGameSceneAsync();
        }

        [UnityTest]
        public IEnumerator ActivateFailure_ReturnsToLobbyAfterCommit() => Run(async () =>
        {
            ScopeLifecycleProbe.Failure = "Activate";
            Assert.IsNotNull(await CaptureAsync(_flow.TransitionToGameSceneAsync()));
            Assert.AreEqual(NetworkSceneMask.Lobby, _runtime.ScopeManager.ActiveSceneMask);
            Assert.IsTrue(_runtime.ScopeManager.HasInstance(NetworkPrefabId.LobbyNetworkRoot));
            Assert.IsFalse(_runtime.ScopeManager.HasInstance(NetworkPrefabId.GameRuntimeNetworkRoot));
            Assert.IsFalse(AddressableSceneLoadService.Shared.IsLoaded(Fixtures + "PipelineGame.unity"));
            Assert.IsFalse(_flow.IsTransitioning);
        });

        [UnityTest]
        public IEnumerator CommitRequiresRuntimeReady_ActivateRequiresCleanup() => Run(async () =>
        {
            await AddressableSceneLoadService.Shared.LoadSceneAsync(
                Fixtures + "PipelineGame.unity", LoadSceneMode.Additive, CancellationToken.None);
            await _scope.PrepareForAllClientsAsync(NetworkSceneMask.GameRuntime | NetworkSceneMask.GameUI, Timeout, CancellationToken.None);
            _scope.SpawnPreparedScope();
            await _scope.WaitForRootsReadyForAllClientsAsync(Timeout, CancellationToken.None);
            var context = (NetworkScopePrepareContext)Get(_scope, "_localContext");
            Assert.Throws<InvalidOperationException>(() => _runtime.ScopeManager.CommitPreparedScope(context));
            await _scope.RunPreCommitStagesForAllClientsAsync(Timeout, CancellationToken.None);
            Assert.IsTrue(context.IsRuntimeReady);
            Assert.IsFalse(context.IsActivated);
            await _scope.CommitForAllClientsAsync(Timeout, CancellationToken.None);
            Assert.Throws<InvalidOperationException>(() => _runtime.ScopeManager.ActivatePreparedScope(context));
            Assert.Throws<InvalidOperationException>(() => _runtime.ScopeManager.DespawnPreparedScopeRoots(context));
            await _scope.CleanupObsoleteScopeForAllClientsAsync(Timeout, CancellationToken.None);
            Assert.AreSame(context, Get(_scope, "_localContext"));
            await AddressableSceneLoadService.Shared.UnloadSceneAsync(Fixtures + "PipelineLobby.unity", CancellationToken.None);
            _scope.ActivateForAllClients();
            Assert.IsTrue(context.IsActivated);
            Assert.IsNull(Get(_scope, "_localContext"));
        });

        [UnityTest]
        public IEnumerator LocalTimeout_DrainsBeforeNextWriter() => Run(async () =>
        {
            var operation = new SceneFlowLocalOperation();
            var finish = new UniTaskCompletionSource();
            bool wrote = false;
            Assert.IsInstanceOf<TimeoutException>(await CaptureAsync(operation.RunAsync(async _ => { await finish.Task; wrote = true; }, 0.03f, CancellationToken.None)));
            Assert.IsTrue(operation.IsRunning);
            Assert.IsInstanceOf<InvalidOperationException>(await CaptureAsync(operation.RunAsync(_ => UniTask.CompletedTask, Timeout, CancellationToken.None)));
            finish.TrySetResult();
            await operation.CancelAndDrainAsync(Timeout, CancellationToken.None);
            Assert.IsTrue(wrote);
            await operation.RunAsync(_ => UniTask.CompletedTask, Timeout, CancellationToken.None);
        });

        [UnityTest]
        public IEnumerator CancelledSceneLoad_IsStillOwnedAndCanBeUnloaded() => Run(async () =>
        {
            var loader = AddressableSceneLoadService.Shared;
            string address = Fixtures + "PipelineGame.unity";
            using var cancellation = new CancellationTokenSource();
            UniTask<Scene> load = loader.LoadSceneAsync(address, LoadSceneMode.Additive, cancellation.Token);
            cancellation.Cancel();
            Exception failure = null;
            try { await load; }
            catch (Exception exception) { failure = exception; }
            Assert.IsInstanceOf<OperationCanceledException>(failure);
            await UniTask.WhenAll(loader.UnloadSceneAsync(address, CancellationToken.None),
                loader.UnloadSceneAsync(address, CancellationToken.None));
            Scene scene = SceneManager.GetSceneByPath(address);
            Assert.IsFalse(scene.IsValid() && scene.isLoaded);
            Assert.IsFalse(loader.IsLoaded(address));
        });

        [UnityTest]
        public IEnumerator PrepareTimeout_DoesNotPublishAfterRollback() => Run(async () =>
        {
            if (_host) LogAssert.Expect(LogType.Error, new Regex(@"^\[NetworkScopeBarrier\] Revision=\d+, Prepare 失败"));
            Exception failure = null;
            try { await _scope.PrepareForAllClientsAsync(NetworkSceneMask.GameRuntime, 0.001f, CancellationToken.None); }
            catch (Exception exception) { failure = exception; }
            Assert.IsNotNull(failure);
            await _scope.RollbackForAllClientsAsync(Timeout, CancellationToken.None);
            await UniTask.Delay(80, ignoreTimeScale: true);
            Assert.IsFalse(_runtime.ScopeManager.IsPreparing);
            Assert.IsFalse(_runtime.PrefabRegistry.IsPrepared(NetworkPrefabId.GameRuntimeNetworkRoot));
            Assert.IsNull(Get(_scope, "_localContext"));
            await _scope.PrepareForAllClientsAsync(NetworkSceneMask.GameRuntime, Timeout, CancellationToken.None);
            await _scope.RollbackForAllClientsAsync(Timeout, CancellationToken.None);
        });

        [UnityTest]
        public IEnumerator AckWait_IsBoundedAndSupportsDedicatedServerWithoutClients() => Run(async () =>
        {
            var barrier = new NetworkBarrierState();
            barrier.Begin(_network, 10, "missing-ready-ack");
            float scale = Time.timeScale;
            try
            {
                Time.timeScale = 0;
                Exception error = await CaptureAsync(barrier.WaitAsync(_network, 0.04f, CancellationToken.None));
                if (_host) Assert.IsInstanceOf<TimeoutException>(error);
                else Assert.IsNull(error);
            }
            finally { Time.timeScale = scale; }
        });

        [Test]
        public void Ack_RejectsStaleDuplicateAndUnknownSender()
        {
            var barrier = new NetworkBarrierState();
            barrier.Begin(new ulong[] { 10, 20 }, 7, "test");
            barrier.Complete(6, 10, true, "");
            barrier.Complete(7, 99, false, "unknown");
            var pending = (HashSet<ulong>)Get(barrier, "_pendingClients");
            Assert.AreEqual(2, pending.Count);
            barrier.Complete(7, 10, false, "initialization failed");
            barrier.Complete(7, 10, true, "");
            Assert.AreEqual(1, pending.Count);
            var failures = (Dictionary<ulong, string>)Get(barrier, "_failures");
            Assert.AreEqual("initialization failed", failures[10]);
        }

        private static async UniTask<Exception> CaptureAsync(UniTask task)
        {
            bool old = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
            try { await task; return null; }
            catch (Exception exception) { return exception; }
            finally { LogAssert.ignoreFailingMessages = old; }
        }

        // Defer task creation until the Test Runner has entered its coroutine/log scope.
        private static IEnumerator Run(Func<UniTask> operation)
        {
            yield return operation().ToCoroutine();
        }

        public static void Set(object target, string name, object value)
            => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        private static object Get(object target, string name)
            => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    }
}
#endif
