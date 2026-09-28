using System;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public static class AddressablesPreloadTool
{
    private const string PreloadLabel = "Preload";

    [MenuItem("Tools/Addressables/同步远端 Preload 标签")]
    public static void SyncRemotePreloadLabel()
    {
        AddressableAssetSettings settings =
            AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null)
        {
            Debug.LogError("[Preload] 找不到 AddressableAssetSettings");
            return;
        }

        // 确保 Addressables Settings 中存在 Preload 标签。
        settings.AddLabel(PreloadLabel, false);

        int remoteGroupCount = 0;
        int entryCount = 0;

        foreach (AddressableAssetGroup group in settings.groups)
        {
            if (group == null)
                continue;

            BundledAssetGroupSchema schema =
                group.GetSchema<BundledAssetGroupSchema>();

            // 不是 Bundle Group，例如一些特殊内置 Group，直接跳过。
            if (schema == null)
                continue;

            string loadPath = schema.LoadPath.GetValue(settings);

            if (!IsRemoteLoadPath(loadPath))
                continue;

            remoteGroupCount++;

            foreach (AddressableAssetEntry entry in group.entries)
            {
                if (entry == null)
                    continue;

                if (entry.SetLabel(
                        PreloadLabel,
                        true,
                        force: true,
                        postEvent: false))
                {
                    entryCount++;
                }
            }

            Debug.Log(
                $"[Preload] Remote Group：{group.Name}，LoadPath={loadPath}");
        }

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        Debug.Log(
            $"<color=green>[Preload] 同步完成：</color>" +
            $"Remote Group {remoteGroupCount} 个，" +
            $"新增/修改 Preload Entry {entryCount} 个");
    }

    private static bool IsRemoteLoadPath(string loadPath)
    {
        if (string.IsNullOrWhiteSpace(loadPath))
            return false;

        return loadPath.StartsWith(
                   "http://",
                   StringComparison.OrdinalIgnoreCase) ||
               loadPath.StartsWith(
                   "https://",
                   StringComparison.OrdinalIgnoreCase);
    }
}
