using System;
using Concentus;
using Concentus.Enums;

namespace ProjectGame.HotFix.Voice
{
    /// <summary>
    /// 语音发送端使用的 Opus 编码器。
    /// </summary>
    public sealed class VoiceOpusEncoder : IDisposable
    {
        public const int MaxPacketBytes = 4000;

        private readonly IOpusEncoder _encoder;

        public VoiceOpusEncoder()
        {
            OpusCodecFactory.AttemptToUseNativeLibrary = false;

            _encoder = OpusCodecFactory.CreateEncoder(
                VoiceCapture.SampleRate,
                1,
                OpusApplication.OPUS_APPLICATION_VOIP);

            _encoder.Bitrate = 24000;
            _encoder.Complexity = 5;
            _encoder.SignalType = OpusSignal.OPUS_SIGNAL_VOICE;
            _encoder.UseVBR = true;
            _encoder.UseInbandFEC = false;
            // 静音时启用 DTX；麦克风开关状态由独立控制消息同步。
            _encoder.UseDTX = true;
        }

        public int Encode(float[] pcm, byte[] output)
        {
            return _encoder.Encode(
                pcm,
                VoiceCapture.FrameSamples,
                output,
                output.Length);
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
        private readonly IOpusDecoder _decoder;

        public VoiceOpusDecoder()
        {
            OpusCodecFactory.AttemptToUseNativeLibrary = false;
            _decoder = OpusCodecFactory.CreateDecoder(VoiceCapture.SampleRate, 1);
        }

        public int Decode(byte[] opusData, int opusLength, float[] pcmOutput)
        {
            return _decoder.Decode(
                opusData.AsSpan(0, opusLength),
                pcmOutput,
                VoiceCapture.FrameSamples,
                false);
        }

        public void Dispose()
        {
            _decoder?.Dispose();
        }
    }
}
