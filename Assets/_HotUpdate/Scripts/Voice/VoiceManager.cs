using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Settings;
using ProjectGame.HotFix.Voice.Network;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectGame.HotFix.Voice
{
    public enum VoiceActivityState : byte
    {
        Closed = 0,
        Open = 1,
        Speaking = 2,
    }

    /// <summary>
    /// 跨 Lobby 和 Gameplay 存活的语音系统入口。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkManager))]
    [RequireComponent(typeof(VoiceCapture))]
    public sealed class VoiceManager : MonoBehaviour
    {
        public static VoiceManager Instance { get; private set; }
        public static event Action<VoiceManager> InstanceChanged;

        public event Action<ulong, VoiceActivityState> ActivityChanged;

        [Header("发送")]
        [SerializeField] private Key _pushToTalkKey = Key.V;

        [Header("Gameplay 3D 距离")]
        [SerializeField, Min(0.1f)] private float _minDistance = 2f;
        [SerializeField, Min(1f)] private float _maxDistance = 18f;

        [Header("说话检测")]
        [SerializeField, Min(0.001f)] private float _speakingThreshold = 0.015f;
        [SerializeField, Min(0f)] private float _speakingHoldSeconds = 0.12f;
        [SerializeField, Min(0.5f)] private float _voiceStateHeartbeatSeconds = 1f;
        [SerializeField, Min(1f)] private float _remoteStateTimeout = 3f;

        private readonly Dictionary<ulong, ReceiveChannel> _receiveChannels = new();
        private readonly Dictionary<ulong, Transform> _gameplayAnchors = new();
        private readonly Dictionary<ulong, VoiceActivityState> _activityStates = new();
        private readonly Dictionary<ulong, RemoteVoiceState> _remoteVoiceStates = new();

        private NetworkManager _networkManager;
        private VoiceCapture _capture;
        private VoiceNetworkTransport _transport;
        private VoiceOpusEncoder _encoder;
        private float[] _captureFrame;
        private byte[] _encodedPacket;
        private ushort _sendSequence;
        private bool _localConnected;
        private float _localSpeakingUntil;
        private bool? _lastPublishedMicrophoneOpen;
        private float _nextVoiceStateHeartbeat;
        private VoiceTransmitMode _transmitMode = VoiceTransmitMode.PushToTalk;

        public VoiceTransmitMode TransmitMode => _transmitMode;
        public Key PushToTalkKey => _pushToTalkKey;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            InstanceChanged?.Invoke(this);
            _networkManager = GetComponent<NetworkManager>();
            _capture = GetComponent<VoiceCapture>();

            _networkManager.OnServerStarted += HandleNetworkStarted;
            _networkManager.OnClientStarted += HandleNetworkStarted;
            _networkManager.OnServerStopped += HandleNetworkStopped;
            _networkManager.OnClientStopped += HandleNetworkStopped;
            _networkManager.OnClientConnectedCallback += HandleClientConnected;
            _networkManager.OnClientDisconnectCallback += HandleClientDisconnected;

            ApplySettings(new SettingSaveService().Load().Voice);
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            UpdateRemoteActivities(now);

            if (!_localConnected || !_capture.IsRecording || _transport == null)
                return;

            bool microphoneOpen =
                _transmitMode == VoiceTransmitMode.OpenMicrophone ||
                IsPushToTalkPressed();
            PublishMicrophoneState(microphoneOpen, now);

            // 即使当前不发送也持续取走帧，避免下次按键时发送旧录音。
            while (_capture.TryReadFrame(_captureFrame))
            {
                if (!microphoneOpen)
                    continue;

                if (IsSpeaking(_captureFrame, VoiceCapture.FrameSamples))
                    _localSpeakingUntil = now + _speakingHoldSeconds;

                // 静音时不编码、不发送音频包；保留短暂尾音避免截断词尾。
                if (now >= _localSpeakingUntil)
                    continue;

                int packetLength = _encoder.Encode(_captureFrame, _encodedPacket);
                _transport.SendVoice(
                    _sendSequence++,
                    _encodedPacket,
                    packetLength);
            }

            if (!microphoneOpen)
                _localSpeakingUntil = 0f;

            SetActivity(
                _networkManager.LocalClientId,
                microphoneOpen
                    ? (now < _localSpeakingUntil
                        ? VoiceActivityState.Speaking
                        : VoiceActivityState.Open)
                    : VoiceActivityState.Closed);
        }

        public void ApplySettings(VoiceSettingsData settings)
        {
            _transmitMode = settings.TransmitMode;
        }

        public VoiceActivityState GetActivity(ulong clientId)
        {
            return _activityStates.TryGetValue(clientId, out VoiceActivityState state)
                ? state
                : VoiceActivityState.Closed;
        }

        /// <summary>
        /// Gameplay 玩家生成后，把 3D 播放位置交给语音系统。
        /// </summary>
        public void BindGameplayAnchor(ulong clientId, Transform target)
        {
            _gameplayAnchors[clientId] = target;

            if (_receiveChannels.TryGetValue(clientId, out ReceiveChannel channel))
                channel.Speaker.Set3DTarget(target);
        }

        public void UnbindGameplayAnchor(ulong clientId, Transform target)
        {
            if (!_gameplayAnchors.TryGetValue(clientId, out Transform current) ||
                current != target)
                return;

            _gameplayAnchors.Remove(clientId);

            if (_receiveChannels.TryGetValue(clientId, out ReceiveChannel channel))
                channel.Speaker.Set2D();
        }

        public void ClearGameplayAnchors()
        {
            _gameplayAnchors.Clear();

            foreach (ReceiveChannel channel in _receiveChannels.Values)
                channel.Speaker.Set2D();
        }

        private bool IsPushToTalkPressed()
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard[_pushToTalkKey].isPressed;
        }

        private bool IsSpeaking(float[] samples, int count)
        {
            float squareSum = 0f;
            for (int i = 0; i < count; i++)
                squareSum += samples[i] * samples[i];

            return squareSum / count >= _speakingThreshold * _speakingThreshold;
        }

        private void UpdateRemoteActivities(float now)
        {
            foreach (KeyValuePair<ulong, RemoteVoiceState> pair in _remoteVoiceStates)
            {
                RemoteVoiceState remoteState = pair.Value;
                if (remoteState.MicrophoneOpen &&
                    now - remoteState.LastStateTime > _remoteStateTimeout)
                {
                    remoteState.MicrophoneOpen = false;
                    remoteState.SpeakingUntil = 0f;
                }

                VoiceActivityState state =
                    !remoteState.MicrophoneOpen
                        ? VoiceActivityState.Closed
                        : (now < remoteState.SpeakingUntil
                            ? VoiceActivityState.Speaking
                            : VoiceActivityState.Open);

                SetActivity(pair.Key, state);
            }
        }

        private void PublishMicrophoneState(bool microphoneOpen, float now)
        {
            bool changed = !_lastPublishedMicrophoneOpen.HasValue ||
                _lastPublishedMicrophoneOpen.Value != microphoneOpen;
            bool heartbeatDue = microphoneOpen &&
                now >= _nextVoiceStateHeartbeat;

            if (!changed && !heartbeatDue)
                return;

            _transport.SendMicrophoneState(microphoneOpen);
            _lastPublishedMicrophoneOpen = microphoneOpen;
            _nextVoiceStateHeartbeat = now + _voiceStateHeartbeatSeconds;
        }

        private void SetActivity(ulong clientId, VoiceActivityState state)
        {
            VoiceActivityState previous = GetActivity(clientId);
            if (previous == state)
                return;

            if (state == VoiceActivityState.Closed)
                _activityStates.Remove(clientId);
            else
                _activityStates[clientId] = state;

            ActivityChanged?.Invoke(clientId, state);
        }

        private void HandleNetworkStarted()
        {
            EnsureTransport();
            _transport.RegisterForCurrentRole();
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (!_networkManager.IsClient ||
                clientId != _networkManager.LocalClientId)
                return;

            _localConnected = true;
            EnsureTransport();
            _transport.RegisterForCurrentRole();
            EnsureEncoder();

            if (_capture.StartCapture())
            {
                Debug.Log(
                    $"[Voice] 语音已启动，模式={_transmitMode}，按键={_pushToTalkKey}");
            }
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            if (clientId == _networkManager.LocalClientId)
            {
                EndSession();
                return;
            }

            RemoveReceiveChannel(clientId);
        }

        private void HandleNetworkStopped(bool wasHost)
        {
            EndSession();
        }

        private void EnsureTransport()
        {
            if (_transport != null)
                return;

            _transport = new VoiceNetworkTransport(_networkManager);
            _transport.VoiceReceived += HandleVoiceReceived;
            _transport.VoiceStateReceived += HandleVoiceStateReceived;
        }

        private void EnsureEncoder()
        {
            if (_encoder != null)
                return;

            _encoder = new VoiceOpusEncoder();
            _captureFrame = new float[VoiceCapture.FrameSamples];
            _encodedPacket = new byte[VoiceOpusEncoder.MaxPacketBytes];
        }

        private void HandleVoiceReceived(
            ulong speakerClientId,
            ushort sequence,
            byte[] opusData,
            int opusLength)
        {
            if (_networkManager.IsClient &&
                speakerClientId == _networkManager.LocalClientId)
                return;

            if (!_receiveChannels.TryGetValue(
                    speakerClientId,
                    out ReceiveChannel channel))
            {
                channel = CreateReceiveChannel(speakerClientId);
                _receiveChannels.Add(speakerClientId, channel);
            }

            if (!channel.Accept(sequence))
                return;

            int sampleCount = channel.Decoder.Decode(
                opusData,
                opusLength,
                channel.DecodeBuffer);

            float now = Time.unscaledTime;
            RemoteVoiceState remoteState = GetRemoteVoiceState(speakerClientId);
            if (IsSpeaking(channel.DecodeBuffer, sampleCount))
                remoteState.SpeakingUntil = now + _speakingHoldSeconds;

            if (remoteState.MicrophoneOpen)
            {
                SetActivity(
                    speakerClientId,
                    now < remoteState.SpeakingUntil
                        ? VoiceActivityState.Speaking
                        : VoiceActivityState.Open);
            }

            UpdateSpatialMode(speakerClientId, channel.Speaker);
            channel.Speaker.PushPcm(channel.DecodeBuffer, sampleCount);
        }

        private void HandleVoiceStateReceived(
            ulong speakerClientId,
            bool microphoneOpen)
        {
            if (_networkManager.IsClient &&
                speakerClientId == _networkManager.LocalClientId)
                return;

            RemoteVoiceState remoteState = GetRemoteVoiceState(speakerClientId);
            remoteState.MicrophoneOpen = microphoneOpen;
            remoteState.LastStateTime = Time.unscaledTime;

            if (!microphoneOpen)
                remoteState.SpeakingUntil = 0f;

            SetActivity(
                speakerClientId,
                !microphoneOpen
                    ? VoiceActivityState.Closed
                    : (Time.unscaledTime < remoteState.SpeakingUntil
                        ? VoiceActivityState.Speaking
                        : VoiceActivityState.Open));
        }

        private RemoteVoiceState GetRemoteVoiceState(ulong clientId)
        {
            if (_remoteVoiceStates.TryGetValue(
                    clientId,
                    out RemoteVoiceState state))
                return state;

            state = new RemoteVoiceState();
            _remoteVoiceStates.Add(clientId, state);
            return state;
        }

        private ReceiveChannel CreateReceiveChannel(ulong clientId)
        {
            var speakerObject = new GameObject($"VoiceSpeaker_{clientId}");
            speakerObject.transform.SetParent(transform, false);

            VoiceSpeaker speaker = speakerObject.AddComponent<VoiceSpeaker>();
            speaker.ConfigureDistances(_minDistance, _maxDistance);
            speaker.Set2D();

            return new ReceiveChannel(speaker);
        }

        private void UpdateSpatialMode(
            ulong speakerClientId,
            VoiceSpeaker speaker)
        {
            if (_gameplayAnchors.TryGetValue(
                    speakerClientId,
                    out Transform target) &&
                target != null)
            {
                speaker.Set3DTarget(target);
                return;
            }

            // Lobby 不考虑距离，始终使用 2D 声音。
            speaker.Set2D();
        }

        private void RemoveReceiveChannel(ulong clientId)
        {
            if (_receiveChannels.Remove(
                    clientId,
                    out ReceiveChannel channel))
                channel.Dispose();

            _gameplayAnchors.Remove(clientId);
            _remoteVoiceStates.Remove(clientId);
            SetActivity(clientId, VoiceActivityState.Closed);
        }

        private void EndSession()
        {
            _localConnected = false;
            _capture.StopCapture();

            _encoder?.Dispose();
            _encoder = null;
            _captureFrame = null;
            _encodedPacket = null;
            _sendSequence = 0;

            if (_transport != null)
            {
                _transport.VoiceReceived -= HandleVoiceReceived;
                _transport.VoiceStateReceived -= HandleVoiceStateReceived;
                _transport.Dispose();
                _transport = null;
            }

            foreach (ReceiveChannel channel in _receiveChannels.Values)
                channel.Dispose();

            foreach (KeyValuePair<ulong, VoiceActivityState> pair in _activityStates)
                ActivityChanged?.Invoke(pair.Key, VoiceActivityState.Closed);

            _receiveChannels.Clear();
            _gameplayAnchors.Clear();
            _activityStates.Clear();
            _remoteVoiceStates.Clear();
            _localSpeakingUntil = 0f;
            _lastPublishedMicrophoneOpen = null;
            _nextVoiceStateHeartbeat = 0f;
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            _networkManager.OnServerStarted -= HandleNetworkStarted;
            _networkManager.OnClientStarted -= HandleNetworkStarted;
            _networkManager.OnServerStopped -= HandleNetworkStopped;
            _networkManager.OnClientStopped -= HandleNetworkStopped;
            _networkManager.OnClientConnectedCallback -= HandleClientConnected;
            _networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;

            EndSession();
            Instance = null;
            InstanceChanged?.Invoke(null);
            ActivityChanged = null;
        }

        private sealed class ReceiveChannel : IDisposable
        {
            private bool _hasSequence;
            private ushort _lastSequence;

            public readonly VoiceOpusDecoder Decoder = new();
            public readonly float[] DecodeBuffer =
                new float[VoiceCapture.FrameSamples];
            public readonly VoiceSpeaker Speaker;

            public ReceiveChannel(VoiceSpeaker speaker)
            {
                Speaker = speaker;
            }

            public bool Accept(ushort sequence)
            {
                if (!_hasSequence)
                {
                    _hasSequence = true;
                    _lastSequence = sequence;
                    return true;
                }

                // short 差值可以自然处理 ushort 回绕。
                if (unchecked((short)(sequence - _lastSequence)) <= 0)
                    return false;

                _lastSequence = sequence;
                return true;
            }

            public void Dispose()
            {
                Decoder.Dispose();

                if (Speaker != null)
                    UnityEngine.Object.Destroy(Speaker.gameObject);
            }
        }

        private sealed class RemoteVoiceState
        {
            public bool MicrophoneOpen;
            public float LastStateTime;
            public float SpeakingUntil;
        }
    }
}
