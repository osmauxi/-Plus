using Cysharp.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using System;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Resolvers.Resolvers;
using ProjectGame.HotFix.SceneFlow;
using ProjectGame.HotFix.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectGame.HotFix
{
    /// <summary>
    /// 热更代码入口逻辑
    /// </summary>
    public static class HotFixEntry
    {
        /// <summary>
        /// 初始化热更域并进入大厅场景 
        /// </summary>
        public static void StartGame()
        {
            Debug.Log("<color=#00FF00>===========================================</color>");
            Debug.Log("<color=#00FF00>[HotFix] 热更域激活成功</color>");
            Debug.Log("<color=#00FF00>===========================================</color>");

            RegisterMessagePackResolver();
            ConfigManager.Instance.Init();
            EnterLobbySceneAsync().Forget();
        }

        /// <summary>
        /// 注册预生成的 MessagePack AOT 解析器 
        /// </summary>
        private static void RegisterMessagePackResolver()
        {
            //组合解析器：把我们用 mpc 生成的解析器(GeneratedResolver)和官方的基础解析器 (StandardResolver) 组合在一起 
            StaticCompositeResolver.Instance.Register(GeneratedResolver.Instance,StandardResolver.Instance);

            //覆盖默认选项
            var options = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
            MessagePackSerializer.DefaultOptions = options;

            Debug.Log("[MessagePack] AOT 静态解析器注册完毕");
        }

        /// <summary>
        /// 加载全部配置并切换到大厅场景 
        /// </summary>
        private static async UniTask EnterLobbySceneAsync()
        {
            const string lobbyScenePath = "Assets/_HotUpdate/Scenes/LobbyScene.unity";

            try
            {
                //先加载配表，确保数据就绪后再加载场景（避免 UI Awake->GetTable 时配表尚未加载的时序问题）
                await ConfigManager.Instance.LoadAllConfigsAsync();

                // 离线 UI 和审批在联网前就需要会话对象。先引导持久 Root，再加载纯大厅场景。
                await NetworkSessionBootstrap.EnsureAvailableAsync(default);
                await AddressableSceneLoadService.Shared.LoadSceneAsync(lobbyScenePath, LoadSceneMode.Single, default);

                Debug.Log(
                    "<color=yellow>【通过 Addressables 成功进入联机大厅】</color>");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[HotFixEntry] 进入 Lobby 失败：\n{exception}");
                throw;
            }
        }
    }
}
