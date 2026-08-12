using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// 特效包装器：支持多层级嵌套的复杂特效！
/// </summary>
public class CachedVFXWrapper
{
    private ParticleSystem[] particleSystems;
    private VisualEffect[] visualEffects;
    private Animator[] animators;

    public CachedVFXWrapper(GameObject obj)
    {
        particleSystems = obj.GetComponentsInChildren<ParticleSystem>();
        visualEffects = obj.GetComponentsInChildren<VisualEffect>();
        animators = obj.GetComponentsInChildren<Animator>();
    }

    public bool IsValid => particleSystems.Length > 0 || visualEffects.Length > 0 || animators.Length > 0;

    public void Play(string specificEvent = "")
    {
        foreach (var ps in particleSystems) ps.Play();
        foreach (var vfx in visualEffects)
        {
            // 【起死回生 1】：如果物体被隐藏了，强行激活它！
            if (!vfx.gameObject.activeSelf)
            {
                vfx.gameObject.SetActive(true);
            }

            // 【起死回生 2】：不管三七二十一，先唤醒休眠的 GPU 组件！
            // 调用 Play() 会让组件开机，并默认触发 Inspector 里配好的 Initial Event
            vfx.Play();

            // 【起死回生 3】：如果有特定事件，再额外精准发一次
            if (!string.IsNullOrEmpty(specificEvent))
            {
                // Unity 底层有时候字符串匹配会抽风，加上 PropertyToID 是最稳的 C++ 级底层通信
                vfx.SendEvent(Shader.PropertyToID(specificEvent));
            }
        }
        foreach (var anim in animators) anim.SetTrigger("Play");
    }

    public void Stop()
    {
        foreach (var ps in particleSystems) ps.Stop();
        foreach (var vfx in visualEffects) vfx.Stop();
        foreach (var anim in animators) anim.SetTrigger("Stop");
    }
}

public class MonsterVFXController : NetworkBehaviour
{
    [System.Serializable]
    public struct PrebakedVFX
    {
        [Tooltip("触发指令，例如 'EyeGlow'")]
        public string eventId;

        [Tooltip("提前摆在怪物骨骼下的特效 GameObject")]
        public GameObject vfxObject;
    }

    [Header("预拼装特效绑定")]
    public List<PrebakedVFX> prebakedEffects = new List<PrebakedVFX>();

    // 缓存字典：String -> 包装器
    private Dictionary<string, CachedVFXWrapper> cachedVFX = new Dictionary<string, CachedVFXWrapper>();

    private void Awake()
    {
        // 游戏开始时，直接扫描美术拖进来的物体，装进包装器
        foreach (var item in prebakedEffects)
        {
            if (string.IsNullOrEmpty(item.eventId) || item.vfxObject == null) continue;

            CachedVFXWrapper wrapper = new CachedVFXWrapper(item.vfxObject);

            if (wrapper.IsValid)
            {
                wrapper.Stop(); // 确保初始状态是关闭的
                cachedVFX.Add(item.eventId, wrapper);
            }
            else
            {
                Debug.LogWarning($"[VFX] 绑定的特效 {item.vfxObject.name} 缺少渲染组件！");
            }
        }
    }

    public void BroadcastVFX(string eventId)
    {
        if (IsServer) 
            TriggerVFXClientRpc(eventId);
        else
            TriggerVFXServerRpc(eventId);
    }

    [ServerRpc]
    private void TriggerVFXServerRpc(string eventId) 
    {
        TriggerVFXClientRpc(eventId);
    }
    [ClientRpc]
    private void TriggerVFXClientRpc(string eventId)
    {
        if (cachedVFX.TryGetValue(eventId, out CachedVFXWrapper targetVFX))
        {
            Debug.Log(eventId);
            targetVFX.Play(eventId); 
        }
        else
        {
            Debug.LogWarning($"[VFX] 未在怪物身上找到预拼装的特效绑定: {eventId}");
        }
    }
}
