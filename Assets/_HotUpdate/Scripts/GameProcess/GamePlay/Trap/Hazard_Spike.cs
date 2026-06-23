using UnityEngine;

public class Hazard_Spike : HazardBase
{
    [Header("地刺设置")]
    public float burstDamage = 30f;
    public float cooldown = 3f;
    private float lastTriggerTime = -999f;

    protected override void OnEntityEnter(Health target)
    {
        // 如果还在冷却中，不触发
        if (Time.time < lastTriggerTime + cooldown) return;

        lastTriggerTime = Time.time;

        // 瞬间造成大量伤害，且带上 0.5f 的击退/顿帧权重，让怪物被扎的时候明显僵直一下
        target.TakeDamage(burstDamage, target.transform.position, Vector3.up, 0.5f);

        // 这里可以调用一个 ClientRpc 去播放地刺弹出来的动画和音效
        TriggerSpikeVisualClientRpc();
    }

    [Unity.Netcode.ClientRpc]
    private void TriggerSpikeVisualClientRpc()
    {
        // 播放地刺穿出的动画
        // GetComponent<Animator>().SetTrigger("Stab");
    }
}