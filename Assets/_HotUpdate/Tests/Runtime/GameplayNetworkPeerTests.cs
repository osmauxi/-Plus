using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
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
    /// <summary>
    /// 同一测试进程中的两个真实 NGO/UTP Peer，各自拥有 Bootstrap、Clock 和 PlayerSyncTransport。
    /// 协议端点用于观察收发结果，不使用只能持有一个会话的 GameNetworkRuntime 静态入口。
    /// </summary>
    public sealed class GameplayNetworkPeerTests
    {
        private NetworkManager _server;
        private NetworkManager _client;
        private GameplayNetworkBootstrap _serverBootstrap;
        private GameplayNetworkBootstrap _clientBootstrap;
        private PlayerSyncTransport _serverSync;
        private PlayerSyncTransport _clientSync;
        private GameplayNetworkTickDriver _serverDriver;
        private GameplayNetworkTickDriver _clientDriver;

        [UnityTest]
        public IEnumerator ConnectedPeers_InitializeClockAndRoutePlayerInputFullAndDelta()
        {
            Assert.That(NetworkManager.Singleton, Is.Null);
            ushort port;
            using (var reservation = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0)))
                port = (ushort)((IPEndPoint)reservation.Client.LocalEndPoint).Port;

            _server = CreatePeer("GameplayTickServer", port);
            Assert.That(_server.StartServer(), Is.True);
            _serverBootstrap = new GameplayNetworkBootstrap(_server, new NetworkSimulationConfig(30));
            _serverBootstrap.Initialize();
            _serverDriver = _server.gameObject.AddComponent<GameplayNetworkTickDriver>();
            _serverDriver.Initialize(_serverBootstrap.Runtime.Clock);
            _serverSync = new PlayerSyncTransport(_serverBootstrap.Runtime.Transport);
            _serverSync.Initialize();
            // 故意错开两个 Runtime 的启动时刻，连接后应使用同步时间锚点。
            yield return new WaitForSecondsRealtime(0.2f);

            _client = CreatePeer("GameplayTickClient", port);
            Assert.That(_client.StartClient(), Is.True);
            _clientBootstrap = new GameplayNetworkBootstrap(_client, new NetworkSimulationConfig(30));
            Assert.Throws<InvalidOperationException>(() => _clientBootstrap.Initialize());
            Assert.That(_clientBootstrap.IsInitialized, Is.False);
            yield return WaitFor(() => _client.IsConnectedClient, "NGO 客户端连接");

            _clientBootstrap.Initialize();
            var clock = _clientBootstrap.Runtime.Clock;
            Assert.That(clock.CurrentTick, Is.EqualTo(clock.GetTickAtTime(_client.LocalTime.Time)));
            Assert.That(clock.EstimatedServerTick, Is.EqualTo(clock.GetTickAtTime(_client.ServerTime.Time)));
            Assert.That(clock.TickRate, Is.EqualTo(30));
            Assert.That(_client.NetworkConfig.TickRate, Is.EqualTo(60u));
            _clientDriver = _client.gameObject.AddComponent<GameplayNetworkTickDriver>();
            _clientDriver.Initialize(clock);
            _clientSync = new PlayerSyncTransport(_clientBootstrap.Runtime.Transport);
            _clientSync.Initialize();

            var serverEndpoint = new RecordingEndpoint();
            var ownerEndpoint = new RecordingEndpoint();
            var observerEndpoint = new RecordingEndpoint();
            const ulong ownerId = 17u;
            const ulong observerId = 18u;
            _serverSync.RegisterEndpoint(ownerId, _client.LocalClientId, serverEndpoint);
            _clientSync.RegisterEndpoint(ownerId, _client.LocalClientId, ownerEndpoint);
            _clientSync.RegisterEndpoint(observerId, NetworkManager.ServerClientId, observerEndpoint);

            PlayerInputCommand input = PlayerInputCommand.CreateNeutral(clock.CurrentTick);
            input.WorldMove = Vector2.up;
            input.ReloadRequestSequence = 4;
            _clientSync.SendInputBatch(ownerId, input);
            yield return WaitFor(() => serverEndpoint.InputCount == 1, "Player 输入上行");
            Assert.That(serverEndpoint.LastInput.Tick, Is.EqualTo(input.Tick));
            Assert.That(serverEndpoint.LastInput.WorldMove, Is.EqualTo(input.WorldMove));
            Assert.That(serverEndpoint.LastInput.ReloadRequestSequence, Is.EqualTo(4));
            Assert.That(serverEndpoint.LastSender, Is.EqualTo(_client.LocalClientId));

            var baseline = new PlayerSimulationState
            {
                Tick = _serverBootstrap.Runtime.Clock.CurrentTick,
                Position = new Vector3(1f, 2f, 3f),
                Rotation = Quaternion.identity,
            };
            var full = PlayerSnapshotPacket.CreateFull(baseline);
            _serverSync.SendSnapshot(_client.LocalClientId, ownerId, full);
            _serverSync.SendSnapshot(_client.LocalClientId, observerId, full);
            yield return WaitFor(() => ownerEndpoint.SnapshotCount == 1 && observerEndpoint.SnapshotCount == 1,
                "Owner/Observer 完整快照下行");

            var next = baseline;
            next.Tick = unchecked(baseline.Tick + 1u);
            next.Position += Vector3.right;
            var delta = PlayerSnapshotPacket.CreateDelta(next, baseline);
            _serverSync.SendSnapshot(_client.LocalClientId, ownerId, delta);
            _serverSync.SendSnapshot(_client.LocalClientId, observerId, delta);
            yield return WaitFor(() => ownerEndpoint.SnapshotCount == 2 && observerEndpoint.SnapshotCount == 2,
                "Owner/Observer 差量快照下行");
            Assert.That(ownerEndpoint.LastSnapshot.Tick, Is.EqualTo(next.Tick));
            Assert.That(ownerEndpoint.LastSnapshot.Position, Is.EqualTo(next.Position));
            Assert.That(observerEndpoint.LastSnapshot.Position, Is.EqualTo(next.Position));
            Assert.That(_clientSync.DroppedDeltaWithoutBaselineCount, Is.Zero);

            // 重新注册端点不能继承上一次 Spawn 的 Full Baseline。
            _clientSync.UnregisterEndpoint(ownerId);
            _clientSync.RegisterEndpoint(ownerId, _client.LocalClientId, ownerEndpoint);
            _serverSync.SendSnapshot(_client.LocalClientId, ownerId, delta);
            yield return WaitFor(() => _clientSync.DroppedDeltaWithoutBaselineCount == 1,
                "清理端点 Baseline 后拒绝旧 Delta");
            Assert.That(ownerEndpoint.SnapshotCount, Is.EqualTo(2));
            var replacementFull = PlayerSnapshotPacket.CreateFull(next);
            _serverSync.SendSnapshot(_client.LocalClientId, ownerId, replacementFull);
            yield return WaitFor(() => ownerEndpoint.SnapshotCount == 3, "新 Full 恢复端点同步");

            // 错误 Owner 的对象路由应拒绝，但后续合法输入仍能到达。
            const ulong forbiddenId = 19u;
            _serverSync.RegisterEndpoint(forbiddenId, _client.LocalClientId + 1u, serverEndpoint);
            _clientSync.SendInputBatch(forbiddenId, input);
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(serverEndpoint.InputCount, Is.EqualTo(1));
            input.Tick = unchecked(input.Tick + 1u);
            _clientSync.SendInputBatch(ownerId, input);
            yield return WaitFor(() => serverEndpoint.InputCount == 2, "拒绝非法路由后合法输入继续");

            uint localBefore = clock.CurrentTick;
            uint serverBefore = _serverBootstrap.Runtime.Clock.CurrentTick;
            _clientDriver.Shutdown();
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(clock.CurrentTick, Is.EqualTo(localBefore));
            Assert.That(_serverBootstrap.Runtime.Clock.CurrentTick, Is.GreaterThan(serverBefore));
        }

        private static NetworkManager CreatePeer(string name, ushort port)
        {
            var obj = new GameObject(name);
            var manager = obj.AddComponent<NetworkManager>();
            var transport = obj.AddComponent<UnityTransport>();
            transport.SetConnectionData("127.0.0.1", port, "127.0.0.1");
            manager.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = transport, TickRate = 60, EnableSceneManagement = false,
            };
            return manager;
        }

        private static IEnumerator WaitFor(Func<bool> condition, string operation)
        {
            double deadline = Time.realtimeSinceStartupAsDouble + 5d;
            while (!condition())
            {
                Assert.That(Time.realtimeSinceStartupAsDouble, Is.LessThan(deadline), operation + " 超时");
                yield return null;
            }
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            _clientDriver?.Shutdown();
            _serverDriver?.Shutdown();
            _clientSync?.Shutdown();
            _serverSync?.Shutdown();
            _clientBootstrap?.Shutdown();
            _serverBootstrap?.Shutdown();
            // 先由 Server 完成远端断开，避免双方同帧销毁 Socket 时仍向已关闭的端口发包。
            if (_server != null && _server.IsListening && _client != null && _client.IsConnectedClient)
            {
                _server.DisconnectClient(_client.LocalClientId);
                yield return WaitFor(() => !_client.IsListening && !_client.ShutdownInProgress,
                    "客户端完成远端断开");
            }
            if (_client != null && _client.IsListening) _client.Shutdown();
            if (_client != null)
                yield return WaitFor(() => !_client.IsListening && !_client.ShutdownInProgress,
                    "客户端关闭");
            if (_server != null && _server.IsListening) _server.Shutdown();
            if (_server != null)
                yield return WaitFor(() => !_server.IsListening && !_server.ShutdownInProgress,
                    "服务端关闭");
            if (_client != null) Object.Destroy(_client.gameObject);
            if (_server != null) Object.Destroy(_server.gameObject);
            yield return null;
        }

        private sealed class RecordingEndpoint : IPlayerSyncEndpoint
        {
            public int InputCount;
            public int SnapshotCount;
            public ulong LastSender;
            public PlayerInputCommand LastInput;
            public PlayerSimulationState LastSnapshot;

            public void ReceiveInputFromClient(ulong senderClientId, in PlayerInputCommand input)
            {
                InputCount++;
                LastSender = senderClientId;
                LastInput = input;
            }

            public void ReceiveServerSnapshot(in PlayerSimulationState state)
            {
                SnapshotCount++;
                LastSnapshot = state;
            }
        }
    }
}
