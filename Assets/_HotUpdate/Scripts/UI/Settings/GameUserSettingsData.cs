using System;
using UnityEngine;

namespace ProjectGame.HotFix.Settings
{
    /// <summary>
    /// 保存玩家本地音频和按键偏好 
    /// </summary>
    [Serializable]
    public sealed class GameUserSettingsData
    {
        public int Version = 1;
        public AudioSettingsData Audio = new AudioSettingsData();
        public string InputBindingOverridesJson = string.Empty;

        /// <summary>
        /// 创建一份不共享引用的默认设置 
        /// </summary>
        public static GameUserSettingsData CreateDefault()
        {
            return new GameUserSettingsData();
        }

        /// <summary>
        /// 修复外部 JSON 中缺失或越界的数据 
        /// </summary>
        public void Normalize()
        {
            Version = Mathf.Max(Version, 1);
            Audio ??= new AudioSettingsData();
            Audio.Normalize();
            InputBindingOverridesJson ??= string.Empty;
        }
    }

    /// <summary>
    /// 保存三个音频通道的线性音量 
    /// </summary>
    [Serializable]
    public sealed class AudioSettingsData
    {
        public float MasterVolume = 1f;
        public float MusicVolume = 0.8f;
        public float SfxVolume = 0.8f;

        /// <summary>
        /// 把所有音量限制到 Slider 使用的有效范围 
        /// </summary>
        public void Normalize()
        {
            MasterVolume = Mathf.Clamp01(MasterVolume);
            MusicVolume = Mathf.Clamp01(MusicVolume);
            SfxVolume = Mathf.Clamp01(SfxVolume);
        }
    }

    /// <summary>
    /// 标识 Setting 页面中的音量通道 
    /// </summary>
    public enum SettingVolumeChannel
    {
        Master,
        Music,
        Sfx
    }
}
