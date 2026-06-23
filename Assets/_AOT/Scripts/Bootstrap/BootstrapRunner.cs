using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using HybridCLR;
namespace ProjectGame.Bootstrap
{
    /* ==============================================================================
     * [架构总览] 游戏启动管线 (Bootstrap Pipeline)
     * * [ ] Phase 1: 基础建设 (当前进度)
     * [x] Addressables 初始化与远端 Catalog 检查
     * [x] 双标签 (AOT_DLL / HotFix_DLL) 隔离下载与加载
     * [x] HybridCLR 元数据补充与程序集激活
     * [x] 反射调用 HotFix 跨域入口
     * * [ ] Phase 2: 异常兜底与 UX 完善
     * [ ] 接入 GetDownloadSizeAsync，实现大于 5MB 的非 WiFi 网络弹窗拦截
     * [ ] 引入事件总线/委托，将下载进度 (PercentComplete) 广播给 BootstrapUI
     * [ ] 实现 Try-Catch 的断线重试状态机 (无缝续传机制)
     * * [ ] Phase 3: 业务移交预留
     * [ ] 在 HotFixEntry 接收端，实现 ConfigManager 的二进制数据流读取
     * [ ] 在 HotFixEntry 接收端，拉起 LobbyUI 和 NGO 联机管理器
     * ==============================================================================*/
    public class BootstrapRunner : MonoBehaviour
    {
        [Header("Addressables 资源标签配置")]
        [SerializeField] private string labelAotDll = "AOT_DLL";
        [SerializeField] private string labelHotFixDll = "Hotfix_DLL";
        [SerializeField] private string labelPreloadAssets = "Preload";

        private async void Start() 
        {
            DontDestroyOnLoad(gameObject);
            await StartPipelineAsync();
        }

        private async UniTask StartPipelineAsync() 
        {
            try
            {
                Debug.Log("[Bootstrap] 初始化引擎寻址系统...");
                //唤醒Addressables引擎。它会在本地沙盒中寻找并读取当前的catalog.json（资源菜单），将其加载到内存中。
                await Addressables.InitializeAsync();
                Debug.Log("[Bootstrap] 检查远端版本 (Catalog)...");
                //向远端服务器请求最新的catalog.hash，与本地比对。返回值是一个包含需要更新的Catalog ID列表。传入false表示“只检查，先别自动下载
                List<string> catalogsToUpdate = await Addressables.CheckForCatalogUpdates(false);
                if (catalogsToUpdate.Count > 0)
                {
                    Debug.Log($"[Bootstrap] 发现新版本，拉取更新目录...");
                    //真正把远端的新菜单（catalog.json）下载到本地覆盖旧菜单。
                    await Addressables.UpdateCatalogs(catalogsToUpdate, false);
                }

                Debug.Log("[Bootstrap] 开始下载差异资源包...");
                // 这里为了演示流程，我们强行拉取我们需要热更的标签。
                // 工业级做法会在这里计算 DownloadSize，并把这三个标签合并拉取。
                await DownloadDependencies(labelAotDll);
                await DownloadDependencies(labelHotFixDll);
                //await DownloadDependencies(labelPreloadAssets);

                Debug.Log("[Bootstrap] 组装底层环境：加载 AOT 元数据...");
                await LoadMetadataForAOTAssemblies();

                Debug.Log("[Bootstrap] 唤醒热更逻辑：加载 HotFix 程序集...");
                List<Assembly> hotFixAssList = await LoadHotFixAssembly();

                Debug.Log("[Bootstrap] 反射进入游戏主流程");
                EnterGame(hotFixAssList);
            }
            catch (Exception ex)
            {
                // 任何断网、内存不足，都会直接穿透到这里被捕获
                Debug.LogError($"[Bootstrap] 致命错误，启动管线中断: {ex.Message}");
                // TODO: 呼出 "网络异常，点击重试" 的 UI 面板
            }
        }

        private async UniTask DownloadDependencies(string label)
        {
            var handle = Addressables.DownloadDependenciesAsync(label, autoReleaseHandle: false);

            try
            {
                while (!handle.IsDone)
                {
                    await UniTask.Yield();
                }

                if (handle.Status == AsyncOperationStatus.Failed)
                {
                    throw new Exception($"下载标签 [{label}] 失败。请检查 Inspector 里的标签名是否与 Addressables Groups 里完全一致！");
                }
            }
            finally
            {
                Addressables.Release(handle);
            }
        }

        //把AOT模块的元数据注入到虚拟机中
        private async UniTask LoadMetadataForAOTAssemblies()
        {
            var handle = Addressables.LoadAssetsAsync<TextAsset>(labelAotDll, null);
            IList<TextAsset> aotDlls = await handle;

            foreach (var asset in aotDlls)
            {
                //将AOT字典以超集模式(SuperSet)注入虚拟机
                RuntimeApi.LoadMetadataForAOTAssembly(asset.bytes, HomologousImageMode.SuperSet);
                Debug.Log($"[HybridCLR] 元数据补充成功: {asset.name}");
            }

            Addressables.Release(handle); //及时清理内存
        }

        //加载热更DLL并返回程序集对象
        private async UniTask<List<Assembly>> LoadHotFixAssembly()
        {
            List<Assembly> loadedAssemblies = new List<Assembly>();

#if UNITY_EDITOR
            //官方规范：Editor环境下，绝不执行Assembly.Load，直接利用反射获取编辑器已经编译好的原生程序集
            Debug.Log("<color=cyan>[HybridCLR] Editor 环境：使用原生程序集启动，跳过二进制装载</color>");
            await UniTask.Yield();
#else
            //真机/打包环境下：老老实实从 Addressables 拉取二进制并 Load
            var handle = Addressables.LoadAssetsAsync<TextAsset>(labelHotFixDll, null);
            IList<TextAsset> hotFixDlls = await handle;

            foreach (var asset in hotFixDlls)
            {
                hotFixAssembly = Assembly.Load(asset.bytes);
                loadedAssemblies.Add(asm);
                Debug.Log($"[HybridCLR] 业务代码激活成功: {asset.name}");
            }

            Addressables.Release(handle);
#endif
            return loadedAssemblies;
        }

        private void EnterGame(List<Assembly> hotFixAssembly)
        {
            Type entryType = null;
#if UNITY_EDITOR
            //Editor下为了开发不用硬编码，进行全局遍历查找
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                entryType = asm.GetType("ProjectGame.HotFix.HotFixEntry");
                if (entryType != null) break;
            }
#else
            //真机下只在加载的几个热更DLL里找
            foreach (var asm in hotfixAssemblies)
            {
                entryType = asm.GetType("ProjectGame.HotFix.HotFixEntry");
                if (entryType != null) 
                break;
            }
#endif
            if (entryType == null)
                throw new Exception("找不到入口类: ProjectGame.HotFix.HotFixEntry");

            //AOT区不能直接引用热更区的类型，所以只能通过反射来寻找和调用
            MethodInfo startMethod = entryType.GetMethod("StartGame", BindingFlags.Public | BindingFlags.Static);
            if (startMethod == null) 
                throw new Exception("找不到静态入口方法: StartGame");

            startMethod.Invoke(null, null);

            //加载过程完毕后卸载此脚本
            Destroy(this.gameObject);
        }
    }
}
