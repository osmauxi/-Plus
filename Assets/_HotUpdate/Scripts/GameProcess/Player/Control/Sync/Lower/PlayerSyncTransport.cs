using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Sync
{
    /// <summary>
    /// 传输层和单个玩家同步总控之间的最小路由契约。
    /// Transport（传输层）只按 NetworkObjectId（网络对象编号）找到端点，具体如何处理输入和快照由端点决定。
    /// </summary>
    public interface IPlayerSyncEndpoint
    {
        /// <summary>把客户端输入交给服务器上对应玩家的权威模拟。</summary>
        void ReceiveInputFromClient(ulong senderClientId, in PlayerInputCommand input);

        /// <summary>把服务器权威状态交给客户端上的预测校正或远端插值模块。</summary>
        void ReceiveServerSnapshot(in PlayerSimulationState state);
    }

    /// <summary>
    /// Gameplay（游戏过程）玩家高频同步专用 Transport（传输层）。
    ///
    /// Client -> Server：
    /// Continuous Input（连续输入），UnreliableSequenced（不保证送达、但只保留较新序列）。
    ///
    /// Server -> Client：
    /// Full Snapshot（完整快照）使用 ReliableSequenced（保证送达并保持序列）；
    /// Delta Snapshot（差量快照）使用 UnreliableSequenced（允许丢包，但不让旧包覆盖新包）。
    ///
    /// Transport 只负责网络数据搬运和路由，不负责预测、模拟、插值和回滚。
    /// </summary>
    public sealed class PlayerSyncTransport
    {
        /// <summary>客户端输入使用的 NGO（Netcode for GameObjects）命名消息标识。</summary>
        private const string InputMessageName = "PG.PlayerSync.Input";

        /// <summary>服务器快照使用的 NGO（Netcode for GameObjects）命名消息标识。</summary>
        private const string SnapshotMessageName = "PG.PlayerSync.Snapshot";

        /// <summary>单个输入消息允许携带的最大输入数：当前输入加两份历史冗余输入。</summary>
        private const int MaxInputRedundancy = 3;

        /// <summary>序列化写入器的初始容量，单位为字节。</summary>
        private const int WriterInitialCapacity = 128;

        /// <summary>序列化写入器允许扩展到的最大容量，单位为字节。</summary>
        private const int WriterMaxCapacity = 512;

        /// <summary>提供命名消息、客户端列表及服务器编号的 NGO 网络管理器。</summary>
        private readonly NetworkManager _networkManager;

        /// <summary>NetworkObjectId（网络对象编号）到本机玩家同步端点的路由表。</summary>
        private readonly Dictionary<ulong, EndpointEntry> _endpoints = new();

        /// <summary>每个网络对象最近收到的 Full Snapshot（完整快照），用于还原后续 Delta Snapshot（差量快照）。</summary>
        private readonly Dictionary<ulong, PlayerSimulationState> _snapshotBaselines = new();

        /// <summary>因缺少匹配 Baseline（基准完整状态）而丢弃的差量快照数量。</summary>
        public int DroppedDeltaWithoutBaselineCount { get; private set; }

        /// <summary>本机客户端累计发送的输入消息有效载荷字节数，不包含底层协议头。</summary>
        public long InputPayloadBytesSent { get; private set; }

        /// <summary>本机服务器累计发送的快照消息有效载荷字节数，不包含底层协议头。</summary>
        public long SnapshotPayloadBytesSent { get; private set; }

        /// <summary>本机客户端累计发送的输入命名消息数量。</summary>
        public int InputMessageSendCount { get; private set; }

        /// <summary>本机服务器累计发送的快照命名消息数量；向不同客户端发送会分别计数。</summary>
        public int SnapshotMessageSendCount { get; private set; }

        /// <summary>是否已经注册当前身份所需的命名消息处理器。</summary>
        private bool _initialized;

        /// <summary>传输层是否可以开始注册端点和收发消息。</summary>
        public bool IsInitialized => _initialized;

        /// <summary>创建依附于指定网络会话的玩家同步传输层。</summary>
        public PlayerSyncTransport(NetworkManager networkManager)
        {
            _networkManager = networkManager != null ? networkManager : throw new ArgumentNullException(nameof(networkManager));
        }

        /// <summary>
        /// 根据本机身份注册消息处理器。Host（主机）同时是服务器和客户端，因此会同时注册输入与快照处理器。
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
                return;

            CustomMessagingManager messaging = _networkManager.CustomMessagingManager;

            if (_networkManager.IsServer)
                messaging.RegisterNamedMessageHandler(InputMessageName, OnInputMessageReceived);

            if (_networkManager.IsClient)
                messaging.RegisterNamedMessageHandler(SnapshotMessageName, OnSnapshotMessageReceived);

            _initialized = true;
        }

        /// <summary>注销消息处理器，清空端点、快照基准和本轮传输统计。</summary>
        public void Shutdown()
        {
            if (!_initialized)
                return;

            CustomMessagingManager messaging = _networkManager.CustomMessagingManager;

            if (_networkManager.IsServer)
                messaging.UnregisterNamedMessageHandler(InputMessageName);

            if (_networkManager.IsClient)
                messaging.UnregisterNamedMessageHandler(SnapshotMessageName);

            _endpoints.Clear();
            _snapshotBaselines.Clear();
            DroppedDeltaWithoutBaselineCount = 0;
            InputPayloadBytesSent = 0;
            SnapshotPayloadBytesSent = 0;
            InputMessageSendCount = 0;
            SnapshotMessageSendCount = 0;
            _initialized = false;
        }

        /// <summary>
        /// 注册本机已经生成的玩家同步端点。
        /// NetworkObjectId（网络对象编号）负责定位玩家，OwnerClientId（拥有者客户端编号）供服务器验证输入发送者。
        /// </summary>
        public void RegisterEndpoint(ulong networkObjectId, ulong ownerClientId, IPlayerSyncEndpoint endpoint)
        {
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));

            _endpoints[networkObjectId] = new EndpointEntry(ownerClientId, endpoint);
        }

        /// <summary>移除已销毁玩家的路由端点及其客户端快照基准。</summary>
        public void UnregisterEndpoint(ulong networkObjectId)
        {
            _endpoints.Remove(networkObjectId);
            _snapshotBaselines.Remove(networkObjectId);
        }

        /// <summary>
        /// Owner Client（拥有该玩家的客户端）-> Server（服务器）。
        /// 一个包最多携带当前输入 + 两份历史输入。
        /// 历史冗余可让服务器在当前包送达时补回前面丢失的输入，不改变每条输入原有的 Tick（固定同步步编号）。
        /// </summary>
        public void SendInputBatch(
            ulong networkObjectId,
            in PlayerInputCommand current,
            PlayerInputCommand? previous1 = null,
            PlayerInputCommand? previous2 = null)
        {
            EnsureInitialized();

            if (!_networkManager.IsClient)
                return;

            byte count = 1;

            if (previous1.HasValue)
                count++;

            if (previous2.HasValue)
                count++;

            using FastBufferWriter writer = new FastBufferWriter(WriterInitialCapacity, Allocator.Temp, WriterMaxCapacity);

            writer.WriteValueSafe(networkObjectId);
            writer.WriteValueSafe(count);

            // 按旧 -> 新顺序写。Server Buffer（服务器输入缓冲）按 Tick 存储，本身不依赖包内顺序，
            // 但这个顺序便于日志、抓包和逐条处理时理解时间关系。
            if (previous2.HasValue)
                writer.WriteNetworkSerializable(previous2.Value);

            if (previous1.HasValue)
                writer.WriteNetworkSerializable(previous1.Value);

            writer.WriteNetworkSerializable(current);

            InputPayloadBytesSent += writer.Length;
            InputMessageSendCount++;

            _networkManager.CustomMessagingManager.SendNamedMessage(
                InputMessageName,
                NetworkManager.ServerClientId,
                writer,
                NetworkDelivery.UnreliableSequenced);
        }

        /// <summary>
        /// Server（服务器）-> Client（客户端）。
        /// Full Snapshot（完整快照）走可靠序列通道；Delta Snapshot（差量快照）走不可靠序列通道。
        /// 两类快照使用相同消息格式，由 <see cref="PlayerSnapshotPacket.Kind"/> 区分内容。
        /// </summary>
        public void SendSnapshot(ulong targetClientId, ulong networkObjectId, in PlayerSnapshotPacket snapshot)
        {
            EnsureInitialized();

            if (!_networkManager.IsServer)
                return;

            if (targetClientId == NetworkManager.ServerClientId)
                return;

            using FastBufferWriter writer = new FastBufferWriter(WriterInitialCapacity, Allocator.Temp, WriterMaxCapacity);

            writer.WriteValueSafe(networkObjectId);
            writer.WriteNetworkSerializable(snapshot);

            SnapshotPayloadBytesSent += writer.Length;
            SnapshotMessageSendCount++;

            // 完整快照是后续差量快照的 Baseline（基准完整状态），必须可靠送达；
            // 差量快照发送频率较高，允许丢弃旧包以降低重传造成的排队延迟。
            NetworkDelivery delivery = snapshot.IsFull
                ? NetworkDelivery.ReliableSequenced
                : NetworkDelivery.UnreliableSequenced;

            _networkManager.CustomMessagingManager.SendNamedMessage(
                SnapshotMessageName,
                targetClientId,
                writer,
                delivery);
        }

        /// <summary>
        /// 服务器输入消息入口：检查消息数量、对象路由和所有权，再把包内每条输入交给对应权威端点。
        /// 任何解析异常只丢弃当前消息，避免损坏服务器同步循环。
        /// </summary>
        private void OnInputMessageReceived(ulong senderClientId, FastBufferReader reader)
        {
            if (!_networkManager.IsServer)
                return;

            try
            {
                reader.ReadValueSafe(out ulong networkObjectId);
                reader.ReadValueSafe(out byte count);

                if (count == 0 || count > MaxInputRedundancy)
                    return;

                if (!_endpoints.TryGetValue(networkObjectId, out EndpointEntry entry))
                    return;

                // Client（客户端）只能操作自己拥有的玩家，阻止伪造其他 NetworkObjectId 的输入。
                if (entry.OwnerClientId != senderClientId)
                    return;

                for (int i = 0; i < count; i++)
                {
                    reader.ReadNetworkSerializable(out PlayerInputCommand input);
                    entry.Endpoint.ReceiveInputFromClient(senderClientId, input);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[{nameof(PlayerSyncTransport)}] Input消息解析失败：{exception.Message}");
            }
        }

        /// <summary>
        /// 客户端快照消息入口：只接受服务器消息，按 NetworkObjectId 路由，并用匹配的完整基准还原差量快照。
        /// </summary>
        private void OnSnapshotMessageReceived(ulong senderClientId, FastBufferReader reader)
        {
            if (!_networkManager.IsClient)
                return;

            if (senderClientId != NetworkManager.ServerClientId)
                return;

            try
            {
                reader.ReadValueSafe(out ulong networkObjectId);

                if (!_endpoints.TryGetValue(networkObjectId, out EndpointEntry entry))
                    return;

                reader.ReadNetworkSerializable(out PlayerSnapshotPacket packet);

                if (packet.IsFull)
                {
                    if (!packet.TryResolve(default, out PlayerSimulationState fullState))
                        return;

                    // Full Snapshot（完整快照）成为新的固定 Baseline（基准完整状态）。
                    // 在下一个完整快照到达前，后续差量都必须声明引用这个 Tick。
                    _snapshotBaselines[networkObjectId] = fullState;

                    entry.Endpoint.ReceiveServerSnapshot(fullState);
                    return;
                }

                if (!_snapshotBaselines.TryGetValue(networkObjectId, out PlayerSimulationState baseline) ||
                    baseline.Tick != packet.BaselineTick)
                {
                    // 可能发生：
                    // 1. Client（客户端）刚加入，还没有收到对应 Full Snapshot（完整快照）。
                    // 2. Reliable（可靠）Full 与 Unreliable（不可靠）Delta 跨传输 Pipeline（管线）发生到达顺序变化。
                    // 3. 这是旧 Baseline（基准完整状态）对应的迟到 Delta（差量快照）。
                    DroppedDeltaWithoutBaselineCount++;
                    return;
                }

                if (!packet.TryResolve(baseline, out PlayerSimulationState resolvedState))
                {
                    DroppedDeltaWithoutBaselineCount++;
                    return;
                }

                entry.Endpoint.ReceiveServerSnapshot(resolvedState);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[{nameof(PlayerSyncTransport)}] Snapshot消息解析失败：{exception.Message}");
            }
        }

        /// <summary>保护所有公开收发方法，避免在消息处理器注册前静默丢失数据。</summary>
        private void EnsureInitialized()
        {
            if (!_initialized)
                throw new InvalidOperationException($"{nameof(PlayerSyncTransport)} 尚未初始化。");
        }

        /// <summary>路由表条目，同时保存玩家拥有者编号和接收消息的同步端点。</summary>
        private readonly struct EndpointEntry
        {
            /// <summary>唯一允许向该玩家发送输入的客户端编号。</summary>
            public ulong OwnerClientId { get; }

            /// <summary>接收已验证输入或已还原快照的玩家同步总控。</summary>
            public IPlayerSyncEndpoint Endpoint { get; }

            /// <summary>创建不可变的玩家同步路由条目。</summary>
            public EndpointEntry(ulong ownerClientId, IPlayerSyncEndpoint endpoint)
            {
                OwnerClientId = ownerClientId;
                Endpoint = endpoint;
            }
        }
    }
}
