using System;
using UnityEngine;

[Serializable]
public struct VATClipData
{
    public string Name;
    public int StartFrame;
    public int FrameCount;
    public float FrameRate;
}

[CreateAssetMenu(menuName = "Monster/VAT Asset")]
public class MonsterVATAsset : ScriptableObject
{
    public Mesh Mesh;
    public Texture2D PositionTexture;
    public Texture2D NormalTexture;

    public int VertexCount;
    public int FrameCount;
    public float FrameRate;

    public VATClipData[] Clips;
}

public class VATBakeSettings
{
    public GameObject Prefab;

    public AnimationClip[] Clips;

    public int FPS = 30;
}