namespace ProjectGame.Core.Navigation
{
    public static class NavigationTopologyKernel
    {
        private static readonly int[] NeighborX = { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static readonly int[] NeighborY = { -1, -1, -1, 0, 0, 1, 1, 1 };

        public static void BuildConnections(
            int width,
            int height,
            byte[] walkable,
            byte[] connectionMask)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int current = y * width + x;

                    if (walkable[current] == 0)
                    {
                        connectionMask[current] = 0;
                        continue;
                    }

                    byte mask = 0;

                    for (int i = 0; i < 8; i++)
                    {
                        int offsetX = NeighborX[i];
                        int offsetY = NeighborY[i];

                        int nextX = x + offsetX;
                        int nextY = y + offsetY;

                        if ((uint)nextX >= (uint)width ||
                            (uint)nextY >= (uint)height)
                            continue;

                        int next = nextY * width + nextX;

                        if (walkable[next] == 0)
                            continue;

                        if (offsetX != 0 && offsetY != 0)
                        {
                            int sideX = y * width + nextX;
                            int sideY = nextY * width + x;

                            if (walkable[sideX] == 0 ||
                                walkable[sideY] == 0)
                                continue;
                        }

                        mask |= (byte)(1 << i);
                    }

                    connectionMask[current] = mask;
                }
            }
        }
    }
}
