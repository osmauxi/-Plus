using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map
{
    public sealed class RoomSpatialBakeBlock : MonoBehaviour
    {
        [SerializeField] private RoomSpatialCell _cellType = RoomSpatialCell.Floor;
        public RoomSpatialCell CellType => _cellType;
    }
}