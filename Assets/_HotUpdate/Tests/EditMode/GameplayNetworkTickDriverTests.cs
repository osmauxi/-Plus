using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectGame.HotFix.Gameplay.Network;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Tests
{
    public sealed class GameplayNetworkTickDriverTests
    {
        private GameObject _object;
        private GameplayNetworkTickDriver _driver;
        private NetworkSimulationClock _clock;

        [SetUp]
        public void SetUp()
        {
            _object = new GameObject(nameof(GameplayNetworkTickDriverTests));
            _driver = _object.AddComponent<GameplayNetworkTickDriver>();
            _clock = new NetworkSimulationClock(30);
            _driver.Initialize(_clock);
        }

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(_object);

        [Test]
        public void PartialFrames_ProduceOneTickAtBoundary()
        {
            Assert.That(_driver.Advance(1d / 60d), Is.Zero);
            Assert.That(_clock.CurrentTick, Is.Zero);
            Assert.That(_driver.Advance(1d / 60d), Is.EqualTo(1));
            Assert.That(_clock.CurrentTick, Is.EqualTo(1u));
        }

        [TestCase(30, 20)]
        [TestCase(30, 30)]
        [TestCase(30, 60)]
        [TestCase(30, 144)]
        [TestCase(20, 60)]
        [TestCase(120, 30)]
        [TestCase(120, 144)]
        public void DifferentRenderRates_ProduceSameSimulationTicks(int tickRate, int frameRate)
        {
            _driver.Shutdown();
            _clock = new NetworkSimulationClock(tickRate);
            _driver.Initialize(_clock);
            for (int i = 0; i < frameRate * 10; i++)
                _driver.Advance(1d / frameRate);

            Assert.That(_clock.CurrentTick, Is.EqualTo((uint)(tickRate * 10)));
            Assert.That(_driver.DroppedTimeSeconds, Is.Zero);
        }

        [Test]
        public void IrregularFrames_PreserveElapsedTimeAcrossManyRemainders()
        {
            // 一组共 0.1 秒，覆盖不派发、一帧多步和带余数的边界。
            for (int i = 0; i < 100; i++)
            {
                _driver.Advance(0.005d);
                _driver.Advance(0.015d);
                _driver.Advance(0.08d);
            }
            Assert.That(_clock.CurrentTick, Is.EqualTo(300u));
            Assert.That(_driver.DroppedTimeSeconds, Is.Zero);
        }

        [Test]
        public void CatchUpAcrossUintWrap_PreservesContinuousEventsAndServerOffset()
        {
            _clock.ResetSession(uint.MaxValue - 1u, uint.MaxValue - 4u);
            var ticks = new List<uint>();
            _clock.TickAdvanced += ticks.Add;
            _driver.Advance(0.1d);
            Assert.That(ticks, Is.EqualTo(new[] { uint.MaxValue, 0u, 1u }));
            Assert.That(unchecked(_clock.CurrentTick - _clock.EstimatedServerTick), Is.EqualTo(3u));
        }

        [Test]
        public void CatchUp_CompletesAllPlayersBeforePostTick_ForEveryTick()
        {
            var order = new List<string>();
            _clock.TickCompleted += tick => order.Add($"post:{tick}");
            _clock.TickAdvanced += tick => order.Add($"player1:{tick}");
            _clock.TickAdvanced += tick => order.Add($"player2:{tick}");

            Assert.That(_driver.Advance(0.1d), Is.EqualTo(3));
            Assert.That(order, Is.EqualTo(new[]
            {
                "player1:1", "player2:1", "post:1",
                "player1:2", "player2:2", "post:2",
                "player1:3", "player2:3", "post:3",
            }));
        }

        [Test]
        public void LongStall_DropsExcessTime_KeepsFractionAndContinuousTickNumbers()
        {
            Assert.That(_driver.Advance(1d + 1d / 60d), Is.EqualTo(GameplayNetworkTickDriver.MaxTicksPerFrame));
            Assert.That(_clock.CurrentTick, Is.EqualTo(4u));
            Assert.That(_driver.DroppedTimeSeconds, Is.EqualTo(26d / 30d).Within(1e-9d));
            Assert.That(_driver.Advance(0d), Is.Zero);
            Assert.That(_driver.Advance(1d / 60d), Is.EqualTo(1));
            Assert.That(_clock.CurrentTick, Is.EqualTo(5u));
        }

        [Test]
        public void InitializeSameClock_DoesNotLoseFraction_AndOtherSessionRequiresShutdown()
        {
            _driver.Advance(1d / 60d);
            _driver.Initialize(_clock);
            Assert.That(_driver.Advance(1d / 60d), Is.EqualTo(1));
            Assert.Throws<InvalidOperationException>(() => _driver.Initialize(new NetworkSimulationClock(30)));
        }

        [Test]
        public void Shutdown_StopsTicks_AndRestartClearsFraction()
        {
            _driver.Advance(1d / 60d);
            _driver.Shutdown();
            Assert.That(_driver.Advance(1d), Is.Zero);
            Assert.That(_clock.CurrentTick, Is.Zero);
            _driver.Initialize(_clock);
            Assert.That(_driver.Advance(1d / 60d), Is.Zero);
            Assert.That(_driver.Advance(1d / 60d), Is.EqualTo(1));
        }

        [Test]
        public void ShutdownDuringDispatch_StopsRemainingCatchUpTicks()
        {
            _clock.TickCompleted += _ => _driver.Shutdown();
            Assert.That(_driver.Advance(0.1d), Is.EqualTo(1));
            Assert.That(_clock.CurrentTick, Is.EqualTo(1u));
        }

        [TestCase(-1d)]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void InvalidDelta_IsRejectedWithoutMutatingClock(double delta)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _driver.Advance(delta));
            Assert.That(_clock.CurrentTick, Is.Zero);
            Assert.That(_driver.Advance(1d / 30d), Is.EqualTo(1));
        }
    }
}
