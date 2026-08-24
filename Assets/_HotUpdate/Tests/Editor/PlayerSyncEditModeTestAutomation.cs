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
        private static TestRunnerApi _runner;
        private static ResultCallbacks _callbacks;

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
            string resultPath = Path.GetFullPath(ResultRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
            if (File.Exists(resultPath))
                File.Delete(resultPath);

            _runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            _callbacks = new ResultCallbacks(resultPath);
            _runner.RegisterCallbacks(_callbacks);

            ExecutionSettings settings = new(new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[] { "HotFix.Gameplay.EditModeTests" },
            })
            {
                runSynchronously = true,
            };

            _runner.Execute(settings);
        }

        private sealed class ResultCallbacks : ICallbacks
        {
            private readonly string _resultPath;

            public ResultCallbacks(string resultPath)
            {
                _resultPath = resultPath;
            }

            public void RunStarted(ITestAdaptor testsToRun) { }

            public void RunFinished(ITestResultAdaptor result)
            {
                File.WriteAllText(_resultPath, result.ToXml().OuterXml);
                Debug.Log($"[PlayerSyncEditModeTests] Passed={result.PassCount}, Failed={result.FailCount}, Result={_resultPath}");
            }

            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
        }
    }
}
