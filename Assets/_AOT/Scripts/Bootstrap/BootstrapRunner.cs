using Cysharp.Threading.Tasks;
using HybridCLR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;

namespace ProjectGame.Bootstrap
{
    /// <summary>
    /// 初始化 Addressables 与 HybridCLR，并把控制权移交给热更入口 
    /// </summary>
    public class BootstrapRunner : MonoBehaviour
    {
        private const string LobbyScenePath =
            "Assets/_HotUpdate/Scenes/LobbyScene.unity";

        [Header("Addressables 资源标签配置")]
        [SerializeField] private string labelAotDll = "AOT_DLL";
        [SerializeField] private string labelHotFixDll = "Hotfix_DLL";

        [Header("启动诊断 UI")]
        [SerializeField] private bool showRuntimeStatus = true;
        [SerializeField, Min(0f)] private float successOverlaySeconds = 6f;
        [SerializeField, Range(4, 16)] private int maxVisibleMessages = 10;
        [SerializeField, Min(1)] private int webRequestTimeoutSeconds = 15;
        [SerializeField, Min(5f)] private float networkStepTimeoutSeconds = 30f;
        [SerializeField, Min(5f)] private float lobbyEnterTimeoutSeconds = 45f;

        private readonly List<string> _recentMessages = new List<string>();
        private BootstrapState _state = BootstrapState.Waiting;
        private string _currentStage = "等待启动";
        private string _lastError = string.Empty;
        private string _downloadLabel = string.Empty;
        private float _downloadProgress = -1f;
        private int _loadedAotMetadataCount;
        private int _loadedHotFixAssemblyCount;
        private GUIStyle _titleStyle;
        private GUIStyle _statusStyle;
        private GUIStyle _messageStyle;
        private GUIStyle _errorStyle;
        private static bool _addressablesNetworkingConfigured;
        private static bool _localResourceRewriteLogged;
        private static HashSet<string> _localIpv4Addresses;
        private static string _localAddressablesRoot;
        private static bool _localAddressablesRootResolved;

        private enum BootstrapState
        {
            Waiting,
            Running,
            Succeeded,
            Failed
        }

        private void OnEnable()
        {
            Application.logMessageReceived += OnLogMessageReceived;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// 保留启动对象并启动完整初始化管线 
        /// </summary>
        private async void Start()
        {
            DontDestroyOnLoad(gameObject);
            SetStage(
                $"启动环境：{Application.platform}，版本 {Application.version}");
            await StartPipelineAsync();
        }

        /// <summary>
        /// 按顺序完成目录更新、资源下载、元数据加载与热更入口调用 
        /// </summary>
        private async UniTask StartPipelineAsync()
        {
            try
            {
                ConfigureAddressablesNetworking();

                SetStage("初始化 Addressables");
                await Addressables.InitializeAsync();

                SetStage("检查远端 Catalog");
                AsyncOperationHandle<List<string>> checkCatalogHandle =
                    Addressables.CheckForCatalogUpdates(false);
                List<string> catalogsToUpdate;
                try
                {
                    catalogsToUpdate = await checkCatalogHandle.ToUniTask();
                    if (checkCatalogHandle.Status ==
                        AsyncOperationStatus.Failed)
                    {
                        throw checkCatalogHandle.OperationException ??
                              new InvalidOperationException(
                                  "检查远端 Catalog 失败");
                    }
                }
                finally
                {
                    if (checkCatalogHandle.IsValid())
                        Addressables.Release(checkCatalogHandle);
                }

                Debug.Log(
                    $"[Bootstrap] Catalog 检查完成，待更新数量：{catalogsToUpdate.Count}");
                if (catalogsToUpdate.Count > 0)
                {
                    SetStage($"更新 Catalog（{catalogsToUpdate.Count}）");
                    AsyncOperationHandle<List<IResourceLocator>> updateHandle =
                        Addressables.UpdateCatalogs(
                        catalogsToUpdate,
                        false);
                    try
                    {
                        await updateHandle.ToUniTask();
                        if (updateHandle.Status ==
                            AsyncOperationStatus.Failed)
                        {
                            throw updateHandle.OperationException ??
                                  new InvalidOperationException(
                                      "更新远端 Catalog 失败");
                        }
                    }
                    finally
                    {
                        if (updateHandle.IsValid())
                            Addressables.Release(updateHandle);
                    }
                }

                SetStage("下载 AOT 元数据与热更程序集");
                await DownloadDependencies(labelAotDll);
                await DownloadDependencies(labelHotFixDll);

                SetStage("加载 AOT 补充元数据");
                await LoadMetadataForAotAssemblies();

                SetStage("加载 HotFix 程序集");
                List<Assembly> hotFixAssemblies =
                    await LoadHotFixAssemblies();
                _loadedHotFixAssemblyCount = hotFixAssemblies.Count;

                SetStage("调用热更入口");
                EnterGame(hotFixAssemblies);
            }
            catch (Exception exception)
            {
                _state = BootstrapState.Failed;
                _currentStage = "启动管线已中断";
                string userFacingError = GetUserFacingError(exception);
                Debug.LogError(
                    $"[Bootstrap] 致命错误，启动管线中断：{exception}");
                _lastError = userFacingError;
            }
        }

        /// <summary>
        /// 为所有 Addressables 请求设置有限超时，并在资源服务器就是本机时优先直读
        /// ServerData。这样同机启动不经过系统代理/VPN；其他电脑仍使用 Profile 中的局域网地址。
        /// </summary>
        private void ConfigureAddressablesNetworking()
        {
            if (_addressablesNetworkingConfigured)
                return;

            _addressablesNetworkingConfigured = true;
            int requestTimeout = Mathf.Max(1, webRequestTimeoutSeconds);

            Func<IResourceLocation, string> previousTransform =
                Addressables.InternalIdTransformFunc;
            Addressables.InternalIdTransformFunc = location =>
            {
                string internalId = previousTransform != null
                    ? previousTransform(location)
                    : location.InternalId;
                return RewriteLocalServerUrl(internalId);
            };

            Action<UnityEngine.Networking.UnityWebRequest> previousOverride =
                Addressables.WebRequestOverride;
            Addressables.WebRequestOverride = request =>
            {
                previousOverride?.Invoke(request);
                if (request.timeout <= 0)
                    request.timeout = requestTimeout;
            };

            Debug.Log(
                $"[Bootstrap] Addressables 网络保护已启用，请求超时 {requestTimeout} 秒");
        }

        private static string RewriteLocalServerUrl(
            string internalId)
        {
            if (!Uri.TryCreate(internalId, UriKind.Absolute, out Uri uri))
                return internalId;
            if (uri.Scheme != Uri.UriSchemeHttp &&
                uri.Scheme != Uri.UriSchemeHttps)
                return internalId;
            if (!IsLocalIpv4Address(uri.Host))
                return internalId;

            if (TryRewriteToLocalAddressablesFile(uri, out string localFileUrl))
                return localFileUrl;

            var builder = new UriBuilder(uri)
            {
                // localhost 比 127.0.0.1 更容易命中 Windows/代理软件的本地地址绕过规则。
                Host = "localhost"
            };
            string rewritten = builder.Uri.AbsoluteUri;

            if (!_localResourceRewriteLogged)
            {
                _localResourceRewriteLogged = true;
                Debug.Log(
                    $"[Bootstrap] 资源服务器 {uri.Host}:{uri.Port} 是本机，" +
                    "未找到本地 ServerData，自动改用 localhost；" +
                    "其他电脑仍使用局域网 IP");
            }

            return rewritten;
        }

        private static bool TryRewriteToLocalAddressablesFile(
            Uri remoteUri,
            out string localFileUrl)
        {
            localFileUrl = null;
            string root = ResolveLocalAddressablesRoot();
            if (string.IsNullOrEmpty(root))
                return false;

            string relativePath = Uri.UnescapeDataString(
                    remoteUri.AbsolutePath.TrimStart('/'))
                .Replace('/', Path.DirectorySeparatorChar);
            if (string.IsNullOrWhiteSpace(relativePath))
                return false;

            string rootPath = Path.GetFullPath(root)
                .TrimEnd(Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
            string filePath = Path.GetFullPath(
                Path.Combine(rootPath, relativePath));
            string rootPrefix = rootPath + Path.DirectorySeparatorChar;
            if (!filePath.StartsWith(rootPrefix,
                    StringComparison.OrdinalIgnoreCase) ||
                !File.Exists(filePath))
            {
                return false;
            }

            localFileUrl = new Uri(filePath).AbsoluteUri;
            if (!_localResourceRewriteLogged)
            {
                _localResourceRewriteLogged = true;
                Debug.Log(
                    $"[Bootstrap] 资源服务器 {remoteUri.Host}:{remoteUri.Port} 是本机，" +
                    $"已绕过代理并直读 {rootPath}；其他电脑仍使用局域网 IP");
            }

            return true;
        }

        private static string ResolveLocalAddressablesRoot()
        {
            if (_localAddressablesRootResolved)
                return _localAddressablesRoot;

            _localAddressablesRootResolved = true;
            string platformFolder = GetAddressablesPlatformFolder();
            if (string.IsNullOrEmpty(platformFolder))
                return null;

            string[] origins =
            {
                Directory.GetCurrentDirectory(),
                Application.dataPath
            };

            foreach (string origin in origins)
            {
                if (string.IsNullOrWhiteSpace(origin))
                    continue;

                DirectoryInfo directory;
                try
                {
                    directory = new DirectoryInfo(origin);
                }
                catch (Exception)
                {
                    continue;
                }

                for (int depth = 0;
                     directory != null && depth < 7;
                     depth++, directory = directory.Parent)
                {
                    string candidate = Path.Combine(
                        directory.FullName,
                        "ServerData",
                        platformFolder);
                    if (!Directory.Exists(candidate))
                        continue;

                    _localAddressablesRoot = Path.GetFullPath(candidate);
                    return _localAddressablesRoot;
                }
            }

            return null;
        }

        private static string GetAddressablesPlatformFolder()
        {
            return Application.platform switch
            {
                RuntimePlatform.WindowsPlayer => "StandaloneWindows64",
                RuntimePlatform.WindowsEditor => "StandaloneWindows64",
                RuntimePlatform.OSXPlayer => "StandaloneOSX",
                RuntimePlatform.OSXEditor => "StandaloneOSX",
                RuntimePlatform.LinuxPlayer => "StandaloneLinux64",
                RuntimePlatform.LinuxEditor => "StandaloneLinux64",
                _ => null
            };
        }

        private static bool IsLocalIpv4Address(string host)
        {
            if (!IPAddress.TryParse(host, out IPAddress target) ||
                target.AddressFamily != AddressFamily.InterNetwork)
                return false;

            if (IPAddress.IsLoopback(target))
                return true;

            if (_localIpv4Addresses == null)
            {
                _localIpv4Addresses = new HashSet<string>(
                    StringComparer.Ordinal);
                try
                {
                    foreach (IPAddress address in
                             Dns.GetHostAddresses(Dns.GetHostName()))
                    {
                        if (address.AddressFamily ==
                            AddressFamily.InterNetwork)
                        {
                            _localIpv4Addresses.Add(address.ToString());
                        }
                    }
                }
                catch (SocketException exception)
                {
                    Debug.LogWarning(
                        $"[Bootstrap] 无法枚举本机 IPv4 地址：{exception.Message}");
                }
            }

            return _localIpv4Addresses.Contains(target.ToString());
        }

        /// <summary>
        /// 下载指定 Addressables 标签的全部依赖资源 
        /// </summary>
        private async UniTask DownloadDependencies(string label)
        {
            _downloadLabel = label;
            _downloadProgress = 0f;
            SetStage($"检查下载大小：{label}");

            AsyncOperationHandle<long> sizeHandle =
                Addressables.GetDownloadSizeAsync(label);
            try
            {
                long downloadSize = await sizeHandle.ToUniTask();
                Debug.Log(
                    $"[Bootstrap] 标签 [{label}] 待下载：{FormatBytes(downloadSize)}");
            }
            finally
            {
                if (sizeHandle.IsValid())
                    Addressables.Release(sizeHandle);
            }

            SetStage($"下载资源：{label}");
            AsyncOperationHandle handle =
                Addressables.DownloadDependenciesAsync(
                    label,
                    autoReleaseHandle: false);

            try
            {
                float timeoutAt =
                    Time.realtimeSinceStartup + networkStepTimeoutSeconds;
                while (!handle.IsDone)
                {
                    if (Time.realtimeSinceStartup >= timeoutAt)
                    {
                        throw new TimeoutException(
                            $"下载标签 [{label}] 超过 {networkStepTimeoutSeconds:F0} 秒。" +
                            "请检查 Catalog 中的服务器 IP、Hosting Service 和防火墙。");
                    }

                    _downloadProgress = handle.PercentComplete;
                    await UniTask.Yield();
                }

                await handle.ToUniTask();
                if (handle.Status == AsyncOperationStatus.Failed)
                    throw new InvalidOperationException(
                        $"下载标签 [{label}] 失败，请检查 Addressables 标签配置");

                _downloadProgress = 1f;
                Debug.Log($"[Bootstrap] 标签 [{label}] 下载完成");
            }
            finally
            {
                Addressables.Release(handle);
                _downloadLabel = string.Empty;
                _downloadProgress = -1f;
            }
        }

        /// <summary>
        /// 加载并补充所有配置为 AOT_DLL 的裁剪程序集元数据 
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
                    LoadImageErrorCode result =
                        RuntimeApi.LoadMetadataForAOTAssembly(
                        asset.bytes,
                        HomologousImageMode.SuperSet);

                    if (result == LoadImageErrorCode.OK ||
                        result == LoadImageErrorCode.HOMOLOGOUS_ASSEMBLY_HAS_LOADED)
                    {
                        _loadedAotMetadataCount++;
                        Debug.Log(
                            $"[HybridCLR] AOT 元数据：{asset.name}，结果 {result}");
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"[HybridCLR] AOT 元数据加载异常：{asset.name}，结果 {result}");
                    }
                }
            }
            finally
            {
                Addressables.Release(handle);
            }
        }

        /// <summary>
        /// 加载全部热更 DLL，并在加载期间按程序集名称解析相互依赖 
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
                Debug.Log(
                    $"[Bootstrap] 找到 {binaries.Count} 个 HotFix DLL：" +
                    string.Join(", ", binaries.Keys));
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
                    assemblyNames.Sort(CompareHotFixAssemblyLoadOrder);
                    Debug.Log(
                        "[Bootstrap] HotFix DLL 装载顺序：" +
                        string.Join(" -> ", assemblyNames));
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
        /// HybridCLR 在 IL2CPP Player 中会比 Mono Editor 更早解析跨程序集字段类型。
        /// 先装载事件、基础网络和配置程序集，并把总入口放到最后，避免消费程序集在其
        /// 依赖程序集尚未注册时触发 TypeLoadException。
        /// </summary>
        private static int CompareHotFixAssemblyLoadOrder(string left,string right)
        {
            int priorityComparison = GetHotFixAssemblyLoadPriority(left)
                .CompareTo(GetHotFixAssemblyLoadPriority(right));
            return priorityComparison != 0
                ? priorityComparison
                : string.Compare(left, right, StringComparison.Ordinal);
        }

        private static int GetHotFixAssemblyLoadPriority(string assemblyName)
        {
            switch (assemblyName)
            {
                case "HotFix.Events":
                    return 0;
                case "HotFix.Gameplay.Network":
                    return 5;
                case "HotFix.Network.Runtime":
                    return 8;
                case "HotFix.Config":
                case "HotFix.Settings":
                case "HotFix.DebugTools":
                    return 10;
                case "HotFix.SceneFlow":
                    return 15;
                case "HotFix.Lobby.Network":
                    return 20;
                case "HotFix.Lobby.World":
                    return 30;
                case "HotFix.Gameplay":
                    return 40;
                case "HotFix.Lobby.UI":
                    return 50;
                case "HotFix.Entry":
                    return 1000;
                default:
                    return 100;
            }
        }

        /// <summary>
        /// 使用 TextAsset 名称建立程序集简单名称到 DLL 字节的索引 
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
        /// 按简单名称加载指定热更程序集，并复用已完成的加载结果 
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
            InitializeNgoSerialization(assembly);
            loadedByName[assemblyName] = assembly;
            loadedAssemblies.Add(assembly);
            Debug.Log($"[HybridCLR] 业务代码激活成功：{assemblyName}");
            return assembly;
        }

        /// <summary>
        /// NGO 的 ILPostProcessor 会在每个业务程序集生成序列化注册入口，
        /// 但动态 Assembly.Load 发生在 Unity 的 RuntimeInitialize 阶段之后，
        /// 因此必须在任何 NetworkBehaviour 实例化或 StartHost 之前主动调用。
        /// </summary>
        private static void InitializeNgoSerialization(Assembly assembly)
        {
            const string helperTypeName =
                "__GEN.NetworkVariableSerializationHelper";
            const string initializeMethodName = "InitializeSerialization";

            Type helperType = assembly.GetType(helperTypeName, false);
            if (helperType == null)
                return;

            MethodInfo initializeMethod = helperType.GetMethod(
                initializeMethodName,
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic);
            if (initializeMethod == null)
            {
                throw new MissingMethodException(
                    $"程序集 {assembly.GetName().Name} 包含 {helperTypeName}，" +
                    $"但找不到 {initializeMethodName}。请重新编译 HybridCLR DLL。");
            }

            initializeMethod.Invoke(null, null);
            Debug.Log(
                $"[HybridCLR][NGO] 序列化注册完成：{assembly.GetName().Name}");
        }

        /// <summary>
        /// 在当前应用域中查找已经加载的同名程序集 
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
        /// 查找并调用约定的热更启动入口 
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

            MethodInfo startMethod = entryType.GetMethod("StartGame",BindingFlags.Public | BindingFlags.Static);
            if (startMethod == null)
                throw new MissingMethodException(entryType.FullName,"StartGame");

            startMethod.Invoke(null, null);
            SetStage("热更入口已调用，等待配表完成并进入 LobbyScene");
            WatchLobbySceneTimeoutAsync().Forget();
        }

        private async UniTask WatchLobbySceneTimeoutAsync()
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(lobbyEnterTimeoutSeconds),
                ignoreTimeScale: true);

            if (_state != BootstrapState.Running)
                return;
            if (SceneManager.GetActiveScene().path == LobbyScenePath)
                return;

            _state = BootstrapState.Failed;
            _currentStage = "等待 LobbyScene 超时";
            _lastError =
                $"热更入口调用后 {lobbyEnterTimeoutSeconds:F0} 秒仍未进入 LobbyScene。" +
                "请检查 Config 标签下载、反序列化和 HotFixEntry 日志。";
            Debug.LogError($"[Bootstrap] {_lastError}");
        }

        private void SetStage(string stage)
        {
            _state = BootstrapState.Running;
            _currentStage = stage;
            Debug.Log($"[Bootstrap] {stage}");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.path != LobbyScenePath && scene.name != "LobbyScene")
                return;

            _state = BootstrapState.Succeeded;
            _currentStage = "配置加载完成，已进入 LobbyScene";
            _downloadProgress = -1f;
            Debug.Log("[Bootstrap] LobbyScene 加载成功，启动管线完成");
            DestroyAfterSuccessAsync().Forget();
        }

        private async UniTask DestroyAfterSuccessAsync()
        {
            if (successOverlaySeconds > 0f)
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(successOverlaySeconds),
                    ignoreTimeScale: true);
            }

            Destroy(gameObject);
        }

        private void OnLogMessageReceived(
            string condition,
            string stackTrace,
            LogType type)
        {
            bool isBootstrapMessage =
                condition.Contains("[Bootstrap]") ||
                condition.Contains("[HybridCLR]") ||
                condition.Contains("[HotFix]") ||
                condition.Contains("[HotFixEntry]") ||
                condition.Contains("[ConfigManager]") ||
                condition.Contains("[MessagePack]");
            bool isImportant =
                type == LogType.Error ||
                type == LogType.Exception ||
                type == LogType.Assert ||
                type == LogType.Warning;

            if (!isBootstrapMessage && !isImportant)
                return;

            string prefix = type == LogType.Warning
                ? "警告"
                : isImportant ? "错误" : "信息";
            string message = condition
                .Replace("\r", " ")
                .Replace("\n", " | ");
            if (message.Length > 220)
                message = message.Substring(0, 220) + "...";

            _recentMessages.Add($"[{prefix}] {message}");
            int maxMessages = Mathf.Max(4, maxVisibleMessages);
            while (_recentMessages.Count > maxMessages)
                _recentMessages.RemoveAt(0);

            if (isImportant && _state != BootstrapState.Failed)
                _lastError = message;
        }

        private void OnGUI()
        {
            if (!showRuntimeStatus)
                return;

            EnsureGuiStyles();

            float margin = Mathf.Max(8f, Screen.width * 0.015f);
            float width = Mathf.Min(760f, Screen.width - margin * 2f);
            float height = Mathf.Min(460f, Screen.height - margin * 2f);
            GUILayout.BeginArea(
                new Rect(margin, margin, width, height),
                GUI.skin.window);

            GUILayout.Label("项目启动诊断", _titleStyle);
            GUILayout.Label(
                $"状态：<color={GetStateColor()}><b>{GetStateText()}</b></color>  " +
                $"阶段：{_currentStage}",
                _statusStyle);
            GUILayout.Label(
                $"场景：{SceneManager.GetActiveScene().name}  |  " +
                $"网络：{Application.internetReachability}  |  " +
                $"AOT 元数据：{_loadedAotMetadataCount}  |  " +
                $"HotFix DLL：{_loadedHotFixAssemblyCount}",
                _messageStyle);

            if (_downloadProgress >= 0f)
            {
                GUILayout.Label(
                    $"下载 {_downloadLabel}：{_downloadProgress * 100f:F0}%",
                    _messageStyle);
                GUILayout.HorizontalSlider(_downloadProgress, 0f, 1f);
            }

            if (!string.IsNullOrEmpty(_lastError))
                GUILayout.Label($"最近异常：{_lastError}", _errorStyle);

            GUILayout.Space(6f);
            GUILayout.Label("启动日志", _statusStyle);
            foreach (string message in _recentMessages)
                GUILayout.Label(message, _messageStyle);

            GUILayout.EndArea();
        }

        private void EnsureGuiStyles()
        {
            if (_titleStyle != null)
                return;

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                richText = true,
                wordWrap = true
            };
            _statusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                richText = true,
                wordWrap = true
            };
            _messageStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                richText = true,
                wordWrap = true
            };
            _errorStyle = new GUIStyle(_messageStyle)
            {
                normal = { textColor = new Color(1f, 0.45f, 0.35f) }
            };
        }

        private string GetStateText()
        {
            switch (_state)
            {
                case BootstrapState.Running:
                    return "运行中";
                case BootstrapState.Succeeded:
                    return "成功";
                case BootstrapState.Failed:
                    return "失败";
                default:
                    return "等待";
            }
        }

        private string GetStateColor()
        {
            switch (_state)
            {
                case BootstrapState.Succeeded:
                    return "#65D46E";
                case BootstrapState.Failed:
                    return "#FF665A";
                case BootstrapState.Running:
                    return "#63B3FF";
                default:
                    return "#D0D0D0";
            }
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes <= 0)
                return "0 B（已缓存或无需下载）";
            if (bytes >= 1024L * 1024L)
                return $"{bytes / (1024f * 1024f):F2} MB";
            if (bytes >= 1024L)
                return $"{bytes / 1024f:F2} KB";
            return $"{bytes} B";
        }

        private static string GetUserFacingError(Exception exception)
        {
            string details = exception.ToString();
            if (details.IndexOf(
                    "Insecure connection not allowed",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                details.IndexOf(
                    "Non-secure network connections disabled",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Player 禁止非安全 HTTP 下载，但当前 Addressables 使用的是 http:// 地址。" +
                       "请在 Player Settings > Other Settings > Allow downloads over HTTP 中允许 HTTP，" +
                       "或把远端服务改为 HTTPS，然后重新构建 Addressables 和 Player。";
            }

            if (details.IndexOf(
                    "Unable to load asset bundle",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                details.IndexOf(
                    "ConnectionError",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "无法连接 Addressables 远端资源。请检查 Catalog 内的服务器 IP、" +
                       "Hosting Service、端口与防火墙，并确认客户端能够访问该地址。";
            }

            string message = exception.GetBaseException().Message;
            return string.IsNullOrWhiteSpace(message)
                ? exception.GetType().Name
                : message;
        }
    }
}
