#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class RoomMinimapBaker
{
    private const int Resolution = 512;
    private const string BakeLayerName = "MinimapBake";
    private const string OutputDirectory = "Assets/_HotUpdate/Art/Minimap/Rooms";

    [MenuItem("Tools/Map/Minimap/Bake Selected Room")]
    private static void BakeSelectedRoom()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogError("[MinimapBake] 请先选中房间根节点。");
            return;
        }

        RoomMinimapBakeSource source = selected.GetComponentInParent<RoomMinimapBakeSource>();
        if (source == null || source.BakeRoot == null)
        {
            Debug.LogError("[MinimapBake] 找不到 RoomMinimapBakeSource 或 BakeRoot。");
            return;
        }

        Bake(source);
    }

    private static void Bake(RoomMinimapBakeSource source)
    {
        int bakeLayer = LayerMask.NameToLayer(BakeLayerName);
        if (bakeLayer < 0)
        {
            Debug.LogError($"[MinimapBake] 请先在 Project Settings > Tags and Layers 中创建 Layer：{BakeLayerName}");
            return;
        }

        Transform bakeRoot = source.BakeRoot;
        bool originalActive = bakeRoot.gameObject.activeSelf;
        Dictionary<Transform, int> originalLayers = new();

        Camera bakeCamera = null;
        RenderTexture renderTexture = null;
        Texture2D texture = null;

        try
        {
            bakeRoot.gameObject.SetActive(true);
            SetLayerRecursive(bakeRoot, bakeLayer, originalLayers);

            Renderer[] renderers = bakeRoot.GetComponentsInChildren<Renderer>(true);
            if (!TryCalculateBounds(renderers, out Bounds bounds))
            {
                Debug.LogError("[MinimapBake] BakeRoot 下没有可渲染 Renderer。");
                return;
            }

            GameObject cameraObject = new GameObject("__MinimapBakeCamera");
            cameraObject.hideFlags = HideFlags.HideAndDontSave;

            bakeCamera = cameraObject.AddComponent<Camera>();
            bakeCamera.orthographic = true;
            bakeCamera.clearFlags = CameraClearFlags.SolidColor;
            bakeCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            bakeCamera.cullingMask = 1 << bakeLayer;
            bakeCamera.allowHDR = false;
            bakeCamera.allowMSAA = false;
            bakeCamera.useOcclusionCulling = false;

            float halfSize = Mathf.Max(bounds.extents.x, bounds.extents.z) + source.Padding;
            float fullSize = halfSize * 2f;
            float cameraHeight = bounds.max.y + 10f;

            bakeCamera.transform.SetPositionAndRotation(
                new Vector3(bounds.center.x, cameraHeight, bounds.center.z),
                Quaternion.Euler(90f, 0f, 0f));

            bakeCamera.orthographicSize = halfSize;
            bakeCamera.nearClipPlane = 0.01f;
            bakeCamera.farClipPlane = cameraHeight - bounds.min.y + 10f;

            renderTexture = new RenderTexture(Resolution, Resolution, 24, RenderTextureFormat.ARGB32);
            renderTexture.Create();

            bakeCamera.targetTexture = renderTexture;
            bakeCamera.Render();

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;

            texture = new Texture2D(Resolution, Resolution, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, Resolution, Resolution), 0, 0);
            texture.Apply();

            RenderTexture.active = previous;

            Directory.CreateDirectory(OutputDirectory);

            string roomName = source.gameObject.name;
            string assetPath = $"{OutputDirectory}/{roomName}_Minimap.png";

            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            ConfigureImporter(assetPath);
            //生成后自动填入RoomMinimapDefinition映射数据
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

            RoomMinimapDefinition definition = source.GetComponent<RoomMinimapDefinition>();
            if (definition == null) definition = source.gameObject.AddComponent<RoomMinimapDefinition>();

            Vector3 localCenter3D = source.transform.InverseTransformPoint(bounds.center);
            Vector2 localCenter = new Vector2(localCenter3D.x, localCenter3D.z);
            Vector2 localSize = new Vector2(fullSize, fullSize);

            definition.EditorSetBakeResult(sprite, localCenter, localSize);

            PrefabUtility.RecordPrefabInstancePropertyModifications(definition);
            EditorUtility.SetDirty(definition.gameObject);
            AssetDatabase.SaveAssets();

            Debug.Log($"[MinimapBake] 完成：{assetPath}", source);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            foreach ((Transform target, int layer) in originalLayers)
                if (target != null) target.gameObject.layer = layer;

            bakeRoot.gameObject.SetActive(originalActive);

            if (bakeCamera != null) UnityEngine.Object.DestroyImmediate(bakeCamera.gameObject);
            if (renderTexture != null)
            {
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }

            if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
        }
    }

    private static bool TryCalculateBounds(Renderer[] renderers, out Bounds bounds)
    {
        bounds = default;
        bool found = false;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || !renderer.enabled) continue;

            if (!found)
            {
                bounds = renderer.bounds;
                found = true;
            }
            else bounds.Encapsulate(renderer.bounds);
        }

        return found;
    }

    private static void SetLayerRecursive(Transform root, int layer, Dictionary<Transform, int> originalLayers)
    {
        foreach (Transform target in root.GetComponentsInChildren<Transform>(true))
        {
            originalLayers[target] = target.gameObject.layer;
            target.gameObject.layer = layer;
        }
    }

    private static void ConfigureImporter(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Compressed;

        importer.SaveAndReimport();

    }
}
#endif