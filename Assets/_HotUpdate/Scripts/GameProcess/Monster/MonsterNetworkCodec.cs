using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    public readonly struct MonsterRoomBeginData
    {
        public readonly Vector2 QuantizationOrigin;
        public readonly Vector2 QuantizationSize;
        public readonly float GroundY;
        public readonly uint MinimumAcceptedTick;

        public MonsterRoomBeginData(Vector2 quantizationOrigin, Vector2 quantizationSize,
            float groundY, uint minimumAcceptedTick)
        {
            if (!MonsterMath.IsFinite(quantizationOrigin) ||
                !MonsterMath.IsPositiveFinite(quantizationSize.x) ||
                !MonsterMath.IsPositiveFinite(quantizationSize.y) ||
                !MonsterMath.IsFinite(groundY))
                throw new ArgumentOutOfRangeException(nameof(quantizationSize));
            QuantizationOrigin = quantizationOrigin;
            QuantizationSize = quantizationSize;
            GroundY = groundY;
            MinimumAcceptedTick = minimumAcceptedTick;
        }
    }

    /// <summary>
    /// 与 NGO Transport 解耦的定长小端编码。PresentationFrame 头为 Tick(4B)+SlotCount(4B)，
    /// 之后是 ActiveMask 和每个 Active Slot 的 PackedPose(4B)+State(1B)。
    /// </summary>
    public static class MonsterNetworkCodec
    {
        private const int PresentationHeaderSize = 8;
        private const int SpawnHeaderSize = 6;
        private const int RoomBeginSize = 24;

        public static int GetRoomBeginByteCount() => RoomBeginSize;

        public static int WriteRoomBegin(in MonsterRoomBeginData data, byte[] destination, int offset = 0)
        {
            ValidateDestination(destination, offset, RoomBeginSize);
            int cursor = offset;
            WriteSingle(destination, ref cursor, data.QuantizationOrigin.x);
            WriteSingle(destination, ref cursor, data.QuantizationOrigin.y);
            WriteSingle(destination, ref cursor, data.QuantizationSize.x);
            WriteSingle(destination, ref cursor, data.QuantizationSize.y);
            WriteSingle(destination, ref cursor, data.GroundY);
            WriteUInt32(destination, ref cursor, data.MinimumAcceptedTick);
            return RoomBeginSize;
        }

        public static MonsterRoomBeginData ReadRoomBegin(byte[] source, int offset, int length)
        {
            ValidateSource(source, offset, length, RoomBeginSize);
            if (length != RoomBeginSize)
                throw new ArgumentException("Monster RoomBegin 长度不一致。", nameof(length));
            int cursor = offset;
            Vector2 origin = new(ReadSingle(source, ref cursor), ReadSingle(source, ref cursor));
            Vector2 size = new(ReadSingle(source, ref cursor), ReadSingle(source, ref cursor));
            float groundY = ReadSingle(source, ref cursor);
            uint tick = ReadUInt32(source, ref cursor);
            return new MonsterRoomBeginData(origin, size, groundY, tick);
        }

        public static int GetPresentationByteCount(MonsterPresentationFrame frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            return checked(PresentationHeaderSize + frame.ActiveMask.Count + frame.ActiveStates.Count * 5);
        }

        public static int WritePresentation(MonsterPresentationFrame frame, byte[] destination, int offset = 0)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            int length = GetPresentationByteCount(frame);
            ValidateDestination(destination, offset, length);
            int cursor = offset;
            WriteUInt32(destination, ref cursor, frame.ServerTick);
            WriteUInt32(destination, ref cursor, checked((uint)frame.SlotCount));
            for (int i = 0; i < frame.ActiveMask.Count; i++) destination[cursor++] = frame.ActiveMask[i];
            for (int i = 0; i < frame.ActiveStates.Count; i++)
            {
                MonsterPackedState state = frame.ActiveStates[i];
                WriteUInt32(destination, ref cursor, state.PackedPose);
                destination[cursor++] = state.State;
            }
            return length;
        }

        public static MonsterPresentationFrame ReadPresentation(byte[] source, int offset, int length)
        {
            ValidateSource(source, offset, length, PresentationHeaderSize);
            int cursor = offset;
            uint tick = ReadUInt32(source, ref cursor);
            uint slotCountValue = ReadUInt32(source, ref cursor);
            if (slotCountValue > int.MaxValue) throw new ArgumentException("Monster Presentation SlotCount 超出本地容量。", nameof(source));
            int slotCount = (int)slotCountValue;
            int maskLength = checked((slotCount + 7) / 8);
            if (length < PresentationHeaderSize + maskLength)
                throw new ArgumentException("Monster Presentation 数据被截断。", nameof(length));
            var mask = new byte[maskLength];
            int activeCount = 0;
            for (int i = 0; i < maskLength; i++)
            {
                byte value = source[cursor++];
                mask[i] = value;
                activeCount += CountBits(value);
            }
            int expectedLength = checked(PresentationHeaderSize + maskLength + activeCount * 5);
            if (length != expectedLength) throw new ArgumentException("Monster Presentation 长度与 ActiveMask 不一致。", nameof(length));
            var states = new MonsterPackedState[activeCount];
            for (int i = 0; i < states.Length; i++)
                states[i] = new MonsterPackedState(ReadUInt32(source, ref cursor), source[cursor++]);
            return new MonsterPresentationFrame(tick, slotCount, mask, states);
        }

        public static int GetSpawnByteCount(MonsterSpawnBatch batch)
        {
            if (batch == null) throw new ArgumentNullException(nameof(batch));
            if (batch.Count > ushort.MaxValue) throw new ArgumentException("单个 Monster SpawnBatch 超过 ushort Count。", nameof(batch));
            return checked(SpawnHeaderSize + batch.Count * 6);
        }

        public static int WriteSpawn(MonsterSpawnBatch batch, byte[] destination, int offset = 0)
        {
            if (batch == null) throw new ArgumentNullException(nameof(batch));
            int length = GetSpawnByteCount(batch);
            ValidateDestination(destination, offset, length);
            int cursor = offset;
            WriteUInt32(destination, ref cursor, checked((uint)batch.StartSlot));
            WriteUInt16(destination, ref cursor, checked((ushort)batch.Count));
            for (int i = 0; i < batch.Count; i++)
            {
                MonsterSpawnData item = batch.Items[i];
                WriteUInt16(destination, ref cursor, item.ConfigIndex);
                WriteUInt32(destination, ref cursor, item.PackedPose);
            }
            return length;
        }

        public static MonsterSpawnBatch ReadSpawn(byte[] source, int offset, int length)
        {
            ValidateSource(source, offset, length, SpawnHeaderSize);
            int cursor = offset;
            uint startSlotValue = ReadUInt32(source, ref cursor);
            if (startSlotValue > int.MaxValue) throw new ArgumentException("Monster Spawn StartSlot 超出本地容量。", nameof(source));
            ushort count = ReadUInt16(source, ref cursor);
            int expectedLength = checked(SpawnHeaderSize + count * 6);
            if (length != expectedLength) throw new ArgumentException("Monster SpawnBatch 长度与 Count 不一致。", nameof(length));
            var items = new MonsterSpawnData[count];
            for (int i = 0; i < count; i++)
                items[i] = new MonsterSpawnData(ReadUInt16(source, ref cursor), ReadUInt32(source, ref cursor));
            return new MonsterSpawnBatch((int)startSlotValue, items);
        }

        private static int CountBits(byte value)
        {
            int count = 0;
            while (value != 0) { value &= (byte)(value - 1); count++; }
            return count;
        }

        private static void ValidateDestination(byte[] buffer, int offset, int length)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || length < 0 || offset > buffer.Length - length)
                throw new ArgumentOutOfRangeException(nameof(offset));
        }

        private static void ValidateSource(byte[] buffer, int offset, int length, int minimumLength)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || length < minimumLength || offset > buffer.Length - length)
                throw new ArgumentOutOfRangeException(nameof(offset));
        }

        private static void WriteUInt16(byte[] buffer, ref int cursor, ushort value)
        {
            buffer[cursor++] = (byte)value;
            buffer[cursor++] = (byte)(value >> 8);
        }

        private static ushort ReadUInt16(byte[] buffer, ref int cursor)
        {
            ushort value = (ushort)(buffer[cursor] | buffer[cursor + 1] << 8);
            cursor += 2;
            return value;
        }

        private static void WriteUInt32(byte[] buffer, ref int cursor, uint value)
        {
            buffer[cursor++] = (byte)value;
            buffer[cursor++] = (byte)(value >> 8);
            buffer[cursor++] = (byte)(value >> 16);
            buffer[cursor++] = (byte)(value >> 24);
        }

        private static uint ReadUInt32(byte[] buffer, ref int cursor)
        {
            uint value = buffer[cursor] | (uint)buffer[cursor + 1] << 8 |
                (uint)buffer[cursor + 2] << 16 | (uint)buffer[cursor + 3] << 24;
            cursor += 4;
            return value;
        }

        private static void WriteSingle(byte[] buffer, ref int cursor, float value) =>
            WriteUInt32(buffer, ref cursor, unchecked((uint)BitConverter.SingleToInt32Bits(value)));

        private static float ReadSingle(byte[] buffer, ref int cursor) =>
            BitConverter.Int32BitsToSingle(unchecked((int)ReadUInt32(buffer, ref cursor)));
    }
}
