using System.Collections;
using UnityEngine;

/// <summary>
/// 音频类别枚举，用于对音频进行分类管理
/// </summary>
public enum AudioCategory
{
    BGM,            // 背景音乐
    SFX_Weapon,     // 武器音效
    SFX_Footstep,   // 脚步声
    SFX_UI,         // 界面交互音效
    SFX_Environment,// 环境音效
    SFX_Monster,    // 怪物音效
    Voice,          // 语音
}

/// <summary>
/// 音频管理器 - 负责管理游戏中所有音频的播放、音量控制和设置持久化
/// 采用单例模式，支持 BGM 淡入淡出、音效对象池、跨场景音频保持
/// </summary>
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// 单例实例，提供全局访问点
    /// </summary>
    public static AudioManager instance;

    [Header("BGM 设置")]
    [SerializeField] private AudioSource bgmSource;      // BGM 专用的 AudioSource 组件
    [SerializeField] private float bgmFadeTime = 1f;     // BGM 淡入淡出时间（秒）

    [Header("SFX 设置")]
    [SerializeField] private int sfxPoolSize = 10;       // 音效对象池初始大小
    [SerializeField] private float minSfxInterval = 0.05f; // 音效播放最小间隔，防止过于频繁

    [Header("音量默认值")]
    [SerializeField, Range(0f, 1f)] private float defaultBGMVolume = 0.7f; // BGM 默认音量
    [SerializeField, Range(0f, 1f)] private float defaultSFXVolume = 0.8f; // SFX 默认音量

    private AudioPool sfxPool;           // 音效对象池实例
    private float currentBGMVolume;      // 当前 BGM 音量值
    private float currentSFXVolume;      // 当前 SFX 音量值
    private float lastSFXPlayTime;       // 上一次播放音效的时间戳
    private bool isBGMPlaying;           // BGM 是否正在播放
    private bool isBGMFading;            // BGM 是否正在淡入淡出过程中
    private bool isSFXMuted;             // SFX 是否静音
    private bool isBGMMuted;             // BGM 是否静音

    /// <summary>
    /// Unity 生命周期 - 初始化时调用
    /// 负责单例初始化和 DontDestroyOnLoad 设置
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// 初始化 AudioManager 的各个子系统
    /// 包括 BGM Source 创建、设置加载、音效池初始化、事件订阅
    /// </summary>
    private void Initialize()
    {
        if (bgmSource == null)
        {
            GameObject bgmObj = new GameObject("BGMSource");
            bgmObj.transform.SetParent(transform);
            bgmSource = bgmObj.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        LoadSettings();
        bgmSource.volume = isBGMMuted ? 0f : currentBGMVolume;

        sfxPool = new AudioPool(sfxPoolSize, transform);
        sfxPool.SetMasterVolume(isSFXMuted ? 0f : currentSFXVolume);

        LocalEventCenter.Instance.AddEventListener<float>("OnBGMVolumeChanged", HandleBGMVolumeChanged);
        LocalEventCenter.Instance.AddEventListener<float>("OnSFXVolumeChanged", HandleSFXVolumeChanged);
        LocalEventCenter.Instance.AddEventListener<bool>("OnBGMuteChanged", HandleBGMMuteChanged);
        LocalEventCenter.Instance.AddEventListener<bool>("OnSFXMuteChanged", HandleSFXMuteChanged);
    }

    /// <summary>
    /// Unity 生命周期 - 销毁时调用
    /// 清理事件订阅，防止内存泄漏
    /// </summary>
    private void OnDestroy()
    {
        LocalEventCenter.Instance.RemoveEventListener<float>("OnBGMVolumeChanged", HandleBGMVolumeChanged);
        LocalEventCenter.Instance.RemoveEventListener<float>("OnSFXVolumeChanged", HandleSFXVolumeChanged);
        LocalEventCenter.Instance.RemoveEventListener<bool>("OnBGMuteChanged", HandleBGMMuteChanged);
        LocalEventCenter.Instance.RemoveEventListener<bool>("OnSFXMuteChanged", HandleSFXMuteChanged);
    }

    /// <summary>
    /// 事件处理 - BGM 音量变化
    /// </summary>
    private void HandleBGMVolumeChanged(float volume) => SetBGMVolume(volume);

    /// <summary>
    /// 事件处理 - SFX 音量变化
    /// </summary>
    private void HandleSFXVolumeChanged(float volume) => SetSFXVolume(volume);

    /// <summary>
    /// 事件处理 - BGM 静音状态变化
    /// </summary>
    private void HandleBGMMuteChanged(bool muted) => MuteBGM(muted);

    /// <summary>
    /// 事件处理 - SFX 静音状态变化
    /// </summary>
    private void HandleSFXMuteChanged(bool muted) => MuteSFX(muted);

    /// <summary>
    /// Unity 生命周期 - 每帧调用
    /// 更新音效对象池状态，回收已播放完成的 AudioSource
    /// </summary>
    private void Update()
    {
        sfxPool?.Update();
    }

    #region BGM 控制

    /// <summary>
    /// 播放 BGM，支持淡入效果
    /// </summary>
    /// <param name="clip">要播放的音频片段</param>
    /// <param name="fadeInTime">淡入时间（秒），-1 表示使用默认值</param>
    public void PlayBGM(AudioClip clip, float fadeInTime = -1f)
    {
        if (clip == null) return;

        if (fadeInTime < 0f) fadeInTime = bgmFadeTime;

        StopAllCoroutines();

        if (bgmSource.clip == clip && isBGMPlaying) return;

        bgmSource.clip = clip;
        bgmSource.Play();

        if (fadeInTime > 0f)
        {
            bgmSource.volume = 0f;
            StartCoroutine(FadeBGM(isBGMMuted ? 0f : currentBGMVolume, fadeInTime));
        }
        else
        {
            bgmSource.volume = isBGMMuted ? 0f : currentBGMVolume;
        }

        isBGMPlaying = true;
        LocalEventCenter.Instance.EventTrigger<AudioClip>("OnBGMChanged", clip);
        SaveSettings();
    }

    /// <summary>
    /// 停止 BGM，支持淡出效果
    /// </summary>
    /// <param name="fadeOutTime">淡出时间（秒），-1 表示使用默认值</param>
    public void StopBGM(float fadeOutTime = -1f)
    {
        if (!isBGMPlaying) return;

        if (fadeOutTime < 0f) fadeOutTime = bgmFadeTime;

        if (fadeOutTime > 0f)
        {
            StartCoroutine(FadeBGM(0f, fadeOutTime, () =>
            {
                bgmSource.Stop();
                isBGMPlaying = false;
            }));
        }
        else
        {
            bgmSource.Stop();
            isBGMPlaying = false;
        }
    }

    /// <summary>
    /// 暂停 BGM 播放
    /// </summary>
    public void PauseBGM()
    {
        if (isBGMPlaying) bgmSource.Pause();
    }

    /// <summary>
    /// 恢复 BGM 播放
    /// </summary>
    public void ResumeBGM()
    {
        if (isBGMPlaying) bgmSource.UnPause();
    }

    /// <summary>
    /// 设置 BGM 音量
    /// </summary>
    /// <param name="volume">音量值（0-1）</param>
    public void SetBGMVolume(float volume)
    {
        currentBGMVolume = Mathf.Clamp01(volume);
        if (!isBGMFading && !isBGMMuted) bgmSource.volume = currentBGMVolume;
        LocalEventCenter.Instance.EventTrigger<float>("OnBGMVolumeChanged", currentBGMVolume);
        SaveSettings();
    }

    /// <summary>
    /// 设置 BGM 静音状态
    /// </summary>
    /// <param name="mute">是否静音</param>
    public void MuteBGM(bool mute)
    {
        isBGMMuted = mute;
        bgmSource.volume = mute ? 0f : currentBGMVolume;
        LocalEventCenter.Instance.EventTrigger<bool>("OnBGMuteChanged", isBGMMuted);
        SaveSettings();
    }

    /// <summary>
    /// BGM 淡入淡出协程
    /// </summary>
    /// <param name="targetVolume">目标音量</param>
    /// <param name="duration">淡入淡出时间（秒）</param>
    /// <param name="onComplete">淡入淡出完成后的回调</param>
    private IEnumerator FadeBGM(float targetVolume, float duration, System.Action onComplete = null)
    {
        isBGMFading = true;
        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        bgmSource.volume = targetVolume;
        isBGMFading = false;
        onComplete?.Invoke();
    }

    #endregion

    #region SFX 控制

    /// <summary>
    /// 播放 2D 音效
    /// </summary>
    /// <param name="clip">要播放的音频片段</param>
    /// <param name="volume">音量值（0-1）</param>
    /// <param name="pitch">音调（1.0 为正常）</param>
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        if (Time.time - lastSFXPlayTime < minSfxInterval) return;
        lastSFXPlayTime = Time.time;

        float finalVolume = isSFXMuted ? 0f : volume * currentSFXVolume;
        sfxPool.Play(clip, Vector3.zero, finalVolume, pitch);

        LocalEventCenter.Instance.EventTrigger<string>("OnSFXPlayed", clip.name);
    }

    /// <summary>
    /// 在指定位置播放 3D 音效
    /// </summary>
    /// <param name="clip">要播放的音频片段</param>
    /// <param name="position">音效播放的世界坐标位置</param>
    /// <param name="volume">音量值（0-1）</param>
    /// <param name="pitch">音调（1.0 为正常）</param>
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        float finalVolume = isSFXMuted ? 0f : volume * currentSFXVolume;
        sfxPool.Play(clip, position, finalVolume, pitch);

        LocalEventCenter.Instance.EventTrigger<string>("OnSFXPlayedAtPosition", clip.name);
    }

    /// <summary>
    /// 随机播放一组音效中的一个
    /// </summary>
    /// <param name="clips">音频片段数组</param>
    /// <param name="volume">音量值（0-1）</param>
    /// <param name="pitchVariance">音调随机波动范围</param>
    public void PlayRandomSFX(AudioClip[] clips, float volume = 1f, float pitchVariance = 0f)
    {
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        float pitch = pitchVariance > 0f ? 1f + Random.Range(-pitchVariance, pitchVariance) : 1f;
        PlaySFX(clip, volume, pitch);
    }

    /// <summary>
    /// 设置 SFX 音量
    /// </summary>
    /// <param name="volume">音量值（0-1）</param>
    public void SetSFXVolume(float volume)
    {
        currentSFXVolume = Mathf.Clamp01(volume);
        sfxPool.SetMasterVolume(isSFXMuted ? 0f : currentSFXVolume);
        LocalEventCenter.Instance.EventTrigger<float>("OnSFXVolumeChanged", currentSFXVolume);
        SaveSettings();
    }

    /// <summary>
    /// 设置 SFX 静音状态
    /// </summary>
    /// <param name="mute">是否静音</param>
    public void MuteSFX(bool mute)
    {
        isSFXMuted = mute;
        sfxPool.SetMasterVolume(mute ? 0f : currentSFXVolume);
        LocalEventCenter.Instance.EventTrigger<bool>("OnSFXMuteChanged", isSFXMuted);
        SaveSettings();
    }

    #endregion

    #region 设置持久化

    /// <summary>
    /// 从 PlayerPrefs 加载音频设置
    /// 包括 BGM/SFX 音量和静音状态
    /// </summary>
    private void LoadSettings()
    {
        currentBGMVolume = PlayerPrefs.GetFloat("AudioManager_BGMVolume", defaultBGMVolume);
        currentSFXVolume = PlayerPrefs.GetFloat("AudioManager_SFXVolume", defaultSFXVolume);
        isBGMMuted = PlayerPrefs.GetInt("AudioManager_BGMMuted", 0) == 1;
        isSFXMuted = PlayerPrefs.GetInt("AudioManager_SFXMuted", 0) == 1;
    }

    /// <summary>
    /// 保存音频设置到 PlayerPrefs
    /// 在音量、静音状态变化时自动调用
    /// </summary>
    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("AudioManager_BGMVolume", currentBGMVolume);
        PlayerPrefs.SetFloat("AudioManager_SFXVolume", currentSFXVolume);
        PlayerPrefs.SetInt("AudioManager_BGMMuted", isBGMMuted ? 1 : 0);
        PlayerPrefs.SetInt("AudioManager_SFXMuted", isSFXMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    #endregion

    #region 查询接口

    /// <summary>
    /// BGM 是否正在播放
    /// </summary>
    public bool IsBGMPlaying => isBGMPlaying;

    /// <summary>
    /// 获取当前 BGM 音量
    /// </summary>
    public float GetBGMVolume => currentBGMVolume;

    /// <summary>
    /// 获取当前 SFX 音量
    /// </summary>
    public float GetSFXVolume => currentSFXVolume;

    /// <summary>
    /// BGM 是否静音
    /// </summary>
    public bool IsBGMMuted => isBGMMuted;

    /// <summary>
    /// SFX 是否静音
    /// </summary>
    public bool IsSFXMuted => isSFXMuted;

    /// <summary>
    /// 获取当前播放的 BGM 音频片段
    /// </summary>
    public AudioClip GetCurrentBGM => bgmSource.clip;

    #endregion
}
