using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map
{
    public static class RoomSpatialUtility
    {
        public static bool WorldToCell(Transform roomRoot,Vector3 worldPosition,out Vector2Int cell)
        {
            //将玩家位置映射到房间局部位置
            Vector3 local = roomRoot.InverseTransformPoint(worldPosition);
            //原始中心在房间中点，坐标范围为-35 - 35.要转换到0-70.所以+35，再除单个格子大小然后取整
            int x = Mathf.FloorToInt((local.x + RoomSpatialData.HalfSize) / RoomSpatialData.CellSize);
            int y = Mathf.FloorToInt((local.z + RoomSpatialData.HalfSize) / RoomSpatialData.CellSize);

            cell = new Vector2Int(x, y);
            return RoomSpatialData.IsValid(x, y);
        }

        public static Vector3 CellToWorld(Transform roomRoot, int x, int y, float localY = 0f)
        {
            //+0.5为人为偏移，我们希望尽可能朝中心走
            float localX = -RoomSpatialData.HalfSize + (x + 0.5f) * RoomSpatialData.CellSize;
            float localZ = -RoomSpatialData.HalfSize + (y + 0.5f) * RoomSpatialData.CellSize;

            return roomRoot.TransformPoint(new Vector3(localX, localY, localZ));
        }
    }
}