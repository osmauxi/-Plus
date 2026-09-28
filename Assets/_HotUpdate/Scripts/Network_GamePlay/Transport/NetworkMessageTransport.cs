using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

namespace ProjectGame.HotFix.Gameplay.Network
{
    public delegate void NetworkMessageHandler(ulong senderClientId, FastBufferReader reader);

    /// <summary>
    /// Gameplay 通用 Named Message 传输层。
    /// 只负责注册、路由、发送、Delivery 映射和 Payload 统计，不理解任何业务消息格式。
    /// </summary>
    public sealed class NetworkMessageTransport
    {
        private readonly NetworkManager _networkManager;
        private readonly NetworkTransportStats _stats;
        private readonly Dictionary<string, NetworkMessageHandler> _handlers = new(StringComparer.Ordinal);
        private readonly List<ulong> _connectedClientIds = new();
        private readonly List<ulong> _remoteClientIds = new();

        public bool IsInitialized { get; private set; }

        public bool IsServer => _networkManager.IsServer;

        public bool IsClient => _networkManager.IsClient;

        /// <summary>
        /// 当前 Peer 可见的全部已连接 ClientId；列表实例在会话内保持稳定
        /// </summary>
        public IReadOnlyList<ulong> ConnectedClientIds => _connectedClientIds;

        /// <summary>
        /// Server/Host 侧排除 ServerClientId 后的远端 ClientId，用于直接广播
        /// </summary>
        public IReadOnlyList<ulong> RemoteClientIds => _remoteClientIds;

        public ulong LocalClientId => _networkManager.LocalClientId;

        public ulong ServerClientId => NetworkManager.ServerClientId;

        public bool HasRemoteClients => _remoteClientIds.Count != 0;

        public NetworkMessageTransport(NetworkManager networkManager, NetworkTransportStats stats)
        {
            _networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
        }
        /// <summary>
        /// 只确认NGO依旧初始化，能够监听消息
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized)
                return;

            if (!_networkManager.IsListening || _networkManager.CustomMessagingManager == null)
                throw new InvalidOperationException("NGO 尚未开始监听，无法初始化 Gameplay 消息传输层。");

            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
            RebuildClientLists();
            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (!IsInitialized)
                return;

            _networkManager.OnClientConnectedCallback -= OnClientConnected;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;

            CustomMessagingManager messaging = _networkManager.CustomMessagingManager;
            if (messaging != null)
            {
                foreach (string messageName in _handlers.Keys)
                    messaging.UnregisterNamedMessageHandler(messageName);
            }

            _handlers.Clear();
            _connectedClientIds.Clear();
            _remoteClientIds.Clear();
            _stats.Reset();
            IsInitialized = false;
        }

        public void RegisterHandler(string messageName, NetworkMessageHandler handler)
        {
            EnsureInitialized();
            ValidateMessageName(messageName);

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (_handlers.ContainsKey(messageName))
                throw new InvalidOperationException($"Gameplay 网络消息 {messageName} 已经注册。");

            _handlers.Add(messageName, handler);
            try
            {
                //注册NGO通信，在受到messageName时，传senderClientId，reader到Dispatch方法
                _networkManager.CustomMessagingManager.RegisterNamedMessageHandler(
                    messageName,
                    (senderClientId, reader) => Dispatch(messageName, senderClientId, reader));
            }
            catch
            {
                _handlers.Remove(messageName);
                throw;
            }
        }

        public void UnregisterHandler(string messageName)
        {
            EnsureInitialized();
            ValidateMessageName(messageName);

            if (!_handlers.Remove(messageName))
                return;

            CustomMessagingManager messaging = _networkManager.CustomMessagingManager;
            if (messaging != null)
                messaging.UnregisterNamedMessageHandler(messageName);
        }

        public void SendToServer(string messageName,FastBufferWriter writer,NetworkDeliveryClass delivery)
        {
            EnsureInitialized();
            ValidateMessageName(messageName);

            if (!_networkManager.IsClient)
                return;

            _networkManager.CustomMessagingManager.SendNamedMessage(
                messageName,
                NetworkManager.ServerClientId,
                writer,
                ResolveDelivery(delivery));

            _stats.RecordSent(messageName, writer.Length);
        }

        public void SendToClient(ulong clientId,string messageName,FastBufferWriter writer,NetworkDeliveryClass delivery)
        {
            EnsureInitialized();
            ValidateMessageName(messageName);

            if (!_networkManager.IsServer)
                return;

            _networkManager.CustomMessagingManager.SendNamedMessage(
                messageName,
                clientId,
                writer,
                ResolveDelivery(delivery));

            _stats.RecordSent(messageName, writer.Length);
        }

        public void SendToClients(IReadOnlyList<ulong> clientIds, string messageName, FastBufferWriter writer, NetworkDeliveryClass delivery)
        {
            EnsureInitialized();
            ValidateMessageName(messageName);
            if (!_networkManager.IsServer) 
                return;
            if (clientIds == null) 
                throw new ArgumentNullException(nameof(clientIds));
            if (clientIds.Count == 0) 
                return;

            _networkManager.CustomMessagingManager.SendNamedMessage(messageName, clientIds, writer, ResolveDelivery(delivery));

            for (int i = 0; i < clientIds.Count; i++) 
                _stats.RecordSent(messageName, writer.Length);
        }

        public void SendToRemoteClients(string messageName, FastBufferWriter writer, NetworkDeliveryClass delivery)
        {
            SendToClients(_remoteClientIds, messageName, writer, delivery);
        }

        public static NetworkDelivery ResolveDelivery(NetworkDeliveryClass delivery)
        {
            //根据传输性质使用不同的传输方案
            switch (delivery)
            {
                case NetworkDeliveryClass.Command:
                case NetworkDeliveryClass.DeltaSnapshot:
                case NetworkDeliveryClass.UnreliableEvent:
                    return NetworkDelivery.UnreliableSequenced;

                case NetworkDeliveryClass.FullSnapshot:
                case NetworkDeliveryClass.ReliableEvent:
                    return NetworkDelivery.ReliableSequenced;

                default:
                    throw new ArgumentOutOfRangeException(nameof(delivery), delivery, "未知的 Gameplay Delivery 语义。");
            }
        }
        /// <summary>
        /// 根据messageName找出NetworkMessageHandler委托，并赋参触发。
        /// </summary>
        private void Dispatch(string messageName, ulong senderClientId, FastBufferReader reader)
        {
            if (_handlers.TryGetValue(messageName, out NetworkMessageHandler handler))
                handler(senderClientId, reader);
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException($"{nameof(NetworkMessageTransport)} 尚未初始化。");
        }

        private void RebuildClientLists()
        {
            _connectedClientIds.Clear();
            _remoteClientIds.Clear();

            IReadOnlyList<ulong> clientIds = _networkManager.ConnectedClientsIds;
            for (int i = 0; i < clientIds.Count; i++)
                AddClient(clientIds[i]);
        }

        private void OnClientConnected(ulong clientId) => AddClient(clientId);

        private void OnClientDisconnected(ulong clientId)
        {
            _connectedClientIds.Remove(clientId);
            _remoteClientIds.Remove(clientId);
        }

        private void AddClient(ulong clientId)
        {
            if (_connectedClientIds.Contains(clientId))
                return;

            _connectedClientIds.Add(clientId);
            if (_networkManager.IsServer && clientId != NetworkManager.ServerClientId)
                _remoteClientIds.Add(clientId);
        }

        private static void ValidateMessageName(string messageName)
        {
            if (string.IsNullOrWhiteSpace(messageName))
                throw new ArgumentException("消息名称不能为空。", nameof(messageName));
        }
    }
}
