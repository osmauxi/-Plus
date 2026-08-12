using UnityEngine;

/// <summary>
/// 高级索敌插件：基于仇恨值驱动，带有粘性阈值防抽搐，且支持无仇恨时就近索敌兜底。
/// </summary>
public class Targeter_AggroDriven : MonoBehaviour, ITargetingModule
{
    [Header("感知参数")]
    public float visionRange = 20f;
    public float loseTargetRange = 25f;
    public float searchInterval = 0.5f;

    [Header("仇恨参数")]
    [Tooltip("仇恨粘性阈值：新目标的仇恨必须大于当前目标仇恨的多少倍，怪物才会转移目标？(推荐 1.2 - 1.5)")]
    public float stickinessMultiplier = 1.2f;
    [Tooltip("首次发现玩家时赋予的初始仇恨值")]
    public float initialAggro = 20f;

    private float nextSearchTime;

    public void ExecuteTick(AIBlackboard bb)
    {
        if (bb.Brain != null && bb.Brain.IsHitStopped) return;

        if (Time.time < nextSearchTime) return;
        nextSearchTime = Time.time + searchInterval;

        // ==========================================
        // 1. 清理死人和丢失的目标
        // ==========================================
        if (bb.HasTarget)
        {
            float dist = Vector3.Distance(transform.position, bb.CurrentTarget.position);
            Health targetHealth = bb.CurrentTarget.GetComponent<Health>();

            if (targetHealth == null || targetHealth.isDead || dist > loseTargetRange)
            {
                // 目标无效了，清空并把它从仇恨列表里彻底抹除
                if (bb.CurrentTarget != null) bb.ThreatTable.Remove(bb.CurrentTarget);
                bb.CurrentTarget = null;
                bb.IsTargetInAttackRange = false;
            }
            else
            {
                bb.TargetPosition = bb.CurrentTarget.position;
            }
        }

        // ==========================================
        // 2. 仇恨大脑运转核心
        // ==========================================
        Transform highestThreatTarget = bb.GetHighestThreatTarget();

        if (highestThreatTarget != null)
        {
            // 情况 A：有人拉了仇恨！
            if (!bb.HasTarget)
            {
                // 刚才没目标，现在直接锁定这个最招人恨的
                bb.CurrentTarget = highestThreatTarget;
                bb.TargetPosition = highestThreatTarget.position;
            }
            else if (bb.CurrentTarget != highestThreatTarget)
            {
                // 刚才有目标，但最高仇恨者换人了！有人在疯狂输出试图拉走仇恨！
                float currentThreat = bb.GetThreatOf(bb.CurrentTarget);
                float highestThreat = bb.GetThreatOf(highestThreatTarget);

                // 【策略核心：粘性判定】只有当新人的仇恨碾压当前目标时，才抛弃旧爱
                if (highestThreat > currentThreat * stickinessMultiplier)
                {
                    bb.CurrentTarget = highestThreatTarget;
                    bb.TargetPosition = highestThreatTarget.position;
                }
            }
        }
        else
        {
            // ==========================================
            // 3. 兜底策略：根本没人打我，退化为就近索敌
            // ==========================================
            if (!bb.HasTarget)
            {
                //Transform nearest = PlayerManager.Instance.GetNearestPlayer(transform.position);

                //if (nearest != null)
                //{
                //    Health pHealth = nearest.GetComponent<Health>();
                //    if (pHealth != null && !pHealth.isDead)
                //    {
                //        float dist = Vector3.Distance(transform.position, nearest.position);
                //        if (dist <= visionRange)
                //        {
                //            // 看到最近的玩家了，锁定他！
                //            bb.CurrentTarget = nearest;
                //            bb.TargetPosition = nearest.position;

                //            // 极小细节：给他强行塞一点初始仇恨。
                //            // 这样即便另外一个远处的玩家用小手枪刮了一滴血，怪物也不会因为 1 > 0 立刻回头。
                //            bb.AddThreat(nearest, initialAggro);
                //        }
                //    }
                //}
            }
        }

        bb.CanSeeTarget = bb.HasTarget;
    }
}