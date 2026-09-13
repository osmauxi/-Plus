using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace ProjectGame.HotFix.Tests.GameplayUI
{
    /// <summary>只运行 UI 框架测试及 InputManager 回归；结果写入 Temp/GameplayUI/EditModeResults.xml。</summary>
    public static class GameplayUITestAutomation
    {
        public static string LastResult { get; private set; } = "Not run";

        [MenuItem("Tools/Tests/Run Gameplay UI Tests")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                throw new System.InvalidOperationException("请在非播放且编译完成后执行 UI EditMode 测试");
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            var callback = new Results();
            api.RegisterCallbacks(callback);
            try
            {
                var ui = new Filter { testMode = TestMode.EditMode, assemblyNames = new[] { "HotFix.Gameplay.UITests" } };
                var input = new Filter { testMode = TestMode.EditMode,
                    testNames = new[] { "ProjectGame.HotFix.Tests.EditMode.InputManagerTests" } };
                api.Execute(new ExecutionSettings(ui, input) { runSynchronously = true });
            }
            finally
            {
                api.UnregisterCallbacks(callback);
                Object.DestroyImmediate(api);
            }
        }

        private sealed class Results : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) => LastResult = "Running";
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/GameplayUI"));
                Directory.CreateDirectory(directory);
                string path = Path.Combine(directory, "EditModeResults.xml");
                var document = new XmlDocument();
                document.LoadXml(result.ToXml().OuterXml);
                document.Save(path);
                LastResult = $"Passed={result.PassCount}; Failed={result.FailCount}; Skipped={result.SkipCount}; XML={path}";
                Debug.Log("[Gameplay UI Tests] " + LastResult);
            }
        }
    }
}
