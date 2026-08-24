using NUnit.Framework;
using ProjectGame.HotFix.Gameplay.Player.Movement;
using ProjectGame.HotFix.Gameplay.Player.Stamina;
using ProjectGame.HotFix.Gameplay.Player.State;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using System.Reflection;
using UnityEngine;

namespace ProjectGame.HotFix.Tests.EditMode
{
    public sealed class PlayerSyncPipelineTests
    {
        private GameObject _playerObject;
        private PlayerSimulation _simulation;
        private PlayerSimulationClock _clock;
        private PlayerSyncConfig _config;

        [SetUp]
        public void SetUp()
        {
            _playerObject = new GameObject("PlayerSyncPipelineTests");
            CharacterController characterController = _playerObject.AddComponent<CharacterController>();
            IPlayerCharacterBody body = new CharacterControllerPlayerBody(_playerObject.transform, characterController);
            PlayerMotor motor = new(body, new PlayerMovementConfig());
            PlayerLocomotionController locomotion = new(
                motor,
                new PlayerStaminaConfig(),
                new PlayerActionConfig());
            _simulation = new PlayerSimulation(locomotion);
            _config = new PlayerSyncConfig();
            _clock = new PlayerSimulationClock(_config.SimulationTickRate);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_playerObject);
        }

        [Test]
        public void SimulationStack_UsesPlainCSharpServices()
        {
            Assert.That(typeof(MonoBehaviour).IsAssignableFrom(typeof(PlayerMotor)), Is.False);
            Assert.That(typeof(MonoBehaviour).IsAssignableFrom(typeof(PlayerLocomotionController)), Is.False);
            Assert.That(typeof(MonoBehaviour).IsAssignableFrom(typeof(PlayerSimulation)), Is.False);
            Assert.That(typeof(MonoBehaviour).IsAssignableFrom(typeof(PlayerSimulationClock)), Is.False);
        }

        [Test]
        public void SimulationClock_AdvancesResetsAndWraps()
        {
            PlayerSimulationClock clock = new(30);

            Assert.That(clock.TickRate, Is.EqualTo(30));
            Assert.That(clock.TickDeltaTime, Is.EqualTo(1f / 30f).Within(0.000001f));

            clock.Reset(100u);
            Assert.That(clock.AdvanceOneTick(), Is.EqualTo(101u));
            Assert.That(clock.CurrentTick, Is.EqualTo(101u));

            clock.Reset(uint.MaxValue);
            Assert.That(clock.AdvanceOneTick(), Is.EqualTo(0u));
        }

        [Test]
        public void Prediction_ConsumesTickFromSimulationClock()
        {
            PlayerPrediction prediction = new(_simulation, _clock, _config);
            prediction.Reset(10u);

            uint tick = _clock.AdvanceOneTick();
            PlayerInputCommand input = PlayerInputCommand.CreateNeutral(0u);
            PlayerSimulationState state = prediction.Predict(ref input, tick);

            Assert.That(tick, Is.EqualTo(11u));
            Assert.That(input.Tick, Is.EqualTo(tick));
            Assert.That(state.Tick, Is.EqualTo(tick));
            Assert.That(prediction.CurrentTick, Is.EqualTo(tick));
        }

        [Test]
        public void AimDirectionChange_TriggersImmediateInputSend()
        {
            PlayerPrediction prediction = new(_simulation, _clock, _config);
            prediction.Reset();

            PlayerInputCommand first = new()
            {
                Tick = 1,
                AimDirection = Vector2.right,
                Buttons = PlayerInputButtons.AimHeld,
            };
            Assert.That(prediction.ShouldSend(first), Is.True);
            prediction.MarkSent(first);

            PlayerInputCommand changedAim = first;
            changedAim.Tick = 2;
            changedAim.AimDirection = Vector2.left;

            Assert.That(prediction.ShouldSend(changedAim), Is.True);
        }

        [Test]
        public void ServerAuthority_AcceptsNextTick_AndRejectsAlreadyProcessedTick()
        {
            PlayerServerAuthority authority = new(_simulation, _clock, _config);
            PlayerSimulationState initial = _simulation.CaptureState(100);
            authority.Initialize(initial);

            PlayerInputCommand input = PlayerInputCommand.CreateNeutral(101);
            input.WorldMove = Vector2.up;

            Assert.That(authority.PushInput(input), Is.True);
            uint tick = _clock.AdvanceOneTick();
            PlayerSimulationState result = authority.SimulateNextTick(tick);
            Assert.That(result.Tick, Is.EqualTo(101));
            Assert.That(authority.LastInputResolveResult, Is.EqualTo(PlayerServerAuthority.ResolveResult.Exact));
            Assert.That(authority.PushInput(input), Is.False);
            Assert.That(authority.OutdatedInputCount, Is.EqualTo(1));
        }

        [Test]
        public void ServerAuthority_StoresSanitizedInputBeforeSimulation()
        {
            PlayerServerAuthority authority = new(_simulation, _clock, _config);
            authority.Initialize(_simulation.CaptureState(20));

            PlayerInputCommand untrustedInput = new()
            {
                Tick = 21,
                WorldMove = new Vector2(3f, 4f),
                AimDirection = new Vector2(12f, 0f),
                Buttons = (PlayerInputButtons)byte.MaxValue,
            };

            Assert.That(authority.PushInput(untrustedInput), Is.True);
            uint tick = _clock.AdvanceOneTick();
            authority.SimulateNextTick(tick);

            FieldInfo resolvedInputField = typeof(PlayerServerAuthority).GetField(
                "_lastResolvedInput",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(resolvedInputField, Is.Not.Null);

            PlayerInputCommand resolved = (PlayerInputCommand)resolvedInputField.GetValue(authority);
            Assert.That(resolved.WorldMove.magnitude, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(resolved.AimDirection, Is.EqualTo(Vector2.right));
            Assert.That(resolved.Buttons, Is.EqualTo(
                PlayerInputButtons.AimHeld |
                PlayerInputButtons.SprintHeld |
                PlayerInputButtons.FireHeld));
        }

        [Test]
        public void ServerAuthority_RetimesNewestLateInputWithinConfiguredWindow()
        {
            PlayerServerAuthority authority = new(_simulation, _clock, _config);
            authority.Initialize(_simulation.CaptureState(100));

            PlayerInputCommand late = PlayerInputCommand.CreateNeutral(98);
            late.WorldMove = Vector2.up;

            Assert.That(authority.PushInput(late), Is.True);
            Assert.That(authority.RetimedLateAcceptedInputCount, Is.EqualTo(1));

            uint tick = _clock.AdvanceOneTick();
            PlayerSimulationState result = authority.SimulateNextTick(tick);
            Assert.That(result.Tick, Is.EqualTo(101));
            Assert.That(authority.LastInputResolveResult,
                Is.EqualTo(PlayerServerAuthority.ResolveResult.RetimedLate));
            Assert.That(authority.RetimedLateInputTickCount, Is.EqualTo(1));

            PlayerInputCommand olderRedundancy = late.WithTick(97);
            Assert.That(authority.PushInput(olderRedundancy), Is.False);
            Assert.That(authority.RetimedLateAcceptedInputCount, Is.EqualTo(1));
        }

        [Test]
        public void SnapshotAheadOfPrediction_PerformsHardResync()
        {
            PlayerPrediction prediction = new(_simulation, _clock, _config);
            prediction.Reset(10);
            PlayerSimulationState serverState = _simulation.CaptureState(15);
            serverState.Position = new Vector3(7f, 0f, -3f);

            Assert.That(prediction.Reconcile(serverState), Is.True);
            Assert.That(prediction.CurrentTick, Is.EqualTo(15));
            Assert.That(prediction.LastConfirmedTick, Is.EqualTo(15));
            Assert.That(prediction.HardResyncCount, Is.EqualTo(1));
            Assert.That(_playerObject.transform.position, Is.EqualTo(serverState.Position));
        }

        // Action HFSM / rollback contract:
        // 1. FireHeld 只按固定冷却递增事件序号；
        // 2. Reload 边沿只消费一次并压制 Fire；
        // 3. Hit 中断 Reload，Dead 再压制 Hit；
        // 4. ActionState 必须进入 Delta，并在预测不一致时触发回滚。
        [Test]
        public void FireHeld_ProducesSequenceAtFixedCadence()
        {
            PlayerInputCommand input = PlayerInputCommand.CreateNeutral(1u);
            input.Buttons = PlayerInputButtons.FireHeld;

            PlayerSimulationState first = _simulation.Simulate(input, 1f / 30f);
            Assert.That(first.ControlState.CombatMode, Is.EqualTo(PlayerCombatMode.Firing));
            Assert.That(first.ActionState.ShotSequence, Is.EqualTo(1u));

            PlayerSimulationState current = first;
            for (uint tick = 2u; tick <= 5u; tick++)
            {
                input.Tick = tick;
                current = _simulation.Simulate(input, 1f / 30f);
            }

            Assert.That(current.ActionState.ShotSequence, Is.EqualTo(2u));
        }

        [Test]
        public void ReloadRequest_BlocksFireAndIsConsumedOnlyOnce()
        {
            PlayerInputCommand input = PlayerInputCommand.CreateNeutral(1u);
            input.Buttons = PlayerInputButtons.FireHeld;
            input.ReloadRequestSequence = 1;

            PlayerSimulationState reloading = _simulation.Simulate(input, 1f / 30f);
            Assert.That(reloading.ControlState.CombatMode, Is.EqualTo(PlayerCombatMode.Reloading));
            Assert.That(reloading.ActionState.ShotSequence, Is.EqualTo(0u));
            Assert.That(reloading.ActionState.LastReloadRequestSequence, Is.EqualTo(1));

            input.Tick = 2u;
            PlayerSimulationState continued = _simulation.Simulate(input, 1f / 30f);
            Assert.That(continued.ControlState.CombatMode, Is.EqualTo(PlayerCombatMode.Reloading));
            Assert.That(continued.ActionState.ShotSequence, Is.EqualTo(0u));
            Assert.That(
                continued.ActionState.ReloadTicksRemaining,
                Is.LessThan(reloading.ActionState.ReloadTicksRemaining));
        }

        [Test]
        public void HitInterruptsReload_AndDeathOverridesHit()
        {
            PlayerInputCommand reload = PlayerInputCommand.CreateNeutral(1u);
            reload.ReloadRequestSequence = 1;
            _simulation.Simulate(reload, 1f / 30f);

            Assert.That(_simulation.ApplyHit(1f / 30f), Is.True);
            PlayerSimulationState hit = _simulation.CaptureState(1u);
            Assert.That(hit.ControlState.IsHitReacting, Is.True);
            Assert.That(hit.ControlState.CombatMode, Is.EqualTo(PlayerCombatMode.Ready));
            Assert.That(hit.ActionState.ReloadTicksRemaining, Is.EqualTo(0));
            Assert.That(hit.ActionState.HitSequence, Is.EqualTo(1u));

            _simulation.SetLifeState(PlayerLifeState.Dead);
            PlayerSimulationState dead = _simulation.CaptureState(2u);
            Assert.That(dead.ControlState.IsDead, Is.True);
            Assert.That(dead.ControlState.IsHitReacting, Is.False);
            Assert.That(dead.ControlState.CombatMode, Is.EqualTo(PlayerCombatMode.Ready));
            Assert.That(_simulation.ApplyHit(1f / 30f), Is.False);
        }

        [Test]
        public void ActionStateChange_IsIncludedInDeltaSnapshot()
        {
            PlayerSimulationState baseline = _simulation.CaptureState(10u);
            PlayerSimulationState current = baseline;
            current.Tick = 11u;
            current.ActionState.ShotSequence = 1u;

            PlayerSnapshotPacket packet = PlayerSnapshotPacket.CreateDelta(current, baseline);

            Assert.That(
                packet.DirtyMask & PlayerStateDirtyMask.ActionState,
                Is.EqualTo(PlayerStateDirtyMask.ActionState));
            Assert.That(packet.TryResolve(baseline, out PlayerSimulationState resolved), Is.True);
            Assert.That(resolved.ActionState.ShotSequence, Is.EqualTo(1u));
        }

        [Test]
        public void ActionStateMismatch_TriggersPredictionRollback()
        {
            PlayerPrediction prediction = new(_simulation, _clock, _config);
            prediction.Reset(0u);

            uint tick = _clock.AdvanceOneTick();
            PlayerInputCommand input = PlayerInputCommand.CreateNeutral(tick);
            PlayerSimulationState predicted = prediction.Predict(ref input, tick);
            PlayerSimulationState serverState = predicted;
            serverState.ActionState.HitSequence = 1u;

            Assert.That(prediction.Reconcile(serverState), Is.True);
            Assert.That(prediction.RollbackCount, Is.EqualTo(1));
            Assert.That(_simulation.ActionState.HitSequence, Is.EqualTo(1u));
        }

        [Test]
        public void AimBodyTurn_StartsAboveStartAngle_AndStopsInsideStopAngle()
        {
            PlayerInputCommand input = PlayerInputCommand.CreateNeutral(1u);
            input.AimDirection = Vector2.right;
            input.Buttons = PlayerInputButtons.AimHeld;

            PlayerSimulationState turning = _simulation.Simulate(input, 1f / 30f);
            Assert.That(turning.IsAimBodyTurning, Is.True);

            turning.Rotation = Quaternion.Euler(0f, 80f, 0f);
            turning.IsAimBodyTurning = true;
            _simulation.RestoreState(turning);

            input.Tick = 2u;
            PlayerSimulationState stopped = _simulation.Simulate(input, 1f / 30f);
            Assert.That(stopped.IsAimBodyTurning, Is.False);
        }

        [Test]
        public void AimBodyTurn_RestoredHysteresisStateControlsMidBand()
        {
            PlayerSimulationState restored = _simulation.CaptureState(0u);
            PlayerInputCommand input = PlayerInputCommand.CreateNeutral(1u);
            input.AimDirection = DirectionFromYaw(40f);
            input.Buttons = PlayerInputButtons.AimHeld;

            restored.IsAimBodyTurning = true;
            _simulation.RestoreState(restored);
            Assert.That(_simulation.Simulate(input, 1f / 30f).IsAimBodyTurning, Is.True);

            restored.IsAimBodyTurning = false;
            _simulation.RestoreState(restored);
            input.Tick = 2u;
            Assert.That(_simulation.Simulate(input, 1f / 30f).IsAimBodyTurning, Is.False);
        }

        [Test]
        public void AimBodyTurningChange_IsIncludedInDeltaSnapshot()
        {
            PlayerSimulationState baseline = _simulation.CaptureState(10u);
            PlayerSimulationState current = baseline;
            current.Tick = 11u;
            current.IsAimBodyTurning = true;

            PlayerSnapshotPacket packet = PlayerSnapshotPacket.CreateDelta(current, baseline);

            Assert.That(
                packet.DirtyMask & PlayerStateDirtyMask.AimBodyTurning,
                Is.EqualTo(PlayerStateDirtyMask.AimBodyTurning));
            Assert.That(packet.TryResolve(baseline, out PlayerSimulationState resolved), Is.True);
            Assert.That(resolved.IsAimBodyTurning, Is.True);
        }

        [Test]
        public void AimBodyTurningMismatch_TriggersPredictionRollback()
        {
            PlayerPrediction prediction = new(_simulation, _clock, _config);
            prediction.Reset(0u);

            uint tick = _clock.AdvanceOneTick();
            PlayerInputCommand input = PlayerInputCommand.CreateNeutral(tick);
            input.AimDirection = DirectionFromYaw(40f);
            input.Buttons = PlayerInputButtons.AimHeld;
            PlayerSimulationState predicted = prediction.Predict(ref input, tick);
            Assert.That(predicted.IsAimBodyTurning, Is.False);

            PlayerSimulationState serverState = predicted;
            serverState.IsAimBodyTurning = true;

            Assert.That(prediction.Reconcile(serverState), Is.True);
            Assert.That(prediction.RollbackCount, Is.EqualTo(1));
            Assert.That(_simulation.IsAimBodyTurning, Is.True);
        }

        [Test]
        public void PivotDirection_IsLatchedRestoredAndIncludedInDeltaSnapshot()
        {
            PlayerSimulationState moving = _simulation.CaptureState(10u);
            moving.Rotation = Quaternion.identity;
            moving.Velocity = Vector3.forward * 4f;
            moving.PivotDirection = PlayerPivotDirection.None;
            _simulation.RestoreState(moving);
            PlayerSimulationState baseline = _simulation.CaptureState(10u);

            PlayerInputCommand input = PlayerInputCommand.CreateNeutral(11u);
            input.WorldMove = Vector2.down;
            PlayerSimulationState pivoting = _simulation.Simulate(input, 1f / 30f);

            Assert.That(pivoting.PivotDirection, Is.EqualTo(PlayerPivotDirection.Backward));
            Assert.That(_simulation.MotionState.IsPivoting, Is.True);
            Assert.That(_simulation.MotionState.PivotDirection, Is.EqualTo(PlayerPivotDirection.Backward));

            PlayerSnapshotPacket packet = PlayerSnapshotPacket.CreateDelta(pivoting, baseline);
            Assert.That(
                packet.DirtyMask & PlayerStateDirtyMask.PivotDirection,
                Is.EqualTo(PlayerStateDirtyMask.PivotDirection));
            Assert.That(packet.TryResolve(baseline, out PlayerSimulationState resolved), Is.True);
            Assert.That(resolved.PivotDirection, Is.EqualTo(PlayerPivotDirection.Backward));

            _simulation.RestoreState(resolved);
            Assert.That(_simulation.MotionState.IsPivoting, Is.True);
            Assert.That(_simulation.MotionState.PivotDirection, Is.EqualTo(PlayerPivotDirection.Backward));
        }

        [Test]
        public void PivotDirectionMismatch_TriggersPredictionRollback()
        {
            PlayerPrediction prediction = new(_simulation, _clock, _config);
            prediction.Reset(0u);

            uint tick = _clock.AdvanceOneTick();
            PlayerInputCommand input = PlayerInputCommand.CreateNeutral(tick);
            PlayerSimulationState predicted = prediction.Predict(ref input, tick);
            PlayerSimulationState serverState = predicted;
            serverState.PivotDirection = PlayerPivotDirection.Left;

            Assert.That(prediction.Reconcile(serverState), Is.True);
            Assert.That(prediction.RollbackCount, Is.EqualTo(1));
            Assert.That(_simulation.MotionState.PivotDirection, Is.EqualTo(PlayerPivotDirection.Left));
        }


        [Test]
        public void RemoteInterpolation_SamplesAcrossUIntTickOverflow()
        {
            PlayerRemoteInterpolation interpolation = new(_config);
            PlayerSimulationState beforeWrap = _simulation.CaptureState(uint.MaxValue - 1u);
            beforeWrap.Position = Vector3.zero;
            PlayerSimulationState afterWrap = beforeWrap;
            afterWrap.Tick = 1u;
            afterWrap.Position = new Vector3(3f, 0f, 0f);

            interpolation.PushSnapshot(beforeWrap);
            interpolation.PushSnapshot(afterWrap);

            Assert.That(interpolation.BufferedSnapshotCount, Is.EqualTo(2));
            Assert.That(interpolation.TrySample(1f, out PlayerSimulationState sampled), Is.True);
            Assert.That(sampled.Position.x, Is.InRange(0f, 3f));
        }

        private static Vector2 DirectionFromYaw(float yawDegrees)
        {
            float radians = yawDegrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
        }

    }
}
