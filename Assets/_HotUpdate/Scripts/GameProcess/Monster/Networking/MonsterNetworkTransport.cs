using ProjectGame.HotFix.Gameplay.Network;
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
        void ReceiveDespawn(int slot);
        void ReceiveRoomEnd();
        void ReceiveGroupMembership(ref FastBufferReader reader);
        void ReceiveGroupSnapshot(MonsterGroupSnapshot frame, byte intervalTicks);
    }

    /// <summary>
    /// 生命周期、群组成员表和对应首帧可靠有序传输，周期姿态使用不可靠传输。
    /// </summary>
    public sealed class MonsterNetworkTransport
    {
        private static readonly ProfilerMarker GroupTransportMarker =
            new("Monster.Network.GroupTransport");
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
        private readonly MonsterGroupSnapshot _receiveGroupSnapshot = new(MonsterNetworkGroupIndex.GroupCapacity);

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
                    _transport.RegisterHandler(RoomBeginMessageName, OnRoomBeginReceived);
                    _transport.RegisterHandler(SpawnMessageName, OnSpawnReceived);
                    _transport.RegisterHandler(DespawnMessageName, OnDespawnReceived);
                    _transport.RegisterHandler(RoomEndMessageName, OnRoomEndReceived);
                    _transport.RegisterHandler(GroupMembershipMessageName, OnGroupMembershipReceived);
                    _transport.RegisterHandler(GroupInitialSnapshotMessageName, OnGroupSnapshotReceived);
                    _transport.RegisterHandler(GroupSnapshotMessageName, OnGroupSnapshotReceived);

                }
                catch
                {
                    _transport.UnregisterHandler(RoomBeginMessageName);
                    _transport.UnregisterHandler(SpawnMessageName);
                    _transport.UnregisterHandler(DespawnMessageName);
                    _transport.UnregisterHandler(RoomEndMessageName);
                    _transport.UnregisterHandler(GroupMembershipMessageName);
                    _transport.UnregisterHandler(GroupInitialSnapshotMessageName);
                    _transport.UnregisterHandler(GroupSnapshotMessageName);
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
            int length = MonsterNetworkCodec.GetSpawnByteCount(batch);
            ValidatePayloadLength(length);

            FastBufferWriter writer = new(length, Allocator.Temp, length);
            try 
            { 
                MonsterNetworkCodec.WriteSpawn(ref writer, batch);
                _transport.SendToRemoteClients(SpawnMessageName, writer, NetworkDeliveryClass.FullSnapshot);
            }
            finally
            {
                writer.Dispose();
            }
        }
        internal void SendGroupMembership(IReadOnlyList<ulong> clients, MonsterNetworkGroupIndex.Group group)
        {
            using var groupTransportScope = GroupTransportMarker.Auto();
            ReadOnlySpan<int> slots = group.MemberSlots;
            int length = MonsterNetworkCodec.GetGroupMembershipByteCount(slots.Length);
            FastBufferWriter writer = new(length, Allocator.Temp, length);
            try
            {
                MonsterNetworkCodec.WriteGroupMembership(ref writer, group.Id, group.MembershipRevision, slots, length);
                _transport.SendToClients(clients, GroupMembershipMessageName, writer,NetworkDeliveryClass.ReliableEvent);
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
            int length = MonsterNetworkCodec.GetGroupSnapshotBatchByteCount(frames);
            FastBufferWriter writer = new(length, Allocator.Temp, length);
            try
            {
                MonsterNetworkCodec.WriteGroupSnapshotBatch(ref writer, frames, intervalTicks, length);
                string name = isInitialSnapshot ? GroupInitialSnapshotMessageName : GroupSnapshotMessageName;
                NetworkDeliveryClass delivery = isInitialSnapshot ? NetworkDeliveryClass.ReliableEvent : NetworkDeliveryClass.UnreliableSnapshot;
                _transport.SendToClients(clients, name, writer, delivery);
            }
            finally
            {
                writer.Dispose();
            }
        }

        internal void SendDespawn(int slot)
        {
            FastBufferWriter writer = new(sizeof(uint), Allocator.Temp, sizeof(uint));
            try
            {
                MonsterNetworkCodec.WriteDespawn(ref writer, slot);
                _transport.SendToRemoteClients(DespawnMessageName, writer, NetworkDeliveryClass.ReliableEvent);
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
            using var marker = SendRoomEndMarker.Auto();
            //有可能存在NGO不接受0容量发包的情况，这里进行规避
            using FastBufferWriter writer = new(1, Allocator.Temp, 1);
            _transport.SendToRemoteClients(RoomEndMessageName, writer, NetworkDeliveryClass.ReliableEvent);
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
            {
                _transport.UnregisterHandler(SpawnMessageName);
                _transport.UnregisterHandler(DespawnMessageName);
                _transport.UnregisterHandler(RoomBeginMessageName);
                _transport.UnregisterHandler(RoomEndMessageName);
                _transport.UnregisterHandler(GroupMembershipMessageName);
                _transport.UnregisterHandler(GroupInitialSnapshotMessageName);
                _transport.UnregisterHandler(GroupSnapshotMessageName);
            }
            _handlersRegistered = false;
            _receiver = null;
            IsInitialized = false;
        }
    }
}
