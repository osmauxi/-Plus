using UnityEngine;
using UnityEngine.AI;

public class Mover_MeleeChase : MonoBehaviour, IMovementModule
{
    [Header("移动参数配置")]
    public float stopDistance = 2.0f;
    public float pathUpdateInterval = 0.2f;

    [Header("转向配置")]
    public bool faceTargetWhenStopped = true;
    public float rotationSpeed = 8f;

    private float nextUpdateTime;

    public void ExecuteTick(AIBlackboard bb)
    {
        if (bb.Brain != null && bb.Brain.IsHitStopped) return;

        // --- 以下为你原本的逻辑 ---

        if (!bb.HasTarget)
        {
            StopAgent(bb);
            UpdateAnimatorSpeed(bb, 0f); // 没目标，速度硬设为 0
            return;
        }

        float dist = Vector3.Distance(transform.position, bb.CurrentTarget.position);
        bb.DistanceToTarget = dist;

        // 拦截器：如果正在攻击，绝对不允许移动！
        if (bb.IsAttacking)
        {
            StopAgent(bb);
            if (faceTargetWhenStopped) FaceTarget(bb);
            UpdateAnimatorSpeed(bb, 0f); // 攻击中，步伐动画强制归零
            return;
        }

        bb.Rb.velocity = new Vector3(0, bb.Rb.velocity.y, 0);

        if (dist <= stopDistance)
        {
            StopAgent(bb);
            if (faceTargetWhenStopped) FaceTarget(bb);
            bb.IsTargetInAttackRange = true;
            UpdateAnimatorSpeed(bb, 0f); // 踩刹车，步伐动画归零
        }
        else
        {
            bb.IsTargetInAttackRange = false;

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

            // 正常追击时，读取真实速度给动画机
            UpdateAnimatorSpeed(bb, bb.Agent.velocity.magnitude);
        }
    }

    private void UpdateAnimatorSpeed(AIBlackboard bb, float speed)
    {
        if (bb.Anim != null)
        {
            bb.Anim.SetFloat("MoveSpeed", speed, 0.1f, Time.deltaTime);
        }
    }

    private void StopAgent(AIBlackboard bb)
    {
        if (bb.Agent.isActiveAndEnabled && !bb.Agent.isStopped)
        {
            bb.Agent.isStopped = true;
            bb.Agent.velocity = Vector3.zero;
            bb.IsMoving = false;
        }
    }

    private void FaceTarget(AIBlackboard bb)
    {
        if (bb.CurrentTarget == null) return;
        Vector3 direction = (bb.CurrentTarget.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }
    }
}