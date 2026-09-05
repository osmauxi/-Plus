using HybridCLR.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景与程序集交收门禁：Player 只能内置 AOT Boot 场景，其余场景必须以
/// HotUpdateScene 标签进入 Addressables；同时阻止 Boot 引用 HotFix 组件、
/// 热更场景引用未登记的 HotFix 程序集以及 Missing Script。
/// </summary>
public sealed class SceneAssemblyHandoverValidator : IPreprocessBuildWithReport
{
    private const string BootScenePath =
        "Assets/_HotUpdate/Scenes/BootStrapScene.unity";
    private const string HotUpdateSceneRoot =
        "Assets/_HotUpdate/Scenes";
    private const string HotUpdateSceneLabel = "HotUpdateScene";

    public int callbackOrder => -1000;

    [MenuItem("Tools/Validation/Scene And Assembly Handover")]
    public static void ValidateFromMenu()
    {
        IReadOnlyList<string> errors = ValidateProject();
        if (errors.Count > 0)
        {
            string message = BuildFailureMessage(errors);
            Debug.LogError(message);
            throw new BuildFailedException(message);
        }

        Debug.Log(
            "<color=green>[SceneAssemblyHandoverValidator] " +
            "场景与程序集交收检查通过</color>");
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        IReadOnlyList<string> errors = ValidateProject();
        if (errors.Count > 0)
            throw new BuildFailedException(BuildFailureMessage(errors));
    }

    private static IReadOnlyList<string> ValidateProject()
    {
        var errors = new List<string>();
        ValidateBuildSettings(errors);
        ValidateAddressableScenes(errors);
        return errors;
    }

    private static void ValidateBuildSettings(ICollection<string> errors)
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        EditorBuildSettingsScene[] enabledScenes =
            scenes.Where(scene => scene.enabled).ToArray();

        if (enabledScenes.Length != 1 ||
            !string.Equals(
                enabledScenes[0].path,
                BootScenePath,
                StringComparison.Ordinal))
        {
            errors.Add(
                "Player Build Settings 必须且只能启用 BootStrapScene；" +
                "所有其他场景必须通过 Addressables 加载。");
        }

        string bootGuid = AssetDatabase.AssetPathToGUID(BootScenePath);
        if (string.IsNullOrWhiteSpace(bootGuid))
            errors.Add($"找不到 Boot 场景：{BootScenePath}");

        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings != null &&
            !string.IsNullOrWhiteSpace(bootGuid) &&
            settings.FindAssetEntry(bootGuid) != null)
        {
            errors.Add("BootStrapScene 不能同时加入 Addressables。");
        }

        ValidateSceneContents(
            BootScenePath,
            isBootScene: true,
            allowTestAssembly: false,
            new HashSet<string>(
                SettingsUtil.HotUpdateAssemblyNamesExcludePreserved,
                StringComparer.Ordinal),
            networkObjectHashes: null,
            errors);
    }

    private static void ValidateAddressableScenes(ICollection<string> errors)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            errors.Add("找不到 AddressableAssetSettings。");
            return;
        }

        var hotUpdateAssemblies = new HashSet<string>(
            SettingsUtil.HotUpdateAssemblyNamesExcludePreserved,
            StringComparer.Ordinal);
        var networkObjectHashes = new Dictionary<uint, string>();
        string[] sceneGuids = AssetDatabase.FindAssets(
            "t:Scene",
            new[] { HotUpdateSceneRoot });

        foreach (string guid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid)
                .Replace('\\', '/');
            if (string.Equals(
                    scenePath,
                    BootScenePath,
                    StringComparison.Ordinal))
            {
                continue;
            }

            var entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                errors.Add($"热更场景未加入 Addressables：{scenePath}");
            }
            else
            {
                if (!entry.labels.Contains(HotUpdateSceneLabel))
                    errors.Add(
                        $"热更场景缺少 {HotUpdateSceneLabel} 标签：{scenePath}");

                if (!string.Equals(
                        entry.address,
                        scenePath,
                        StringComparison.Ordinal))
                {
                    errors.Add(
                        $"场景 Address 必须使用完整 Asset Path：{scenePath}；" +
                        $"当前为 {entry.address}");
                }
            }

            if (EditorBuildSettings.scenes.Any(
                    scene => scene.enabled &&
                             string.Equals(
                                 scene.path,
                                 scenePath,
                                 StringComparison.Ordinal)))
            {
                errors.Add(
                    $"热更场景不能出现在 Player Build Settings：{scenePath}");
            }

            ValidateSceneContents(
                scenePath,
                isBootScene: false,
                allowTestAssembly: scenePath.Contains("/Tests/"),
                hotUpdateAssemblies,
                networkObjectHashes,
                errors);
        }
    }

    private static void ValidateSceneContents(
        string scenePath,
        bool isBootScene,
        bool allowTestAssembly,
        ISet<string> hotUpdateAssemblies,
        IDictionary<uint, string> networkObjectHashes,
        ICollection<string> errors)
    {
        Scene existingScene = SceneManager.GetSceneByPath(scenePath);
        bool openedForValidation =
            !existingScene.IsValid() || !existingScene.isLoaded;
        Scene scene = openedForValidation
            ? EditorSceneManager.OpenScene(
                scenePath,
                OpenSceneMode.Additive)
            : existingScene;

        try
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform transform in
                         root.GetComponentsInChildren<Transform>(true))
                {
                    int missingCount =
                        GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                            transform.gameObject);
                    if (missingCount > 0)
                    {
                        errors.Add(
                            $"场景存在 Missing Script：{scenePath} / " +
                            $"{GetHierarchyPath(transform)}，数量 {missingCount}");
                    }

                    foreach (MonoBehaviour behaviour in
                             transform.GetComponents<MonoBehaviour>())
                    {
                        if (behaviour == null)
                            continue;

                        string assemblyName =
                            behaviour.GetType().Assembly.GetName().Name;
                        string componentLocation =
                            $"{scenePath} / {GetHierarchyPath(transform)}";
                        bool looksLikeHotFix =
                            assemblyName.StartsWith(
                                "HotFix.",
                                StringComparison.Ordinal);

                        if (isBootScene && looksLikeHotFix)
                        {
                            errors.Add(
                                $"Boot 场景禁止引用热更组件：{scenePath} / " +
                                $"{GetHierarchyPath(transform)} -> " +
                                $"{behaviour.GetType().FullName} ({assemblyName})");
                        }
                        else if (!isBootScene &&
                                 !allowTestAssembly &&
                                 looksLikeHotFix &&
                                 !hotUpdateAssemblies.Contains(assemblyName))
                        {
                            errors.Add(
                                $"场景引用的热更程序集未加入 HybridCLR Settings：" +
                                $"{scenePath} / {GetHierarchyPath(transform)} -> " +
                                assemblyName);
                        }

                        if (!isBootScene &&
                            networkObjectHashes != null &&
                            string.Equals(
                                behaviour.GetType().FullName,
                                "Unity.Netcode.NetworkObject",
                                StringComparison.Ordinal))
                        {
                            var prefabIdProperty = behaviour.GetType()
                                .GetProperty("PrefabIdHash");
                            uint hash = prefabIdProperty == null
                                ? 0
                                : (uint)prefabIdProperty.GetValue(behaviour);
                            if (hash == 0)
                            {
                                errors.Add(
                                    $"Addressable 场景 NetworkObject Hash 为 0：" +
                                    componentLocation);
                            }
                            else if (networkObjectHashes.TryGetValue(
                                         hash,
                                         out string existingLocation) &&
                                     !string.Equals(
                                         existingLocation,
                                         componentLocation,
                                         StringComparison.Ordinal))
                            {
                                errors.Add(
                                    $"Addressable 场景 NetworkObject Hash 重复：" +
                                    $"{hash}；{existingLocation}；" +
                                    componentLocation);
                            }
                            else
                            {
                                networkObjectHashes[hash] = componentLocation;
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            if (openedForValidation)
                EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static string GetHierarchyPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }

    private static string BuildFailureMessage(IReadOnlyList<string> errors)
    {
        return "[SceneAssemblyHandoverValidator] 交收检查失败：\n- " +
               string.Join("\n- ", errors);
    }
}
