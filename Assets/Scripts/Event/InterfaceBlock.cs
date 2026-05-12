using Unity.Netcode;
using UnityEngine;

public interface INetEvent : INetworkSerializable
{
    //自动转发，True时，服务器不需要进行验证，直接走转发
    bool AutoBroadcast => false;
}

public interface IWeaponEffect
{
    // 装备到武器时触发
    void OnEquip(GameObject weaponObj, CharacterStatCollection stats);

    // 开火前触发（返回 false 可以取消本次默认开火）
    bool OnBeforeFire(WeaponBase weapon, CharacterStatCollection stats);

    // 开火后触发（用于处理后坐力、枪管发热等）
    void OnAfterFire(WeaponBase weapon, CharacterStatCollection stats);

    // 子弹生成时触发
    void OnProjectileSpawn(ProjectileBase projectile, CharacterStatCollection stats);

    // 击中时触发
    void OnHit(ProjectileBase projectile, GameObject target, Vector3 hitPoint, CharacterStatCollection stats);

    // 子弹销毁时触发
    void OnProjectileDestroyed(ProjectileBase projectile, Vector3 destroyPoint, CharacterStatCollection stats);
}

public interface IUpgradeableEffect
{
    public void Upgrade();
}

/// <summary>
/// 基础 AI 模块接口
/// </summary>
public interface IAIModule
{
    // 每个模块每一帧都要执行自己的逻辑，必须把黑板传给它！
    void ExecuteTick(AIBlackboard blackboard);
}

// 细分三大职责，语义更明确
public interface ITargetingModule : IAIModule { } // 负责找目标
public interface IMovementModule : IAIModule { }  // 负责寻路和走位
public interface IAttackModule : IAIModule { }    // 负责扣动扳机和播动画

public interface IInteractable
{
    // 决定该物体当前是否可以被交互（比如宝箱开过一次后就变为 false）
    bool IsInteractable { get; }

    // 玩家靠近时，UI 上显示的提示词（例如 "按 F 开启宝箱", "按 F 献祭 30% 生命值"）
    string InteractPrompt { get; }

    // 当玩家按下交互键时触发。传入触发者的 GameObject，方便溯源。
    void OnInteract(GameObject interactor);
}
public interface IKnockbackable
{
    /// <summary>
    /// 接收击退指令
    /// </summary>
    /// <param name="force">算好的纯横向击退力</param>
    void ApplyKnockback(Vector3 force);
}