using UnityEngine;
using UnityEngine.UI;

public sealed class MinimapRoomView : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Image _layoutImage;
    [SerializeField] private Image _typeIcon;

    public RectTransform RectTransform => _rectTransform;

    private void Awake()
    {
        if (_rectTransform == null) 
            _rectTransform = (RectTransform)transform;
        _layoutImage.raycastTarget = false;
        _typeIcon.raycastTarget = false;
    }

    public void SetLayout(Sprite sprite) => _layoutImage.sprite = sprite;

    public void SetTransform(Vector2 position, Vector2 size, float worldYaw)
    {
        _rectTransform.anchoredPosition = position;
        _rectTransform.sizeDelta = size;
        _rectTransform.localEulerAngles = new Vector3(0f, 0f, -worldYaw);
    }

    public void SetTypeIcon(Sprite sprite)
    {
        _typeIcon.sprite = sprite;
        _typeIcon.enabled = sprite != null;
    }

    public void SetVisible(bool visible) => gameObject.SetActive(visible);
}