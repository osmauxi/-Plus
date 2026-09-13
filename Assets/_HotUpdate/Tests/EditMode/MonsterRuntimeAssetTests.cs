using NUnit.Framework;
using ProjectGame.HotFix.Gameplay.Combat.Health;
using ProjectGame.HotFix.Gameplay.Monsters;
using ProjectGame.HotFix.Gameplay.Network;
using ProjectGame.HotFix.Gameplay.Runtime;
using ProjectGame.HotFix.Gameplay.Spawning;
using ProjectGame.HotFix.Gameplay.Weapon;
using UnityEditor;
using UnityEngine;

namespace ProjectGame.HotFix.Tests.EditMode
{
    public sealed class MonsterRuntimeAssetTests
    {
        private const string GameRootPath = "Assets/_HotUpdate/Prefabs/Network/GameRoot.prefab";

        [Test]
        public void GameRoot_RegistersMonsterBeforeWeaponAndPlayerHealth()
        {
            GameObject gameRoot = AssetDatabase.LoadAssetAtPath<GameObject>(GameRootPath);
            Assert.That(gameRoot, Is.Not.Null, GameRootPath);

            GameRuntimeBootstrap bootstrap = gameRoot.GetComponentInChildren<GameRuntimeBootstrap>(true);
            GameNetworkRuntime network = gameRoot.GetComponentInChildren<GameNetworkRuntime>(true);
            MonsterRuntimeService monster = gameRoot.GetComponentInChildren<MonsterRuntimeService>(true);
            MonsterBattleManager battleManager = gameRoot.GetComponentInChildren<MonsterBattleManager>(true);
            WeaponRuntimeService weapon = gameRoot.GetComponentInChildren<WeaponRuntimeService>(true);
            GameLevelFlowController levelFlow = gameRoot.GetComponentInChildren<GameLevelFlowController>(true);
            PlayerHealthRuntimeService playerHealth =
                gameRoot.GetComponentInChildren<PlayerHealthRuntimeService>(true);
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(network, Is.Not.Null);
            Assert.That(monster, Is.Not.Null);
            Assert.That(battleManager, Is.Not.Null);
            Assert.That(weapon, Is.Not.Null);
            Assert.That(playerHealth, Is.Not.Null);
            Assert.That(levelFlow, Is.Not.Null);

            var serializedMonster = new SerializedObject(monster);
            Assert.That(serializedMonster.FindProperty("_weaponRuntimeService").objectReferenceValue,
                Is.SameAs(weapon), "Monster 必须在 Weapon 初始化前注入 Projectile Resolver。");
            var serializedBattleManager = new SerializedObject(battleManager);
            Assert.That(serializedBattleManager.FindProperty("_monsterRuntimeService").objectReferenceValue,
                Is.SameAs(monster));

            var serializedBootstrap = new SerializedObject(bootstrap);
            SerializedProperty services = serializedBootstrap.FindProperty("_runtimeServiceComponents");
            int networkIndex = FindIndex(services, network);
            int monsterIndex = FindIndex(services, monster);
            int weaponIndex = FindIndex(services, weapon);
            int playerHealthIndex = FindIndex(services, playerHealth);
            int managerIndex = FindIndex(services, battleManager);
            int levelFlowIndex = FindIndex(services, levelFlow);

            Assert.That(monsterIndex, Is.GreaterThan(networkIndex));
            Assert.That(monsterIndex, Is.LessThan(weaponIndex));
            Assert.That(monsterIndex, Is.LessThan(playerHealthIndex));
            Assert.That(Count(services, monster), Is.EqualTo(1));
            Assert.That(managerIndex, Is.GreaterThan(playerHealthIndex));
            Assert.That(managerIndex, Is.LessThan(levelFlowIndex));
            Assert.That(Count(services, battleManager), Is.EqualTo(1));
        }

        [TestCase("Room_Grid_Start")]
        [TestCase("Room_Grid_Monster")]
        [TestCase("Room_Grid_Boss")]
        public void RoomPrefab_ProvidesGroundSamplingRegion(string prefabName)
        {
            string path = $"Assets/_HotUpdate/Prefabs/Rooms/{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, path);
            var roomView = prefab.GetComponentInChildren<ProjectGame.HotFix.Gameplay.Map.View.RoomView>(true);
            Assert.That(roomView, Is.Not.Null, path);
            RoomSpawnRegion region = roomView.SpawnRegion;
            Assert.That(region.LocalSize.x, Is.GreaterThan(0));
            Assert.That(region.LocalSize.y, Is.GreaterThan(0));
            Assert.That(region.LocalSize.z, Is.GreaterThan(0));
            Assert.That(region.ProbeMask.value, Is.Not.Zero);
            Assert.That(region.GroundMask.value, Is.Not.Zero);
        }

        private static int FindIndex(SerializedProperty array, Object target)
        {
            for (int i = 0; i < array.arraySize; i++)
            {
                if (array.GetArrayElementAtIndex(i).objectReferenceValue == target) return i;
            }
            return -1;
        }

        private static int Count(SerializedProperty array, Object target)
        {
            int count = 0;
            for (int i = 0; i < array.arraySize; i++)
            {
                if (array.GetArrayElementAtIndex(i).objectReferenceValue == target) count++;
            }
            return count;
        }
    }
}
