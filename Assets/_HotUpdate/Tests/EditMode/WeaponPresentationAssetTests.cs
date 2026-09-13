using NUnit.Framework;
using ProjectGame.HotFix.Character;
using ProjectGame.HotFix.Gameplay.Player;
using ProjectGame.HotFix.Gameplay.Runtime;
using ProjectGame.HotFix.Gameplay.Weapon.Presentation;
using UnityEditor;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Tests
{
    public sealed class WeaponPresentationAssetTests
    {
        private const string BulletPrefabPath =
            "Assets/_HotUpdate/Prefabs/Weapon/Presentation/Bullet.prefab";

        [Test]
        public void BulletPrefab_IsColliderFreeLocalTracer()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BulletPrefabPath);

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<WeaponProjectileView>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<TrailRenderer>(), Is.Not.Null);
            Assert.That(prefab.GetComponentsInChildren<Collider>(true), Is.Empty);
        }

        [TestCase("Character_Weapon_00")]
        [TestCase("Character_Weapon_01")]
        [TestCase("Character_Weapon_02")]
        [TestCase("Character_Weapon_03")]
        public void WeaponPrefab_HasAllPresentationAnchorsAndEffects(string prefabName)
        {
            string path =
                $"Assets/_HotUpdate/Prefabs/LobbyScene/Character_Weapon/{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Component weaponView = prefab != null ? prefab.GetComponent("WeaponView") : null;

            Assert.That(prefab, Is.Not.Null, path);
            Assert.That(weaponView, Is.Not.Null, path);

            var serializedView = new SerializedObject(weaponView);
            AssertReference(serializedView, "_shellEjectionPoint", path);
            AssertReference(serializedView, "_reloadVfxPoint", path);
            AssertReference(serializedView, "_muzzleFlashVfx", path);
            AssertReference(serializedView, "_shellEjectionVfx", path);
            AssertReference(serializedView, "_reloadVfx", path);
        }

        [TestCase("Character_Weapon_00")]
        [TestCase("Character_Weapon_01")]
        [TestCase("Character_Weapon_02")]
        [TestCase("Character_Weapon_03")]
        public void ShellEjection_AppendsParticlesWithoutClearingPreviousShell(string prefabName)
        {
            string path =
                $"Assets/_HotUpdate/Prefabs/LobbyScene/Character_Weapon/{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject instance = Object.Instantiate(prefab);
            try
            {
                WeaponView weaponView = instance.GetComponent<WeaponView>();
                var serializedView = new SerializedObject(weaponView);
                var particle = serializedView.FindProperty("_shellEjectionVfx")
                    .objectReferenceValue as ParticleSystem;
                Assert.That(particle, Is.Not.Null, path);

                weaponView.StopAllWeaponVFX();
                weaponView.PlayShellEjectionVFX();
                Assert.That(particle.particleCount, Is.EqualTo(1));
                weaponView.PlayShellEjectionVFX();
                Assert.That(particle.particleCount, Is.EqualTo(2),
                    "第二枪应追加弹壳，不能清空第一枪仍存活的弹壳粒子");
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void RuntimePrefabs_ContainAndInitializeWeaponPresentationBridge()
        {
            GameObject player = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_HotUpdate/Prefabs/Character/PlayerRuntimeRoot.prefab");
            Assert.That(player, Is.Not.Null);
            Assert.That(player.GetComponent<PlayerWeaponPresentationController>(), Is.Not.Null);

            GameObject gameRoot = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_HotUpdate/Prefabs/Network/GameRoot.prefab");
            Assert.That(gameRoot, Is.Not.Null);
            WeaponPresentationService presentation =
                gameRoot.GetComponentInChildren<WeaponPresentationService>(true);
            GameRuntimeBootstrap bootstrap = gameRoot.GetComponentInChildren<GameRuntimeBootstrap>(true);
            Assert.That(presentation, Is.Not.Null);
            Assert.That(bootstrap, Is.Not.Null);

            var serializedBootstrap = new SerializedObject(bootstrap);
            SerializedProperty services = serializedBootstrap.FindProperty("_runtimeServiceComponents");
            bool registered = false;
            for (int i = 0; i < services.arraySize; i++)
            {
                if (services.GetArrayElementAtIndex(i).objectReferenceValue == presentation)
                {
                    registered = true;
                    break;
                }
            }

            Assert.That(registered, Is.True,
                "WeaponPresentationService 必须进入 GameRuntimeBootstrap 的统一生命周期。");
        }

        private static void AssertReference(SerializedObject target, string propertyName, string path)
        {
            SerializedProperty property = target.FindProperty(propertyName);
            Assert.That(property, Is.Not.Null, $"{path}: missing {propertyName}");
            Assert.That(property.objectReferenceValue, Is.Not.Null, $"{path}: {propertyName} is null");
        }
    }
}
