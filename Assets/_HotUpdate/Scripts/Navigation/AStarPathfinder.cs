using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Navigation
{
    public readonly struct GridPoint 
    {
        public readonly int X;
        public readonly int Y;

        public GridPoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
    public interface INavigationGrid : IAStarGrid
    {
        int GetAdditionalCost(int x, int y);
    }
    public interface IAStarGrid 
    {
        int Width { get; }
        int Height { get; }
        bool IsWalkable(int x, int y);
    }
    /// <summary>
    /// 纯C#八向移动A*寻路算法
    /// </summary>
    public sealed class AStarPathfinder
    {
        private static readonly int[] NeighborX = { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static readonly int[] NeighborY = { -1, -1, -1, 0, 0, 1, 1, 1 };

        private int[] _gCost = Array.Empty<int>();
        //记录是从哪一个格子走过来的
        private int[] _parent = Array.Empty<int>();
        //0未访问，1打开(被搜索发现了，但还没走)，2关闭(已经搜索且展开算过了)
        private byte[] _state = Array.Empty<byte>();

        private readonly List<int> _open = new();

        public bool TryFindPath(IAStarGrid grid, GridPoint start, GridPoint goal, List<GridPoint> result)
        {
            result.Clear();

            if (!IsValid(grid, start.X, start.Y) || !IsValid(grid, goal.X, goal.Y))
                return false;
            if (!grid.IsWalkable(start.X, start.Y) || !grid.IsWalkable(goal.X, goal.Y))
                return false;

            if (start.X == goal.X && start.Y == goal.Y)
            {
                result.Add(start);
                return true;
            }
            //初始化数组
            int nodeCount = grid.Width * grid.Height;
            EnsureCapacity(nodeCount);

            Array.Fill(_gCost, int.MaxValue, 0, nodeCount);
            Array.Fill(_parent, -1, 0, nodeCount);
            Array.Clear(_state, 0, nodeCount);
            _open.Clear();

            int startIndex = ToIndex(start.X, start.Y, grid.Width);
            int goalIndex = ToIndex(goal.X, goal.Y, grid.Width);

            _gCost[startIndex] = 0;
            _state[startIndex] = 1;
            _open.Add(startIndex);

            while (_open.Count > 0)
            {
                int current = PopBestOpen(grid.Width, goal);
                if (current == goalIndex)
                {
                    BuildPath(current, grid.Width, result);
                    return true;
                }

                _state[current] = 2;
                int currentX = current % grid.Width;
                int currentY = current / grid.Width;

                for (int i = 0; i < NeighborX.Length; i++)
                {
                    int offsetX = NeighborX[i];
                    int offsetY = NeighborY[i];
                    int nextX = currentX + offsetX;
                    int nextY = currentY + offsetY;

                    if (!IsValid(grid, nextX, nextY) || !grid.IsWalkable(nextX, nextY))
                        continue;

                    //斜向移动时，两侧正交格必须都能走，防止穿墙角。
                    if (offsetX != 0 && offsetY != 0 &&
                        (!grid.IsWalkable(currentX + offsetX, currentY) ||
                         !grid.IsWalkable(currentX, currentY + offsetY)))
                        continue;

                    int next = ToIndex(nextX, nextY, grid.Width);
                    if (_state[next] == 2)
                        continue;
                    //计算新的G,斜向开销为14，正向开销10，14是取路程大小根号二来的
                    int newCost = _gCost[current] + (offsetX != 0 && offsetY != 0 ? 14 : 10);
                    if (newCost >= _gCost[next])
                        continue;

                    _gCost[next] = newCost;
                    _parent[next] = current;

                    if (_state[next] == 1)
                        continue;

                    _state[next] = 1;
                    _open.Add(next);
                }
            }

            return false;
        }
        /// <summary>
        /// 扫一遍所有open，找出F最小的，也就是到达目的地开销最小的节点
        /// 算是手写PriorityQueue，但是时间复杂度更高一些
        /// </summary>
        private int PopBestOpen(int width, GridPoint goal)
        {
            int bestListIndex = 0;
            int bestNode = _open[0];
            int bestH = Heuristic(bestNode % width, bestNode / width, goal.X, goal.Y);
            int bestF = _gCost[bestNode] + bestH;

            for (int i = 1; i < _open.Count; i++)
            {
                int node = _open[i];
                int h = Heuristic(node % width, node / width, goal.X, goal.Y);
                int f = _gCost[node] + h;

                if (f > bestF || f == bestF && h >= bestH)
                    continue;

                bestListIndex = i;
                bestNode = node;
                bestF = f;
                bestH = h;
            }
            //这里删除逻辑也能优化，Open是不吃顺序的
            _open.RemoveAt(bestListIndex);
            return bestNode;
        }

        private void BuildPath(int goalIndex, int width, List<GridPoint> result)
        {
            //Parent因为遍历顺序，读出来是反着的，输出通过reverse倒一遍
            for (int current = goalIndex; current >= 0; current = _parent[current])
                result.Add(new GridPoint(current % width, current / width));

            result.Reverse();
        }

        /// <summary>
        /// 8方向下的距离启发函数
        /// </summary>
        private static int Heuristic(int x, int y, int goalX, int goalY)
        {
            int dx = Math.Abs(goalX - x);
            int dy = Math.Abs(goalY - y);
            int diagonal = Math.Min(dx, dy);
            int straight = Math.Max(dx, dy) - diagonal;
            return diagonal * 14 + straight * 10;
        }

        private void EnsureCapacity(int count)
        {
            if (_gCost.Length >= count)
                return;

            _gCost = new int[count];
            _parent = new int[count];
            _state = new byte[count];
        }
        /// <summary>
        /// 换算X,Y到一维数组
        /// </summary>
        private static int ToIndex(int x, int y, int width) => y * width + x;
        private static bool IsValid(IAStarGrid grid, int x, int y) => (uint)x < grid.Width && (uint)y < grid.Height;
    }


}
