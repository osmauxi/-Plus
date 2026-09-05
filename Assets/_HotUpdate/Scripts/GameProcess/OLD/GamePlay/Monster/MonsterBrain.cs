using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AIBlackboard))]
public class MonsterBrain : NetworkBehaviour, IKnockbackable
{
    private AIBlackboard blackboard;

    // 存储当前装配的模块
    private ITargetingModule targeter;
    private IMovementModule mover;
    private IAttackModule attacker;

    [Header("顿帧与霸体配置")]
    [Tooltip("怪物最多能忍受的累计顿帧时间(秒) 超过此值将触发霸体反扑！")]
    public float maxHitStopTolerance = 0.6f;
    [Tooltip("触发霸体后，霸体持续的时间(秒)")]
    public float immunityDuration = 1.5f;

    [Header("击退累加配置")]
    [Tooltip("多长时间内的连续受击会被视为【击退累加】(散弹枪建议 0.1f)")]
    public float accumulationWindow = 0.1f;
    private float lastKnockbackTime = -999f;
    private Coroutine knockbackRoutine;

    private float currentHitStopAccumulation = 0f;
    private float immunityEndTime = 0f; // 霸体结束的时间点

    public bool IsHitStopped { get; private set; }
    private float stunEndTime = 0f;

    // 状态快照封存
    private Vector3 savedVelocity;
    private bool wasKinematic;
    private bool wasAgentStopped;
    private Vector3 savedAgentVelocity;

    private void Awake()
    {
        targeter = GetComponent<ITargetingModule>();
        mover = GetComponent<IMovementModule>();
        attacker = GetComponent<IAttackModule>();

        blackboard = GetComponent<AIBlackboard>();

        if (targeter == null || mover == null || attacker == null)
        {
            Debug.LogError("AI模块缺失");
        }
    }
    public bool ApplyHitStop(float duration)
    {
        if (!IsServer) return false;

        // 【核心机制】：霸体期间，无视一切顿帧硬控！
        if (Time.time < immunityEndTime)
        {
            return false;
        }

        stunEndTime = Time.time + duration;

        // 积累忍耐度
        currentHitStopAccumulation += duration;

        // 如果忍耐度爆表，触发霸体！
        if (currentHitStopAccumulation >= maxHitStopTolerance)
        {
            // 霸体结束时间 = 当前时间 + 这次顿帧本身的时间 + 额外奖励的无敌时间
            immunityEndTime = Time.time + duration + immunityDuration;
            currentHitStopAccumulation = 0f; // 清空积蓄槽

            // 可选：在这里触发一个特效，比如怪物身体爆出红光，提示玩家“它生气了霸体了！”
            // GetComponent<EntityFXManager>()?.PlayHyperArmorFlash();
        }

        if (!IsHitStopped)
        {
            IsHitStopped = true;

            if (blackboard.Agent != null && blackboard.Agent.isActiveAndEnabled)
            {
                wasAgentStopped = blackboard.Agent.isStopped;
                savedAgentVelocity = blackboard.Agent.velocity;
                blackboard.Agent.isStopped = true;
            }

            if (blackboard.Rb != null)
            {
                savedVelocity = blackboard.Rb.velocity;
                wasKinematic = blackboard.Rb.isKinematic;
                blackboard.Rb.isKinematic = true;
            }
        }

        return true; // 顿帧应用成功
    }

    private void Update()
    {
        if (!IsServer) return;

        // ==========================================
        // 时间静止接管逻辑
        // ==========================================
        if (IsHitStopped)
        {
            if (Time.time >= stunEndTime)
            {
                // 【时间恢复】：解冻所有状态，把惯性还给怪物
                IsHitStopped = false;

                if (blackboard.Agent != null && blackboard.Agent.isActiveAndEnabled && blackboard.Agent.isOnNavMesh)
                {
                    blackboard.Agent.isStopped = wasAgentStopped;
                    blackboard.Agent.velocity = savedAgentVelocity;
                }
                if (blackboard.Rb != null)
                {
                    blackboard.Rb.isKinematic = wasKinematic;
                    blackboard.Rb.velocity = savedVelocity;
                }
            }
            else
            {
                return; // 时间冻结中，大脑停止派发任何指令
            }
        }

        targeter.ExecuteTick(blackboard);
        mover.ExecuteTick(blackboard);
        attacker.ExecuteTick(blackboard);
    }

    #region 击退
    public void ApplyKnockback(Vector3 force)
    {
        if (!gameObject.activeInHierarchy) return;

        bool isAccumulating = (Time.time - lastKnockbackTime) <= accumulationWindow;
        lastKnockbackTime = Time.time;

        if (isAccumulating)
        {
            // 【保护与升级】：如果当前没有被时停冻结，正常叠加力量
            if (!blackboard.Rb.isKinematic)
            {
                blackboard.Rb.AddForce(force, ForceMode.Impulse);
            }
            else
            {
                // 【塞尔达时停机制】：如果怪物正处于霸体/时停的运动学状态，把受力转化为速度，存入解冻初速度中！
                savedVelocity += force / blackboard.Rb.mass;
            }
        }
        else
        {
            if (knockbackRoutine != null) StopCoroutine(knockbackRoutine);
            knockbackRoutine = StartCoroutine(KnockbackSequence(force));
        }
    }

    private IEnumerator KnockbackSequence(Vector3 initialForce)
    {
        if (blackboard.Agent.isActiveAndEnabled)
        {
            blackboard.Agent.enabled = false;
        }

        // 【防崩溃保护】：只有在非 Kinematic 状态下，才能赋予速度和推力
        if (!blackboard.Rb.isKinematic)
        {
            blackboard.Rb.velocity = new Vector3(0, blackboard.Rb.velocity.y, 0);
            blackboard.Rb.AddForce(initialForce, ForceMode.Impulse);
        }
        else
        {
            // 时停状态下起步，直接将动量注入到解冻后的保存速度里
            savedVelocity = new Vector3(0, savedVelocity.y, 0) + (initialForce / blackboard.Rb.mass);
        }

        yield return new WaitForFixedUpdate();

        // 【逻辑同步】：如果你正在被顿帧冻结，必须让击退协程“暂停”，等待你解冻后再开始计算滑行！
        while (IsHitStopped)
        {
            yield return null;
        }

        // 动态滑行检测
        while (Time.time - lastKnockbackTime < 0.5f && new Vector3(blackboard.Rb.velocity.x, 0, blackboard.Rb.velocity.z).sqrMagnitude > 0.5f)
        {
            yield return null;
        }

        // 【终极防线】：刹车前最后一次检查，彻底杜绝报错！
        if (!blackboard.Rb.isKinematic)
        {
            blackboard.Rb.velocity = new Vector3(0, blackboard.Rb.velocity.y, 0);
        }

        blackboard.Agent.enabled = true;
    }
    #endregion
}