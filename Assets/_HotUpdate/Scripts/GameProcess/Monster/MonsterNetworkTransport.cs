using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Network;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    public interface IMonsterNetworkReceiver
    {
        void ReceiveRoomBegin(MonsterRoomBeginData data);
        void ReceiveSpawnBatch(MonsterSpawnBatch batch);
        void ReceivePresentationFrame(MonsterPresentationFrame frame);
        void ReceiveRoomEnd();
    }

    /// <summary>Monster Named Message 边界：Spawn 可靠，Presentation 高频不可靠。</summary>
    public sealed class MonsterNetworkTransport
    {
        internal const string SpawnMessageName = "PG.Monster.Spawn";
        internal const string PresentationMessageName = "PG.Monster.Presentation";
        internal const string RoomBeginMessageName = "PG.Monster.RoomBegin";
        internal const string RoomEndMessageName = "PG.Monster.RoomEnd";
        private const int LengthPrefixSize = sizeof(int);
        private const int MaxPayloadBytes = 2 * 1024 * 1024;

        private readonly NetworkMessageTransport _transport;
        private IMonsterNetworkReceiver _receiver;
        private bool _handlersRegistered;

        public bool IsInitialized { get; private set; }

        public MonsterNetworkTransport(NetworkMessageTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public void Initialize(IMonsterNetworkReceiver receiver)
        {
            if (IsInitialized) return;
            if (!_transport.IsInitialized)
                throw new InvalidOperationException("Gameplay 通用 Transport 尚未初始化。");
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            if (_transport.IsClient)
            {
                _transport.RegisterHandler(RoomBeginMessageName, OnRoomBeginReceived);
                try
                {
                    _transport.RegisterHandler(SpawnMessageName, OnSpawnReceived);
                    _transport.RegisterHandler(PresentationMessageName, OnPresentationReceived);
                    _transport.RegisterHandler(RoomEndMessageName, OnRoomEndReceived);
                }
                catch
                {
                    _transport.UnregisterHandler(RoomBeginMessageName);
                    _transport.UnregisterHandler(SpawnMessageName);
                    _transport.UnregisterHandler(PresentationMessageName);
                    _transport.UnregisterHandler(RoomEndMessageName);
                    throw;
                }
                _handlersRegistered = true;
            }
            IsInitialized = true;
        }

        public void SendSpawn(IReadOnlyList<ulong> clientIds, MonsterSpawnBatch batch)
        {
            EnsureServer(clientIds);
            int length = MonsterNetworkCodec.GetSpawnByteCount(batch);
            byte[] payload = new byte[length];
            MonsterNetworkCodec.WriteSpawn(batch, payload);
            Send(clientIds, SpawnMessageName, payload, NetworkDeliveryClass.ReliableEvent);
        }

        public void SendRoomBegin(IReadOnlyList<ulong> clientIds, in MonsterRoomBeginData data)
        {
            EnsureServer(clientIds);
            byte[] payload = new byte[MonsterNetworkCodec.GetRoomBeginByteCount()];
            MonsterNetworkCodec.WriteRoomBegin(data, payload);
            Send(clientIds, RoomBeginMessageName, payload, NetworkDeliveryClass.ReliableEvent);
        }

        public void SendRoomEnd(IReadOnlyList<ulong> clientIds)
        {
            EnsureServer(clientIds);
            Send(clientIds, RoomEndMessageName, Array.Empty<byte>(), NetworkDeliveryClass.ReliableEvent);
        }

        public void SendPresentation(IReadOnlyList<ulong> clientIds, MonsterPresentationFrame frame)
        {
            EnsureServer(clientIds);
            int length = MonsterNetworkCodec.GetPresentationByteCount(frame);
            byte[] payload = new byte[length];
            MonsterNetworkCodec.WritePresentation(frame, payload);
            Send(clientIds, PresentationMessageName, payload, NetworkDeliveryClass.UnreliableEvent);
        }

        private void Send(IReadOnlyList<ulong> clientIds, string messageName, byte[] payload,
            NetworkDeliveryClass delivery)
        {
            if (payload.Length > MaxPayloadBytes)
                throw new InvalidOperationException($"Monster Payload 超过 {MaxPayloadBytes} bytes。");
            using FastBufferWriter writer = new(payload.Length + LengthPrefixSize,
                Allocator.Temp, payload.Length + LengthPrefixSize);
            writer.WriteValueSafe(payload.Length);
            writer.WriteBytesSafe(payload);
            for (int i = 0; i < clientIds.Count; i++)
            {
                ulong clientId = clientIds[i];
                if (clientId == NetworkManager.ServerClientId) continue;
                _transport.SendToClient(clientId, messageName, writer, delivery);
            }
        }

        private void OnSpawnReceived(ulong senderClientId, FastBufferReader reader)
        {
            if (!CanReceive(senderClientId)) return;
            try
            {
                byte[] payload = ReadPayload(reader);
                _receiver.ReceiveSpawnBatch(MonsterNetworkCodec.ReadSpawn(payload, 0, payload.Length));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[{nameof(MonsterNetworkTransport)}] Spawn 解析失败：{exception.Message}");
            }
        }

        private void OnRoomBeginReceived(ulong senderClientId, FastBufferReader reader)
        {
            if (!CanReceive(senderClientId)) return;
            try
            {
                byte[] payload = ReadPayload(reader);
                _receiver.ReceiveRoomBegin(MonsterNetworkCodec.ReadRoomBegin(payload, 0, payload.Length));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[{nameof(MonsterNetworkTransport)}] RoomBegin 解析失败：{exception.Message}");
            }
        }

        private void OnRoomEndReceived(ulong senderClientId, FastBufferReader reader)
        {
            if (!CanReceive(senderClientId)) return;
            try
            {
                byte[] payload = ReadPayload(reader);
                if (payload.Length != 0) throw new InvalidOperationException("Monster RoomEnd Payload 必须为空。");
                _receiver.ReceiveRoomEnd();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[{nameof(MonsterNetworkTransport)}] RoomEnd 解析失败：{exception.Message}");
            }
        }

        private void OnPresentationReceived(ulong senderClientId, FastBufferReader reader)
        {
            if (!CanReceive(senderClientId)) return;
            try
            {
                byte[] payload = ReadPayload(reader);
                _receiver.ReceivePresentationFrame(
                    MonsterNetworkCodec.ReadPresentation(payload, 0, payload.Length));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[{nameof(MonsterNetworkTransport)}] Presentation 解析失败：{exception.Message}");
            }
        }

        private bool CanReceive(ulong senderClientId) => _transport.IsClient &&
            senderClientId == NetworkManager.ServerClientId && _receiver != null;

        private static byte[] ReadPayload(FastBufferReader reader)
        {
            reader.ReadValueSafe(out int length);
            if (length < 0 || length > MaxPayloadBytes || length != reader.Length - reader.Position)
                throw new InvalidOperationException($"Monster Payload 长度非法：{length}。");
            var payload = new byte[length];
            reader.ReadBytesSafe(ref payload, length);
            return payload;
        }

        private void EnsureServer(IReadOnlyList<ulong> clientIds)
        {
            if (!IsInitialized) throw new InvalidOperationException("MonsterNetworkTransport 尚未初始化。");
            if (!_transport.IsServer) throw new InvalidOperationException("只有 Server 可以发送 Monster 状态。");
            if (clientIds == null) throw new ArgumentNullException(nameof(clientIds));
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;
            if (_handlersRegistered && _transport.IsInitialized)
            {
                _transport.UnregisterHandler(SpawnMessageName);
                _transport.UnregisterHandler(PresentationMessageName);
                _transport.UnregisterHandler(RoomBeginMessageName);
                _transport.UnregisterHandler(RoomEndMessageName);
            }
            _handlersRegistered = false;
            _receiver = null;
            IsInitialized = false;
        }
    }
}
