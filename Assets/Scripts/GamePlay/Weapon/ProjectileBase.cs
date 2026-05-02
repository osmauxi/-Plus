using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ProjectileBase : MonoBehaviour
{
    [Header("子弹基础属性")]
    public float baseDamage;
    public float speed;
    public int currentBounces; // 当前剩余弹射次数
    public int currentPierces; // 当前剩余穿透次数

    public float maxLifeTime = 5f;
    private Coroutine lifeTimerCoroutine;
    // 谁发射的？（用于区分敌我，防止痛击队友）
    private GameObject owner;
    private string targetTag;
    private Vector3 initialScale;

    // 发射者的属性快照（用于特效读取，比如爆炸伤害随玩家基础伤害提升）
    private CharacterStatCollection snapshotStats;

    // 携带的特效列表
    private List<IWeaponEffect> activeEffects;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        initialScale = transform.localScale;
    }

    // 【核心管线】：由 WeaponBase 实例化子弹后瞬间调用
    public void Init(GameObject owner, float damage, float speed, int bounces, int pierces,
                     List<IWeaponEffect> effects, CharacterStatCollection stats, Vector3 inheritedVelocity)
    {
        this.owner = owner;
        this.baseDamage = damage;
        this.speed = speed;
        this.currentBounces = bounces;
        this.currentPierces = pierces;
        this.snapshotStats = stats;
        if (owner.CompareTag("Player"))
        {
            targetTag = "Enemy"; // 玩家打怪物
        }
        else if (owner.CompareTag("Enemy"))
        {
            targetTag = "Player"; // 怪物打玩家
        }
        else
        {
            targetTag = "Untagged"; // 兜底防错
        }

        float sizeMod = stats.GetStatValue(StatType.ProjectileSize);
        // 使用原始大小乘以缩放倍率，绝对安全，不会被对象池污染
        transform.localScale = initialScale * sizeMod;

        // 浅拷贝特效列表
        this.activeEffects = new List<IWeaponEffect>(effects);

        // 触发所有特效的 "出生" 钩子
        foreach (var effect in activeEffects)
        {
            effect.OnProjectileSpawn(this, snapshotStats);
        }
        //惯性动量叠加
        Vector3 bulletSelfVelocity = transform.forward * speed;
        // 抹除 Y 轴惯性（防止玩家跳跃或下落时子弹往地上砸），并叠加 XZ 轴惯性
        Vector3 flatInheritedVelocity = new Vector3(inheritedVelocity.x, 0, inheritedVelocity.z);
        // 最终速度 = 自身速度 + 玩家惯性
        rb.velocity = bulletSelfVelocity + flatInheritedVelocity;

        if (lifeTimerCoroutine != null) StopCoroutine(lifeTimerCoroutine);
        lifeTimerCoroutine = StartCoroutine(LifeTimerRoutine());
    }
    private IEnumerator LifeTimerRoutine()
    {
        yield return new WaitForSeconds(maxLifeTime);
        DestroyProjectile();
    }
    private void OnTriggerEnter(Collider other)
    {
        // 防御性编程：不打自己人
        if (other.gameObject == owner) return;

        bool hitTarget = other.CompareTag(targetTag);
        bool hitWall = other.CompareTag("Wall");

        if (hitTarget)
        {
            Health targetHealth = other.GetComponentInParent<Health>();
            if (!targetHealth.isDead)
            {
                // hitPoint: 获取子弹和目标碰撞体的表面最近接触点，让飙血特效完美贴合在肉体表面！
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                //Debug.Log(other.transform.position - hitPoint);
                // hitDirection: 子弹当前的飞行方向，用于计算击退和血迹喷溅角度！

                // 1. 获取子弹现在的物理大小倍率
                float sizeBonus = snapshotStats.GetStatValue(StatType.ProjectileSize);

                // 2. 伤害附加值 (直接线性映射)：基础伤害10点时倍率是1，伤害30点时倍率就是3！
                float damageBonus = baseDamage / 10f;

                // 3. 最终综合权值：直接让大小和伤害相乘，上限依然保护在 20 倍防崩溃
                float hitWeight = Mathf.Clamp(sizeBonus * damageBonus, 0.5f, 20.0f);
                targetHealth.TakeDamage(baseDamage, hitPoint, transform.forward,hitWeight);
                GlobalLocalVFXPool.Instance.GetVFX("HitBlood", hitPoint, Quaternion.LookRotation(-transform.forward), hitWeight);
            }

            // 2. 触发所有特效的 "击中" 钩子 (比如爆出一团火、引雷)
            foreach (var effect in activeEffects)
            {
                effect.OnHit(this, other.gameObject, transform.position, snapshotStats);
            }

            // 3. 处理穿透逻辑
            if (currentPierces > 0)
            {
                currentPierces--;
            }
            else
            {
                DestroyProjectile();
            }
        }
        else if (hitWall)
        {
            // 处理弹射逻辑
            if (currentBounces > 0)
            {
                currentBounces--;
                // 简单的物理反射算初速度方向 (需要确保墙壁有 normal)
                // rb.velocity = Vector3.Reflect(rb.velocity, hitInfo.normal);

                // 暂时简单的反弹逻辑示意
                transform.forward = Vector3.Reflect(transform.forward, (transform.position - other.ClosestPoint(transform.position)).normalized);
                rb.velocity = transform.forward * speed;
            }
            else
            {
                DestroyProjectile();
            }
        }
    }

    private void DestroyProjectile()
    {
        // 触发销毁钩子 (比如冰爆术：子弹消失时冻结周围)
        foreach (var effect in activeEffects)
        {
            effect.OnDestroy(this, transform.position, snapshotStats);
        }

        LocalObjectPool.instance.RetToPool(gameObject);
    }
}