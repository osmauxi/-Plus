using NUnit.Framework;
using ProjectGame.HotFix.UI.Gameplay;
using UnityEditor;
using UnityEngine;

namespace ProjectGame.HotFix.Tests.GameplayUI
{
    public sealed class GameplayUIPresenterTests
    {
        [Test]
        public void CoverAndRestore_CancelVisibleWork_PreserveOpenSession_AndRefreshData()
        {
            var root = new GameObject("Presenter lifecycle");
            try
            {
                var view = root.AddComponent<TestGameplayView>();
                var presenter = root.AddComponent<TestGameplayPresenter>();
                var serialized = new SerializedObject(presenter);
                serialized.FindProperty("_view").objectReferenceValue = view;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                IGameplayUIPresenter lifecycle = presenter;
                lifecycle.Initialize(new Navigation());
                lifecycle.Initialize(new Navigation());
                Assert.That(root.GetComponent<CanvasGroup>().alpha, Is.Zero);
                lifecycle.SetPresentation(true, true, true);
                var firstToken = presenter.CurrentVisibleToken;
                Assert.That(firstToken.IsCancellationRequested, Is.False);
                lifecycle.SetPresentation(true, false, false);
                Assert.That(firstToken.IsCancellationRequested, Is.True);
                Assert.That(presenter.IsOpen, Is.True);
                Assert.That(root.activeSelf, Is.True);
                Assert.That(root.GetComponent<CanvasGroup>().blocksRaycasts, Is.False);
                lifecycle.SetPresentation(true, true, true);
                Assert.That(presenter.RenderCount, Is.EqualTo(2));
                Assert.That(presenter.OpenCount, Is.EqualTo(1));
                Assert.That(presenter.InitializeCount, Is.EqualTo(1));
                var secondToken = presenter.CurrentVisibleToken;
                lifecycle.Shutdown();
                lifecycle.Shutdown();
                Assert.That(secondToken.IsCancellationRequested, Is.True);
                Assert.That(presenter.CloseCount, Is.EqualTo(1));
                Assert.That(presenter.ShutdownCount, Is.EqualTo(1));
                Assert.That(root.GetComponent<CanvasGroup>().alpha, Is.Zero);
                Assert.That(root.GetComponent<CanvasGroup>().interactable, Is.False);
            }
            finally { Object.DestroyImmediate(root); }
        }

        private sealed class Navigation : IGameplayUINavigation
        {
            public bool Show(GameplayUIId id) => true;
            public bool Hide(GameplayUIId id) => true;
            public bool Toggle(GameplayUIId id) => true;
            public bool TryNavigateBack() => true;
        }
    }
}
