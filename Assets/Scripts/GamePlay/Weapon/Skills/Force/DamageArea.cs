using UnityEngine;

public class DamageArea : MonoBehaviour
{
    private float duration;
    private float tickTimer;
    private float damagePerTick;
    private bool isTrueDamage;

    public void InitOrRefresh(float lifeTime, float damage, bool trueDmg)
    {
        duration += lifeTime; // 叠加时间
        damagePerTick = damage;
        isTrueDamage = trueDmg;
    }

    private void Update()
    {
        if (duration <= 0) { Destroy(gameObject); return; } // 或者退回对象池

        duration -= Time.deltaTime;
        tickTimer += Time.deltaTime;

        // 每秒一跳
        if (tickTimer >= 1.0f)
        {
            tickTimer = 0;
            Collider[] hits = Physics.OverlapSphere(transform.position, 4f); // 辐射半径
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy") && hit.TryGetComponent<Health>(out var health) && !health.isDead)
                {
                    // 发送伤害，最后一项传入 isTrueDamage
                    health.TakeDamage(damagePerTick, hit.transform.position, Vector3.zero, 0f, null, isTrueDamage);
                }
            }
        }
    }
}