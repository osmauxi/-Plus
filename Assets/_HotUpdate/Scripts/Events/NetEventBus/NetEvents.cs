using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace ProjectGame.HotFix.Core.NetworkEvents
{
    public static class NetEvents
    {
        private static INetEventBus _bus = new NetEventBus();

        public static INetEventBus Bus => _bus;

        public static void SetBus(INetEventBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public static void Initialize(NetworkManager networkManager)
        {
            _bus.Initialize(networkManager);
        }

        public static void Shutdown()
        {
            _bus.Shutdown();
        }

        public static void Register<TEvent>()
            where TEvent : struct, INetEvent
        {
            _bus.Register<TEvent>();
        }

        public static IDisposable SubscribeRequest<TEvent>(
            NetEventHandler<TEvent> handler)
            where TEvent : struct, INetEvent
        {
            return _bus.SubscribeRequest(handler);
        }

        public static IDisposable SubscribeBroadcast<TEvent>(
            NetEventHandler<TEvent> handler)
            where TEvent : struct, INetEvent
        {
            return _bus.SubscribeBroadcast(handler);
        }

        public static void SendRequestToServer<TEvent>(
            TEvent eventData,
            NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
            where TEvent : struct, INetEvent
        {
            _bus.SendRequestToServer(eventData, delivery);
        }

        public static void BroadcastFromServer<TEvent>(
            TEvent eventData,
            NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
            where TEvent : struct, INetEvent
        {
            _bus.BroadcastFromServer(eventData, delivery);
        }

        public static void SendToClient<TEvent>(
            ulong clientId,
            TEvent eventData,
            NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
            where TEvent : struct, INetEvent
        {
            _bus.SendToClient(clientId, eventData, delivery);
        }

        public static void SendToClients<TEvent>(
            IReadOnlyList<ulong> clientIds,
            TEvent eventData,
            NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
            where TEvent : struct, INetEvent
        {
            _bus.SendToClients(clientIds, eventData, delivery);
        }
    }
}