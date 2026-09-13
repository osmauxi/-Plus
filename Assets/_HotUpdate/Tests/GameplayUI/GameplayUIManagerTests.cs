using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;
using ProjectGame.HotFix.Core.Events;
using ProjectGame.HotFix.Gameplay.Events;
using ProjectGame.HotFix.Gameplay.Input;
using ProjectGame.HotFix.Gameplay.State;
using ProjectGame.HotFix.UI.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectGame.HotFix.Tests.GameplayUI
{
    public sealed class GameplayUIManagerTests : InputTestFixture
    {
        private const string InputPath = "Assets/_HotUpdate/Resources/Input/GameplayInputActions.inputactions";
        private readonly List<GameObject> _objects = new();
        private IEventBus _previousBus;
        private InputManager _input;
        private GameplayUIManager _manager;
        private Probe _hud, _status, _roll, _settings;

        public override void Setup()
        {
            base.Setup();
            _previousBus = LocalEvents.Bus;
            LocalEvents.SetBus(new LocalEventBus("GameplayUITests"));
            _input = Create("Input").AddComponent<InputManager>();
            var serialized = new SerializedObject(_input);
            serialized.FindProperty("_inputActionsTemplate").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _input.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
            _input.ApplyBindingOverrides(string.Empty);
            _input.SetBaseContext(InputContext.Gameplay);
            _manager = Create("Manager").AddComponent<GameplayUIManager>();
            _hud = new Probe(GameplayUIId.GameplayHUD);
            _status = new Probe(GameplayUIId.PlayerStatus);
            _roll = new Probe(GameplayUIId.EffectRoll);
            _settings = new Probe(GameplayUIId.Settings);
            Initialize();
        }

        public override void TearDown()
        {
            if (_manager != null) _manager.Shutdown();
            if (_input != null) _input.ShutdownAsync(CancellationToken.None).GetAwaiter().GetResult();
            for (int i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
            _objects.Clear();
            LocalEvents.SetBus(_previousBus);
            base.TearDown();
        }

        private GameObject Create(string name)
        {
            var value = new GameObject(name);
            _objects.Add(value);
            return value;
        }

        private void Initialize()
        {
            _manager.Initialize(new[] { _hud, _status, _roll, _settings }, _input);
            _manager.SetGameplayActive(true);
        }

        [Test]
        public void EnterGameplay_ShowsOnlyHud_WithoutBlockingInput()
        {
            Assert.That(_hud.IsVisible, Is.True);
            Assert.That(_hud.IsInteractive, Is.False);
            Assert.That(_manager.TopUI, Is.EqualTo(GameplayUIId.None));
            Assert.That(_status.IsOpen || _roll.IsOpen || _settings.IsOpen, Is.False);
            Assert.That(_input.CurrentContext, Is.EqualTo(InputContext.Gameplay));
        }

        [Test]
        public void RepeatedShow_DoesNotDuplicateStackOrLifecycle()
        {
            Assert.That(_manager.Show(GameplayUIId.PlayerStatus), Is.True);
            Assert.That(_manager.Show(GameplayUIId.PlayerStatus), Is.True);
            Assert.That(_status.OpenCount, Is.EqualTo(1));
            Assert.That(_status.InitializeCount, Is.EqualTo(1));
            Assert.That(_manager.CloseTop(), Is.True);
            Assert.That(_manager.TopUI, Is.EqualTo(GameplayUIId.None));
            Assert.That(_input.CurrentContext, Is.EqualTo(InputContext.Gameplay));
        }

        [Test]
        public void CoverScreen_KeepsOpenSession_AndRestoresSamePresenter()
        {
            _manager.Show(GameplayUIId.PlayerStatus);
            _manager.Show(GameplayUIId.EffectRoll);
            Assert.That(_status.IsOpen, Is.True);
            Assert.That(_status.IsVisible, Is.False);
            Assert.That(_hud.IsOpen, Is.True);
            Assert.That(_hud.IsVisible, Is.False);
            _manager.Hide(GameplayUIId.EffectRoll);
            Assert.That(_status.IsVisible && _status.IsInteractive, Is.True);
            Assert.That(_status.OpenCount, Is.EqualTo(1));
            Assert.That(_hud.IsVisible, Is.True);
        }

        [Test]
        public void Modal_BlocksUnderlyingInteraction_AndKeepsUnderlyingInputLease()
        {
            _manager.Show(GameplayUIId.PlayerStatus);
            _manager.Show(GameplayUIId.Settings);
            Assert.That(_status.IsVisible, Is.True);
            Assert.That(_status.IsInteractive, Is.False);
            Assert.That(_settings.IsInteractive, Is.True);
            _manager.TryNavigateBack();
            Assert.That(_status.IsInteractive, Is.True);
            Assert.That(_input.CurrentContext, Is.EqualTo(InputContext.UI));
            _manager.TryNavigateBack();
            Assert.That(_input.CurrentContext, Is.EqualTo(InputContext.Gameplay));
        }

        [Test]
        public void ClosingCoveredScreen_DoesNotResurrectItLater()
        {
            _manager.Show(GameplayUIId.PlayerStatus);
            _manager.Show(GameplayUIId.EffectRoll);
            _manager.Hide(GameplayUIId.PlayerStatus);
            _manager.Hide(GameplayUIId.EffectRoll);
            Assert.That(_status.IsOpen, Is.False);
            Assert.That(_manager.TopUI, Is.EqualTo(GameplayUIId.None));
            Assert.That(_input.CurrentContext, Is.EqualTo(InputContext.Gameplay));
        }

        [Test]
        public void RequiredRoll_CannotToggleOrBackAway_ButCanOpenSettingsAndBeClosedByBusiness()
        {
            _manager.Show(GameplayUIId.EffectRoll);
            Assert.That(_manager.Toggle(GameplayUIId.EffectRoll), Is.False);
            Assert.That(_manager.CloseTop(), Is.False);
            _manager.TryNavigateBack();
            Assert.That(_manager.TopUI, Is.EqualTo(GameplayUIId.Settings));
            Assert.That(_roll.IsOpen, Is.True);
            _manager.TryNavigateBack();
            Assert.That(_manager.TopUI, Is.EqualTo(GameplayUIId.EffectRoll));
            Assert.That(_roll.IsInteractive, Is.True);
            Assert.That(_manager.Hide(GameplayUIId.EffectRoll), Is.True);
        }

        [Test]
        public void PresenterConsumesBack_BeforeManagerClosesItsPage()
        {
            _manager.Show(GameplayUIId.PlayerStatus);
            _status.ConsumeBack = true;
            Assert.That(_manager.TryNavigateBack(), Is.True);
            Assert.That(_status.IsOpen, Is.True);
            _status.ConsumeBack = false;
            _manager.TryNavigateBack();
            Assert.That(_status.IsOpen, Is.False);
        }

        [Test]
        public void ExternalInputLease_IsNotReleasedWhenOurPagesClose()
        {
            using (_input.AcquireContext(InputContext.UI, this))
            {
                _manager.Show(GameplayUIId.Settings);
                _manager.Hide(GameplayUIId.Settings);
                Assert.That(_input.CurrentContext, Is.EqualTo(InputContext.UI));
            }
            Assert.That(_input.CurrentContext, Is.EqualTo(InputContext.Gameplay));
        }

        [Test]
        public void GameplayStateExit_ClearsAllPages_AndNextEntryStartsWithHud()
        {
            _manager.Show(GameplayUIId.EffectRoll);
            _manager.Show(GameplayUIId.Settings);
            LocalEvents.Publish(new GameStateChangedEvent(GameState.GamePlaying, GameState.MapGenerating));
            Assert.That(_hud.IsOpen || _status.IsOpen || _roll.IsOpen || _settings.IsOpen, Is.False);
            Assert.That(_input.CurrentContext, Is.EqualTo(InputContext.Disabled));
            Assert.That(_manager.Show(GameplayUIId.Settings), Is.False);
            LocalEvents.Publish(new GameStateChangedEvent(GameState.MapGenerating, GameState.GamePlaying));
            Assert.That(_hud.IsVisible, Is.True);
            Assert.That(_manager.TopUI, Is.EqualTo(GameplayUIId.None));
            Assert.That(_input.CurrentContext, Is.EqualTo(InputContext.Gameplay));
        }

        [Test]
        public void ExplicitHudHide_SurvivesCoverRestore_UntilResetRequested()
        {
            _manager.Hide(GameplayUIId.GameplayHUD);
            _manager.Show(GameplayUIId.EffectRoll);
            _manager.Hide(GameplayUIId.EffectRoll);
            Assert.That(_hud.IsVisible, Is.False);
            _manager.ResetToGameplay();
            Assert.That(_hud.IsVisible, Is.True);
        }

        [Test]
        public void LocalRequestFacade_UsesSameRouting_AndUnsubscribesOnShutdown()
        {
            GameplayUIRequests.Show(GameplayUIId.PlayerStatus);
            Assert.That(_status.IsInteractive, Is.True);
            _manager.Shutdown();
            GameplayUIRequests.Show(GameplayUIId.Settings);
            Assert.That(_settings.IsOpen, Is.False);
            Assert.That(LocalEvents.GetSubscriberCount<GameplayUIRequest>(), Is.Zero);
            Assert.That(_input.CurrentContext, Is.EqualTo(InputContext.Gameplay));
            _manager.Shutdown();
            Assert.That(_status.ShutdownCount, Is.EqualTo(1));
        }

        [Test]
        public void DuplicateAndMissingRegistrations_FailBeforeInitializingAnyModule()
        {
            _manager.Shutdown();
            var a = new Probe(GameplayUIId.PlayerStatus);
            var b = new Probe(GameplayUIId.PlayerStatus);
            Assert.Throws<InvalidOperationException>(() => _manager.Initialize(new[] { a, b }, _input));
            Assert.That(a.InitializeCount, Is.Zero);
            Assert.That(_manager.IsInitialized, Is.False);
            Assert.Throws<InvalidOperationException>(() => _manager.Initialize(new Probe[] { null }, _input));
            Assert.That(_input.CurrentContext, Is.EqualTo(InputContext.Gameplay));
        }

        [Test]
        public void EmptyFramework_RejectsUnregisteredPages_WithoutTakingInput()
        {
            _manager.Shutdown();
            _manager.Initialize(Array.Empty<IGameplayUIPresenter>(), _input);
            _manager.SetGameplayActive(true);
            Assert.That(_manager.Show(GameplayUIId.Settings), Is.False);
            Assert.That(_manager.Hide(GameplayUIId.None), Is.False);
            Assert.That(_manager.TryNavigateBack(), Is.False);
            Assert.That(_input.CurrentContext, Is.EqualTo(InputContext.Gameplay));
        }

        [Test]
        public void NavigationFromLifecycleCallback_IsDeferredUntilCurrentTransitionCompletes()
        {
            _status.OnShow = () => _manager.Show(GameplayUIId.Settings);
            _manager.Show(GameplayUIId.PlayerStatus);
            Assert.That(_manager.TopUI, Is.EqualTo(GameplayUIId.Settings));
            Assert.That(_status.IsVisible, Is.True);
            Assert.That(_status.IsInteractive, Is.False);
            _manager.Hide(GameplayUIId.Settings);
            Assert.That(_status.IsInteractive, Is.True);
        }

        [Test]
        public void FailingPresenter_ReleasesInputAndShutsDownOtherPresenters()
        {
            _status.OnShow = () => throw new InvalidOperationException("Expected UI presenter failure");
            LogAssert.Expect(LogType.Exception, new Regex("Expected UI presenter failure"));
            Assert.That(_manager.Show(GameplayUIId.PlayerStatus), Is.False);
            Assert.That(_manager.IsInitialized, Is.False);
            Assert.That(_hud.IsVisible, Is.False);
            Assert.That(_input.CurrentContext, Is.EqualTo(InputContext.Gameplay));
        }

        [Test]
        public void MenuActions_WorkInGameplayAndUI_AndRespectDisabledContext()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Press(keyboard.escapeKey);
            Assert.That(_input.MenuBackPressedThisFrame, Is.True);
            Release(keyboard.escapeKey);
            _manager.Show(GameplayUIId.Settings);
            Press(keyboard.tabKey);
            Assert.That(_input.PlayerStatusPressedThisFrame, Is.True);
            Release(keyboard.tabKey);
            using (_input.AcquireContext(InputContext.Disabled, this))
            {
                Press(keyboard.escapeKey);
                Assert.That(_input.MenuBackPressedThisFrame, Is.False);
                Assert.That(_manager.Show(GameplayUIId.PlayerStatus), Is.False);
                Release(keyboard.escapeKey);
            }
            Press(keyboard.escapeKey);
            Assert.That(_input.MenuBackPressedThisFrame, Is.True);
        }

        [Test]
        public void OldInputLease_CannotReleaseNewLeaseAfterReinitialization()
        {
            IDisposable old = _input.AcquireContext(InputContext.UI, this);
            _input.ShutdownAsync(CancellationToken.None).GetAwaiter().GetResult();
            _input.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
            _input.SetBaseContext(InputContext.Gameplay);
            IDisposable current = _input.AcquireContext(InputContext.UI, this);
            old.Dispose();
            Assert.That(_input.CurrentContext, Is.EqualTo(InputContext.UI));
            current.Dispose();
            Assert.That(_input.CurrentContext, Is.EqualTo(InputContext.Gameplay));
        }

        [Test]
        public void EventSystem_UsesRuntimeClone_AndDoesNotDoubleRouteCancel()
        {
            _manager.Shutdown();
            GameObject eventObject = Create("UI EventSystem");
            eventObject.SetActive(false);
            eventObject.AddComponent<EventSystem>();
            var module = eventObject.AddComponent<InputSystemUIInputModule>();
            module.enabled = false;
            var serialized = new SerializedObject(_manager);
            serialized.FindProperty("_inputModule").objectReferenceValue = module;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Initialize();
            eventObject.SetActive(true);
            _manager.Show(GameplayUIId.Settings);
            Assert.That(module.actionsAsset, Is.SameAs(_input.RuntimeInputActions));
            Assert.That(module.point.action, Is.SameAs(_input.RuntimeInputActions.FindAction("UI/Point")));
            Assert.That(module.cancel, Is.Null);
            Assert.That(module.enabled, Is.True);
            _manager.Hide(GameplayUIId.Settings);
            Assert.That(module.enabled, Is.False);
            Assert.That(_input.IsGameplayInputEnabled, Is.True);
            Assert.That(module.GetComponent<EventSystem>().enabled, Is.False);
        }

        [Test]
        public void ExternalDisabledContext_SuspendsAndRestoresPageInteractionOnUpdate()
        {
            _manager.Show(GameplayUIId.PlayerStatus);
            var tick = typeof(GameplayUIManager).GetMethod("LateUpdate",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            using (_input.AcquireContext(InputContext.Disabled, this))
            {
                tick.Invoke(_manager, null);
                Assert.That(_status.IsVisible, Is.True);
                Assert.That(_status.IsInteractive, Is.False);
            }
            tick.Invoke(_manager, null);
            Assert.That(_status.IsInteractive, Is.True);
            Assert.That(_input.CurrentContext, Is.EqualTo(InputContext.UI));
        }

        [Test]
        public void VisibilityNotifications_ObserveFullyCommittedPageState()
        {
            _manager.Show(GameplayUIId.PlayerStatus);
            bool notified = false;
            _manager.VisibilityChanged += data =>
            {
                if (data.Id != GameplayUIId.GameplayHUD || data.IsVisible) return;
                notified = true;
                Assert.That(_roll.IsVisible, Is.True);
                Assert.That(_status.IsVisible, Is.False);
            };
            _manager.Show(GameplayUIId.EffectRoll);
            Assert.That(notified, Is.True);
        }

        private sealed class Probe : IGameplayUIPresenter
        {
            public GameplayUIId Id { get; }
            public GameplayUIPolicy Policy => GameplayUIPolicy.For(Id);
            public bool IsInitialized { get; private set; }
            public bool IsOpen { get; private set; }
            public bool IsVisible { get; private set; }
            public bool IsInteractive { get; private set; }
            public int InitializeCount, OpenCount, ShutdownCount;
            public bool ConsumeBack;
            public Action OnShow;

            public Probe(GameplayUIId id) => Id = id;
            public void Initialize(IGameplayUINavigation navigation)
            {
                if (IsInitialized) return;
                IsInitialized = true;
                InitializeCount++;
            }
            public void SetPresentation(bool open, bool visible, bool interactive)
            {
                bool newlyVisible = visible && !IsVisible;
                if (open && !IsOpen) OpenCount++;
                IsOpen = open;
                IsVisible = visible;
                IsInteractive = interactive;
                if (newlyVisible) OnShow?.Invoke();
            }
            public void Refresh() { }
            public bool TryHandleBackRequest() => ConsumeBack;
            public void Shutdown()
            {
                if (!IsInitialized) return;
                IsInitialized = IsOpen = IsVisible = IsInteractive = false;
                ShutdownCount++;
            }
        }
    }
}
