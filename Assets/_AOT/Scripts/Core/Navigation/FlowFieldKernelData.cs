using UnityEngine;

namespace ProjectGame.Core.Navigation
{
    public readonly struct NavigationGridKernelData
    {
        public readonly int Width;
        public readonly int Height;
        public readonly byte[] Walkable;
        public readonly byte[] ConnectionMask;
        public readonly int[] TraversalCost;

        public NavigationGridKernelData(
            int width,
            int height,
            byte[] walkable,
            byte[] connectionMask,
            int[] traversalCost)
        {
            Width = width;
            Height = height;
            Walkable = walkable;
            ConnectionMask = connectionMask;
            TraversalCost = traversalCost;
        }
    }

    public readonly struct FlowFieldSource
    {
        public readonly int X;
        public readonly int Y;
        public readonly int SourceIndex;

        public FlowFieldSource(int x, int y, int sourceIndex)
        {
            X = x;
            Y = y;
            SourceIndex = sourceIndex;
        }
    }

    public readonly struct FlowFieldCellDebugData
    {
        public readonly int Cost;
        public readonly Vector2 Direction;
        public readonly int SourceIndex;

        public bool IsReachable => Cost != int.MaxValue;
        public bool IsSource => Cost == 0;

        public FlowFieldCellDebugData(
            int cost,
            Vector2 direction,
            int sourceIndex)
        {
            Cost = cost;
            Direction = direction;
            SourceIndex = sourceIndex;
        }
    }
}
