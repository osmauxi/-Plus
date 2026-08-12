using System;

namespace ProjectGame.HotFix.Gameplay.Player.Sync
{
    /// <summary>
    /// 固定容量 Tick 环形缓存。
    /// O(1) 写入 / 查询，不随运行时间增长，不产生每 Tick GC。
    /// </summary>
    public sealed class TickRingBuffer<T> where T : struct
    {
        private readonly uint[] _ticks;
        private readonly T[] _values;
        private readonly bool[] _occupied;

        public int Capacity { get; }

        public TickRingBuffer(int capacity)
        {
            if (capacity <= 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), "TickRingBuffer 容量必须大于 1。");

            Capacity = capacity;

            _ticks = new uint[capacity];
            _values = new T[capacity];
            _occupied = new bool[capacity];
        }

        public void Store(uint tick, in T value)
        {
            int index = GetIndex(tick);

            _ticks[index] = tick;
            _values[index] = value;
            _occupied[index] = true;
        }

        public bool TryGet(uint tick, out T value)
        {
            int index = GetIndex(tick);

            if (_occupied[index] && _ticks[index] == tick)
            {
                value = _values[index];
                return true;
            }

            value = default;
            return false;
        }

        public bool Contains(uint tick)
        {
            int index = GetIndex(tick);
            return _occupied[index] && _ticks[index] == tick;
        }

        public bool Remove(uint tick)
        {
            int index = GetIndex(tick);

            if (!_occupied[index] || _ticks[index] != tick)
                return false;

            _occupied[index] = false;
            _values[index] = default;
            return true;
        }

        public void Clear()
        {
            Array.Clear(_occupied, 0, _occupied.Length);
            Array.Clear(_values, 0, _values.Length);
        }

        private int GetIndex(uint tick) => (int)(tick % (uint)Capacity);
    }
}