#if UNITY_EDITOR
using ProjectGame.HotFix.Gameplay.Map;
using ProjectGame.HotFix.Gameplay.Map.View;
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
    private const string SpatialSuffix = "_Spatial.asset";

    private static readonly string[] GridRoomPrefabPaths =
    {
        "Assets/_HotUpdate/Prefabs/Rooms/Room_Grid_Start.prefab",
        "Assets/_HotUpdate/Prefabs/Rooms/Room_Grid_Monster_01.prefab",
        "Assets/_HotUpdate/Prefabs/Rooms/Room_Grid_Monster_02.prefab",
        "Assets/_HotUpdate/Prefabs/Rooms/Room_Grid_Monster_03.prefab",
        "Assets/_HotUpdate/Prefabs/Rooms/Room_Grid_Monster_04.prefab",
        "Assets/_HotUpdate/Prefabs/Rooms/Room_Grid_Monster_05.prefab",
        "Assets/_HotUpdate/Prefabs/Rooms/Room_Grid_Boss.prefab"
    };

    [MenuItem("Tools/Map/Minimap/Bake All Grid Rooms")]
    private static void BakeAllGridRooms()
    {
        int successCount = 0;

        foreach (string prefabPath in GridRoomPrefabPaths)
        {
            GameObject prefabRoot = null;

            try
            {
                prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                RoomMinimapBakeSource source = prefabRoot.GetComponent<RoomMinimapBakeSource>();

                if (source == null || source.BakeRoot == null)
                {
                    Debug.LogError($"[MinimapBake] Prefab 缺少有效的 RoomMinimapBakeSource：{prefabPath}");
                    continue;
                }

                if (!Bake(source, Path.GetFileNameWithoutExtension(prefabPath)))
                    continue;

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                successCount++;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[MinimapBake] 烘焙 Prefab 失败：{prefabPath}\n{exception}");
            }
            finally
            {
                if (prefabRoot != null)
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[MinimapBake] Grid 房间批量烘焙完成：{successCount}/{GridRoomPrefabPaths.Length}");
    }

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

        if (Bake(source, source.gameObject.name))
            AssetDatabase.SaveAssets();
    }

    private static bool Bake(RoomMinimapBakeSource source, string roomName)
    {
        int bakeLayer = LayerMask.NameToLayer(BakeLayerName);
        if (bakeLayer < 0)
        {
            Debug.LogError($"[MinimapBake] 请先在 Project Settings > Tags and Layers 中创建 Layer：{BakeLayerName}");
            return false;
        }

        Transform bakeRoot = source.BakeRoot;
        bool originalActive = bakeRoot.gameObject.activeSelf;

        GameObject renderRoot = null;
        Camera bakeCamera = null;
        RenderTexture renderTexture = null;
        Texture2D texture = null;
        RenderTexture previousActive = RenderTexture.active;

        try
        {
            bakeRoot.gameObject.SetActive(true);
            Renderer[] sourceRenderers = bakeRoot.GetComponentsInChildren<Renderer>(true);
            if (!TryCalculateBounds(sourceRenderers, out Bounds sourceBounds))
            {
                Debug.LogError("[MinimapBake] BakeRoot 中没有可渲染的 Renderer。");
                return false;
            }
            RoomSpatialCell[] spatialCells = BakeSpatialCells(source.transform, sourceRenderers);

            // LoadPrefabContents 使用独立 Preview Scene，Camera.Render 不会可靠地隔离该 Scene；
            // 直接渲染时会混入当前编辑场景中 MinimapBake 层的对象。
            // 因此把纯烘焙副本放到当前场景的远端，确保相机只覆盖本次处理的房间。
            renderRoot = UnityEngine.Object.Instantiate(bakeRoot.gameObject);
            renderRoot.name = "__MinimapBakeRoot";
            renderRoot.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.SceneManagement.Scene activeScene =
                UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (renderRoot.scene != activeScene)
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(renderRoot, activeScene);
            renderRoot.transform.position += new Vector3(10000f, 0f, 10000f);
            SetLayerRecursive(renderRoot.transform, bakeLayer, new Dictionary<Transform, int>());

            Renderer[] renderers = renderRoot.GetComponentsInChildren<Renderer>(true);
            if (!TryCalculateBounds(renderers, out Bounds renderBounds))
            {
                Debug.LogError("[MinimapBake] 临时渲染副本中没有可渲染的 Renderer。");
                return false;
            }

            GameObject cameraObject = new GameObject("__MinimapBakeCamera");
            cameraObject.hideFlags = HideFlags.HideAndDontSave;
            //临时创建正交Camera
            bakeCamera = cameraObject.AddComponent<Camera>();
            bakeCamera.orthographic = true;
            bakeCamera.clearFlags = CameraClearFlags.SolidColor;
            bakeCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            bakeCamera.cullingMask = 1 << bakeLayer;
            bakeCamera.allowHDR = false;
            bakeCamera.allowMSAA = false;
            bakeCamera.useOcclusionCulling = false;

            float halfSize = Mathf.Max(sourceBounds.extents.x, sourceBounds.extents.z) + source.Padding;
            float fullSize = halfSize * 2f;
            float cameraHeight = renderBounds.max.y + 10f;
            //相机丢到正上方，只拍RenderTexture
            bakeCamera.transform.SetPositionAndRotation(
                new Vector3(renderBounds.center.x, cameraHeight, renderBounds.center.z),
                Quaternion.Euler(90f, 0f, 0f));

            bakeCamera.orthographicSize = halfSize;
            bakeCamera.nearClipPlane = 0.01f;
            bakeCamera.farClipPlane = cameraHeight - renderBounds.min.y + 10f;

            renderTexture = new RenderTexture(Resolution, Resolution, 24, RenderTextureFormat.ARGB32);
            renderTexture.Create();

            bakeCamera.targetTexture = renderTexture;
            bakeCamera.Render();

            RenderTexture.active = renderTexture;
            texture = new Texture2D(Resolution, Resolution, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, Resolution, Resolution), 0, 0);
            texture.Apply();

            Directory.CreateDirectory(OutputDirectory);
            string assetPath = $"{OutputDirectory}/{roomName}_Minimap.png";
            string spatialPath = $"{OutputDirectory}/{roomName}{SpatialSuffix}";
            RoomSpatialData spatialData = SaveSpatialData(spatialPath, spatialCells);
            RoomView roomView = source.GetComponent<RoomView>();
            if (roomView == null)
                throw new InvalidOperationException($"房间 {roomName} 缺少 RoomView。");

            roomView.EditorSetSpatialData(spatialData);
            EditorUtility.SetDirty(roomView);

            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            ConfigureImporter(assetPath);

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            RoomMinimapDefinition definition = source.GetComponent<RoomMinimapDefinition>();
            if (definition == null)
                definition = source.gameObject.AddComponent<RoomMinimapDefinition>();

            Vector3 localCenter3D = source.transform.InverseTransformPoint(sourceBounds.center);
            Vector2 localCenter = new Vector2(localCenter3D.x, localCenter3D.z);
            Vector2 localSize = new Vector2(fullSize, fullSize);
            definition.EditorSetBakeResult(sprite, localCenter, localSize);

            if (PrefabUtility.IsPartOfPrefabInstance(definition))
                PrefabUtility.RecordPrefabInstancePropertyModifications(definition);

            EditorUtility.SetDirty(definition.gameObject);
            Debug.Log($"[MinimapBake] 完成：{assetPath}", source);
            Selection.activeObject = sprite;
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            return false;
        }
        finally
        {
            RenderTexture.active = previousActive;

            bakeRoot.gameObject.SetActive(originalActive);

            if (renderRoot != null)
                UnityEngine.Object.DestroyImmediate(renderRoot);

            if (bakeCamera != null)
                UnityEngine.Object.DestroyImmediate(bakeCamera.gameObject);

            if (renderTexture != null)
            {
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }

            if (texture != null)
                UnityEngine.Object.DestroyImmediate(texture);
        }
    }
    /// <summary>
    /// 计算整个小地图布局大小
    /// </summary>
    private static bool TryCalculateBounds(Renderer[] renderers, out Bounds bounds)
    {
        bounds = default;
        bool found = false;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || !renderer.enabled)
                continue;

            if (!found)
            {
                bounds = renderer.bounds;
                found = true;
            }
            else
                bounds.Encapsulate(renderer.bounds);
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
        if (importer == null)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.SaveAndReimport();
    }

    /// <param name="roomRoot">房间 Transform</param>
    /// <param name="renderers"> 所有 Bake 方块</param>
    /// <returns></returns>
    private static RoomSpatialCell[] BakeSpatialCells(Transform roomRoot, Renderer[] renderers)
    {
        var cells = new RoomSpatialCell[RoomSpatialData.GridSize * RoomSpatialData.GridSize];

        for (int y = 0; y < RoomSpatialData.GridSize; y++)
        {
            for (int x = 0; x < RoomSpatialData.GridSize; x++)
            {
                //遍历每一格，算当前Cell中心的RoomLocalPos
                Vector3 localPoint = new Vector3(
                    -RoomSpatialData.HalfSize + (x + 0.5f) * RoomSpatialData.CellSize,
                    0f,
                    -RoomSpatialData.HalfSize + (y + 0.5f) * RoomSpatialData.CellSize);
                //转WorldPos
                Vector3 worldPoint = roomRoot.TransformPoint(localPoint);
                RoomSpatialCell result = RoomSpatialCell.Empty;
                //遍历所有Renderer
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer renderer = renderers[i];
                    RoomSpatialBakeBlock block = renderer.GetComponentInParent<RoomSpatialBakeBlock>();
                    if (block == null || !ContainsXZ(renderer, worldPoint))
                        continue;

                    if (block.CellType > result)
                        result = block.CellType;

                    if (result == RoomSpatialCell.Obstacle)
                        break;
                }

                cells[y * RoomSpatialData.GridSize + x] = result;
            }
        }

        return cells;
    }
    /// <summary>
    /// 算这个Cell中心有没有落入方块
    /// </summary>
    private static bool ContainsXZ(Renderer renderer, Vector3 worldPoint)
    {
        MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
            return false;

        Vector3 point = renderer.transform.InverseTransformPoint(worldPoint);
        Bounds bounds = meshFilter.sharedMesh.bounds;

        return point.x >= bounds.min.x && point.x <= bounds.max.x &&
               point.z >= bounds.min.z && point.z <= bounds.max.z;
    }

    private static RoomSpatialData SaveSpatialData(string assetPath, RoomSpatialCell[] cells)
    {
        RoomSpatialData data = AssetDatabase.LoadAssetAtPath<RoomSpatialData>(assetPath);

        if (data == null)
        {
            data = ScriptableObject.CreateInstance<RoomSpatialData>();
            AssetDatabase.CreateAsset(data, assetPath);
        }

        data.EditorSetCells(cells);
        EditorUtility.SetDirty(data);
        return data;
    }
}
#endif
