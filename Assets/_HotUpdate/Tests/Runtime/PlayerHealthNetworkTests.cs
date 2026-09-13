using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using ProjectGame.HotFix.Gameplay.Combat.Health;
using ProjectGame.HotFix.Gameplay.Network;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectGame.HotFix.Tests.Runtime
{
    public sealed class PlayerHealthNetworkTests
    {
        private NetworkManager _server, _client;
        private NetworkPrefabsList _serverPrefabs, _clientPrefabs;
        private PlayerHealthRuntimeService _serverHealth, _clientHealth;

        [UnityTest]
        public IEnumerator LateJoin_ReceivesFullState_ThenDamageAndHealReplicate()
        {
            Assert.That(NetworkManager.Singleton, Is.Null);
            var prefab = Resources.Load<GameObject>("PlayerHealthNetworkTestPlayer");
            Assert.That(prefab, Is.Not.Null);
            ushort port;
            using (var reservation = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0)))
                port = (ushort)((IPEndPoint)reservation.Client.LocalEndPoint).Port;

            _server = CreatePeer("HealthServer", port, prefab, out _serverPrefabs);
            Assert.That(_server.StartServer(), Is.True);
            var serverClock = new NetworkSimulationClock(30);
            serverClock.ResetSession(10);
            _serverHealth = _server.gameObject.AddComponent<PlayerHealthRuntimeService>();
            _serverHealth.Initialize(_server, serverClock);
            var spawned = Object.Instantiate(prefab);
            spawned.GetComponent<NetworkObject>().Spawn();
            ulong objectId = spawned.GetComponent<NetworkObject>().NetworkObjectId;

            // 玩家已经存在后客户端才开始连接；服务先于首个 Spawn 回调建立。
            _client = CreatePeer("HealthClient", port, prefab, out _clientPrefabs);
            Assert.That(_client.StartClient(), Is.True);
            var clientClock = new NetworkSimulationClock(30);
            clientClock.ResetSession(10);
            _clientHealth = _client.gameObject.AddComponent<PlayerHealthRuntimeService>();
            _clientHealth.Initialize(_client, clientClock);
            yield return WaitFor(() => _client.IsConnectedClient &&
                _client.SpawnManager.SpawnedObjects.ContainsKey(objectId), "晚加入玩家完整 Spawn");
            var remote = _client.SpawnManager.SpawnedObjects[objectId].GetComponent<PlayerHealthNetworkState>();
            yield return WaitFor(() => remote.TryGetHealth(out _), "晚加入生命完整快照");
            Assert.That(remote.TryGetHealth(out var initial), Is.True);
            Assert.That(initial.CurrentHealth, Is.EqualTo(100));
            Assert.That(initial.CurrentShield, Is.Zero);
            Assert.That(initial.Definition.MaxShield, Is.EqualTo(50));

            Assert.That(_serverHealth.TryAddShield(objectId, 50), Is.True);
            Assert.That(_serverHealth.TryDamage(objectId, 80), Is.True);
            serverClock.AdvanceOneTick();
            yield return WaitFor(() => remote.TryGetHealth(out var state) &&
                state.CurrentHealth == 70 && state.CurrentShield == 0, "伤害快照下行");
            Assert.That(_serverHealth.TryHeal(objectId, 20), Is.True);
            serverClock.AdvanceOneTick();
            yield return WaitFor(() => remote.TryGetHealth(out var state) && state.CurrentHealth == 90,
                "治疗快照下行");
        }

        private static NetworkManager CreatePeer(string name, ushort port, GameObject prefab,
            out NetworkPrefabsList list)
        {
            var go = new GameObject(name);
            var manager = go.AddComponent<NetworkManager>();
            var transport = go.AddComponent<UnityTransport>();
            transport.SetConnectionData("127.0.0.1", port, "127.0.0.1");
            manager.NetworkConfig = new NetworkConfig
            { NetworkTransport = transport, TickRate = 60, EnableSceneManagement = false };
            list = ScriptableObject.CreateInstance<NetworkPrefabsList>();
            list.Add(new NetworkPrefab { Prefab = prefab });
            manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Add(list);
            return manager;
        }

        private static IEnumerator WaitFor(Func<bool> condition, string operation)
        {
            double deadline = Time.realtimeSinceStartupAsDouble + 5;
            while (!condition())
            {
                Assert.That(Time.realtimeSinceStartupAsDouble, Is.LessThan(deadline), operation + " 超时");
                yield return null;
            }
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_clientHealth != null) yield return _clientHealth.ShutdownAsync(default).ToCoroutine();
            if (_serverHealth != null) yield return _serverHealth.ShutdownAsync(default).ToCoroutine();
            if (_server != null && _server.IsListening && _client != null && _client.IsConnectedClient)
            {
                _server.DisconnectClient(_client.LocalClientId);
                yield return WaitFor(() => !_client.IsListening && !_client.ShutdownInProgress, "生命测试客户端断开");
            }
            if (_client != null && _client.IsListening) _client.Shutdown();
            if (_server != null && _server.IsListening) _server.Shutdown();
            if (_client != null) Object.Destroy(_client.gameObject);
            if (_server != null) Object.Destroy(_server.gameObject);
            if (_clientPrefabs != null) Object.Destroy(_clientPrefabs);
            if (_serverPrefabs != null) Object.Destroy(_serverPrefabs);
            yield return null;
        }
    }
}
