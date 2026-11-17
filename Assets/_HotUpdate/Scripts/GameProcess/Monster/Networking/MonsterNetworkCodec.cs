using System;
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
        private const int SpawnHeaderSize = 6;
        private const int RoomBeginSize = 24;
        private const int GroupSnapshotHeaderSize = 15;
        private const int GroupSnapshotBatchHeaderSize = 2;
        private const int GroupMemberStateSize = 5;
        private const int GroupMembershipHeaderSize = 10;

        public static int GetRoomBeginByteCount() => RoomBeginSize;

        internal static void WriteDespawn(ref FastBufferWriter writer, int slot) =>
            writer.WriteValueSafe(slot);

        internal static int ReadDespawn(ref FastBufferReader reader)
        {
            reader.ReadValueSafe(out int slot);
            return slot;
        }

        public static int WriteRoomBegin(ref FastBufferWriter writer,in MonsterRoomBeginData data)
        {
            if (!writer.TryBeginWrite(RoomBeginSize))
                throw new OverflowException("Monster RoomBegin Writer 容量不足。");

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

        /// <summary>按成员表的顺序写入一组的完整状态，满组为 655 字节。</summary>
        internal static void WriteGroupSnapshot(
            ref FastBufferWriter writer, MonsterGroupSnapshot frame, byte intervalTicks, int length)
        {
            using var serializeScope = SerializeMarker.Auto();
            // Transport 按编码长度创建 Writer，这里只登记可写范围。
            writer.TryBeginWrite(length);
            WriteGroupSnapshotPayload(ref writer, frame, intervalTicks);
        }

        internal static void WriteGroupSnapshotBatch(
            ref FastBufferWriter writer,
            IReadOnlyList<MonsterGroupSnapshot> frames,
            IReadOnlyList<byte> intervalTicks,
            int length)
        {
            using var serializeScope = SerializeMarker.Auto();
            writer.TryBeginWrite(length);
            WriteUInt16(ref writer, checked((ushort)frames.Count));
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
                writer.WriteByte(state.PackedEvents);
            }
        }

        internal static int ReadGroupSnapshotBatchHeader(ref FastBufferReader reader)
        {
            return ReadUInt16(ref reader);
        }
        internal static int GetGroupMembershipByteCount(int count) => GroupMembershipHeaderSize + count * sizeof(uint);
        /// <summary>
        /// 单个组的成员表序列化方法，包含群组ID，当前组版本，和成员槽位列表
        /// 不传输具体成员状态，给客户端判定当前组对应槽位，未变时可直接复用
        /// </summary>
        internal static void WriteGroupMembership(
            ref FastBufferWriter writer,
            uint groupId,
            uint membershipRevision,
            ReadOnlySpan<int> memberSlots,
            int length)
        {
            writer.TryBeginWrite(length);

            WriteUInt32(ref writer, groupId);
            WriteUInt32(ref writer, membershipRevision);
            WriteUInt16(ref writer, (ushort)memberSlots.Length);

            foreach (int slot in memberSlots)
                WriteUInt32(ref writer, (uint)slot);
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
            if (count > MonsterNetworkGroupIndex.GroupCapacity || !reader.TryBeginRead(count * sizeof(uint)))
                throw new ArgumentException("Monster GroupMembership 成员表超出缓冲范围。");

            return count;
        }

        internal static void ReadGroupMembershipSlots(ref FastBufferReader reader, Span<int> memberSlots)
        {
            for (int i = 0; i < memberSlots.Length; i++)
                memberSlots[i] = checked((int)ReadUInt32(ref reader));
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

            if (intervalTicks != MonsterNetworkGroupIndex.InnerInterval &&
                intervalTicks != MonsterNetworkGroupIndex.MiddleInterval &&
                intervalTicks != MonsterNetworkGroupIndex.OuterInterval)
                throw new ArgumentException("Monster GroupSnapshot 发送间隔非法。");

            if (memberCount == 0 || memberCount > MonsterNetworkGroupIndex.GroupCapacity ||
                !reader.TryBeginRead(memberCount * GroupMemberStateSize))
                throw new ArgumentException("Monster GroupSnapshot 成员数量或包体长度非法。");

            destination.BeginWrite(groupId, membershipRevision, serverTick, memberCount);
            MonsterPackedState[] states = destination.MemberBuffer;

            for (int i = 0; i < memberCount; i++)
            {
                uint packedPose = ReadUInt32(ref reader);
                reader.ReadByte(out byte packedEvents);
                states[i] = new MonsterPackedState(packedPose, packedEvents);
            }
        }


        public static int GetSpawnByteCount(MonsterSpawnBatch batch) =>
            checked(SpawnHeaderSize + (ushort)batch.Count * 6);

        public static void WriteSpawn(ref FastBufferWriter writer, MonsterSpawnBatch batch)
        {
            int length = GetSpawnByteCount(batch);
            if (!writer.TryBeginWrite(length))
                throw new OverflowException("Monster Spawn Writer 容量不足。");

            WriteUInt32(ref writer, checked((uint)batch.StartSlot));
            WriteUInt16(ref writer, checked((ushort)batch.Count));

            for (int i = 0; i < batch.Count; i++)
            {
                MonsterSpawnData item = batch.Items[i];
                WriteUInt16(ref writer, item.ConfigIndex);
                WriteUInt32(ref writer, item.PackedPose);
            }
        }

        public static MonsterSpawnBatch ReadSpawn(ref FastBufferReader reader)
        {
            if (!reader.TryBeginRead(SpawnHeaderSize))
                throw new ArgumentException("Monster Spawn 包头被截断。");

            int startSlot = checked((int)ReadUInt32(ref reader));
            ushort count = ReadUInt16(ref reader);
            if (!reader.TryBeginRead(count * 6))
                throw new ArgumentException("Monster Spawn 成员数据被截断。");
            var items = new MonsterSpawnData[count];

            for (int i = 0; i < count; i++)
                items[i] = new MonsterSpawnData(ReadUInt16(ref reader),ReadUInt32(ref reader));

            return new MonsterSpawnBatch(startSlot, items);
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
