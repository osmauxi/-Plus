using NUnit.Framework;
using ProjectGame.HotFix.Gameplay.CameraSystem;
using UnityEngine;

namespace ProjectGame.HotFix.Tests.EditMode
{
    public sealed class CameraModelsTests
    {
        private CameraMotionModel _motion;
        private CameraCompositionModel _composition;

        [SetUp]
        public void SetUp()
        {
            _motion = new CameraMotionModel();
            _motion.Reset(
                initialYaw: 0f,
                viewHeight: 20f,
                rotationSmoothTime: 0f,
                minViewHeight: 10f,
                maxViewHeight: 30f,
                zoomSmoothTime: 0f,
                baseFov: 60f,
                minFov: 35f,
                maxFov: 80f,
                fovSmoothTime: 0f);

            _composition = new CameraCompositionModel();
            //_composition.Reset(
            //    maxAimOffset: 3f,
            //    aimDeadZone: 1f,
            //    fullAimDistance: 8f,
            //    aimSmoothTime: 0f,
            //    aimReturnSmoothTime: 0f,
            //    maxMovementOffset: 1.5f,
            //    movementDeadZoneSpeed: 0.5f,
            //    fullMovementSpeed: 7f,
            //    movementSmoothTime: 0f,
            //    movementReturnSmoothTime: 0f,
            //    aimMovementWeight: 0f);
        }

        [Test]
        public void Motion_PersistentEffectsStackAndRemoveIndependently()
        {
            _motion.SetZoomModifier(CameraEffectId.Aim, -2f, 0f);
            _motion.SetZoomModifier(CameraEffectId.PlayerHit, 1f, 0f);
            _motion.SetFovModifier(CameraEffectId.Aim, -8f, 0f);
            _motion.SetFovModifier(CameraEffectId.PlayerHit, 2f, 0f);
            _motion.Snap();

            Assert.That(_motion.CurrentViewHeight, Is.EqualTo(19f).Within(0.001f));
            Assert.That(_motion.CurrentFov, Is.EqualTo(54f).Within(0.001f));

            _motion.SetZoomModifier(CameraEffectId.PlayerHit, 0f, 0f);
            _motion.SetFovModifier(CameraEffectId.PlayerHit, 0f, 0f);
            _motion.Snap();

            Assert.That(_motion.CurrentViewHeight, Is.EqualTo(18f).Within(0.001f));
            Assert.That(_motion.CurrentFov, Is.EqualTo(52f).Within(0.001f));
        }

        [Test]
        public void Motion_TransientKicksAttackAndRelease()
        {
            _motion.PlayZoomKick(2f, 0f, 0f, holdTime: 0.05f);
            _motion.PlayFovKick(4f, 0f, 0f, holdTime: 0.05f);
            _motion.Tick(0.01f);

            Assert.That(_motion.CurrentViewHeight, Is.EqualTo(22f).Within(0.01f));
            Assert.That(_motion.CurrentFov, Is.EqualTo(64f).Within(0.01f));

            _motion.Tick(0.05f);

            Assert.That(_motion.CurrentViewHeight, Is.EqualTo(20f).Within(0.01f));
            Assert.That(_motion.CurrentFov, Is.EqualTo(60f).Within(0.01f));
        }

        [Test]
        public void Motion_ResetCanPreservePersistentEffects()
        {
            _motion.SetZoomModifier(CameraEffectId.Aim, -2f, 0f);
            _motion.SetFovModifier(CameraEffectId.Aim, -8f, 0f);

            _motion.Reset(
                initialYaw: 90f,
                viewHeight: 24f,
                rotationSmoothTime: 0f,
                minViewHeight: 10f,
                maxViewHeight: 30f,
                zoomSmoothTime: 0f,
                baseFov: 65f,
                minFov: 35f,
                maxFov: 80f,
                fovSmoothTime: 0f,
                clearPersistentModifiers: false);
            _motion.Snap();

            Assert.That(_motion.CurrentYaw, Is.EqualTo(90f).Within(0.001f));
            Assert.That(_motion.CurrentViewHeight, Is.EqualTo(22f).Within(0.001f));
            Assert.That(_motion.CurrentFov, Is.EqualTo(57f).Within(0.001f));
        }

        [Test]
        public void Composition_AimUsesDeadZoneAndMaximumDistance()
        {
            _composition.SetAimActive(true);
            _composition.UpdateAimTarget(Vector3.zero, new Vector3(0.5f, 0f, 0f));
            _composition.Tick(0.02f);

            Assert.That(_composition.CurrentOffset, Is.EqualTo(Vector3.zero));

            _composition.UpdateAimTarget(Vector3.zero, new Vector3(10f, 0f, 0f));
            _composition.Tick(0.02f);

            Assert.That(_composition.CurrentOffset.x, Is.EqualTo(3f).Within(0.01f));
            Assert.That(_composition.CurrentOffset.z, Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void Composition_MovementUsesAdjacentSamplesAndIgnoresFirstSample()
        {
            _composition.UpdateMovementTarget(Vector3.zero, 1f);
            _composition.Tick(0.02f);
            Assert.That(_composition.CurrentOffset, Is.EqualTo(Vector3.zero));

            _composition.UpdateMovementTarget(new Vector3(7f, 0f, 0f), 1f);
            _composition.Tick(0.02f);

            Assert.That(_composition.CurrentOffset.x, Is.EqualTo(1.5f).Within(0.01f));
        }

        [Test]
        public void Composition_ResetMovementTrackingPreventsTargetSwitchJump()
        {
            _composition.UpdateMovementTarget(Vector3.zero, 1f);
            _composition.UpdateMovementTarget(new Vector3(7f, 0f, 0f), 1f);
            _composition.Tick(0.02f);
            Assert.That(_composition.CurrentOffset.x, Is.GreaterThan(1f));

            _composition.ResetMovementTracking();
            _composition.UpdateMovementTarget(new Vector3(100f, 0f, 0f), 1f);
            _composition.Tick(0.02f);

            Assert.That(_composition.CurrentOffset, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void Composition_AimCanTakeExclusiveControlFromMovement()
        {
            _composition.SetAimActive(true);
            _composition.UpdateAimTarget(Vector3.zero, new Vector3(0f, 0f, 10f));
            _composition.UpdateMovementTarget(Vector3.zero, 1f);
            _composition.UpdateMovementTarget(new Vector3(7f, 0f, 0f), 1f);
            _composition.Tick(0.02f);

            Assert.That(_composition.CurrentOffset.x, Is.EqualTo(0f).Within(0.01f));
            Assert.That(_composition.CurrentOffset.z, Is.EqualTo(3f).Within(0.01f));
        }
    }
}
