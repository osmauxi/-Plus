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

    // 发射者的属性快照（用于特效读取，比如爆炸伤害随玩家基础伤害提升）
    private CharacterStatCollection snapshotStats;

    // 携带的特效列表
    private List<IWeaponEffect> activeEffects;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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

        bool hitEnemy = other.CompareTag("Enemy");
        bool hitWall = other.CompareTag("Wall");

        if (hitEnemy)
        {
            // 1. 造成基础伤害 (这里调用你未来的血量系统)
            // other.GetComponent<Health>().TakeDamage(baseDamage);

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