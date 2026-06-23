using UnityEngine;

/// <summary>
/// 音频事件数据结构体
/// 用于 LocalEventCenter 事件系统中的数据传递
/// </summary>
public struct AudioEvent
{
    public string audioName;       // 音频名称
    public AudioCategory category; // 音频类别
    public float volume;           // 音量
    public Vector3 position;       // 位置（3D 音效用）

    public AudioEvent(string name, AudioCategory cat, float vol = 1f, Vector3 pos = default)
    {
        audioName = name;
        category = cat;
        volume = vol;
        position = pos;
    }
}

/// <summary>
/// 音频设置变更事件数据
/// </summary>
public struct AudioSettingsEvent
{
    public float bgmVolume;   // BGM 音量
    public float sfxVolume;   // SFX 音量
    public bool bgmMuted;     // BGM 是否静音
    public bool sfxMuted;     // SFX 是否静音

    public AudioSettingsEvent(float bgmVol, float sfxVol, bool bgmMute, bool sfxMute)
    {
        bgmVolume = bgmVol;
        sfxVolume = sfxVol;
        bgmMuted = bgmMute;
        sfxMuted = sfxMute;
    }
}

/// <summary>
/// BGM 切换事件数据
/// </summary>
public struct BGMChangeEvent
{
    public AudioClip newBGM;   // 新的 BGM 片段
    public AudioClip oldBGM;   // 旧的 BGM 片段
    public float fadeTime;     // 淡入淡出时间

    public BGMChangeEvent(AudioClip newClip, AudioClip oldClip, float fade = 1f)
    {
        newBGM = newClip;
        oldBGM = oldClip;
        fadeTime = fade;
    }
}
