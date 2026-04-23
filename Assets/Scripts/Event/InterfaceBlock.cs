using Unity.Netcode;
using UnityEngine;
public interface IMovementDriver//策略模式接口，用于局内实时切换状态同步和帧同步
{
    // 初始化：把 Controller 传进去，方便 Driver 访问 RPC 和 组件
    void Initialize(PlayerController controller);

    // 对应 Unity 的 Update (处理输入、表现层插值)
    void OnUpdate(float deltaTime);

    // 对应 Unity 的 FixedUpdate (处理核心物理模拟)
    void OnFixedUpdate(float deltaTime);

    // 对应 OnNetworkSpawn (初始化状态)
    void OnNetworkSpawn();

    void OnDisable();
}
public interface IPhysicsQuery//物理查询接口，用于检测地面和碰撞
 //帧同步极其严格，要求所有方法必须具有确定性，也就是所有设备计算结果必须一致，physics这些方法是具有不确定性的，需要自己写实现
{
    bool CheckGround(Vector3 position,Vector3 offset, float radius, LayerMask layer,out float groundHeight);
    bool CheckCollision(Vector3 start, Vector3 end, float radius, LayerMask layer);
    bool CheckObstacle(Vector3 start, Vector3 end, float radius, LayerMask layer);
}

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
    void OnDestroy(ProjectileBase projectile, Vector3 destroyPoint, CharacterStatCollection stats);
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