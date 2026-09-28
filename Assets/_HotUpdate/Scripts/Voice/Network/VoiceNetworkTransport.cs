using System;
using Unity.Collections;
using Unity.Netcode;

namespace ProjectGame.HotFix.Voice.Network
{
    /// <summary>
    /// 使用 NGO Named Message 转发 Opus 语音包。
    /// </summary>
    public sealed class VoiceNetworkTransport : IDisposable
    {
        private const string ClientVoiceMessage = "VoiceC2S";
        private const string ServerVoiceMessage = "VoiceS2C";
        private const string ClientStateMessage = "VoiceStateC2S";
        private const string ServerStateMessage = "VoiceStateS2C";

        private readonly NetworkManager _networkManager;
        private byte[] _receiveBuffer = new byte[VoiceOpusEncoder.MaxPacketBytes];
        private bool _serverHandlerRegistered;
        private bool _clientHandlerRegistered;

        public event Action<ulong, ushort, byte[], int> VoiceReceived;
        public event Action<ulong, bool> VoiceStateReceived;

        public VoiceNetworkTransport(NetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        public void RegisterForCurrentRole()
        {
            CustomMessagingManager messaging = _networkManager.CustomMessagingManager;

            if (_networkManager.IsServer && !_serverHandlerRegistered)
            {
                messaging.RegisterNamedMessageHandler(ClientVoiceMessage, OnClientVoiceReceived);
                messaging.RegisterNamedMessageHandler(ClientStateMessage, OnClientStateReceived);
                _serverHandlerRegistered = true;
            }

            if (_networkManager.IsClient && !_clientHandlerRegistered)
            {
                messaging.RegisterNamedMessageHandler(ServerVoiceMessage, OnServerVoiceReceived);
                messaging.RegisterNamedMessageHandler(ServerStateMessage, OnServerStateReceived);
                _clientHandlerRegistered = true;
            }
        }

        /// <summary>
        /// 麦克风开关使用独立低频消息，不再依赖静音音频包维持状态。
        /// </summary>
        public void SendMicrophoneState(bool microphoneOpen)
        {
            if (_networkManager.IsHost)
            {
                RelayMicrophoneState(
                    _networkManager.LocalClientId,
                    microphoneOpen);
                return;
            }

            if (!_networkManager.IsClient)
                return;

            using var writer = new FastBufferWriter(
                sizeof(byte),
                Allocator.Temp);
            writer.WriteValueSafe(microphoneOpen ? (byte)1 : (byte)0);

            _networkManager.CustomMessagingManager.SendNamedMessage(
                ClientStateMessage,
                NetworkManager.ServerClientId,
                writer,
                NetworkDelivery.ReliableSequenced);
        }

        public void SendVoice(ushort sequence, byte[] opusData, int opusLength)
        {
            if (opusLength <= 0)
                return;

            // Host 已经在服务端进程内，直接走中继，避免给自己发送 C2S。
            if (_networkManager.IsHost)
            {
                RelayVoice(
                    _networkManager.LocalClientId,
                    sequence,
                    opusData,
                    opusLength);
                return;
            }

            if (!_networkManager.IsClient)
                return;

            using var writer = new FastBufferWriter(
                sizeof(ushort) + opusLength,
                Allocator.Temp);

            writer.WriteValueSafe(sequence);
            writer.WriteBytesSafe(opusData, opusLength);

            _networkManager.CustomMessagingManager.SendNamedMessage(
                ClientVoiceMessage,
                NetworkManager.ServerClientId,
                writer,
                NetworkDelivery.Unreliable);
        }

        private void OnClientVoiceReceived(ulong senderClientId, FastBufferReader reader)
        {
            if (!_networkManager.IsServer)
                return;

            reader.ReadValueSafe(out ushort sequence);

            int opusLength = reader.Length - reader.Position;
            if (opusLength <= 0 || opusLength > _receiveBuffer.Length)
                return;

            reader.ReadBytesSafe(ref _receiveBuffer, opusLength);
            RelayVoice(senderClientId, sequence, _receiveBuffer, opusLength);
        }

        private void OnClientStateReceived(
            ulong senderClientId,
            FastBufferReader reader)
        {
            if (!_networkManager.IsServer)
                return;

            reader.ReadValueSafe(out byte state);
            RelayMicrophoneState(senderClientId, state != 0);
        }

        private void RelayVoice(
            ulong speakerClientId,
            ushort sequence,
            byte[] opusData,
            int opusLength)
        {
            // Host 的本地客户端直接消费远端语音。
            if (_networkManager.IsHost &&
                speakerClientId != _networkManager.LocalClientId)
            {
                VoiceReceived?.Invoke(
                    speakerClientId,
                    sequence,
                    opusData,
                    opusLength);
            }

            using var writer = new FastBufferWriter(
                sizeof(ulong) + sizeof(ushort) + opusLength,
                Allocator.Temp);

            writer.WriteValueSafe(speakerClientId);
            writer.WriteValueSafe(sequence);
            writer.WriteBytesSafe(opusData, opusLength);

            foreach (ulong clientId in _networkManager.ConnectedClientsIds)
            {
                if (clientId == speakerClientId)
                    continue;

                if (_networkManager.IsHost &&
                    clientId == _networkManager.LocalClientId)
                    continue;

                _networkManager.CustomMessagingManager.SendNamedMessage(
                    ServerVoiceMessage,
                    clientId,
                    writer,
                    NetworkDelivery.Unreliable);
            }
        }

        private void RelayMicrophoneState(
            ulong speakerClientId,
            bool microphoneOpen)
        {
            if (_networkManager.IsHost &&
                speakerClientId != _networkManager.LocalClientId)
            {
                VoiceStateReceived?.Invoke(
                    speakerClientId,
                    microphoneOpen);
            }

            using var writer = new FastBufferWriter(
                sizeof(ulong) + sizeof(byte),
                Allocator.Temp);
            writer.WriteValueSafe(speakerClientId);
            writer.WriteValueSafe(microphoneOpen ? (byte)1 : (byte)0);

            foreach (ulong clientId in _networkManager.ConnectedClientsIds)
            {
                if (clientId == speakerClientId)
                    continue;

                if (_networkManager.IsHost &&
                    clientId == _networkManager.LocalClientId)
                    continue;

                _networkManager.CustomMessagingManager.SendNamedMessage(
                    ServerStateMessage,
                    clientId,
                    writer,
                    NetworkDelivery.ReliableSequenced);
            }
        }

        private void OnServerVoiceReceived(
            ulong senderClientId,
            FastBufferReader reader)
        {
            reader.ReadValueSafe(out ulong speakerClientId);
            reader.ReadValueSafe(out ushort sequence);

            int opusLength = reader.Length - reader.Position;
            if (opusLength <= 0 || opusLength > _receiveBuffer.Length)
                return;

            reader.ReadBytesSafe(ref _receiveBuffer, opusLength);
            VoiceReceived?.Invoke(
                speakerClientId,
                sequence,
                _receiveBuffer,
                opusLength);
        }

        private void OnServerStateReceived(
            ulong senderClientId,
            FastBufferReader reader)
        {
            reader.ReadValueSafe(out ulong speakerClientId);
            reader.ReadValueSafe(out byte state);
            VoiceStateReceived?.Invoke(speakerClientId, state != 0);
        }

        public void Dispose()
        {
            CustomMessagingManager messaging = _networkManager.CustomMessagingManager;

            if (_serverHandlerRegistered)
            {
                messaging.UnregisterNamedMessageHandler(ClientVoiceMessage);
                messaging.UnregisterNamedMessageHandler(ClientStateMessage);
                _serverHandlerRegistered = false;
            }

            if (_clientHandlerRegistered)
            {
                messaging.UnregisterNamedMessageHandler(ServerVoiceMessage);
                messaging.UnregisterNamedMessageHandler(ServerStateMessage);
                _clientHandlerRegistered = false;
            }

            VoiceReceived = null;
            VoiceStateReceived = null;
        }
    }
}
