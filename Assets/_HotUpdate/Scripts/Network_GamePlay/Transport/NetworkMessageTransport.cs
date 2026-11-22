using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Profiling;

namespace ProjectGame.HotFix.Gameplay.Network
{
    public delegate void NetworkMessageHandler(ulong senderClientId, FastBufferReader reader);

    /// <summary>
    /// Gameplay 通用消息传输层。
    /// 只负责注册、路由、发送、Delivery 映射和 Payload 统计，不理解任何业务消息格式。
    /// </summary>
    public sealed class NetworkMessageTransport
    {
        private readonly NetworkManager _networkManager;
        private readonly NetworkTransportStats _stats;
        private static readonly ProfilerMarker SendNamedMessageMarker =
            new("Monster.Network.SendNamedMessage");
        private static readonly ProfilerMarker SendUnnamedMessageMarker =
            new("Monster.Network.SendUnnamedMessage");
        private static readonly ProfilerMarker RecordSentMarker =
            new("Monster.Network.RecordSent");
        private readonly Dictionary<string, NetworkMessageHandler> _handlers = new(StringComparer.Ordinal);
        private readonly Dictionary<byte, NetworkMessageHandler> _routedUnnamedHandlers = new();
        private NetworkMessageHandler _unnamedHandler;
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
                if (_unnamedHandler != null)
                    messaging.OnUnnamedMessage -= DispatchUnnamed;
            }

            _handlers.Clear();
            _routedUnnamedHandlers.Clear();
            _unnamedHandler = null;
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

        public void RegisterUnnamedHandler(NetworkMessageHandler handler)
        {
            EnsureInitialized();
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            if (_unnamedHandler != null)
                throw new InvalidOperationException("Gameplay Unnamed Message 处理器已经注册。");

            bool wasEmpty = _routedUnnamedHandlers.Count == 0;
            _unnamedHandler = handler;
            if (wasEmpty)
                _networkManager.CustomMessagingManager.OnUnnamedMessage += DispatchUnnamed;
        }

        public void RegisterUnnamedHandler(byte channel, NetworkMessageHandler handler)
        {
            EnsureInitialized();
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            if (_routedUnnamedHandlers.ContainsKey(channel))
                throw new InvalidOperationException($"Gameplay Unnamed Message 通道 {channel} 已经注册。");

            bool wasEmpty = _unnamedHandler == null && _routedUnnamedHandlers.Count == 0;
            _routedUnnamedHandlers.Add(channel, handler);
            if (wasEmpty)
                _networkManager.CustomMessagingManager.OnUnnamedMessage += DispatchUnnamed;
        }

        public void UnregisterUnnamedHandler()
        {
            EnsureInitialized();
            if (_unnamedHandler == null)
                return;

            CustomMessagingManager messaging = _networkManager.CustomMessagingManager;
            if (messaging != null && _routedUnnamedHandlers.Count == 0)
                messaging.OnUnnamedMessage -= DispatchUnnamed;
            _unnamedHandler = null;
        }

        public void UnregisterUnnamedHandler(byte channel)
        {
            EnsureInitialized();
            _routedUnnamedHandlers.Remove(channel);
            if (_unnamedHandler == null && _routedUnnamedHandlers.Count == 0)
                _networkManager.CustomMessagingManager.OnUnnamedMessage -= DispatchUnnamed;
        }

        public void SendToServer(string messageName,FastBufferWriter writer,NetworkDeliveryClass delivery)
        {
            EnsureInitialized();
            ValidateMessageName(messageName);

            if (!_networkManager.IsClient)
                return;

            using (SendNamedMessageMarker.Auto())
                _networkManager.CustomMessagingManager.SendNamedMessage(
                    messageName,
                    NetworkManager.ServerClientId,
                    writer,
                    ResolveDelivery(delivery));

            using (RecordSentMarker.Auto())
                _stats.RecordSent(messageName, writer.Length);
        }

        public void SendToClient(ulong clientId,string messageName,FastBufferWriter writer,NetworkDeliveryClass delivery)
        {
            EnsureInitialized();
            ValidateMessageName(messageName);

            if (!_networkManager.IsServer)
                return;

            using (SendNamedMessageMarker.Auto())
                _networkManager.CustomMessagingManager.SendNamedMessage(
                    messageName,
                    clientId,
                    writer,
                    ResolveDelivery(delivery));

            using (RecordSentMarker.Auto())
                _stats.RecordSent(messageName, writer.Length);
        }

        public void SendUnnamedToClient(
            ulong clientId,
            string statsName,
            FastBufferWriter writer,
            NetworkDeliveryClass delivery)
        {
            EnsureInitialized();
            ValidateMessageName(statsName);

            if (!_networkManager.IsServer)
                return;

            using (SendUnnamedMessageMarker.Auto())
                _networkManager.CustomMessagingManager.SendUnnamedMessage(
                    clientId,
                    writer,
                    ResolveDelivery(delivery));

            using (RecordSentMarker.Auto())
                _stats.RecordSent(statsName, writer.Length);
        }

        public void SendToClients(IReadOnlyList<ulong> clientIds, string messageName, FastBufferWriter writer, NetworkDeliveryClass delivery)
        {
            EnsureInitialized();
            ValidateMessageName(messageName);
            if (!_networkManager.IsServer) 
                return;
            if (clientIds.Count == 0) 
                return;

            using (SendNamedMessageMarker.Auto())
                _networkManager.CustomMessagingManager.SendNamedMessage(messageName, clientIds, writer, ResolveDelivery(delivery));

            using (RecordSentMarker.Auto())
                for (int i = 0; i < clientIds.Count; i++)
                    _stats.RecordSent(messageName, writer.Length);
        }

        public void SendUnnamedToClients(
            IReadOnlyList<ulong> clientIds,
            string statsName,
            FastBufferWriter writer,
            NetworkDeliveryClass delivery)
        {
            EnsureInitialized();
            ValidateMessageName(statsName);
            if (!_networkManager.IsServer || clientIds.Count == 0)
                return;

            if (clientIds.Count == 1)
            {
                using (SendUnnamedMessageMarker.Auto())
                    _networkManager.CustomMessagingManager.SendUnnamedMessage(
                        clientIds[0],
                        writer,
                        ResolveDelivery(delivery));

                using (RecordSentMarker.Auto())
                    _stats.RecordSent(statsName, writer.Length);
                return;
            }

            using (SendUnnamedMessageMarker.Auto())
                _networkManager.CustomMessagingManager.SendUnnamedMessage(
                    clientIds,
                    writer,
                    ResolveDelivery(delivery));

            using (RecordSentMarker.Auto())
                for (int i = 0; i < clientIds.Count; i++)
                    _stats.RecordSent(statsName, writer.Length);
        }

        public void SendToRemoteClients(string messageName, FastBufferWriter writer, NetworkDeliveryClass delivery)
        {
            SendToClients(_remoteClientIds, messageName, writer, delivery);
        }

        public void SendUnnamedToRemoteClients(string statsName, FastBufferWriter writer, NetworkDeliveryClass delivery)
        {
            SendUnnamedToClients(_remoteClientIds, statsName, writer, delivery);
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

                case NetworkDeliveryClass.UnreliableSnapshot:
                    return NetworkDelivery.Unreliable;

                case NetworkDeliveryClass.FullSnapshot:
                case NetworkDeliveryClass.ReliableFragmentedEvent:
                    return NetworkDelivery.ReliableFragmentedSequenced;

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
            _stats.RecordReceived(messageName, reader.Length);
            if (_handlers.TryGetValue(messageName, out NetworkMessageHandler handler))
                handler(senderClientId, reader);
        }

        private void DispatchUnnamed(ulong senderClientId, FastBufferReader reader)
        {
            const string unnamedMessageName = "PG.Unnamed";
            _stats.RecordReceived(unnamedMessageName, reader.Length);

            int startPosition = reader.Position;
            if (!reader.TryBeginRead(1))
            {
                _unnamedHandler?.Invoke(senderClientId, reader);
                return;
            }
            reader.ReadByte(out byte channel);
            if (_routedUnnamedHandlers.TryGetValue(channel, out NetworkMessageHandler routedHandler))
            {
                routedHandler(senderClientId, reader);
                return;
            }

            reader.Seek(startPosition);
            _unnamedHandler?.Invoke(senderClientId, reader);
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
