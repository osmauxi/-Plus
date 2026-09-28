using System;
using UnityEngine;

namespace ProjectGame.HotFix.Settings
{
    public enum VoiceTransmitMode : byte
    {
        PushToTalk = 0,
        OpenMicrophone = 1,
    }

    /// <summary>
    /// 保存玩家本地音频、语音和按键偏好。
    /// </summary>
    [Serializable]
    public sealed class GameUserSettingsData
    {
        public int Version = 2;
        public AudioSettingsData Audio = new AudioSettingsData();
        public VoiceSettingsData Voice = new VoiceSettingsData();
        public string InputBindingOverridesJson = string.Empty;

        public static GameUserSettingsData CreateDefault()
        {
            return new GameUserSettingsData();
        }

        public void Normalize()
        {
            Version = Mathf.Max(Version, 2);
            Audio ??= new AudioSettingsData();
            Voice ??= new VoiceSettingsData();

            Audio.Normalize();
            Voice.Normalize();
            InputBindingOverridesJson ??= string.Empty;
        }
    }

    /// <summary>
    /// 保存三个音频通道的线性音量。
    /// </summary>
    [Serializable]
    public sealed class AudioSettingsData
    {
        public float MasterVolume = 1f;
        public float MusicVolume = 0.8f;
        public float SfxVolume = 0.8f;

        public void Normalize()
        {
            MasterVolume = Mathf.Clamp01(MasterVolume);
            MusicVolume = Mathf.Clamp01(MusicVolume);
            SfxVolume = Mathf.Clamp01(SfxVolume);
        }
    }

    /// <summary>
    /// 保存本地麦克风的发送方式。
    /// </summary>
    [Serializable]
    public sealed class VoiceSettingsData
    {
        public VoiceTransmitMode TransmitMode =
            VoiceTransmitMode.PushToTalk;

        public void Normalize()
        {
            if (!Enum.IsDefined(typeof(VoiceTransmitMode), TransmitMode))
                TransmitMode = VoiceTransmitMode.PushToTalk;
        }
    }

    public enum SettingVolumeChannel
    {
        Master,
        Music,
        Sfx
    }
}