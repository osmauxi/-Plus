using System;
using System.Threading;

namespace ProjectGame.HotFix.Voice
{
    /// <summary>
    /// 单写单读的 PCM 环形缓冲，主线程写入，音频线程读取。
    /// </summary>
    public sealed class PcmRingBuffer
    {
        private readonly float[] _buffer;
        private long _readCursor;
        private long _writeCursor;
        private int _resetRequested;
        private int _trimToLatestSamples = -1;

        public int Capacity => _buffer.Length;
        public int Count => (int)(Volatile.Read(ref _writeCursor) - Volatile.Read(ref _readCursor));

        public PcmRingBuffer(int capacity)
        {
            _buffer = new float[capacity];
        }

        public bool Write(float[] source, int count)
        {
            long read = Volatile.Read(ref _readCursor);
            long write = Volatile.Read(ref _writeCursor);

            if (count > _buffer.Length - (write - read))
            {
                // 缓冲溢出时让音频线程丢弃旧数据，避免跨线程直接改读游标。
                Volatile.Write(ref _resetRequested, 1);
                return false;
            }

            int writeIndex = (int)(write % _buffer.Length);
            int firstPart = Math.Min(count, _buffer.Length - writeIndex);
            Array.Copy(source, 0, _buffer, writeIndex, firstPart);

            int secondPart = count - firstPart;
            if (secondPart > 0)
                Array.Copy(source, firstPart, _buffer, 0, secondPart);

            Volatile.Write(ref _writeCursor, write + count);
            return true;
        }

        public int Read(float[] destination, int count)
        {
            if (Interlocked.Exchange(ref _resetRequested, 0) != 0)
            {
                Volatile.Write(ref _readCursor, Volatile.Read(ref _writeCursor));
                Volatile.Write(ref _trimToLatestSamples, -1);
                return 0;
            }

            long read = Volatile.Read(ref _readCursor);
            long write = Volatile.Read(ref _writeCursor);
            int trimToLatest = Interlocked.Exchange(
                ref _trimToLatestSamples,
                -1);

            if (trimToLatest >= 0 && write - read > trimToLatest)
            {
                // 只在音频线程推进读游标，避免写入线程与读取线程争抢游标。
                read = write - trimToLatest;
                Volatile.Write(ref _readCursor, read);
            }

            int available = (int)Math.Min(count, write - read);

            if (available <= 0)
                return 0;

            int readIndex = (int)(read % _buffer.Length);
            int firstPart = Math.Min(available, _buffer.Length - readIndex);
            Array.Copy(_buffer, readIndex, destination, 0, firstPart);

            int secondPart = available - firstPart;
            if (secondPart > 0)
                Array.Copy(_buffer, 0, destination, firstPart, secondPart);

            Volatile.Write(ref _readCursor, read + available);
            return available;
        }

        /// <summary>
        /// 请求音频线程丢弃旧样本，只保留队尾的指定数量。
        /// </summary>
        public void TrimToLatest(int remainingSamples)
        {
            int safeRemaining = Math.Max(
                0,
                Math.Min(remainingSamples, _buffer.Length));
            Volatile.Write(ref _trimToLatestSamples, safeRemaining);
        }

        public void Clear()
        {
            // 读游标只由音频线程修改。
            Volatile.Write(ref _resetRequested, 1);
        }
    }
}
