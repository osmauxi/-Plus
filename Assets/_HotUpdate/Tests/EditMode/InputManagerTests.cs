using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.InputSystem;
using RuntimeInputContext = ProjectGame.HotFix.Gameplay.Input.InputContext;
using RuntimeInputManager = ProjectGame.HotFix.Gameplay.Input.InputManager;

namespace ProjectGame.HotFix.Tests.EditMode
{
    public sealed class InputManagerTests : InputTestFixture
    {
        private const string InputActionsPath =
            "Assets/_HotUpdate/Resources/Input/GameplayInputActions.inputactions";

        private GameObject _gameObject;
        private RuntimeInputManager _inputManager;

        public override void Setup()
        {
            base.Setup();

            _gameObject = new GameObject("InputManagerTests");
            _inputManager = _gameObject.AddComponent<RuntimeInputManager>();

            var template = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            Assert.That(template, Is.Not.Null);

            var serialized = new SerializedObject(_inputManager);
            serialized.FindProperty("_inputActionsTemplate").objectReferenceValue = template;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            _inputManager.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        public override void TearDown()
        {
            if (_inputManager != null && _inputManager.IsInitialized)
            {
                _inputManager.ShutdownAsync(CancellationToken.None)
                    .GetAwaiter().GetResult();
            }

            if (_gameObject != null)
                UnityEngine.Object.DestroyImmediate(_gameObject);

            base.TearDown();
        }

        [Test]
        public void Initialize_LoadsPlayerPresetIntoRuntimeClone()
        {
            InputActionAsset template =
                AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);

            Assert.That(_inputManager.IsInitialized, Is.True);
            Assert.That(_inputManager.LastBindingLoadSucceeded, Is.True);
            Assert.That(_inputManager.RuntimeInputActions, Is.Not.SameAs(template));
            Assert.That(_inputManager.CurrentContext, Is.EqualTo(RuntimeInputContext.Disabled));
            Assert.That(_inputManager.IsGameplayInputEnabled, Is.False);
        }

        [Test]
        public void GameplayAndUIContexts_IsolateInputAndSupportNestedLeases()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            _inputManager.SetBaseContext(RuntimeInputContext.Gameplay);

            Press(keyboard.wKey);
            Assert.That(_inputManager.Move.y, Is.EqualTo(1f).Within(0.001f));

            IDisposable firstUI = _inputManager.AcquireContext(
                RuntimeInputContext.UI, new object());
            IDisposable secondUI = _inputManager.AcquireContext(
                RuntimeInputContext.UI, new object());

            Assert.That(_inputManager.CurrentContext, Is.EqualTo(RuntimeInputContext.UI));
            Assert.That(_inputManager.IsGameplayInputEnabled, Is.False);
            Assert.That(_inputManager.Move, Is.EqualTo(Vector2.zero));

            firstUI.Dispose();
            Assert.That(_inputManager.CurrentContext, Is.EqualTo(RuntimeInputContext.UI));

            secondUI.Dispose();
            Assert.That(_inputManager.CurrentContext, Is.EqualTo(RuntimeInputContext.Gameplay));
            Assert.That(_inputManager.IsGameplayInputEnabled, Is.True);
            InputSystem.Update();
            Assert.That(_inputManager.Move.y, Is.EqualTo(1f).Within(0.001f));

            Release(keyboard.wKey);
        }

        [Test]
        public void BindingOverride_ChangesTheControlReadByRuntimeInput()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            _inputManager.SetBaseContext(RuntimeInputContext.Disabled);

            InputAction moveAction = _inputManager.RuntimeInputActions
                .FindActionMap("Gameplay", true)
                .FindAction("Move", true);
            int upBindingIndex = moveAction.bindings
                .Select((binding, index) => (binding, index))
                .First(item => item.binding.isPartOfComposite && item.binding.name == "up")
                .index;

            moveAction.ApplyBindingOverride(upBindingIndex, "<Keyboard>/upArrow");
            string overridesJson = _inputManager.SaveBindingOverridesAsJson();

            Assert.That(_inputManager.ApplyBindingOverrides(overridesJson), Is.True);
            Assert.That(moveAction.bindings[upBindingIndex].effectivePath,
                Is.EqualTo("<Keyboard>/upArrow"));

            _inputManager.SetBaseContext(RuntimeInputContext.Gameplay);
            Press(keyboard.upArrowKey);
            Assert.That(_inputManager.Move.y, Is.EqualTo(1f).Within(0.001f));
            Release(keyboard.upArrowKey);
        }

        [Test]
        public void CameraInput_UsesDiscreteRotationAndRespectsContextIsolation()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            _inputManager.SetBaseContext(RuntimeInputContext.Gameplay);

            Press(keyboard.qKey);
            Assert.That(_inputManager.CameraRotateStep, Is.EqualTo(-1f));
            Release(keyboard.qKey);

            Set(mouse.scroll, new Vector2(0f, 120f));
            Assert.That(_inputManager.CameraZoom.y, Is.EqualTo(120f).Within(0.001f));

            using (_inputManager.AcquireContext(RuntimeInputContext.UI, this))
            {
                Assert.That(_inputManager.CameraRotateStep, Is.Zero);
                Assert.That(_inputManager.CameraZoom, Is.EqualTo(Vector2.zero));
            }
        }
    }

    public sealed class InputManagerTestRunCallback : ICallbacks
    {
        public static bool Finished { get; private set; }
        public static string ResultState { get; private set; } = string.Empty;
        public static string Message { get; private set; } = string.Empty;
        public static string StackTrace { get; private set; } = string.Empty;
        public static int Passed { get; private set; }
        public static int Failed { get; private set; }
        public static int Skipped { get; private set; }
        public static string FailureDetails { get; private set; } = string.Empty;

        public static void Reset()
        {
            Finished = false;
            ResultState = string.Empty;
            Message = string.Empty;
            StackTrace = string.Empty;
            Passed = 0;
            Failed = 0;
            Skipped = 0;
            FailureDetails = string.Empty;
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            ResultState = result.ResultState;
            Message = result.Message ?? string.Empty;
            StackTrace = result.StackTrace ?? string.Empty;
            Passed = result.PassCount;
            Failed = result.FailCount;
            Skipped = result.SkipCount;
            Finished = true;
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (result.FailCount <= 0 || result.HasChildren)
                return;

            FailureDetails +=
                $"{result.FullName}: {result.Message}\n{result.StackTrace}\n";
        }
    }
}
