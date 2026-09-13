using System.IO;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace ProjectGame.HotFix.Tests.Editor
{
    public static class PlayerSyncEditModeTestAutomation
    {
        private const string ResultRelativePath = "Temp/PlayerSyncNetworkTest/editmode-results.xml";
        private const string NetworkTestConfigArgument = "--sync-test-config=";
        private const string NetworkTestRoleArgument = "--sync-test-role=client";
        private const string NetworkTestScene = "Assets/_HotUpdate/Scenes/Tests/PlayerLocomotionTest.unity";
        private const string ClientRunTokenKey = "ProjectGame.PlayerSyncNetworkTest.ClientRunToken";
        private static TestRunnerApi _playModeRunner;

        [InitializeOnLoadMethod]
        private static void RegisterPlayModeResults()
        {
            if (_playModeRunner != null) return;
            _playModeRunner = ScriptableObject.CreateInstance<TestRunnerApi>();
            _playModeRunner.RegisterCallbacks(new ResultCallbacks(
                Path.GetFullPath("Temp/PlayerSyncNetworkTest/tick-playmode-results.xml"),
                "HotFix.Gameplay.NetworkTests"));
        }

        [MenuItem("Tools/ProjectGame/Run Gameplay Clock PlayMode Tests")]
        public static void RunGameplayClockPlayMode()
        {
            RegisterPlayModeResults();
            _playModeRunner.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.PlayMode,
                testNames = new[]
                {
                    "ProjectGame.HotFix.Tests.Runtime.GameplayNetworkRuntimeTests",
                    "ProjectGame.HotFix.Tests.Runtime.GameplayNetworkPeerTests",
                    "ProjectGame.HotFix.Tests.Runtime.PlayerHealthNetworkTests",
                },
            }));
        }

        [InitializeOnLoadMethod]
        private static void RegisterCloneClientAutoStart()
        {
            if (!HasCommandLineArgument(NetworkTestRoleArgument))
                return;

            EditorApplication.update -= TryStartCloneClient;
            EditorApplication.update += TryStartCloneClient;
        }

        private static void TryStartCloneClient()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            string configPath = ResolveCommandLineValue(NetworkTestConfigArgument);
            if (string.IsNullOrWhiteSpace(configPath) || !File.Exists(configPath))
                return;

            string runToken = File.GetLastWriteTimeUtc(configPath).Ticks.ToString();
            string hostReadyPath = Path.Combine(
                Path.GetDirectoryName(configPath) ?? string.Empty,
                "host-ready.flag");
            if (!File.Exists(hostReadyPath) || File.ReadAllText(hostReadyPath) != runToken)
                return;

            if (SessionState.GetString(ClientRunTokenKey, string.Empty) == runToken)
                return;

            SessionState.SetString(ClientRunTokenKey, runToken);
            EditorSceneManager.OpenScene(NetworkTestScene, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        private static bool HasCommandLineArgument(string expected)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length; i++)
            {
                if (string.Equals(arguments[i], expected, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static string ResolveCommandLineValue(string prefix)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return arguments[i].Substring(prefix.Length).Trim('"');
            }
            return null;
        }

        [MenuItem("Tools/ProjectGame/Run Player Sync EditMode Tests")]
        public static void Run()
        {
            RunEditMode(new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[] { "HotFix.Gameplay.EditModeTests" },
            }, ResultRelativePath);
        }

        [MenuItem("Tools/ProjectGame/Run Gameplay Clock EditMode Tests")]
        public static void RunGameplayClockEditMode()
        {
            RunEditMode(new Filter
            {
                testMode = TestMode.EditMode,
                testNames = new[]
                {
                    "ProjectGame.HotFix.Gameplay.Tests.GameplayNetworkFrameworkTests",
                    "ProjectGame.HotFix.Gameplay.Tests.GameplayNetworkTickDriverTests",
                    "ProjectGame.HotFix.Tests.EditMode.PlayerSyncPipelineTests",
                },
            }, "Temp/PlayerSyncNetworkTest/tick-editmode-results.xml");
        }

        private static void RunEditMode(Filter filter, string relativeResultPath)
        {
            string resultPath = Path.GetFullPath(relativeResultPath);
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
            if (File.Exists(resultPath))
                File.Delete(resultPath);

            var runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            var callbacks = new ResultCallbacks(resultPath, "HotFix.Gameplay.EditModeTests");
            runner.RegisterCallbacks(callbacks);
            try
            {
                runner.Execute(new ExecutionSettings(filter) { runSynchronously = true });
            }
            finally
            {
                // EditMode 同步运行完就退订，避免下次专用回归覆盖全量报告或反过来。
                runner.UnregisterCallbacks(callbacks);
                UnityEngine.Object.DestroyImmediate(runner);
            }
        }

        private sealed class ResultCallbacks : ICallbacks
        {
            private readonly string _resultPath;
            private readonly string _assemblyFilter;

            public ResultCallbacks(string resultPath, string assemblyFilter = null)
            {
                _resultPath = resultPath;
                _assemblyFilter = assemblyFilter;
            }

            public void RunStarted(ITestAdaptor testsToRun) { }

            public void RunFinished(ITestResultAdaptor result)
            {
                string xml = result.ToXml().OuterXml;
                if (_assemblyFilter != null && !xml.Contains(_assemblyFilter)) return;
                Directory.CreateDirectory(Path.GetDirectoryName(_resultPath));
                File.WriteAllText(_resultPath, xml);
                Debug.Log($"[PlayerSyncEditModeTests] Passed={result.PassCount}, Failed={result.FailCount}, Result={_resultPath}");
            }

            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
        }
    }
}
