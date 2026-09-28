using UnityEngine;

namespace ProjectGame.HotFix.Voice
{
    /// <summary>
    /// 从系统麦克风持续读取固定 20ms 的单声道 PCM 帧。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VoiceCapture : MonoBehaviour
    {
        public const int SampleRate = 48000;
        public const int FrameMilliseconds = 20;
        public const int FrameSamples = SampleRate * FrameMilliseconds / 1000;

        private const int ClipLengthSeconds = 2;
        private const int MaxBacklogFrames = 3;
        private const int RetainedBacklogFrames = 2;
        private const float LongPollGapSeconds = 0.25f;

        [Header("设备")]
        [SerializeField] private string _deviceName;

        [Header("输入处理")]
        [SerializeField, Range(20f, 200f)]
        private float _highPassFrequency = 80f;
        [SerializeField, Range(4000f, 18000f)]
        private float _lowPassFrequency = 10000f;
        [SerializeField, Range(0.001f, 0.05f)]
        private float _noiseGateThreshold = 0.006f;
        [SerializeField, Range(0.02f, 0.2f)]
        private float _targetVoiceRms = 0.08f;
        [SerializeField, Range(1f, 10f)]
        private float _maximumAutoGain = 6f;

        private AudioClip _microphoneClip;
        private int _readPosition;
        private bool _isRecording;
        private float _highPassPreviousInput;
        private float _highPassPreviousOutput;
        private float _lowPassPreviousOutput;
        private float _automaticGain = 1f;
        private float _gateGain;
        private int _gateHoldFrames;
        private float _lastPollTime;

        public bool IsRecording => _isRecording;
        public float LastInputRms { get; private set; }
        public float LastOutputRms { get; private set; }
        public int DroppedFrameCount { get; private set; }

        public bool StartCapture()
        {
            if (_isRecording)
                return true;

            string device = string.IsNullOrEmpty(_deviceName) ? null : _deviceName;
            _microphoneClip = Microphone.Start(device, true, ClipLengthSeconds, SampleRate);

            if (_microphoneClip == null)
            {
                Debug.LogError("[Voice] 麦克风启动失败。");
                return false;
            }

            if (_microphoneClip.channels != 1)
            {
                Debug.LogError($"[Voice] 仅支持单声道麦克风，当前声道数：{_microphoneClip.channels}");
                Microphone.End(device);
                _microphoneClip = null;
                return false;
            }

            _readPosition = 0;
            ResetInputProcessor();
            _lastPollTime = Time.realtimeSinceStartup;
            DroppedFrameCount = 0;
            _isRecording = true;

            string selectedDevice = string.IsNullOrEmpty(_deviceName)
                ? $"系统默认（Unity 列表首项：{GetFirstDeviceName()}）"
                : _deviceName;
            Debug.Log($"[Voice] 正在使用麦克风：{selectedDevice}");
            return true;
        }

        public void StopCapture()
        {
            if (!_isRecording)
                return;

            string device = string.IsNullOrEmpty(_deviceName) ? null : _deviceName;
            Microphone.End(device);

            _microphoneClip = null;
            _readPosition = 0;
            _isRecording = false;
            ResetInputProcessor();
            _lastPollTime = 0f;
        }

        public bool TryReadFrame(float[] frameBuffer)
        {
            if (!_isRecording || _microphoneClip == null)
                return false;

            string device = string.IsNullOrEmpty(_deviceName) ? null : _deviceName;
            int writePosition = Microphone.GetPosition(device);

            if (writePosition < 0)
                return false;

            float now = Time.realtimeSinceStartup;
            bool resumedAfterStall = now - _lastPollTime > LongPollGapSeconds;
            _lastPollTime = now;

            int availableSamples = writePosition - _readPosition;
            if (availableSamples < 0)
                availableSamples += _microphoneClip.samples;

            int retainedSamples = FrameSamples * RetainedBacklogFrames;
            if (resumedAfterStall ||
                availableSamples > FrameSamples * MaxBacklogFrames)
            {
                // 游戏卡顿或切场景后只保留最新约 40ms，不补发已经过时的录音。
                int droppedSamples = Mathf.Max(0, availableSamples - retainedSamples);
                DroppedFrameCount += droppedSamples / FrameSamples;
                _readPosition = writePosition - retainedSamples;
                if (_readPosition < 0)
                    _readPosition += _microphoneClip.samples;
                availableSamples = retainedSamples;
                ResetFilterHistory();
            }

            if (availableSamples < FrameSamples)
                return false;

            if (!_microphoneClip.GetData(frameBuffer, _readPosition))
                return false;

            _readPosition = (_readPosition + FrameSamples) % _microphoneClip.samples;
            ProcessInput(frameBuffer);
            return true;
        }

        /// <summary>
        /// 对原始麦克风做轻量处理：削减低频轰鸣和高频嘶声，静音时关闭底噪，
        /// 说话时自动提升偏低的输入电平。
        /// </summary>
        private void ProcessInput(float[] samples)
        {
            float deltaTime = 1f / SampleRate;
            float highPassRc = 1f / (2f * Mathf.PI * _highPassFrequency);
            float highPassAlpha = highPassRc / (highPassRc + deltaTime);
            float lowPassRc = 1f / (2f * Mathf.PI * _lowPassFrequency);
            float lowPassAlpha = deltaTime / (lowPassRc + deltaTime);
            float squareSum = 0f;

            for (int i = 0; i < samples.Length; i++)
            {
                float input = samples[i];
                float highPassed = highPassAlpha *
                    (_highPassPreviousOutput + input - _highPassPreviousInput);
                _highPassPreviousInput = input;
                _highPassPreviousOutput = highPassed;

                _lowPassPreviousOutput +=
                    lowPassAlpha * (highPassed - _lowPassPreviousOutput);
                samples[i] = _lowPassPreviousOutput;
                squareSum += samples[i] * samples[i];
            }

            LastInputRms = Mathf.Sqrt(squareSum / samples.Length);

            // 保留约 180ms，避免词尾和短暂停顿被噪声门切断。
            bool voiceDetected = LastInputRms >= _noiseGateThreshold;
            if (voiceDetected)
                _gateHoldFrames = 9;
            else if (_gateHoldFrames > 0)
                _gateHoldFrames--;

            bool gateOpen = _gateHoldFrames > 0;
            float targetGate = gateOpen ? 1f : 0f;
            float gateSpeed = gateOpen ? 0.65f : 0.22f;
            _gateGain = Mathf.MoveTowards(_gateGain, targetGate, gateSpeed);

            float targetGain = voiceDetected
                ? Mathf.Clamp(
                    _targetVoiceRms / Mathf.Max(LastInputRms, 0.0001f),
                    0.65f,
                    _maximumAutoGain)
                : (gateOpen ? _automaticGain : 1f);

            // 降低增益要快，提升增益稍慢，减少突然爆音和底噪抽动。
            float gainSpeed = targetGain < _automaticGain ? 0.65f : 0.18f;
            _automaticGain = Mathf.Lerp(_automaticGain, targetGain, gainSpeed);

            float outputSquareSum = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                float output = Mathf.Clamp(
                    samples[i] * _automaticGain * _gateGain,
                    -0.95f,
                    0.95f);
                samples[i] = output;
                outputSquareSum += output * output;
            }

            LastOutputRms = Mathf.Sqrt(outputSquareSum / samples.Length);
        }

        private void ResetInputProcessor()
        {
            ResetFilterHistory();
            _automaticGain = 1f;
            _gateGain = 0f;
            _gateHoldFrames = 0;
            LastInputRms = 0f;
            LastOutputRms = 0f;
        }

        private void ResetFilterHistory()
        {
            _highPassPreviousInput = 0f;
            _highPassPreviousOutput = 0f;
            _lowPassPreviousOutput = 0f;
        }

        private static string GetFirstDeviceName()
        {
            string[] devices = Microphone.devices;
            return devices.Length > 0 ? devices[0] : "未检测到设备";
        }

        private void OnDestroy()
        {
            StopCapture();
        }
    }
}
