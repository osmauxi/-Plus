using System;
using System.Collections.Generic;

namespace ProjectGame.HotFix.Gameplay.Network
{
    /// <summary>单个消息名称的本地发送统计。</summary>
    public readonly struct NetworkMessageStats
    {
        public long MessageCount { get; }

        public long PayloadBytes { get; }

        public NetworkMessageStats(long messageCount, long payloadBytes)
        {
            MessageCount = messageCount;
            PayloadBytes = payloadBytes;
        }
    }

    /// <summary>
    /// 按Message名称累计本地发送次数与应用层Payload字节数
    /// 用于保证网络功能的可观测性和可调试性
    /// 比如可以看这个信道发送了多少消息，发送了多少字节，是否有异常的流量
    /// </summary>
    public sealed class NetworkTransportStats
    {
        private readonly Dictionary<string, NetworkMessageStats> _sentByMessage = new(StringComparer.Ordinal);

        public long TotalMessageCount { get; private set; }

        public long TotalPayloadBytes { get; private set; }
        /// <summary>
        /// 用于指定查阅某个消息名称的发送统计信息
        /// </summary>
        public bool TryGetSent(string messageName, out NetworkMessageStats stats)
        {
            if (string.IsNullOrWhiteSpace(messageName))
            {
                stats = default;
                return false;
            }

            return _sentByMessage.TryGetValue(messageName, out stats);
        }
        /// <summary>
        /// 获取当前所有消息名称的发送统计信息快照，返回一个只读字典
        /// </summary>
        public IReadOnlyDictionary<string, NetworkMessageStats> GetSentSnapshot()
        {
            return new Dictionary<string, NetworkMessageStats>(_sentByMessage, StringComparer.Ordinal);
        }

        /// <summary>
        /// 记录包的发出，并统计payload字节数等信息，只代表包发出
        /// 不保证包一定被对端收到，或者被对端处理
        /// </summary>
        internal void RecordSent(string messageName, int payloadBytes)
        {
            if (string.IsNullOrWhiteSpace(messageName))
                throw new ArgumentException("消息名称不能为空。", nameof(messageName));

            if (payloadBytes < 0)
                throw new ArgumentOutOfRangeException(nameof(payloadBytes), payloadBytes, "Payload 字节数不能为负数。");

            _sentByMessage.TryGetValue(messageName, out NetworkMessageStats current);
            _sentByMessage[messageName] = new NetworkMessageStats(
                current.MessageCount + 1L,
                current.PayloadBytes + payloadBytes);

            TotalMessageCount++;
            TotalPayloadBytes += payloadBytes;
        }

        internal void Reset()
        {
            _sentByMessage.Clear();
            TotalMessageCount = 0L;
            TotalPayloadBytes = 0L;
        }
    }
}
