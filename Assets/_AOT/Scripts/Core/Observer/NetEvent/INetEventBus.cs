using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace ProjectGame.HotFix.Core.NetworkEvents
{
    public interface INetEventBus
    {
        bool IsInitialized { get; }

        void Initialize(NetworkManager networkManager);

        void Shutdown();

        void Register<TEvent>()
            where TEvent : struct, INetEvent;

        IDisposable SubscribeRequest<TEvent>(
            NetEventHandler<TEvent> handler)
            where TEvent : struct, INetEvent;

        IDisposable SubscribeBroadcast<TEvent>(
            NetEventHandler<TEvent> handler)
            where TEvent : struct, INetEvent;

        void SendRequestToServer<TEvent>(
            TEvent eventData,
            NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
            where TEvent : struct, INetEvent;

        void BroadcastFromServer<TEvent>(
            TEvent eventData,
            NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
            where TEvent : struct, INetEvent;

        void SendToClient<TEvent>(
            ulong clientId,
            TEvent eventData,
            NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
            where TEvent : struct, INetEvent;

        void SendToClients<TEvent>(
            IReadOnlyList<ulong> clientIds,
            TEvent eventData,
            NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
            where TEvent : struct, INetEvent;
    }
}