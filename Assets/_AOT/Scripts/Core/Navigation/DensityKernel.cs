using System;

namespace ProjectGame.Core.Navigation
{
    public sealed class DensityKernel
    {
        private readonly int _costPerInfluence;
        private readonly int _maxDensityCost;
        private readonly float _smoothFactor;

        private int[] _monsterCounts = Array.Empty<int>();
        private int[] _influence = Array.Empty<int>();
        private float[] _smoothedCost = Array.Empty<float>();
        private ushort[] _outputCost = Array.Empty<ushort>();

        private readonly sbyte[] _kernelX;
        private readonly sbyte[] _kernelY;
        private readonly byte[] _kernelWeight;

        public int[] MonsterCounts => _monsterCounts;
        public ushort[] OutputCost => _outputCost;

        public DensityKernel(
            int radius,
            int costPerInfluence,
            int maxDensityCost,
            float smoothFactor)
        {
            _costPerInfluence = costPerInfluence;
            _maxDensityCost = maxDensityCost;
            _smoothFactor = smoothFactor;

            int size = radius * 2 + 1;
            int kernelCount = size * size;

            _kernelX = new sbyte[kernelCount];
            _kernelY = new sbyte[kernelCount];
            _kernelWeight = new byte[kernelCount];

            int index = 0;

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    int absX = x < 0 ? -x : x;
                    int absY = y < 0 ? -y : y;
                    int distance = absX > absY ? absX : absY;

                    _kernelX[index] = (sbyte)x;
                    _kernelY[index] = (sbyte)y;
                    _kernelWeight[index] = (byte)(radius + 1 - distance);
                    index++;
                }
            }
        }

        public void Prepare(int count)
        {
            EnsureCapacity(count);
            Array.Clear(_monsterCounts, 0, count);
            Array.Clear(_influence, 0, count);
        }

        public void Build(byte[] walkable, int width, int height)
        {
            int count = width * height;
            BuildInfluence(walkable, width, height, count);
            BuildCost(count);
        }

        private void BuildInfluence(
            byte[] walkable,
            int width,
            int height,
            int count)
        {
            for (int center = 0; center < count; center++)
            {
                int monsterCount = _monsterCounts[center];

                if (monsterCount == 0)
                    continue;

                int centerX = center % width;
                int centerY = center / width;

                for (int i = 0; i < _kernelX.Length; i++)
                {
                    int targetX = centerX + _kernelX[i];
                    int targetY = centerY + _kernelY[i];

                    if ((uint)targetX >= (uint)width ||
                        (uint)targetY >= (uint)height)
                        continue;

                    int target = targetY * width + targetX;

                    if (walkable[target] == 0)
                        continue;

                    _influence[target] +=
                        monsterCount * _kernelWeight[i];
                }
            }
        }

        private void BuildCost(int count)
        {
            for (int i = 0; i < count; i++)
            {
                int targetCost = _influence[i] * _costPerInfluence;

                if (targetCost > _maxDensityCost)
                    targetCost = _maxDensityCost;

                float current = _smoothedCost[i];
                current += (targetCost - current) * _smoothFactor;

                _smoothedCost[i] = current;
                _outputCost[i] = (ushort)(current + 0.5f);
            }
        }

        private void EnsureCapacity(int count)
        {
            if (_influence.Length == count)
                return;

            _monsterCounts = new int[count];
            _influence = new int[count];
            _smoothedCost = new float[count];
            _outputCost = new ushort[count];
        }
    }
}
