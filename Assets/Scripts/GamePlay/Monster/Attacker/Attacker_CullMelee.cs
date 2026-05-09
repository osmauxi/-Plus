using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacker_CullMelee : MonoBehaviour, IAttackModule
{
    [Header("多技能拓展 (伏笔)")]
    public string[] attackTriggers = { "Attack" };

    [Header("战斗参数")]
    public float attackCooldown = 3.0f;
    public float windUpTime = 0.5f;
    public float lungeTime = 0.3f;
    public float recoverTime = 0.7f;

    [Header("伤害判定配置")]
    public BoxCollider hitboxRef;
    public LayerMask targetLayer;

    [Header("物理参数")]
    public float lungeForce = 15f;

    private float nextAttackTime;
    private Coroutine attackRoutine;
    private MonsterVFXController vfxController;

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

    private IEnumerator PausableWait(MonsterBrain brain, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            if (brain != null && brain.IsHitStopped)
            {
                yield return null;
                continue;
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator AttackSequence(AIBlackboard bb)
    {
        MonsterBrain brain = bb.GetComponent<MonsterBrain>();

        bb.IsAttacking = true;
        FaceTarget(bb);

        string selectedAttack = attackTriggers[0];
        if (attackTriggers.Length > 1)
        {
            selectedAttack = attackTriggers[Random.Range(0, attackTriggers.Length)];
        }

        if (bb.Anim != null) bb.Anim.SetTrigger(selectedAttack);
        if (vfxController != null) vfxController.BroadcastVFX("EyeGlow");

        yield return StartCoroutine(PausableWait(brain, windUpTime));

        if (vfxController != null) vfxController.BroadcastVFX("DashTrail");

        if (lungeForce > 0)
        {
            bb.Rb.AddForce(transform.forward * lungeForce, ForceMode.VelocityChange);
        }

        HashSet<GameObject> alreadyHitTargets = new HashSet<GameObject>();
        float timer = 0f;

        while (timer < lungeTime)
        {
            if (brain != null && brain.IsHitStopped)
            {
                yield return null;
                continue;
            }

            PerformHitDetection(bb, alreadyHitTargets);
            timer += Time.deltaTime;
            yield return null;
        }

        bb.Rb.velocity = new Vector3(0, bb.Rb.velocity.y, 0);

        yield return StartCoroutine(PausableWait(brain, recoverTime));

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