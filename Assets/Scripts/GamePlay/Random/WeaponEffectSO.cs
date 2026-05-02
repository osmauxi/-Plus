using UnityEngine;

// 所有机制特效都必须继承这个类
public abstract class WeaponEffectSO : ScriptableObject, IWeaponEffect
{
    // 提供默认的空实现，返回 true 表示允许正常开火
    public virtual bool OnBeforeFire(WeaponBase weapon, CharacterStatCollection stats) { return true; }

    public virtual void OnEquip(GameObject weaponObj, CharacterStatCollection stats) { }
    public virtual void OnAfterFire(WeaponBase weapon, CharacterStatCollection stats) { }
    public virtual void OnProjectileSpawn(ProjectileBase projectile, CharacterStatCollection stats) { }
    public virtual void OnHit(ProjectileBase projectile, GameObject target, Vector3 hitPoint, CharacterStatCollection stats) { }
    public virtual void OnDestroy(ProjectileBase projectile, Vector3 pos, CharacterStatCollection stats) { }
}