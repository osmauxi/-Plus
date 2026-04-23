using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacker_Melee : MonoBehaviour, IAttackModule
{
    [Header("战斗参数")]
    public float attackCooldown = 3.0f;
    public float prepareTime = 0.5f;
    public float lungeTime = 0.3f;
    public float recoverTime = 0.7f;

    [Header("伤害判定配置")]
    [Tooltip("仅仅作为一个可视化的范围参考，它的 enabled 永远是 false")]
    public BoxCollider hitboxRef;
    public LayerMask targetLayer;       // 勾选 Player 的 Layer

    [Header("物理参数")]
    public float lungeForce = 15f;

    private float nextAttackTime;
    private Coroutine attackRoutine;
    private MonsterVFXController vfxController;

    // 【核心黑科技】：无 GC 内存扫描数组（预分配内存，最大扫 10 个目标）
    private Collider[] hitResults = new Collider[10];

    private void Awake()
    {
        vfxController = GetComponent<MonsterVFXController>();

        // 剥夺 BoxCollider 的物理权利，只留它的尸体（参数）
        if (hitboxRef != null) hitboxRef.enabled = false;
    }

    public void ExecuteTick(AIBlackboard bb)
    {
        if (bb.IsAttacking || Time.time < nextAttackTime || !bb.HasTarget) return;

        if (bb.IsTargetInAttackRange)
        {
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            attackRoutine = StartCoroutine(AttackSequence(bb));
        }
    }

    private IEnumerator AttackSequence(AIBlackboard bb)
    {
        // === 1. 蓄力预警 ===
        bb.IsAttacking = true;
        bb.Anim.SetTrigger("Prepare");
        vfxController.BroadcastVFX("EyeGlow");
        FaceTarget(bb);
        yield return new WaitForSeconds(prepareTime);

        // === 2. 猛扑与【主动帧伤害判定】 ===
        bb.Anim.SetTrigger("Attack");
        vfxController.BroadcastVFX("DashTrail");

        bb.Rb.AddForce(transform.forward * lungeForce, ForceMode.VelocityChange);

        // 【名册建立】：记录这次攻击已经打过的人，防止一秒十刀
        HashSet<GameObject> alreadyHitTargets = new HashSet<GameObject>();
        float timer = 0f;

        // 持续判定循环：在突进的每一帧中主动投射
        while (timer < lungeTime)
        {
            PerformHitDetection(bb, alreadyHitTargets);
            timer += Time.deltaTime;
            yield return null; // 等待下一帧继续扫
        }

        // === 3. 收招僵直 ===
        bb.Rb.velocity = Vector3.zero;
        yield return new WaitForSeconds(recoverTime);

        // === 4. 解锁 ===
        bb.IsAttacking = false;
        nextAttackTime = Time.time + attackCooldown;
    }

    /// <summary>
    /// 主动形状扫描与伤害结算
    /// </summary>
    private void PerformHitDetection(AIBlackboard bb, HashSet<GameObject> alreadyHit)
    {
        if (hitboxRef == null) return;

        // 获取 Box 的世界坐标、旋转和真实大小
        Vector3 boxCenter = hitboxRef.transform.TransformPoint(hitboxRef.center);
        Vector3 boxHalfExtents = Vector3.Scale(hitboxRef.size, hitboxRef.transform.lossyScale) * 0.5f;
        Quaternion boxRotation = hitboxRef.transform.rotation;

        // 不会产生内存碎片的物理扫描
        int hitCount = Physics.OverlapBoxNonAlloc(boxCenter, boxHalfExtents, hitResults, boxRotation, targetLayer);

        for (int i = 0; i < hitCount; i++)
        {
            GameObject targetObj = hitResults[i].gameObject;

            // 如果这个人还没被打过
            if (!alreadyHit.Contains(targetObj))
            {
                alreadyHit.Add(targetObj);

                // 获取玩家血量组件并造成伤害
                Health targetHealth = targetObj.GetComponent<Health>();
                if (targetHealth != null && !targetHealth.isDead)
                {
                    // 从黑板拿到配置库里的攻击力，发起攻击！
                    float damage = bb.EntityConfig.Config.baseDamage;
                    targetHealth.TakeDamage(damage, transform.position, transform.forward);

                    Debug.Log($"[伤害判定] 怪物打中了 {targetObj.name}，造成了 {damage} 点伤害！");
                }
            }
        }
    }

    private void FaceTarget(AIBlackboard bb)
    {
        if (bb.CurrentTarget == null) return;
        Vector3 dir = (bb.CurrentTarget.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
    }
}