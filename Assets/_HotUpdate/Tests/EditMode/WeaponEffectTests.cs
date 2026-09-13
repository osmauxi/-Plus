using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectGame.HotFix.Gameplay.Combat.Health;
using ProjectGame.HotFix.Gameplay.Weapon;
using ProjectGame.HotFix.Gameplay.Weapon.Effects;
using ProjectGame.HotFix.Gameplay.Weapon.Effects.Special;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Tests
{
    public sealed class WeaponEffectTests
    {
        [Test]
        public void ModifierCalculation_PreservesAcquisitionOrder()
        {
            WeaponEffectCatalog catalog = BuildCatalog(
                new[]
                {
                    Effect(1, "MultiplyDamage", new[] { 11 }),
                    Effect(2, "AddDamage", new[] { 12 }),
                },
                new[]
                {
                    Modifier(11, "DamageTimesTwo", WeaponModifierStat.Damage, WeaponModifierOperation.Multiply, 2f),
                    Modifier(12, "DamagePlusTen", WeaponModifierStat.Damage, WeaponModifierOperation.Add, 10f),
                });
            var calculator = new WeaponStatModifierCalculator(catalog);

            var multiplyThenAdd = new PlayerEffectLoadout(catalog);
            multiplyThenAdd.TryAcquire(1);
            multiplyThenAdd.TryAcquire(2);
            WeaponEffectCalculation first = calculator.Calculate(
                BaseStats(), multiplyThenAdd.CreateSnapshot(1), 2, 1f / 30f);

            var addThenMultiply = new PlayerEffectLoadout(catalog);
            addThenMultiply.TryAcquire(2);
            addThenMultiply.TryAcquire(1);
            WeaponEffectCalculation second = calculator.Calculate(
                BaseStats(), addThenMultiply.CreateSnapshot(2), 3, 1f / 30f);

            Assert.That(first.WeaponStats.Damage, Is.EqualTo(30f));
            Assert.That(second.WeaponStats.Damage, Is.EqualTo(40f));
        }

        [Test]
        public void Multiply_UsesExactFactor_AndShieldIsIncluded()
        {
            WeaponEffectCatalog catalog = BuildCatalog(
                new[] { Effect(1, "Mixed", new[] { 11, 12, 13 }) },
                new[]
                {
                    Modifier(11, "DamageUp", WeaponModifierStat.Damage, WeaponModifierOperation.Multiply, 1.2f),
                    Modifier(12, "FireRateDown", WeaponModifierStat.FireRate, WeaponModifierOperation.Multiply, 0.8f),
                    Modifier(13, "ShieldUp", WeaponModifierStat.ShieldCapacity, WeaponModifierOperation.Add, 10f),
                });
            var loadout = new PlayerEffectLoadout(catalog);
            loadout.TryAcquire(1);

            WeaponEffectCalculation result = new WeaponStatModifierCalculator(catalog).Calculate(
                BaseStats(), loadout.CreateSnapshot(1), 2, 1f / 30f, 20f);

            Assert.That(result.WeaponStats.Damage, Is.EqualTo(12f).Within(0.0001f));
            Assert.That(result.WeaponStats.FireRate, Is.EqualTo(4f).Within(0.0001f));
            Assert.That(result.OwnerStats.ShieldCapacity, Is.EqualTo(30f));
        }

        [Test]
        public void Loadout_EnforcesRepeatableAndMaxLevel()
        {
            Config_Effect single = Effect(1, "Single", new[] { 11 });
            Config_Effect stacked = Effect(2, "Stacked", new[] { 11 });
            stacked.Repeatable = true;
            stacked.MaxLevel = 2;
            WeaponEffectCatalog catalog = BuildCatalog(
                new[] { single, stacked },
                new[] { Modifier(11, "Damage", WeaponModifierStat.Damage, WeaponModifierOperation.Add, 1f) });
            var loadout = new PlayerEffectLoadout(catalog);

            Assert.That(loadout.TryAcquire(1), Is.EqualTo(EffectAcquireResult.Success));
            Assert.That(loadout.TryAcquire(1), Is.EqualTo(EffectAcquireResult.NotRepeatable));
            Assert.That(loadout.TryAcquire(2), Is.EqualTo(EffectAcquireResult.Success));
            Assert.That(loadout.TryAcquire(2), Is.EqualTo(EffectAcquireResult.Success));
            Assert.That(loadout.TryAcquire(2), Is.EqualTo(EffectAcquireResult.MaxLevelReached));
            Assert.That(loadout.CreateSnapshot(1).AcquisitionOrder,
                Is.EqualTo(new ushort[] { 1, 2, 2 }));
        }

        [Test]
        public void Roll_ConflictFiltersNormally_ChaosKeepsMaxLevelRule()
        {
            Config_Effect safe = Effect(1, "Safe", new[] { 11 });
            Config_Effect conflict = Effect(2, "Conflict", new[] { 11 });
            conflict.ConflictSchoolIDs = new[] { 9 };
            Config_Effect owned = Effect(3, "Owned", new[] { 11 });
            owned.SchoolIDs = new[] { 9 };
            WeaponEffectCatalog catalog = BuildCatalog(
                new[] { safe, conflict, owned },
                new[] { Modifier(11, "Damage", WeaponModifierStat.Damage, WeaponModifierOperation.Add, 1f) });
            var loadout = new PlayerEffectLoadout(catalog);
            loadout.TryAcquire(3);
            var roller = new EffectRollService(catalog);

            IReadOnlyList<EffectRollOption> normal = roller.Roll(
                WeaponEffectRollPool.Standard, 3, loadout, 123, false, false);
            IReadOnlyList<EffectRollOption> chaos = roller.Roll(
                WeaponEffectRollPool.Standard, 3, loadout, 123, true, false);
            EffectRollResult fallback = roller.RollWithResult(
                WeaponEffectRollPool.Standard, 3, loadout, 123, false, true);

            Assert.That(normal.Count, Is.EqualTo(1));
            Assert.That(normal[0].Effect.Id, Is.EqualTo(1));
            Assert.That(chaos.Count, Is.EqualTo(2));
            Assert.That(Contains(chaos, 1), Is.True);
            Assert.That(Contains(chaos, 2), Is.True);
            Assert.That(Contains(chaos, 3), Is.False);
            Assert.That(fallback.IsChaos, Is.True);
            Assert.That(fallback.Options.Count, Is.EqualTo(2));
        }

        [Test]
        public void RollOffer_OnlyConsumesAnEffectFromTheActiveOffer()
        {
            WeaponEffectCatalog catalog = BuildCatalog(
                new[]
                {
                    Effect(1, "First", new[] { 11 }),
                    Effect(2, "Second", new[] { 11 }),
                },
                new[] { Modifier(11, "Damage", WeaponModifierStat.Damage,
                    WeaponModifierOperation.Add, 1f) });
            var loadout = new PlayerEffectLoadout(catalog);
            var authority = new EffectRollOfferAuthority(new EffectRollService(catalog), loadout);

            EffectRollOffer offer = authority.Create(WeaponEffectRollPool.Standard, 1, 42);
            ushort offered = offer.Options[0].Effect.Id;
            ushort notOffered = offered == 1 ? (ushort)2 : (ushort)1;

            Assert.That(authority.TryConsume(offer.Id, notOffered),
                Is.EqualTo(EffectRollSelectionResult.EffectNotOffered));
            Assert.That(authority.ActiveOffer, Is.SameAs(offer));
            Assert.That(authority.TryConsume(offer.Id, offered),
                Is.EqualTo(EffectRollSelectionResult.Success));
            Assert.That(authority.ActiveOffer, Is.Null);
            Assert.That(authority.TryConsume(offer.Id, offered),
                Is.EqualTo(EffectRollSelectionResult.NoActiveOffer));
        }

        [Test]
        public void RollOffer_RejectsCandidateThatBecameInvalidBeforeSelection()
        {
            WeaponEffectCatalog catalog = BuildCatalog(
                new[] { Effect(1, "Single", new[] { 11 }) },
                new[] { Modifier(11, "Damage", WeaponModifierStat.Damage,
                    WeaponModifierOperation.Add, 1f) });
            var loadout = new PlayerEffectLoadout(catalog);
            var authority = new EffectRollOfferAuthority(new EffectRollService(catalog), loadout);
            EffectRollOffer offer = authority.Create(WeaponEffectRollPool.Standard, 1, 7);
            loadout.TryAcquire(1);

            Assert.That(authority.TryConsume(offer.Id, 1),
                Is.EqualTo(EffectRollSelectionResult.AcquireRejected));
            Assert.That(authority.ActiveOffer, Is.Null);
        }

        [Test]
        public void RollOffer_QueuesRequestsAndCreatesNextAfterResolution()
        {
            Config_Effect standard = Effect(1, "Standard", new[] { 11 });
            Config_Effect mutation = Effect(2, "Mutation", new[] { 11 });
            mutation.RollPool = (int)WeaponEffectRollPool.Mutation;
            WeaponEffectCatalog catalog = BuildCatalog(
                new[] { standard, mutation },
                new[] { Modifier(11, "Damage", WeaponModifierStat.Damage,
                    WeaponModifierOperation.Add, 1f) });
            var loadout = new PlayerEffectLoadout(catalog);
            var authority = new EffectRollOfferAuthority(new EffectRollService(catalog), loadout);

            EffectRollOffer first = authority.Request(
                WeaponEffectRollPool.Standard, 1, 10, fallbackToChaos: false);
            EffectRollOffer queued = authority.Request(
                WeaponEffectRollPool.Mutation, 1, 20, fallbackToChaos: false);

            Assert.That(first, Is.Not.Null);
            Assert.That(queued, Is.Null);
            Assert.That(authority.PendingCount, Is.EqualTo(1));
            Assert.That(authority.TryCreateNext(out _), Is.False);

            Assert.That(authority.TryConsume(first.Id, first.Options[0].Effect.Id),
                Is.EqualTo(EffectRollSelectionResult.Success));
            loadout.TryAcquire(first.Options[0].Effect.Id);

            Assert.That(authority.TryCreateNext(out EffectRollOffer next), Is.True);
            Assert.That(next.Pool, Is.EqualTo(WeaponEffectRollPool.Mutation));
            Assert.That(next.Options[0].Effect.Id, Is.EqualTo(2));
            Assert.That(authority.PendingCount, Is.Zero);
        }

        [Test]
        public void RollPresentation_IsResolvedLocallyFromEffectRollTable()
        {
            WeaponEffectCatalog catalog = BuildCatalog(
                new[] { Effect(1, "LocalPresentation", new[] { 11 }) },
                new[] { Modifier(11, "Damage", WeaponModifierStat.Damage,
                    WeaponModifierOperation.Add, 1f) });

            IReadOnlyList<EffectRollOption> options = new EffectRollService(catalog)
                .ResolveOptions(new ushort[] { 1 }, new byte[] { 0 });

            Assert.That(options[0].Presentation.DisplayName, Is.EqualTo("Effect 1"));
            Assert.That(options[0].Presentation.IconAddress, Is.EqualTo("UI/Effects/1"));
            Assert.That(options[0].NextLevel, Is.EqualTo(1));
        }

        [Test]
        public void SpecialManager_PassesCurrentLevelOncePerProjectileHook()
        {
            Config_Effect photon = SpecialEffect(PhotonMomentumEffectSystem.Id, "PhotonMomentum");
            WeaponEffectCatalog catalog = BuildCatalog(new[] { photon }, Array.Empty<Config_Modifier>());
            var sets = new EffectSetRepository();
            var set = new EffectSet(1, 0,
                new[] { new EffectSnapshot(PhotonMomentumEffectSystem.Id, 2) },
                new[] { PhotonMomentumEffectSystem.Id, PhotonMomentumEffectSystem.Id });
            sets.Register(set);
            var manager = new WeaponSpecialEffectManager(catalog, sets);
            manager.Register(new PhotonMomentumEffectSystem(NoneWeaponSpecialEffectCommandSink.Instance));
            manager.ValidateRegistrations();
            var shot = new ShotContext { ShotId = 1, EffectSetId = 1 };
            var projectile = new ProjectileState { ProjectileId = 1, DamageMultiplier = 1f, SizeMultiplier = 1f };
            var context = new ProjectileHitContext(shot, BaseStats(), 2, Vector3.zero, Vector3.up);

            manager.DispatchHit(ref projectile, context);

            Assert.That(projectile.DamageMultiplier, Is.EqualTo(1.15f).Within(0.0001f));
            Assert.That(projectile.SizeMultiplier, Is.EqualTo(0.95f).Within(0.0001f));
        }

        [Test]
        public void StaticShield_SubmitsShieldCommandInsteadOfMutatingHealth()
        {
            Config_Effect shield = SpecialEffect(StaticShieldEffectSystem.Id, "StaticShield");
            WeaponEffectCatalog catalog = BuildCatalog(new[] { shield }, Array.Empty<Config_Modifier>());
            var sets = new EffectSetRepository();
            sets.Register(new EffectSet(1, 0,
                new[] { new EffectSnapshot(StaticShieldEffectSystem.Id, 3) },
                new[] { StaticShieldEffectSystem.Id }));
            var sink = new RecordingCommandSink();
            var manager = new WeaponSpecialEffectManager(catalog, sets);
            manager.Register(new StaticShieldEffectSystem(sink));
            var shot = new ShotContext { ShotId = 9, OwnerEntityId = 7, EffectSetId = 1 };
            var projectile = new ProjectileState { ProjectileId = 5 };
            var context = new ProjectileHitContext(shot, BaseStats(), 2, Vector3.zero, Vector3.up);

            manager.DispatchOwnerState(7, sets.Get(1), BaseStats());
            manager.DispatchHit(ref projectile, context);

            Assert.That(sink.ShieldCount, Is.EqualTo(1));
            Assert.That(sink.LastShield.OwnerEntityId, Is.EqualTo(7));
            Assert.That(sink.LastShield.Amount, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(sink.LastCapacity.CapacityContribution, Is.EqualTo(25f).Within(0.0001f));
        }

        [Test]
        public void MigratedSpecialSystems_SubmitLevelScaledCommands()
        {
            var sink = new RecordingCommandSink();
            WeaponStatSnapshot stats = BaseStats();
            stats.FireRate = 5f;
            var shot = new ShotContext { ShotId = 9, OwnerEntityId = 7 };
            var projectile = new ProjectileState
            {
                ProjectileId = 5,
                Velocity = Vector3.forward * 20f,
                DamageMultiplier = 1.5f,
                SizeMultiplier = 2f,
            };
            var hit = new ProjectileHitContext(shot, stats, 2, Vector3.one, Vector3.up);
            var equip = new WeaponEffectEquipContext(7, EffectSet.Empty, stats);
            var destroyed = new ProjectileDestroyedEffectContext(shot, stats);

            new OverloadEffectSystem(sink).OnProjectileHit(2, ref projectile, hit);
            new EnergySiphonEffectSystem(sink).OnEquipped(3, equip);
            new EnergySiphonEffectSystem(sink).OnProjectileHit(3, ref projectile, hit);
            new ExecutionerEffectSystem(sink).OnProjectileHit(3, ref projectile, hit);
            new KineticBoostEffectSystem(sink).OnProjectileHit(3, ref projectile, hit);
            new MultiSplitEffectSystem(sink).OnProjectileHit(4, ref projectile, hit);
            new NuclearFissionEffectSystem(sink).OnProjectileHit(3, ref projectile, hit);
            new ShockwaveEffectSystem(sink).OnProjectileDestroyed(3, ref projectile, destroyed);

            Assert.That(sink.LastLightning.Damage, Is.EqualTo(6f).Within(0.0001f));
            Assert.That(sink.LastLightning.JumpCount, Is.EqualTo(2));
            Assert.That(sink.LastLightning.BranchCount, Is.EqualTo(3));
            Assert.That(sink.LastLightning.ApplyInitialDamage, Is.True);
            Assert.That(sink.LastCapacity.CapacityContribution, Is.EqualTo(15f).Within(0.0001f));
            Assert.That(sink.LastShield.Amount, Is.EqualTo(0.9f).Within(0.0001f));
            Assert.That(sink.LastExecute.HealthThreshold, Is.EqualTo(0.3f).Within(0.0001f));
            Assert.That(sink.LastCrowdControl.Duration, Is.EqualTo(2.2f).Within(0.0001f));
            Assert.That(sink.LastSplit.SplitCount, Is.EqualTo(5));
            Assert.That(sink.LastSplit.DamageRatio, Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(sink.LastArea.Radius, Is.EqualTo(6f).Within(0.0001f));
            Assert.That(sink.LastArea.DamagePerTick, Is.EqualTo(4f).Within(0.0001f));
            Assert.That(sink.LastArea.SlowRatio, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(sink.LastArea.TrueDamage, Is.True);
            Assert.That(sink.LastRadial.Radius, Is.EqualTo(1.6f).Within(0.0001f));
            Assert.That(sink.LastRadial.Damage, Is.EqualTo(3f).Within(0.0001f));
            Assert.That(sink.LastRadial.Force, Is.EqualTo(45f).Within(0.0001f));
        }

        [Test]
        public void ShotRepository_RetainedSplitProjectilesKeepShotAlive()
        {
            var shots = new ShotRepository();
            var shot = new ShotContext { ShotId = shots.AllocateShotId() };
            shots.Register(shot);
            shots.RetainProjectiles(shot.ShotId, 2);

            shots.ReleaseProjectile(shot.ShotId);
            shots.ReleaseProjectile(shot.ShotId);
            Assert.That(shots.TryGet(shot.ShotId, out _), Is.True);

            shots.ReleaseProjectile(shot.ShotId);
            Assert.That(shots.TryGet(shot.ShotId, out _), Is.False);
        }

        [Test]
        public void HealthShieldAdapter_AggregatesNumericAndSpecialCapacity()
        {
            var definition = new HealthDefinition(1, 100f, 10f);
            using var health = new HealthRuntime(new HealthCatalog(new[] { definition }));
            HealthEntity entity = health.Register(7, 1);
            uint tick = 1;
            var adapter = new HealthShieldEffectAdapter(health, health, () => tick);

            adapter.Apply(7, new EffectOwnerStatSnapshot(20f));
            adapter.SetShieldCapacity(new ShieldCapacityEffectCommand(2003, 3, 7, 25f));
            adapter.SetShieldCapacity(new ShieldCapacityEffectCommand(2005, 3, 7, 15f));
            health.Step(tick);

            Assert.That(health.TryGetHealth(entity, out HealthSnapshot expanded), Is.True);
            Assert.That(expanded.Definition.MaxShield, Is.EqualTo(60f).Within(0.0001f));

            tick++;
            var projectile = new ProjectileState { ProjectileId = 5 };
            var shot = new ShotContext { ShotId = 9, OwnerEntityId = 7 };
            var hit = new ProjectileHitContext(shot, BaseStats(), 2, Vector3.zero, Vector3.up);
            adapter.AddShield(new ShieldEffectCommand(2005, 3, projectile, hit, 2f));
            health.Step(tick);
            Assert.That(health.TryGetHealth(entity, out HealthSnapshot shielded), Is.True);
            Assert.That(shielded.CurrentShield, Is.EqualTo(2f).Within(0.0001f));

            tick++;
            adapter.SetShieldCapacity(new ShieldCapacityEffectCommand(2003, 0, 7, 0f));
            adapter.SetShieldCapacity(new ShieldCapacityEffectCommand(2005, 0, 7, 0f));
            health.Step(tick);
            Assert.That(health.TryGetHealth(entity, out HealthSnapshot restored), Is.True);
            Assert.That(restored.Definition.MaxShield, Is.EqualTo(20f).Within(0.0001f));
        }

        [Test]
        public void WeaponSystem_UsesEffectiveStatsInsteadOfDefinitionBaseStats()
        {
            WeaponStatSnapshot baseStats = BaseStats();
            var definition = new WeaponDefinition(1, baseStats, 20, true);
            WeaponStatSnapshot effective = baseStats;
            effective.Id = 2;
            effective.FireRate = 10f;
            effective.FireIntervalTicks = 3;
            var state = WeaponSystem.Equip(definition, effective, 1, 2);

            Assert.That(WeaponSystem.Simulate(ref state, definition, effective,
                true, false, 1f / 30f), Is.True);
            Assert.That(state.FireCooldownTicks, Is.EqualTo(3));
            Assert.That(state.StatSnapshotId, Is.EqualTo(2));
            Assert.That(state.EffectSetId, Is.EqualTo(1));
        }

        [Test]
        public void ConfigValidation_RejectsMissingModifierForeignKey()
        {
            Config_Effect effect = Effect(1, "Broken", new[] { 999 });
            var effects = new Dictionary<int, Config_Effect> { [1] = effect };
            var rolls = new Dictionary<int, Config_EffectRoll> { [1] = Roll(1) };

            Assert.Throws<InvalidOperationException>(() =>
                ProjectGame.HotFix.Config.EffectConfigRules.ValidateAll(
                    effects, new Dictionary<int, Config_Modifier>(), rolls));
        }

        private static bool Contains(IReadOnlyList<EffectRollOption> options, ushort id)
        {
            for (int i = 0; i < options.Count; i++)
                if (options[i].Effect.Id == id) return true;
            return false;
        }

        private static WeaponEffectCatalog BuildCatalog(
            IReadOnlyList<Config_Effect> effectRows,
            IReadOnlyList<Config_Modifier> modifierRows)
        {
            var effects = new Dictionary<int, Config_Effect>();
            var modifiers = new Dictionary<int, Config_Modifier>();
            var rolls = new Dictionary<int, Config_EffectRoll>();
            for (int i = 0; i < effectRows.Count; i++)
            {
                effects.Add(effectRows[i].EffectID, effectRows[i]);
                rolls.Add(effectRows[i].EffectID, Roll(effectRows[i].EffectID));
            }
            for (int i = 0; i < modifierRows.Count; i++)
                modifiers.Add(modifierRows[i].ModifierID, modifierRows[i]);
            return new WeaponEffectCatalog(effects, modifiers, rolls);
        }

        private static Config_Effect Effect(int id, string name, int[] modifierIds) => new()
        {
            EffectID = id,
            CodeName = name,
            ModifierIDs = modifierIds,
            Repeatable = false,
            MaxLevel = 1,
            EffectType = (int)WeaponEffectType.Numeric,
            RollPool = (int)WeaponEffectRollPool.Standard,
            BaseWeight = 100f,
            SchoolWeightBonus = 50f,
            Enabled = true,
        };

        private static Config_Effect SpecialEffect(int id, string name) => new()
        {
            EffectID = id,
            CodeName = name,
            ModifierIDs = Array.Empty<int>(),
            Repeatable = true,
            MaxLevel = 5,
            EffectType = (int)WeaponEffectType.Special,
            RollPool = (int)WeaponEffectRollPool.Mutation,
            BaseWeight = 100f,
            SchoolWeightBonus = 50f,
            Enabled = true,
        };

        private static Config_Modifier Modifier(int id, string name, WeaponModifierStat stat,
            WeaponModifierOperation operation, float value) => new()
        {
            ModifierID = id,
            CodeName = name,
            StatType = (int)stat,
            Operation = (int)operation,
            BaseValue = value,
            Enabled = true,
        };

        private static Config_EffectRoll Roll(int id) => new()
        {
            EffectID = id,
            DisplayName = $"Effect {id}",
            Description = "Description",
            UpgradeDescription = "Upgrade",
            IconAddress = $"UI/Effects/{id}",
            Enabled = true,
        };

        private static WeaponStatSnapshot BaseStats() => new()
        {
            Id = 1,
            Damage = 10f,
            FireRate = 5f,
            ReloadTime = 1f,
            FireIntervalTicks = 6,
            ReloadTicks = 30,
            MagSize = 10,
            CritChance = 0.1f,
            CritMultiplier = 2f,
            ProjectileSpeed = 20f,
            ProjectileCount = 1,
            SpreadAngle = 0f,
            BounceCount = 0,
            PierceCount = 0,
            ProjectileSize = 0.1f,
            ProjectileLifeTime = 2f,
        };

        private sealed class RecordingCommandSink : IWeaponSpecialEffectCommandSink
        {
            public int ShieldCount;
            public ShieldEffectCommand LastShield;
            public ShieldCapacityEffectCommand LastCapacity;
            public LightningEffectCommand LastLightning;
            public ExecuteEffectCommand LastExecute;
            public CrowdControlEffectCommand LastCrowdControl;
            public SplitProjectileEffectCommand LastSplit;
            public RadialImpactEffectCommand LastRadial;
            public PersistentAreaEffectCommand LastArea;
            public void EmitLightning(in LightningEffectCommand command) => LastLightning = command;
            public void SpawnStormCloud(in StormCloudEffectCommand command) { }
            public void RemoveOwnerEffect(in OwnerEffectRemovalCommand command) { }
            public void AddShield(in ShieldEffectCommand command)
            {
                ShieldCount++;
                LastShield = command;
            }
            public void SetShieldCapacity(in ShieldCapacityEffectCommand command) => LastCapacity = command;
            public void TryExecute(in ExecuteEffectCommand command) => LastExecute = command;
            public void ApplyCrowdControl(in CrowdControlEffectCommand command) => LastCrowdControl = command;
            public void SpawnProjectiles(in SplitProjectileEffectCommand command) => LastSplit = command;
            public void EmitRadialImpact(in RadialImpactEffectCommand command) => LastRadial = command;
            public void UpsertDamageArea(in PersistentAreaEffectCommand command) => LastArea = command;
        }
    }
}
