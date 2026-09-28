using System;
using UnityEngine;
using ProjectGame.HotFix.Gameplay.Navigation;

namespace ProjectGame.HotFix.Gameplay.Map
{
    /// <summary>
    /// Cell采样时单个格子可能的状态
    /// </summary>
    public enum RoomSpatialCell : byte 
    {
        Empty = 0,
        Floor = 1,
        Obstacle = 2,
    }

    /// <summary>
    /// Cell采样后的数据存放
    /// </summary>
    public sealed class RoomSpatialData : ScriptableObject, IAStarGrid
    {
        /// <summary>
        /// 当前默认地图大小为Plane 7*7*7，对应地面70*70
        /// </summary>
        public const int GridSize = 70;
        /// <summary>
        /// 将地图切分为70*70 4900个格子，每个为1*1，非常直观
        /// </summary>
        public const int CellSize = 1;
        public const float HalfSize = GridSize * CellSize * 0.5f;
        /// <summary>
        /// Cell映射天然为二维数组，但Unity原生序列化不支持二维数组，所以只能把他拍平了
        /// </summary>
        [SerializeField]private RoomSpatialCell[] _cells = new RoomSpatialCell[GridSize * GridSize];

        public int Width => GridSize;
        public int Height => GridSize;

        public RoomSpatialCell Get(int x,int y) 
        {
            return IsValid(x,y) ? _cells[y * GridSize + x]  : RoomSpatialCell.Empty;
        }

        public bool IsWalkable(int x,int y) 
        {
            return Get(x,y) == RoomSpatialCell.Floor;
        }

        public  static bool IsValid(int x, int y)
        {
            return (uint)x < GridSize && (uint)y < GridSize;
        }

#if UNITY_EDITOR
        public void EditorSetCells(RoomSpatialCell[] cells)
        {
            if (cells == null || cells.Length != GridSize * GridSize)
                throw new System.ArgumentException($"RoomSpatialData 必须包含 {GridSize * GridSize} 个 Cell。");

            _cells = cells;
        }
#endif
    }
}