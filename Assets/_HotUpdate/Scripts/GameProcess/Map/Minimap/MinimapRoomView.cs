using UnityEngine;
using UnityEngine.UI;

public sealed class MinimapRoomView : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Image _layoutImage;

    public Vector2 Size => _rectTransform.sizeDelta;

    public void Configure(RectTransform rectTransform, Image layoutImage)
    {
        _rectTransform = rectTransform;
        _layoutImage = layoutImage;
        _layoutImage.raycastTarget = false;
    }

    public void SetLayout(Sprite sprite, Vector2 size)
    {
        _layoutImage.sprite = sprite;
        _layoutImage.preserveAspect = true;
        _rectTransform.anchoredPosition = Vector2.zero;
        _rectTransform.sizeDelta = size;
        _rectTransform.localEulerAngles = Vector3.zero;
    }
}
