using NUnit.Framework;
using ProjectGame.HotFix.UI.Gameplay;
using ProjectGame.HotFix.UI.Gameplay.EffectRoll;
using ProjectGame.HotFix.UI.Gameplay.HUD;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectGame.HotFix.Tests.GameplayUI
{
    public sealed class GameplayUISceneTests
    {
        [Test]
        public void SavedScene_HasWiredCanvasLayers_AndDormantDedicatedEventSystem()
        {
            const string path = "Assets/_HotUpdate/Scenes/UIGameUIScene.unity";
            Scene active = SceneManager.GetActiveScene();
            Scene scene = SceneManager.GetSceneByPath(path);
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                GameplayUIManager manager = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foreach (GameplayUIManager candidate in root.GetComponentsInChildren<GameplayUIManager>(true))
                    {
                        Assert.That(manager, Is.Null, "UI 场景中只能有一个 Manager");
                        manager = candidate;
                    }
                }
                Assert.That(manager, Is.Not.Null);
                var serialized = new SerializedObject(manager);
                int previousOrder = int.MinValue;
                foreach (string property in new[] { "_hudRoot", "_screenRoot", "_modalRoot" })
                {
                    var layer = serialized.FindProperty(property).objectReferenceValue as RectTransform;
                    Assert.That(layer, Is.Not.Null, property);
                    Assert.That(layer.gameObject.scene, Is.EqualTo(scene));
                    Canvas canvas = layer.GetComponent<Canvas>();
                    Assert.That(canvas, Is.Not.Null);
                    Assert.That(canvas.overrideSorting, Is.True);
                    Assert.That(canvas.sortingOrder, Is.GreaterThan(previousOrder));
                    previousOrder = canvas.sortingOrder;
                }
                var module = serialized.FindProperty("_inputModule").objectReferenceValue as InputSystemUIInputModule;
                Assert.That(module, Is.Not.Null);
                Assert.That(module.gameObject.scene, Is.EqualTo(scene));
                Assert.That(module.enabled, Is.False);
                Assert.That(module.GetComponent<EventSystem>().enabled, Is.False);
                Assert.That(module.actionsAsset, Is.Null, "输入应在运行时绑定到 InputManager 克隆");
            }
            finally
            {
                if (active.IsValid() && active.isLoaded) SceneManager.SetActiveScene(active);
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void SavedScene_HasRegisteredHud_WithGnuFontAndThreeHealthLayers()
        {
            const string path = "Assets/_HotUpdate/Scenes/UIGameUIScene.unity";
            const string fontPath = "Assets/_HotUpdate/Fonts/GNUUnifont9FullHintInstrUCSUR SDF.asset";
            var expectedFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            Scene active = SceneManager.GetActiveScene();
            Scene scene = SceneManager.GetSceneByPath(path);
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                GameplayUIManager manager = null;
                GameplayHUDPresenter presenter = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    manager ??= root.GetComponentInChildren<GameplayUIManager>(true);
                    presenter ??= root.GetComponentInChildren<GameplayHUDPresenter>(true);
                }
                Assert.That(manager, Is.Not.Null);
                Assert.That(presenter, Is.Not.Null);
                var pso = new SerializedObject(presenter);
                var view = pso.FindProperty("_view").objectReferenceValue as GameplayHUDView;
                Assert.That(view, Is.Not.Null);
                Assert.That(view.transform.parent.name, Is.EqualTo("HUDLayer"));
                var vso = new SerializedObject(view);
                var local = vso.FindProperty("_localHealth").objectReferenceValue as HUDHealthBarView;
                var remote = vso.FindProperty("_remoteHealthPrefab").objectReferenceValue as HUDHealthBarView;
                Assert.That(local, Is.Not.Null);
                Assert.That(remote, Is.Not.Null);
                Assert.That(((RectTransform)local.transform).rect.width,
                    Is.GreaterThan(((RectTransform)remote.transform).rect.width));
                foreach (string field in new[] { "_healthFill", "_bufferFill", "_shieldFill" })
                    Assert.That(new SerializedObject(local).FindProperty(field).objectReferenceValue, Is.TypeOf<Image>());
                foreach (TMP_Text text in view.GetComponentsInChildren<TMP_Text>(true))
                    Assert.That(text.font, Is.EqualTo(expectedFont), text.name);
                var mso = new SerializedObject(manager);
                var list = mso.FindProperty("_presenters");
                bool found = false;
                for (int i = 0; i < list.arraySize; i++)
                    found |= list.GetArrayElementAtIndex(i).objectReferenceValue == presenter;
                Assert.That(found, Is.True);
            }
            finally
            {
                if (active.IsValid() && active.isLoaded) SceneManager.SetActiveScene(active);
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void SavedScene_HasRegisteredEffectRollPresenter_WithThreePrefabCards()
        {
            const string scenePath = "Assets/_HotUpdate/Scenes/UIGameUIScene.unity";
            const string cardPath =
                "Assets/_HotUpdate/Prefabs/UI/Gameplay/EffectRoll/EffectRollCard.prefab";
            const string fontPath =
                "Assets/_HotUpdate/Fonts/GNUUnifont9FullHintInstrUCSUR SDF.asset";
            GameObject cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(cardPath);
            TMP_FontAsset expectedFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            Assert.That(cardPrefab, Is.Not.Null);
            Assert.That(expectedFont, Is.Not.Null);
            Assert.That(cardPrefab.GetComponent<EffectRollCardView>(), Is.Not.Null);
            Button cardButton = cardPrefab.GetComponentInChildren<Button>(true);
            Assert.That(cardButton, Is.Not.Null);
            foreach (Graphic graphic in cardPrefab.GetComponentsInChildren<Graphic>(true))
            {
                if (graphic == cardButton.targetGraphic)
                    Assert.That(graphic.raycastTarget, Is.True, "Button 图像必须接收卡片点击");
                else
                    Assert.That(graphic.raycastTarget, Is.False,
                        $"{graphic.name} 只能展示，不能遮挡卡片 Button");
            }
            foreach (TMP_Text text in cardPrefab.GetComponentsInChildren<TMP_Text>(true))
                Assert.That(text.font, Is.EqualTo(expectedFont));
            foreach (Component component in cardPrefab.GetComponents<Component>())
                Assert.That(component == null || component.GetType().Name != "ModifierUICard", Is.True,
                    "新卡片不能继续依赖旧 ModifierUICard");

            Scene active = SceneManager.GetActiveScene();
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                GameplayUIManager manager = null;
                EffectRollPresenter presenter = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    manager ??= root.GetComponentInChildren<GameplayUIManager>(true);
                    presenter ??= root.GetComponentInChildren<EffectRollPresenter>(true);
                }
                Assert.That(manager, Is.Not.Null);
                Assert.That(presenter, Is.Not.Null);
                Assert.That(presenter.Id, Is.EqualTo(GameplayUIId.EffectRoll));

                var presenterSo = new SerializedObject(presenter);
                var view = presenterSo.FindProperty("_view").objectReferenceValue as EffectRollView;
                Assert.That(view, Is.Not.Null);
                Assert.That(view.transform.parent.name, Is.EqualTo("ScreenLayer"));
                foreach (TMP_Text text in view.GetComponentsInChildren<TMP_Text>(true))
                    Assert.That(text.font, Is.EqualTo(expectedFont), text.name);

                var viewSo = new SerializedObject(view);
                var cards = viewSo.FindProperty("_cards");
                Assert.That(cards.arraySize, Is.EqualTo(3));
                for (int i = 0; i < cards.arraySize; i++)
                {
                    var card = cards.GetArrayElementAtIndex(i).objectReferenceValue as EffectRollCardView;
                    Assert.That(card, Is.Not.Null);
                    Object source = PrefabUtility.GetCorrespondingObjectFromSource(card.gameObject);
                    Assert.That(AssetDatabase.GetAssetPath(source), Is.EqualTo(cardPath));
                }

                var managerSo = new SerializedObject(manager);
                var registered = managerSo.FindProperty("_presenters");
                bool found = false;
                for (int i = 0; i < registered.arraySize; i++)
                    found |= registered.GetArrayElementAtIndex(i).objectReferenceValue == presenter;
                Assert.That(found, Is.True, "EffectRollPresenter 必须注册到 GameplayUIManager");
            }
            finally
            {
                if (active.IsValid() && active.isLoaded) SceneManager.SetActiveScene(active);
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
