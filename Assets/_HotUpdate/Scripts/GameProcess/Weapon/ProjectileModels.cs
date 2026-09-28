using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Weapon
{
    /// <summary>
    /// 相同性质子弹的共同上下文，用于实现增量传输，不需要每个子弹都存一些重复度很高的内容
    /// </summary>
    public struct ShotContext
    {
        public ulong ShotId;
        public uint FireTick;
        public uint ShotSequence;
        public ulong OwnerEntityId;
        public ushort WeaponId;
        public uint RandomSeed;
        public Vector3 Origin;
        public Vector3 AimDirection;
        public ushort StatSnapshotId;
        public ushort EffectSetId;
    }

    /// <summary>
    /// 已确认的服务器碰撞事实，不可回滚，主要是表现层事实，触发音效与VFX等
    /// </summary>
    public struct ProjectileImpact
    {
        public ShotContext Shot;
        public ProjectileState Projectile;
        public Vector3 Point;
        public Vector3 Normal;
        public bool HasTarget;
        public ulong TargetEntityId;
        /// <summary>
        /// 服务端根据本次实际伤害、命中时弹体尺寸与暴击状态计算出的表现权重。
        /// 该值不设上限，由命中特效决定如何消费。
        /// </summary>
        public float VfxWeight;
        /// <summary>
        /// 伤害、特殊 Effect 与穿透/反弹全部结算后的最终结果。
        /// 表现层直接消费这个结果，不需要重复推导子弹是否继续存活。
        /// </summary>
        public ProjectileHitResolution Resolution;
    }

    /// <summary>把权威命中数据映射为本地命中特效使用的无上限权重。</summary>
    public static class ProjectileImpactVfxWeight
    {
        // 当前标准武器为 20 伤害、0.1 弹体尺寸，因此普通命中的基础权重为 1。
        public const float ReferenceDamage = 20f;
        public const float ReferenceProjectileSize = 0.1f;
        public const float CriticalEmphasis = 1.5f;

        public static float Calculate(float damage, float projectileSize, bool isCritical)
        {
            float damageWeight = damage / ReferenceDamage;
            float sizeWeight = projectileSize / ReferenceProjectileSize;
            float criticalWeight = isCritical ? CriticalEmphasis : 1f;
            return damageWeight * sizeWeight * criticalWeight;
        }
    }

    /// <summary>
    /// 特殊 Effect 已经完成出生阶段修改后的权威子弹快照。
    /// VisualSize 是当前子弹真正使用的表现尺寸，客户端不需要反查服务端属性快照。
    /// </summary>
    public readonly struct ProjectileSpawn
    {
        public readonly ShotContext Shot;
        public readonly ProjectileState Projectile;
        public readonly float VisualSize;
        // 当前弹体真正进入权威世界的 Tick。分裂弹晚于母弹出生，不能复用 Shot.FireTick 做表现快进。
        public readonly uint SpawnTick;

        public ProjectileSpawn(in ShotContext shot, in ProjectileState projectile, float visualSize)
            : this(shot, projectile, visualSize, shot.FireTick)
        {
        }

        public ProjectileSpawn(
            in ShotContext shot,
            in ProjectileState projectile,
            float visualSize,
            uint spawnTick)
        {
            Shot = shot;
            Projectile = projectile;
            VisualSize = visualSize;
            SpawnTick = spawnTick;
        }
    }

    /// <summary>服务端输出接口。VFX/音效模块可订阅并另行广播；预测回放不能发布这些事件。</summary>
    public interface IWeaponEventSource
    {
        event Action<ShotContext> ShotFired;
        event Action<ProjectileSpawn> ProjectileSpawned;
        event Action<ProjectileImpact> ProjectileImpact;
        event Action<ProjectileState> ProjectileRemoved;
        event Action<WeaponSpecialVfxEvent> SpecialEffectVfx;
    }

    public enum WeaponSpecialVfxType : byte
    {
        Execution = 0,
        Shockwave = 1,
        RadiationArea = 2,
        KineticBoost = 3,
        StormCloudStart = 4,
        StormCloudStop = 5,
        LightningArc = 6,
    }

    /// <summary>
    /// 特殊 Effect 的纯表现事实。伤害和控制仍只在服务器计算；客户端只按这些数据画轻量特效。
    /// </summary>
    public readonly struct WeaponSpecialVfxEvent
    {
        public readonly WeaponSpecialVfxType Type;
        public readonly ulong InstanceId;
        public readonly ulong OwnerEntityId;
        public readonly Vector3 Position;
        public readonly Vector3 TargetPosition;
        public readonly float Radius;
        public readonly float Duration;

        public WeaponSpecialVfxEvent(
            WeaponSpecialVfxType type,
            ulong instanceId,
            ulong ownerEntityId,
            Vector3 position,
            Vector3 targetPosition,
            float radius,
            float duration)
        {
            Type = type;
            InstanceId = instanceId;
            OwnerEntityId = ownerEntityId;
            Position = position;
            TargetPosition = targetPosition;
            Radius = radius;
            Duration = duration;
        }
    }

    [Flags]
    public enum ProjectileFlags : ushort
    {
        None = 0,
        Critical = 1 << 0,
        //用于生成时的特殊检测逻辑
        SpawnChecked = 1 << 1,
    }

    /// <summary>
    /// 一个实际创建出来的子弹的完整状态信息
    /// </summary>
    public struct ProjectileState
    {
        public uint ProjectileId;
        public ulong ShotId;
        public Vector3 Position;
        public Vector3 Velocity;
        public float RemainingLifeTime;
        public float DamageMultiplier;
        public float SizeMultiplier;
        public byte PierceRemaining;
        public byte BounceRemaining;
        public byte Generation;
        public ushort HitCount;
        public ProjectileFlags Flags;
    }
    /// <summary>
    /// 给Effect使用的，标记这次命中的信息
    /// </summary>
    public readonly struct ProjectileHitContext
    {
        public readonly ShotContext Shot;
        public readonly WeaponStatSnapshot Stats;
        public readonly ulong TargetEntityId;
        public readonly Vector3 HitPoint;
        public readonly Vector3 HitNormal;

        public ProjectileHitContext(
            in ShotContext shot,
            in WeaponStatSnapshot stats,
            ulong targetEntityId,
            Vector3 hitPoint,
            Vector3 hitNormal)
        {
            Shot = shot;
            Stats = stats;
            TargetEntityId = targetEntityId;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
        }
    }

    public readonly struct ProjectileSpawnEffectContext
    {
        public readonly ShotContext Shot;
        public readonly WeaponStatSnapshot Stats;
        public ProjectileSpawnEffectContext(in ShotContext shot, in WeaponStatSnapshot stats)
        {
            Shot = shot;
            Stats = stats;
        }
    }

    public readonly struct ProjectileDestroyedEffectContext
    {
        public readonly ShotContext Shot;
        public readonly WeaponStatSnapshot Stats;
        public ProjectileDestroyedEffectContext(in ShotContext shot, in WeaponStatSnapshot stats)
        {
            Shot = shot;
            Stats = stats;
        }
    }
    /// <summary>
    /// 将弹丸命中事件交给效果系统，决定有哪些额外Effect响应
    /// </summary>
    public interface IProjectileEffectDispatcher
    {
        void DispatchSpawn(ref ProjectileState projectile, in ProjectileSpawnEffectContext context);

        void DispatchHit(ref ProjectileState projectile, in ProjectileHitContext context);

        void DispatchBounce(
            ref ProjectileState projectile,
            in ShotContext shot,
            in WeaponStatSnapshot stats,
            Vector3 hitPoint,
            Vector3 hitNormal);

        void DispatchDestroyed(ref ProjectileState projectile, in ProjectileDestroyedEffectContext context);
    }

    public sealed class NoneProjectileEffectDispatcher : IProjectileEffectDispatcher
    {
        public static readonly NoneProjectileEffectDispatcher Instance = new NoneProjectileEffectDispatcher();

        private NoneProjectileEffectDispatcher()
        {
        }

        public void DispatchSpawn(ref ProjectileState projectile, in ProjectileSpawnEffectContext context)
        {
        }

        public void DispatchHit(ref ProjectileState projectile, in ProjectileHitContext context)
        {
        }

        public void DispatchBounce(
            ref ProjectileState projectile,
            in ShotContext shot,
            in WeaponStatSnapshot stats,
            Vector3 hitPoint,
            Vector3 hitNormal)
        {
        }

        public void DispatchDestroyed(ref ProjectileState projectile, in ProjectileDestroyedEffectContext context)
        {
        }
    }

    public sealed class ProjectileSimulationConfig
    {
        public LayerMask TargetMask;
        public LayerMask WorldMask;
        public float MinimumRadius = 0.02f;
        public float SurfaceOffset = 0.01f;
        public int MaxCollisionsPerTick = 8;
    }

    public enum ProjectileHitResolution : byte
    {
        Destroy = 0,
        Continue = 1,
    }
}
