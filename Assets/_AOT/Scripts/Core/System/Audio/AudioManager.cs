using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum BGMType { Title, Gameplay, Boss, Victory }

public enum AudioCategory
{
    BGM, SFX_Weapon, SFX_Footstep, SFX_UI, SFX_Environment, SFX_Monster, Voice,
    SFX_Bullet_Lightning, SFX_Bullet_Explosion, SFX_Bullet_Laser, SFX_Bullet_Normal,
    SFX_Monster_Walk, SFX_Monster_Roar, SFX_Monster_Lunge, SFX_Player_Walk, SFX_Reload,
    SFX_Footstep_Poison, SFX_Footstep_Ice, SFX_Footstep_Lava, SFX_Bullet_Hit, SFX_Shell_Drop,
    SFX_Bullet_Hit_Wall, SFX_Player_Hurt, SFX_Monster_Hurt, SFX_Chest_Open,
    SFX_Portal_Activate, SFX_Portal_Teleport,
    SFX_Skill_ThunderCloud, SFX_Skill_LightningHit, SFX_Skill_Explosion
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("BGM 设置")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource titleBGMSource;
    [SerializeField] private AudioSource gameplayBGMSource;
    [SerializeField] private float bgmFadeTime = 1f;

    [Header("SFX 设置")]
    [SerializeField] private int sfxPoolSize = 100;
    [SerializeField] private float minSfxInterval = 0.05f;

    [Header("音量默认值")]
    [SerializeField, Range(0f, 1f)] private float defaultBGMVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float defaultSFXVolume = 0.8f;

    [Header("音频配置")]
    [SerializeField] private AudioConfigSO audioConfig;

    private AudioMixer runtimeAudioMixer;
    private AudioMixerGroup musicMixerGroup;
    private AudioMixerGroup sfxMixerGroup;

    private AudioPool sfxPool;
    private float currentBGMVolume;
    private float currentSFXVolume;

    // ==========================================
    // 【核心修复】：将全局冷却改为“每首音效独立的冷却字典”
    // 这样爆炸声和子弹命中声在同一帧播放时，就不会互相吞音了！
    // ==========================================
    private Dictionary<AudioClip, float> clipPlayTimes = new Dictionary<AudioClip, float>();

    private bool isBGMPlaying;
    private bool isBGMFading;
    private bool isSFXMuted;
    private bool isBGMMuted;
    private BGMType currentBGMType;
    private Coroutine bgmFadeCoroutine;

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
    /// 创建音频源、连接 Mixer Group 并恢复 AudioManager 自身状态 
    /// </summary>
    private void Initialize()
    {
        runtimeAudioMixer = Resources.Load<AudioMixer>("Audio/LobbyAudioMixer");
        musicMixerGroup = runtimeAudioMixer.FindMatchingGroups("Music")[0];
        sfxMixerGroup = runtimeAudioMixer.FindMatchingGroups("SFX")[0];

        if (titleBGMSource == null)
        {
            GameObject titleObj = new GameObject("TitleBGMSource");
            titleObj.transform.SetParent(transform);
            titleBGMSource = titleObj.AddComponent<AudioSource>();
            titleBGMSource.loop = true;
            titleBGMSource.playOnAwake = false;
        }

        if (gameplayBGMSource == null)
        {
            GameObject gameplayObj = new GameObject("GameplayBGMSource");
            gameplayObj.transform.SetParent(transform);
            gameplayBGMSource = gameplayObj.AddComponent<AudioSource>();
            gameplayBGMSource.loop = true;
            gameplayBGMSource.playOnAwake = false;
        }

        bgmSource = titleBGMSource;
        titleBGMSource.outputAudioMixerGroup = musicMixerGroup;
        gameplayBGMSource.outputAudioMixerGroup = musicMixerGroup;

        LoadSettings();
        titleBGMSource.volume = isBGMMuted ? 0f : currentBGMVolume;
        gameplayBGMSource.volume = isBGMMuted ? 0f : currentBGMVolume;

        sfxPool = new AudioPool(sfxPoolSize, transform, sfxMixerGroup);
        sfxPool.SetMasterVolume(isSFXMuted ? 0f : currentSFXVolume);

        LocalEventCenter.Instance.AddEventListener<float>("OnBGMVolumeChanged", HandleBGMVolumeChanged);
        LocalEventCenter.Instance.AddEventListener<float>("OnSFXVolumeChanged", HandleSFXVolumeChanged);
        LocalEventCenter.Instance.AddEventListener<bool>("OnBGMuteChanged", HandleBGMMuteChanged);
        LocalEventCenter.Instance.AddEventListener<bool>("OnSFXMuteChanged", HandleSFXMuteChanged);
    }

    private void OnDestroy()
    {
        LocalEventCenter.Instance.RemoveEventListener<float>("OnBGMVolumeChanged", HandleBGMVolumeChanged);
        LocalEventCenter.Instance.RemoveEventListener<float>("OnSFXVolumeChanged", HandleSFXVolumeChanged);
        LocalEventCenter.Instance.RemoveEventListener<bool>("OnBGMuteChanged", HandleBGMMuteChanged);
        LocalEventCenter.Instance.RemoveEventListener<bool>("OnSFXMuteChanged", HandleSFXMuteChanged);
    }

    private void HandleBGMVolumeChanged(float volume) => SetBGMVolume(volume);
    private void HandleSFXVolumeChanged(float volume) => SetSFXVolume(volume);
    private void HandleBGMMuteChanged(bool muted) => MuteBGM(muted);
    private void HandleSFXMuteChanged(bool muted) => MuteSFX(muted);

    private void Update()
    {
        sfxPool?.Update();
    }

    #region BGM 控制
    public void SwitchBGM(BGMType type, AudioClip clip = null, float fadeTime = -1f)
    {
        if (fadeTime < 0f) fadeTime = bgmFadeTime;
        if (currentBGMType == type && isBGMPlaying) return;

        AudioSource targetSource = type switch
        {
            BGMType.Title => titleBGMSource,
            BGMType.Gameplay => gameplayBGMSource,
            _ => titleBGMSource
        };

        if (clip == null && audioConfig != null)
        {
            clip = type switch
            {
                BGMType.Title => audioConfig.titleBGM,
                BGMType.Gameplay => audioConfig.gameplayBGM,
                BGMType.Boss => audioConfig.bossBGM,
                BGMType.Victory => audioConfig.victoryBGM,
                _ => null
            };
        }

        if (clip == null) return;

        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = null;
        }

        if (isBGMPlaying && bgmSource != null && bgmSource.isPlaying)
        {
            bgmFadeCoroutine = StartCoroutine(CrossFadeBGM(bgmSource, targetSource, clip, fadeTime));
        }
        else
        {
            bgmSource = targetSource;
            bgmSource.clip = clip;
            bgmSource.volume = isBGMMuted ? 0f : 0f;
            bgmSource.Play();
            bgmFadeCoroutine = StartCoroutine(FadeBGM(isBGMMuted ? 0f : currentBGMVolume, fadeTime));
        }

        currentBGMType = type;
        isBGMPlaying = true;
        LocalEventCenter.Instance.EventTrigger<AudioClip>("OnBGMChanged", clip);
    }

    public void PlayBGM(AudioClip clip, float fadeInTime = -1f) => SwitchBGM(BGMType.Gameplay, clip, fadeInTime);

    public void StopBGM(float fadeOutTime = -1f)
    {
        if (!isBGMPlaying) return;

        if (fadeOutTime < 0f) fadeOutTime = bgmFadeTime;

        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = null;
        }

        if (fadeOutTime > 0f)
        {
            bgmFadeCoroutine = StartCoroutine(FadeBGM(0f, fadeOutTime, () =>
            {
                titleBGMSource.Stop();
                gameplayBGMSource.Stop();
                isBGMPlaying = false;
            }));
        }
        else
        {
            titleBGMSource.Stop();
            gameplayBGMSource.Stop();
            isBGMPlaying = false;
        }
    }

    public void PauseBGM()
    {
        if (isBGMPlaying)
        {
            titleBGMSource.Pause();
            gameplayBGMSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (isBGMPlaying)
        {
            titleBGMSource.UnPause();
            gameplayBGMSource.UnPause();
        }
    }

    public void SetBGMVolume(float volume)
    {
        currentBGMVolume = Mathf.Clamp01(volume);
        if (!isBGMFading && !isBGMMuted)
        {
            titleBGMSource.volume = currentBGMVolume;
            gameplayBGMSource.volume = currentBGMVolume;
        }
        LocalEventCenter.Instance.EventTrigger<float>("OnBGMVolumeChanged", currentBGMVolume);
        SaveSettings();
    }

    public void MuteBGM(bool mute)
    {
        isBGMMuted = mute;
        float volume = mute ? 0f : currentBGMVolume;
        titleBGMSource.volume = volume;
        gameplayBGMSource.volume = volume;
        LocalEventCenter.Instance.EventTrigger<bool>("OnBGMuteChanged", isBGMMuted);
        SaveSettings();
    }

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
        bgmFadeCoroutine = null;
    }

    private IEnumerator CrossFadeBGM(AudioSource oldSource, AudioSource newSource, AudioClip newClip, float duration)
    {
        isBGMFading = true;
        float startVolume = oldSource.volume;
        float elapsed = 0f;

        newSource.clip = newClip;
        newSource.volume = 0f;
        newSource.Play();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            oldSource.volume = Mathf.Lerp(startVolume, 0f, t);
            newSource.volume = Mathf.Lerp(0f, isBGMMuted ? 0f : currentBGMVolume, t);
            yield return null;
        }

        oldSource.volume = 0f;
        oldSource.Stop();
        newSource.volume = isBGMMuted ? 0f : currentBGMVolume;
        bgmSource = newSource;
        isBGMFading = false;
        bgmFadeCoroutine = null;
    }
    #endregion

    #region SFX 控制
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        // 【核心修复】：为每一个 AudioClip 提供独立的冷却判定！
        if (clipPlayTimes.TryGetValue(clip, out float lastTime))
        {
            if (Time.time - lastTime < minSfxInterval) return;
        }
        clipPlayTimes[clip] = Time.time;

        float finalVolume = isSFXMuted ? 0f : volume * currentSFXVolume;
        sfxPool.Play(clip, Vector3.zero, finalVolume, pitch);

        LocalEventCenter.Instance.EventTrigger<string>("OnSFXPlayed", clip.name);
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        // 3D 音效同样受独立冷却保护
        if (clipPlayTimes.TryGetValue(clip, out float lastTime))
        {
            if (Time.time - lastTime < minSfxInterval) return;
        }
        clipPlayTimes[clip] = Time.time;

        float finalVolume = isSFXMuted ? 0f : volume * currentSFXVolume;
        sfxPool.Play(clip, position, finalVolume, pitch);

        LocalEventCenter.Instance.EventTrigger<string>("OnSFXPlayedAtPosition", clip.name);
    }

    public void PlayRandomSFX(AudioClip[] clips, float volume = 1f, float pitchVariance = 0f)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        float pitch = pitchVariance > 0f ? 1f + Random.Range(-pitchVariance, pitchVariance) : 1f;
        PlaySFX(clip, volume, pitch);
    }

    public void PlaySFXByCategory(AudioCategory category, float volume = 1f)
    {
        if (audioConfig == null) return;

        AudioClip clip = category switch
        {
            AudioCategory.SFX_Bullet_Lightning => audioConfig.lightningBullet,
            AudioCategory.SFX_Bullet_Explosion => audioConfig.explosionBullet,
            AudioCategory.SFX_Bullet_Laser => audioConfig.laserBullet,
            AudioCategory.SFX_Bullet_Normal => audioConfig.normalBullet,
            AudioCategory.SFX_Monster_Walk => GetRandomClip(audioConfig.monsterWalk),
            AudioCategory.SFX_Monster_Roar => GetRandomClip(audioConfig.monsterRoar),
            AudioCategory.SFX_Monster_Lunge => GetRandomClip(audioConfig.monsterLunge),
            AudioCategory.SFX_Player_Walk => GetRandomClip(audioConfig.playerWalk),
            AudioCategory.SFX_Reload => audioConfig.reload,
            AudioCategory.SFX_Footstep_Poison => GetRandomClip(audioConfig.footstepPoison),
            AudioCategory.SFX_Footstep_Ice => GetRandomClip(audioConfig.footstepIce),
            AudioCategory.SFX_Footstep_Lava => GetRandomClip(audioConfig.footstepLava),
            AudioCategory.SFX_Bullet_Hit => audioConfig.bulletHit,
            AudioCategory.SFX_Bullet_Hit_Wall => audioConfig.bulletHitWall,
            AudioCategory.SFX_Player_Hurt => GetRandomClip(audioConfig.playerHurt),
            AudioCategory.SFX_Monster_Hurt => GetRandomClip(audioConfig.monsterHurt),
            AudioCategory.SFX_Chest_Open => audioConfig.chestOpen,
            AudioCategory.SFX_Portal_Activate => audioConfig.portalActivate,
            AudioCategory.SFX_Portal_Teleport => audioConfig.levelUp,
            AudioCategory.SFX_Skill_ThunderCloud => audioConfig.skillThunderCloud,
            AudioCategory.SFX_Skill_LightningHit => audioConfig.skillLightningHit,
            AudioCategory.SFX_Skill_Explosion => audioConfig.skillExplosion,
            _ => null
        };

        if (clip != null) PlaySFX(clip, volume);
    }

    public void SetSFXVolume(float volume)
    {
        currentSFXVolume = Mathf.Clamp01(volume);
        sfxPool.SetMasterVolume(isSFXMuted ? 0f : currentSFXVolume);
        LocalEventCenter.Instance.EventTrigger<float>("OnSFXVolumeChanged", currentSFXVolume);
        SaveSettings();
    }

    public void MuteSFX(bool mute)
    {
        isSFXMuted = mute;
        sfxPool.SetMasterVolume(mute ? 0f : currentSFXVolume);
        LocalEventCenter.Instance.EventTrigger<bool>("OnSFXMuteChanged", isSFXMuted);
        SaveSettings();
    }
    #endregion

    #region 设置持久化
    private void LoadSettings()
    {
        currentBGMVolume = PlayerPrefs.GetFloat("AudioManager_BGMVolume", defaultBGMVolume);
        currentSFXVolume = PlayerPrefs.GetFloat("AudioManager_SFXVolume", defaultSFXVolume);
        isBGMMuted = PlayerPrefs.GetInt("AudioManager_BGMMuted", 0) == 1;
        isSFXMuted = PlayerPrefs.GetInt("AudioManager_SFXMuted", 0) == 1;
    }

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
    public bool IsBGMPlaying => isBGMPlaying;
    public float GetBGMVolume => currentBGMVolume;
    public float GetSFXVolume => currentSFXVolume;
    public bool IsBGMMuted => isBGMMuted;
    public bool IsSFXMuted => isSFXMuted;
    public AudioClip GetCurrentBGM => bgmSource.clip;
    public BGMType CurrentBGMType => currentBGMType;
    #endregion

    #region 辅助方法
    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
    #endregion
}
