using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 移动插件：通用近战追击
/// 职责：向目标靠近，到达攻击距离后刹车，并持续面向目标
/// </summary>
public class Mover_MeleeChase : MonoBehaviour, IMovementModule
{
    [Header("移动参数配置")]
    [Tooltip("距离目标多近时停下（应约等于攻击动作的判定距离）")]
    public float stopDistance = 2.0f;

    [Tooltip("寻路更新频率（秒）。不要每帧更新，极度节省CPU性能！")]
    public float pathUpdateInterval = 0.2f;

    [Header("转向配置")]
    [Tooltip("停下或攻击时，是否依然用代码平滑看向目标（防止攻击打歪）")]
    public bool faceTargetWhenStopped = true;
    public float rotationSpeed = 8f;

    // 内部计时器
    private float nextUpdateTime;

    public void ExecuteTick(AIBlackboard bb)
    {
        //黑板上没目标，直接停车休假
        if (!bb.HasTarget)
        {
            StopAgent(bb);
            return;
        }

        // 2. 核心数学：计算距离，并立刻写在黑板上给其他模块看！
        float dist = Vector3.Distance(transform.position, bb.CurrentTarget.position);
        bb.DistanceToTarget = dist;

        // 3. 拦截器：如果杀手（攻击模块）正在播攻击动画，司机绝对不能动！
        if (bb.IsAttacking)
        {
            StopAgent(bb);
            // 即使停下，也可以选择死死盯住玩家，防止玩家绕背躲开攻击
            if (faceTargetWhenStopped) FaceTarget(bb);
            return;
        }

        // 4. 距离判定：到达攻击范围了吗？
        if (dist <= stopDistance)
        {
            // 踩刹车！
            StopAgent(bb);
            if (faceTargetWhenStopped) FaceTarget(bb);

            // 【关键】：在黑板上留下记号，告诉杀手可以开火了！
            bb.IsTargetInAttackRange = true;
        }
        else
        {
            // 距离不够，继续踩油门追击
            bb.IsTargetInAttackRange = false;

            // 性能优化：每隔 pathUpdateInterval 秒才重新算一次路
            if (Time.time >= nextUpdateTime)
            {
                nextUpdateTime = Time.time + pathUpdateInterval;

                if (bb.Agent.isActiveAndEnabled)
                {
                    bb.Agent.isStopped = false;
                    bb.Agent.SetDestination(bb.CurrentTarget.position);
                    bb.IsMoving = true;
                }
            }
        }
        if (bb.Anim != null && bb.Agent.isActiveAndEnabled)
        {
            bb.Anim.SetFloat("MoveSpeed", bb.Agent.velocity.magnitude);
        }
    }

    // ==========================================
    // 内部工具方法
    // ==========================================
    private void StopAgent(AIBlackboard bb)
    {
        if (bb.Agent.isActiveAndEnabled && !bb.Agent.isStopped)
        {
            bb.Agent.isStopped = true;
            bb.Agent.velocity = Vector3.zero; // 清除惯性滑动
            bb.IsMoving = false;
        }
    }

    private void FaceTarget(AIBlackboard bb)
    {
        if (bb.CurrentTarget == null) return;

        // 计算纯 2D 平面旋转（忽略Y轴高度差，防止怪物抬头看天或低头看地）
        Vector3 direction = (bb.CurrentTarget.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }
    }
}