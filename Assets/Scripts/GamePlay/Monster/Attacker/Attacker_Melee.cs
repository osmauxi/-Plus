using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacker_Melee : MonoBehaviour, IAttackModule
{
    [Header("多技能拓展 (伏笔)")]
    [Tooltip("怪物拥有的所有攻击动作名称。普通怪只填一个 Attack，精英怪可以填 Attack_1, Attack_2 等")]
    public string[] attackTriggers = { "Attack" };

    [Header("战斗参数")]
    public float attackCooldown = 3.0f;
    [Tooltip("攻击前摇：从触发动画开始，到真正扑出去/产生伤害判定的等待时间")]
    public float windUpTime = 0.5f;
    [Tooltip("伤害判定持续时间 (猛扑持续时间)")]
    public float lungeTime = 0.3f;
    [Tooltip("收招僵直：扑完之后原地罚站的时间")]
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
        // 锁定 AI 状态，让司机 (Mover) 踩刹车
        bb.IsAttacking = true;
        FaceTarget(bb);

        // ==========================================
        // 【伏笔】：多技能随机抽选系统
        // 如果数组里配了多个技能，这里会随机挑一个放
        // ==========================================
        string selectedAttack = attackTriggers[0];
        if (attackTriggers.Length > 1)
        {
            selectedAttack = attackTriggers[Random.Range(0, attackTriggers.Length)];
        }

        // === 1. 触发动画与前摇 ===
        // 立刻播放攻击动画
        if (bb.Anim != null) bb.Anim.SetTrigger(selectedAttack);
        if (vfxController != null) vfxController.BroadcastVFX("EyeGlow");

        // 等待怪物把手举起来（前摇）
        yield return new WaitForSeconds(windUpTime);

        // === 2. 猛扑与【主动帧伤害判定】 ===
        if (vfxController != null) vfxController.BroadcastVFX("DashTrail");

        // 如果想让怪物原地挥击，把 lungeForce 设为 0 即可
        if (lungeForce > 0)
        {
            bb.Rb.AddForce(transform.forward * lungeForce, ForceMode.VelocityChange);
        }

        HashSet<GameObject> alreadyHitTargets = new HashSet<GameObject>();
        float timer = 0f;

        while (timer < lungeTime)
        {
            PerformHitDetection(bb, alreadyHitTargets);
            timer += Time.deltaTime;
            yield return null;
        }

        // === 3. 收招僵直 ===
        bb.Rb.velocity = new Vector3(0, bb.Rb.velocity.y, 0); // 保留 Y 轴防穿地，清空 XZ 轴
        yield return new WaitForSeconds(recoverTime);

        // === 4. 解锁 ===
        bb.IsAttacking = false;
        nextAttackTime = Time.time + attackCooldown;
    }

    private void PerformHitDetection(AIBlackboard bb, HashSet<GameObject> alreadyHit)
    {
        if (hitboxRef == null) return;

        Vector3 boxCenter = hitboxRef.transform.TransformPoint(hitboxRef.center);
        Vector3 boxHalfExtents = Vector3.Scale(hitboxRef.size, hitboxRef.transform.lossyScale) * 0.5f;
        Quaternion boxRotation = hitboxRef.transform.rotation;

        int hitCount = Physics.OverlapBoxNonAlloc(boxCenter, boxHalfExtents, hitResults, boxRotation, targetLayer);

        for (int i = 0; i < hitCount; i++)
        {
            GameObject targetObj = hitResults[i].gameObject;

            if (!alreadyHit.Contains(targetObj))
            {
                alreadyHit.Add(targetObj);

                Health targetHealth = targetObj.GetComponentInParent<Health>();
                if (targetHealth != null && !targetHealth.isDead)
                {
                    float damage = bb.EntityConfig.Config.baseDamage;
                    targetHealth.TakeDamage(damage, transform.position, transform.forward);
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