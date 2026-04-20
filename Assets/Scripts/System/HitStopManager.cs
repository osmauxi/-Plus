using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitStopManager : MonoBehaviour
{
    public static HitStopManager Instance { get; private set; }

    // 记录每个 Animator 对应的“解冻时间戳”
    private Dictionary<Animator, float> frozenAnimators = new Dictionary<Animator, float>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // 每一帧检查是否有怪物该解冻了
        if (frozenAnimators.Count == 0) return;

        // 使用临时列表存储需要解冻的键，避免在遍历字典时修改字典引发报错
        List<Animator> toUnfreeze = new List<Animator>();
        float currentTime = Time.unscaledTime; // 必须使用不受 Time.timeScale 影响的时间

        foreach (var kvp in frozenAnimators)
        {
            if (currentTime >= kvp.Value)
            {
                toUnfreeze.Add(kvp.Key);
            }
        }

        foreach (var anim in toUnfreeze)
        {
            Unfreeze(anim);
        }
    }

    /// <summary>
    /// 触发局部顿帧
    /// </summary>
    /// <param name="animator">需要定格的动画机</param>
    /// <param name="duration">定格时间（秒）</param>
    public void Freeze(Animator animator, float duration)
    {
        if (animator == null) return;

        float unfreezeTime = Time.unscaledTime + duration;

        // 如果怪物已经在顿帧状态中，比较并保留更晚的解冻时间（防连击Bug）
        if (frozenAnimators.ContainsKey(animator))
        {
            if (unfreezeTime > frozenAnimators[animator])
            {
                frozenAnimators[animator] = unfreezeTime;
            }
        }
        else
        {
            // 第一次进入顿帧，记录时间并暂停动画
            frozenAnimators.Add(animator, unfreezeTime);
            animator.speed = 0f;
        }
    }

    private void Unfreeze(Animator animator)
    {
        if (animator != null)
        {
            animator.speed = 1f; // 恢复正常播放速度
        }
        frozenAnimators.Remove(animator);
    }
}