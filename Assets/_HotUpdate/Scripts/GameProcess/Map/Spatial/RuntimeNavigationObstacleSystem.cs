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
                    _grid.SetBlocked(x, y, blocked);
            }
        }

        /// <summary>
        /// BoxCollider.bounds的四个XZ角转换到RoomLocal
        /// 当前规则限定墙壁是顺着房间方向来，不出现斜向移动的情况
        /// 斜向移动目前会出现占用额外地块进Block的情况
        /// </summary>
        private bool TryCalculateCells(RuntimeNavigationObstacle obstacle,out RectInt cells)
        {
            cells = default;

            BoxCollider box = obstacle.Bounds;

            if (box == null)
                return false;

            Bounds bounds = box.bounds;

            Vector3 min = _roomRoot.InverseTransformPoint(
                new Vector3(bounds.min.x, bounds.center.y, bounds.min.z));

            Vector3 max = _roomRoot.InverseTransformPoint(
                new Vector3(bounds.max.x, bounds.center.y, bounds.max.z));

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