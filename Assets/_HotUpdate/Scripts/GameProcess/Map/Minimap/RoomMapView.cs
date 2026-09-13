using System.Collections.Generic;
using UnityEngine;

public sealed class MinimapView : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private RectTransform _rotationRoot;
    [SerializeField] private RectTransform _mapContent;
    [SerializeField] private RectTransform _roomLayer;
    [SerializeField] private RectTransform _connectionLayer;
    [SerializeField] private RectTransform _topologyMarkerLayer;
    [SerializeField] private RectTransform _entityMarkerLayer;

    [Header("Prefabs")]
    [SerializeField] private MinimapRoomView _roomPrefab;

    private readonly Dictionary<int, MinimapRoomView> _rooms = new();

    public RectTransform EntityMarkerLayer => _entityMarkerLayer;

    public MinimapRoomView CreateRoom(int roomId, Sprite layout, Vector2 position, Vector2 size, float worldYaw, Sprite typeIcon = null)
    {
        if (_rooms.ContainsKey(roomId)) return _rooms[roomId];

        MinimapRoomView room = Instantiate(_roomPrefab, _roomLayer);
        room.name = $"MinimapRoom_{roomId}";
        room.SetLayout(layout);
        room.SetTransform(position, size, worldYaw);
        room.SetTypeIcon(typeIcon);
        _rooms.Add(roomId, room);
        return room;
    }

    public bool TryGetRoom(int roomId, out MinimapRoomView room) => _rooms.TryGetValue(roomId, out room);

    public void SetMapPosition(Vector2 position) => _mapContent.anchoredPosition = position;
    public void SetMapScale(float scale) => _mapContent.localScale = Vector3.one * scale;
    public void SetMapRotation(float cameraYaw) => _rotationRoot.localEulerAngles = new Vector3(0f, 0f, -cameraYaw);

    public void ClearRooms()
    {
        foreach (MinimapRoomView room in _rooms.Values)
            if (room != null) Destroy(room.gameObject);

        _rooms.Clear();
    }
}