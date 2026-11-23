using ProjectGame.HotFix.Gameplay.Network;
using ProjectGame.Core.Spatial;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Profiling;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    public interface IMonsterNetworkReceiver
    {
        void ReceiveRoomBegin(MonsterRoomBeginData data);
        void ReceiveSpawnBatch(MonsterSpawnBatch batch);
        void ReceiveDespawn(ulong entityId);
        void ReceiveRoomEnd();
        void ReceiveGroupMembership(ref FastBufferReader reader);
        void ReceiveGroupSnapshot(MonsterGroupSnapshot frame, byte intervalTicks);
    }

    /// <summary>
    /// 生命周期、群组成员表和对应首帧可靠有序传输，周期姿态使用不可靠传输。
    /// </summary>
    public sealed class MonsterNetworkTransport
    {
        private enum MonsterNetworkMessageKind : byte
        {
            RoomBegin = 1,
            Spawn = 2,
            Despawn = 3,
            RoomEnd = 4,
            GroupMembership = 5,
            GroupInitialSnapshot = 6,
            GroupSnapshot = 7,
        }

        private static readonly ProfilerMarker GroupTransportMarker =
            new("Monster.Network.GroupTransport");
        private static readonly ProfilerMarker GroupMembershipWriteMarker =
            new("Monster.Network.GroupMembership.Write");
        private static readonly ProfilerMarker GroupSnapshotWriteMarker =
            new("Monster.Network.GroupSnapshot.Write");
        private static readonly ProfilerMarker GroupSnapshotSendMarker =
            new("Monster.Network.GroupSnapshot.Send");
        private static readonly ProfilerMarker SendRoomBeginMarker =
            new("Monster.Network.SendRoomBegin");
        private static readonly ProfilerMarker ReceiveRoomBeginMarker =
            new("Monster.Network.ReceiveRoomBegin");
        private static readonly ProfilerMarker SendSpawnMarker =
            new("Monster.Network.SendSpawn");
        private static readonly ProfilerMarker ReceiveSpawnMarker =
            new("Monster.Network.ReceiveSpawn");
        private static readonly ProfilerMarker SendRoomEndMarker =
            new("Monster.Network.SendRoomEnd");
        private static readonly ProfilerMarker ReceiveRoomEndMarker =
            new("Monster.Network.ReceiveRoomEnd");

        internal const string SpawnMessageName = "PG.Monster.Spawn";
        internal const string DespawnMessageName = "PG.Monster.Despawn";
        internal const string RoomBeginMessageName = "PG.Monster.RoomBegin";
        internal const string RoomEndMessageName = "PG.Monster.RoomEnd";
        internal const string GroupMembershipMessageName = "PG.Monster.GroupMembership";
        internal const string GroupInitialSnapshotMessageName = "PG.Monster.GroupInitialSnapshot";
        internal const string GroupSnapshotMessageName = "PG.Monster.GroupSnapshot";

        private const int MaxPayloadBytes = 2 * 1024 * 1024;

        private readonly NetworkMessageTransport _transport;
        private readonly MonsterGroupSnapshot _receiveGroupSnapshot = new(MonsterNetworkGroupKernel.GroupCapacity);

        //收到数据丢给谁
        private IMonsterNetworkReceiver _receiver;
        private bool _handlersRegistered;

        public bool IsInitialized { get; private set; }

        public MonsterNetworkTransport(NetworkMessageTransport transport)
        {
            _transport = transport;
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
                    _transport.RegisterUnnamedHandler(OnUnnamedMessageReceived);

                }
                catch
                {
                    _transport.UnregisterUnnamedHandler();
                    throw;
                }
                _handlersRegistered = true;
            }

            IsInitialized = true;
        }

        public void SendSpawn(MonsterSpawnBatch batch)
        {
            EnsureServer();
            using var marker = SendSpawnMarker.Auto();
            //获取字节数后生成数组，写入，发出，结构基本相同
            int length = 1 + MonsterNetworkCodec.GetSpawnByteCount(batch);
            ValidatePayloadLength(length);

            FastBufferWriter writer = new(length, Allocator.Temp, length);
            try 
            { 
                if (!writer.TryBeginWrite(length))
                    throw new OverflowException("Monster Spawn Writer 容量不足。");
                writer.WriteByte((byte)MonsterNetworkMessageKind.Spawn);
                MonsterNetworkCodec.WriteSpawn(ref writer, batch);
                _transport.SendUnnamedToRemoteClients(SpawnMessageName, writer, NetworkDeliveryClass.FullSnapshot);
            }
            finally
            {
                writer.Dispose();
            }
        }
        internal void SendGroupMembership(
            IReadOnlyList<ulong> clients,
            uint groupId,
            uint membershipRevision,
            ReadOnlySpan<ulong> memberEntityIds)
        {
            using var groupTransportScope = GroupTransportMarker.Auto();
            int payloadLength = MonsterNetworkCodec.GetGroupMembershipByteCount(memberEntityIds.Length);
            int length = 1 + payloadLength;
            FastBufferWriter writer = new(length, Allocator.Temp, length);
            try
            {
                if (!writer.TryBeginWrite(length))
                    throw new OverflowException("Monster GroupMembership Writer 容量不足。");
                using (GroupMembershipWriteMarker.Auto())
                {
                    writer.WriteByte((byte)MonsterNetworkMessageKind.GroupMembership);
                    MonsterNetworkCodec.WriteGroupMembership(
                        ref writer, groupId, membershipRevision, memberEntityIds, payloadLength);
                }
                _transport.SendUnnamedToClients(clients, GroupMembershipMessageName, writer,NetworkDeliveryClass.ReliableEvent);
            }
            finally
            {
                writer.Dispose();
            }
        }
        internal void SendGroupSnapshotBatch(
            IReadOnlyList<ulong> clients,
            IReadOnlyList<MonsterGroupSnapshot> frames,
            IReadOnlyList<byte> intervalTicks,
            bool isInitialSnapshot)
        {
            using var groupTransportScope = GroupTransportMarker.Auto();
            int payloadLength = MonsterNetworkCodec.GetGroupSnapshotBatchByteCount(frames);
            int length = 1 + payloadLength;
            FastBufferWriter writer = new(length, Allocator.Temp, length);
            try
            {
                if (!writer.TryBeginWrite(length))
                    throw new OverflowException("Monster GroupSnapshot Writer 容量不足。");
                using (GroupSnapshotWriteMarker.Auto())
                {
                    writer.WriteByte((byte)(isInitialSnapshot
                        ? MonsterNetworkMessageKind.GroupInitialSnapshot
                        : MonsterNetworkMessageKind.GroupSnapshot));
                    MonsterNetworkCodec.WriteGroupSnapshotBatch(ref writer, frames, intervalTicks, payloadLength);
                }
                string name = isInitialSnapshot ? GroupInitialSnapshotMessageName : GroupSnapshotMessageName;
                NetworkDeliveryClass delivery = isInitialSnapshot ? NetworkDeliveryClass.ReliableEvent : NetworkDeliveryClass.UnreliableSnapshot;
                using (GroupSnapshotSendMarker.Auto())
                    _transport.SendUnnamedToClients(clients, name, writer, delivery);
            }
            finally
            {
                writer.Dispose();
            }
        }

        internal void SendDespawn(ulong entityId)
        {
            FastBufferWriter writer = new(1 + sizeof(ulong), Allocator.Temp, 1 + sizeof(ulong));
            try
            {
                if (!writer.TryBeginWrite(1 + sizeof(ulong)))
                    throw new OverflowException("Monster Despawn Writer 容量不足。");
                writer.WriteByte((byte)MonsterNetworkMessageKind.Despawn);
                MonsterNetworkCodec.WriteDespawn(ref writer, entityId);
                _transport.SendUnnamedToRemoteClients(DespawnMessageName, writer, NetworkDeliveryClass.ReliableEvent);
            }
            finally
            {
                writer.Dispose();
            }
        }

        public void SendRoomBegin(in MonsterRoomBeginData data)
        {
            EnsureServer();
            using var marker = SendRoomBeginMarker.Auto();
            int length = 1 + MonsterNetworkCodec.GetRoomBeginByteCount();
            FastBufferWriter writer = new(length, Allocator.Temp, length);
            //因为using变量不让用ref传，所以这里退而使用try finally保证释放
            try
            {
                if (!writer.TryBeginWrite(length))
                    throw new OverflowException("Monster RoomBegin Writer 容量不足。");
                writer.WriteByte((byte)MonsterNetworkMessageKind.RoomBegin);
                MonsterNetworkCodec.WriteRoomBegin(ref writer, data);
                _transport.SendUnnamedToRemoteClients(RoomBeginMessageName, writer, NetworkDeliveryClass.ReliableEvent);
            }
            finally
            {
                writer.Dispose();
            }
        }

        public void SendRoomEnd()
        {
            EnsureServer();
            using var marker = SendRoomEndMarker.Auto();
            //有可能存在NGO不接受0容量发包的情况，这里进行规避
            using FastBufferWriter writer = new(1, Allocator.Temp, 1);
            if (!writer.TryBeginWrite(1))
                throw new OverflowException("Monster RoomEnd Writer 容量不足。");
            writer.WriteByte((byte)MonsterNetworkMessageKind.RoomEnd);
            _transport.SendUnnamedToRemoteClients(RoomEndMessageName, writer, NetworkDeliveryClass.ReliableEvent);
        }
        private static void ValidatePayloadLength(int length)
        {
            if (length < 0 || length > MaxPayloadBytes)
                throw new InvalidOperationException($"Monster Payload 长度非法：{length}。");
        }

        /// <summary>路由所有怪物 Unnamed Message，并把首字节后的 payload 交给原有解析器。</summary>
        private void OnUnnamedMessageReceived(ulong senderClientId, FastBufferReader reader)
        {
            if (!CanReceive(senderClientId))
                return;

            try
            {
                if (!reader.TryBeginRead(1))
                    throw new ArgumentException("Monster Unnamed Message 缺少消息类型。");
                reader.ReadByte(out byte kind);
                switch ((MonsterNetworkMessageKind)kind)
                {
                    case MonsterNetworkMessageKind.RoomBegin:
                        OnRoomBeginReceived(senderClientId, reader);
                        break;
                    case MonsterNetworkMessageKind.Spawn:
                        OnSpawnReceived(senderClientId, reader);
                        break;
                    case MonsterNetworkMessageKind.Despawn:
                        OnDespawnReceived(senderClientId, reader);
                        break;
                    case MonsterNetworkMessageKind.RoomEnd:
                        OnRoomEndReceived(senderClientId, reader);
                        break;
                    case MonsterNetworkMessageKind.GroupMembership:
                        OnGroupMembershipReceived(senderClientId, reader);
                        break;
                    case MonsterNetworkMessageKind.GroupInitialSnapshot:
                    case MonsterNetworkMessageKind.GroupSnapshot:
                        OnGroupSnapshotReceived(senderClientId, reader);
                        break;
                    default:
                        throw new ArgumentException($"未知的 Monster Unnamed Message 类型：{kind}。");
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[{nameof(MonsterNetworkTransport)}] Unnamed Message 解析失败：{exception.Message}");
            }
        }

        /// <summary>包收到后就丢出去。</summary>
        private void OnSpawnReceived(ulong senderClientId, FastBufferReader reader)
        {
            if (!CanReceive(senderClientId)) 
                return;
            try
            {
                using var marker = ReceiveSpawnMarker.Auto();
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
                using var marker = ReceiveRoomBeginMarker.Auto();
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
                using var marker = ReceiveRoomEndMarker.Auto();
                if (reader.Length != reader.Position)
                    throw new InvalidOperationException("Monster RoomEnd Payload 必须为空。");
                _receiver.ReceiveRoomEnd();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[{nameof(MonsterNetworkTransport)}] RoomEnd 解析失败：{exception.Message}");
            }
        }
        private void OnGroupMembershipReceived(ulong senderClientId, FastBufferReader reader)
        {
            if (!CanReceive(senderClientId))
                return;

            _receiver.ReceiveGroupMembership(ref reader);
        }
        private void OnGroupSnapshotReceived(ulong senderClientId, FastBufferReader reader)
        {
            if (!CanReceive(senderClientId))
                return;

            int count = MonsterNetworkCodec.ReadGroupSnapshotBatchHeader(ref reader);
            for (int i = 0; i < count; i++)
            {
                MonsterNetworkCodec.ReadGroupSnapshotInto(ref reader, _receiveGroupSnapshot, out byte intervalTicks);
                // 由接收者在本次调用内消费，下一条群组快照会复用缓冲。
                _receiver.ReceiveGroupSnapshot(_receiveGroupSnapshot, intervalTicks);
            }
        }

        private void OnDespawnReceived(ulong senderClientId, FastBufferReader reader)
        {
            if (!CanReceive(senderClientId))
                return;

            _receiver.ReceiveDespawn(MonsterNetworkCodec.ReadDespawn(ref reader));
        }
        private bool CanReceive(ulong senderClientId) => _transport.IsClient &&
            senderClientId == NetworkManager.ServerClientId;

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
                _transport.UnregisterUnnamedHandler();
            _handlersRegistered = false;
            _receiver = null;
            IsInitialized = false;
        }
    }
}
