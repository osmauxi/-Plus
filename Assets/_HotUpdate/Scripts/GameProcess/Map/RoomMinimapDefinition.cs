using UnityEngine;

[DisallowMultipleComponent]
public sealed class RoomMinimapDefinition : MonoBehaviour
{
    [SerializeField] private Sprite _layoutSprite;

    [Header("Bake Mapping")]
    [SerializeField] private Vector2 _localCenter;
    [SerializeField] private Vector2 _localSize;

    [Header("Overview")]
    [SerializeField] private Vector2 _overviewSize = new(70f, 70f);

    public Sprite LayoutSprite => _layoutSprite;
    public Vector2 LocalCenter => _localCenter;
    public Vector2 LocalSize => _localSize;
    public Vector2 OverviewSize => _overviewSize;

#if UNITY_EDITOR
    public void EditorSetBakeResult(Sprite sprite, Vector2 localCenter, Vector2 localSize)
    {
        _layoutSprite = sprite;
        _localCenter = localCenter;
        _localSize = localSize;
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}