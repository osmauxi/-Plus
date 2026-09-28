using System;
using System.Collections.Generic;

namespace ProjectGame.HotFix.Gameplay.Weapon.Effects
{
    public interface IPlayerEffectStateSource
    {
        IReadOnlyList<ushort> EffectAcquisitionOrder { get; }
        EffectSet CurrentEffectSet { get; }
        EffectOwnerStatSnapshot CurrentOwnerEffectStats { get; }
        event Action<EffectSet, EffectOwnerStatSnapshot> EffectsChanged;
    }
    public enum EffectAcquireResult : byte
    {
        Success = 0,
        UnknownEffect = 1,
        NotRepeatable = 2,
        MaxLevelReached = 3,
    }

    /// <summary>
    /// 玩家Effect数据，数值计算根据顺序来
    /// </summary>
    public sealed class PlayerEffectLoadout
    {
        private readonly WeaponEffectCatalog _catalog;
        /// <summary>
        /// 玩家获取Effect的顺序
        /// </summary>
        private readonly List<ushort> _acquisitionOrder = new();
        /// <summary>
        /// Effect第一次出现的顺序，更新快照包用的到
        /// </summary>
        private readonly List<ushort> _uniqueOrder = new();
        /// <summary>
        /// Effect的等级
        /// </summary>
        private readonly Dictionary<ushort, byte> _levels = new();

        public int AcquisitionCount => _acquisitionOrder.Count;
        public IReadOnlyList<ushort> AcquisitionOrder => _acquisitionOrder;

        public PlayerEffectLoadout(WeaponEffectCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public byte GetLevel(ushort effectId) => _levels.TryGetValue(effectId, out byte level) ? level : (byte)0;
        /// <summary>
        /// LoadOut本身不知道这个Effect能不能叠加以及最大等级，去Definition找并返回判定
        /// </summary>
        public EffectAcquireResult CanAcquire(ushort effectId)
        {
            if (!_catalog.TryGetEffect(effectId, out WeaponEffectDefinition definition))
                return EffectAcquireResult.UnknownEffect;
            byte level = GetLevel(effectId);
            if (level == 0) 
                return EffectAcquireResult.Success;
            if (!definition.Repeatable) 
                return EffectAcquireResult.NotRepeatable;
            return level >= definition.MaxLevel ? EffectAcquireResult.MaxLevelReached : EffectAcquireResult.Success;
        }
        /// <summary>
        /// 更新Effect时触发
        /// </summary>
        public EffectAcquireResult TryAcquire(ushort effectId)
        {
            EffectAcquireResult result = CanAcquire(effectId);
            if (result != EffectAcquireResult.Success) 
                return result;

            byte previous = GetLevel(effectId);
            if (previous == 0) 
                _uniqueOrder.Add(effectId);
            _levels[effectId] = checked((byte)(previous + 1));
            _acquisitionOrder.Add(effectId);
            return EffectAcquireResult.Success;
        }
        /// <summary>
        /// 根据完整Effect获取顺序，重建完整的LoadOut信息
        /// </summary>
        public void Rebuild(IEnumerable<ushort> acquisitionOrder)
        {
            if (acquisitionOrder == null) throw new ArgumentNullException(nameof(acquisitionOrder));
            Clear();
            foreach (ushort effectId in acquisitionOrder)
            {
                EffectAcquireResult result = TryAcquire(effectId);
                if (result != EffectAcquireResult.Success)
                {
                    Clear();
                    throw new InvalidOperationException($"Effect 获取序列非法：EffectID={effectId}，原因={result}。");
                }
            }
        }
        /// <summary>
        /// Effect更改时，重新生成权威EffectSet
        /// </summary>
        public EffectSet CreateSnapshot(ushort snapshotId)
        {
            if (_acquisitionOrder.Count == 0) 
                return EffectSet.Empty;
            var effects = new EffectSnapshot[_uniqueOrder.Count];
            ulong mask = 0;
            for (int i = 0; i < _uniqueOrder.Count; i++)
            {
                ushort id = _uniqueOrder[i];
                effects[i] = new EffectSnapshot(id, _levels[id]);
                mask |= 1UL << (id & 63);
            }

            return new EffectSet(snapshotId, mask, effects, _acquisitionOrder.ToArray());
        }

        public void Clear()
        {
            _acquisitionOrder.Clear();
            _uniqueOrder.Clear();
            _levels.Clear();
        }
    }
}
