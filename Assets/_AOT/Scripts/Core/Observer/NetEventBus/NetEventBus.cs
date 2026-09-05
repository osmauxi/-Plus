using ProjectGame.HotFix.Core.Events;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Core.NetworkEvents
{
    public sealed class NetEventBus : INetEventBus
    {
        private readonly Dictionary<Type, ushort> _idByType = new();
        private readonly Dictionary<ushort, Type> _typeById = new();
        //每个事件都有自己独有的Dispatcher用于管理反序列化和触发，Dispatcher中维护了所有订阅的事件处理器列表
        private readonly Dictionary<ushort, IEventDispatcher> _dispatchers = new();
        private readonly List<ulong> _remoteClientIds = new();

        private readonly NetEventBusConfig _config;
        private NetworkManager _networkManager;

        public bool IsInitialized { get; private set; }

        public NetEventBus(NetEventBusConfig config = null)
        {
            _config = config ?? new NetEventBusConfig();
        }

        public void Initialize(NetworkManager networkManager)
        {
            if (IsInitialized)
            {
                return;
            }

            _networkManager = networkManager != null
                ? networkManager
                : throw new ArgumentNullException(nameof(networkManager));

            var messaging = _networkManager.CustomMessagingManager;
            if (messaging == null)
            {
                throw new InvalidOperationException(
                    "CustomMessagingManager is null. Make sure NetworkManager is properly initialized.");
            }
            //_config.RequestMessageName这种长字符串在注册进Handler之后会通过内部算法转化为ID
            //所有Send包在带有_config.RequestMessageName作为表头时都会使用这个ID作为表头发送，然后在Handler中进行匹配
            //_config.Name决定这个包走的是监听处理器还是广播处理器
            messaging.RegisterNamedMessageHandler(
                _config.RequestMessageName,
                OnRequestMessageReceived);

            messaging.RegisterNamedMessageHandler(
                _config.BroadcastMessageName,
                OnBroadcastMessageReceived);

            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (!IsInitialized || _networkManager == null)
            {
                return;
            }

            var messaging = _networkManager.CustomMessagingManager;
            if (messaging != null)
            {
                messaging.UnregisterNamedMessageHandler(_config.RequestMessageName);
                messaging.UnregisterNamedMessageHandler(_config.BroadcastMessageName);
            }

            IsInitialized = false;
            _networkManager = null;
        }

        public void Register<TEvent>() where TEvent : struct, INetEvent
        {
            var eventType = typeof(TEvent);
            var id = GetEventIdOrThrow(eventType);

            if (_idByType.TryGetValue(eventType, out var existingId))
            {
                if (existingId != id)
                {
                    throw new InvalidOperationException(
                        $"NetEvent type {eventType.FullName} has already been registered with id {existingId}, " +
                        $"but now tried to register id {id}.");
                }

                return;
            }

            if (_typeById.TryGetValue(id, out var existingType))
            {
                throw new InvalidOperationException(
                    $"Duplicated NetEventId {id}. " +
                    $"Existing type: {existingType.FullName}, new type: {eventType.FullName}");
            }

            var dispatcher = new EventDispatcher<TEvent>();

            _idByType.Add(eventType, id);
            _typeById.Add(id, eventType);
            _dispatchers.Add(id, dispatcher);
        }
        /// <summary>
        /// 订阅监听，决定谁监听这个事件的请求，服务器收到请求后会触发对应的请求事件处理器 
        /// </summary>
        public IDisposable SubscribeRequest<TEvent>(NetEventHandler<TEvent> handler) where TEvent : struct, INetEvent
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var dispatcher = GetDispatcher<TEvent>();
            return dispatcher.SubscribeRequest(handler);
        }
        /// <summary>
        /// 订阅广播，决定谁接受这个事件的广播，客户端需要接收广播 
        /// </summary>
        public IDisposable SubscribeBroadcast<TEvent>(NetEventHandler<TEvent> handler) where TEvent : struct, INetEvent
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var dispatcher = GetDispatcher<TEvent>();
            return dispatcher.SubscribeBroadcast(handler);
        }
        /// <summary>
        /// 客户端向服务器发送请求事件 服务器收到请求后会触发对应的请求事件处理器 
        /// </summary>
        //ReliableSequenced指可靠有序传输
        public void SendRequestToServer<TEvent>( TEvent eventData, NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
            where TEvent : struct, INetEvent
        {
            EnsureInitialized();
            EnsureClientOrHost();

            var eventId = GetRegisteredEventId<TEvent>();

            //如果发送请求的是Host端，直接本地巡回 
            if (_networkManager.IsServer && _networkManager.IsClient)
            {
                if (_config.InvokeHostRequestLocally)
                {
                    DispatchRequestLocally(eventId, eventData, _networkManager.LocalClientId);
                }

                return;
            }

            using var writer = CreateWriter(eventId, eventData);
            _networkManager.CustomMessagingManager.SendNamedMessage(
                _config.RequestMessageName,
                NetworkManager.ServerClientId,
                writer,
                delivery);
        }
        /// <summary>
        /// 将事件进行广播，所有客户端都会收到广播事件 
        /// </summary>
        public void BroadcastFromServer<TEvent>(TEvent eventData,NetworkDelivery delivery = NetworkDelivery.ReliableSequenced) where TEvent : struct, INetEvent
        {
            EnsureInitialized();
            EnsureServer();

            var eventId = GetRegisteredEventId<TEvent>();

            //如果当前广播对象是Host端自己，则直接本地巡回，不发包 
            if (_networkManager.IsHost && _config.InvokeHostBroadcastLocally)
            {
                DispatchBroadcastLocally(eventId, eventData, NetworkManager.ServerClientId);
            }

            using var writer = CreateWriter(eventId, eventData);

            _networkManager.CustomMessagingManager.SendNamedMessageToAll(
                _config.BroadcastMessageName,
                writer,
                delivery);
        }
        /// <summary>
        /// 发送到指定客户端，只有指定的客户端会收到事件 
        /// </summary>
        public void SendToClient<TEvent>(
            ulong clientId,
            TEvent eventData,
            NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
            where TEvent : struct, INetEvent
        {
            EnsureInitialized();
            EnsureServer();

            var eventId = GetRegisteredEventId<TEvent>();

            //如果这里发现Host自己是广播目标，则不发包直接本地巡回
            if (_networkManager.IsHost &&
                clientId == _networkManager.LocalClientId &&
                _config.InvokeHostBroadcastLocally)
            {
                DispatchBroadcastLocally(eventId, eventData, NetworkManager.ServerClientId);
                return;
            }

            using var writer = CreateWriter(eventId, eventData);

            _networkManager.CustomMessagingManager.SendNamedMessage(
                _config.BroadcastMessageName,
                clientId,
                writer,
                delivery);
        }
        /// <summary>
        /// 发送到指定客户端列表，只有指定的客户端会收到事件 
        /// </summary>
        public void SendToClients<TEvent>(
            IReadOnlyList<ulong> clientIds,
            TEvent eventData,
            NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
            where TEvent : struct, INetEvent
        {
            EnsureInitialized();
            EnsureServer();

            if (clientIds == null || clientIds.Count == 0)
            {
                return;
            }

            var eventId = GetRegisteredEventId<TEvent>();

            //过滤掉Host进行本地循环，传入remoteClientIds把纯客户端的ID传入发送包
            bool shouldInvokeHostLocally = false;

            if (_networkManager.IsHost)
            {
                for (int i = 0; i < clientIds.Count; i++)
                {
                    if (clientIds[i] == _networkManager.LocalClientId)
                    {
                        shouldInvokeHostLocally = true;
                        break;
                    }
                }
            }

            if (shouldInvokeHostLocally &&
                _config.InvokeHostBroadcastLocally)
            {
                DispatchBroadcastLocally(
                    eventId,
                    eventData,
                    NetworkManager.ServerClientId);
            }

            IReadOnlyList<ulong> recipients = clientIds;
            if (shouldInvokeHostLocally)
            {
                //主线程同步发送可安全复用该列表，避免每次群发都创建临时 List 
                _remoteClientIds.Clear();
                for (int i = 0; i < clientIds.Count; i++)
                {
                    var clientId = clientIds[i];
                    if (clientId != _networkManager.LocalClientId)
                    {
                        _remoteClientIds.Add(clientId);
                    }
                }

                if (_remoteClientIds.Count == 0)
                {
                    return;
                }

                recipients = _remoteClientIds;
            }


            using var writer = CreateWriter(eventId, eventData);

            _networkManager.CustomMessagingManager.SendNamedMessage(
                _config.BroadcastMessageName,
                recipients,
                writer,
                delivery);
        }

        private FastBufferWriter CreateWriter<TEvent>(ushort eventId, TEvent eventData) where TEvent : struct, INetEvent
        {
            var writer = new FastBufferWriter(_config.InitialWriterCapacity, Allocator.Temp,_config.MaxWriterCapacity);

            writer.WriteValueSafe(eventId);
            writer.WriteValueSafe(in eventData, default(FastBufferWriter.ForNetworkSerializable));

            return writer;
        }
        /// <summary>
        /// 管理请求信息包的收报和反序列化
        /// </summary>
        private void OnRequestMessageReceived(ulong senderClientId,FastBufferReader reader)
        {
            if (!IsInitialized || !_networkManager.IsServer)
            {
                return;
            }
            //读取事件ID
            if (!TryReadEventId(ref reader, out var eventId))
            {
                return;
            }
            //如果没有对应的事件处理器，则打印警告信息
            if (!_dispatchers.TryGetValue(eventId, out var dispatcher))
            {
                Debug.LogWarning($"Received unknown request NetEvent id: {eventId}");
                return;
            }

            dispatcher.DispatchRequest(ref reader, senderClientId);
        }

        private void OnBroadcastMessageReceived(
            ulong senderClientId,
            FastBufferReader reader)
        {
            if (!IsInitialized || !_networkManager.IsClient)
            {
                return;
            }

            if (!TryReadEventId(ref reader, out var eventId))
            {
                return;
            }

            if (!_dispatchers.TryGetValue(eventId, out var dispatcher))
            {
                Debug.LogWarning($"Received unknown broadcast NetEvent id: {eventId}");
                return;
            }

            dispatcher.DispatchBroadcast(ref reader, senderClientId);
        }

        private bool TryReadEventId(ref FastBufferReader reader,out ushort eventId)
        {
            eventId = default;

            try
            {
                reader.ReadValueSafe(out eventId);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return false;
            }
        }

        private void DispatchRequestLocally<TEvent>(
            ushort eventId,
            TEvent eventData,
            ulong senderClientId)
            where TEvent : struct, INetEvent
        {
            if (_dispatchers.TryGetValue(eventId, out var dispatcher))
            {
                ((EventDispatcher<TEvent>)dispatcher).PublishRequest(eventData, senderClientId);
            }
        }

        private void DispatchBroadcastLocally<TEvent>(ushort eventId,TEvent eventData,ulong senderClientId) where TEvent : struct, INetEvent
        {
            if (_dispatchers.TryGetValue(eventId, out var dispatcher))
            {
                ((EventDispatcher<TEvent>)dispatcher).PublishBroadcast(eventData, senderClientId);
            }
        }

        private EventDispatcher<TEvent> GetDispatcher<TEvent>() where TEvent : struct, INetEvent
        {
            var eventId = GetRegisteredEventId<TEvent>();

            if (!_dispatchers.TryGetValue(eventId, out var dispatcher))
            {
                throw new InvalidOperationException(
                    $"NetEvent {typeof(TEvent).FullName} is not registered.");
            }

            return (EventDispatcher<TEvent>)dispatcher;
        }

        private ushort GetRegisteredEventId<TEvent>() where TEvent : struct, INetEvent
        {
            var eventType = typeof(TEvent);

            if (_idByType.TryGetValue(eventType, out var id))
            {
                return id;
            }

            throw new InvalidOperationException(
                $"NetEvent {eventType.FullName} is not registered. " +
                $"Call NetEvents.Register<{eventType.Name}>() during bootstrap.");
        }

        private static ushort GetEventIdOrThrow(Type eventType)
        {
            //通过反射读取我们实现的自定义特性[NetEventId]，获取事件的唯一标识符 
            var attribute = eventType.GetCustomAttribute<NetEventIdAttribute>();

            if (attribute == null)
            {
                throw new InvalidOperationException(
                    $"NetEvent {eventType.FullName} is missing [NetEventId].");
            }

            return attribute.Id;
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized || _networkManager == null)
            {
                throw new InvalidOperationException(
                    "NetEventBus is not initialized.");
            }
        }

        private void EnsureServer()
        {
            if (!_networkManager.IsServer)
            {
                throw new InvalidOperationException(
                    "This NetEventBus operation can only be called on server.");
            }
        }

        private void EnsureClientOrHost()
        {
            if (!_networkManager.IsClient && !_networkManager.IsHost)
            {
                throw new InvalidOperationException(
                    "This NetEventBus operation can only be called on client or host.");
            }
        }

        private interface IEventDispatcher
        {
            void DispatchRequest(ref FastBufferReader reader, ulong senderClientId);

            void DispatchBroadcast(ref FastBufferReader reader, ulong senderClientId);
        }
        //每个事件都独立对应自己的一个EventDispatcher，用来专门对应反序列化和事件触发，优点如下：
        //首先是IL2CPP的AOT问题，每个事件结构体都会显式调用EventDispatcher<TEvent>，这样能避免出现泛型事件没有显示调用导致的缺失问题
        //相对于上一作的type object字典，这里避免了装箱拆箱的性能损耗 
        //看似结构体与类一一对应，占用内存更多，但实际上EventDispatcher<TEvent>是一个泛型类，
        //只有在第一次注册时才会生成对应的类实例，之后发送、接收、订阅都复用这个Dispatcher实例，内存占用主要和注册的网络事件类型数量成正比 
        private sealed class EventDispatcher<TEvent> : IEventDispatcher where TEvent : struct, INetEvent
        {
            private readonly HandlerList _requestHandlers = new();
            private readonly HandlerList _broadcastHandlers = new();

            public IDisposable SubscribeRequest(NetEventHandler<TEvent> handler)
            {
                //闭包将当前事件的取消订阅操作封装在EventSubscription中，返回给调用者，调用者可以通过Dispose取消订阅 
                return _requestHandlers.Subscribe(handler);
            }

            public IDisposable SubscribeBroadcast(NetEventHandler<TEvent> handler)
            {
                return _broadcastHandlers.Subscribe(handler);
            }

            public void DispatchRequest(ref FastBufferReader reader, ulong senderClientId)
            {
                //这是对应事件结构体的独有EventDispatcher内部，使用可以直接使用default创建一个空的结构体实例，然后通过反序列化填充数据     
                var eventData = default(TEvent);

                try
                {
                    //读出数据填入eventData
                    reader.ReadValueSafe(out eventData, default(FastBufferWriter.ForNetworkSerializable));
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    return;
                }
                
                PublishRequest(eventData, senderClientId);
            }

            public void DispatchBroadcast(ref FastBufferReader reader,ulong senderClientId)
            {
                var eventData = default(TEvent);

                try
                {
                    reader.ReadValueSafe(out eventData, default(FastBufferWriter.ForNetworkSerializable));
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    return;
                }

                PublishBroadcast(eventData, senderClientId);
            }
            /// <summary>
            /// 客户端请求事件的处理器，服务器收到请求后会触发对应的请求事件处理器 
            /// </summary>
            public void PublishRequest(TEvent eventData, ulong senderClientId)
            {
                _requestHandlers.Publish(eventData, senderClientId);
            }
            /// <summary>
            /// 服务器广播事件的处理器，所有客户端都会收到广播事件
            /// </summary>
            public void PublishBroadcast(TEvent eventData,ulong senderClientId)
            {
                _broadcastHandlers.Publish(eventData, senderClientId);
            }

            private sealed class HandlerList
            {
                private readonly List<NetEventHandler<TEvent>> _handlers = new();
                private int _dispatchDepth;
                private int _handlerCount;

                public IDisposable Subscribe(NetEventHandler<TEvent> handler)
                {
                    _handlers.Add(handler);
                    _handlerCount++;
                    return new EventSubscription(() => Remove(handler));
                }

                public void Publish(TEvent eventData,ulong senderClientId)
                {
                    //依旧线程锁，取出快照后马上还锁
                    //当前实现按主线程运行：记录派发开始时的数量，取消项先置空，派发结束后再原地压缩 
                    if (_handlerCount == 0)
                    {
                        return;
                    }

                    var publishCount = _handlers.Count;
                    _dispatchDepth++;
                    try
                    {
                        for (int i = 0; i < publishCount; i++)
                        {
                            var handler = _handlers[i];
                            if (handler == null)
                            {
                                continue;
                            }

                            try
                            {
                                handler.Invoke(eventData, senderClientId);
                            }
                            catch (Exception exception)
                            {
                                Debug.LogException(exception);
                            }
                        }
                    }
                    finally
                    {
                        _dispatchDepth--;
                        if (_dispatchDepth == 0)
                        {
                            RemoveInactiveHandlers();
                        }
                    }
                }

                private void Remove(NetEventHandler<TEvent> handler)
                {
                    for (int i = 0; i < _handlers.Count; i++)
                    {
                        if (!ReferenceEquals(_handlers[i], handler))
                        {
                            continue;
                        }

                        _handlerCount--;
                        if (_dispatchDepth == 0)
                        {
                            _handlers.RemoveAt(i);
                        }
                        else
                        {
                            _handlers[i] = null;
                        }

                        return;
                    }
                }

                private void RemoveInactiveHandlers()
                {
                    for (int i = _handlers.Count - 1; i >= 0; i--)
                    {
                        if (_handlers[i] == null)
                        {
                            _handlers.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}
