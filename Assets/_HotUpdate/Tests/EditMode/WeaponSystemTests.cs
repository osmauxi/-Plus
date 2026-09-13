using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Gameplay.Player.State;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using NUnit.Framework;
using ProjectGame.HotFix.Gameplay.Weapon;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

namespace ProjectGame.HotFix.Gameplay.Tests
{
    public sealed class WeaponSystemTests
    {
        [Test]
        public void WeaponAmmoChange_IsIncludedInDelta_AndNetworkRoundtrip()
        {
            var baseline = new PlayerSimulationState { Tick = 1 };
            baseline.ActionState.Weapon = WeaponSystem.Equip(new WeaponDefinition(0, CreateStats(), 20, true));
            var changed = baseline;
            changed.Tick = 2;
            changed.ActionState.Weapon.CurrentAmmo--;
            var packet = PlayerSnapshotPacket.CreateDelta(changed, baseline);
            Assert.That((packet.DirtyMask & PlayerStateDirtyMask.ActionState) != 0, Is.True);
            using var writer = new FastBufferWriter(128, Allocator.Temp, 512);
            writer.WriteNetworkSerializable(packet);
            using var reader = new FastBufferReader(writer, Allocator.Temp);
            reader.ReadNetworkSerializable(out PlayerSnapshotPacket received);
            Assert.That(received.TryResolve(baseline, out var resolved), Is.True);
            Assert.That(resolved.ActionState.Weapon.Equals(changed.ActionState.Weapon), Is.True);
        }

        private sealed class TestTarget : IProjectileHitTarget
        {
            public ulong EntityId { get; set; }
            public bool IsAlive => true;
            public int Hits;
            public void ApplyProjectileDamage(in ProjectileDamageContext context) => Hits++;
        }

        [Test]
        public void Projectile_IgnoresOwner_AndOnlyDamagesCompoundTargetOnce()
        {
            var owner = new GameObject("WeaponTestOwner");
            var target = new GameObject("WeaponTestTarget");
            try
            {
                const int layer = 9;
                owner.layer = target.layer = layer;
                owner.AddComponent<BoxCollider>().size = Vector3.one;
                owner.AddComponent<ProjectileHitTargetAdapter>().BindIdentity(42);
                target.transform.position = new Vector3(0, 0, 2);
                target.AddComponent<BoxCollider>().size = Vector3.one;
                target.AddComponent<SphereCollider>().radius = 0.6f;
                var receiver = new TestTarget { EntityId = 88 };
                target.AddComponent<ProjectileHitTargetAdapter>().Bind(receiver);
                Physics.SyncTransforms();
                var stats = CreateStats();
                stats.ProjectileCount = 1; stats.SpreadAngle = 0; stats.PierceCount = 3;
                var shots = new ShotRepository(); var world = new ProjectileWorld();
                var repository = new WeaponStatSnapshotRepository(); repository.Register(stats);
                new ShotBuilder(shots, world).Build(42, 0, 1, 1, Vector3.zero, Vector3.forward, stats, 0);
                var simulation = new ProjectileSimulation(world, shots, repository,
                    new ProjectileSimulationConfig { TargetMask = 1 << layer });
                simulation.Tick(0.2f);
                simulation.Tick(0.2f);
                Assert.That(receiver.Hits, Is.EqualTo(1));
            }
            finally { UnityEngine.Object.DestroyImmediate(owner); UnityEngine.Object.DestroyImmediate(target); }
        }

        [Test]
        public void WorldHit_StopsProjectile_AndInitialOverlapIsNotMissed()
        {
            var wall = new GameObject("WeaponTestWall");
            try
            {
                wall.layer = 7;
                wall.AddComponent<BoxCollider>().size = Vector3.one;
                Physics.SyncTransforms();
                var stats = CreateStats(); stats.ProjectileCount = 1;
                var shots = new ShotRepository(); var world = new ProjectileWorld();
                var repository = new WeaponStatSnapshotRepository(); repository.Register(stats);
                new ShotBuilder(shots, world).Build(42, 0, 1, 1, Vector3.zero, Vector3.forward, stats, 0);
                var simulation = new ProjectileSimulation(world, shots, repository,
                    new ProjectileSimulationConfig { WorldMask = 1 << 7 });
                int impacts = 0;
                simulation.Impact += impact =>
                {
                    impacts++;
                    Assert.That(impact.Resolution, Is.EqualTo(ProjectileHitResolution.Destroy));
                };
                simulation.Tick(0.1f);
                Assert.That(impacts, Is.EqualTo(1));
                Assert.That(world.Count, Is.Zero);
                Assert.That(shots.Count, Is.Zero);
            }
            finally { UnityEngine.Object.DestroyImmediate(wall); }
        }

        [Test]
        public void TargetImpact_IsPublishedAfterDamageAndContainsResolvedProjectileState()
        {
            var target = new GameObject("WeaponImpactOrderTarget");
            try
            {
                const int layer = 10;
                target.layer = layer;
                target.transform.position = new Vector3(0f, 0f, 1f);
                target.AddComponent<BoxCollider>().size = Vector3.one;
                var receiver = new TestTarget { EntityId = 99 };
                target.AddComponent<ProjectileHitTargetAdapter>().Bind(receiver);
                Physics.SyncTransforms();

                WeaponStatSnapshot stats = CreateStats();
                stats.ProjectileCount = 1;
                stats.SpreadAngle = 0f;
                stats.PierceCount = 0;
                var shots = new ShotRepository();
                var world = new ProjectileWorld();
                var repository = new WeaponStatSnapshotRepository();
                repository.Register(stats);
                new ShotBuilder(shots, world).Build(
                    42, 0, 1, 1, Vector3.zero, Vector3.forward, stats, 0);

                var simulation = new ProjectileSimulation(world, shots, repository,
                    new ProjectileSimulationConfig { TargetMask = 1 << layer });
                bool impactPublished = false;
                simulation.Impact += impact =>
                {
                    impactPublished = true;
                    Assert.That(receiver.Hits, Is.EqualTo(1), "Impact 必须在伤害提交后发布");
                    Assert.That(impact.Projectile.HitCount, Is.EqualTo(1));
                    Assert.That(impact.Resolution, Is.EqualTo(ProjectileHitResolution.Destroy));
                };

                simulation.Tick(0.1f);

                Assert.That(impactPublished, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void AutomaticFire_UsesAmmoAndFixedTickCooldown()
        {
            var definition = new WeaponDefinition(0, CreateStats(), 20, true);
            var state = WeaponSystem.Equip(definition);
            int shots = 0;
            for (int i = 0; i < 30; i++) if (WeaponSystem.Simulate(ref state, definition, true, false, 1f / 30)) shots++;
            Assert.That(shots, Is.EqualTo(5));
            Assert.That(state.CurrentAmmo, Is.EqualTo(7));
            Assert.That(state.ShotSequence, Is.EqualTo(5));
        }

        [Test]
        public void Reload_TransfersOnlyAvailableReserve_AndBlocksFire()
        {
            var definition = new WeaponDefinition(0, CreateStats(), 3, true);
            var state = WeaponSystem.Equip(definition);
            state.CurrentAmmo = 1;
            Assert.That(WeaponSystem.Simulate(ref state, definition, true, true, 0.1f), Is.False);
            for (int i = 0; i < 9; i++)
            {
                Assert.That(WeaponSystem.Simulate(ref state, definition, true, false, 0.1f), Is.False);
                Assert.That(state.CurrentAmmo, Is.EqualTo(1));
            }
            Assert.That(WeaponSystem.Simulate(ref state, definition, true, false, 0.1f), Is.False);
            Assert.That(state.CurrentAmmo, Is.EqualTo(4));
            Assert.That(state.ReserveAmmo, Is.Zero);
            Assert.That(state.IsReloading, Is.False);
        }

        [Test]
        public void EmptyMagazine_AutoReloads_EmptyReserveCannotFireOrReload()
        {
            var definition = new WeaponDefinition(0, CreateStats(), 3, true);
            var state = WeaponSystem.Equip(definition);
            state.CurrentAmmo = 0;
            WeaponSystem.Simulate(ref state, definition, true, false, 0.1f);
            Assert.That(state.IsReloading, Is.True);
            WeaponSystem.Interrupt(ref state);
            state.ReserveAmmo = 0;
            Assert.That(WeaponSystem.Simulate(ref state, definition, true, true, 0.1f), Is.False);
            Assert.That(state.IsReloading, Is.False);
            Assert.That(state.ShotSequence, Is.Zero);
        }

        [Test]
        public void FullMagazine_DoesNotReload_InterruptDoesNotCreateAmmo()
        {
            var definition = new WeaponDefinition(0, CreateStats(), 20, true);
            var state = WeaponSystem.Equip(definition);
            WeaponSystem.Simulate(ref state, definition, false, true, 0.1f);
            Assert.That(state.IsReloading, Is.False);
            state.CurrentAmmo = 2;
            WeaponSystem.Simulate(ref state, definition, false, true, 0.1f);
            WeaponSystem.Interrupt(ref state);
            Assert.That(state.CurrentAmmo, Is.EqualTo(2));
            Assert.That(state.ReserveAmmo, Is.EqualTo(20));
            Assert.That(state.ReloadTicksRemaining, Is.Zero);
        }

        [Test]
        public void RestoreReplay_ProducesIdenticalAmmoTimersAndSequence()
        {
            var definition = new WeaponDefinition(0, CreateStats(), 20, true);
            var saved = WeaponSystem.Equip(definition);
            saved.ShotSequence = uint.MaxValue;
            var first = saved;
            var replay = saved;
            for (int i = 0; i < 120; i++)
            {
                WeaponSystem.Simulate(ref first, definition, true, i == 25, 1f / 30);
                WeaponSystem.Simulate(ref replay, definition, true, i == 25, 1f / 30);
                Assert.That(replay.Equals(first), Is.True);
            }
        }

        [Test]
        public void PlayerStateMachine_ProjectsWeaponSequence_DeathCancelsReload()
        {
            var definition = new WeaponDefinition(0, CreateStats(), 20, true);
            var machine = new PlayerStateMachine(new PlayerActionConfig()) { WeaponDefinition = definition };
            var control = PlayerControlState.CreateDefault();
            var action = new PlayerActionRuntimeState { Weapon = WeaponSystem.Equip(definition) };
            machine.Simulate(ref control, ref action, new PlayerStateInput(false, true, false, true, 0), true, 1f / 30);
            Assert.That(action.ShotSequence, Is.EqualTo(action.Weapon.ShotSequence));
            Assert.That(action.Weapon.CurrentAmmo, Is.EqualTo(11));
            machine.Simulate(ref control, ref action, new PlayerStateInput(false, false, false, false, 1), true, 1f / 30);
            machine.SetLifeState(ref control, ref action, PlayerLifeState.Dead);
            Assert.That(action.Weapon.IsReloading, Is.False);
            Assert.That(action.Weapon.CurrentAmmo, Is.EqualTo(11));
        }

        [Test]
        public void PlayerStateMachine_FireRequiresAim()
        {
            var definition = new WeaponDefinition(0, CreateStats(), 20, true);
            var machine = new PlayerStateMachine(new PlayerActionConfig()) { WeaponDefinition = definition };
            var control = PlayerControlState.CreateDefault();
            var action = new PlayerActionRuntimeState { Weapon = WeaponSystem.Equip(definition) };

            machine.Simulate(ref control, ref action,
                new PlayerStateInput(false, false, false, true), true, 1f / 30f);

            Assert.That(action.Weapon.ShotSequence, Is.Zero);
            Assert.That(action.Weapon.CurrentAmmo, Is.EqualTo(12));
            Assert.That(control.CombatMode, Is.EqualTo(PlayerCombatMode.Ready));

            machine.Simulate(ref control, ref action,
                new PlayerStateInput(false, true, false, true), true, 1f / 30f);

            Assert.That(action.Weapon.ShotSequence, Is.EqualTo(1));
            Assert.That(action.Weapon.CurrentAmmo, Is.EqualTo(11));
            Assert.That(control.CombatMode, Is.EqualTo(PlayerCombatMode.Firing));
        }

        [Test]
        public void Catalog_FreezesConfig_RejectsDisabledAndInvalidEntries()
        {
            //var row = new Config_Weapon { WeaponID = 0, Enabled = true, Damage = 20, FireRate = 10,
            //    ReloadTime = 2, MagSize = 30, ReserveAmmo = 120, AutoReload = true, CritMultiplier = 2,
            //    ProjectileSpeed = 60, ProjectileLifeTime = 2, ProjectileSize = 0.1f, ProjectileCount = 1 };
            //var rows = new Dictionary<int, Config_Weapon> { [0] = row, [2] = new Config_Weapon { WeaponID = 2 } };
            //var catalog = new WeaponCatalog(rows, new WeaponStatSnapshotRepository());
            //row.Damage = 999;
            //Assert.That(catalog.Get(0).Stats.Damage, Is.EqualTo(20));
            //Assert.Throws<InvalidOperationException>(() => catalog.Get(2))
            //row.ProjectileCount = 0;
            //Assert.Throws<ArgumentException>(() => WeaponConfigRules.Validate(row));
        }

        [Test]
        public void Projectiles_ReleaseShotAfterLastPelletExpires()
        {
            var shots = new ShotRepository();
            var world = new ProjectileWorld();
            var stats = CreateStats();
            var repository = new WeaponStatSnapshotRepository();
            repository.Register(stats);
            new ShotBuilder(shots, world).Build(ulong.MaxValue - 1, 0, 1, 1, Vector3.zero, Vector3.forward, stats, 0);
            var simulation = new ProjectileSimulation(world, shots, repository, new ProjectileSimulationConfig());
            simulation.Tick(stats.ProjectileLifeTime);
            Assert.That(world.Count, Is.Zero);
            Assert.That(shots.Count, Is.Zero);
        }

        [Test]
        public void ShotIds_DoNotTruncateNetworkObjectId_OrCollideAfterReequip()
        {
            var shots = new ShotRepository();
            var builder = new ShotBuilder(shots, new ProjectileWorld());
            var stats = CreateStats();
            var first = builder.Build(1, 0, 1, 1, Vector3.zero, Vector3.forward, stats, 0);
            var second = builder.Build(1ul << 32, 0, 1, 1, Vector3.zero, Vector3.forward, stats, 0);
            var third = builder.Build(1, 0, 1, 1, Vector3.zero, Vector3.forward, stats, 0);
            Assert.That(second.OwnerEntityId, Is.EqualTo(1ul << 32));
            Assert.That(first.ShotId, Is.Not.EqualTo(second.ShotId));
            Assert.That(third.ShotId, Is.Not.EqualTo(first.ShotId));
        }

        [Test]
        public void StatSnapshots_CannotBeOverwrittenWhileProjectilesReferenceThem()
        {
            var repository = new WeaponStatSnapshotRepository();
            var stats = CreateStats();
            repository.Register(stats);
            stats.Damage++;
            Assert.Throws<ArgumentException>(() => repository.Register(stats));
        }

        [Test]
        public void StatRepository_RejectsInvalidSimulationValues()
        {
            var repository = new WeaponStatSnapshotRepository();
            WeaponStatSnapshot snapshot = CreateStats();
            snapshot.FireRate = float.NaN;

            Assert.Throws<ArgumentException>(() => repository.Register(snapshot));
        }

        [Test]
        public void EffectRepository_AlwaysContainsTheEmptyEffectSet()
        {
            var repository = new EffectSetRepository();

            Assert.That(repository.Get(0), Is.SameAs(EffectSet.Empty));

            repository.Clear();
            Assert.That(repository.Get(0), Is.SameAs(EffectSet.Empty));
        }

        [Test]
        public void ShotBuilder_SameInputsProduceTheSameProjectiles()
        {
            ProjectileWorld firstWorld = BuildShot(out ShotContext firstShot);
            ProjectileWorld secondWorld = BuildShot(out ShotContext secondShot);

            Assert.That(secondShot.ShotId, Is.EqualTo(firstShot.ShotId));
            Assert.That(secondShot.RandomSeed, Is.EqualTo(firstShot.RandomSeed));
            Assert.That(secondWorld.Count, Is.EqualTo(firstWorld.Count));

            for (int i = 0; i < firstWorld.Count; i++)
            {
                ProjectileState first = firstWorld.Get(i);
                ProjectileState second = secondWorld.Get(i);
                Assert.That(second.Velocity, Is.EqualTo(first.Velocity));
                Assert.That(second.Flags, Is.EqualTo(first.Flags));
            }
        }

        private static ProjectileWorld BuildShot(out ShotContext shot)
        {
            var shots = new ShotRepository();
            var projectiles = new ProjectileWorld();
            var builder = new ShotBuilder(shots, projectiles);
            WeaponStatSnapshot stats = CreateStats();

            shot = builder.Build(
                42,
                7,
                100,
                3,
                new Vector3(1f, 2f, 3f),
                Vector3.forward,
                stats,
                0);
            return projectiles;
        }

        private static WeaponStatSnapshot CreateStats()
        {
            return new WeaponStatSnapshot
            {
                Id = 1,
                Damage = 10f,
                FireRate = 5f,
                ReloadTime = 1f,
                MagSize = 12,
                CritChance = 0.25f,
                CritMultiplier = 2f,
                ProjectileSpeed = 20f,
                ProjectileCount = 3,
                SpreadAngle = 8f,
                ProjectileSize = 0.1f,
                ProjectileLifeTime = 2f,
            };
        }
    }
}
