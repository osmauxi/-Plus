using Cysharp.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Resolvers.Resolvers;
using ProjectGame.HotFix.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace ProjectGame.HotFix
{
    /// <summary>
    /// 热更代码入口逻辑
    /// </summary>
    public static class HotFixEntry
    {
        // 这个方法会被 AOT 区的 BootstrapRunner 通过反射调用
        public static void StartGame()
        {
            Debug.Log("<color=#00FF00>===========================================</color>");
            Debug.Log("<color=#00FF00>[HotFix] 热更域激活成功</color>");
            Debug.Log("<color=#00FF00>===========================================</color>");

            RegisterMessagePackResolver();
            ConfigManager.Instance.Init();
            EnterLobbyScene();
        }

        //MessagePack的反序列化是反射动态生成IL代码来执行，而AOT平台是不允许动态生成代码的。
        //为了解决这个问题，我们使用mpc工具在编译阶段提前生成好反序列化代码，这样反序列化就会直接调用已有方法。
        //这个方法就是用来注册我们生成的静态解析器的，确保MessagePack在运行时能够正确找到并使用它。
        private static void RegisterMessagePackResolver()
        {
            //组合解析器：把我们用 mpc 生成的解析器(GeneratedResolver)和官方的基础解析器 (StandardResolver) 组合在一起。
            StaticCompositeResolver.Instance.Register(
                GeneratedResolver.Instance,
                StandardResolver.Instance
            );

            //覆盖默认选项
            var options = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
            MessagePackSerializer.DefaultOptions = options;

            Debug.Log("[MessagePack] AOT 静态解析器注册完毕");
        }
    
        private static async void TestLoadExcelData()
        {
            Debug.Log("准备从 Addressables 拉取配表 Config_Item...");

            // 注意：这里的 "Config_Item" 是你在 Addressables Groups 里给它起的 Address 名字
            var handle = Addressables.LoadAssetAsync<TextAsset>("Config_Item");
            await handle.Task;

            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                byte[] rawBytes = handle.Result.bytes;

                // 终极反序列化：把乱码瞬间变回 C# 字典！
                var itemDict = MessagePackSerializer.Deserialize<Dictionary<int, Config_Item>>(rawBytes);

                // 见证奇迹：精准读取 ID 为 1000 的装备
                if (itemDict.TryGetValue(1001, out Config_Item woodSword))
                {
                    Debug.Log($"<color=yellow>【配表读取成功】</color>");
                    Debug.Log($"武器ID: {woodSword.ItemID}");
                    Debug.Log($"武器名: {woodSword.Name}");
                    Debug.Log($"武器描述: {woodSword.Description}");
                }
            }
            else
            {
                Debug.LogError("Addressables 加载配表失败！请检查 Address 名字是否填对！");
            }

        }

        private static async void EnterLobbyScene()
        {
            await LoadingUI.Show("正在进入大厅...");

            //并行：场景加载 + 配置表全量加载
            var sceneLoadTask = Addressables.LoadSceneAsync("LobbyScene", LoadSceneMode.Single).Task.AsUniTask();
            var configLoadTask = ConfigManager.Instance.LoadAllConfigsAsync();

            await UniTask.WhenAll(sceneLoadTask, configLoadTask);

            LoadingUI.Hide();
            Debug.Log("<color=yellow>【成功进入联机大厅】</color>");
        }
    }
}