using System;
using NUnit.Framework;
using ProjectGame.HotFix.Gameplay.Network;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using Unity.Netcode;

namespace ProjectGame.HotFix.Gameplay.Tests
{
    public sealed class GameplayNetworkFrameworkTests
    {
        [Test]
        public void Clock_AdvancesOneSharedTickAndPreservesUintWraparound()
        {
            var clock = new NetworkSimulationClock(30);
            uint observedTick = uint.MaxValue;
            int notificationCount = 0;
            clock.TickAdvanced += tick =>
            {
                observedTick = tick;
                notificationCount++;
            };

            Assert.That(clock.AdvanceOneTick(), Is.EqualTo(1u));
            Assert.That(observedTick, Is.EqualTo(1u));
            Assert.That(notificationCount, Is.EqualTo(1));
            Assert.That(clock.TickDeltaTime, Is.EqualTo(1f / 30f));

            clock.ResetSession(uint.MaxValue);
            Assert.That(clock.AdvanceOneTick(), Is.EqualTo(0u));
            Assert.That(observedTick, Is.EqualTo(0u));
            Assert.That(notificationCount, Is.EqualTo(2));
        }

        [TestCase(19)]
        [TestCase(121)]
        public void Config_RejectsTickRateOutsideSupportedSessionRange(int tickRate)
        {
            Assert.Throws<InvalidOperationException>(() => new NetworkSimulationConfig(tickRate));
        }

        [TestCase(NetworkDeliveryClass.Command, NetworkDelivery.UnreliableSequenced)]
        [TestCase(NetworkDeliveryClass.FullSnapshot, NetworkDelivery.ReliableSequenced)]
        [TestCase(NetworkDeliveryClass.DeltaSnapshot, NetworkDelivery.UnreliableSequenced)]
        [TestCase(NetworkDeliveryClass.ReliableEvent, NetworkDelivery.ReliableSequenced)]
        [TestCase(NetworkDeliveryClass.UnreliableEvent, NetworkDelivery.UnreliableSequenced)]
        public void DeliveryClass_MapsGameplaySemanticsCentrally(
            NetworkDeliveryClass deliveryClass,
            NetworkDelivery expected)
        {
            Assert.That(NetworkMessageTransport.ResolveDelivery(deliveryClass), Is.EqualTo(expected));
        }

        [Test]
        public void Stats_StartWithAnImmutableEmptySnapshot()
        {
            var stats = new NetworkTransportStats();

            Assert.That(stats.TotalMessageCount, Is.Zero);
            Assert.That(stats.TotalPayloadBytes, Is.Zero);
            Assert.That(stats.GetSentSnapshot(), Is.Empty);
            Assert.That(stats.TryGetSent("PG.Unknown", out _), Is.False);
        }

        [Test]
        public void PlayerCursor_ResetDoesNotMutateGlobalSessionClock()
        {
            var sessionClock = new NetworkSimulationClock(30);
            var playerCursor = new PlayerSimulationClock(30);
            sessionClock.ResetSession(100u);
            playerCursor.Reset(100u);

            playerCursor.Reset(80u);

            Assert.That(playerCursor.CurrentTick, Is.EqualTo(80u));
            Assert.That(sessionClock.CurrentTick, Is.EqualTo(100u));

            sessionClock.AdvanceOneTick();
            playerCursor.SynchronizeToSessionTick(sessionClock.CurrentTick);
            Assert.That(playerCursor.CurrentTick, Is.EqualTo(101u));
        }
    }
}
