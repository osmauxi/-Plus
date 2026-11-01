using UnityEditor;
using UnityEngine;
using ProjectGame.HotFix.Gameplay.Monsters;

internal static class MonsterVATRenderProfileBaker
{
    private const string OutputFolder = "Assets/VAT";

    public static MonsterVATRenderProfileAsset Bake(
        GameObject sourceRoot,
        string assetName,
        MonsterVATAsset vatAsset,
        Material vatMaterial,
        float crossFadeDuration,
        float movementThreshold,
        float hitFlashDuration,
        Color hitFlashColor)
    {
        SkinnedMeshRenderer sourceRenderer =
            sourceRoot.GetComponentInChildren<SkinnedMeshRenderer>(true);

        MonsterVATRenderProfileAsset profile = ScriptableObject.CreateInstance<MonsterVATRenderProfileAsset>();
        profile.name = $"{assetName}_RenderProfile";
        profile.Asset = vatAsset;
        profile.Material = vatMaterial;
        profile.SourceMaterial = sourceRenderer.sharedMaterial;
        if (profile.SourceMaterial == null)
        {
            Debug.LogError("Profile 烘焙失败：SkinnedMeshRenderer 缺少源材质。", sourceRoot);
            Object.DestroyImmediate(profile);
            return null;
        }
        profile.RootScale = sourceRoot.transform.localScale;
        profile.RendererLocalToRoot =
            sourceRoot.transform.worldToLocalMatrix * sourceRenderer.transform.localToWorldMatrix;
        profile.CrossFadeDuration = crossFadeDuration;
        profile.MovementThreshold = movementThreshold;
        profile.HitFlashDuration = hitFlashDuration;
        profile.HitFlashColor = hitFlashColor;
        profile.ShadowCastingMode = sourceRenderer.shadowCastingMode;
        profile.ReceiveShadows = sourceRenderer.receiveShadows;
        profile.Layer = sourceRoot.layer;

        if (!AssetDatabase.IsValidFolder(OutputFolder))
            AssetDatabase.CreateFolder("Assets", "VAT");

        string assetPath = $"{OutputFolder}/{assetName}_RenderProfile.asset";
        Object existingObject = AssetDatabase.LoadMainAssetAtPath(assetPath);
        if (existingObject == null)
        {
            AssetDatabase.CreateAsset(profile, assetPath);
        }
        else if (existingObject is MonsterVATRenderProfileAsset existingProfile)
        {
            EditorUtility.CopySerialized(profile, existingProfile);
            EditorUtility.SetDirty(existingProfile);
            Object.DestroyImmediate(profile);
            profile = existingProfile;
        }
        else
        {
            Object.DestroyImmediate(profile);
            return null;
        }

        AssetDatabase.SaveAssets();
        BakeCatalogEntry(assetName, profile);
        Debug.Log($"Monster VAT Profile 烘焙完成：{assetPath}", sourceRoot);
        return profile;
    }

    private static void BakeCatalogEntry(
        string localPoolId,
        MonsterVATRenderProfileAsset profile)
    {
        const string catalogPath =
            OutputFolder + "/MonsterVATRenderProfileCatalog.asset";
        MonsterVATRenderProfileCatalog catalog =
            AssetDatabase.LoadAssetAtPath<MonsterVATRenderProfileCatalog>(catalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<MonsterVATRenderProfileCatalog>();
            catalog.name = "MonsterVATRenderProfileCatalog";
            AssetDatabase.CreateAsset(catalog, catalogPath);
        }

        catalog.SetProfile(localPoolId, profile);
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
    }

}
