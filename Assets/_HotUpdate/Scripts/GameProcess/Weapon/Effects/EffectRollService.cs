using System;
using System.Collections.Generic;

namespace ProjectGame.HotFix.Gameplay.Weapon.Effects
{
    public readonly struct EffectRollResult
    {
        public readonly IReadOnlyList<EffectRollOption> Options;
        /// <summary>包含显式 Chaos，以及候选不足后实际启用的 Chaos 保底。</summary>
        public readonly bool IsChaos;

        public EffectRollResult(IReadOnlyList<EffectRollOption> options, bool isChaos)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            IsChaos = isChaos;
        }
    }

    public readonly struct EffectRollOption
    {
        public readonly WeaponEffectDefinition Effect;
        public readonly WeaponEffectRollDefinition Presentation;
        public readonly byte CurrentLevel;
        public byte NextLevel => checked((byte)(CurrentLevel + 1));

        public EffectRollOption(
            WeaponEffectDefinition effect,
            WeaponEffectRollDefinition presentation,
            byte currentLevel)
        {
            Effect = effect;
            Presentation = presentation;
            CurrentLevel = currentLevel;
        }
    }

    /// <summary>普通/异变共用的候选过滤与无放回轮盘赌；Chaos 只关闭冲突和流派权重。</summary>
    public sealed class EffectRollService
    {
        private readonly WeaponEffectCatalog _catalog;

        public EffectRollService(WeaponEffectCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public IReadOnlyList<EffectRollOption> Roll(
            WeaponEffectRollPool pool,
            int amount,
            PlayerEffectLoadout loadout,
            int seed,
            bool chaos = false,
            bool fallbackToChaos = true)
            => RollWithResult(pool, amount, loadout, seed, chaos, fallbackToChaos).Options;

        public EffectRollResult RollWithResult(
            WeaponEffectRollPool pool,
            int amount,
            PlayerEffectLoadout loadout,
            int seed,
            bool chaos = false,
            bool fallbackToChaos = true)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (loadout == null) throw new ArgumentNullException(nameof(loadout));
            if (amount == 0) return new EffectRollResult(Array.Empty<EffectRollOption>(), chaos);

            var candidates = BuildCandidates(pool, loadout, chaos);
            bool effectiveChaos = chaos;
            if (!chaos && fallbackToChaos && candidates.Count < amount)
            {
                candidates = BuildCandidates(pool, loadout, true);
                effectiveChaos = true;
            }

            var random = new Random(seed);
            var results = new List<EffectRollOption>(Math.Min(amount, candidates.Count));
            while (results.Count < amount && candidates.Count > 0)
            {
                int index = SelectIndex(candidates, random);
                Candidate selected = candidates[index];
                results.Add(new EffectRollOption(
                    selected.Effect,
                    _catalog.GetRoll(selected.Effect.Id),
                    loadout.GetLevel(selected.Effect.Id)));
                candidates.RemoveAt(index);
            }

            return new EffectRollResult(results, effectiveChaos);
        }

        /// <summary>客户端仅接收服务端候选 ID；展示文本与图片地址始终从本机 EffectRoll 表解析。</summary>
        public IReadOnlyList<EffectRollOption> ResolveOptions(
            IReadOnlyList<ushort> effectIds,
            IReadOnlyList<byte> currentLevels)
        {
            if (effectIds == null) throw new ArgumentNullException(nameof(effectIds));
            if (currentLevels == null) throw new ArgumentNullException(nameof(currentLevels));
            if (effectIds.Count != currentLevels.Count)
                throw new ArgumentException("Effect Roll 候选 ID 与等级数量不一致。");

            var result = new EffectRollOption[effectIds.Count];
            var uniqueIds = new HashSet<ushort>();
            for (int i = 0; i < result.Length; i++)
            {
                ushort id = effectIds[i];
                if (!uniqueIds.Add(id))
                    throw new InvalidOperationException($"Effect Roll 候选包含重复 EffectID={id}。");
                WeaponEffectDefinition effect = _catalog.GetEffect(id);
                byte level = currentLevels[i];
                if (level >= effect.MaxLevel)
                    throw new InvalidOperationException($"EffectID={id} 的候选等级已达到上限。");
                result[i] = new EffectRollOption(effect, _catalog.GetRoll(id), level);
            }
            return Array.AsReadOnly(result);
        }

        private List<Candidate> BuildCandidates(
            WeaponEffectRollPool pool,
            PlayerEffectLoadout loadout,
            bool chaos)
        {
            HashSet<ushort> ownedSchools = CollectOwnedSchools(loadout);
            var candidates = new List<Candidate>();
            foreach (WeaponEffectDefinition effect in _catalog.Effects)
            {
                if (effect.RollPool != pool || loadout.CanAcquire(effect.Id) != EffectAcquireResult.Success)
                    continue;
                if (!chaos && HasConflict(effect.ConflictSchoolIds, ownedSchools))
                    continue;

                float weight = 1f;
                if (!chaos)
                {
                    weight = effect.BaseWeight;
                    for (int i = 0; i < effect.SchoolIds.Length; i++)
                        if (ownedSchools.Contains(effect.SchoolIds[i]))
                            weight += effect.SchoolWeightBonus;
                }

                if (weight > 0f) candidates.Add(new Candidate(effect, weight));
            }

            return candidates;
        }

        private HashSet<ushort> CollectOwnedSchools(PlayerEffectLoadout loadout)
        {
            var result = new HashSet<ushort>();
            foreach (ushort effectId in loadout.AcquisitionOrder)
            {
                WeaponEffectDefinition effect = _catalog.GetEffect(effectId);
                for (int i = 0; i < effect.SchoolIds.Length; i++) result.Add(effect.SchoolIds[i]);
            }
            return result;
        }

        private static bool HasConflict(ushort[] conflicts, HashSet<ushort> ownedSchools)
        {
            for (int i = 0; i < conflicts.Length; i++)
                if (ownedSchools.Contains(conflicts[i])) return true;
            return false;
        }

        private static int SelectIndex(List<Candidate> candidates, Random random)
        {
            double total = 0;
            for (int i = 0; i < candidates.Count; i++) total += candidates[i].Weight;
            double value = random.NextDouble() * total;
            double accumulated = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                accumulated += candidates[i].Weight;
                if (value <= accumulated) return i;
            }
            return candidates.Count - 1;
        }

        private readonly struct Candidate
        {
            public readonly WeaponEffectDefinition Effect;
            public readonly float Weight;
            public Candidate(WeaponEffectDefinition effect, float weight)
            {
                Effect = effect;
                Weight = weight;
            }
        }
    }
}
