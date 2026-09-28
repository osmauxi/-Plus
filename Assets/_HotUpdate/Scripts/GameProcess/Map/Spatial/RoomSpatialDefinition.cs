using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map
{
    public sealed class RoomSpatialDefinition : MonoBehaviour
    {
        [SerializeField] private RoomSpatialData _spatialData;

        public RoomSpatialData SpatialData => _spatialData;

#if UNITY_EDITOR
        public void EditorSetSpatialData(RoomSpatialData spatialData) => _spatialData = spatialData;
#endif
    }
}