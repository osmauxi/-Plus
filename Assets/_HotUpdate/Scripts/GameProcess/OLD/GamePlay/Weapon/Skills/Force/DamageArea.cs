using UnityEngine;
using System.Collections.Generic;

public class DamageArea : MonoBehaviour
{
    private float duration;
    private float tickTimer;
    private float damagePerTick;
    private bool isTrueDamage;
    private CharacterStatCollection sourceStats;

    // 动态升级参数
    private float currentRadius;
    private float currentSlowRatio;

    private WeaponBase cachedWeapon;
    private ExecutionerEffect cachedExecutioner;
    private bool hasCheckedExecutioner = false;

    public void InitOrRefresh(float lifeTime, float damage, bool trueDmg, CharacterStatCollection stats, float radius, float slowRatio)
    {
        duration += lifeTime;
        damagePerTick = damage;
        isTrueDamage = trueDmg;
        sourceStats = stats;

        currentRadius = radius;
        currentSlowRatio = slowRatio;

        if (cachedWeapon == null && sourceStats != null)
        {
            cachedWeapon = sourceStats.GetComponentInChildren<WeaponBase>();
        }
    }

    private void Update()
    {
        if (duration <= 0) { Cleanup(); return; }
        duration -= Time.deltaTime;
        tickTimer += Time.deltaTime;

        if (tickTimer >= 1.0f)
        {
            tickTimer = 0;
            ApplyAreaDamage();
        }
    }

    private void ApplyAreaDamage()
    {
        // 【修改点 2】：利用动态升级的范围进行索敌
        Collider[] hits = Physics.OverlapSphere(transform.position, currentRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy") && hit.transform.root.TryGetComponent<Health>(out var health) && !health.isDead)
            {
                health.TakeDamage(damagePerTick, hit.transform.position, Vector3.zero, 0f, null, isTrueDamage);
                CheckAndApplyExecution(health, hit.transform.position);

                // 【绝赞机制】：触发减速！
                if (hit.transform.root.TryGetComponent<MonsterEntity>(out var monster))
                {
                    // 比如词条写的是减速 0.3，传给怪物的就是保留 0.7 倍移速
                    // 给 1.2 秒的持续时间，确保它走出圈后还能被黏住零点几秒，手感更好
                    float speedMultiplier = Mathf.Max(0.1f, 1f - currentSlowRatio);
                    monster.ApplySlow(speedMultiplier, 1.2f);
                }
            }
        }
    }

    private void CheckAndApplyExecution(Health targetHealth, Vector3 hitPoint)
    {
        if (sourceStats == null || targetHealth.isDead) return;

        // 如果还没遍历过，去玩家的技能列表里找找有没有《处决者》
        if (!hasCheckedExecutioner && cachedWeapon != null)
        {
            foreach (var effect in cachedWeapon.activeEffects)
            {
                if (effect is ExecutionerEffect exec)
                {
                    cachedExecutioner = exec;
                    break;
                }
            }
            hasCheckedExecutioner = true; // 即使没找到也永久关闭检测大门，省性能！
        }

        // 如果拥有斩杀词条，顺便执行死神降临！(这里给一个向上击飞的方向 Vector3.up 表现死状)
        if (cachedExecutioner != null)
        {
            cachedExecutioner.TryExecute(targetHealth, sourceStats, hitPoint, Vector3.up);
        }
    }

    private void Cleanup()
    {
        // ==========================================
        // 生命周期：对象池安全复位
        // ==========================================
        duration = 0;
        tickTimer = 0;
        hasCheckedExecutioner = false;
        cachedExecutioner = null;

        // 把自动回收脚本交还，让对象池收走这个特效
        if (TryGetComponent<MonoBehaviour>(out var autoReturn)) autoReturn.enabled = true;
        GlobalLocalVFXPool.Instance.ReturnVFX("OnRadiation", this.gameObject);
    }
}