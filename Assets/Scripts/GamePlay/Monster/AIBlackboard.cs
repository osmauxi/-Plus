using UnityEngine;

/// <summary>
/// AI 共享黑板：只存放数据，绝对不写任何行为逻辑！
/// </summary>
public class AIBlackboard : MonoBehaviour
{
    [Header("感知数据 (情报员写入)")]
    public Transform CurrentTarget;     // 当前锁定的目标
    public Vector3 TargetPosition;      // 目标位置（用于丢失目标时去最后已知位置）
    public bool HasTarget => CurrentTarget != null;

    [Header("空间数据 (司机写入)")]
    public float DistanceToTarget;      // 距离目标的实时距离
    public bool IsMoving;               // 是否正在移动

    [Header("战斗数据 (杀手写入)")]
    public bool IsAttacking;            // 攻击动作是否正在执行（如果为 true，司机必须停车）
    public bool CanSeeTarget;           // 视线内是否有遮挡
    public bool IsTargetInAttackRange;  // 司机写入：车已经开到攻击距离了，杀手你可以动手了！

    [Header("通用实体引用")]
    // 方便各个模块快速获取基础组件，省去每次 GetComponent 的性能开销
    [HideInInspector] public UnityEngine.AI.NavMeshAgent Agent;
    [HideInInspector] public Animator Anim;
    [HideInInspector] public Rigidbody Rb;
    [HideInInspector] public MonsterEntity EntityConfig;

    private void Awake()
    {
        Agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        Anim = GetComponentInChildren<Animator>();
        Rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// 洗脏数据（对象池复用时调用）
    /// </summary>
    public void ClearBlackboard()
    {
        CurrentTarget = null;
        TargetPosition = Vector3.zero;
        DistanceToTarget = float.MaxValue;
        IsMoving = false;
        IsAttacking = false;
        CanSeeTarget = false;
    }
}