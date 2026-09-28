using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Map.Flow;
using ProjectGame.HotFix.Gameplay.Map.Generation;
using UnityEngine;
using UnityEngine.UI;

public enum MinimapDisplayMode : byte
{
    Overview,
    Tactical
}

public sealed class MinimapView : MonoBehaviour
{
    [Header("Viewport")]
    [SerializeField] private RectTransform _viewport;

    [Header("Overview")]
    [SerializeField] private RectTransform _overviewRoot;
    [SerializeField] private RectTransform _overviewMapContent;
    [SerializeField] private RectTransform _overviewConnectionLayer;
    [SerializeField] private RectTransform _overviewRoomLayer;

    [Header("Tactical")]
    [SerializeField] private RectTransform _tacticalRoot;
    [SerializeField] private RectTransform _tacticalRoomLayer;
    [SerializeField] private RectTransform _tacticalEntityLayer;

    [Header("Style")]
    [SerializeField] private Sprite _squareSprite;

    private readonly Dictionary<int, MinimapRoomNodeView> _overviewRooms = new();
    private readonly Dictionary<int, Image> _overviewConnections = new();
    private readonly Dictionary<int, Image> _enemyMarkers = new();

    private MinimapRoomView _tacticalRoom;
    private RectTransform _playerMarker;

    public RectTransform Viewport => _viewport;
    public Vector2 TacticalRoomSize => _tacticalRoom.Size;

    public void Configure(
        RectTransform viewport,
        RectTransform overviewRoot,
        RectTransform overviewMapContent,
        RectTransform overviewConnectionLayer,
        RectTransform overviewRoomLayer,
        RectTransform tacticalRoot,
        RectTransform tacticalRoomLayer,
        RectTransform tacticalEntityLayer,
        Sprite squareSprite)
    {
        _viewport = viewport;
        _overviewRoot = overviewRoot;
        _overviewMapContent = overviewMapContent;
        _overviewConnectionLayer = overviewConnectionLayer;
        _overviewRoomLayer = overviewRoomLayer;
        _tacticalRoot = tacticalRoot;
        _tacticalRoomLayer = tacticalRoomLayer;
        _tacticalEntityLayer = tacticalEntityLayer;
        _squareSprite = squareSprite;
    }

    public void SetMode(MinimapDisplayMode mode)
    {
        _overviewRoot.gameObject.SetActive(mode == MinimapDisplayMode.Overview);
        _tacticalRoot.gameObject.SetActive(mode == MinimapDisplayMode.Tactical);
    }

    public void CreateOverviewRoom(int roomId, RoomType roomType, Vector2 position, float side)
    {
        GameObject root = new GameObject($"OverviewRoom_{roomId}", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        RectTransform rect = (RectTransform)root.transform;
        SetCentered(rect, _overviewRoomLayer);

        Image border = root.GetComponent<Image>();
        border.sprite = _squareSprite;
        border.raycastTarget = false;

        Image background = CreateImage("Background", rect, _squareSprite, Color.white);
        Image typeIcon = CreateImage("RoomType", rect, _squareSprite, Color.white);
        CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();

        MinimapRoomNodeView node = root.AddComponent<MinimapRoomNodeView>();
        node.Configure(rect, border, background, typeIcon, canvasGroup);
        node.SetTransform(position, side);
        node.SetRoomType(roomType);
        _overviewRooms.Add(roomId, node);
    }

    public void CreateOverviewConnection(int connectionId, Vector2 start, Vector2 end)
    {
        Image image = CreateImage($"OverviewConnection_{connectionId}", _overviewConnectionLayer,
            _squareSprite, new Color(0.7f, 0.8f, 0.9f, 0.75f));
        RectTransform rect = image.rectTransform;
        Vector2 delta = end - start;
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = start;
        rect.sizeDelta = new Vector2(delta.magnitude, 2f);
        rect.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        _overviewConnections.Add(connectionId, image);
    }

    public void SetOverviewRoomState(int roomId, RoomFogState fog, bool current, float revealedAlpha) =>
        _overviewRooms[roomId].SetState(fog, current, revealedAlpha);

    public void SetOverviewConnectionVisible(int connectionId, bool visible)
    {
        if (_overviewConnections.TryGetValue(connectionId, out Image connection))
            connection.gameObject.SetActive(visible);
    }

    public void SetOverviewTransform(Vector2 position, float scale)
    {
        _overviewMapContent.anchoredPosition = position;
        _overviewMapContent.localScale = Vector3.one * scale;
    }

    public void ShowTacticalRoom(Sprite layout, Vector2 worldSize, float viewportFill)
    {
        if (_tacticalRoom == null)
            _tacticalRoom = CreateTacticalRoom();

        Vector2 available = _viewport.rect.size * viewportFill;
        float scale = Mathf.Min(available.x / worldSize.x, available.y / worldSize.y);
        _tacticalRoom.SetLayout(layout, worldSize * scale);
    }

    public void SetTacticalPlayer(Vector2 position, float localYaw)
    {
        if (_playerMarker == null)
            _playerMarker = CreatePlayerMarker();

        _playerMarker.anchoredPosition = position;
        _playerMarker.localEulerAngles = new Vector3(0f, 0f, -localYaw);
        _playerMarker.gameObject.SetActive(true);
        _playerMarker.SetAsLastSibling();
    }

    public void HideAllEnemyMarkers()
    {
        foreach (Image marker in _enemyMarkers.Values)
            marker.gameObject.SetActive(false);
    }

    public void SetEnemyMarker(int slot, Vector2 position, bool boss)
    {
        if (!_enemyMarkers.TryGetValue(slot, out Image marker))
        {
            marker = CreateImage($"EnemyMarker_{slot}", _tacticalEntityLayer, _squareSprite, Color.red);
            _enemyMarkers.Add(slot, marker);
        }

        marker.color = boss ? new Color(1f, 0.65f, 0.1f, 1f) : new Color(1f, 0.2f, 0.2f, 1f);
        marker.rectTransform.anchoredPosition = position;
        marker.rectTransform.sizeDelta = boss ? new Vector2(9f, 9f) : new Vector2(6f, 6f);
        marker.gameObject.SetActive(true);
    }

    public void Clear()
    {
        foreach (MinimapRoomNodeView room in _overviewRooms.Values)
            Destroy(room.gameObject);
        _overviewRooms.Clear();

        foreach (Image connection in _overviewConnections.Values)
            Destroy(connection.gameObject);
        _overviewConnections.Clear();

        if (_tacticalRoom != null)
            Destroy(_tacticalRoom.gameObject);
        _tacticalRoom = null;

        if (_playerMarker != null)
            Destroy(_playerMarker.gameObject);
        _playerMarker = null;

        foreach (Image marker in _enemyMarkers.Values)
            Destroy(marker.gameObject);
        _enemyMarkers.Clear();

        _overviewMapContent.anchoredPosition = Vector2.zero;
        _overviewMapContent.localScale = Vector3.one;
        SetMode(MinimapDisplayMode.Overview);
    }

    private MinimapRoomView CreateTacticalRoom()
    {
        Image image = CreateImage("TacticalRoomLayout", _tacticalRoomLayer, _squareSprite, Color.white);
        image.preserveAspect = true;
        MinimapRoomView room = image.gameObject.AddComponent<MinimapRoomView>();
        room.Configure(image.rectTransform, image);
        return room;
    }

    private RectTransform CreatePlayerMarker()
    {
        GameObject root = new GameObject("LocalPlayerMarker", typeof(RectTransform));
        RectTransform rect = (RectTransform)root.transform;
        SetCentered(rect, _tacticalEntityLayer);
        rect.sizeDelta = new Vector2(12f, 16f);

        Image body = CreateImage("Body", rect, _squareSprite, new Color(0.15f, 0.95f, 1f, 1f));
        body.rectTransform.sizeDelta = new Vector2(8f, 8f);
        body.rectTransform.localEulerAngles = new Vector3(0f, 0f, 45f);

        Image heading = CreateImage("Heading", rect, _squareSprite, new Color(0.85f, 1f, 1f, 1f));
        heading.rectTransform.sizeDelta = new Vector2(3f, 7f);
        heading.rectTransform.anchoredPosition = new Vector2(0f, 6f);
        return rect;
    }

    private static Image CreateImage(string name, RectTransform parent, Sprite sprite, Color color)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = (RectTransform)gameObject.transform;
        SetCentered(rect, parent);
        Image image = gameObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static void SetCentered(RectTransform rect, RectTransform parent)
    {
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;
    }
}
