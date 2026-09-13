using System;
using Unity.Netcode;

namespace ProjectGame.HotFix.Gameplay.Weapon
{
    /// <summary>
    /// 一个Effect状态快照，只包含ID和等级
    /// </summary>
    public readonly struct EffectSnapshot
    {
        public readonly ushort EffectId;
        public readonly byte Stack;

        public EffectSnapshot(ushort effectId, byte stack)
        {
            EffectId = effectId;
            Stack = stack;
        }
    }

    public sealed class EffectSet
    {
        public static readonly EffectSet Empty = new EffectSet(0, 0, Array.Empty<EffectSnapshot>(), Array.Empty<ushort>());

        public ushort Id { get; }
        public ulong Mask { get; }
        /// <summary>
        /// 每种Effect只保留一项，Stack 表示当前等级
        /// </summary>
        public EffectSnapshot[] Effects { get; }
        /// <summary>
        /// 每次选择保留一项，用于严格按先后顺序重放Modifier
        /// </summary>
        public ushort[] AcquisitionOrder { get; }

        public EffectSet(ushort id, ulong mask, EffectSnapshot[] effects)
            : this(id, mask, effects, Array.Empty<ushort>())
        {
        }

        public EffectSet(ushort id, ulong mask, EffectSnapshot[] effects, ushort[] acquisitionOrder)
        {
            Id = id;
            Mask = mask;
            Effects = effects == null || effects.Length == 0
                ? Array.Empty<EffectSnapshot>() : (EffectSnapshot[])effects.Clone();
            AcquisitionOrder = acquisitionOrder == null || acquisitionOrder.Length == 0
                ? Array.Empty<ushort>() : (ushort[])acquisitionOrder.Clone();
        }

        public bool TryGetLevel(ushort effectId, out byte level)
        {
            for (int i = 0; i < Effects.Length; i++)
            {
                if (Effects[i].EffectId != effectId) continue;
                level = Effects[i].Stack;
                return true;
            }

            level = 0;
            return false;
        }
    }
    /// <summary>
    /// 存这把枪的数据，是枪械的原始数据，纯数值快照
    /// 所有影响模拟结果的状态都要放到这里
    /// </summary>
    public struct WeaponStatSnapshot
    {
        public ushort Id;
        public float Damage;
        public float FireRate;
        public float ReloadTime;
        // 当前有效属性在固定网络 Tick 下的派生值
        public ushort FireIntervalTicks;
        public ushort ReloadTicks;

        public ushort MagSize;
        public float CritChance;
        public float CritMultiplier;
        public float ProjectileSpeed;
        /// <summary>
        /// 一枪内多发子弹时会压缩为一个上下文传输，共享相同参数
        /// </summary>
        public ushort ProjectileCount;
        public float SpreadAngle;
        public byte BounceCount;
        public byte PierceCount;
        public float ProjectileSize;
        public float ProjectileLifeTime;
    }

    [Flags]
    public enum WeaponRuntimeFlags : byte
    {
        None = 0,
        Reloading = 1 << 0,
        Disabled = 1 << 1,
    }
    /// <summary>
    /// 当前玩家使用时枪械的状态，本身数据贴合预测回滚系统，存时间状态
    /// </summary>
    public struct WeaponRuntimeState : INetworkSerializable, IEquatable<WeaponRuntimeState>
    {
        public ushort WeaponId;
        /// <summary>
        /// 玩家当前数值状态已经更新到第几版
        /// </summary>
        public ushort SnapshotVersion;
        public ushort CurrentAmmo;
        /// <summary>
        /// 某个玩家这把武器逻辑上成功射击次数的单调递增序号
        /// 比较此值能判断玩家是否开了枪
        /// </summary>
        public uint ShotSequence;
        public ushort ReserveAmmo;
        public ushort FireCooldownTicks;
        public ushort ReloadTicksRemaining;
        /// <summary>
        /// 这次具体使用哪一份WeaponStatSnapshot
        /// </summary>
        public ushort StatSnapshotId;
        /// <summary>
        /// 这次具体使用哪一份EffectSet
        /// </summary>
        public ushort EffectSetId;
        public WeaponRuntimeFlags Flags;

        public bool IsReloading => (Flags & WeaponRuntimeFlags.Reloading) != 0;
        public bool IsDisabled => (Flags & WeaponRuntimeFlags.Disabled) != 0;
        public bool IsEquipped => StatSnapshotId != 0;

        public bool Equals(WeaponRuntimeState other) => WeaponId == other.WeaponId &&
            SnapshotVersion == other.SnapshotVersion && CurrentAmmo == other.CurrentAmmo &&
            ReserveAmmo == other.ReserveAmmo && ShotSequence == other.ShotSequence &&
            FireCooldownTicks == other.FireCooldownTicks && ReloadTicksRemaining == other.ReloadTicksRemaining &&
            StatSnapshotId == other.StatSnapshotId && EffectSetId == other.EffectSetId && Flags == other.Flags;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref WeaponId);
            serializer.SerializeValue(ref SnapshotVersion);
            serializer.SerializeValue(ref CurrentAmmo);
            serializer.SerializeValue(ref ReserveAmmo);
            serializer.SerializeValue(ref ShotSequence);
            serializer.SerializeValue(ref FireCooldownTicks);
            serializer.SerializeValue(ref ReloadTicksRemaining);
            serializer.SerializeValue(ref StatSnapshotId);
            serializer.SerializeValue(ref EffectSetId);
            serializer.SerializeValue(ref Flags);
        }
    }

    /// <summary>
    /// 枪械固定的参数值，标明这把枪是什么
    /// </summary>
    public sealed class WeaponDefinition
    {
        public ushort WeaponId { get; }
        public WeaponStatSnapshot Stats { get; }
        public ushort ReserveAmmo { get; }
        public bool AutoReload { get; }
        public WeaponDefinition(ushort weaponId, in WeaponStatSnapshot stats, ushort reserveAmmo, bool autoReload)
        {
            WeaponStatSnapshotRepository.Validate(stats);
            WeaponId = weaponId;
            Stats = stats;
            ReserveAmmo = reserveAmmo;
            AutoReload = autoReload;
        }
    }

    /// <summary>UI 读取值副本。Changed 包含预测和纠正结果，不能作为伤害凭据。</summary>
    public interface IWeaponStateSource
    {
        WeaponRuntimeState CurrentWeaponState { get; }
        WeaponDefinition Definition { get; }
        event Action<WeaponRuntimeState> WeaponStateChanged;
    }
}
