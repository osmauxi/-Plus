using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// 编译全部 HybridCLR 热更程序集，并同步对应的 Addressables DLL 条目 
/// </summary>
public class HotUpdateBuilderTool
{
    private const string DestinationAssetFolder = "Assets/_HotUpdate/DLLS";
    private const string HotfixGroupName = "HotfixDLLs";
    private const string HotfixLabel = "Hotfix_DLL";
    private const string AotMetadataLabel = "AOT_DLL";
    private const string HotfixAssemblyPrefix = "HotFix.";
    private const string HotfixSourceRoot = "Assets/_HotUpdate/Scripts";

    [Serializable]
    private sealed class AssemblyDefinitionData
    {
        public string name;
        public string[] includePlatforms;
        public string[] optionalUnityReferences;
        public string[] defineConstraints;
    }

    /// <summary>
    /// 扫描热更源码目录，把尚未登记的运行时 HotFix.* asmdef 加入 HybridCLR。
    /// 测试程序集与 Editor-only 程序集不会被自动加入。
    /// </summary>
    [MenuItem("Tools/HotUpdate/扫描并登记遗漏程序集")]
    public static void RegisterMissingHotFixAssemblies()
    {
        IReadOnlyList<string> addedAssemblies =
            RegisterMissingHotFixAssembliesInternal();
        string message = addedAssemblies.Count == 0
            ? "所有运行时 HotFix.* 程序集都已加入 HybridCLR 热更管线。"
            : "已加入以下热更程序集：\n\n" +
              string.Join("\n", addedAssemblies) +
              "\n\n请继续执行 HybridCLR Generate/All，再构建并同步 DLL。";

        EditorUtility.DisplayDialog("HybridCLR 热更程序集同步", message, "确定");
    }

    /// <summary>
    /// 编译、复制并注册当前 HybridCLR 配置中的全部热更 DLL 
    /// </summary>
    [MenuItem("Tools/HotUpdate/编译并同步 DLL")]
    public static void BuildAndCopyHotUpdateDlls()
    {
        RegisterMissingHotFixAssembliesInternal();
        Debug.Log("[HotUpdateBuilderTool] 开始执行 HybridCLR 编译...");

        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        CompileDllCommand.CompileDll(target);
        SyncCompiledDlls(target);
    }

    /// <summary>
    /// 新 Player 基线使用：刷新 HybridCLR 生成代码，再同步热更与 AOT 元数据 DLL。
    /// </summary>
    public static void GenerateAllAndSyncDlls()
    {
        RegisterMissingHotFixAssembliesInternal();
        Debug.Log("[HotUpdateBuilderTool] 开始执行 HybridCLR Generate/All...");
        PrebuildCommand.GenerateAll();
        SyncCompiledDlls(EditorUserBuildSettings.activeBuildTarget);
    }

    private static void SyncCompiledDlls(BuildTarget target)
    {
        string sourceDirectory =
            SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
        string destinationDirectory = GetDestinationDirectory();
        Directory.CreateDirectory(destinationDirectory);

        IReadOnlyList<string> hotUpdateDlls =
            SettingsUtil.HotUpdateAssemblyFilesExcludePreserved.ToArray();
        IReadOnlyList<string> supplementalMetadataAssemblies =
            GetSupplementalMetadataAssemblies();
        CopyHotUpdateDlls(
            sourceDirectory,
            destinationDirectory,
            hotUpdateDlls);

        string aotMetadataSourceDirectory =
            SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
        CopyHotUpdateDlls(
            aotMetadataSourceDirectory,
            destinationDirectory,
            supplementalMetadataAssemblies);

        AssetDatabase.Refresh();
        SyncAddressableEntries(hotUpdateDlls);
        SyncAotMetadataEntries(supplementalMetadataAssemblies);

        Debug.Log(
            "<color=cyan><b>[HotUpdateBuilderTool] HotFix DLL 与 AOT 补充元数据更新完毕</b></color>");
    }

    private static IReadOnlyList<string>
        RegisterMissingHotFixAssembliesInternal()
    {
        HybridCLRSettings settings = HybridCLRSettings.Instance;
        var definitions = (settings.hotUpdateAssemblyDefinitions ??
                           Array.Empty<AssemblyDefinitionAsset>())
            .Where(definition => definition != null)
            .ToList();
        var registeredNames = new HashSet<string>(
            definitions
                .Select(ReadAssemblyDefinition)
                .Where(data => data != null &&
                               !string.IsNullOrWhiteSpace(data.name))
                .Select(data => data.name),
            StringComparer.Ordinal);
        var candidates = new List<(string Name,
            AssemblyDefinitionAsset Asset)>();

        foreach (string guid in AssetDatabase.FindAssets(
                     "t:AssemblyDefinitionAsset",
                     new[] { HotfixSourceRoot }))
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            AssemblyDefinitionAsset asset =
                AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(
                    assetPath);
            AssemblyDefinitionData data = ReadAssemblyDefinition(asset);
            if (!IsRuntimeHotFixAssembly(data, assetPath))
                continue;

            candidates.Add((data.name, asset));
        }

        var addedAssemblies = new List<string>();
        foreach ((string name, AssemblyDefinitionAsset asset) in candidates
                     .OrderBy(candidate => candidate.Name,
                         StringComparer.Ordinal))
        {
            if (!registeredNames.Add(name))
                continue;

            definitions.Add(asset);
            addedAssemblies.Add(name);
        }

        if (addedAssemblies.Count == 0)
        {
            Debug.Log(
                "[HotUpdateBuilderTool] HybridCLR 热更程序集登记已是最新状态");
            return addedAssemblies;
        }

        settings.hotUpdateAssemblyDefinitions = definitions.ToArray();
        HybridCLRSettings.Save();
        AssetDatabase.SaveAssets();
        Debug.Log(
            "[HotUpdateBuilderTool] 已自动加入 HybridCLR 热更程序集：" +
            string.Join(", ", addedAssemblies));
        return addedAssemblies;
    }

    private static AssemblyDefinitionData ReadAssemblyDefinition(
        AssemblyDefinitionAsset asset)
    {
        return asset == null
            ? null
            : JsonUtility.FromJson<AssemblyDefinitionData>(asset.text);
    }

    private static bool IsRuntimeHotFixAssembly(
        AssemblyDefinitionData data,
        string assetPath)
    {
        if (data == null ||
            string.IsNullOrWhiteSpace(data.name) ||
            !data.name.StartsWith(
                HotfixAssemblyPrefix,
                StringComparison.Ordinal))
        {
            return false;
        }

        string normalizedPath = assetPath.Replace('\\', '/');
        if (normalizedPath.IndexOf(
                "/Editor/",
                StringComparison.OrdinalIgnoreCase) >= 0 ||
            normalizedPath.IndexOf(
                "/Tests/",
                StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return false;
        }

        if (data.includePlatforms != null &&
            data.includePlatforms.Length > 0 &&
            data.includePlatforms.All(platform =>
                platform.Equals(
                    "Editor",
                    StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        bool usesTestAssemblies =
            data.optionalUnityReferences?.Any(reference =>
                reference.Equals(
                    "TestAssemblies",
                    StringComparison.OrdinalIgnoreCase)) == true;
        bool requiresUnityTests = data.defineConstraints?.Any(constraint =>
            constraint.IndexOf(
                "UNITY_INCLUDE_TESTS",
                StringComparison.OrdinalIgnoreCase) >= 0) == true;
        return !usesTestAssemblies && !requiresUnityTests;
    }

    /// <summary>
    /// 使用 HybridCLR 分析器生成的完整 AOT 补充列表，避免手工列表遗漏
    /// AOT_Core 等实际承载泛型实现的程序集。
    /// </summary>
    private static IReadOnlyList<string> GetSupplementalMetadataAssemblies()
    {
        Type generatedType =
            Type.GetType("AOTGenericReferences, Assembly-CSharp");
        object generatedValue = generatedType?
            .GetField("PatchedAOTAssemblyList")?
            .GetValue(null);
        if (generatedValue is not IEnumerable<string> generatedList)
        {
            throw new InvalidOperationException(
                "无法读取 HybridCLR 的 PatchedAOTAssemblyList。请先执行 Generate/All，" +
                "再构建并同步热更 DLL。");
        }

        string[] assemblies = generatedList
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        if (assemblies.Length == 0)
        {
            throw new InvalidOperationException(
                "HybridCLR 的 PatchedAOTAssemblyList 为空。请先执行 Generate/All，" +
                "再构建并同步热更 DLL。");
        }

        return assemblies;
    }

    /// <summary>
    /// 返回热更 DLL 在项目中的绝对输出目录 
    /// </summary>
    private static string GetDestinationDirectory()
    {
        return Path.GetFullPath(
            Path.Combine(Application.dataPath, "..", DestinationAssetFolder));
    }

    /// <summary>
    /// 把 HybridCLR 编译目录中的 DLL 复制为 Unity 可导入的 bytes 资源 
    /// </summary>
    private static void CopyHotUpdateDlls(
        string sourceDirectory,
        string destinationDirectory,
        IReadOnlyList<string> dllNames)
    {
        foreach (string dllName in dllNames)
        {
            string sourcePath = Path.Combine(sourceDirectory, dllName);
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException(
                    $"找不到 HybridCLR 编译产物：{sourcePath}",
                    sourcePath);

            string destinationPath =
                Path.Combine(destinationDirectory, dllName + ".bytes");
            File.Copy(sourcePath, destinationPath, true);
            Debug.Log($"[DLL同步] 成功更新热更程序集: {dllName}.bytes");
        }
    }

    /// <summary>
    /// 将当前热更 DLL 全量同步到指定 Addressables Group 和标签 
    /// </summary>
    private static void SyncAddressableEntries(IReadOnlyList<string> dllNames)
    {
        AddressableAssetSettings settings =
            AddressableAssetSettingsDefaultObject.Settings;
        AddressableAssetGroup group = settings.FindGroup(HotfixGroupName);
        if (group == null)
            throw new InvalidOperationException(
                $"Addressables 中不存在 Group：{HotfixGroupName}");

        settings.AddLabel(HotfixLabel);
        var desiredGuids = new HashSet<string>();

        foreach (string dllName in dllNames)
        {
            string assetPath = $"{DestinationAssetFolder}/{dllName}.bytes";
            AssetDatabase.ImportAsset(
                assetPath,
                ImportAssetOptions.ForceSynchronousImport);

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrWhiteSpace(guid))
                throw new InvalidOperationException(
                    $"无法取得热更 DLL 的资源 GUID：{assetPath}");

            desiredGuids.Add(guid);
            AddressableAssetEntry entry =
                settings.CreateOrMoveEntry(guid, group, false, false);
            entry.address = dllName;
            entry.SetLabel(HotfixLabel, true, true, false);
        }

        RemoveStaleHotfixEntries(settings, group, desiredGuids);
        settings.SetDirty(
            AddressableAssetSettings.ModificationEvent.EntryMoved,
            group,
            true,
            true);
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// 将热更代码会调用到泛型方法的裁剪后 AOT 程序集加入 AOT_DLL 标签。
    /// </summary>
    private static void SyncAotMetadataEntries(
        IReadOnlyList<string> dllNames)
    {
        AddressableAssetSettings settings =
            AddressableAssetSettingsDefaultObject.Settings;
        AddressableAssetGroup group = settings.FindGroup(HotfixGroupName);
        if (group == null)
            throw new InvalidOperationException(
                $"Addressables 中不存在 Group：{HotfixGroupName}");

        settings.AddLabel(AotMetadataLabel);
        var desiredGuids = new HashSet<string>();

        foreach (string dllName in dllNames)
        {
            string assetPath = $"{DestinationAssetFolder}/{dllName}.bytes";
            AssetDatabase.ImportAsset(
                assetPath,
                ImportAssetOptions.ForceSynchronousImport);

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrWhiteSpace(guid))
                throw new InvalidOperationException(
                    $"无法取得 AOT 补充元数据的资源 GUID：{assetPath}");

            desiredGuids.Add(guid);
            AddressableAssetEntry entry =
                settings.CreateOrMoveEntry(guid, group, false, false);
            entry.address = dllName;
            entry.SetLabel(AotMetadataLabel, true, true, false);
        }

        AddressableAssetEntry[] entries = group.entries.ToArray();
        foreach (AddressableAssetEntry entry in entries)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(entry.guid);
            bool isManagedAotEntry =
                entry.labels.Contains(AotMetadataLabel) &&
                assetPath.StartsWith(
                    DestinationAssetFolder,
                    StringComparison.OrdinalIgnoreCase);

            if (isManagedAotEntry && !desiredGuids.Contains(entry.guid))
                settings.RemoveAssetEntry(entry.guid, false);
        }

        settings.SetDirty(
            AddressableAssetSettings.ModificationEvent.EntryMoved,
            group,
            true,
            true);
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// 移除已经不属于 HybridCLR 配置的旧热更 Addressables 条目 
    /// </summary>
    private static void RemoveStaleHotfixEntries(
        AddressableAssetSettings settings,
        AddressableAssetGroup group,
        IReadOnlyCollection<string> desiredGuids)
    {
        AddressableAssetEntry[] entries = group.entries.ToArray();
        foreach (AddressableAssetEntry entry in entries)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(entry.guid);
            bool isManagedHotfixEntry =
                entry.labels.Contains(HotfixLabel) &&
                assetPath.StartsWith(
                    DestinationAssetFolder,
                    StringComparison.OrdinalIgnoreCase);

            if (isManagedHotfixEntry && !desiredGuids.Contains(entry.guid))
                settings.RemoveAssetEntry(entry.guid, false);
        }
    }
}
