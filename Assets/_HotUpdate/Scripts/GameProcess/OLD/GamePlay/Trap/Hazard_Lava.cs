using UnityEngine;

public class Hazard_Lava : HazardBase
{
    [Header("岩浆设置")]
    public float damagePerTick = 5f;

    // 我们只需要重写 Tick 方法
    protected override void OnHazardTick(Health target)
    {
        // hitWeight 传 0，防止玩家/怪物站在岩浆里被无限顿帧卡死
        // 如果你的 TakeDamage 加了 attacker 参数，这里直接传 null (天然伤害不计仇恨)
        target.TakeDamage(damagePerTick, target.transform.position, Vector3.up, 0f);
    }
}