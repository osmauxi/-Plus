using System;
using System.IO;
using System.Linq;
using ProjectGame.Bootstrap;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// Addressables 基线构建和版本维护入口。
/// </summary>
public static class AddressablesContentBuildTool
{
    internal const string BaselineRoot = "AddressablesBaselines";
    internal const string ReleaseRoot = "AddressablesReleases";
    internal const string ContentStateFileName = "addressables_content_state.bin";
    internal const string ReleaseManifestFileName = "release-manifest.json";

    [MenuItem("Tools/Addressables/发布中心")]
    private static void OpenDashboard()
    {
        AddressablesContentBuildWindow.Open();
    }

    public static void BuildNewBaseline()
    {
        TryBuildNewBaseline(
            PlayerSettings.bundleVersion,
            true,
            HotUpdateReleaseManifest.DefaultProtocolVersion,
            LanEndpoint.DefaultContentPort,
            LanEndpoint.DefaultGamePort,
            true);
    }

    internal static bool TryApplyBuildVersion(
        string version,
        out string normalizedVersion,
        out string error)
    {
        normalizedVersion = (version ?? string.Empty).Trim();
        error = string.Empty;

        if (!IsValidVersion(normalizedVersion, out error))
            return false;

        AddressableAssetSettings settings =
            AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            error = "项目中没有找到 AddressableAssetSettings。";
            return false;
        }

        PlayerSettings.bundleVersion = normalizedVersion;
        settings.OverridePlayerVersion = normalizedVersion;

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        Debug.Log(
            $"[ContentBuild] 已应用构建版本：{normalizedVersion}。" +
            "Player 与 Addressables Catalog 版本已同步。");
        return true;
    }

    internal static bool TryBuildNewBaseline(
        string version,
        bool buildPlayer,
        int protocolVersion,
        int contentPort,
        int gamePort,
        bool showCompletionDialog)
    {
        string normalizedVersion = (version ?? string.Empty).Trim();
        if (!IsValidVersion(normalizedVersion, out string versionError))
        {
            Debug.LogError($"[ContentBuild] {versionError}");
            if (showCompletionDialog)
            {
                EditorUtility.DisplayDialog(
                    "构建版本无效",
                    versionError,
                    "确定");
            }
            return false;
        }

        AddressableAssetSettings settings =
            AddressableAssetSettingsDefaultObject.Settings;
        string platform = GetCurrentPlatform();
        string baselineStatePath = GetBaselineStatePath(
            platform,
            normalizedVersion);

        // Baseline 一旦被正式 Player 使用，就不应被无意覆盖。
        if (File.Exists(baselineStatePath))
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "Baseline 已存在",
                $"当前 Baseline 已存在：\n\n" +
                $"{baselineStatePath}\n\n" +
                "覆盖后，旧 Player 的 Content Update 基准可能失效。\n" +
                "确定要重新构建并覆盖吗？",
                "覆盖",
                "取消");

            if (!overwrite)
                return false;
        }

        if (!TryApplyBuildVersion(
                normalizedVersion,
                out string appliedVersion,
                out versionError))
        {
            Debug.LogError($"[ContentBuild] {versionError}");
            if (showCompletionDialog)
            {
                EditorUtility.DisplayDialog(
                    "应用构建版本失败",
                    versionError,
                    "确定");
            }
            return false;
        }
        normalizedVersion = appliedVersion;

        try
        {
            Debug.Log(
                $"[ContentBuild] 开始构建 Baseline：" +
                $"{normalizedVersion} / {platform}");

            EditorUtility.DisplayProgressBar(
                "Addressables Baseline",
                "正在同步远端资源标签...",
                0.1f);
            AddressablesPreloadTool.SyncRemotePreloadLabel();

            EditorUtility.DisplayProgressBar(
                "Addressables Baseline",
                "正在生成并同步 HybridCLR 代码与 DLL...",
                0.35f);
            HotUpdateBuilderTool.GenerateAllAndSyncDlls();

            EditorUtility.DisplayProgressBar(
                "Addressables Baseline",
                "正在完整构建 Addressables...",
                0.65f);
            AddressableAssetSettings.BuildPlayerContent(
                out AddressablesPlayerBuildResult result);

            if (!string.IsNullOrEmpty(result.Error))
                throw new InvalidOperationException(result.Error);

            string generatedStatePath =
                FindGeneratedContentState(settings, platform);
            if (string.IsNullOrEmpty(generatedStatePath))
            {
                throw new FileNotFoundException(
                    "Addressables 构建成功，但找不到 " +
                    ContentStateFileName);
            }

            string baselineDirectory = Path.GetDirectoryName(
                baselineStatePath);
            if (string.IsNullOrEmpty(baselineDirectory))
                throw new InvalidOperationException("无法确定 Baseline 保存目录。");

            Directory.CreateDirectory(baselineDirectory);
            File.Copy(generatedStatePath, baselineStatePath, true);

            string playerOutput = string.Empty;
            if (buildPlayer)
            {
                EditorUtility.DisplayProgressBar(
                    "Addressables Baseline",
                    "正在构建匹配此基线的 Player...",
                    0.85f);
                playerOutput = BuildPlayer(normalizedVersion, platform);
            }

            WriteReleaseManifest(
                platform,
                normalizedVersion,
                0,
                protocolVersion,
                contentPort,
                gamePort);

            string message =
                $"版本：{normalizedVersion}\n" +
                $"平台：{platform}\n" +
                $"状态文件：{baselineStatePath}\n" +
                $"发布清单：{GetServerManifestPath()}\n" +
                (buildPlayer ? $"Player：{playerOutput}\n" : string.Empty) +
                $"耗时：{result.Duration:F2} 秒";

            Debug.Log($"[ContentBuild] Baseline 构建完成\n{message}");
            if (showCompletionDialog)
            {
                EditorUtility.DisplayDialog(
                    "Baseline 构建完成",
                    message,
                    "确定");
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            if (showCompletionDialog)
            {
                EditorUtility.DisplayDialog(
                    "Baseline 构建失败",
                    exception.Message,
                    "确定");
            }
            return false;
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    internal static bool TryBuildContentUpdate(
        BaselineSnapshotInfo baseline,
        int contentRevision,
        int protocolVersion,
        int contentPort,
        int gamePort)
    {
        if (baseline == null)
        {
            EditorUtility.DisplayDialog("无法构建更新", "请先选择一个基线。", "确定");
            return false;
        }

        string platform = GetCurrentPlatform();
        HotUpdateReleaseManifest published = ReadPublishedManifest();
        if (published != null &&
            published.BaselineVersion == baseline.Version &&
            contentRevision <= published.ContentRevision)
        {
            EditorUtility.DisplayDialog(
                "内容修订号无效",
                $"当前已发布 {published.BaselineVersion}." +
                $"{published.ContentRevision}，新修订号必须更大。",
                "确定");
            return false;
        }

        try
        {
            Debug.Log(
                $"[ContentBuild] 开始构建 Content Update：" +
                $"{baseline.Version}.{contentRevision} / {platform}");

            EditorUtility.DisplayProgressBar(
                "Addressables Content Update",
                "正在同步远端资源标签...",
                0.15f);
            AddressablesPreloadTool.SyncRemotePreloadLabel();

            EditorUtility.DisplayProgressBar(
                "Addressables Content Update",
                "正在编译并同步 HotFix DLL...",
                0.4f);
            HotUpdateBuilderTool.BuildAndCopyHotUpdateDlls();

            EditorUtility.DisplayProgressBar(
                "Addressables Content Update",
                "正在根据已选基线生成差异内容...",
                0.7f);
            AddressablesPlayerBuildResult result =
                ContentUpdateScript.BuildContentUpdate(
                    AddressableAssetSettingsDefaultObject.Settings,
                    baseline.StatePath);
            if (result == null)
                throw new InvalidOperationException("基线状态文件无效，Addressables 拒绝构建更新。");
            if (!string.IsNullOrEmpty(result.Error))
                throw new InvalidOperationException(result.Error);

            HotUpdateReleaseManifest manifest = WriteReleaseManifest(
                platform,
                baseline.Version,
                contentRevision,
                protocolVersion,
                contentPort,
                gamePort);

            string message =
                $"发布：{manifest.BaselineVersion}.{manifest.ContentRevision}\n" +
                $"协议：{manifest.ProtocolVersion}\n" +
                $"基线状态：{baseline.StatePath}\n" +
                $"发布目录：{GetRemoteBuildDirectory()}\n" +
                $"耗时：{result.Duration:F2} 秒";
            Debug.Log($"[ContentBuild] Content Update 构建完成\n{message}");
            EditorUtility.DisplayDialog("Content Update 构建完成", message, "确定");
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog("Content Update 构建失败", exception.Message, "确定");
            return false;
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    internal static string GetCurrentPlatform()
    {
        return PlatformMappingService.GetPlatformPathSubFolder();
    }

    internal static BaselineSnapshotInfo[] GetBaselineSnapshots(string platform)
    {
        string platformDirectory = Path.Combine(BaselineRoot, platform);
        if (!Directory.Exists(platformDirectory))
            return Array.Empty<BaselineSnapshotInfo>();

        return Directory
            .GetDirectories(platformDirectory)
            .Select(directory =>
            {
                string statePath = Path.Combine(
                    directory,
                    ContentStateFileName);
                if (!File.Exists(statePath))
                    return null;

                var fileInfo = new FileInfo(statePath);
                return new BaselineSnapshotInfo(
                    Path.GetFileName(directory),
                    statePath,
                    fileInfo.LastWriteTime,
                    fileInfo.Length);
            })
            .Where(snapshot => snapshot != null)
            .OrderByDescending(snapshot => snapshot.LastWriteTime)
            .ToArray();
    }

    internal static string GetBaselineStatePath(
        string platform,
        string version)
    {
        return Path.Combine(
            BaselineRoot,
            platform,
            version,
            ContentStateFileName);
    }

    internal static string GetRemoteBuildDirectory()
    {
        AddressableAssetSettings settings =
            AddressableAssetSettingsDefaultObject.Settings;
        return settings.RemoteCatalogBuildPath.GetValue(settings);
    }

    internal static string GetServerManifestPath()
    {
        return Path.Combine(
            GetRemoteBuildDirectory(),
            ReleaseManifestFileName);
    }

    internal static HotUpdateReleaseManifest ReadPublishedManifest()
    {
        string path = GetServerManifestPath();
        return File.Exists(path)
            ? JsonUtility.FromJson<HotUpdateReleaseManifest>(File.ReadAllText(path))
            : null;
    }

    private static HotUpdateReleaseManifest WriteReleaseManifest(
        string platform,
        string baselineVersion,
        int contentRevision,
        int protocolVersion,
        int contentPort,
        int gamePort)
    {
        var manifest = new HotUpdateReleaseManifest
        {
            BaselineVersion = baselineVersion,
            ContentRevision = Mathf.Max(0, contentRevision),
            ProtocolVersion = Mathf.Max(1, protocolVersion),
            ContentPort = Mathf.Clamp(contentPort, 1, 65535),
            GamePort = Mathf.Clamp(gamePort, 1, 65535),
            CatalogHash = ReadLatestCatalogHash(),
            BuildTarget = EditorUserBuildSettings.activeBuildTarget.ToString(),
            BuiltAtUtc = DateTime.UtcNow.ToString("O")
        };

        string json = JsonUtility.ToJson(manifest, true);
        string serverPath = GetServerManifestPath();
        Directory.CreateDirectory(Path.GetDirectoryName(serverPath));
        WriteTextAtomically(serverPath, json);

        string archivePath = Path.Combine(
            ReleaseRoot,
            platform,
            baselineVersion,
            $"content-{manifest.ContentRevision:D4}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(archivePath));
        File.WriteAllText(archivePath, json);
        AssetDatabase.Refresh();
        return manifest;
    }

    private static void WriteTextAtomically(string path, string contents)
    {
        string temporaryPath = path + ".tmp";
        File.WriteAllText(temporaryPath, contents);
        if (File.Exists(path))
            File.Replace(temporaryPath, path, null);
        else
            File.Move(temporaryPath, path);
    }

    private static string ReadLatestCatalogHash()
    {
        string serverDirectory = GetRemoteBuildDirectory();
        if (!Directory.Exists(serverDirectory))
            return string.Empty;

        string hashPath = Directory
            .GetFiles(serverDirectory, "catalog*.hash", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
        return hashPath == null ? string.Empty : File.ReadAllText(hashPath).Trim();
    }

    private static string BuildPlayer(string version, string platform)
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
        if (scenes.Length == 0)
            throw new InvalidOperationException("Build Settings 中没有启用的场景。");

        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        string productName = PlayerSettings.productName;
        string outputName = target == BuildTarget.StandaloneWindows ||
                            target == BuildTarget.StandaloneWindows64
            ? productName + ".exe"
            : target == BuildTarget.StandaloneOSX
                ? productName + ".app"
                : productName;
        string outputPath = Path.Combine("Builds", platform, version, outputName);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

        BuildOptions options = EditorUserBuildSettings.development
            ? BuildOptions.Development
            : BuildOptions.None;
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = target,
            options = options
        });
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Player 构建失败：{report.summary.result}，" +
                $"错误 {report.summary.totalErrors} 个。");
        }

        return outputPath;
    }

    private static bool IsValidVersion(string version, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(version))
        {
            error = "版本不能为空。";
            return false;
        }

        if (version == "." ||
            version == ".." ||
            version.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            version.Contains("/") ||
            version.Contains("\\"))
        {
            error = "版本会作为 Baseline 目录名使用，不能包含路径或非法字符。";
            return false;
        }

        return true;
    }

    private static string FindGeneratedContentState(
        AddressableAssetSettings settings,
        string platform)
    {
        string defaultPath = Path.Combine(
            settings.ConfigFolder,
            platform,
            ContentStateFileName);
        if (File.Exists(defaultPath))
            return defaultPath;

        if (!string.IsNullOrWhiteSpace(settings.ContentStateBuildPath))
        {
            string customPath = Path.Combine(
                settings.ContentStateBuildPath,
                ContentStateFileName);
            if (File.Exists(customPath))
                return customPath;
        }

        if (!Directory.Exists(settings.ConfigFolder))
            return null;

        return Directory
            .GetFiles(
                settings.ConfigFolder,
                ContentStateFileName,
                SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }
}

internal sealed class BaselineSnapshotInfo
{
    public BaselineSnapshotInfo(
        string version,
        string statePath,
        DateTime lastWriteTime,
        long fileSize)
    {
        Version = version;
        StatePath = statePath;
        LastWriteTime = lastWriteTime;
        FileSize = fileSize;
    }

    public string Version { get; }
    public string StatePath { get; }
    public DateTime LastWriteTime { get; }
    public long FileSize { get; }
}

/// <summary>
/// 显示基线状态，并维护下一次 Player/Addressables 构建使用的版本。
/// </summary>
public sealed class AddressablesContentBuildWindow : EditorWindow
{
    internal const int LanTabIndex = 1;

    private const string WindowTitle = "Addressables 发布中心";
    private static readonly string[] TabNames =
    {
        "版本与构建",
        "局域网",
        "辅助工具"
    };

    private string _buildVersion;
    private string _platform;
    private BaselineSnapshotInfo[] _baselineSnapshots =
        Array.Empty<BaselineSnapshotInfo>();
    private Vector2 _scrollPosition;
    private string _statusMessage;
    private MessageType _statusType = MessageType.Info;
    private int _selectedTab;
    private int _selectedBaselineIndex;
    private int _contentRevision = 1;
    private int _protocolVersion = HotUpdateReleaseManifest.DefaultProtocolVersion;
    private int _contentPort = LanEndpoint.DefaultContentPort;
    private int _gamePort = LanEndpoint.DefaultGamePort;
    private bool _buildPlayerWithBaseline = true;
    private AddressablesLanRemotePanel _lanPanel;

    internal static void Open(int selectedTab = 0)
    {
        var window = GetWindow<AddressablesContentBuildWindow>();
        window.titleContent = new GUIContent(WindowTitle);
        window.minSize = new Vector2(620f, 480f);
        window._selectedTab = Mathf.Clamp(selectedTab, 0, TabNames.Length - 1);
        window.RefreshState(resetBuildVersion: true);
        window.Show();
    }

    private void OnEnable()
    {
        _lanPanel = new AddressablesLanRemotePanel(Repaint);
        _lanPanel.Enable();
        RefreshState(resetBuildVersion: true);
    }

    private void OnDisable()
    {
        _lanPanel?.Disable();
    }

    private void OnFocus()
    {
        RefreshState(resetBuildVersion: false);
    }

    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        EditorGUILayout.LabelField(
            "Addressables 发布中心",
            EditorStyles.boldLabel);
        _selectedTab = GUILayout.Toolbar(_selectedTab, TabNames);
        EditorGUILayout.Space(10f);

        if (_selectedTab == LanTabIndex)
            _lanPanel.Draw();
        else if (_selectedTab == 2)
            DrawUtilitySection();
        else
            DrawBuildPage();

        if (!string.IsNullOrEmpty(_statusMessage))
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(_statusMessage, _statusType);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawBuildPage()
    {
        EditorGUILayout.HelpBox(
            "Baseline 是 Content Update 的历史参照；本次构建版本会同时写入 " +
            "PlayerSettings.bundleVersion 与 Addressables Catalog。",
            MessageType.Info);

        DrawBaselineSection();
        EditorGUILayout.Space(12f);
        DrawVersionSection();
        EditorGUILayout.Space(12f);
        DrawBuildSection();
        EditorGUILayout.Space(12f);
        DrawPendingSection();
    }

    private static void DrawUtilitySection()
    {
        EditorGUILayout.LabelField("辅助工具", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "这些操作已收拢到发布中心；原有实现保持独立，便于单独维护。",
            MessageType.Info);

        if (GUILayout.Button("扫描并登记遗漏的 HotFix 程序集", GUILayout.Height(30f)))
            HotUpdateBuilderTool.RegisterMissingHotFixAssemblies();

        if (GUILayout.Button("编译并同步 HotFix DLL", GUILayout.Height(30f)))
            HotUpdateBuilderTool.BuildAndCopyHotUpdateDlls();

        if (GUILayout.Button("同步远端 Preload 标签", GUILayout.Height(30f)))
            AddressablesPreloadTool.SyncRemotePreloadLabel();

        EditorGUILayout.Space(10f);
        if (GUILayout.Button("打开 Addressables Groups"))
        {
            EditorApplication.ExecuteMenuItem(
                "Window/Asset Management/Addressables/Groups");
        }

        if (GUILayout.Button("打开 Addressables Hosting"))
        {
            EditorApplication.ExecuteMenuItem(
                "Window/Asset Management/Addressables/Hosting");
        }
    }

    private void DrawBaselineSection()
    {
        EditorGUILayout.LabelField("当前基线", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("目标平台", _platform ?? "未知");

        BaselineSnapshotInfo current = GetSelectedBaseline();
        if (current == null)
        {
            EditorGUILayout.HelpBox(
                "当前平台还没有 Baseline。首次发布前需要创建一个完整基线包。",
                MessageType.Warning);
            return;
        }

        string[] baselineNames = _baselineSnapshots
            .Select(snapshot => snapshot.Version)
            .ToArray();
        _selectedBaselineIndex = EditorGUILayout.Popup(
            "用于内容更新的基线",
            _selectedBaselineIndex,
            baselineNames);
        current = GetSelectedBaseline();

        EditorGUILayout.LabelField("已选基线版本", current.Version);
        EditorGUILayout.LabelField(
            "生成时间",
            current.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"));
        EditorGUILayout.LabelField("状态文件大小", $"{current.FileSize:N0} bytes");
        EditorGUILayout.LabelField("状态文件", current.StatePath);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("在资源管理器中显示"))
            EditorUtility.RevealInFinder(Path.GetFullPath(current.StatePath));
        if (GUILayout.Button("刷新状态"))
            RefreshState(resetBuildVersion: false);
        EditorGUILayout.EndHorizontal();

        HotUpdateReleaseManifest published =
            AddressablesContentBuildTool.ReadPublishedManifest();
        EditorGUILayout.LabelField(
            "当前发布",
            published == null
                ? "尚无发布清单"
                : $"{published.BaselineVersion}.{published.ContentRevision} / " +
                  $"协议 {published.ProtocolVersion}");
    }

    private void DrawVersionSection()
    {
        AddressableAssetSettings settings =
            AddressableAssetSettingsDefaultObject.Settings;

        EditorGUILayout.LabelField("本次构建版本", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "当前 Player 版本",
            PlayerSettings.bundleVersion);
        EditorGUILayout.LabelField(
            "当前 Catalog 版本",
            settings != null ? settings.OverridePlayerVersion : "未找到 Settings");

        _buildVersion = EditorGUILayout.TextField(
            "修改为",
            _buildVersion ?? string.Empty);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("应用版本"))
            ApplyBuildVersion();
        if (GUILayout.Button("恢复当前版本"))
            _buildVersion = PlayerSettings.bundleVersion;
        EditorGUILayout.EndHorizontal();

        if (_baselineSnapshots.Any(
                snapshot => snapshot.Version == _buildVersion?.Trim()))
        {
            EditorGUILayout.HelpBox(
                "该版本已经存在 Baseline。再次创建将触发覆盖确认。" +
                "常规资源热更不应覆盖旧基线。",
                MessageType.Warning);
        }
    }

    private void DrawBuildSection()
    {
        EditorGUILayout.LabelField("构建操作", EditorStyles.boldLabel);

        _protocolVersion = Mathf.Max(
            1,
            EditorGUILayout.IntField("联机协议版本", _protocolVersion));
        _contentPort = EditorGUILayout.IntField("资源端口", _contentPort);
        _gamePort = EditorGUILayout.IntField("游戏端口", _gamePort);
        _buildPlayerWithBaseline = EditorGUILayout.Toggle(
            "基线同时构建 Player",
            _buildPlayerWithBaseline);

        GUI.backgroundColor = new Color(0.65f, 0.85f, 1f);
        if (GUILayout.Button("应用版本并创建完整基线包", GUILayout.Height(34f)))
        {
            if (AddressablesContentBuildTool.TryBuildNewBaseline(
                    _buildVersion,
                    _buildPlayerWithBaseline,
                    _protocolVersion,
                    _contentPort,
                    _gamePort,
                    true))
            {
                _statusMessage = "Baseline 构建完成。";
                _statusType = MessageType.Info;
                RefreshState(resetBuildVersion: true);
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.HelpBox(
            "基线会执行 HybridCLR Generate/All、完整构建 Addressables、保存 " +
            "content state 与发布清单；可同时产出严格匹配的 Player。",
            MessageType.None);

        EditorGUILayout.Space(8f);
        _contentRevision = Mathf.Max(
            1,
            EditorGUILayout.IntField("内容修订号", _contentRevision));

        GUI.backgroundColor = new Color(0.65f, 1f, 0.72f);
        if (GUILayout.Button("按已选基线构建 Content Update", GUILayout.Height(34f)))
        {
            if (AddressablesContentBuildTool.TryBuildContentUpdate(
                    GetSelectedBaseline(),
                    _contentRevision,
                    _protocolVersion,
                    _contentPort,
                    _gamePort))
            {
                _statusMessage =
                    $"Content Update {_contentRevision} 构建完成。";
                _statusType = MessageType.Info;
                RefreshState(resetBuildVersion: false);
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.HelpBox(
            "内容更新只编译 HotFix DLL，并使用已选 content state 生成差异 Catalog/Bundle；" +
            "不会覆盖基线，也不会重新构建 Player。",
            MessageType.None);
    }

    private void DrawPendingSection()
    {
        EditorGUILayout.LabelField("当前逻辑边界", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "已覆盖基线、Content Update、发布清单和联机版本校验。" +
            "暂未加入原子目录切换、清单签名与分阶段下载；发布时请等构建完成后再让客户端连接。",
            MessageType.Warning);
    }

    private void ApplyBuildVersion()
    {
        if (!AddressablesContentBuildTool.TryApplyBuildVersion(
                _buildVersion,
                out string normalizedVersion,
                out string error))
        {
            _statusMessage = error;
            _statusType = MessageType.Error;
            return;
        }

        _buildVersion = normalizedVersion;
        _statusMessage =
            $"已应用版本 {normalizedVersion}，后续 Player 与 Catalog 将使用该版本。";
        _statusType = MessageType.Info;
        RefreshState(resetBuildVersion: false);
    }

    private void RefreshState(bool resetBuildVersion)
    {
        string selectedVersion = GetSelectedBaseline()?.Version;
        _platform = AddressablesContentBuildTool.GetCurrentPlatform();
        _baselineSnapshots =
            AddressablesContentBuildTool.GetBaselineSnapshots(_platform);

        int preservedIndex = Array.FindIndex(
            _baselineSnapshots,
            snapshot => snapshot.Version == selectedVersion);
        _selectedBaselineIndex = preservedIndex >= 0 ? preservedIndex : 0;

        HotUpdateReleaseManifest published =
            AddressablesContentBuildTool.ReadPublishedManifest();
        if (published != null)
        {
            _protocolVersion = published.ProtocolVersion;
            _contentPort = published.ContentPort;
            _gamePort = published.GamePort;
            _contentRevision = Mathf.Max(1, published.ContentRevision + 1);
        }

        if (resetBuildVersion || string.IsNullOrWhiteSpace(_buildVersion))
            _buildVersion = PlayerSettings.bundleVersion;

        Repaint();
    }

    private BaselineSnapshotInfo GetSelectedBaseline()
    {
        return _baselineSnapshots.Length == 0
            ? null
            : _baselineSnapshots[Mathf.Clamp(
                _selectedBaselineIndex,
                0,
                _baselineSnapshots.Length - 1)];
    }
}
