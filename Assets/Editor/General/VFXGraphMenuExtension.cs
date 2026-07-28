using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;
using System.IO;
using System.Linq;

public static class VFXGraphAssetCreator
{
    // 自定义目标文件夹路径（你可以修改这里）
    private const string targetFolder = "Assets/vfx";

    [MenuItem("GameObject/Create/VFX Graph", false, 20)]
    static void CreateVFXGraphAssetFromHierarchy()
    {
        // 1. 确保目标文件夹存在（不存在则自动创建）
        EnsureFolderExists(targetFolder);

        // 2. 记录当前所有 VisualEffectAsset 的 GUID（用于后面找新增的文件）
        var beforeAssets = AssetDatabase.FindAssets("t:VisualEffectAsset");

        // 3. 执行官方菜单命令，创建一个新的 .vfx 文件
        EditorApplication.ExecuteMenuItem("Assets/Create/Visual Effects/Visual Effect Graph");

        // 4. 延迟一帧，等待资源创建完成后再移动它
        EditorApplication.delayCall += () =>
        {
            // 找出新增的 VisualEffectAsset
            var afterAssets = AssetDatabase.FindAssets("t:VisualEffectAsset");
            var newGuid = afterAssets.Except(beforeAssets).FirstOrDefault();

            if (string.IsNullOrEmpty(newGuid))
            {
                Debug.LogWarning("未能找到新创建的 VFX Graph 资源。");
                return;
            }

            // 获取原始路径和文件名
            string originalPath = AssetDatabase.GUIDToAssetPath(newGuid);
            string fileName = Path.GetFileName(originalPath);

            // 生成目标路径（自动处理重名，比如生成 "New VFX Graph 1.vfx"）
            string targetPath = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/{fileName}");

            // 移动资源到目标文件夹
            string error = AssetDatabase.MoveAsset(originalPath, targetPath);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"移动资源失败: {error}");
                return;
            }

            AssetDatabase.Refresh();

            // 加载移动后的资源并高亮选中
            var asset = AssetDatabase.LoadAssetAtPath<VisualEffectAsset>(targetPath);
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);

            // 自动在场景中创建对应的 GameObject
            CreateGameObjectWithVFXAsset(asset);
        };
    }

    // 递归创建文件夹（如果父文件夹也不存在，一并创建）
    private static void EnsureFolderExists(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        string parent = Path.GetDirectoryName(folderPath);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolderExists(parent);

        string folderName = Path.GetFileName(folderPath);
        AssetDatabase.CreateFolder(parent, folderName);
        AssetDatabase.Refresh();
    }

    // 创建带 Visual Effect 组件的 GameObject
    private static void CreateGameObjectWithVFXAsset(VisualEffectAsset vfxAsset)
    {
        GameObject go = new GameObject(vfxAsset.name);
        var vfxComponent = go.AddComponent<VisualEffect>();
        vfxComponent.visualEffectAsset = vfxAsset;

        if (Selection.activeTransform != null)
            go.transform.SetParent(Selection.activeTransform, false);

        Undo.RegisterCreatedObjectUndo(go, "Create VFX GameObject");
        Selection.activeGameObject = go;
    }
}
