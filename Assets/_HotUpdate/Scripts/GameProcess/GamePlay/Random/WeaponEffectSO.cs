using UnityEngine;
// 所有机制特效都必须继承这个类
public abstract class WeaponEffectSO : ScriptableObject, IWeaponEffect
{
    [Header("词条绑定 ID")]
    [Tooltip("必须与 ModifierDataSO 中的 modifierId 保持完全一致！")]
    public string modifierId;
    // 提供默认的空实现，返回 true 表示允许正常开火
    public virtual bool OnBeforeFire(WeaponBase weapon, CharacterStatCollection stats) { return true; }

    public virtual void OnEquip(GameObject weaponObj, CharacterStatCollection stats) { }
    public virtual void OnAfterFire(WeaponBase weapon, CharacterStatCollection stats) { }
    public virtual void OnProjectileSpawn(ProjectileBase projectile, CharacterStatCollection stats) { }
    public virtual void OnHit(ProjectileBase projectile, GameObject target, Vector3 hitPoint, CharacterStatCollection stats) { }
    public virtual void OnProjectileDestroyed(ProjectileBase projectile, Vector3 pos, CharacterStatCollection stats) { }

    /// <summary>
    /// 获取玩家当前拥有该词条的层数 (NGO 联机架构核心)
    /// </summary>
    public int GetCurrentStacks(CharacterStatCollection stats)
    {
        // 从玩家身上获取词条管理器
        if (stats.TryGetComponent<PlayerModifierHandler>(out var handler))
        {
            // 从缓存字典中读取当前词条的层数
            if (handler.cachedStackCounts.TryGetValue(modifierId, out int stacks))
            {
                return stacks;
            }
        }
        return 0; // 没找到或者没拿到，视为 0 层
    }
}