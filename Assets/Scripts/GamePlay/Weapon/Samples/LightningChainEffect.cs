using UnityEngine;

public class LightningChainEffect : IWeaponEffect
{
    private float procChance;      // 触发概率
    private float lightningDamage; // 雷电伤害
    private float jumpRadius;      // 弹跳半径

    // 构造函数：当玩家捡起这个词条时，初始化它的参数
    public LightningChainEffect(float chance = 0.25f, float damage = 15f, float radius = 5f)
    {
        procChance = chance;
        lightningDamage = damage;
        jumpRadius = radius;
    }

    public void OnEquip(GameObject weaponObj, CharacterStatCollection stats) { /* 枪管滋滋冒电 */ }

    public void OnProjectileSpawn(ProjectileBase projectile, CharacterStatCollection stats)
    {
        // 把子弹变成蓝色发光体
        projectile.GetComponent<Renderer>().material.color = Color.cyan;
    }

    //只在击中时做事！
    public void OnHit(ProjectileBase projectile, GameObject target, Vector3 hitPoint, CharacterStatCollection stats)
    {
        // 1. 判定触发概率
        if (Random.value > procChance) return;

        Debug.Log("触发雷电链！");

        // 2. 找到周围的敌人 (利用 LayerMask 筛选 Enemy 层)
        Collider[] hitEnemies = Physics.OverlapSphere(hitPoint, jumpRadius);
        foreach (var col in hitEnemies)
        {
            if (col.gameObject == target) continue; // 不重复电击当前目标

            if (col.CompareTag("Enemy"))
            {
                // TODO: 播放一条从 hitPoint 到 col.transform.position 的闪电粒子连线

                // TODO: 造成雷电伤害
                // col.GetComponent<Health>().TakeDamage(lightningDamage);
                Debug.Log($"雷电链电击了 {col.name}，造成 {lightningDamage} 点伤害！");
            }
        }
    }


    public bool OnBeforeFire(WeaponBase weapon, CharacterStatCollection stats)
    {
        return true;
    }

    public void OnAfterFire(WeaponBase weapon, CharacterStatCollection stats)
    {
       
    }

    public void OnProjectileDestroyed(ProjectileBase projectile, Vector3 destroyPoint, CharacterStatCollection stats)
    {
        
    }
}