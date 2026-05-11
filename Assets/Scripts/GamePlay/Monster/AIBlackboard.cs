using System.Collections.Generic;
using System.Linq;
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

    [Header("仇恨系统状态")]
    // 存储每个玩家(Transform)对应的仇恨值(float)
    public Dictionary<Transform, float> ThreatTable = new Dictionary<Transform, float>();
    public float threatDecayRate = 5f;// 仇恨衰减速率（每秒衰减多少点仇恨）
    public MonsterBrain Brain { get; private set; }

    private void Awake()
    {
        Brain = GetComponent<MonsterBrain>();
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

    private void Update()
    {
        DecayThreat();
    }
    #region 仇恨系统方法
    /// <summary>
    /// 【通用方法 1】：增加仇恨 (受到攻击或玩家使用嘲讽技能时调用)
    /// </summary>
    public void AddThreat(Transform attacker, float amount)
    {
        if (attacker == null) return;

        if (ThreatTable.ContainsKey(attacker))
        {
            ThreatTable[attacker] += amount;
        }
        else
        {
            ThreatTable.Add(attacker, amount);
        }
    }

    /// <summary>
    /// 【通用方法 2】：获取当前仇恨值最高的存活玩家
    /// </summary>
    public Transform GetHighestThreatTarget()
    {
        // 自动清理掉已经销毁或者死亡的玩家键值对
        var deadKeys = ThreatTable.Keys.Where(k => k == null || k.GetComponent<Health>().isDead).ToList();
        foreach (var key in deadKeys) ThreatTable.Remove(key);

        if (ThreatTable.Count == 0) return null;

        // 找出字典中 value 最大的那个 Transform
        return ThreatTable.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    /// <summary>
    /// 【通用方法 3】：获取指定目标的仇恨值
    /// </summary>
    public float GetThreatOf(Transform target)
    {
        if (target != null && ThreatTable.TryGetValue(target, out float threat))
        {
            return threat;
        }
        return 0f;
    }

    private void DecayThreat()
    {
        if (ThreatTable.Count == 0) return;

        // 因为要在遍历中修改字典的值，所以需要转成 List
        var keys = new List<Transform>(ThreatTable.Keys);
        foreach (var key in keys)
        {
            ThreatTable[key] -= threatDecayRate * Time.deltaTime;
            if (ThreatTable[key] <= 0)
            {
                ThreatTable[key] = 0;
            }
        }
    }
    #endregion
}