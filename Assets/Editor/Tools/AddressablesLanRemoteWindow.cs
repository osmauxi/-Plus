using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 为局域网联调维护当前 Addressables Profile 的远端加载地址。
/// </summary>
public sealed class AddressablesLanRemotePanel
{
    private const int DefaultHostingPort = 64482;

    private readonly Action _repaint;
    private string _host = string.Empty;
    private int _port = DefaultHostingPort;
    private int _gamePort = 7777;
    private bool _useHttps;
    private int _catalogRequestTimeoutSeconds = 15;
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

    public AddressablesLanRemotePanel(Action repaint)
    {
        _repaint = repaint;
    }

    public void Enable()
    {
        RefreshLanAddresses();
        LoadCurrentRemoteAddress();
    }

    public void Disable()
    {
        if (_connectivityPoll != null)
            EditorApplication.update -= _connectivityPoll;
        _connectivityPoll = null;
        _connectivityRequest?.Dispose();
        _connectivityRequest = null;
    }

    public void Draw()
    {
        EditorGUILayout.LabelField("局域网发布配置", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "这里修改的是当前 Addressables Profile 的 Remote.LoadPath。" +
            "地址变化后需要重新构建 Addressables；客户端启动时会把 Catalog 中的" +
            "服务器地址替换为自动发现结果，因此无需仅为 IP 变化重打 Player。",
            MessageType.Info);

        DrawCurrentConfiguration();
        EditorGUILayout.Space(8f);
        DrawDetectedAddresses();
        EditorGUILayout.Space(8f);
        DrawAddressEditor();
        EditorGUILayout.Space(12f);
        DrawActions();
        EditorGUILayout.Space(12f);
        DrawDiscoveryService();

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
        EditorGUILayout.LabelField("当前 Profile", profileName);
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
            ApplyRemoteAddress();

        if (GUILayout.Button("应用开发测试 HTTP 策略"))
            ApplyDevelopmentHttpPolicy();

        if (GUILayout.Button("测试本机 Hosting 连接"))
            TestHostingConnection();
        EditorGUILayout.LabelField(
            "连接测试",
            _connectivityResult,
            EditorStyles.wordWrappedLabel);

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
            $"放行 UDP {ProjectGame.Bootstrap.LanDiscoveryProtocol.Port}、" +
            $"TCP {_port} 与 UDP {_gamePort}。",
            MessageType.Warning);
    }

    private void DrawDiscoveryService()
    {
        EditorGUILayout.LabelField("客户端自动发现", EditorStyles.boldLabel);
        _gamePort = EditorGUILayout.IntField("游戏端口", _gamePort);
        EditorGUILayout.LabelField(
            "发现服务",
            AddressablesLanDiscoveryService.IsRunning ? "运行中" : "未启动");

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("启动自动发现服务"))
        {
            AddressablesLanDiscoveryService.Start(
                _port,
                Mathf.Clamp(_gamePort, 1, 65535),
                _useHttps);
        }
        if (GUILayout.Button("停止自动发现服务"))
            AddressablesLanDiscoveryService.Stop();
        EditorGUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(AddressablesLanDiscoveryService.LastError))
        {
            EditorGUILayout.HelpBox(
                AddressablesLanDiscoveryService.LastError,
                MessageType.Error);
        }

        EditorGUILayout.HelpBox(
            "服务启动后，客户端会通过 UDP 广播取得本机 IP、资源端口、游戏端口与发布版本。" +
            "Addressables Hosting 仍需单独启动。",
            MessageType.Info);
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

    private bool ApplyRemoteAddress()
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
        EditorUtility.DisplayDialog(
            "远端地址已更新",
            $"Remote.LoadPath = {remoteUrl}\n\n" +
            "请重新构建 Addressables；现有 Player 会在启动时替换服务器地址。",
            "确定");

        _repaint();
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
        _repaint();

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
            _repaint();
        };
        EditorApplication.update += _connectivityPoll;
    }

    private static string FindCatalogHashPath()
    {
        AddressableAssetSettings settings =
            AddressableAssetSettingsDefaultObject.Settings;
        string root = settings.RemoteCatalogBuildPath.GetValue(settings);
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

    private void RefreshLanAddresses()
    {
        _lanAddresses = DetectLanAddresses();
        if (string.IsNullOrWhiteSpace(_host) && _lanAddresses.Count > 0)
            _host = _lanAddresses[0].Address;
        _repaint();
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
