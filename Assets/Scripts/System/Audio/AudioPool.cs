using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音效对象池 - 负责管理 AudioSource 的复用，减少频繁创建销毁带来的性能开销
/// 支持 2D 和 3D 音效，自动处理播放完成后的状态回收
/// </summary>
public class AudioPool
{
    /// <summary>
    /// 对象池中的单个项目，包含 AudioSource 和播放状态
    /// </summary>
    private class PoolItem
    {
        public AudioSource source;   // 音频源组件
        public bool isPlaying;       // 是否正在播放

        /// <summary>
        /// 构造函数，初始化 PoolItem
        /// </summary>
        /// <param name="source">要管理的 AudioSource</param>
        public PoolItem(AudioSource source)
        {
            this.source = source;
            this.isPlaying = false;
        }
    }

    private readonly List<PoolItem> pool;  // 对象池列表
    private readonly Transform parent;     // 父节点，用于组织层级结构
    private float masterVolume = 1f;       // 主音量控制，影响池中所有音效

    /// <summary>
    /// 构造函数，初始化音效对象池
    /// </summary>
    /// <param name="initialSize">初始对象池大小</param>
    /// <param name="parent">父节点 Transform，用于挂载生成的 AudioSource</param>
    public AudioPool(int initialSize, Transform parent)
    {
        this.parent = parent;
        pool = new List<PoolItem>(initialSize);

        for (int i = 0; i < initialSize; i++)
        {
            PoolItem item = CreateNewPoolItem();
            pool.Add(item);
        }
    }

    /// <summary>
    /// 创建新的对象池项目
    /// 配置 AudioSource 的基本参数
    /// </summary>
    /// <returns>新创建的 PoolItem 实例</returns>
    private PoolItem CreateNewPoolItem()
    {
        GameObject obj = new GameObject("SFXSource");
        obj.transform.SetParent(parent);

        AudioSource source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.maxDistance = 50f;

        return new PoolItem(source);
    }

    /// <summary>
    /// 播放音效
    /// 从对象池中获取空闲的 AudioSource 进行播放
    /// </summary>
    /// <param name="clip">要播放的音频片段</param>
    /// <param name="position">播放位置，Vector3.zero 表示 2D 音效，非零表示 3D 音效</param>
    /// <param name="volume">音量值（0-1）</param>
    /// <param name="pitch">音调（1.0 为正常）</param>
    public void Play(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        PoolItem item = GetFreePoolItem();

        item.source.clip = clip;
        item.source.transform.position = position;
        item.source.volume = volume * masterVolume;
        item.source.pitch = pitch;
        item.source.spatialBlend = position.magnitude > 0.01f ? 1f : 0f;
        item.isPlaying = true;

        item.source.Play();
    }

    /// <summary>
    /// 设置对象池主音量
    /// 影响池中所有正在播放和即将播放的音效
    /// </summary>
    /// <param name="volume">音量值（0-1）</param>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// 清空对象池
    /// 停止所有播放并销毁所有 AudioSource GameObject
    /// </summary>
    public void Clear()
    {
        foreach (PoolItem item in pool)
        {
            if (item.source.isPlaying) item.source.Stop();
            Object.Destroy(item.source.gameObject);
        }
        pool.Clear();
    }

    /// <summary>
    /// 更新方法，由 AudioManager 每帧调用
    /// 检查并回收已播放完成的 AudioSource 状态
    /// </summary>
    public void Update()
    {
        foreach (PoolItem item in pool)
        {
            if (item.isPlaying && !item.source.isPlaying)
            {
                item.isPlaying = false;
                item.source.clip = null;
            }
        }
    }

    /// <summary>
    /// 获取空闲的对象池项目
    /// 如果没有空闲项目，则自动扩容创建新项目
    /// </summary>
    /// <returns>可用的 PoolItem 实例</returns>
    private PoolItem GetFreePoolItem()
    {
        foreach (PoolItem item in pool)
        {
            if (!item.isPlaying)
            {
                return item;
            }
        }

        PoolItem newItem = CreateNewPoolItem();
        pool.Add(newItem);
        return newItem;
    }
}
