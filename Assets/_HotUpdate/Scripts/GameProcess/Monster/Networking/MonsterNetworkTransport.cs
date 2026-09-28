using ProjectGame.HotFix.Gameplay.Network;
using System;
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

    /// <summary>
    /// Spawn可靠，Presentation高频不可靠
    /// </summary>
    public sealed class MonsterNetworkTransport
    {
        //注册四种消息类型
        internal const string SpawnMessageName = "PG.Monster.Spawn";
        internal const string PresentationMessageName = "PG.Monster.Presentation";
        internal const string RoomBeginMessageName = "PG.Monster.RoomBegin";
        internal const string RoomEndMessageName = "PG.Monster.RoomEnd";

        private const int MaxPayloadBytes = 2 * 1024 * 1024;

        private readonly NetworkMessageTransport _transport;

        //收到数据丢给谁
        private IMonsterNetworkReceiver _receiver;
        private bool _handlersRegistered;

        public bool IsInitialized { get; private set; }

        public MonsterNetworkTransport(NetworkMessageTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public void Initialize(IMonsterNetworkReceiver receiver)
        {
            if (IsInitialized) 
                return;
            if (!_transport.IsInitialized)
                throw new InvalidOperationException("Gameplay 通用 Transport 尚未初始化。");
            _receiver = receiver;
            if (_transport.IsClient)
            {
                try
                {
                    _transport.RegisterHandler(RoomBeginMessageName, OnRoomBeginReceived);
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

        public void SendSpawn(MonsterSpawnBatch batch)
        {
            EnsureServer();
            //获取字节数后生成数组，写入，发出，结构基本相同
            int length = MonsterNetworkCodec.GetSpawnByteCount(batch);
            ValidatePayloadLength(length);

            FastBufferWriter writer = new(length, Allocator.Temp, length);
            try 
            { 
                MonsterNetworkCodec.WriteSpawn(ref writer, batch);
                _transport.SendToRemoteClients(SpawnMessageName, writer, NetworkDeliveryClass.ReliableEvent);
            }
            finally
            {
                writer.Dispose();
            }
        }

        public void SendRoomBegin(in MonsterRoomBeginData data)
        {
            EnsureServer();
            int length = MonsterNetworkCodec.GetRoomBeginByteCount();
            FastBufferWriter writer = new(length, Allocator.Temp, length);
            //因为using变量不让用ref传，所以这里退而使用try finally保证释放
            try
            {
                MonsterNetworkCodec.WriteRoomBegin(ref writer, data);
                _transport.SendToRemoteClients(RoomBeginMessageName, writer, NetworkDeliveryClass.ReliableEvent);
            }
            finally
            {
                writer.Dispose();
            }
        }

        public void SendRoomEnd()
        {
            EnsureServer();
            //有可能存在NGO不接受0容量发包的情况，这里进行规避
            using FastBufferWriter writer = new(1, Allocator.Temp, 1);
            _transport.SendToRemoteClients(RoomEndMessageName, writer, NetworkDeliveryClass.ReliableEvent);
        }
        /// <summary>
        /// Presentation每帧发完整状态，走不可靠传输
        /// </summary>
        public void SendPresentation(MonsterPresentationFrame frame)
        {
            EnsureServer();
            int length = MonsterNetworkCodec.GetPresentationByteCount(frame);
            ValidatePayloadLength(length);

            FastBufferWriter writer = new(length, Allocator.Temp, length);
            try 
            {     
                MonsterNetworkCodec.WritePresentation(ref writer, frame);
                _transport.SendToRemoteClients(PresentationMessageName, writer, NetworkDeliveryClass.UnreliableEvent);
            }
            finally
            {
                writer.Dispose();
            }
        }
        private static void ValidatePayloadLength(int length)
        {
            if (length < 0 || length > MaxPayloadBytes)
                throw new InvalidOperationException($"Monster Payload 长度非法：{length}。");
        }

        /// <summary>
        /// 包收到后就丢出去
        /// </summary>
        private void OnSpawnReceived(ulong senderClientId, FastBufferReader reader)
        {
            if (!CanReceive(senderClientId)) 
                return;
            try
            {
                ValidatePayloadLength(reader.Length - reader.Position);
                _receiver.ReceiveSpawnBatch(MonsterNetworkCodec.ReadSpawn(ref reader));
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
                ValidatePayloadLength(reader.Length - reader.Position);
                _receiver.ReceiveRoomBegin(MonsterNetworkCodec.ReadRoomBegin(ref reader));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[{nameof(MonsterNetworkTransport)}] RoomBegin 解析失败：{exception.Message}");
            }
        }
        /// <summary>
        /// Roomend只是通知
        /// </summary>
        /// <param name="senderClientId"></param>
        /// <param name="reader"></param>
        private void OnRoomEndReceived(ulong senderClientId, FastBufferReader reader)
        {
            if (!CanReceive(senderClientId)) return;
            try
            {
                byte[] payload = ReadPayload(reader);
                if (payload.Length != 0) 
                    throw new InvalidOperationException("Monster RoomEnd Payload 必须为空。");
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
                ValidatePayloadLength(reader.Length - reader.Position);
                _receiver.ReceivePresentationFrame(
                    MonsterNetworkCodec.ReadPresentation(ref reader));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[{nameof(MonsterNetworkTransport)}] Presentation 解析失败：{exception.Message}");
            }
        }
        /// <summary>
        /// 是Client，消息发送者为Server，Receiver存在
        /// </summary>
        private bool CanReceive(ulong senderClientId) => _transport.IsClient &&
            senderClientId == NetworkManager.ServerClientId && _receiver != null;

        private static byte[] ReadPayload(FastBufferReader reader)
        {
            int length = reader.Length - reader.Position;

            var payload = new byte[length];
            reader.ReadBytesSafe(ref payload, length);
            return payload;
        }

        private void EnsureServer()
        {
            if (!IsInitialized) 
                throw new InvalidOperationException("MonsterNetworkTransport 尚未初始化。");
            if (!_transport.IsServer) 
                throw new InvalidOperationException("只有 Server 可以发送 Monster 状态。");
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
