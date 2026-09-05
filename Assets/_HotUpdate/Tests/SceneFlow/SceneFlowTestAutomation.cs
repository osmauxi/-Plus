#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using ProjectGame.HotFix.Network.Runtime;
using ProjectGame.HotFix.SceneFlow;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectGame.HotFix.Tests.SceneFlow
{
    public static class SceneFlowTestAutomation
    {
        private static TestRunnerApi _runner;

        [InitializeOnLoadMethod]
        private static void RegisterResults()
        {
            if (_runner != null) return;
            _runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            _runner.RegisterCallbacks(new Results());
        }

        [MenuItem("Tools/ProjectGame/Run SceneFlow Pipeline Tests")]
        public static void Run()
        {
            CreateFixtures();
            SceneFlowPipelineTests.CreateBootstrapFixture();
            RegisterResults();
            _runner.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.PlayMode,
                assemblyNames = new[] { "HotFix.SceneFlow.Tests" }
            }));
        }

        public static void CreateFixtures()
        {
            string folder = SceneFlowPipelineTests.Fixtures;
            Directory.CreateDirectory(folder);
            Scene original = SceneManager.GetActiveScene();
            foreach (string name in new[] { "PipelineLobby", "PipelineGame", "PipelineUI" })
            {
                if (File.Exists(folder + name + ".unity")) continue;
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                EditorSceneManager.SaveScene(scene, folder + name + ".unity");
                EditorSceneManager.CloseScene(scene, true);
            }
            SceneManager.SetActiveScene(original);
            Scene preview = EditorSceneManager.NewPreviewScene();
            try
            {
                var entries = new List<NetworkPrefabEntry>();
                foreach (string name in new[] { "Session", "Lobby", "Game" })
                {
                    var id = name == "Session" ? NetworkPrefabId.NetworkSessionRoot :
                        name == "Lobby" ? NetworkPrefabId.LobbyNetworkRoot : NetworkPrefabId.GameRuntimeNetworkRoot;
                    string path = folder + name + ".prefab";
                    if (!File.Exists(path))
                    {
                        var root = new GameObject("PipelineTest" + name);
                        SceneManager.MoveGameObjectToScene(root, preview);
                        NetworkObject networkObject = root.AddComponent<NetworkObject>();
                        networkObject.SceneMigrationSynchronization = false;
                        SceneFlowPipelineTests.Set(root.AddComponent<NetworkScopeMember>(), "_id", id);
                        if (name == "Session")
                        {
                            var flow = root.AddComponent<GameSceneFlowController>();
                            SceneFlowPipelineTests.Set(flow, "_lobbySceneAddress", folder + "PipelineLobby.unity");
                            SceneFlowPipelineTests.Set(flow, "_gameRuntimeSceneAddress", folder + "PipelineGame.unity");
                            SceneFlowPipelineTests.Set(flow, "_gameUISceneAddress", folder + "PipelineUI.unity");
                            SceneFlowPipelineTests.Set(flow, "_operationTimeoutSeconds", 4f);
                        }
                        else
                        {
                            root.AddComponent<ScopeLifecycleProbe>().Id = id;
                            var child = new GameObject("InactiveOptionalBinder");
                            child.transform.SetParent(root.transform);
                            child.AddComponent<ScopeBindOnlyProbe>();
                            child.SetActive(false);
                        }
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                        Object.DestroyImmediate(root);
                    }
                    var entry = new NetworkPrefabEntry();
                    SceneFlowPipelineTests.Set(entry, "_id", id);
                    SceneFlowPipelineTests.Set(entry, "_prefab", new AssetReferenceGameObject(AssetDatabase.AssetPathToGUID(path)));
                    SceneFlowPipelineTests.Set(entry, "_sceneMask", name == "Session" ?
                        NetworkSceneMask.Lobby | NetworkSceneMask.GameRuntime | NetworkSceneMask.GameUI :
                        name == "Lobby" ? NetworkSceneMask.Lobby : NetworkSceneMask.GameRuntime | NetworkSceneMask.GameUI);
                    SceneFlowPipelineTests.Set(entry, "_lifetime", name == "Session" ? NetworkPrefabLifetime.Persistent : NetworkPrefabLifetime.SceneScoped);
                    SceneFlowPipelineTests.Set(entry, "_ownerSceneName", name == "Session" ? string.Empty :
                        name == "Lobby" ? "PipelineLobby" : "PipelineGame");
                    SceneFlowPipelineTests.Set(entry, "_spawnOrder", (int)id);
                    entries.Add(entry);
                }
                if (!File.Exists(folder + "Catalog.asset"))
                {
                    var catalog = ScriptableObject.CreateInstance<NetworkPrefabCatalog>();
                    SceneFlowPipelineTests.Set(catalog, "_entries", entries);
                    AssetDatabase.CreateAsset(catalog, folder + "Catalog.asset");
                }
                else
                {
                    var catalog = AssetDatabase.LoadAssetAtPath<NetworkPrefabCatalog>(folder + "Catalog.asset");
                    SceneFlowPipelineTests.Set(catalog, "_entries", entries);
                    EditorUtility.SetDirty(catalog);
                    AssetDatabase.SaveAssetIfDirty(catalog);
                }
            }
            finally { EditorSceneManager.ClosePreviewScene(preview); }
        }

        private sealed class Results : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                if (!result.ToXml().OuterXml.Contains("HotFix.SceneFlow.Tests")) return;
                string path = Path.GetFullPath("Temp/SceneFlowPipeline/playmode-results.xml");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, result.ToXml().OuterXml);
                Debug.Log($"[SceneFlowTests] Passed={result.PassCount}, Failed={result.FailCount}, Results={path}");
            }
        }
    }
}
#endif
