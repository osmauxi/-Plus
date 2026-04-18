using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponBase : NetworkBehaviour
{
    [Header("核心引用")]
    public CharacterStatCollection stats;    // 属性大脑
    public Transform firePoint;              // 子弹发射口
    public string projectilePoolId = "Bullet"; // 对象池中的子弹ID

    [Header("弹药状态")]
    public int currentAmmo;
    private bool isReloading = false;
    private float lastFireTime;

    // 当前武器携带的所有肉鸽效果器（雷电、弹射等）
    public List<IWeaponEffect> activeEffects = new List<IWeaponEffect>();

    public override void OnNetworkSpawn()
    {
        // 初始弹匣加满
        currentAmmo = (int)stats.GetStatValue(StatType.MagSize);
    }

    private void Update()
    {
        // 只有本地玩家才能控制开火输入
        if (!IsOwner) return;

        HandleInput();
    }

    private void HandleInput()
    {
        // 1. 处理手动换弹
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < stats.GetStatValue(StatType.MagSize))
        {
            StartCoroutine(ReloadRoutine());
            return;
        }

        // 2. 处理射击逻辑（支持连发）
        if (Input.GetMouseButton(0))
        {
            TryFire();
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            // 给枪增加多重射击
            stats.AddModifier(StatType.ProjectileCount, 2, StatModType.Flat, this);
            Debug.Log("获得多重射击！");
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            // 给枪挂载雷电链特效
            AddOrUpgradeEffect(new LightningChainEffect());
            Debug.Log("获得雷电链！");
        }
    }

    public void TryFire()
    {
        if (isReloading) return;

        // 检查弹药
        if (currentAmmo <= 0)
        {
            StartCoroutine(ReloadRoutine());
            return;
        }

        // 检查射击冷却 (1.0 / 射速 = 间隔时间)
        float fireInterval = 1f / stats.GetStatValue(StatType.FireRate);
        if (Time.time - lastFireTime < fireInterval) return;

        // ==========================================
        // 【合成点 1】：触发开火前拦截
        // ==========================================
        foreach (var effect in activeEffects)
        {
            // 如果某个词条拦截了开火（比如它自己接管了连发逻辑），直接 return
            if (!effect.OnBeforeFire(this, stats)) 
                return;
        }
        // 执行发射
        ExecuteFire();
        // ==========================================
        // 【合成点 2】：触发开火后置事件
        // ==========================================
        foreach (var effect in activeEffects)
        {
            effect.OnAfterFire(this, stats);
        }
    }

    private void ExecuteFire()
    {
        lastFireTime = Time.time;
        currentAmmo--;

        int bulletCount = Mathf.Max(1, (int)stats.GetStatValue(StatType.ProjectileCount));
        float spread = stats.GetStatValue(StatType.SpreadAngle);

        // 如果有多发子弹但没配散布角度，给一个保底的扇形角度（比如每多一发加 15 度）
        if (bulletCount > 1 && spread <= 0.1f) spread = 15f * bulletCount;

        // ==========================================
        // 修复 1：枪管视差 (精准指哪打哪)
        // ==========================================
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // 以枪口的高度建一个虚拟水平面，防止子弹往地下打
        Plane groundPlane = new Plane(Vector3.up, firePoint.position);

        Vector3 exactAimPoint = firePoint.position + firePoint.forward * 10f; // 兜底方向
        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            exactAimPoint = ray.GetPoint(rayDistance);
        }

        // 算出枪口到鼠标的绝对精准方向
        Vector3 baseFireDirection = (exactAimPoint - firePoint.position).normalized;
        Quaternion baseRotation = Quaternion.LookRotation(baseFireDirection);

        // ==========================================
        // 修复 2：获取玩家当前惯性
        // ==========================================
        Vector3 playerVelocity = Vector3.zero;
        // 假设武器挂在玩家子节点，通过 transform.root 往上找玩家的 Rigidbody
        if (transform.root.TryGetComponent<Rigidbody>(out Rigidbody playerRb))
        {
            playerVelocity = playerRb.velocity;
        }

        // ==========================================
        // 修复 3：扇形弹道均匀散布
        // ==========================================
        float startAngle = -spread / 2f;
        float angleStep = bulletCount > 1 ? spread / (bulletCount - 1) : 0f;

        for (int i = 0; i < bulletCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);

            // 在精准方向的基础上，加上扇形偏移角度
            Quaternion bulletRotation = baseRotation * Quaternion.Euler(0, currentAngle, 0);

            // 发送 RPC，把玩家当前的惯性也传过去！
            FireProjectileServerRpc(firePoint.position, bulletRotation, playerVelocity);
        }
    }
    [ServerRpc]
    private void FireProjectileServerRpc(Vector3 pos, Quaternion rot, Vector3 inheritedVelocity)
    {
        SpawnProjectile(pos, rot, inheritedVelocity);
    }

    private void SpawnProjectile(Vector3 pos, Quaternion rot, Vector3 inheritedVelocity)
    {
        GameObject bulletObj = LocalObjectPool.instance.GetT(projectilePoolId, pos, null);
        bulletObj.transform.rotation = rot;

        ProjectileBase projectile = bulletObj.GetComponent<ProjectileBase>();

        float finalDmg = stats.GetStatValue(StatType.Damage);
        bool isCrit = Random.value < stats.GetStatValue(StatType.CritChance);
        if (isCrit) finalDmg *= stats.GetStatValue(StatType.CritDamage);

        projectile.Init(
            owner: this.gameObject,
            damage: finalDmg,
            speed: stats.GetStatValue(StatType.ProjectileSpeed),
            bounces: (int)stats.GetStatValue(StatType.BounceCount),
            pierces: (int)stats.GetStatValue(StatType.PierceCount),
            effects: activeEffects,
            stats: stats,
            inheritedVelocity: inheritedVelocity // 把惯性传给子弹
        );
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        Debug.Log("正在换弹...");

        // 读取属性系统中的换弹时间
        yield return new WaitForSeconds(stats.GetStatValue(StatType.ReloadTime));

        currentAmmo = (int)stats.GetStatValue(StatType.MagSize);
        isReloading = false;
        Debug.Log("换弹完成！");
    }

    // 拾取/升级词条的接口
    public void AddOrUpgradeEffect(IWeaponEffect newEffect)
    {
        var existing = activeEffects.Find(e => e.GetType() == newEffect.GetType());
        if (existing != null)
        {
            if (existing is IUpgradeableEffect upgradeable) upgradeable.Upgrade();
        }
        else
        {
            activeEffects.Add(newEffect);
            newEffect.OnEquip(this.gameObject, stats);
        }
    }
}