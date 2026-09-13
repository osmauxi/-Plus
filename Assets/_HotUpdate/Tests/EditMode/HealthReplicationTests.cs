using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectGame.HotFix.Gameplay.Combat.Health;

namespace ProjectGame.HotFix.Gameplay.Tests
{
    public sealed class HealthReplicationTests
    {
        private static HealthRuntime Create() => new HealthRuntime(new HealthCatalog(new[] { new HealthDefinition(0, 100, 50) }));

        private static float Hp(IHealthStateSource source, HealthEntity entity)
        {
            Assert.That(source.TryGetHealth(entity, out var state), Is.True);
            return state.CurrentHealth;
        }

        [Test]
        public void FullThenDelta_ReplicatesManyEntities_AndDuplicateFramesAreIgnored()
        {
            using var server = Create();
            using var client = new HealthReplica(server.SessionId);
            var a = server.Register(0, 0); var b = server.Register(ulong.MaxValue, 0);
            Assert.That(client.Apply(server.CaptureFullSnapshot()), Is.EqualTo(HealthFrameResult.Applied));
            server.CaptureDelta();
            server.TryEnqueue(HealthCommand.Damage(1, 1, default, a, 20));
            server.TryEnqueue(HealthCommand.AddShield(1, 1, default, b, 30));
            server.Step(1);
            var delta = server.CaptureDelta();
            int changes = 0; client.StateChanged += _ => changes++;
            Assert.That(client.Apply(delta), Is.EqualTo(HealthFrameResult.Applied));
            Assert.That(client.Apply(delta), Is.EqualTo(HealthFrameResult.Stale));
            Assert.That(changes, Is.EqualTo(2));
            Assert.That(Hp(client, a), Is.EqualTo(80));
            Assert.That(client.TryGetHealth(b, out var state), Is.True);
            Assert.That(state.CurrentShield, Is.EqualTo(30));
        }

        [Test]
        public void LateJoinFullInsideDeltaInterval_DoesNotReapplyEarlierChanges()
        {
            using var server = Create();
            var target = server.Register(1, 0);
            server.TryEnqueue(HealthCommand.Damage(1, 1, default, target, 10)); server.Step(1);
            var full = server.CaptureFullSnapshot();
            using var client = new HealthReplica(server.SessionId);
            client.Apply(full);
            server.TryEnqueue(HealthCommand.Damage(2, 2, default, target, 10)); server.Step(2);
            var delta = server.CaptureDelta();
            Assert.That(delta.FromRevision, Is.LessThan(full.ToRevision));
            Assert.That(client.Apply(delta), Is.EqualTo(HealthFrameResult.Applied));
            Assert.That(Hp(client, target), Is.EqualTo(80));
        }

        [Test]
        public void MissingDelta_RejectsWithoutMutation_AndFullSnapshotRecovers()
        {
            using var server = Create();
            using var client = new HealthReplica(server.SessionId);
            var target = server.Register(1, 0);
            client.Apply(server.CaptureFullSnapshot()); server.CaptureDelta();
            server.TryEnqueue(HealthCommand.Damage(1, 1, default, target, 10)); server.Step(1);
            var lost = server.CaptureDelta();
            server.TryEnqueue(HealthCommand.Damage(2, 2, default, target, 10)); server.Step(2);
            var next = server.CaptureDelta();
            Assert.That(client.Apply(next), Is.EqualTo(HealthFrameResult.MissingBaseline));
            Assert.That(Hp(client, target), Is.EqualTo(100));
            Assert.That(client.Apply(server.CaptureFullSnapshot()), Is.EqualTo(HealthFrameResult.Applied));
            Assert.That(Hp(client, target), Is.EqualTo(80));
            Assert.That(client.Apply(lost), Is.EqualTo(HealthFrameResult.Stale));
        }

        [Test]
        public void FirstDeltaAndForeignSession_AreRejected_EvenWhenRevisionLooksNewer()
        {
            using var server = Create();
            using var foreign = Create();
            using var client = new HealthReplica(server.SessionId);
            server.Register(1, 0); foreign.Register(1, 0);
            Assert.That(client.Apply(server.CaptureDelta()), Is.EqualTo(HealthFrameResult.NeedsFullSnapshot));
            Assert.That(client.Apply(foreign.CaptureFullSnapshot()), Is.EqualTo(HealthFrameResult.WrongSession));
            Assert.That(client.Count, Is.Zero);
            Assert.That(client.IsInitialized, Is.False);
            Assert.That(client.Apply(server.CaptureFullSnapshot()), Is.EqualTo(HealthFrameResult.Applied));
        }

        [Test]
        public void EmptyFullSnapshot_InitializesAndRemovesEntitiesAfterDespawn()
        {
            using var server = Create();
            using var client = new HealthReplica(server.SessionId);
            Assert.That(client.Apply(server.CaptureFullSnapshot()), Is.EqualTo(HealthFrameResult.Applied));
            var target = server.Register(1, 0);
            client.Apply(server.CaptureDelta());
            int removals = 0;
            client.StateChanged += c => { if (c.Kind == HealthChangeKind.Removed) removals++; };
            server.Unregister(target);
            client.Apply(server.CaptureFullSnapshot());
            Assert.That(client.Count, Is.Zero);
            Assert.That(removals, Is.EqualTo(1));
        }

        [Test]
        public void PooledReplacementDelta_RemovesOldBindings_AndOldFrameCannotResurrectIt()
        {
            using var server = Create();
            using var client = new HealthReplica(server.SessionId);
            using var bindings = new HealthEntityBindings<object>(client);
            var old = server.Register(7, 0);
            var oldFull = server.CaptureFullSnapshot();
            client.Apply(oldFull); server.CaptureDelta();
            var collider = new object(); bindings.Bind(old, new[] { collider });
            var changes = new List<HealthChangeKind>(); client.StateChanged += c => changes.Add(c.Kind);
            server.Unregister(old); var next = server.Register(7, 0);
            client.Apply(server.CaptureDelta());
            Assert.That(changes, Is.EqualTo(new[] { HealthChangeKind.Removed, HealthChangeKind.Registered }));
            Assert.That(client.TryGetHealth(old, out _), Is.False);
            Assert.That(client.TryGetHealth(next, out _), Is.True);
            Assert.That(bindings.TryResolve(collider, out _), Is.False);
            Assert.That(client.Apply(oldFull), Is.EqualTo(HealthFrameResult.Stale));
        }

        [Test]
        public void CoalescedTombstone_RemovesEvenAnOlderGenerationKnownByClient()
        {
            using var server = Create();
            using var client = new HealthReplica(server.SessionId);
            var old = server.Register(1, 0);
            client.Apply(server.CaptureFullSnapshot()); server.CaptureDelta();
            server.Unregister(old); var next = server.Register(1, 0); server.Unregister(next);
            var delta = server.CaptureDelta();
            Assert.That(delta.Changes.Count, Is.EqualTo(1));
            Assert.That(delta.Changes[0].State.Entity, Is.EqualTo(next));
            client.Apply(delta);
            Assert.That(client.Count, Is.Zero);
        }

        [Test]
        public void FramesOwnImmutableCopies_AndCaptureFullDoesNotConsumeBroadcastDelta()
        {
            using var server = Create();
            var target = server.Register(1, 0);
            var full = server.CaptureFullSnapshot();
            var source = new List<HealthChange>(full.Changes);
            var copy = new HealthFrame(server.SessionId, true, 0, full.ToRevision, source);
            source.Clear();
            Assert.That(copy.Changes.Count, Is.EqualTo(1));
            Assert.Throws<NotSupportedException>(() => ((IList<HealthChange>)copy.Changes).Clear());
            server.TryEnqueue(HealthCommand.Damage(1, 1, default, target, 10)); server.Step(1);
            Assert.That(full.Changes[0].State.CurrentHealth, Is.EqualTo(100));
            var delta = server.CaptureDelta();
            Assert.That(delta.FromRevision, Is.Zero);
            Assert.That(delta.Changes[0].State.CurrentHealth, Is.EqualTo(90));
            Assert.That(server.CaptureDelta().Changes, Is.Empty);
        }

        [Test]
        public void InvalidFramesAndSnapshots_FailBeforeClientMutation()
        {
            using var server = Create();
            var target = server.Register(1, 0);
            server.TryGetHealth(target, out var state);
            var change = new HealthChange(HealthChangeKind.Registered, state, state.Revision);
            Assert.Throws<ArgumentException>(() => new HealthFrame(server.SessionId, true, 0, 1, new[] { change, change }));
            Assert.Throws<ArgumentException>(() => new HealthFrame(server.SessionId, false, 1, 2, Array.Empty<HealthChange>()));
            Assert.Throws<ArgumentException>(() => new HealthFrame(server.SessionId, true, 0, 1, new[] { default(HealthChange) }));
            Assert.Throws<ArgumentException>(() => new HealthSnapshot(target, state.Definition, float.NaN, 0, false, 0, false, 1));
            Assert.Throws<ArgumentException>(() => new HealthSnapshot(target, state.Definition, 101, 0, false, 0, false, 1));
        }

        [Test]
        public void ReplicaObserverFailure_CommitsEntireFrameAndNotifiesOtherObservers()
        {
            using var server = Create();
            using var client = new HealthReplica(server.SessionId);
            server.Register(1, 0); server.Register(2, 0);
            int count = 0;
            client.StateChanged += _ => throw new InvalidOperationException("broken UI");
            client.StateChanged += _ => count++;
            var full = server.CaptureFullSnapshot();
            Assert.Throws<AggregateException>(() => client.Apply(full));
            Assert.That(client.Count, Is.EqualTo(2));
            Assert.That(count, Is.EqualTo(2));
            Assert.That(client.Apply(full), Is.EqualTo(HealthFrameResult.Stale));
        }

        [Test]
        public void DeltaCoalescesState_WhileAuthorityStillReportsEachDeathAndRevive()
        {
            using var server = Create();
            using var client = new HealthReplica(server.SessionId);
            var target = server.Register(1, 0);
            client.Apply(server.CaptureFullSnapshot()); server.CaptureDelta();
            int deaths = 0, revives = 0;
            server.Died += _ => deaths++; server.Revived += _ => revives++;
            server.TryEnqueue(HealthCommand.Damage(1, 1, default, target, 200));
            server.TryEnqueue(HealthCommand.Revive(2, 1, default, target, 20));
            server.Step(1);
            var delta = server.CaptureDelta();
            Assert.That(delta.Changes.Count, Is.EqualTo(1));
            client.Apply(delta);
            Assert.That(Hp(client, target), Is.EqualTo(20));
            Assert.That(deaths, Is.EqualTo(1)); Assert.That(revives, Is.EqualTo(1));
        }

        [Test]
        public void ReplicaDisposal_ClearsStateAndRejectsFurtherFrames()
        {
            using var server = Create();
            using var client = new HealthReplica(server.SessionId);
            server.Register(1, 0);
            var full = server.CaptureFullSnapshot(); client.Apply(full);
            client.Dispose();
            Assert.That(client.Count, Is.Zero);
            Assert.Throws<ObjectDisposedException>(() => client.Apply(full));
        }
    }
}
