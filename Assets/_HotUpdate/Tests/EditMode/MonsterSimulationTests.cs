using System.Collections.Generic;
using NUnit.Framework;
using ProjectGame.HotFix.Gameplay.Combat.Health;
using ProjectGame.HotFix.Gameplay.Monsters;
using ProjectGame.HotFix.Gameplay.Weapon;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Tests
{
    public sealed class MonsterSimulationTests
    {
        private static MonsterRuntimeConfig Config(int id = 0, float speed = 2, float range = 1,
            uint windup = 2, uint recovery = 2, int healthProfile = 0) =>
            new MonsterRuntimeConfig(id, speed, MonsterTargetModule.NearestTarget,
                MonsterMoveModule.DirectChase, MonsterAttackModule.MeleeAttack,
                range, windup, recovery, 10, healthProfile, 0);

        private static MonsterAttackCatalog AttackCatalog() => new MonsterAttackCatalog(new[]
        {
            new MonsterAttackProfile(new Vector3(0, 1, 0.5f), 0.5f, 1, (LayerMask)1),
        });

        [Test]
        public void World_UsesStableNonRecycledSlots_AndClearStartsAtZero()
        {
            var world = new MonsterWorld(0);
            int first = world.Create(3, new Vector2(1, 2), 45);
            int second = world.Create(4, new Vector2(3, 4), 90);
            Assert.That(first, Is.Zero);
            Assert.That(second, Is.EqualTo(1));
            Assert.That(world.Kill(first), Is.True);
            Assert.That(world.Kill(first), Is.False);
            Assert.That(world.Create(5, Vector2.zero, 0), Is.EqualTo(2));
            Assert.That(world.SlotCount, Is.EqualTo(3));
            Assert.That(world.AliveCount, Is.EqualTo(2));

            world.Target[0] = new MonsterTargetData { HasTarget = true, ClientId = 99 };
            world.Move[0] = new MonsterMoveData { DesiredVelocity = Vector2.one };
            world.Attack[0] = new MonsterAttackData { Phase = MonsterAttackPhase.Recovery, PhaseEndTick = 99 };
            world.Presentation[0] = new MonsterPresentationData { AttackSequence = 7 };
            world.Clear();
            int reused = world.Create(6, new Vector2(8, 9), 180);
            Assert.That(reused, Is.Zero);
            Assert.That(world.Target[0].HasTarget, Is.False);
            Assert.That(world.Move[0].DesiredVelocity, Is.EqualTo(Vector2.zero));
            Assert.That(world.Attack[0].Phase, Is.EqualTo(MonsterAttackPhase.Ready));
            Assert.That(world.Presentation[0].AttackSequence, Is.Zero);
        }

        [Test]
        public void Simulation_RunsTargetAttackMoveMotorInBatches()
        {
            var configs = new MonsterRuntimeCatalog(new[] { Config() });
            var attacks = AttackCatalog();
            var resolver = new RecordingAttackResolver();
            var simulation = new MonsterSimulation(configs, attacks, resolver);
            var world = new MonsterWorld();
            int slot = world.Create(0, Vector2.zero, 0);
            var players = new MonsterPlayerTargetBuffer();
            players.Add(20, new Vector2(5, 0));
            players.Add(10, new Vector2(3, 0));

            simulation.Tick(world, players, 1, 0.5f);
            Assert.That(world.Target[slot].ClientId, Is.EqualTo(10));
            Assert.That(world.Motion[slot].Position, Is.EqualTo(new Vector2(1, 0)));
            Assert.That(world.Motion[slot].Yaw, Is.EqualTo(90).Within(0.001f));

            players.Clear();
            players.Add(10, new Vector2(1.5f, 0));
            simulation.Tick(world, players, 2, 0.5f);
            Assert.That(world.Attack[slot].Phase, Is.EqualTo(MonsterAttackPhase.Windup));
            Assert.That(world.Presentation[slot].AttackSequence, Is.EqualTo(1));
            Assert.That(world.Motion[slot].Position, Is.EqualTo(new Vector2(1, 0)),
                "Ready→Windup 的同 Tick 必须立即停止移动。");

            simulation.Tick(world, players, 3, 0.5f);
            Assert.That(resolver.Requests, Is.Empty);
            simulation.Tick(world, players, 4, 0.5f, groundY: 3f);
            Assert.That(resolver.Requests.Count, Is.EqualTo(1));
            Assert.That(resolver.Requests[0].Slot, Is.EqualTo(slot));
            Assert.That(resolver.Requests[0].TargetClientId, Is.EqualTo(10));
            Assert.That(resolver.Requests[0].Origin.y, Is.EqualTo(4f).Within(0.001f));
            Assert.That(world.Attack[slot].Phase, Is.EqualTo(MonsterAttackPhase.Recovery));

            simulation.Tick(world, players, 5, 0.5f);
            Assert.That(world.Motion[slot].Velocity, Is.EqualTo(Vector2.zero));
            simulation.Tick(world, players, 6, 0.5f);
            Assert.That(world.Attack[slot].Phase, Is.EqualTo(MonsterAttackPhase.Ready));
            Assert.That(world.Motion[slot].Velocity.sqrMagnitude, Is.GreaterThan(0),
                "Recovery→Ready 的同 Tick 应恢复移动。");
        }

        [Test]
        public void HealthDeath_KillsWorldSlot_AndOnlyProjectileRequestsBloodReaction()
        {
            var world = new MonsterWorld();
            int slot = world.Create(0, Vector2.zero, 0);
            using var health = new HealthRuntime(new HealthCatalog(new[] { new HealthDefinition(0, 50) }));
            using var binding = new MonsterHealthBinding(world, health);
            HealthEntity entity = binding.Register(slot, 0);
            MonsterDeathReaction reaction = default;
            int reactions = 0;
            binding.DeathReactionRequested += value => { reaction = value; reactions++; };

            health.TryEnqueue(HealthCommand.Damage(1, 1, default, entity, 100,
                impact: new HealthImpact(new Vector3(1, 2, 3), Vector3.forward,
                    origin: DamageOriginType.Projectile)));
            health.Step(1);

            Assert.That(world.IsActive(slot), Is.False);
            Assert.That(world.AliveCount, Is.Zero);
            Assert.That(reactions, Is.EqualTo(1));
            Assert.That(reaction.HitPoint, Is.EqualTo(new Vector3(1, 2, 3)));
            Assert.That(reaction.Direction, Is.EqualTo(Vector3.back));
            Assert.That(binding.TryGetSlot(entity, out int resolved), Is.True);
            Assert.That(resolved, Is.EqualTo(slot));
        }

        [Test]
        public void SpawnSelector_RespectsBudgetDifficultyAndPerWaveLimit()
        {
            var runtime = new MonsterRuntimeCatalog(new[] { Config(10), Config(11) });
            var catalog = new MonsterSpawnCatalog(new[]
            {
                new MonsterSpawnConfig(0, 3, 1, maxPerWave: 2),
                new MonsterSpawnConfig(1, 5, 1, minDifficulty: 2),
            }, runtime);
            var selected = new List<ushort>();
            int remaining = new MonsterSpawnSelector(catalog, seed: 1).Select(10, 0, 20, selected);
            Assert.That(selected, Is.EqualTo(new ushort[] { 0, 0 }));
            Assert.That(remaining, Is.EqualTo(4));
        }

        [Test]
        public void RoomRuntime_ComposesSpawnTickFrameAndRoomCleanup()
        {
            var configs = new MonsterRuntimeCatalog(new[] { Config() });
            var quantizer = new MonsterPoseQuantizer(Vector2.zero, Vector2.one * 20);
            using var health = new HealthRuntime(new HealthCatalog(new[] { new HealthDefinition(0, 100) }));
            using var room = new MonsterRoomRuntime(configs, AttackCatalog(), quantizer, health);
            room.BeginRoom(expectedTotalMonsterCount: 100);
            MonsterSpawnBatch batch = room.Spawn(new[]
            {
                new MonsterSpawnPlan(0, new Vector2(1, 1), 0),
                new MonsterSpawnPlan(0, new Vector2(2, 2), 90),
            });
            Assert.That(batch.StartSlot, Is.Zero);
            Assert.That(room.World.AliveCount, Is.EqualTo(2));
            Assert.That(health.Count, Is.EqualTo(2));

            room.Tick(System.Array.Empty<MonsterPlayerTarget>(), 1, 1f / 30f);
            MonsterPresentationFrame frame = room.CaptureFrame(1);
            Assert.That(frame.ActiveStates.Count, Is.EqualTo(2));
            room.EndRoom();
            Assert.That(room.World.SlotCount, Is.Zero);
            Assert.That(health.Count, Is.Zero);

            room.BeginRoom();
            Assert.That(room.Spawn(new[] { new MonsterSpawnPlan(0, Vector2.zero, 0) }).StartSlot, Is.Zero);
        }

        [Test]
        public void ProjectileResolver_UsesCentralColliderBinding_AndPreservesDeathOrigin()
        {
            var world = new MonsterWorld();
            int slot = world.Create(0, Vector2.zero, 0);
            using var health = new HealthRuntime(new HealthCatalog(new[] { new HealthDefinition(0, 20) }));
            using var monsters = new MonsterHealthBinding(world, health);
            HealthEntity entity = monsters.Register(slot, 0);
            using var colliders = new HealthEntityBindings<Collider>(health);
            var gameObject = new GameObject("MonsterColliderTest");
            try
            {
                Collider collider = gameObject.AddComponent<CapsuleCollider>();
                colliders.Bind(entity, new[] { collider });
                var resolver = new MonsterProjectileHealthResolver(colliders, health, () => 1);
                Assert.That(resolver.TryResolve(collider, out ProjectileResolvedTarget target), Is.True);
                resolver.ApplyDamage(target, new ProjectileDamageContext(99, 100, 1, 50,
                    new Vector3(1, 0, 2), Vector3.right, false));
                health.Step(1);
                Assert.That(world.IsActive(slot), Is.False);
                Assert.That(health.TryGetHealth(entity, out HealthSnapshot state), Is.True);
                Assert.That(state.IsAlive, Is.False);
            }
            finally { Object.DestroyImmediate(gameObject); }
        }

        private sealed class RecordingAttackResolver : IMonsterAttackResolver
        {
            internal readonly List<MonsterAttackRequest> Requests = new List<MonsterAttackRequest>();
            public void Execute(in MonsterAttackRequest request) => Requests.Add(request);
        }
    }
}
