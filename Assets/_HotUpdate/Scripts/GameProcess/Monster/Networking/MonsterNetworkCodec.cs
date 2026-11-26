using System;
using ProjectGame.Core.Monsters;
using ProjectGame.Core.Spatial;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Profiling;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 为了底层数据结构与NGO进行解耦，不用他的序列化接口，转为自己写好这些数据的
    /// 序列化方法
    /// </summary>
    public static class MonsterNetworkCodec
    {
        private static readonly ProfilerMarker SerializeMarker =
            new ProfilerMarker("Monster.Presentation.GroupSerialize");
        private static readonly ProfilerMarker DecodeMarker =
            new ProfilerMarker("Monster.Presentation.GroupDecode");

        //已经算好的各数据结构额外头数据的大小
        private const int SpawnHeaderSize = 2;
        private const int RoomBeginSize = 24;
        private const int GroupSnapshotHeaderSize = 15;
        private const int GroupSnapshotBatchHeaderSize = 2;
        private const int GroupMemberStateSize = 6;
        private const int GroupMembershipHeaderSize = 10;

        public static int GetRoomBeginByteCount() => RoomBeginSize;

        internal static void WriteDespawn(ref FastBufferWriter writer, ulong entityId) =>
            WriteUInt64(ref writer, entityId);

        internal static ulong ReadDespawn(ref FastBufferReader reader)
        {
            if (!reader.TryBeginRead(sizeof(ulong)))
                throw new ArgumentException("Monster Despawn 数据被截断。");
            return ReadUInt64(ref reader);
        }

        public static int WriteRoomBegin(ref FastBufferWriter writer, in MonsterRoomBeginData data)
        {
            WriteSingle(ref writer, data.QuantizationOrigin.x);
            WriteSingle(ref writer, data.QuantizationOrigin.y);
            WriteSingle(ref writer, data.QuantizationSize.x);
            WriteSingle(ref writer, data.QuantizationSize.y);
            WriteSingle(ref writer, data.GroundY);
            WriteUInt32(ref writer, data.MinimumAcceptedTick);
            return RoomBeginSize;
        }

        public static MonsterRoomBeginData ReadRoomBegin(ref FastBufferReader reader)
        {
            if (!reader.TryBeginRead(RoomBeginSize))
                throw new ArgumentException("Monster RoomBegin 数据被截断。");
            Vector2 origin = new(ReadSingle(ref reader), ReadSingle(ref reader));
            Vector2 size = new(ReadSingle(ref reader), ReadSingle(ref reader));
            float groundY = ReadSingle(ref reader);
            uint tick = ReadUInt32(ref reader);
            return new MonsterRoomBeginData(origin, size, groundY, tick);
        }
        internal static int GetGroupSnapshotByteCount(int count) =>
            GroupSnapshotHeaderSize + count * GroupMemberStateSize;

        internal static int GetGroupSnapshotBatchByteCount(
            IReadOnlyList<MonsterGroupSnapshot> frames)
        {
            int length = GroupSnapshotBatchHeaderSize;
            for (int i = 0; i < frames.Count; i++)
                length += GetGroupSnapshotByteCount(frames[i].MemberCount);
            return length;
        }

        internal static void WriteGroupSnapshotBatch(
            ref FastBufferWriter writer,
            IReadOnlyList<MonsterGroupSnapshot> frames,
            IReadOnlyList<byte> intervalTicks)
        {
            using var serializeScope = SerializeMarker.Auto();
            WriteUInt16(ref writer, (ushort)frames.Count);
            for (int i = 0; i < frames.Count; i++)
                WriteGroupSnapshotPayload(ref writer, frames[i], intervalTicks[i]);
        }

        private static void WriteGroupSnapshotPayload(
            ref FastBufferWriter writer, MonsterGroupSnapshot frame, byte intervalTicks)
        {
            int memberCount = frame.MemberCount;

            WriteUInt32(ref writer, frame.GroupId);
            WriteUInt32(ref writer, frame.MembershipRevision);
            WriteUInt32(ref writer, frame.ServerTick);
            WriteUInt16(ref writer, (ushort)memberCount);
            writer.WriteByte(intervalTicks);

            MonsterPackedState[] states = frame.MemberBuffer;
            for (int i = 0; i < memberCount; i++)
            {
                MonsterPackedState state = states[i];
                WriteUInt32(ref writer, state.PackedPose);
                WriteUInt16(ref writer, state.PackedEvents);
            }
        }

        internal static int ReadGroupSnapshotBatchHeader(ref FastBufferReader reader)
        {
            if (!reader.TryBeginRead(GroupSnapshotBatchHeaderSize))
                throw new ArgumentException("Monster GroupSnapshot 批次头不完整。");
            return ReadUInt16(ref reader);
        }
        internal static int GetGroupMembershipByteCount(int count) => GroupMembershipHeaderSize + count * sizeof(ulong);
        /// <summary>
        /// 单个组的成员表序列化方法，包含群组ID，当前组版本，和成员身份列表。
        /// 不传输具体成员状态，给客户端通过身份映射找到本地表现槽位。
        /// </summary>
        internal static void WriteGroupMembership(
            ref FastBufferWriter writer,
            uint groupId,
            uint membershipRevision,
            ReadOnlySpan<ulong> memberEntityIds)
        {
            WriteUInt32(ref writer, groupId);
            WriteUInt32(ref writer, membershipRevision);
            WriteUInt16(ref writer, (ushort)memberEntityIds.Length);

            foreach (ulong entityId in memberEntityIds)
                WriteUInt64(ref writer, entityId);
        }

        internal static int ReadGroupMembershipHeader(
            ref FastBufferReader reader,
            out uint groupId,
            out uint membershipRevision)
        {
            if (!reader.TryBeginRead(GroupMembershipHeaderSize))
                throw new ArgumentException("Monster GroupMembership 包头不完整。");

            groupId = ReadUInt32(ref reader);
            membershipRevision = ReadUInt32(ref reader);
            int count = ReadUInt16(ref reader);
            if (count > MonsterNetworkGroupKernel.GroupCapacity || !reader.TryBeginRead(count * sizeof(ulong)))
                throw new ArgumentException("Monster GroupMembership 成员表超出缓冲范围。");

            return count;
        }

        internal static void ReadGroupMembershipEntityIds(ref FastBufferReader reader, Span<ulong> memberEntityIds)
        {
            for (int i = 0; i < memberEntityIds.Length; i++)
                memberEntityIds[i] = ReadUInt64(ref reader);
        }

        internal static void ReadGroupSnapshotInto(
            ref FastBufferReader reader, MonsterGroupSnapshot destination, out byte intervalTicks)
        {
            using var decodeScope = DecodeMarker.Auto();
            if (!reader.TryBeginRead(GroupSnapshotHeaderSize))
                throw new ArgumentException("Monster GroupSnapshot 包头不完整。");

            uint groupId = ReadUInt32(ref reader);
            uint membershipRevision = ReadUInt32(ref reader);
            uint serverTick = ReadUInt32(ref reader);
            int memberCount = ReadUInt16(ref reader);
            reader.ReadByte(out intervalTicks);

            if (intervalTicks == 0 || memberCount == 0 || memberCount > MonsterNetworkGroupKernel.GroupCapacity ||
                !reader.TryBeginRead(memberCount * GroupMemberStateSize))
                throw new ArgumentException("Monster GroupSnapshot 发送间隔、成员数量或包体长度非法。");

            destination.BeginWrite(groupId, membershipRevision, serverTick, memberCount);
            MonsterPackedState[] states = destination.MemberBuffer;

            for (int i = 0; i < memberCount; i++)
            {
                uint packedPose = ReadUInt32(ref reader);
                ushort packedEvents = ReadUInt16(ref reader);
                states[i] = new MonsterPackedState(packedPose, packedEvents);
            }
        }


        public static int GetSpawnByteCount(MonsterSpawnBatch batch) =>
            SpawnHeaderSize + batch.Count * 14;

        public static void WriteSpawn(ref FastBufferWriter writer, MonsterSpawnBatch batch)
        {
            WriteUInt16(ref writer, (ushort)batch.Count);

            for (int i = 0; i < batch.Count; i++)
            {
                MonsterSpawnData item = batch.Items[i];
                WriteUInt64(ref writer, item.EntityId);
                WriteUInt16(ref writer, item.ConfigIndex);
                WriteUInt32(ref writer, item.PackedPose);
            }
        }

        public static MonsterSpawnBatch ReadSpawn(ref FastBufferReader reader)
        {
            if (!reader.TryBeginRead(SpawnHeaderSize))
                throw new ArgumentException("Monster Spawn 包头被截断。");

            ushort count = ReadUInt16(ref reader);
            if (!reader.TryBeginRead(count * 14))
                throw new ArgumentException("Monster Spawn 成员数据被截断。");
            var items = new MonsterSpawnData[count];

            for (int i = 0; i < count; i++)
                items[i] = new MonsterSpawnData(
                    ReadUInt64(ref reader), ReadUInt16(ref reader), ReadUInt32(ref reader));

            return new MonsterSpawnBatch(items);
        }
        private static void WriteUInt16(ref FastBufferWriter writer,ushort value)
        {
            //16bit两字节，0X1234会先写34再写12，低有效字节在前
            writer.WriteByte((byte)value);
            writer.WriteByte((byte)(value >> 8));
        }

        private static ushort ReadUInt16(ref FastBufferReader reader)
        {
            //buffer[cursor + 1]左移4位跟原始四位取或拼一起获得原始数据
            reader.ReadByte(out byte b0);
            reader.ReadByte(out byte b1);
            return (ushort)(b0 | b1 << 8);
        }

        private static void WriteUInt32(ref FastBufferWriter writer, uint value)
        {
            writer.WriteByte((byte)value);
            writer.WriteByte((byte)(value >> 8));
            writer.WriteByte((byte)(value >> 16));
            writer.WriteByte((byte)(value >> 24));
        }

        private static uint ReadUInt32(ref FastBufferReader reader)
        {
            reader.ReadByte(out byte b0);
            reader.ReadByte(out byte b1);
            reader.ReadByte(out byte b2);
            reader.ReadByte(out byte b3);
            return b0 | (uint)b1 << 8 | (uint)b2 << 16 | (uint)b3 << 24;
        }

        private static void WriteUInt64(ref FastBufferWriter writer, ulong value)
        {
            WriteUInt32(ref writer, (uint)value);
            WriteUInt32(ref writer, (uint)(value >> 32));
        }

        private static ulong ReadUInt64(ref FastBufferReader reader) =>
            ReadUInt32(ref reader) | (ulong)ReadUInt32(ref reader) << 32;
        /// <summary>
        /// 写浮点数直接把三十二位拆出来当int传，走的还是自己写的UInt32序列化方法
        /// 浮点最高位可能为1(负数)转uint要Unchecked
        /// </summary>
        private static void WriteSingle(ref FastBufferWriter writer, float value) =>
     WriteUInt32(ref writer, unchecked((uint)BitConverter.SingleToInt32Bits(value)));

        private static float ReadSingle(ref FastBufferReader reader) =>
       BitConverter.Int32BitsToSingle(unchecked((int)ReadUInt32(ref reader)));
    }
}
