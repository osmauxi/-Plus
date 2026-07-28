using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

/// <summary>
/// 编译全部 HybridCLR 热更程序集，并同步对应的 Addressables DLL 条目。
/// </summary>
public class HotUpdateBuilderTool
{
    private const string DestinationAssetFolder = "Assets/_HotUpdate/DLLS";
    private const string HotfixGroupName = "HotfixDLLs";
    private const string HotfixLabel = "Hotfix_DLL";

    /// <summary>
    /// 编译、复制并注册当前 HybridCLR 配置中的全部热更 DLL。
    /// </summary>
    [MenuItem("Tools/HotUpdate/Build And Sync DLLs")]
    public static void BuildAndCopyHotUpdateDlls()
    {
        Debug.Log("[HotUpdateBuilderTool] 开始执行 HybridCLR 编译...");

        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        CompileDllCommand.CompileDll(target);

        string sourceDirectory =
            SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
        string destinationDirectory = GetDestinationDirectory();
        Directory.CreateDirectory(destinationDirectory);

        IReadOnlyList<string> hotUpdateDlls =
            SettingsUtil.HotUpdateAssemblyFilesExcludePreserved.ToArray();
        CopyHotUpdateDlls(
            sourceDirectory,
            destinationDirectory,
            hotUpdateDlls);

        AssetDatabase.Refresh();
        SyncAddressableEntries(hotUpdateDlls);

        Debug.Log("<color=cyan><b>[HotUpdateBuilderTool] DLL更新完毕</b></color>");
    }

    /// <summary>
    /// 返回热更 DLL 在项目中的绝对输出目录。
    /// </summary>
    private static string GetDestinationDirectory()
    {
        return Path.GetFullPath(
            Path.Combine(Application.dataPath, "..", DestinationAssetFolder));
    }

    /// <summary>
    /// 把 HybridCLR 编译目录中的 DLL 复制为 Unity 可导入的 bytes 资源。
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
    /// 将当前热更 DLL 全量同步到指定 Addressables Group 和标签。
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
    /// 移除已经不属于 HybridCLR 配置的旧热更 Addressables 条目。
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
