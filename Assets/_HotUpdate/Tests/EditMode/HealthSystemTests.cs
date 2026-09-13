using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectGame.HotFix.Gameplay.Combat.Health;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Tests
{
    public sealed class HealthSystemTests
    {
        private static HealthRuntime Create(uint age = 120, int capacity = 4096) => new HealthRuntime(new HealthCatalog(new[]
        {
            new HealthDefinition(0, 100, 50, 100, 3),
            new HealthDefinition(1, 200, 0, 0, 3),
        }), age, capacity);

        private static HealthSnapshot Read(IHealthStateSource source, HealthEntity entity)
        {
            Assert.That(source.TryGetHealth(entity, out var state), Is.True);
            return state;
        }

        [Test]
        public void PlayersAndMonsters_UseSameWorld_WithIndependentDefinitionsAndState()
        {
            using var runtime = Create();
            var player = runtime.Register(0, 0);
            var monster = runtime.Register(ulong.MaxValue, 1);
            runtime.TryEnqueue(HealthCommand.Damage(1, 0, default, player, 40));
            runtime.TryEnqueue(HealthCommand.Damage(1, 0, default, monster, 40));
            runtime.Step(0);
            Assert.That(Read(runtime, player).CurrentHealth, Is.EqualTo(80));
            Assert.That(Read(runtime, monster).CurrentHealth, Is.EqualTo(160));
            Assert.That(runtime.Count, Is.EqualTo(2));
        }

        [Test]
        public void PhysicalDamage_UsesDefenseThenShield_ReportsActualLossAndOverkill()
        {
            using var runtime = Create();
            var target = runtime.Register(1, 0, 30);
            var results = new List<HealthResult>();
            runtime.CommandResolved += results.Add;
            runtime.TryEnqueue(HealthCommand.Damage(1, 1, default, target, 100));
            runtime.TryEnqueue(HealthCommand.Damage(2, 1, default, target, 1000));
            runtime.Step(1);
            Assert.That(results[0].MitigatedDamage, Is.EqualTo(50));
            Assert.That(results[0].ShieldLost, Is.EqualTo(30));
            Assert.That(results[0].HealthLost, Is.EqualTo(20));
            Assert.That(results[0].ShieldBroken, Is.True);
            Assert.That(results[1].HealthLost, Is.EqualTo(80));
            Assert.That(results[1].WasKilled, Is.True);
            Assert.That(Read(runtime, target).CurrentHealth, Is.Zero);
        }

        [Test]
        public void TrueDamage_DoesNotImplicitlyBypassShield_AndSmallDamageIsNotRoundedUp()
        {
            using var runtime = Create();
            var target = runtime.Register(1, 0, 20);
            runtime.TryEnqueue(HealthCommand.Damage(1, 1, default, target, 10, HealthDamageType.True));
            runtime.TryEnqueue(HealthCommand.Damage(2, 1, default, target, 0.5f,
                flags: HealthDamageFlags.BypassShield));
            runtime.Step(1);
            Assert.That(Read(runtime, target).CurrentShield, Is.EqualTo(10));
            Assert.That(Read(runtime, target).CurrentHealth, Is.EqualTo(99.75f));
        }

        [Test]
        public void DeathOccursOnce_HealingCannotRevive_ExplicitReviveResetsTransientState()
        {
            using var runtime = Create();
            var target = runtime.Register(1, 0, 30);
            int deaths = 0, revives = 0;
            var results = new List<HealthResult>();
            runtime.Died += _ => deaths++;
            runtime.Revived += _ => revives++;
            runtime.CommandResolved += results.Add;
            runtime.TryEnqueue(HealthCommand.SetInvulnerable(1, 1, target, true));
            runtime.TryEnqueue(HealthCommand.Damage(2, 1, default, target, 1000,
                flags: HealthDamageFlags.BypassShield | HealthDamageFlags.BypassInvulnerability, gateChannel: 1));
            runtime.TryEnqueue(HealthCommand.Damage(3, 1, default, target, 10));
            runtime.TryEnqueue(HealthCommand.Heal(4, 1, default, target, 100));
            runtime.TryEnqueue(HealthCommand.AddShield(5, 1, default, target, 10));
            runtime.TryEnqueue(HealthCommand.Revive(6, 1, default, target, 1000));
            runtime.Step(1);
            Assert.That(deaths, Is.EqualTo(1));
            Assert.That(revives, Is.EqualTo(1));
            Assert.That(results[2].Code, Is.EqualTo(HealthResultCode.Dead));
            Assert.That(results[3].Code, Is.EqualTo(HealthResultCode.Dead));
            Assert.That(results[4].Code, Is.EqualTo(HealthResultCode.Dead));
            var state = Read(runtime, target);
            Assert.That(state.CurrentHealth, Is.EqualTo(100));
            Assert.That(state.CurrentShield, Is.Zero);
            Assert.That(state.IsInvulnerable || state.HasTakenDamage, Is.False);
            runtime.TryEnqueue(HealthCommand.Damage(7, 2, default, target, 10, gateChannel: 1));
            runtime.Step(2);
            Assert.That(Read(runtime, target).CurrentHealth, Is.EqualTo(95));
        }

        [Test]
        public void Gate_IsOptInAndScopedBySourceAndChannel_ExactExpiryAllowsHit()
        {
            using var runtime = Create();
            var target = runtime.Register(1, 1);
            var sourceA = runtime.Register(2, 1);
            var sourceB = runtime.Register(3, 1);
            var results = new List<HealthResult>();
            runtime.CommandResolved += results.Add;
            runtime.TryEnqueue(HealthCommand.Damage(1, 10, sourceA, target, 10, gateChannel: 1));
            runtime.TryEnqueue(HealthCommand.Damage(2, 10, sourceA, target, 10, gateChannel: 1));
            runtime.TryEnqueue(HealthCommand.Damage(3, 10, sourceB, target, 10, gateChannel: 1));
            runtime.TryEnqueue(HealthCommand.Damage(4, 10, sourceA, target, 10, gateChannel: 2));
            runtime.TryEnqueue(HealthCommand.Damage(5, 10, sourceA, target, 10));
            runtime.TryEnqueue(HealthCommand.Damage(6, 10, sourceA, target, 10));
            runtime.Step(10);
            Assert.That(results[1].Code, Is.EqualTo(HealthResultCode.DamageGated));
            Assert.That(Read(runtime, target).CurrentHealth, Is.EqualTo(150));
            runtime.TryEnqueue(HealthCommand.Damage(7, 12, sourceA, target, 10, gateChannel: 1));
            runtime.Step(12);
            Assert.That(Read(runtime, target).CurrentHealth, Is.EqualTo(150));
            runtime.TryEnqueue(HealthCommand.Damage(8, 13, sourceA, target, 10, gateChannel: 1));
            runtime.Step(13);
            Assert.That(Read(runtime, target).CurrentHealth, Is.EqualTo(140));
        }

        [Test]
        public void InvulnerabilityAndGateBypass_AreIndependent_AndRejectedEventsStayConsumed()
        {
            using var runtime = Create();
            var target = runtime.Register(1, 1);
            var hit = HealthCommand.Damage(2, 1, default, target, 10);
            var results = new List<HealthResult>();
            runtime.CommandResolved += results.Add;
            runtime.TryEnqueue(HealthCommand.SetInvulnerable(1, 1, target, true));
            runtime.TryEnqueue(hit);
            runtime.TryEnqueue(HealthCommand.Damage(3, 1, default, target, 10,
                flags: HealthDamageFlags.BypassInvulnerability, gateChannel: 1));
            runtime.TryEnqueue(HealthCommand.Damage(4, 1, default, target, 10,
                flags: HealthDamageFlags.BypassInvulnerability, gateChannel: 1));
            runtime.TryEnqueue(HealthCommand.Damage(5, 1, default, target, 10,
                flags: HealthDamageFlags.BypassInvulnerability | HealthDamageFlags.BypassDamageGate, gateChannel: 1));
            runtime.TryEnqueue(HealthCommand.SetInvulnerable(6, 1, target, false));
            runtime.TryEnqueue(hit);
            runtime.Step(1);
            Assert.That(results[1].Code, Is.EqualTo(HealthResultCode.Invulnerable));
            Assert.That(results[3].Code, Is.EqualTo(HealthResultCode.DamageGated));
            Assert.That(results[6].Code, Is.EqualTo(HealthResultCode.Duplicate));
            Assert.That(Read(runtime, target).CurrentHealth, Is.EqualTo(180));
        }

        [Test]
        public void DuplicateEvents_IncludeSourceAndTarget_AndExpireWithOriginalCommand()
        {
            using var runtime = Create(age: 2);
            var a = runtime.Register(1, 1);
            var b = runtime.Register(2, 1);
            var hit = HealthCommand.Damage(1, 1, default, a, 10);
            var codes = new List<HealthResultCode>();
            runtime.CommandResolved += r => codes.Add(r.Code);
            runtime.TryEnqueue(hit); runtime.TryEnqueue(hit);
            runtime.TryEnqueue(HealthCommand.Damage(1, 1, default, b, 10));
            runtime.TryEnqueue(HealthCommand.Damage(1, 1, b, a, 10));
            runtime.Step(1);
            runtime.TryEnqueue(hit); runtime.Step(3);
            runtime.TryEnqueue(hit); runtime.Step(4);
            Assert.That(codes, Is.EqualTo(new[] { HealthResultCode.Applied, HealthResultCode.Duplicate,
                HealthResultCode.Applied, HealthResultCode.Applied, HealthResultCode.Duplicate, HealthResultCode.Expired }));
            Assert.That(Read(runtime, a).CurrentHealth, Is.EqualTo(180));
        }

        [Test]
        public void PooledEntity_ReusesIdButNotGeneration_OldRequestAndRemovalCannotTouchNewLife()
        {
            using var runtime = Create();
            var old = runtime.Register(0, 0, 40);
            runtime.TryEnqueue(HealthCommand.Damage(1, 1, default, old, 1000));
            Assert.That(runtime.Unregister(old), Is.True);
            var next = runtime.Register(0, 0);
            Assert.That(next.Generation, Is.GreaterThan(old.Generation));
            Assert.That(runtime.Unregister(old), Is.False);
            HealthResult result = default;
            runtime.CommandResolved += r => result = r;
            runtime.Step(1);
            Assert.That(result.Code, Is.EqualTo(HealthResultCode.UnknownTarget));
            Assert.That(Read(runtime, next).CurrentHealth, Is.EqualTo(100));
            Assert.That(Read(runtime, next).CurrentShield, Is.Zero);
        }

        [Test]
        public void SourceMayDespawnBeforeProjectileHits_TargetMustStillMatchGeneration()
        {
            using var runtime = Create();
            var target = runtime.Register(1, 1);
            var source = runtime.Register(2, 1);
            runtime.Unregister(source);
            runtime.TryEnqueue(HealthCommand.Damage(1, 1, source, target, 10));
            runtime.Step(1);
            Assert.That(Read(runtime, target).CurrentHealth, Is.EqualTo(190));
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        [TestCase(-1f)]
        [TestCase(0f)]
        public void InvalidAmounts_NeverChangeState(float amount)
        {
            using var runtime = Create();
            var target = runtime.Register(1, 1);
            var revision = runtime.Revision;
            HealthResult result = default;
            runtime.CommandResolved += r => result = r;
            runtime.TryEnqueue(HealthCommand.Damage(1, 1, default, target, amount));
            runtime.Step(1);
            Assert.That(result.Code, Is.EqualTo(HealthResultCode.InvalidCommand));
            Assert.That(runtime.Revision, Is.EqualTo(revision));
        }

        [Test]
        public void InvalidContextOrDefaultCommand_IsRejectedBeforeMutatingWorld()
        {
            using var runtime = Create();
            var target = runtime.Register(1, 1);
            var codes = new List<HealthResultCode>();
            runtime.CommandResolved += r => codes.Add(r.Code);
            runtime.TryEnqueue(default);
            runtime.TryEnqueue(HealthCommand.Damage(1, 1, default, target, 10,
                impact: new HealthImpact(new Vector3(float.NaN, 0, 0), Vector3.zero)));
            runtime.TryEnqueue(HealthCommand.Reconfigure(2, 1, target, null));
            runtime.Step(1);
            Assert.That(codes, Is.All.EqualTo(HealthResultCode.InvalidCommand));
        }

        [Test]
        public void ExtremeFiniteValues_DoNotOverflow_HealingAndShieldClampAtLimits()
        {
            var catalog = new HealthCatalog(new[] { new HealthDefinition(0, float.MaxValue, float.MaxValue, float.MaxValue) });
            using var runtime = new HealthRuntime(catalog);
            var target = runtime.Register(1, 0, float.MaxValue);
            runtime.TryEnqueue(HealthCommand.Damage(1, 1, default, target, float.MaxValue, HealthDamageType.True));
            runtime.TryEnqueue(HealthCommand.AddShield(2, 1, default, target, float.MaxValue));
            runtime.TryEnqueue(HealthCommand.AddShield(3, 1, default, target, float.MaxValue));
            runtime.TryEnqueue(HealthCommand.Heal(4, 1, default, target, float.MaxValue));
            runtime.Step(1);
            Assert.That(Read(runtime, target).CurrentHealth, Is.EqualTo(float.MaxValue));
            Assert.That(Read(runtime, target).CurrentShield, Is.EqualTo(float.MaxValue));
        }

        [Test]
        public void Reconfigure_ClampsValuesWithoutHealingOrReviving()
        {
            using var runtime = Create();
            var target = runtime.Register(1, 0, 50);
            runtime.TryEnqueue(HealthCommand.Reconfigure(1, 1, target, new HealthDefinition(3, 60, 10)));
            runtime.Step(1);
            Assert.That(Read(runtime, target).CurrentHealth, Is.EqualTo(60));
            Assert.That(Read(runtime, target).CurrentShield, Is.EqualTo(10));
            runtime.TryEnqueue(HealthCommand.Reconfigure(2, 2, target, new HealthDefinition(4, 200, 100)));
            runtime.Step(2);
            Assert.That(Read(runtime, target).CurrentHealth, Is.EqualTo(60));
            runtime.TryEnqueue(HealthCommand.Damage(3, 3, default, target, 1000));
            runtime.TryEnqueue(HealthCommand.Reconfigure(4, 3, target, new HealthDefinition(5, 500)));
            runtime.Step(3);
            Assert.That(Read(runtime, target).IsAlive, Is.False);
        }

        [Test]
        public void TickWrapAndGateExpiry_Work_WhileRepeatedOrBackwardStepsAreRejected()
        {
            using var runtime = Create();
            var target = runtime.Register(1, 1);
            runtime.TryEnqueue(HealthCommand.Damage(1, uint.MaxValue - 1, default, target, 10, gateChannel: 1));
            runtime.Step(uint.MaxValue - 1);
            runtime.TryEnqueue(HealthCommand.Damage(2, 0, default, target, 10, gateChannel: 1));
            runtime.Step(0);
            Assert.That(Read(runtime, target).CurrentHealth, Is.EqualTo(190));
            runtime.TryEnqueue(HealthCommand.Damage(3, 1, default, target, 10, gateChannel: 1));
            runtime.Step(1);
            Assert.That(Read(runtime, target).CurrentHealth, Is.EqualTo(180));
            Assert.Throws<ArgumentOutOfRangeException>(() => runtime.Step(1));
            Assert.Throws<ArgumentOutOfRangeException>(() => runtime.Step(0));
        }

        [Test]
        public void QueueBackpressureAndFutureRequests_AreExplicit()
        {
            using var runtime = Create(capacity: 1);
            var target = runtime.Register(1, 1);
            HealthResult result = default;
            runtime.CommandResolved += r => result = r;
            Assert.That(runtime.TryEnqueue(HealthCommand.Damage(1, 2, default, target, 10)), Is.True);
            Assert.That(runtime.TryEnqueue(HealthCommand.Damage(2, 1, default, target, 10)), Is.False);
            runtime.Step(1);
            Assert.That(result.Code, Is.EqualTo(HealthResultCode.FutureTick));
            Assert.That(runtime.PendingCount, Is.Zero);
        }

        [Test]
        public void ObserverFailures_DoNotAbortBatch_AndEnqueuedReactionsWaitForNextTick()
        {
            using var runtime = Create();
            var target = runtime.Register(1, 1);
            int observations = 0;
            var observedHealth = new List<float>();
            runtime.CommandResolved += _ => throw new InvalidOperationException("broken presenter");
            runtime.CommandResolved += r =>
            {
                observations++;
                observedHealth.Add(Read(runtime, target).CurrentHealth);
                if (r.Command.EventId == 1) runtime.TryEnqueue(HealthCommand.Heal(3, 2, default, target, 5));
            };
            runtime.TryEnqueue(HealthCommand.Damage(1, 1, default, target, 10));
            runtime.TryEnqueue(HealthCommand.Damage(2, 1, default, target, 10));
            var error = Assert.Throws<AggregateException>(() => runtime.Step(1));
            Assert.That(error.InnerExceptions.Count, Is.EqualTo(2));
            Assert.That(observations, Is.EqualTo(2));
            Assert.That(runtime.PendingCount, Is.EqualTo(1));
            Assert.That(Read(runtime, target).CurrentHealth, Is.EqualTo(180));
            Assert.Throws<AggregateException>(() => runtime.Step(2));
            Assert.That(Read(runtime, target).CurrentHealth, Is.EqualTo(185));
            Assert.That(observedHealth, Is.EqualTo(new[] { 180f, 180f, 185f }));
        }

        [Test]
        public void ReactionOutput_PreservesIndependentImpact_AndCanBeSuppressed()
        {
            using var runtime = Create();
            var target = runtime.Register(1, 1);
            var reactions = new List<HealthResult>();
            runtime.ReactionRequested += reactions.Add;
            var impact = new HealthImpact(new Vector3(1, 2, 3), Vector3.right, 7, 8);
            runtime.TryEnqueue(HealthCommand.Damage(1, 1, default, target, 1, impact: impact));
            runtime.TryEnqueue(HealthCommand.Damage(2, 1, default, target, 100,
                flags: HealthDamageFlags.SuppressReaction, impact: impact));
            runtime.Step(1);
            Assert.That(reactions.Count, Is.EqualTo(1));
            Assert.That(reactions[0].Command.Impact.Strength, Is.EqualTo(7));
            Assert.That(reactions[0].Command.Impact.ReactionProfileId, Is.EqualTo(8));
            Assert.That(Read(runtime, target).CurrentHealth, Is.EqualTo(99));
        }

        [Test]
        public void CentralBindings_AreAtomicAndCleanedOnDespawn_WithoutPerEntityComponents()
        {
            using var runtime = Create();
            using var bindings = new HealthEntityBindings<object>(runtime);
            var a = runtime.Register(1, 1); var b = runtime.Register(2, 1);
            var head = new object(); var body = new object(); var other = new object();
            bindings.Bind(a, new[] { head, body }); bindings.Bind(b, new[] { other });
            Assert.Throws<InvalidOperationException>(() => bindings.Bind(a, new[] { head, other }));
            Assert.That(bindings.TryResolve(body, out var resolved), Is.True);
            Assert.That(resolved, Is.EqualTo(a));
            runtime.Unregister(a);
            Assert.That(bindings.TryResolve(body, out _), Is.False);
            Assert.That(bindings.Count, Is.EqualTo(1));
            var next = runtime.Register(1, 1);
            bindings.Bind(next, new[] { body });
            Assert.That(bindings.TryResolve(body, out resolved), Is.True);
            Assert.That(resolved, Is.EqualTo(next));
        }

        [Test]
        public void CatalogValidationAndRuntimeDisposal_DoNotLeaveMutableState()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HealthDefinition(0, float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => new HealthDefinition(0, 10, defense: -1));
            var definition = new HealthDefinition(0, 10);
            Assert.Throws<ArgumentException>(() => new HealthCatalog(new[] { definition, definition }));
            using var runtime = Create();
            var target = runtime.Register(1, 1);
            var snapshot = Read(runtime, target);
            runtime.Dispose();
            Assert.That(runtime.TryGetHealth(target, out _), Is.False);
            Assert.That(snapshot.CurrentHealth, Is.EqualTo(200));
            Assert.Throws<ObjectDisposedException>(() => runtime.TryEnqueue(default));
            Assert.Throws<ObjectDisposedException>(() => runtime.Register(2, 1));
        }
    }
}
