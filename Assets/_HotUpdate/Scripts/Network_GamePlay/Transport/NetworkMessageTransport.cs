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

        public bool IsInitialized { get; private set; }

        public bool IsServer => _networkManager.IsServer;

        public bool IsClient => _networkManager.IsClient;

        public NetworkMessageTransport(NetworkManager networkManager, NetworkTransportStats stats)
        {
            _networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
        }
        /// <summary>
        /// 只确认NGO依旧初始化，能够监听消息
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Initialize()
        {
            if (IsInitialized)
                return;

            if (!_networkManager.IsListening || _networkManager.CustomMessagingManager == null)
                throw new InvalidOperationException("NGO 尚未开始监听，无法初始化 Gameplay 消息传输层。");

            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (!IsInitialized)
                return;

            CustomMessagingManager messaging = _networkManager.CustomMessagingManager;
            if (messaging != null)
            {
                foreach (string messageName in _handlers.Keys)
                    messaging.UnregisterNamedMessageHandler(messageName);
            }

            _handlers.Clear();
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

            _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(messageName);
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

        private static void ValidateMessageName(string messageName)
        {
            if (string.IsNullOrWhiteSpace(messageName))
                throw new ArgumentException("消息名称不能为空。", nameof(messageName));
        }
    }
}
