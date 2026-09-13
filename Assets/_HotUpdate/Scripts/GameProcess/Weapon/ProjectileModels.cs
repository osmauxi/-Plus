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
        /// 伤害、特殊 Effect 与穿透/反弹全部结算后的最终结果。
        /// 表现层直接消费这个结果，不需要重复推导子弹是否继续存活。
        /// </summary>
        public ProjectileHitResolution Resolution;
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

        public ProjectileSpawn(in ShotContext shot, in ProjectileState projectile, float visualSize)
        {
            Shot = shot;
            Projectile = projectile;
            VisualSize = visualSize;
        }
    }

    /// <summary>服务端输出接口。VFX/音效模块可订阅并另行广播；预测回放不能发布这些事件。</summary>
    public interface IWeaponEventSource
    {
        event Action<ShotContext> ShotFired;
        event Action<ProjectileSpawn> ProjectileSpawned;
        event Action<ProjectileImpact> ProjectileImpact;
        event Action<ProjectileState> ProjectileRemoved;
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
    /// 受到了一次什么伤害
    /// </summary>
    public readonly struct ProjectileDamageContext
    {
        public readonly ulong SourceEntityId;
        public readonly ulong ShotId;
        public readonly uint ProjectileId;
        public readonly float Damage;
        public readonly Vector3 HitPoint;
        public readonly Vector3 HitDirection;
        public readonly bool IsCritical;

        public ProjectileDamageContext(
            ulong sourceEntityId,
            ulong shotId,
            uint projectileId,
            float damage,
            Vector3 hitPoint,
            Vector3 hitDirection,
            bool isCritical)
        {
            SourceEntityId = sourceEntityId;
            ShotId = shotId;
            ProjectileId = projectileId;
            Damage = damage;
            HitPoint = hitPoint;
            HitDirection = hitDirection;
            IsCritical = isCritical;
        }
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
    /// 声明一个可伤害对象，武器层与Health层的桥接接口
    /// </summary>
    public interface IProjectileHitTarget
    {
        ulong EntityId { get; }
        bool IsAlive { get; }

        void ApplyProjectileDamage(in ProjectileDamageContext context);
    }

    /// <summary>碰撞体解析后的瞬时句柄；Generation 供集中 Health 映射防止复用旧实体。</summary>
    public readonly struct ProjectileResolvedTarget
    {
        public readonly ulong EntityId;
        public readonly uint Generation;
        public readonly bool IsAlive;
        internal readonly IProjectileHitTarget ComponentTarget;
        internal readonly IProjectileTargetResolver Resolver;

        public ProjectileResolvedTarget(ulong entityId, uint generation, bool isAlive)
            : this(entityId, generation, isAlive, null, null) { }

        internal ProjectileResolvedTarget(ulong entityId, uint generation, bool isAlive,
            IProjectileHitTarget componentTarget, IProjectileTargetResolver resolver = null)
        {
            EntityId = entityId;
            Generation = generation;
            IsAlive = isAlive;
            ComponentTarget = componentTarget;
            Resolver = resolver;
        }

        internal ProjectileResolvedTarget WithResolver(IProjectileTargetResolver resolver) =>
            new ProjectileResolvedTarget(EntityId, Generation, IsAlive, ComponentTarget, resolver);
    }

    /// <summary>把 Projectile Physics 与具体命中身份方案解耦；Monster 使用集中 Collider Binding。</summary>
    public interface IProjectileTargetResolver
    {
        bool TryResolve(Collider collider, out ProjectileResolvedTarget target);
        void ApplyDamage(in ProjectileResolvedTarget target, in ProjectileDamageContext context);
    }

    /// <summary>按顺序组合集中 Monster、玩家或其他可伤害对象解析器。</summary>
    public sealed class CompositeProjectileTargetResolver : IProjectileTargetResolver
    {
        private readonly IProjectileTargetResolver[] _resolvers;

        public CompositeProjectileTargetResolver(params IProjectileTargetResolver[] resolvers)
        {
            if (resolvers == null || resolvers.Length == 0)
                throw new ArgumentException("至少需要一个 Projectile TargetResolver。", nameof(resolvers));
            _resolvers = new IProjectileTargetResolver[resolvers.Length];
            for (int i = 0; i < resolvers.Length; i++)
                _resolvers[i] = resolvers[i] ?? throw new ArgumentException("Projectile TargetResolver 不能为空。", nameof(resolvers));
        }

        public bool TryResolve(Collider collider, out ProjectileResolvedTarget target)
        {
            for (int i = 0; i < _resolvers.Length; i++)
            {
                IProjectileTargetResolver resolver = _resolvers[i];
                if (!resolver.TryResolve(collider, out target)) continue;
                if (target.Resolver == null) target = target.WithResolver(resolver);
                return true;
            }
            target = default;
            return false;
        }

        public void ApplyDamage(in ProjectileResolvedTarget target, in ProjectileDamageContext context)
        {
            if (target.Resolver == null)
                throw new InvalidOperationException("ProjectileResolvedTarget 缺少来源 Resolver。");
            target.Resolver.ApplyDamage(target, context);
        }
    }

    /// <summary>旧玩家/旧实体的兼容路径；新数据实体应注入集中式 Resolver。</summary>
    public sealed class ComponentProjectileTargetResolver : IProjectileTargetResolver
    {
        public static readonly ComponentProjectileTargetResolver Instance = new ComponentProjectileTargetResolver();
        private ComponentProjectileTargetResolver() { }

        public bool TryResolve(Collider collider, out ProjectileResolvedTarget target)
        {
            target = default;
            if (collider == null) return false;
            ProjectileHitTargetAdapter adapter = collider.GetComponentInParent<ProjectileHitTargetAdapter>();
            if (adapter == null || !adapter.HasEntityId) return false;
            target = new ProjectileResolvedTarget(adapter.EntityId, 0,
                adapter.Target == null || adapter.Target.IsAlive, adapter.Target);
            return true;
        }

        public void ApplyDamage(in ProjectileResolvedTarget target, in ProjectileDamageContext context) =>
            target.ComponentTarget?.ApplyProjectileDamage(context);
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
