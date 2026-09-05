using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 为局域网联调维护当前 Addressables Profile 的远端加载地址。
/// </summary>
public sealed class AddressablesLanRemoteWindow : EditorWindow
{
    private const int DefaultHostingPort = 64482;
    private const string MenuPath = "Tools/Addressables/LAN Remote Address";

    private string _host = string.Empty;
    private int _port = DefaultHostingPort;
    private bool _useHttps;
    private int _catalogRequestTimeoutSeconds = 15;
    private Vector2 _scrollPosition;
    private List<LanAddress> _lanAddresses = new List<LanAddress>();
    private UnityWebRequest _connectivityRequest;
    private EditorApplication.CallbackFunction _connectivityPoll;
    private string _connectivityResult = "尚未测试";

    private sealed class LanAddress
    {
        public string Address;
        public string AdapterName;
        public bool HasGateway;
    }

    [MenuItem(MenuPath)]
    private static void OpenWindow()
    {
        var window = GetWindow<AddressablesLanRemoteWindow>();
        window.titleContent = new GUIContent("Addressables LAN");
        window.minSize = new Vector2(560f, 470f);
        window.Show();
    }

    private void OnEnable()
    {
        RefreshLanAddresses();
        LoadCurrentRemoteAddress();
    }

    private void OnDisable()
    {
        if (_connectivityPoll != null)
            EditorApplication.update -= _connectivityPoll;
        _connectivityPoll = null;
        _connectivityRequest?.Dispose();
        _connectivityRequest = null;
    }

    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        EditorGUILayout.LabelField("Addressables 局域网远端地址", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "这里修改的是当前 Addressables Profile 的 Remote.LoadPath。" +
            "地址变化后需要重新构建 Addressables；已构建的 Player 也需要重新构建，" +
            "否则仍会使用旧 Catalog 地址。",
            MessageType.Info);

        DrawCurrentConfiguration();
        EditorGUILayout.Space(8f);
        DrawDetectedAddresses();
        EditorGUILayout.Space(8f);
        DrawAddressEditor();
        EditorGUILayout.Space(12f);
        DrawActions();

        EditorGUILayout.EndScrollView();
    }

    private void DrawCurrentConfiguration()
    {
        AddressableAssetSettings settings =
            AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            EditorGUILayout.HelpBox(
                "项目中没有找到 Addressables Settings。",
                MessageType.Error);
            return;
        }

        string profileName = settings.profileSettings.GetProfileName(
            settings.activeProfileId);
        string remoteLoadPath = settings.profileSettings.GetValueByName(
            settings.activeProfileId,
            AddressableAssetSettings.kRemoteLoadPath);

        EditorGUILayout.LabelField("当前配置", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Active Profile", profileName);
        EditorGUILayout.LabelField("Remote.LoadPath", remoteLoadPath);
        EditorGUILayout.LabelField(
            "HTTP 策略",
            PlayerSettings.insecureHttpOption.ToString());
        EditorGUILayout.LabelField(
            "Development Build",
            EditorUserBuildSettings.development ? "开启" : "关闭");
        EditorGUILayout.LabelField(
            "Catalog 请求超时",
            settings.CatalogRequestsTimeout > 0
                ? $"{settings.CatalogRequestsTimeout} 秒"
                : "未设置（可能无限等待）");
    }

    private void DrawDetectedAddresses()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("检测到的物理网卡", EditorStyles.boldLabel);
        if (GUILayout.Button("重新检测", GUILayout.Width(90f)))
            RefreshLanAddresses();
        EditorGUILayout.EndHorizontal();

        if (_lanAddresses.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "没有检测到可用的以太网或无线 IPv4 地址，请手动输入主机名/IP。",
                MessageType.Warning);
            return;
        }

        foreach (LanAddress address in _lanAddresses)
        {
            EditorGUILayout.BeginHorizontal();
            string gatewayText = address.HasGateway ? "，有默认网关" : string.Empty;
            EditorGUILayout.LabelField(
                $"{address.Address}  ({address.AdapterName}{gatewayText})");
            if (GUILayout.Button("使用", GUILayout.Width(70f)))
                _host = address.Address;
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawAddressEditor()
    {
        EditorGUILayout.LabelField("目标地址", EditorStyles.boldLabel);
        _host = EditorGUILayout.TextField("IP / 主机名", _host);
        _port = EditorGUILayout.IntField("Hosting 端口", _port);
        _useHttps = EditorGUILayout.Toggle("使用 HTTPS", _useHttps);
        _catalogRequestTimeoutSeconds = EditorGUILayout.IntField(
            "Catalog 请求超时（秒）",
            _catalogRequestTimeoutSeconds);

        if (TryBuildRemoteUrl(out string remoteUrl, out _))
            EditorGUILayout.SelectableLabel(
                remoteUrl,
                EditorStyles.textField,
                GUILayout.Height(EditorGUIUtility.singleLineHeight));
    }

    private void DrawActions()
    {
        if (GUILayout.Button("应用远端地址"))
            ApplyRemoteAddress(true);

        if (GUILayout.Button("应用开发测试 HTTP 策略"))
            ApplyDevelopmentHttpPolicy();

        if (GUILayout.Button("测试本机 Hosting 连接"))
            TestHostingConnection();
        EditorGUILayout.LabelField(
            "连接测试",
            _connectivityResult,
            EditorStyles.wordWrappedLabel);

        GUI.backgroundColor = new Color(0.6f, 0.85f, 1f);
        if (GUILayout.Button("应用地址、同步热更 DLL 并构建 Addressables"))
            ApplyAndBuildAddressables();
        GUI.backgroundColor = Color.white;

        if (GUILayout.Button("打开 Addressables Hosting 窗口"))
        {
            EditorApplication.ExecuteMenuItem(
                "Window/Asset Management/Addressables/Hosting");
        }

        if (GUILayout.Button("打开局域网联机操作指南"))
        {
            UnityEngine.Object guide = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(
                "Assets/局域网联机与Addressables操作指南.md");
            if (guide != null)
                AssetDatabase.OpenAsset(guide);
        }

        EditorGUILayout.HelpBox(
            "本工具不会修改 Windows 防火墙。两台电脑联调还需要在资源/Host 电脑上" +
            "放行 TCP 64482 与 UDP 7777。",
            MessageType.Warning);
    }

    private void LoadCurrentRemoteAddress()
    {
        AddressableAssetSettings settings =
            AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
            return;

        if (settings.CatalogRequestsTimeout > 0)
            _catalogRequestTimeoutSeconds = settings.CatalogRequestsTimeout;

        string currentValue = settings.profileSettings.GetValueByName(
            settings.activeProfileId,
            AddressableAssetSettings.kRemoteLoadPath);
        if (!Uri.TryCreate(currentValue, UriKind.Absolute, out Uri uri))
        {
            if (_lanAddresses.Count > 0)
                _host = _lanAddresses[0].Address;
            return;
        }

        _host = uri.Host;
        _port = uri.IsDefaultPort ? DefaultHostingPort : uri.Port;
        _useHttps = uri.Scheme.Equals(
            Uri.UriSchemeHttps,
            StringComparison.OrdinalIgnoreCase);
    }

    private void ApplyAndBuildAddressables()
    {
        if (!ApplyRemoteAddress(false))
            return;

        if (!_useHttps &&
            PlayerSettings.insecureHttpOption == InsecureHttpOption.NotAllowed)
        {
            bool applyPolicy = EditorUtility.DisplayDialog(
                "HTTP 下载被禁止",
                "当前地址使用 HTTP，但 Player 禁止 HTTP 下载。是否应用开发测试策略，" +
                "并开启 Development Build？",
                "应用",
                "取消构建");
            if (!applyPolicy)
                return;
            ApplyDevelopmentHttpPolicy();
        }

        try
        {
            Debug.Log("[Addressables LAN] 开始编译并同步 HybridCLR 热更 DLL...");
            HotUpdateBuilderTool.BuildAndCopyHotUpdateDlls();
            Debug.Log("[Addressables LAN] 开始构建 Addressables Player Content...");
            AddressableAssetSettings.BuildPlayerContent(
                out AddressablesPlayerBuildResult result);
            if (!string.IsNullOrEmpty(result.Error))
                throw new InvalidOperationException(result.Error);

            EditorUtility.DisplayDialog(
                "Addressables 构建完成",
                $"远端地址：{BuildRemoteUrl()}\n" +
                $"构建耗时：{result.Duration:F2} 秒\n\n" +
                "热更 DLL 已重新编译并同步。\n" +
                "下一步：启动 Hosting Service，并重新构建 Player。",
                "确定");
            Debug.Log(
                $"[Addressables LAN] 构建成功，Remote.LoadPath={BuildRemoteUrl()}");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog(
                "Addressables 构建失败",
                exception.Message,
                "确定");
        }
    }

    private bool ApplyRemoteAddress(bool showDialog)
    {
        if (!TryBuildRemoteUrl(out string remoteUrl, out string error))
        {
            EditorUtility.DisplayDialog("地址无效", error, "确定");
            return false;
        }

        AddressableAssetSettings settings =
            AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            EditorUtility.DisplayDialog(
                "Addressables 未配置",
                "项目中没有找到 Addressables Settings。",
                "确定");
            return false;
        }

        settings.profileSettings.SetValue(
            settings.activeProfileId,
            AddressableAssetSettings.kRemoteLoadPath,
            remoteUrl);
        settings.CatalogRequestsTimeout =
            Mathf.Max(1, _catalogRequestTimeoutSeconds);
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        Debug.Log($"[Addressables LAN] Remote.LoadPath 已更新：{remoteUrl}");
        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "远端地址已更新",
                $"Remote.LoadPath = {remoteUrl}\n\n" +
                "请重新构建 Addressables 和 Player。",
                "确定");
        }

        Repaint();
        return true;
    }

    private void TestHostingConnection()
    {
        if (_connectivityRequest != null)
        {
            EditorUtility.DisplayDialog(
                "正在测试",
                "已有一个 Hosting 连接测试正在执行，请稍候。",
                "确定");
            return;
        }

        if (!TryBuildRemoteUrl(out string remoteUrl, out string error))
        {
            EditorUtility.DisplayDialog("地址无效", error, "确定");
            return;
        }

        string catalogHashPath = FindCatalogHashPath();
        if (string.IsNullOrEmpty(catalogHashPath))
        {
            EditorUtility.DisplayDialog(
                "没有可测试的 Catalog",
                "请先构建 Addressables，再执行连接测试。",
                "确定");
            return;
        }

        var baseUri = new Uri(remoteUrl.TrimEnd('/') + "/");
        bool isLocalServer = _lanAddresses.Any(
            address => address.Address == baseUri.Host);
        if (isLocalServer)
        {
            var loopbackUri = new UriBuilder(baseUri)
            {
                Host = IPAddress.Loopback.ToString()
            };
            baseUri = loopbackUri.Uri;
        }

        string testUrl = new Uri(
            baseUri,
            Path.GetFileName(catalogHashPath)).AbsoluteUri;
        _connectivityResult = $"测试中：{testUrl}";
        Repaint();

        _connectivityRequest = UnityWebRequest.Get(testUrl);
        _connectivityRequest.timeout =
            Mathf.Max(1, _catalogRequestTimeoutSeconds);
        _connectivityRequest.SetRequestHeader("Cache-Control", "no-cache");
        _connectivityRequest.SendWebRequest();

        _connectivityPoll = () =>
        {
            if (_connectivityRequest == null ||
                !_connectivityRequest.isDone)
                return;

            EditorApplication.update -= _connectivityPoll;
            _connectivityPoll = null;

            bool success =
                _connectivityRequest.result == UnityWebRequest.Result.Success;
            long responseCode = _connectivityRequest.responseCode;
            string requestError = _connectivityRequest.error;
            _connectivityResult = success
                ? $"成功：HTTP {responseCode}，{testUrl}"
                : $"失败：{requestError}（HTTP {responseCode}），{testUrl}";

            if (success)
                Debug.Log($"[Addressables LAN] Hosting 连接成功：{testUrl}");
            else
                Debug.LogError(
                    $"[Addressables LAN] Hosting 连接失败：{_connectivityResult}");

            _connectivityRequest.Dispose();
            _connectivityRequest = null;
            Repaint();
        };
        EditorApplication.update += _connectivityPoll;
    }

    private static string FindCatalogHashPath()
    {
        string root = Path.Combine(
            "ServerData",
            EditorUserBuildSettings.activeBuildTarget.ToString());
        if (!Directory.Exists(root))
            return null;

        return Directory
            .GetFiles(root, "catalog*.hash", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }

    private static void ApplyDevelopmentHttpPolicy()
    {
        PlayerSettings.insecureHttpOption =
            InsecureHttpOption.DevelopmentOnly;
        EditorUserBuildSettings.development = true;
        AssetDatabase.SaveAssets();
        Debug.Log(
            "[Addressables LAN] 已设置为仅 Development Build 允许 HTTP，并开启 Development Build。");
    }

    private bool TryBuildRemoteUrl(
        out string remoteUrl,
        out string error)
    {
        remoteUrl = string.Empty;
        error = string.Empty;

        string cleanHost = (_host ?? string.Empty).Trim();
        if (cleanHost.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            cleanHost = cleanHost.Substring("http://".Length);
        else if (cleanHost.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            cleanHost = cleanHost.Substring("https://".Length);

        int slashIndex = cleanHost.IndexOf('/');
        if (slashIndex >= 0)
            cleanHost = cleanHost.Substring(0, slashIndex);

        int colonIndex = cleanHost.LastIndexOf(':');
        if (colonIndex > 0 &&
            int.TryParse(cleanHost.Substring(colonIndex + 1), out int embeddedPort))
        {
            cleanHost = cleanHost.Substring(0, colonIndex);
            _port = embeddedPort;
        }

        if (string.IsNullOrWhiteSpace(cleanHost))
        {
            error = "请输入局域网 IP 或可解析的主机名。";
            return false;
        }

        if (_port < 1 || _port > 65535)
        {
            error = "端口必须在 1 到 65535 之间。";
            return false;
        }

        string scheme = _useHttps ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
        string candidate = $"{scheme}://{cleanHost}:{_port}";
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out Uri uri) ||
            string.IsNullOrWhiteSpace(uri.Host))
        {
            error = $"无法识别地址：{candidate}";
            return false;
        }

        _host = cleanHost;
        remoteUrl = candidate;
        return true;
    }

    private string BuildRemoteUrl()
    {
        TryBuildRemoteUrl(out string remoteUrl, out _);
        return remoteUrl;
    }

    private void RefreshLanAddresses()
    {
        _lanAddresses = DetectLanAddresses();
        if (string.IsNullOrWhiteSpace(_host) && _lanAddresses.Count > 0)
            _host = _lanAddresses[0].Address;
        Repaint();
    }

    private static List<LanAddress> DetectLanAddresses()
    {
        var result = new List<LanAddress>();
        foreach (NetworkInterface adapter in
                 NetworkInterface.GetAllNetworkInterfaces())
        {
            if (adapter.OperationalStatus != OperationalStatus.Up)
                continue;
            if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
                adapter.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
                continue;
            if (IsVirtualAdapter(adapter))
                continue;

            IPInterfaceProperties properties;
            try
            {
                properties = adapter.GetIPProperties();
            }
            catch (NetworkInformationException)
            {
                continue;
            }

            bool hasGateway = properties.GatewayAddresses.Any(
                gateway => gateway.Address.AddressFamily ==
                           AddressFamily.InterNetwork &&
                           !gateway.Address.Equals(IPAddress.Any));

            foreach (UnicastIPAddressInformation unicast in
                     properties.UnicastAddresses)
            {
                IPAddress address = unicast.Address;
                if (address.AddressFamily != AddressFamily.InterNetwork ||
                    IPAddress.IsLoopback(address) ||
                    address.ToString().StartsWith(
                        "169.254.",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                result.Add(new LanAddress
                {
                    Address = address.ToString(),
                    AdapterName = adapter.Name,
                    HasGateway = hasGateway
                });
            }
        }

        return result
            .OrderByDescending(address => address.HasGateway)
            .ThenBy(address => address.AdapterName)
            .ThenBy(address => address.Address)
            .ToList();
    }

    private static bool IsVirtualAdapter(NetworkInterface adapter)
    {
        string identity = $"{adapter.Name} {adapter.Description}".ToLowerInvariant();
        string[] virtualKeywords =
        {
            "virtual", "vmware", "hyper-v", "vbox", "loopback",
            "wsl", "docker", "tailscale", "zerotier"
        };
        return virtualKeywords.Any(identity.Contains);
    }
}
