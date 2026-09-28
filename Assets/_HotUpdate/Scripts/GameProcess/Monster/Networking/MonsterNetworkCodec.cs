using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 为了底层数据结构与NGO进行解耦，不用他的序列化接口，转为自己写好这些数据的
    /// 序列化方法
    /// </summary>
    public static class MonsterNetworkCodec
    {
        //已经算好的各数据结构额外头数据的大小
        private const int PresentationHeaderSize = 8;
        private const int SpawnHeaderSize = 6;
        private const int RoomBeginSize = 24;

        public static int GetRoomBeginByteCount() => RoomBeginSize;

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
            int length = reader.Length - reader.Position;
            if (length != RoomBeginSize)
                throw new ArgumentException("Monster RoomBegin 长度不一致。");

            if (!reader.TryBeginRead(RoomBeginSize))
                throw new OverflowException("Monster RoomBegin 数据被截断。");

            Vector2 origin = new(ReadSingle(ref reader), ReadSingle(ref reader));
            Vector2 size = new(ReadSingle(ref reader), ReadSingle(ref reader));
            float groundY = ReadSingle(ref reader);
            uint tick = ReadUInt32(ref reader);
            return new MonsterRoomBeginData(origin, size, groundY, tick);
        }
        /// <summary>
        /// 因为Presentation大小主要看怪物数量，所以序列化要明确获取他当前的实际大小
        /// </summary>
        public static int GetPresentationByteCount(MonsterPresentationFrame frame)
        {
            if (frame == null) 
                throw new ArgumentNullException(nameof(frame));
            return checked(PresentationHeaderSize + frame.ActiveMask.Count + frame.ActiveStates.Count * 6);
        }

        public static void WritePresentation(ref FastBufferWriter writer, MonsterPresentationFrame frame)
        {
            if (frame == null) 
                throw new ArgumentNullException(nameof(frame));
            int length = GetPresentationByteCount(frame);
            if (!writer.TryBeginWrite(length))
                throw new OverflowException("Monster Presentation Writer 容量不足。");

            //头信息额外写了八字节的ServerTick和SlotCount
            WriteUInt32(ref writer, frame.ServerTick);
            WriteUInt32(ref writer, checked((uint)frame.SlotCount));
            //因为掩码位本身就是byte了，可以直接写
            for (int i = 0; i < frame.ActiveMask.Count; i++)
                writer.WriteByte(frame.ActiveMask[i]);
            for (int i = 0; i < frame.ActiveStates.Count; i++)
            {
                //这里直接不管Slot的顺序了，不然要传四字节的uint，只传packedState
                //顺序拿到之后自己通过ActiveMask隐式推导
                MonsterPackedState state = frame.ActiveStates[i];
                WriteUInt32(ref writer, state.PackedPose);
                writer.WriteByte(state.AttackSequence);
                writer.WriteByte(state.HitSequence);
            }
        }

        public static MonsterPresentationFrame ReadPresentation(ref FastBufferReader reader)
        {
            int length = reader.Length - reader.Position;

            if (length < PresentationHeaderSize)
                throw new ArgumentException("Monster Presentation Header 被截断。");

            if (!reader.TryBeginRead(length))
                throw new OverflowException("Monster Presentation 数据被截断。");

            uint tick = ReadUInt32(ref reader);
            uint slotCountValue = ReadUInt32(ref reader);

            if (slotCountValue > int.MaxValue) 
                throw new ArgumentException("Monster Presentation SlotCount 超出本地容量。");
            int slotCount = (int)slotCountValue;
            //向上取整
            int maskLength = checked((slotCount + 7) / 8);
            if (length < PresentationHeaderSize + maskLength)
                throw new ArgumentException("Monster Presentation 数据被截断。", nameof(length));
            var mask = new byte[maskLength];
            int activeCount = 0;
            for (int i = 0; i < maskLength; i++)
            {
                //一次循环直接写八位01到mask，统计activeCount决定后面有多少个packedState
                reader.ReadByte(out byte value);
                mask[i] = value;
                activeCount += CountBits(value);
            }
            int expectedLength = checked(PresentationHeaderSize + maskLength + activeCount * 6);
            if (length != expectedLength) 
                throw new ArgumentException("Monster Presentation 长度与 ActiveMask 不一致。", nameof(length));
            var states = new MonsterPackedState[activeCount];
            for (int i = 0; i < states.Length; i++)
            {
                uint packedPose = ReadUInt32(ref reader);
                reader.ReadByte(out byte attackSequence);
                reader.ReadByte(out byte hitSequence);
                states[i] = new MonsterPackedState(packedPose, attackSequence, hitSequence);
            }
            return new MonsterPresentationFrame(tick, slotCount, mask, states);
        }

        public static int GetSpawnByteCount(MonsterSpawnBatch batch)
        {
            if (batch == null) 
                throw new ArgumentNullException(nameof(batch));
            if (batch.Count > ushort.MaxValue) 
                throw new ArgumentException("单个 Monster SpawnBatch 超过 ushort Count。", nameof(batch));
            return checked(SpawnHeaderSize + batch.Count * 6);
        }

        public static void WriteSpawn(ref FastBufferWriter writer, MonsterSpawnBatch batch)
        {
            if (batch == null) throw new ArgumentNullException(nameof(batch));

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
            int length = reader.Length - reader.Position;

            if (length < SpawnHeaderSize)
                throw new ArgumentException("Monster Spawn Header 被截断。");

            if (!reader.TryBeginRead(length))
                throw new OverflowException("Monster Spawn 数据被截断。");

            uint startSlotValue = ReadUInt32(ref reader);

            if (startSlotValue > int.MaxValue)
                throw new ArgumentException("Monster Spawn StartSlot 超出本地容量。");

            ushort count = ReadUInt16(ref reader);

            int expectedLength = checked(SpawnHeaderSize + count * 6);

            if (length != expectedLength)
                throw new ArgumentException("Monster SpawnBatch 长度与 Count 不一致。");

            var items = new MonsterSpawnData[count];

            for (int i = 0; i < count; i++)
                items[i] = new MonsterSpawnData(ReadUInt16(ref reader),ReadUInt32(ref reader));

            return new MonsterSpawnBatch((int)startSlotValue, items);
        }
        /// <summary>
        /// 统计此byte有多少个1
        /// </summary>
        private static int CountBits(byte value)
        {
            int count = 0;
            while (value != 0) 
            {
                value &= (byte)(value - 1); 
                count++; 
            }
            return count;
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
