using System;
using UnityEngine;

namespace ProjectGame.HotFix.Voice
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class VoiceSpeaker : MonoBehaviour
    {
        private const int RingBufferMilliseconds = 500;
        private const int PlaybackClockSeconds = 1;
        private const int PrebufferFrames = 3;
        private const int MaxQueuedFrames = 10;
        private const int TargetQueuedFrames = 5;

        private AudioSource _audioSource;
        private AudioClip _streamClip;
        private PcmRingBuffer _pcmBuffer;
        private float[] _audioThreadBuffer;
        private Transform _followTarget;
        private bool _started;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _pcmBuffer = new PcmRingBuffer(
                VoiceCapture.SampleRate * RingBufferMilliseconds / 1000);
            AudioSettings.GetDSPBufferSize(
                out int dspBufferLength,
                out _);
            _audioThreadBuffer = new float[
                Mathf.Max(dspBufferLength, VoiceCapture.FrameSamples)];

            // 使用普通静音 Clip 驱动 AudioSource；真实语音由音频线程直接写入。
            _streamClip = AudioClip.Create(
                "VoicePlaybackClock",
                VoiceCapture.SampleRate * PlaybackClockSeconds,
                1,
                VoiceCapture.SampleRate,
                false);

            _audioSource.clip = _streamClip;
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
            _audioSource.dopplerLevel = 0f;
            _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;

            Set2D();
        }

        public void ConfigureDistances(float minDistance, float maxDistance)
        {
            _audioSource.minDistance = minDistance;
            _audioSource.maxDistance = maxDistance;
        }

        /// <summary>
        /// Lobby 使用 2D 播放，不考虑玩家之间的距离。
        /// </summary>
        public void Set2D()
        {
            _followTarget = null;
            _audioSource.spatialBlend = 0f;
        }

        /// <summary>
        /// Gameplay 跟随玩家位置并启用 3D 衰减。
        /// </summary>
        public void Set3DTarget(Transform target)
        {
            _followTarget = target;
            transform.position = target.position;
            _audioSource.spatialBlend = 1f;
        }

        public void PushPcm(float[] pcm, int sampleCount)
        {
            if (!_pcmBuffer.Write(pcm, sampleCount))
                return;

            if (_pcmBuffer.Count > VoiceCapture.FrameSamples * MaxQueuedFrames)
            {
                // 游戏语音优先实时性，积压超过 200ms 时丢弃旧语音。
                _pcmBuffer.TrimToLatest(
                    VoiceCapture.FrameSamples * TargetQueuedFrames);
            }

            // 先积累约 60ms，降低网络轻微抖动带来的断音。
            if (!_started && _pcmBuffer.Count >= VoiceCapture.FrameSamples * PrebufferFrames)
            {
                _audioSource.Play();
                _started = true;
            }
        }

        private void LateUpdate()
        {
            if (_followTarget != null)
            {
                transform.position = _followTarget.position;
                return;
            }

            if (_audioSource.spatialBlend > 0f)
                Set2D();
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (_pcmBuffer == null || _audioThreadBuffer == null)
            {
                Array.Clear(data, 0, data.Length);
                return;
            }

            int frameCount = data.Length / channels;
            int frameOffset = 0;

            while (frameOffset < frameCount)
            {
                int chunkFrames = Mathf.Min(
                    _audioThreadBuffer.Length,
                    frameCount - frameOffset);
                int read = _pcmBuffer.Read(
                    _audioThreadBuffer,
                    chunkFrames);

                for (int i = 0; i < chunkFrames; i++)
                {
                    float sample = i < read ? _audioThreadBuffer[i] : 0f;
                    int outputIndex = (frameOffset + i) * channels;
                    for (int channel = 0; channel < channels; channel++)
                        data[outputIndex + channel] = sample;
                }

                frameOffset += chunkFrames;
            }
        }

        private void OnDestroy()
        {
            if (_audioSource != null)
                _audioSource.Stop();

            if (_streamClip != null)
                Destroy(_streamClip);
        }
    }
}
