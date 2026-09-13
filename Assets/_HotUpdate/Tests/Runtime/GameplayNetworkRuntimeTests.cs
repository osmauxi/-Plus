using System;
using System.Collections;
using System.Threading;
using NUnit.Framework;
using ProjectGame.HotFix.Gameplay.Network;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectGame.HotFix.Tests.Runtime
{
    public sealed class GameplayNetworkRuntimeTests
    {
        private GameObject _managerObject;
        private GameObject _runtimeObject;
        private GameObject _playerObject;
        private NetworkManager _manager;
        private GameNetworkRuntime _runtime;
        private NetworkPrefabsList _prefabs;
        private GameObject _duplicateRuntimeObject;

        [UnityTest]
        public IEnumerator FailedInitialize_CanRetry_AndDuplicateRuntimeCannotReplaceSession()
        {
            Assert.That(NetworkManager.Singleton, Is.Null);
            _managerObject = new GameObject("GameplayClockLifecycleManager");
            _manager = _managerObject.AddComponent<NetworkManager>();
            var transport = _managerObject.AddComponent<UnityTransport>();
            transport.SetConnectionData("127.0.0.1", 0, "127.0.0.1");
            _manager.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = transport, TickRate = 60, EnableSceneManagement = false,
            };
            _runtimeObject = new GameObject("GameplayClockLifecycleRuntime");
            _runtime = _runtimeObject.AddComponent<GameNetworkRuntime>();
            var driver = _runtimeObject.GetComponent<GameplayNetworkTickDriver>();
            driver.enabled = false;
            Assert.Throws<InvalidOperationException>(() =>
                _runtime.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult());
            Assert.That(driver.IsInitialized, Is.False);
            Assert.That(GameNetworkRuntime.Gameplay, Is.Null);
            Assert.That(GameNetworkRuntime.PlayerSync, Is.Null);

            Assert.That(_manager.StartHost(), Is.True);
            _runtime.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
            var original = GameNetworkRuntime.Gameplay;
            driver.Advance(1d / 60d);
            _runtime.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(GameNetworkRuntime.Gameplay, Is.SameAs(original));
            Assert.That(driver.Advance(1d / 60d), Is.EqualTo(1));

            _duplicateRuntimeObject = new GameObject("DuplicateGameplayClockRuntime");
            var duplicate = _duplicateRuntimeObject.AddComponent<GameNetworkRuntime>();
            Assert.Throws<InvalidOperationException>(() =>
                duplicate.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult());
            Object.Destroy(_duplicateRuntimeObject);
            yield return null;
            Assert.That(GameNetworkRuntime.Gameplay, Is.SameAs(original));
            Assert.That(driver.IsInitialized, Is.True);

            _runtime.ShutdownAsync(CancellationToken.None).GetAwaiter().GetResult();
            _runtime.ShutdownAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(driver.IsInitialized, Is.False);
            Assert.That(original.IsInitialized, Is.False);
            _runtime.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(GameNetworkRuntime.Gameplay, Is.Not.SameAs(original));
            Assert.That(GameNetworkRuntime.PlayerSync.IsInitialized, Is.True);
            Assert.That(driver.Advance(1d / 60d), Is.Zero, "新会话不继承旧会话余数。");
            Assert.That(driver.Advance(1d / 60d), Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator IndependentClock_DrivesHostPlayerThroughWarpRespawnAndShutdown()
        {
            Assert.That(NetworkManager.Singleton, Is.Null, "集成测试需要空闲网络会话。");
            _managerObject = new GameObject("GameplayClockTestManager");
            _manager = _managerObject.AddComponent<NetworkManager>();
            var transport = _managerObject.AddComponent<UnityTransport>();
            transport.SetConnectionData("127.0.0.1", 0, "127.0.0.1");
            _manager.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = transport,
                TickRate = 60,
                EnableSceneManagement = false,
            };
            var prefab = Resources.Load<GameObject>("PlayerSyncNetworkTestPlayer");
            Assert.That(prefab, Is.Not.Null);
            _prefabs = ScriptableObject.CreateInstance<NetworkPrefabsList>();
            _prefabs.Add(new NetworkPrefab { Prefab = prefab });
            _manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Add(_prefabs);
            Assert.That(_manager.StartHost(), Is.True);

            _runtimeObject = new GameObject("GameplayClockTestRuntime");
            _runtime = _runtimeObject.AddComponent<GameNetworkRuntime>();
            _runtime.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
            var driver = _runtimeObject.GetComponent<GameplayNetworkTickDriver>();
            var clock = GameNetworkRuntime.Gameplay.Clock;
            Assert.That(clock.TickRate, Is.EqualTo(30));
            Assert.That(driver.IsInitialized, Is.True);

            // 手动控制步进，让 NGO 自己持续运行，确认它不会再驱动 Gameplay Clock。
            driver.enabled = false;
            uint initialTick = clock.CurrentTick;
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(clock.CurrentTick, Is.EqualTo(initialTick));

            _playerObject = Object.Instantiate(prefab, Vector3.up * 2f, Quaternion.identity);
            var networkObject = _playerObject.GetComponent<NetworkObject>();
            var player = _playerObject.GetComponent<PlayerSyncController>();
            networkObject.SpawnWithOwnership(NetworkManager.ServerClientId);
            Assert.That(player.IsSimulationInitialized, Is.True);
            int serverSteps = 0;
            player.ServerTickCompleted += _ => serverSteps++;
            PlayerInputCommand input = PlayerInputCommand.CreateNeutral(0u);
            input.WorldMove = Vector2.up;
            player.SubmitLocalInput(input);
            Assert.That(driver.Advance(0.1d), Is.EqualTo(3));
            Assert.That(serverSteps, Is.EqualTo(3), "Host 应只走权威模拟一次。");
            Assert.That(player.SimulationTick, Is.EqualTo(clock.CurrentTick));

            // NGO 时间此时已独立前进；Warp 必须保留 Gameplay 的连续权威 Tick。
            uint beforeWarp = clock.CurrentTick;
            player.ResetAfterWarp(new Vector3(5f, 2f, 5f), Quaternion.identity);
            Assert.That(player.SimulationTick, Is.EqualTo(beforeWarp));
            Assert.That(driver.Advance(1d / 30d), Is.EqualTo(1));
            Assert.That(player.SimulationTick, Is.EqualTo(unchecked(beforeWarp + 1u)));

            networkObject.Despawn(false);
            driver.Advance(1d / 30d);
            Assert.That(serverSteps, Is.EqualTo(4), "Despawn 必须退订。");
            networkObject.SpawnWithOwnership(NetworkManager.ServerClientId);
            driver.Advance(1d / 30d);
            Assert.That(serverSteps, Is.EqualTo(5), "再次 Spawn 只能订阅一次。");
            Assert.That(player.SimulationTick, Is.EqualTo(clock.CurrentTick));

            // 真实 Update 在 timeScale=0 下仍推进网络模拟。
            uint beforeUpdate = clock.CurrentTick;
            float previousScale = Time.timeScale;
            try
            {
                Time.timeScale = 0f;
                driver.enabled = true;
                yield return new WaitForSecondsRealtime(0.15f);
                Assert.That(clock.CurrentTick, Is.GreaterThan(beforeUpdate));
            }
            finally
            {
                Time.timeScale = previousScale;
            }

            networkObject.Despawn(false);
            _runtime.ShutdownAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(driver.IsInitialized, Is.False);
            Assert.That(GameNetworkRuntime.Gameplay, Is.Null);
            Assert.That(driver.Advance(0.1d), Is.Zero);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_playerObject != null)
            {
                var networkObject = _playerObject.GetComponent<NetworkObject>();
                if (networkObject.IsSpawned) networkObject.Despawn(false);
                Object.Destroy(_playerObject);
            }
            if (_runtime != null)
                _runtime.ShutdownAsync(CancellationToken.None).GetAwaiter().GetResult();
            if (_runtimeObject != null) Object.Destroy(_runtimeObject);
            if (_duplicateRuntimeObject != null) Object.Destroy(_duplicateRuntimeObject);
            if (_manager != null) _manager.Shutdown();
            yield return null;
            if (_managerObject != null) Object.Destroy(_managerObject);
            if (_prefabs != null) Object.Destroy(_prefabs);
            yield return null;
        }
    }
}
