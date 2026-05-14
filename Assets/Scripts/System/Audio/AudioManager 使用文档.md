# AudioManager 使用文档

## 目录

1. [功能概述](#1-功能概述)
2. [快速开始](#2-快速开始)
3. [API 接口](#3-api-接口)
4. [事件系统](#4-事件系统)
5. [网络同步](#5-网络同步)
6. [配置说明](#6-配置说明)
7. [注意事项](#7-注意事项)
8. [常见问题](#8-常见问题)

---

## 1. 功能概述

AudioManager 是 Unity 项目的音频管理系统，负责所有音频的播放、音量控制和设置持久化。

### 核心特性

- **BGM 管理**：背景音乐播放、淡入淡出、暂停/恢复
- **SFX 管理**：音效对象池、2D/3D 音效、随机播放
- **音量控制**：BGM/SFX独立音量控制、静音开关
- **设置持久化**：自动保存玩家音频设置到 PlayerPrefs
- **事件驱动**：通过 LocalEventCenter 发送音频事件
- **跨场景保持**：DontDestroyOnLoad，场景切换音频不中断

### 文件结构

```
Assets/Scripts/System/Audio/
├── AudioManager.cs      # 主管理器
├── AudioPool.cs         # 音效对象池
└── AudioEvents.cs       # 音频事件结构体
```

---

## 2. 快速开始

### 2.1 场景配置

1. **创建 AudioManager GameObject**
   ```
   Hierarchy 右键 → Create Empty → 命名为 "AudioManager"
   Add Component → AudioManager
   ```

2. **放置位置**
   - 放入初始场景（如 LoadingScene）
   - 脚本会自动执行 `DontDestroyOnLoad`

3. **参数配置**（可选，有默认值）
   ```
   BGM 设置:
   ├── BGM Fade Time: 1          // BGM 淡入淡出时间（秒）
   
   SFX 设置:
   ├── SFX Pool Size: 10         // 音效对象池初始大小
   └── Min SFX Interval: 0.05    // 音效播放最小间隔
   
   音量默认值:
   ├── Default BGM Volume: 0.7   // BGM 默认音量
   └── Default SFX Volume: 0.8   // SFX 默认音量
   ```

### 2.2 基础使用

```csharp
using UnityEngine;

public class TestAudio : MonoBehaviour
{
    public AudioClip bgmClip;
    public AudioClip sfxClip;
    
    void Start()
    {
        // 播放 BGM，2 秒淡入
        AudioManager.instance.PlayBGM(bgmClip, 2f);
        
        // 播放音效
        AudioManager.instance.PlaySFX(sfxClip, 0.8f);
    }
}
```

---

## 3. API 接口

### 3.1 BGM 控制

#### PlayBGM
```csharp
/// 播放背景音乐，支持淡入效果
/// <param name="clip">音频片段</param>
/// <param name="fadeInTime">淡入时间（秒），-1 使用默认值</param>
AudioManager.instance.PlayBGM(AudioClip clip, float fadeInTime = -1f);
```

#### StopBGM
```csharp
/// 停止背景音乐，支持淡出效果
/// <param name="fadeOutTime">淡出时间（秒），-1 使用默认值</param>
AudioManager.instance.StopBGM(float fadeOutTime = -1f);
```

#### PauseBGM / ResumeBGM
```csharp
AudioManager.instance.PauseBGM();   // 暂停
AudioManager.instance.ResumeBGM();  // 恢复
```

#### SetBGMVolume / MuteBGM
```csharp
AudioManager.instance.SetBGMVolume(0.5f);  // 设置音量 (0-1)
AudioManager.instance.MuteBGM(true);       // 静音
```

### 3.2 SFX 控制

#### PlaySFX
```csharp
/// 播放 2D 音效
/// <param name="clip">音频片段</param>
/// <param name="volume">音量 (0-1)</param>
/// <param name="pitch">音调 (1.0 正常)</param>
AudioManager.instance.PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f);
```

#### PlaySFXAtPosition
```csharp
/// 播放 3D 音效
/// <param name="clip">音频片段</param>
/// <param name="position">世界坐标位置</param>
/// <param name="volume">音量 (0-1)</param>
/// <param name="pitch">音调 (1.0 正常)</param>
AudioManager.instance.PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f);
```

#### PlayRandomSFX
```csharp
/// 随机播放一组音效中的一个
/// <param name="clips">音频片段数组</param>
/// <param name="volume">音量 (0-1)</param>
/// <param name="pitchVariance">音调随机波动范围</param>
AudioManager.instance.PlayRandomSFX(AudioClip[] clips, float volume = 1f, float pitchVariance = 0f);
```

#### SetSFXVolume / MuteSFX
```csharp
AudioManager.instance.SetSFXVolume(0.8f);  // 设置音量 (0-1)
AudioManager.instance.MuteSFX(true);       // 静音
```

### 3.3 查询接口

```csharp
bool isPlaying = AudioManager.instance.IsBGMPlaying;      // BGM 是否播放
float bgmVol = AudioManager.instance.GetBGMVolume;        // BGM 音量
float sfxVol = AudioManager.instance.GetSFXVolume;        // SFX 音量
bool bgmMuted = AudioManager.instance.IsBGMMuted;         // BGM 是否静音
bool sfxMuted = AudioManager.instance.IsSFXMuted;         // SFX 是否静音
AudioClip current = AudioManager.instance.GetCurrentBGM;  // 当前 BGM
```

---

## 4. 事件系统

### 4.1 订阅音频事件

```csharp
using UnityEngine;

public class AudioEventListener : MonoBehaviour
{
    void OnEnable()
    {
        // 订阅 BGM 切换事件
        LocalEventCenter.Instance.AddEventListener<AudioClip>("OnBGMChanged", HandleBGMChanged);
        
        // 订阅音效播放事件
        LocalEventCenter.Instance.AddEventListener<string>("OnSFXPlayed", HandleSFXPlayed);
        
        // 订阅音量变化事件
        LocalEventCenter.Instance.AddEventListener<float>("OnBGMVolumeChanged", HandleBGMVolumeChanged);
    }
    
    void OnDisable()
    {
        // 取消订阅（重要，防止内存泄漏）
        LocalEventCenter.Instance.RemoveEventListener<AudioClip>("OnBGMChanged", HandleBGMChanged);
        LocalEventCenter.Instance.RemoveEventListener<string>("OnSFXPlayed", HandleSFXPlayed);
        LocalEventCenter.Instance.RemoveEventListener<float>("OnBGMVolumeChanged", HandleBGMVolumeChanged);
    }
    
    void HandleBGMChanged(AudioClip newClip)
    {
        Debug.Log($"BGM 切换为：{newClip.name}");
    }
    
    void HandleSFXPlayed(string sfxName)
    {
        Debug.Log($"播放音效：{sfxName}");
    }
    
    void HandleBGMVolumeChanged(float volume)
    {
        Debug.Log($"BGM 音量：{volume}");
    }
}
```

### 4.2 事件列表

| 事件名 | 数据类型 | 触发时机 |
|--------|----------|----------|
| `OnBGMChanged` | `AudioClip` | BGM 切换时 |
| `OnSFXPlayed` | `string` | 2D 音效播放时 |
| `OnSFXPlayedAtPosition` | `string` | 3D 音效播放时 |
| `OnBGMVolumeChanged` | `float` | BGM 音量变化时 |
| `OnSFXVolumeChanged` | `float` | SFX 音量变化时 |
| `OnBGMuteChanged` | `bool` | BGM 静音状态变化时 |
| `OnSFXMuteChanged` | `bool` | SFX 静音状态变化时 |

---

## 5. 网络同步

### 5.1 使用 AudioEvents 结构体

```csharp
using UnityEngine;
using Unity.Netcode;

public struct AudioPlayNetEvent : INetEvent
{
    public string audioName;
    public Vector3 position;
    public bool isBGM;
    public bool AutoBroadcast => true;  // 自动广播给所有客户端
}
```

### 5.2 网络音频同步示例

```csharp
public class NetworkAudioSync : NetworkBehaviour
{
    public AudioClip sharedAudio;
    
    [ServerRpc]
    void RequestPlaySFXServerRpc()
    {
        // 服务器收到请求后，广播给所有客户端播放
        NetEventCenter.Instance.Send(new AudioPlayNetEvent 
        { 
            audioName = sharedAudio.name,
            position = transform.position,
            isBGM = false 
        });
    }
    
    void OnEnable()
    {
        NetEventCenter.Instance.Subscribe<AudioPlayNetEvent>(HandleAudioPlay);
    }
    
    void OnDisable()
    {
        NetEventCenter.Instance.Unsubscribe<AudioPlayNetEvent>(HandleAudioPlay);
    }
    
    void HandleAudioPlay(AudioPlayNetEvent evt, ulong senderId)
    {
        if (!NetUtils.Filter(evt, senderId, true)) return;
        
        // 所有客户端同步播放音效
        AudioClip clip = Resources.Load<AudioClip>("Audio/" + evt.audioName);
        if (evt.isBGM)
            AudioManager.instance.PlayBGM(clip);
        else
            AudioManager.instance.PlaySFXAtPosition(clip, evt.position);
    }
}
```

---

## 6. 配置说明

### 6.1 Inspector 配置

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `BGM Source` | AudioSource | 自动生成 | BGM 专用音源 |
| `BGM Fade Time` | float | 1 | BGM 淡入淡出时间 |
| `SFX Pool Size` | int | 10 | 音效对象池大小 |
| `Min SFX Interval` | float | 0.05 | 音效播放间隔 |
| `Default BGM Volume` | float | 0.7 | BGM 默认音量 |
| `Default SFX Volume` | float | 0.8 | SFX 默认音量 |

### 6.2 PlayerPrefs 存储

| 键名 | 类型 | 说明 |
|------|------|------|
| `AudioManager_BGMVolume` | float | BGM 音量 |
| `AudioManager_SFXVolume` | float | SFX 音量 |
| `AudioManager_BGMMuted` | int (0/1) | BGM 静音 |
| `AudioManager_SFXMuted` | int (0/1) | SFX 静音 |

### 6.3 读取设置

```csharp
float bgmVolume = PlayerPrefs.GetFloat("AudioManager_BGMVolume", 0.7f);
float sfxVolume = PlayerPrefs.GetFloat("AudioManager_SFXVolume", 0.8f);
bool bgmMuted = PlayerPrefs.GetInt("AudioManager_BGMMuted", 0) == 1;
bool sfxMuted = PlayerPrefs.GetInt("AudioManager_SFXMuted", 0) == 1;
```

---

## 7. 注意事项

### 7.1 单例访问

```csharp
// ✅ 正确：使用小写 instance
AudioManager.instance.PlaySFX(clip);

// ❌ 错误：大写 Instance 会报错
AudioManager.Instance.PlaySFX(clip);
```

### 7.2 对象池优化

- 默认 SFX 对象池大小为 10
- 如果同屏音效较多，建议增大到 20-30
- 对象池会自动扩容，但预先设置更好

### 7.3 3D 音效

- `PlaySFX` 播放 2D 音效（无空间感）
- `PlaySFXAtPosition` 播放 3D 音效（有距离衰减）
- 3D 音效最大距离 50 米

### 7.4 资源加载

```csharp
// 方式 1：Inspector 拖入
public AudioClip clip;

// 方式 2：Resources 加载
AudioClip clip = Resources.Load<AudioClip>("Audio/clipName");

// 方式 3：Addressables（需自行集成）
// var handle = Addressables.LoadAssetAsync<AudioClip>("clipName");
```

### 7.5 内存管理

- 事件订阅后务必在 `OnDisable` 取消订阅
- AudioClip 使用完后及时释放（Addressables）
- 对象池无需手动管理，自动回收

---

## 8. 常见问题

### Q1: 场景切换后 BGM 中断？

**A**: 确保 AudioManager GameObject 在初始场景，脚本会自动 `DontDestroyOnLoad`。

### Q2: 音效播放有延迟？

**A**: 
1. 检查 `Min SFX Interval` 是否设置过小
2. 增大 `SFX Pool Size` 避免对象池不足
3. 预加载常用音效到内存

### Q3: 音量设置不保存？

**A**: PlayerPrefs 在应用关闭时自动保存，也可手动调用 `PlayerPrefs.Save()`。

### Q4: 多个 AudioManager 实例？

**A**: 单例会自动销毁重复实例，确保场景只有一个 AudioManager。

### Q5: 如何集成 AudioMixer？

**A**: 当前版本未集成 AudioMixer，可在 `AudioPool.CreateNewPoolItem()` 中添加 `audioMixer` 设置。

---

## 附录：完整示例

```csharp
using UnityEngine;
using UnityEngine.UI;

public class AudioManagerDemo : MonoBehaviour
{
    [Header("音频引用")]
    public AudioClip mainBGM;
    public AudioClip[] footstepClips;
    public AudioClip explosionSFX;
    
    [Header("UI 引用")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Toggle bgmMuteToggle;
    public Toggle sfxMuteToggle;
    
    void Start()
    {
        // 播放 BGM
        AudioManager.instance.PlayBGM(mainBGM, 2f);
        
        // 初始化 UI
        bgmSlider.value = AudioManager.instance.GetBGMVolume;
        sfxSlider.value = AudioManager.instance.GetSFXVolume;
        bgmMuteToggle.isOn = AudioManager.instance.IsBGMMuted;
        sfxMuteToggle.isOn = AudioManager.instance.IsSFXMuted;
    }
    
    public void OnFootstep()
    {
        AudioManager.instance.PlayRandomSFX(footstepClips, 0.5f, 0.1f);
    }
    
    public void OnExplosion()
    {
        AudioManager.instance.PlaySFXAtPosition(explosionSFX, transform.position, 1f);
    }
    
    public void OnBGMVolumeChanged(float value)
    {
        AudioManager.instance.SetBGMVolume(value);
    }
    
    public void OnSFXVolumeChanged(float value)
    {
        AudioManager.instance.SetSFXVolume(value);
    }
    
    public void OnBGMuteChanged(bool isMuted)
    {
        AudioManager.instance.MuteBGM(isMuted);
    }
    
    public void OnSFXMuteChanged(bool isMuted)
    {
        AudioManager.instance.MuteSFX(isMuted);
    }
}
```

---

**文档版本**: 1.0  
**最后更新**: 2026-05-11  
**维护者**: 系统组
