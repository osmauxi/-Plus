using ProjectGame.HotFix.Gameplay.Navigation;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map
{

    public sealed class RuntimeNavigationObstacleSystem
    {
        private readonly RuntimeNavigationGrid _grid;
        private readonly Transform _roomRoot;
        private readonly List<ObstacleState> _states = new();
        private readonly int[] _blockerCounts;

        private struct ObstacleState
        {
            public RuntimeNavigationObstacle Obstacle;
            public RectInt Cells;
            public bool IsBlocking;
        }

        public RuntimeNavigationObstacleSystem(RuntimeNavigationGrid grid,Transform roomRoot)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _roomRoot = roomRoot ?? throw new ArgumentNullException(nameof(roomRoot));
            _blockerCounts = new int[grid.Width * grid.Height];

            RegisterRoomObstacles();
        }
        /// <summary>
        /// 用于高频改变地块阻挡信息的物体Tick
        /// </summary>
        public void Tick()
        {
            for (int i = 0; i < _states.Count; i++)
            {
                ObstacleState state = _states[i];
                if (!state.Obstacle.TrackMovement || !state.IsBlocking)
                    continue;

                UpdateMovingObstacle(i, state);
            }
        }
        private void UpdateMovingObstacle(int index, ObstacleState state)
        {
            if (!TryCalculateCells(state.Obstacle, out RectInt newCells))
                return;

            if (newCells.Equals(state.Cells))
                return;

            SetBlocked(state.Cells, false);
            SetBlocked(newCells, true);

            state.Cells = newCells;
            _states[index] = state;
        }
        /// <summary>
        /// 低频，甚至一次性修改地块信息的物块调用，不进入Tick
        /// </summary>
        public void SetBlocking(RuntimeNavigationObstacle obstacle,bool blocking)
        {
            for (int i = 0; i < _states.Count; i++)
            {
                ObstacleState state = _states[i];

                if (state.Obstacle != obstacle)
                    continue;

                if (state.IsBlocking == blocking)
                    return;

                if (blocking)
                    TryCalculateCells(obstacle, out state.Cells);
                SetBlocked(state.Cells, blocking);

                state.IsBlocking = blocking;
                _states[i] = state;
                return;
            }
        }
        private void RegisterRoomObstacles()
        {
            RuntimeNavigationObstacle[] obstacles =
                _roomRoot.GetComponentsInChildren<RuntimeNavigationObstacle>(true);

            for (int i = 0; i < obstacles.Length; i++)
            {
                RuntimeNavigationObstacle obstacle = obstacles[i];

                if (!TryCalculateCells(obstacle, out RectInt cells))
                    continue;

                _states.Add(new ObstacleState
                {
                    Obstacle = obstacle,
                    Cells = cells,
                    IsBlocking = obstacle.gameObject.activeInHierarchy
                });

                if (obstacle.gameObject.activeInHierarchy)
                    SetBlocked(cells, true);
            }
        }
        private void SetBlocked(RectInt cells, bool blocked)
        {
            for (int y = cells.yMin; y < cells.yMax; y++)
            {
                for (int x = cells.xMin; x < cells.xMax; x++)
                {
                    int index = y * _grid.Width + x;
                    // 巡回墙可能相交；一面墙移开不能清掉另一面墙仍占据的格子。
                    _blockerCounts[index] += blocked ? 1 : -1;
                    _grid.SetBlocked(x, y, _blockerCounts[index] > 0);
                }
            }
        }

        /// <summary>
        /// 将 BoxCollider 转换为房间局部 AABB，旋转的障碍保守占据其包围范围。
        /// </summary>
        private bool TryCalculateCells(RuntimeNavigationObstacle obstacle,out RectInt cells)
        {
            cells = default;

            BoxCollider box = obstacle.Bounds;

            if (box == null)
                return false;

            // 直接使用 Transform，保证移墙后同一 Tick 就能更新导航，无需同步整个物理场景。
            Matrix4x4 matrix = _roomRoot.worldToLocalMatrix * box.transform.localToWorldMatrix;
            Vector3 center = matrix.MultiplyPoint3x4(box.center);
            Vector3 half = box.size * 0.5f;
            Vector3 extent = new Vector3(
                Mathf.Abs(matrix.m00) * half.x + Mathf.Abs(matrix.m01) * half.y + Mathf.Abs(matrix.m02) * half.z,
                Mathf.Abs(matrix.m10) * half.x + Mathf.Abs(matrix.m11) * half.y + Mathf.Abs(matrix.m12) * half.z,
                Mathf.Abs(matrix.m20) * half.x + Mathf.Abs(matrix.m21) * half.y + Mathf.Abs(matrix.m22) * half.z);
            Vector3 min = center - extent;
            Vector3 max = center + extent;

            int minX = Mathf.FloorToInt(
                (Mathf.Min(min.x, max.x) + RoomSpatialData.HalfSize) /
                RoomSpatialData.CellSize);

            int maxX = Mathf.FloorToInt(
                (Mathf.Max(min.x, max.x) + RoomSpatialData.HalfSize) /
                RoomSpatialData.CellSize);

            int minY = Mathf.FloorToInt(
                (Mathf.Min(min.z, max.z) + RoomSpatialData.HalfSize) /
                RoomSpatialData.CellSize);

            int maxY = Mathf.FloorToInt(
                (Mathf.Max(min.z, max.z) + RoomSpatialData.HalfSize) /
                RoomSpatialData.CellSize);

            minX = Mathf.Clamp(minX, 0, _grid.Width - 1);
            maxX = Mathf.Clamp(maxX, 0, _grid.Width - 1);
            minY = Mathf.Clamp(minY, 0, _grid.Height - 1);
            maxY = Mathf.Clamp(maxY, 0, _grid.Height - 1);

            cells = new RectInt(
                minX,
                minY,
                maxX - minX + 1,
                maxY - minY + 1);

            return true;
        }
    }
}
