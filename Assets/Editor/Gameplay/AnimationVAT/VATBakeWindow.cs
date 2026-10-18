using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VATBakeWindow : EditorWindow
{
    private GameObject _prefab;
    [SerializeField] private AnimationClip[] _clips = System.Array.Empty<AnimationClip>();
    private float _frameRate = 30f;

    [MenuItem("Tools/Monster VAT/Baker")]
    private static void Open()
    {
        GetWindow<VATBakeWindow>();
    }

    private void OnGUI()
    {
        _prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _prefab, typeof(GameObject), false);
        _frameRate = EditorGUILayout.FloatField("Frame Rate", _frameRate);
        SerializedObject serializedWindow = new SerializedObject(this);
        serializedWindow.Update();
        EditorGUILayout.PropertyField(serializedWindow.FindProperty("_clips"), true);
        serializedWindow.ApplyModifiedProperties();

        if (GUILayout.Button("Bake"))
            Bake();
    }

    private void Bake()
    {
        if (_prefab == null)
        {
            Debug.LogError("没有Prefab");
            return;
        }

        GameObject instance = Instantiate(_prefab);
        instance.hideFlags = HideFlags.HideAndDontSave;

        SkinnedMeshRenderer renderer = instance.GetComponentInChildren<SkinnedMeshRenderer>();

        if (renderer == null)
        {
            Debug.LogError("Prefab需要SkinnedMeshRenderer");
            DestroyImmediate(instance);
            return;
        }

        Animator animator = instance.GetComponentInChildren<Animator>();
        AnimationClip[] clips = _clips.Length > 0
            ? _clips
            : animator != null && animator.runtimeAnimatorController != null
                ? animator.runtimeAnimatorController.animationClips
                : System.Array.Empty<AnimationClip>();
        if (clips.Length == 0)
        {
            Debug.LogError("请指定需要烘焙的 AnimationClip。");
            DestroyImmediate(instance);
            return;
        }

        GameObject animationRoot = FindAnimationRoot(instance.transform, renderer, clips[0]);
        Debug.Log($"VAT SamplingRoot:{animationRoot.name} IsHuman:{(animator != null && animator.isHuman)} Renderer:{renderer.gameObject.name} RootBone:{renderer.rootBone.name} BoneCount:{renderer.bones.Length}");

        Mesh sourceMesh = renderer.sharedMesh;
        int vertexCount = sourceMesh.vertexCount;
        Mesh vatMesh = CreateVATMesh(sourceMesh);

        Mesh bakedMesh = new Mesh();
        List<Vector3[]> allFrames = new();
        List<Vector3[]> allNormalFrames = new();
        List<VATClipData> clipData = new();
        List<Vector3[]> clipValidationFrames = new();

        int currentFrame = 0;

        foreach (AnimationClip clip in clips)
        {
            DebugClipBindings(clip, animationRoot);
            clipValidationFrames.Add(DebugClipSampling(clip, animationRoot, renderer));

            int frameCount = Mathf.CeilToInt(clip.length * _frameRate);
            int startFrame = currentFrame;

            for (int frame = 0; frame < frameCount; frame++)
            {
                float time = frame / _frameRate;

                clip.SampleAnimation(animationRoot, time);
                renderer.BakeMesh(bakedMesh, true);
                Vector3[] bakedNormals = bakedMesh.normals;
                if (bakedNormals.Length != vertexCount)
                {
                    Debug.LogError($"VAT法线烘焙失败 Clip:{clip.name} Frame:{frame} Normals:{bakedNormals.Length} Vertices:{vertexCount}");
                    DestroyImmediate(bakedMesh);
                    DestroyImmediate(vatMesh);
                    DestroyImmediate(instance);
                    return;
                }
                allNormalFrames.Add(bakedNormals);
                allFrames.Add(bakedMesh.vertices);
                currentFrame++;
            }

            clipData.Add(new VATClipData
            {
                Name = clip.name,
                StartFrame = startFrame,
                FrameCount = frameCount,
                FrameRate = _frameRate
            });
        }

        DebugClipDifferences(clips, clipValidationFrames);
        DebugFrameDifferences(allFrames);

        Texture2D texture = BuildPositionTexture(allFrames, vertexCount);
        Texture2D normalTexture = BuildNormalTexture(allNormalFrames, vertexCount);

        SaveAsset(texture, normalTexture, vatMesh, clipData.ToArray(), allFrames.Count);

        Debug.Log($"VAT Bake完成 Vertex:{vertexCount} Frames:{allFrames.Count}");

        DestroyImmediate(instance);
    }
    /// <summary>
    /// 将动画顶点信息写入Texture中，Texture天然作为信息承载的载体供Shader读取
    /// </summary>
    private Texture2D BuildPositionTexture(List<Vector3[]> frames, int vertexCount)
    {
        //RGBAHalf，单像素载16bit
        Texture2D texture = new Texture2D(vertexCount, frames.Count, TextureFormat.RGBAHalf, false, true);

        for (int y = 0; y < frames.Count; y++)
        {
            Vector3[] vertices = frames[y];

            for (int x = 0; x < vertexCount; x++)
            {
                Vector3 position = vertices[x];

                texture.SetPixel(x, y, new Color(position.x, position.y, position.z, 1));
            }
        }

        texture.Apply();

        return texture;
    }


    private void SaveAsset(Texture2D positionTexture, Texture2D normalTexture, Mesh mesh, VATClipData[] clips, int frameCount)
    {
        string folder = "Assets/VAT";

        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "VAT");

        positionTexture.name = $"{_prefab.name}_Position";
        normalTexture.name = $"{_prefab.name}_Normal";
        mesh.name = $"{_prefab.name}_VATMesh";

        string positionPath = $"{folder}/{_prefab.name}_Position.asset";
        string normalPath = $"{folder}/{_prefab.name}_Normal.asset";
        string meshPath = $"{folder}/{_prefab.name}_VATMesh.asset";
        string assetPath = $"{folder}/{_prefab.name}_VAT.asset";

        Texture2D savedPositionTexture = SaveOrUpdateAsset(positionTexture, positionPath);
        Texture2D savedNormalTexture = SaveOrUpdateAsset(normalTexture, normalPath);
        Mesh savedMesh = SaveOrUpdateAsset(mesh, meshPath);

        if (savedPositionTexture == null || savedNormalTexture == null || savedMesh == null)
        {
            Debug.LogError("VAT资源保存失败，请检查Console中的资源类型错误。");
            return;
        }

        MonsterVATAsset asset = CreateInstance<MonsterVATAsset>();
        asset.name = $"{_prefab.name}_VAT";
        asset.Mesh = savedMesh;
        asset.PositionTexture = savedPositionTexture;
        asset.NormalTexture = savedNormalTexture;
        asset.VertexCount = savedMesh.vertexCount;
        asset.FrameCount = frameCount;
        asset.FrameRate = _frameRate;
        asset.Clips = clips;

        MonsterVATAsset savedAsset = SaveOrUpdateAsset(asset, assetPath);

        if (savedAsset == null)
        {
            Debug.LogError("VAT主资源保存失败，请检查Console中的资源类型错误。");
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"VAT资源保存完成：{assetPath}，顶点数：{savedMesh.vertexCount}，帧数：{frameCount}");
    }
    /// <summary>
    /// 辅助方法，用于保存或更新资源，如果资源已存在，则更新其内容，否则创建新资源。
    /// </summary>
    private static T SaveOrUpdateAsset<T>(T source, string path) where T : Object
    {
        Object existingObject = AssetDatabase.LoadMainAssetAtPath(path);

        if (existingObject == null)
        {
            AssetDatabase.CreateAsset(source, path);
            return source;
        }

        T existingAsset = existingObject as T;

        if (existingAsset == null)
        {
            Debug.LogError($"VAT资源类型不匹配：{path}，当前类型：{existingObject.GetType().Name}，需要类型：{typeof(T).Name}");
            DestroyImmediate(source);
            return null;
        }

        EditorUtility.CopySerialized(source, existingAsset);
        EditorUtility.SetDirty(existingAsset);
        DestroyImmediate(source);

        return existingAsset;
    }


    private Mesh CreateVATMesh(Mesh source)
    {
        //创建一个新的Mesh，并将原始Mesh的顶点信息复制到新Mesh中，同时为每个顶点生成UV2坐标
        Mesh mesh = Instantiate(source);
        //UV通常用于纹理映射，而在VAT中，我们将UV2用于存储顶点索引信息，
        //以便在Shader中根据顶点索引从PositionTexture中获取对应的顶点位置。
        Vector2[] uv2 = new Vector2[source.vertexCount];

        for (int i = 0; i < source.vertexCount; i++)
            uv2[i] = new Vector2(i, 0);

        mesh.uv2 = uv2;
        mesh.name = source.name + "_VAT";

        return mesh;
    }

    private Texture2D BuildNormalTexture(List<Vector3[]> frames, int vertexCount)
    {
        Texture2D texture = new Texture2D(vertexCount, frames.Count, TextureFormat.RGBAHalf, false, true);
        texture.name = $"{_prefab.name}_Normal";
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < frames.Count; y++)
        {
            Vector3[] normals = frames[y];

            for (int x = 0; x < vertexCount; x++)
            {
                Vector3 normal = normals[x].normalized;
                texture.SetPixel(x, y, new Color(normal.x, normal.y, normal.z, 1f));
            }
        }

        texture.Apply(false, false);
        return texture;
    }

    private static void DebugClipBindings(AnimationClip clip, GameObject animationRoot)
    {
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
        int transformBindings = 0;
        int resolvedBindings = 0;
        int unresolvedBindings = 0;

        Debug.Log($"Clip:{clip.name} TotalBindings:{bindings.Length}");

        foreach (EditorCurveBinding binding in bindings)
        {
            if (binding.type != typeof(Transform))
            {
                continue;
            }

            transformBindings++;

            Transform target = string.IsNullOrEmpty(binding.path) ? animationRoot.transform : animationRoot.transform.Find(binding.path);

            if (target != null)
            {
                resolvedBindings++;
            }
            else
            {
                unresolvedBindings++;
                Debug.LogWarning($"Unresolved Clip:{clip.name} Path:{binding.path} Property:{binding.propertyName}");
            }

        }

        Debug.Log($"Clip:{clip.name} TransformBindings:{transformBindings} Resolved:{resolvedBindings} Unresolved:{unresolvedBindings}");
    }

    private static Vector3[] DebugClipSampling(AnimationClip clip, GameObject animationRoot, SkinnedMeshRenderer renderer)
    {
        Mesh frame0 = new Mesh();
        Mesh frameMid = new Mesh();
        float midTime = clip.length * 0.5f;
        Transform[] bones = renderer.bones;
        Vector3[] positions0 = new Vector3[bones.Length];
        Quaternion[] rotations0 = new Quaternion[bones.Length];

        clip.SampleAnimation(animationRoot, 0f);
        for (int i = 0; i < bones.Length; i++)
        {
            positions0[i] = bones[i].localPosition;
            rotations0[i] = bones[i].localRotation;
        }
        renderer.BakeMesh(frame0, true);

        clip.SampleAnimation(animationRoot, midTime);
        renderer.BakeMesh(frameMid, true);

        int changedBoneCount = 0;
        List<string> changedBones = new();
        for (int i = 0; i < bones.Length; i++)
        {
            float positionDelta = Vector3.Distance(positions0[i], bones[i].localPosition);
            float rotationDelta = Quaternion.Angle(rotations0[i], bones[i].localRotation);
            if (positionDelta <= 0.00001f && rotationDelta <= 0.001f)
            {
                continue;
            }

            changedBoneCount++;
            if (changedBones.Count < 3)
            {
                changedBones.Add($"{bones[i].name}(PositionDelta:{positionDelta:F6}, RotationDelta:{rotationDelta:F3})");
            }
        }

        Vector3[] vertices0 = frame0.vertices;
        Vector3[] verticesMid = frameMid.vertices;
        GetVertexDifferences(vertices0, verticesMid, out int changedVertexCount, out float maxVertexDelta, out float averageVertexDelta);

        Debug.Log($"VAT Clip:{clip.name} Time:0.000->{midTime:F3} ChangedBones:{changedBoneCount}/{bones.Length} ChangedVertices:{changedVertexCount}/{vertices0.Length} MaxVertexDelta:{maxVertexDelta:F6} AverageVertexDelta:{averageVertexDelta:F6} Bones:[{string.Join(", ", changedBones)}]");

        DestroyImmediate(frame0);
        DestroyImmediate(frameMid);

        return verticesMid;
    }

    private static void DebugClipDifferences(AnimationClip[] clips, List<Vector3[]> clipFrames)
    {
        if (clipFrames.Count < 2)
        {
            return;
        }

        for (int i = 1; i < clipFrames.Count; i++)
        {
            GetVertexDifferences(clipFrames[0], clipFrames[i], out int changedVertexCount, out float maxVertexDelta, out float averageVertexDelta);
            Debug.Log($"VAT ClipDifference:{clips[0].name}->{clips[i].name} ChangedVertices:{changedVertexCount}/{clipFrames[0].Length} MaxVertexDelta:{maxVertexDelta:F6} AverageVertexDelta:{averageVertexDelta:F6}");
        }
    }

    private static void DebugFrameDifferences(List<Vector3[]> frames)
    {
        Vector3[] firstFrame = frames[0];
        int framesDifferentFromFirst = 0;
        float maxVertexDelta = 0f;

        for (int i = 1; i < frames.Count; i++)
        {
            GetVertexDifferences(firstFrame, frames[i], out int changedVertexCount, out float frameMaxVertexDelta, out _);
            if (changedVertexCount > 0)
            {
                framesDifferentFromFirst++;
            }

            maxVertexDelta = Mathf.Max(maxVertexDelta, frameMaxVertexDelta);
        }

        Debug.Log($"VAT FrameValidation TotalFrames:{frames.Count} FramesDifferentFromFirst:{framesDifferentFromFirst} MaxVertexDelta:{maxVertexDelta:F6}");
    }

    private static void GetVertexDifferences(Vector3[] verticesA, Vector3[] verticesB, out int changedVertexCount, out float maxDelta, out float averageDelta)
    {
        changedVertexCount = 0;
        maxDelta = 0f;
        float totalDelta = 0f;

        for (int i = 0; i < verticesA.Length; i++)
        {
            float delta = Vector3.Distance(verticesA[i], verticesB[i]);
            if (delta > 0.00001f)
            {
                changedVertexCount++;
            }

            maxDelta = Mathf.Max(maxDelta, delta);
            totalDelta += delta;
        }

        averageDelta = verticesA.Length > 0 ? totalDelta / verticesA.Length : 0f;
    }

    private static GameObject FindAnimationRoot(Transform prefabRoot, SkinnedMeshRenderer renderer, AnimationClip clip)
    {
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
        Transform bestRoot = prefabRoot;
        int bestResolvedCount = -1;

        for (Transform candidate = renderer.transform; candidate != null; candidate = candidate.parent)
        {
            int resolvedCount = 0;
            for (int i = 0; i < bindings.Length; i++)
            {
                EditorCurveBinding binding = bindings[i];
                if (binding.type == typeof(Transform)
                    && (string.IsNullOrEmpty(binding.path) || candidate.Find(binding.path) != null))
                {
                    resolvedCount++;
                }
            }

            if (resolvedCount > bestResolvedCount)
            {
                bestResolvedCount = resolvedCount;
                bestRoot = candidate;
            }
        }

        return bestRoot.gameObject;
    }
}
