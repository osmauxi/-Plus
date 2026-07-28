using Cysharp.Threading.Tasks;
using HybridCLR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectGame.Bootstrap
{
    /// <summary>
    /// 初始化 Addressables 与 HybridCLR，并把控制权移交给热更入口。
    /// </summary>
    public class BootstrapRunner : MonoBehaviour
    {
        [Header("Addressables 资源标签配置")]
        [SerializeField] private string labelAotDll = "AOT_DLL";
        [SerializeField] private string labelHotFixDll = "Hotfix_DLL";

        /// <summary>
        /// 保留启动对象并启动完整初始化管线。
        /// </summary>
        private async void Start()
        {
            DontDestroyOnLoad(gameObject);
            await StartPipelineAsync();
        }

        /// <summary>
        /// 按顺序完成目录更新、资源下载、元数据加载与热更入口调用。
        /// </summary>
        private async UniTask StartPipelineAsync()
        {
            try
            {
                Debug.Log("[Bootstrap] 初始化引擎寻址系统...");
                await Addressables.InitializeAsync();

                Debug.Log("[Bootstrap] 检查远端版本 (Catalog)...");
                List<string> catalogsToUpdate =
                    await Addressables.CheckForCatalogUpdates(false);
                if (catalogsToUpdate.Count > 0)
                {
                    Debug.Log("[Bootstrap] 发现新版本，拉取更新目录...");
                    await Addressables.UpdateCatalogs(
                        catalogsToUpdate,
                        false);
                }

                Debug.Log("[Bootstrap] 开始下载差异资源包...");
                await DownloadDependencies(labelAotDll);
                await DownloadDependencies(labelHotFixDll);

                Debug.Log("[Bootstrap] 组装底层环境：加载 AOT 元数据...");
                await LoadMetadataForAotAssemblies();

                Debug.Log("[Bootstrap] 唤醒热更逻辑：加载 HotFix 程序集...");
                List<Assembly> hotFixAssemblies =
                    await LoadHotFixAssemblies();

                Debug.Log("[Bootstrap] 反射进入游戏主流程");
                EnterGame(hotFixAssemblies);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[Bootstrap] 致命错误，启动管线中断：{exception}");
            }
        }

        /// <summary>
        /// 下载指定 Addressables 标签的全部依赖资源。
        /// </summary>
        private static async UniTask DownloadDependencies(string label)
        {
            AsyncOperationHandle handle =
                Addressables.DownloadDependenciesAsync(
                    label,
                    autoReleaseHandle: false);

            try
            {
                await handle.ToUniTask();
                if (handle.Status == AsyncOperationStatus.Failed)
                    throw new InvalidOperationException(
                        $"下载标签 [{label}] 失败，请检查 Addressables 标签配置");
            }
            finally
            {
                Addressables.Release(handle);
            }
        }

        /// <summary>
        /// 加载并补充所有配置为 AOT_DLL 的裁剪程序集元数据。
        /// </summary>
        private async UniTask LoadMetadataForAotAssemblies()
        {
            AsyncOperationHandle<IList<TextAsset>> handle =
                Addressables.LoadAssetsAsync<TextAsset>(labelAotDll, null);

            try
            {
                IList<TextAsset> aotDlls = await handle.ToUniTask();
                foreach (TextAsset asset in aotDlls)
                {
                    RuntimeApi.LoadMetadataForAOTAssembly(
                        asset.bytes,
                        HomologousImageMode.SuperSet);
                    Debug.Log(
                        $"[HybridCLR] 元数据补充成功：{asset.name}");
                }
            }
            finally
            {
                Addressables.Release(handle);
            }
        }

        /// <summary>
        /// 加载全部热更 DLL，并在加载期间按程序集名称解析相互依赖。
        /// </summary>
        private async UniTask<List<Assembly>> LoadHotFixAssemblies()
        {
            var loadedAssemblies = new List<Assembly>();

#if UNITY_EDITOR
            Debug.Log(
                "<color=cyan>[HybridCLR] Editor 环境：使用原生程序集启动，跳过二进制装载</color>");
            await UniTask.Yield();
#else
            AsyncOperationHandle<IList<TextAsset>> handle =
                Addressables.LoadAssetsAsync<TextAsset>(
                    labelHotFixDll,
                    null);

            try
            {
                IList<TextAsset> assets = await handle.ToUniTask();
                Dictionary<string, byte[]> binaries =
                    BuildHotFixBinaryMap(assets);
                var loadedByName = new Dictionary<string, Assembly>();

                ResolveEventHandler resolver = (_, args) =>
                {
                    string requestedName =
                        new AssemblyName(args.Name).Name;
                    return LoadAssemblyByName(
                        requestedName,
                        binaries,
                        loadedByName,
                        loadedAssemblies);
                };

                AppDomain.CurrentDomain.AssemblyResolve += resolver;
                try
                {
                    var assemblyNames =
                        new List<string>(binaries.Keys);
                    foreach (string assemblyName in assemblyNames)
                    {
                        LoadAssemblyByName(
                            assemblyName,
                            binaries,
                            loadedByName,
                            loadedAssemblies);
                    }
                }
                finally
                {
                    AppDomain.CurrentDomain.AssemblyResolve -= resolver;
                }
            }
            finally
            {
                Addressables.Release(handle);
            }
#endif

            return loadedAssemblies;
        }

        /// <summary>
        /// 使用 TextAsset 名称建立程序集简单名称到 DLL 字节的索引。
        /// </summary>
        private static Dictionary<string, byte[]> BuildHotFixBinaryMap(
            IList<TextAsset> assets)
        {
            var binaries = new Dictionary<string, byte[]>(
                StringComparer.Ordinal);

            foreach (TextAsset asset in assets)
            {
                string assemblyName =
                    Path.GetFileNameWithoutExtension(asset.name);
                if (!binaries.TryAdd(assemblyName, asset.bytes))
                    throw new InvalidOperationException(
                        $"存在重复的热更程序集资源：{assemblyName}");
            }

            return binaries;
        }

        /// <summary>
        /// 按简单名称加载指定热更程序集，并复用已完成的加载结果。
        /// </summary>
        private static Assembly LoadAssemblyByName(
            string assemblyName,
            IReadOnlyDictionary<string, byte[]> binaries,
            IDictionary<string, Assembly> loadedByName,
            ICollection<Assembly> loadedAssemblies)
        {
            if (loadedByName.TryGetValue(
                    assemblyName,
                    out Assembly loadedAssembly))
                return loadedAssembly;

            Assembly existingAssembly = FindLoadedAssembly(assemblyName);
            if (existingAssembly != null)
                return existingAssembly;

            if (!binaries.TryGetValue(assemblyName, out byte[] bytes))
                return null;

            Assembly assembly = Assembly.Load(bytes);
            loadedByName[assemblyName] = assembly;
            loadedAssemblies.Add(assembly);
            Debug.Log($"[HybridCLR] 业务代码激活成功：{assemblyName}");
            return assembly;
        }

        /// <summary>
        /// 在当前应用域中查找已经加载的同名程序集。
        /// </summary>
        private static Assembly FindLoadedAssembly(string assemblyName)
        {
            foreach (Assembly assembly in
                     AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == assemblyName)
                    return assembly;
            }

            return null;
        }

        /// <summary>
        /// 查找并调用约定的热更启动入口。
        /// </summary>
        private void EnterGame(IReadOnlyList<Assembly> hotFixAssemblies)
        {
            Type entryType = null;

#if UNITY_EDITOR
            foreach (Assembly assembly in
                     AppDomain.CurrentDomain.GetAssemblies())
            {
                entryType = assembly.GetType(
                    "ProjectGame.HotFix.HotFixEntry");
                if (entryType != null)
                    break;
            }
#else
            foreach (Assembly assembly in hotFixAssemblies)
            {
                entryType = assembly.GetType(
                    "ProjectGame.HotFix.HotFixEntry");
                if (entryType != null)
                    break;
            }
#endif

            if (entryType == null)
                throw new InvalidOperationException(
                    "找不到入口类：ProjectGame.HotFix.HotFixEntry");

            MethodInfo startMethod = entryType.GetMethod(
                "StartGame",
                BindingFlags.Public | BindingFlags.Static);
            if (startMethod == null)
                throw new MissingMethodException(
                    entryType.FullName,
                    "StartGame");

            startMethod.Invoke(null, null);
            Destroy(gameObject);
        }
    }
}
