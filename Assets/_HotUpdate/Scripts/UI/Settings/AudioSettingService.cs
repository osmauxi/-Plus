using UnityEngine;
using UnityEngine.Audio;

namespace ProjectGame.HotFix.Settings
{
    /// <summary>
    /// 把线性音量转换为 dB 并应用到大厅 AudioMixer。
    /// </summary>
    public sealed class AudioSettingService
    {
        public const string MasterParameter = "MasterVolume";
        public const string MusicParameter = "MusicVolume";
        public const string SfxParameter = "SfxVolume";

        private readonly AudioMixer _audioMixer;

        /// <summary>
        /// 缓存必须由场景绑定的 AudioMixer。
        /// </summary>
        public AudioSettingService(AudioMixer audioMixer)
        {
            _audioMixer = audioMixer;
        }

        /// <summary>
        /// 一次性应用三个音频通道的当前值。
        /// </summary>
        public void Apply(AudioSettingsData data)
        {
            _audioMixer.SetFloat(MasterParameter, LinearToDecibel(data.MasterVolume));
            _audioMixer.SetFloat(MusicParameter, LinearToDecibel(data.MusicVolume));
            _audioMixer.SetFloat(SfxParameter, LinearToDecibel(data.SfxVolume));
        }

        /// <summary>
        /// 将 0 到 1 的线性音量转换为 AudioMixer 使用的 dB。
        /// </summary>
        private static float LinearToDecibel(float value)
        {
            return value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;
        }
    }
}
