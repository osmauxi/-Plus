using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectGame.HotFix.Gameplay.Monsters;
using UnityEngine;

namespace ProjectGame.HotFix.Tests.EditMode
{
    public sealed class MonsterConfigAssemblerTests
    {
        [Test]
        public void Build_SortsIdsAndConvertsSecondsToCeilingTicks()
        {
            var health = new Dictionary<int, Config_Health>
            {
                [20] = Health(20, 200, 0.051f),
                [10] = Health(10, 100, 0),
            };
            var attacks = new Dictionary<int, Config_MonsterAttack>
            {
                [200] = Attack(200, 3),
                [100] = Attack(100, 2),
            };
            var runtime = new Dictionary<int, Config_MonsterRuntime>
            {
                [20] = Runtime(20, 20, 200, 0.11f, 0.2f),
                [10] = Runtime(10, 10, 100, 0.05f, 0.1f),
            };
            var spawns = new Dictionary<int, Config_MonsterSpawn>
            {
                [20] = Spawn(20),
                [10] = Spawn(10),
            };
            var views = new Dictionary<int, Config_MonsterView>
            {
                [20] = View(20),
                [10] = View(10),
            };

            MonsterCatalogBundle bundle = MonsterConfigAssembler.Build(
                health, attacks, runtime, spawns, views, 20);

            Assert.That(bundle.Runtime.TryGetIndex(10, out ushort firstIndex), Is.True);
            Assert.That(firstIndex, Is.EqualTo(0));
            Assert.That(bundle.Runtime.TryGetIndex(20, out ushort secondIndex), Is.True);
            Assert.That(secondIndex, Is.EqualTo(1));
            ref readonly MonsterRuntimeConfig second = ref bundle.Runtime.Get(secondIndex);
            Assert.That(second.AttackProfileIndex, Is.EqualTo(1));
            Assert.That(second.WindupTicks, Is.EqualTo(3));
            Assert.That(second.RecoveryTicks, Is.EqualTo(4));
            Assert.That(bundle.Health.GetDefinition(20).DamageGateTicks, Is.EqualTo(2));
            Assert.That(bundle.Spawns.Get(0).ConfigIndex, Is.EqualTo(0));
            Assert.That(bundle.Views.Get(1).LocalPoolId, Is.EqualTo("pool-20"));
        }

        [Test]
        public void Build_RequiresSpawnAndViewForEveryEnabledRuntime()
        {
            var health = new Dictionary<int, Config_Health> { [10] = Health(10, 100, 0) };
            var attacks = new Dictionary<int, Config_MonsterAttack> { [100] = Attack(100, 2) };
            var runtime = new Dictionary<int, Config_MonsterRuntime> { [10] = Runtime(10, 10, 100) };
            var spawns = new Dictionary<int, Config_MonsterSpawn> { [10] = Spawn(10) };

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                MonsterConfigAssembler.Build(health, attacks, runtime, spawns,
                    new Dictionary<int, Config_MonsterView>(), 30));

            StringAssert.Contains("缺少 MonsterView", exception.Message);
        }

        [Test]
        public void Build_SkipsDisabledRuntimeAndItsDependentRows()
        {
            var health = new Dictionary<int, Config_Health>
            {
                [10] = Health(10, 100, 0),
                [20] = Health(20, 100, 0),
            };
            var attacks = new Dictionary<int, Config_MonsterAttack>
            {
                [100] = Attack(100, 2),
                [200] = Attack(200, 2),
            };
            var disabled = Runtime(10, 10, 100);
            disabled.Enabled = false;
            var runtime = new Dictionary<int, Config_MonsterRuntime>
            {
                [10] = disabled,
                [20] = Runtime(20, 20, 200),
            };
            var spawns = new Dictionary<int, Config_MonsterSpawn>
            {
                [10] = Spawn(10),
                [20] = Spawn(20),
            };
            var views = new Dictionary<int, Config_MonsterView>
            {
                [10] = View(10),
                [20] = View(20),
            };

            MonsterCatalogBundle bundle = MonsterConfigAssembler.Build(
                health, attacks, runtime, spawns, views, 30);

            Assert.That(bundle.Runtime.Count, Is.EqualTo(1));
            Assert.That(bundle.Spawns.Count, Is.EqualTo(1));
            Assert.That(bundle.Runtime.TryGetIndex(10, out _), Is.False);
            Assert.That(bundle.Runtime.TryGetIndex(20, out ushort index), Is.True);
            Assert.That(bundle.Views.Get(index).LocalPoolId, Is.EqualTo("pool-20"));
        }

        [Test]
        public void Build_RejectsDictionaryKeyMismatch()
        {
            var health = new Dictionary<int, Config_Health> { [10] = Health(11, 100, 0) };

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                MonsterConfigAssembler.Build(health,
                    new Dictionary<int, Config_MonsterAttack>(),
                    new Dictionary<int, Config_MonsterRuntime>(),
                    new Dictionary<int, Config_MonsterSpawn>(),
                    new Dictionary<int, Config_MonsterView>(), 30));

            StringAssert.Contains("字典键 10 与首列 ID 11 不一致", exception.Message);
        }

        private static Config_Health Health(int id, float maxHealth, float damageGateSeconds) => new Config_Health
        {
            ProfileId = id,
            MaxHealth = maxHealth,
            MaxShield = 0,
            Defense = 2,
            DamageGateSeconds = damageGateSeconds,
        };

        private static Config_MonsterAttack Attack(int id, float distance) => new Config_MonsterAttack
        {
            AttackProfileId = id,
            OriginOffsetX = 0,
            OriginOffsetY = 1,
            OriginOffsetZ = 0,
            Radius = 0.5f,
            Distance = distance,
            TargetLayerMask = 64,
        };

        private static Config_MonsterRuntime Runtime(int id, int healthId, int attackId,
            float windup = 0.1f, float recovery = 0.2f) => new Config_MonsterRuntime
        {
            ConfigId = id,
            Name = $"monster-{id}",
            Enabled = true,
            MoveSpeed = 2,
            TargetModule = "NearestTarget",
            MoveModule = "DirectChase",
            AttackModule = "MeleeAttack",
            AttackRange = 2,
            WindupSeconds = windup,
            RecoverySeconds = recovery,
            AttackDamage = 10,
            HealthProfileId = healthId,
            AttackProfileId = attackId,
        };

        private static Config_MonsterSpawn Spawn(int id) => new Config_MonsterSpawn
        {
            ConfigId = id,
            Cost = 10,
            Weight = 1,
            MinDifficulty = 0,
            MaxDifficulty = 10,
            MaxPerWave = 5,
        };

        private static Config_MonsterView View(int id) => new Config_MonsterView
        {
            ConfigId = id,
            LocalPoolId = $"pool-{id}",
        };
    }
}
