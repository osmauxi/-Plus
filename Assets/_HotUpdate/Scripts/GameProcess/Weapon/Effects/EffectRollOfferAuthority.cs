using System;
using System.Collections.Generic;

namespace ProjectGame.HotFix.Gameplay.Weapon.Effects
{
    public enum EffectRollSelectionResult : byte
    {
        Success = 0,
        NoActiveOffer = 1,
        OfferMismatch = 2,
        EffectNotOffered = 3,
        AcquireRejected = 4,
        Cancelled = 5,
    }

    /// <summary>
    /// 一次Effect Roll的只读快照；展示数据来自本机EffectRoll表
    /// </summary>
    public sealed class EffectRollOffer
    {
        private readonly EffectRollOption[] _options;
        private readonly IReadOnlyList<EffectRollOption> _readOnlyOptions;

        public uint Id { get; }
        public WeaponEffectRollPool Pool { get; }
        public bool IsChaos { get; }
        public IReadOnlyList<EffectRollOption> Options => _readOnlyOptions;

        public EffectRollOffer(uint id, WeaponEffectRollPool pool, bool isChaos,
            IReadOnlyList<EffectRollOption> options)
        {
            if (id == 0) 
                throw new ArgumentOutOfRangeException(nameof(id));
            if (!Enum.IsDefined(typeof(WeaponEffectRollPool), pool))
                throw new ArgumentOutOfRangeException(nameof(pool));
            if (options == null) 
                throw new ArgumentNullException(nameof(options));

            Id = id;
            Pool = pool;
            IsChaos = isChaos;
            _options = new EffectRollOption[options.Count];
            for (int i = 0; i < options.Count; i++) 
                _options[i] = options[i];
            _readOnlyOptions = Array.AsReadOnly(_options);
        }

        public bool Contains(ushort effectId)
        {
            for (int i = 0; i < _options.Length; i++)
                if (_options[i].Effect.Id == effectId) return true;
            return false;
        }

        public ushort[] CopyEffectIds()
        {
            var result = new ushort[_options.Length];
            for (int i = 0; i < result.Length; i++) result[i] = _options[i].Effect.Id;
            return result;
        }

        public byte[] CopyCurrentLevels()
        {
            var result = new byte[_options.Length];
            for (int i = 0; i < result.Length; i++) result[i] = _options[i].CurrentLevel;
            return result;
        }
    }

    public interface IEffectRollOfferSource
    {
        EffectRollOffer CurrentEffectRollOffer { get; }
        event Action<EffectRollOffer> EffectRollOffered;
        event Action<uint, ushort, EffectRollSelectionResult> EffectRollResolved;
        /// <summary>
        /// 仅Server/Host接受；为当前所有玩家各创建一份指定池的权威Offer
        /// </summary>
        bool RequestEffectRollBroadcast(WeaponEffectRollPool pool);
        bool RequestEffectSelection(uint offerId, ushort effectId);
    }

    /// <summary>
    /// 服务端 Roll 权威边界。只保存一份当前候选，选择时同时校验 OfferID、候选成员和最新等级。
    /// 这里不直接写 NetworkList，网络适配层验证成功后再提交唯一一次状态变更。
    /// </summary>
    public sealed class EffectRollOfferAuthority
    {
        private readonly EffectRollService _rolls;
        private readonly PlayerEffectLoadout _loadout;
        private readonly Queue<EffectRollRequest> _pendingRequests = new();
        private uint _nextOfferId;

        public EffectRollOffer ActiveOffer { get; private set; }
        public int PendingCount => _pendingRequests.Count;

        public EffectRollOfferAuthority(EffectRollService rolls, PlayerEffectLoadout loadout)
        {
            _rolls = rolls ?? throw new ArgumentNullException(nameof(rolls));
            _loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
        }

        public EffectRollOffer Create(WeaponEffectRollPool pool, int amount, int seed,
            bool chaos = false, bool fallbackToChaos = true)
        {
            uint id = unchecked(++_nextOfferId);
            if (id == 0) id = unchecked(++_nextOfferId);
            EffectRollResult result = _rolls.RollWithResult(
                pool, amount, _loadout, seed, chaos, fallbackToChaos);
            ActiveOffer = new EffectRollOffer(id, pool, result.IsChaos, result.Options);
            return ActiveOffer;
        }

        /// <summary>
        /// 当前没有候选时立即创建；已有候选时按触发顺序累计缓存，等待本轮结算后再生成。
        /// 延迟生成可以让下一轮候选读取本轮选择后的最新 Effect 等级与冲突状态。
        /// </summary>
        public EffectRollOffer Request(WeaponEffectRollPool pool, int amount, int seed,
            bool chaos = false, bool fallbackToChaos = true)
        {
            if (ActiveOffer == null)
                return Create(pool, amount, seed, chaos, fallbackToChaos);

            _pendingRequests.Enqueue(new EffectRollRequest(
                pool, amount, seed, chaos, fallbackToChaos));
            return null;
        }

        public bool TryCreateNext(out EffectRollOffer offer)
        {
            if (ActiveOffer != null || _pendingRequests.Count == 0)
            {
                offer = null;
                return false;
            }

            EffectRollRequest request = _pendingRequests.Dequeue();
            offer = Create(request.Pool, request.Amount, request.Seed,
                request.Chaos, request.FallbackToChaos);
            return true;
        }

        public EffectRollSelectionResult TryConsume(uint offerId, ushort effectId)
        {
            EffectRollOffer offer = ActiveOffer;
            if (offer == null) return EffectRollSelectionResult.NoActiveOffer;
            if (offer.Id != offerId) return EffectRollSelectionResult.OfferMismatch;
            if (!offer.Contains(effectId)) return EffectRollSelectionResult.EffectNotOffered;
            if (_loadout.CanAcquire(effectId) != EffectAcquireResult.Success)
            {
                ActiveOffer = null;
                return EffectRollSelectionResult.AcquireRejected;
            }

            ActiveOffer = null;
            return EffectRollSelectionResult.Success;
        }

        public void Cancel() => ActiveOffer = null;

        public void ClearPending() => _pendingRequests.Clear();

        private readonly struct EffectRollRequest
        {
            public readonly WeaponEffectRollPool Pool;
            public readonly int Amount;
            public readonly int Seed;
            public readonly bool Chaos;
            public readonly bool FallbackToChaos;

            public EffectRollRequest(WeaponEffectRollPool pool, int amount, int seed,
                bool chaos, bool fallbackToChaos)
            {
                Pool = pool;
                Amount = amount;
                Seed = seed;
                Chaos = chaos;
                FallbackToChaos = fallbackToChaos;
            }
        }
    }
}
