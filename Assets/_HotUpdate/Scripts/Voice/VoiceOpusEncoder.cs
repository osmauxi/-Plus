using System;
using ProjectGame.AOT.Voice;
using UnityEngine;

namespace ProjectGame.HotFix.Voice
{
    /// <summary>
    /// 语音发送端使用的 Opus 编码器。
    /// </summary>
    public sealed class VoiceOpusEncoder : IDisposable
    {
        public const int MaxPacketBytes = 4000;

        private readonly OpusNativeEncoder _encoder;

        public VoiceOpusEncoder(float[] pcm, byte[] packet)
        {
            _encoder = new OpusNativeEncoder(
                VoiceCapture.SampleRate,
                VoiceCapture.FrameSamples,
                pcm,
                packet);

            Debug.Log("[Voice] Opus 编码器后端：Native libopus 1.5.2 (AOT)");
        }

        public int Encode()
        {
            return _encoder.Encode();
        }

        public void Dispose()
        {
            _encoder?.Dispose();
        }
    }

    /// <summary>
    /// 每个远端玩家独立持有一个 Opus 解码器。
    /// </summary>
    public sealed class VoiceOpusDecoder : IDisposable
    {
        private readonly OpusNativeDecoder _decoder;

        public VoiceOpusDecoder(float[] pcmOutput)
        {
            _decoder = new OpusNativeDecoder(
                VoiceCapture.SampleRate,
                VoiceCapture.FrameSamples,
                pcmOutput);
        }

        public int Decode(byte[] opusData, int opusLength)
        {
            return _decoder.Decode(opusData, opusLength);
        }

        public void Dispose()
        {
            _decoder?.Dispose();
        }
    }
}
