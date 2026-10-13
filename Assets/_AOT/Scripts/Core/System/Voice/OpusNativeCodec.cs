using System;
using System.Runtime.InteropServices;

namespace ProjectGame.AOT.Voice
{
    /// <summary>
    /// AOT 层的 libopus 编码入口。固定复用 Voice 的 PCM/Packet 缓冲区，热路径不做托管分配。
    /// </summary>
    public sealed class OpusNativeEncoder : IDisposable
    {
        private const string LibraryName = "opus";
        private const int ApplicationVoip = 2048;
        private const int SetBitrateRequest = 4002;
        private const int SetVbrRequest = 4006;
        private const int SetComplexityRequest = 4010;
        private const int SetInbandFecRequest = 4012;
        private const int SetDtxRequest = 4016;
        private const int SetSignalRequest = 4024;
        private const int SignalVoice = 3001;

        private readonly IntPtr _state;
        private readonly int _frameSamples;
        private readonly int _packetCapacity;
        private readonly GCHandle _pcmHandle;
        private readonly GCHandle _packetHandle;

        public OpusNativeEncoder(
            int sampleRate,
            int frameSamples,
            float[] pcm,
            byte[] packet)
        {
            _state = opus_encoder_create(
                sampleRate,
                1,
                ApplicationVoip,
                out int error);
            if (error != 0 || _state == IntPtr.Zero)
                throw new InvalidOperationException(
                    $"libopus encoder create failed: {error}");

            SetControl(SetBitrateRequest, 24000);
            SetControl(SetComplexityRequest, 5);
            SetControl(SetSignalRequest, SignalVoice);
            SetControl(SetVbrRequest, 1);
            SetControl(SetInbandFecRequest, 0);
            SetControl(SetDtxRequest, 1);

            _frameSamples = frameSamples;
            _packetCapacity = packet.Length;
            _pcmHandle = GCHandle.Alloc(pcm, GCHandleType.Pinned);
            _packetHandle = GCHandle.Alloc(packet, GCHandleType.Pinned);
        }

        public int Encode()
        {
            int result = opus_encode_float(
                _state,
                _pcmHandle.AddrOfPinnedObject(),
                _frameSamples,
                _packetHandle.AddrOfPinnedObject(),
                _packetCapacity);
            if (result < 0)
                throw new InvalidOperationException(
                    $"libopus encode failed: {result}");

            return result;
        }

        private void SetControl(int request, int value)
        {
            int result = opus_encoder_ctl(_state, request, value);
            if (result < 0)
                throw new InvalidOperationException(
                    $"libopus encoder ctl {request} failed: {result}");
        }

        public void Dispose()
        {
            _packetHandle.Free();
            _pcmHandle.Free();
            opus_encoder_destroy(_state);
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr opus_encoder_create(
            int sampleRate,
            int channels,
            int application,
            out int error);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void opus_encoder_destroy(IntPtr state);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int opus_encode_float(
            IntPtr state,
            IntPtr pcm,
            int frameSize,
            IntPtr packet,
            int packetCapacity);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int opus_encoder_ctl(
            IntPtr state,
            int request,
            int value);
    }

    /// <summary>
    /// AOT 层的 libopus 解码入口。输出缓冲区和稳定的网络接收缓冲区只固定一次。
    /// </summary>
    public sealed class OpusNativeDecoder : IDisposable
    {
        private const string LibraryName = "opus";

        private readonly IntPtr _state;
        private readonly int _frameSamples;
        private readonly GCHandle _pcmHandle;
        private GCHandle _packetHandle;
        private byte[] _packet;

        public OpusNativeDecoder(
            int sampleRate,
            int frameSamples,
            float[] pcm)
        {
            _state = opus_decoder_create(sampleRate, 1, out int error);
            if (error != 0 || _state == IntPtr.Zero)
                throw new InvalidOperationException(
                    $"libopus decoder create failed: {error}");

            _frameSamples = frameSamples;
            _pcmHandle = GCHandle.Alloc(pcm, GCHandleType.Pinned);
        }

        public int Decode(byte[] packet, int packetLength)
        {
            if (!ReferenceEquals(_packet, packet))
            {
                if (_packetHandle.IsAllocated)
                    _packetHandle.Free();

                _packet = packet;
                _packetHandle = GCHandle.Alloc(packet, GCHandleType.Pinned);
            }

            int result = opus_decode_float(
                _state,
                _packetHandle.AddrOfPinnedObject(),
                packetLength,
                _pcmHandle.AddrOfPinnedObject(),
                _frameSamples,
                0);
            if (result < 0)
                throw new InvalidOperationException(
                    $"libopus decode failed: {result}");

            return result;
        }

        public void Dispose()
        {
            if (_packetHandle.IsAllocated)
                _packetHandle.Free();

            _pcmHandle.Free();
            opus_decoder_destroy(_state);
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr opus_decoder_create(
            int sampleRate,
            int channels,
            out int error);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void opus_decoder_destroy(IntPtr state);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int opus_decode_float(
            IntPtr state,
            IntPtr packet,
            int packetLength,
            IntPtr pcm,
            int frameSize,
            int decodeFec);
    }
}
