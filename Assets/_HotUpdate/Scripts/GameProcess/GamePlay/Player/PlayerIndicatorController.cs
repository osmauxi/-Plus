using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerIndicatorController : NetworkBehaviour
{
    // 全局单例，方便目标主动找上门
    public static PlayerIndicatorController LocalInstance;

    [Header("对象池配置")]
    public string arrowPoolId = "IndicatorArrow"; // 你的对象池ID

    [Header("环绕配置")]
    public float ringRadius = 2.0f;       // 箭头离玩家中心的距离
    public float heightOffset = 1.0f;     // 腰部高度偏移

    [Header("透明度淡化 (距离)")]
    public float fadeStartDist = 3f;      // 小于 3 米时完全透明
    public float fadeEndDist = 10f;       // 大于 10 米时完全不透明 (Alpha=1)

    [Header("颜色配置")]
    public Color teammateColor = Color.green;
    public Color monsterColor = Color.red;
    public Color chestColor = new Color(1f, 0.8f, 0f); // 金色

    // 追踪字典：目标 Transform -> 对应的箭头组件
    private Dictionary<Transform, IndicatorArrow> activeArrows = new Dictionary<Transform, IndicatorArrow>();

    // 队友专属追踪
    private PlayerController teammateTarget;
    private IndicatorArrow teammateArrow;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }
        else
        {
            // 非本地玩家不需要跑这个脚本
            this.enabled = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner && LocalInstance == this)
        {
            LocalInstance = null;
        }
    }

    // ==========================================
    // 目标注册与注销 (由 TargetableIndicator 呼叫)
    // ==========================================
    public void RegisterTarget(Transform target, IndicatorType type)
    {
        if (activeArrows.ContainsKey(target)) return;
        Debug.Log(1122);
        // 从对象池拿出一个箭头
        GameObject arrowObj = LocalObjectPool.instance.GetT(arrowPoolId, transform.position);
        if (arrowObj != null && arrowObj.TryGetComponent<IndicatorArrow>(out var arrowComp))
        {
            Color targetColor = type == IndicatorType.Monster ? monsterColor : chestColor;
            arrowComp.Setup(targetColor);
            activeArrows.Add(target, arrowComp);
        }
    }

    public void UnregisterTarget(Transform target)
    {
        if (activeArrows.TryGetValue(target, out IndicatorArrow arrow))
        {
            activeArrows.Remove(target);
            // 归还到对象池
            LocalObjectPool.instance.RetToPool(arrow.gameObject);
        }
    }

    // ==========================================
    // 纯表现层：在 LateUpdate 中计算环绕与淡化
    // ==========================================
    private void LateUpdate()
    {
        // 1. 动态寻找队友 (如果队友中途加入或复活)
        CheckTeammate();

        Vector3 waistCenter = transform.position + Vector3.up * heightOffset;

        // 2. 更新队友箭头
        if (teammateTarget != null && teammateArrow != null)
        {
            UpdateArrowTransform(teammateArrow, waistCenter, teammateTarget.transform.position);
        }

        // 3. 更新所有动态目标的箭头 (怪物/宝箱)
        // 使用 List 缓存 Key 防止在遍历时字典被修改报错
        List<Transform> invalidTargets = null;

        foreach (var kvp in activeArrows)
        {
            Transform target = kvp.Key;
            IndicatorArrow arrow = kvp.Value;

            if (target == null || !target.gameObject.activeInHierarchy)
            {
                if (invalidTargets == null) invalidTargets = new List<Transform>();
                invalidTargets.Add(target);
                continue;
            }

            UpdateArrowTransform(arrow, waistCenter, target.position);
        }

        // 4. 清理意外丢失的目标 (比如被直接 Destroy 的怪物)
        if (invalidTargets != null)
        {
            foreach (var t in invalidTargets) UnregisterTarget(t);
        }
    }

    private void UpdateArrowTransform(IndicatorArrow arrow, Vector3 center, Vector3 targetPos)
    {
        // 算方向 (忽略 Y 轴高度差，保持箭头在同一水平面)
        Vector3 dir = (targetPos - center);
        dir.y = 0;
        float distance = dir.magnitude;

        if (distance > 0.01f)
        {
            dir.Normalize();

            // 摆放位置：以玩家为中心，沿方向推出去 radius 的距离
            arrow.transform.position = center + dir * ringRadius;

            // 旋转：让 Sprite 的 Y 轴 (或者 Z 轴) 指向目标
            // 假设你的 2D Sprite 是朝上的，我们用 LookRotation 配合转正角度
            arrow.transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(90, 0, 0);
        }

        // 计算距离淡化
        float alpha = Mathf.Clamp01((distance - fadeStartDist) / (fadeEndDist - fadeStartDist));
        arrow.SetAlpha(alpha);
    }

    private void CheckTeammate()
    {
        // 如果没有队友，且是双人模式，去全局列表里找
        if (teammateTarget == null && !GameStateController.instance.isSolo.Value)
        {
            foreach (var p in PlayerManager.Instance.AllPlayers)
            {
                if (!p.IsOwner) // 找到那个不是自己的玩家
                {
                    teammateTarget = p;

                    // 生成队友专属绿箭头
                    GameObject arrowObj = LocalObjectPool.instance.GetT(arrowPoolId, transform.position,transform);
                    if (arrowObj.TryGetComponent<IndicatorArrow>(out teammateArrow))
                    {
                        teammateArrow.Setup(teammateColor);
                    }
                    break;
                }
            }
        }
    }
}