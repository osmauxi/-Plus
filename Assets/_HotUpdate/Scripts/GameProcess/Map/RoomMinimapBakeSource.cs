using UnityEngine;

[DisallowMultipleComponent]
public sealed class RoomMinimapBakeSource : MonoBehaviour
{
    [SerializeField] private Transform _bakeRoot;
    [SerializeField, Min(0f)] private float _padding = 1f;

    public Transform BakeRoot => _bakeRoot;
    public float Padding => _padding;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_bakeRoot == null) _bakeRoot = transform.Find("MinimapBakeRoot");
    }
#endif
}