using UnityEngine;

/// <summary>
/// 寻敌插件：永远锁定距离最近的玩家
/// </summary>
public class Targeter_NearestPlayer : MonoBehaviour, ITargetingModule
{
    [Header("感知参数")]
    public float visionRange = 20f;      // 视野范围
    public float loseTargetRange = 25f;  // 丢失目标的范围（稍微大一点，防止在边界反复横跳）
    public float searchInterval = 0.5f;  // 寻敌频率（不要每帧都找，省性能）

    private float nextSearchTime;

    public void ExecuteTick(AIBlackboard bb)
    {
        // 优化：不需要每一帧都去遍历所有玩家计算距离，0.5秒查一次足够了
        if (Time.time < nextSearchTime) return;
        nextSearchTime = Time.time + searchInterval;

        // 如果我们本来就有目标，先检查他有没有跑太远或者死掉
        if (bb.HasTarget)
        {
            float dist = Vector3.Distance(transform.position, bb.CurrentTarget.position);

            // TODO: 未来如果玩家身上有 Health 组件，这里还要加上 bb.CurrentTarget.GetComponent<Health>().isDead 的判断
            if (dist > loseTargetRange && bb.CurrentTarget.GetComponent<Health>().isDead)
            {
                // 目标跑太远，丢掉目标
                bb.CurrentTarget = null;
            }
            else
            {
                // 目标还在，顺手更新一下目标最后的位置
                bb.TargetPosition = bb.CurrentTarget.position;
            }
        }

        // 如果当前没目标，开始找！
        if (!bb.HasTarget)
        {
            // 调用我们之前设计的 PlayerManager 工具库
            Transform nearest = PlayerManager.Instance.GetNearestPlayer(transform.position);

            if (nearest != null)
            {
                float dist = Vector3.Distance(transform.position, nearest.position);
                // 只有在视野范围内才锁定
                if (dist <= visionRange)
                {
                    // 【关键步骤：往黑板上写字！】
                    bb.CurrentTarget = nearest;
                    bb.TargetPosition = nearest.position;
                }
            }
        }

        // 视线遮挡检测 (可选的高级逻辑)：
        // 如果想做“掩体系统”，这里可以从怪物发射一条射线(Raycast)到玩家。
        // 如果打中墙壁，就把 bb.CanSeeTarget 设为 false，否则设为 true。
        // 这里为了简单，我们假设只要有目标就能看到。
        bb.CanSeeTarget = bb.HasTarget;
    }
}