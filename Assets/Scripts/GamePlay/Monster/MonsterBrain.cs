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

    private void Awake()
    {                                                                           
        blackboard = GetComponent<AIBlackboard>();

        // 【自动组装】找找我身上插了哪些乐高积木？
        targeter = GetComponent<ITargetingModule>();
        mover = GetComponent<IMovementModule>();
        attacker = GetComponent<IAttackModule>();

        if (targeter == null || mover == null || attacker == null)
        {
            Debug.LogError($"[AI 拼装错误] 怪物 {gameObject.name} 缺少核心 AI 模块！请检查组件挂载。");
        }
    }

    private void Update()
    {
        // 铁律：所有的 AI 思考必须在服务器进行！
        if (!IsServer) return;

        // 严格按照：看(找人) -> 走(寻路) -> 打(攻击) 的顺序执行
        if (targeter != null) targeter.ExecuteTick(blackboard);
        if (mover != null) mover.ExecuteTick(blackboard);
        if (attacker != null) attacker.ExecuteTick(blackboard);
    }
}