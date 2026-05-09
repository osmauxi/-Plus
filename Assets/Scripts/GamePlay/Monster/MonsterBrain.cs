using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(AIBlackboard))]
public class MonsterBrain : NetworkBehaviour
{
    private AIBlackboard blackboard;

    // 存储当前装配的模块
    private ITargetingModule targeter;
    private IMovementModule mover;
    private IAttackModule attacker;

    [Header("顿帧与霸体配置")]
    [Tooltip("怪物最多能忍受的累计顿帧时间(秒)。超过此值将触发霸体反扑！")]
    public float maxHitStopTolerance = 0.6f;
    [Tooltip("触发霸体后，霸体持续的时间(秒)")]
    public float immunityDuration = 1.5f;

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
}