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

/// <summary>
/// VAT资源文件，包含了怪物的网格、位置纹理、法线纹理以及动画剪辑数据
/// </summary>
[CreateAssetMenu(menuName = "Monster/VAT Asset")]
public class MonsterVATAsset : ScriptableObject
{
    //基础网格
    public Mesh Mesh;
    //渲染出的纹理，记录了每一帧的顶点位置，用于控制顶点动画
    public Texture2D PositionTexture;
    //渲染出的纹理，记录了每一帧的法线位置，用于绘制法线贴图，增强视觉效果
    public Texture2D NormalTexture;
    //顶点数量
    public int VertexCount;
    //播放帧数
    public int FrameCount;
    //播放速度
    public float FrameRate;
    //对纹路数据内容进行切分，每个切分对应一个动画剪辑
    public VATClipData[] Clips;
}

public class VATBakeSettings
{
    public GameObject Prefab;

    public AnimationClip[] Clips;

    public int FPS = 30;
}