using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.Bootstrap;
using ProjectGame.HotFix.SceneFlow;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace ProjectGame.HotFix.Netcode
{
    /// <summary>
    /// Lobby UI 与自动化基准共用的局域网连接入口，不负责 Profile、Ready 或场景业务。
    /// </summary>
    public static class LobbyConnectionService
    {
        public static async UniTask StartHostAsync(
            string persistentPlayerId,
            string hostAddress,
            ushort port,
            CancellationToken cancellationToken)
        {
            NetworkManager manager = await PrepareAsync(
                persistentPlayerId,
                cancellationToken);
            UnityTransport transport = RequireTransport(manager);
            transport.SetConnectionData(NormalizeHost(hostAddress), port, "0.0.0.0");
            if (!manager.StartHost())
                throw new InvalidOperationException($"Host 启动失败，UDP 端口可能被占用：{port}");
        }

        public static async UniTask StartClientAsync(
            string persistentPlayerId,
            string hostAddress,
            ushort port,
            CancellationToken cancellationToken)
        {
            NetworkManager manager = await PrepareAsync(
                persistentPlayerId,
                cancellationToken);
            UnityTransport transport = RequireTransport(manager);
            transport.SetConnectionData(NormalizeHost(hostAddress), port);
            if (!manager.StartClient())
                throw new InvalidOperationException(
                    "NGO 拒绝启动 Client，可能仍在关闭上一网络会话。");
        }

        private static async UniTask<NetworkManager> PrepareAsync(
            string persistentPlayerId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(persistentPlayerId))
                throw new ArgumentException("玩家持久 ID 不能为空。", nameof(persistentPlayerId));
            if (NetworkSessionBootstrap.Instance == null)
                throw new InvalidOperationException("NetworkSessionBootstrap 尚未创建。");

            await NetworkSessionBootstrap.Instance.PrepareConnectionAsync(cancellationToken);
            NetworkManager manager = NetworkManager.Singleton;
            if (manager == null)
                throw new InvalidOperationException("NetworkManager 尚未创建。");
            if (manager.IsListening)
                throw new InvalidOperationException("当前 Peer 已经启动网络会话。");

            manager.NetworkConfig.ConnectionData =
                LanConnectionPayload.Create(persistentPlayerId.Trim());
            return manager;
        }

        private static UnityTransport RequireTransport(NetworkManager manager)
        {
            UnityTransport transport = manager.GetComponent<UnityTransport>();
            return transport != null
                ? transport
                : throw new InvalidOperationException("NetworkManager 缺少 UnityTransport。");
        }

        private static string NormalizeHost(string hostAddress)
        {
            string value = (hostAddress ?? string.Empty).Trim().Replace("\u200B", string.Empty);
            return string.IsNullOrWhiteSpace(value) ? "127.0.0.1" : value;
        }
    }
}
