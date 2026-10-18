using UnityEngine;

public class VATPreviewController : MonoBehaviour
{
    public MonsterVATAsset Asset;
    public Material Material;
    public float FPS = 30f;

    private static readonly int PositionTextureHash = Shader.PropertyToID("_PositionTexture");
    private static readonly int NormalTextureHash = Shader.PropertyToID("_NormalTexture");
    private static readonly int VertexCountHash = Shader.PropertyToID("_VertexCount");
    private static readonly int FrameCountHash = Shader.PropertyToID("_FrameCount");
    private static readonly int CurrentFrameHash = Shader.PropertyToID("_CurrentFrame");
    private static readonly int NextFrameHash = Shader.PropertyToID("_NextFrame");
    private static readonly int BlendHash = Shader.PropertyToID("_Blend");

    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    private float _frame;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();
        if (Asset == null || Material == null)
            return;
        Material.SetTexture(PositionTextureHash, Asset.PositionTexture);
        Material.SetTexture(NormalTextureHash, Asset.NormalTexture);
        Material.SetFloat(VertexCountHash, Asset.VertexCount);
        Material.SetFloat(FrameCountHash, Asset.FrameCount);
        Material.enableInstancing = true;
    }

    private void Update()
    {
        if (Asset == null || Material == null || _renderer == null || Asset.FrameCount <= 0)
            return;
        _frame = Mathf.Repeat(_frame + FPS * Time.deltaTime, Asset.FrameCount);
        float currentFrame = Mathf.Floor(_frame);
        _renderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetFloat(CurrentFrameHash, currentFrame);
        _propertyBlock.SetFloat(NextFrameHash, currentFrame);
        _propertyBlock.SetFloat(BlendHash, 0f);
        _renderer.SetPropertyBlock(_propertyBlock);
    }
}
