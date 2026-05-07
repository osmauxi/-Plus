using System.Collections;
using UnityEngine;

public class Attacker_StationaryMelee : MonoBehaviour, IAttackModule
{
    [Header("多技能拓展 (伏笔)")]
    public string[] attackTriggers = { "Attack" };

    [Header("战斗节奏配置")]
    public float attackCooldown = 2.0f;
    [Tooltip("前摇：从动画开始到【造成伤害】的等待时间")]
    public float windUpTime = 0.5f;
    [Tooltip("后摇：造成伤害后，原地保持罚站状态的时间")]
    public float recoverTime = 0.8f;

    [Header("伤害判定配置")]
    public BoxCollider hitboxRef;
    public LayerMask targetLayer;

    private float nextAttackTime;
    private Coroutine attackRoutine;
    private MonsterVFXController vfxController;

    // 无 GC 内存扫描数组
    private Collider[] hitResults = new Collider[10];

    private void Awake()
    {
        vfxController = GetComponent<MonsterVFXController>();
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
        // === 0. 锁定状态，物理刹车 ===
        bb.IsAttacking = true;
        bb.Rb.velocity = new Vector3(0, bb.Rb.velocity.y, 0); // 绝对禁止滑步
        FaceTarget(bb);

        string selectedAttack = attackTriggers[0];
        if (attackTriggers.Length > 1)
        {
            selectedAttack = attackTriggers[Random.Range(0, attackTriggers.Length)];
        }

        // === 1. 触发动画，进入【前摇】 ===
        if (bb.Anim != null) bb.Anim.SetTrigger(selectedAttack);
        if (vfxController != null) vfxController.BroadcastVFX("EyeGlow");

        yield return new WaitForSeconds(windUpTime);

        // === 2. 瞬间爆发！【单次伤害判定】 ===
        // 相比猛扑模块，这里不再使用 while 循环和 HashSet，只在这一帧抽检一次
        PerformSingleHitDetection(bb);

        // === 3. 进入【后摇】罚站 ===
        yield return new WaitForSeconds(recoverTime);

        // === 4. 彻底结束，解锁 AI ===
        bb.IsAttacking = false;
        nextAttackTime = Time.time + attackCooldown;
    }

    /// <summary>
    /// 单帧瞬发伤害判定
    /// </summary>
    private void PerformSingleHitDetection(AIBlackboard bb)
    {
        if (hitboxRef == null) return;

        Vector3 boxCenter = hitboxRef.transform.TransformPoint(hitboxRef.center);
        Vector3 boxHalfExtents = Vector3.Scale(hitboxRef.size, hitboxRef.transform.lossyScale) * 0.5f;
        Quaternion boxRotation = hitboxRef.transform.rotation;

        int hitCount = Physics.OverlapBoxNonAlloc(boxCenter, boxHalfExtents, hitResults, boxRotation, targetLayer);

        for (int i = 0; i < hitCount; i++)
        {
            GameObject targetObj = hitResults[i].gameObject;
            Health targetHealth = targetObj.GetComponentInParent<Health>();

            if (targetHealth != null && !targetHealth.isDead)
            {
                float damage = bb.EntityConfig.Config.baseDamage;
                // 将当前怪物的坐标和正前方传递过去，用于计算击退或飙血方向
                targetHealth.TakeDamage(damage, transform.position, transform.forward);
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