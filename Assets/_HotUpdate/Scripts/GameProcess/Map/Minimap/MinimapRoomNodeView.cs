using ProjectGame.HotFix.Gameplay.Map.Flow;
using ProjectGame.HotFix.Gameplay.Map.Generation;
using UnityEngine;
using UnityEngine.UI;

public sealed class MinimapRoomNodeView : MonoBehaviour
{
    private static readonly Color NormalBorder = new(0.594f, 0.58f, 0.58f, 1f);
    private static readonly Color CurrentBorder = new(0.15f, 0.95f, 1f, 1f);

    private RectTransform _rectTransform;
    private Image _border;
    private Image _background;
    private Image _typeIcon;
    private CanvasGroup _canvasGroup;

    public Vector2 Position => _rectTransform.anchoredPosition;
    public Vector2 Size => _rectTransform.sizeDelta;

    public void Configure(RectTransform rectTransform, Image border, Image background,
        Image typeIcon, CanvasGroup canvasGroup)
    {
        _rectTransform = rectTransform;
        _border = border;
        _background = background;
        _typeIcon = typeIcon;
        _canvasGroup = canvasGroup;
    }

    public void SetTransform(Vector2 position, float side)
    {
        _rectTransform.anchoredPosition = position;
        _rectTransform.sizeDelta = new Vector2(side, side);
        _background.rectTransform.sizeDelta = new Vector2(side * 0.8f, side * 0.8f);
        _typeIcon.rectTransform.sizeDelta = new Vector2(side * 0.34f, side * 0.34f);
    }

    public void SetRoomType(RoomType roomType)
    {
        _typeIcon.color = roomType switch
        {
            RoomType.Start => new Color(0.25f, 0.9f, 0.45f, 1f),
            RoomType.Combat => new Color(0.95f, 0.25f, 0.22f, 1f),
            RoomType.Elite => new Color(0.72f, 0.35f, 1f, 1f),
            RoomType.Treasure => new Color(1f, 0.78f, 0.16f, 1f),
            RoomType.Shop => new Color(0.2f, 0.82f, 0.95f, 1f),
            RoomType.Boss => new Color(1f, 0.46f, 0.08f, 1f),
            _ => Color.white
        };
        _typeIcon.rectTransform.localEulerAngles = new Vector3(0f, 0f,
            roomType == RoomType.Start || roomType == RoomType.Elite || roomType == RoomType.Boss
                ? 45f
                : 0f);
    }

    public void SetState(RoomFogState fog, bool current, float revealedAlpha)
    {
        gameObject.SetActive(fog != RoomFogState.Hidden);
        _canvasGroup.alpha = fog == RoomFogState.Revealed ? revealedAlpha : 1f;
        _border.color = current ? CurrentBorder : NormalBorder;
        _background.color = new Color(0.74f, 0.74f, 0.74f, 1f);
    }
}
