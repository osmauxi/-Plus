using UnityEngine;

namespace ProjectGame.Core.Navigation
{
    public readonly struct FlowTargetCellData
    {
        public const byte Valid = 1;
        public const byte Recovery = 2;

        public readonly byte Flags;
        public readonly int SourceIndex;
        public readonly Vector2 WorldDirection;
        public readonly Vector2 RecoveryPosition;

        public FlowTargetCellData(
            byte flags,
            int sourceIndex,
            Vector2 worldDirection,
            Vector2 recoveryPosition)
        {
            Flags = flags;
            SourceIndex = sourceIndex;
            WorldDirection = worldDirection;
            RecoveryPosition = recoveryPosition;
        }

        public static FlowTargetCellData Invalid =>
            new(0, -1, Vector2.zero, Vector2.zero);
    }

    public readonly struct MonsterTargetResult
    {
        public const byte HasTarget = 1;
        public const byte Recovering = 2;
        public const byte TargetWalkable = 4;

        public readonly byte Flags;
        public readonly Vector2 TargetPosition;
        public readonly float TargetDistanceSqr;
        public readonly Vector2 FlowDirection;

        public MonsterTargetResult(
            byte flags,
            Vector2 targetPosition,
            float targetDistanceSqr,
            Vector2 flowDirection)
        {
            Flags = flags;
            TargetPosition = targetPosition;
            TargetDistanceSqr = targetDistanceSqr;
            FlowDirection = flowDirection;
        }

        public static MonsterTargetResult None =>
            new(0, Vector2.zero, float.MaxValue, Vector2.zero);
    }

    /// <summary>
    /// 只处理已经准备好的 Cell/Player/Monster 数组，不依赖 MonsterWorld、Transform 或 HotFix 容器。
    /// </summary>
    public static class FlowTargetKernel
    {
        public static void ResolveBatch(
            int[] activeSlots,
            int activeCount,
            Vector2[] monsterPositions,
            int[] fineCellOfSlot,
            FlowTargetCellData[] cells,
            Vector2[] playerPositions,
            byte[] playerWalkable,
            int playerCount,
            MonsterTargetResult[] results)
        {
            if (activeSlots == null || monsterPositions == null || fineCellOfSlot == null ||
                cells == null || playerPositions == null || playerWalkable == null || results == null)
                return;

            int count = Mathf.Min(activeCount, activeSlots.Length);
            for (int activeIndex = 0; activeIndex < count; activeIndex++)
            {
                int slot = activeSlots[activeIndex];
                if ((uint)slot >= (uint)monsterPositions.Length ||
                    (uint)slot >= (uint)fineCellOfSlot.Length ||
                    (uint)slot >= (uint)results.Length)
                    continue;

                int cellIndex = fineCellOfSlot[slot];
                if ((uint)cellIndex >= (uint)cells.Length)
                {
                    results[slot] = MonsterTargetResult.None;
                    continue;
                }

                FlowTargetCellData cell = cells[cellIndex];
                int sourceIndex = cell.SourceIndex;
                if ((uint)sourceIndex >= (uint)playerCount ||
                    (uint)sourceIndex >= (uint)playerPositions.Length ||
                    (uint)sourceIndex >= (uint)playerWalkable.Length)
                {
                    results[slot] = MonsterTargetResult.None;
                    continue;
                }

                Vector2 targetPosition = playerPositions[sourceIndex];
                byte flags = 0;
                if ((cell.Flags & FlowTargetCellData.Recovery) != 0)
                {
                    Vector2 recoveryOffset = cell.RecoveryPosition - monsterPositions[slot];
                    if (recoveryOffset.sqrMagnitude <= 0.000001f)
                    {
                        results[slot] = MonsterTargetResult.None;
                        continue;
                    }

                    flags |= MonsterTargetResult.Recovering;
                    results[slot] = new MonsterTargetResult(
                        (byte)(flags | MonsterTargetResult.HasTarget |
                               (playerWalkable[sourceIndex] != 0
                                   ? MonsterTargetResult.TargetWalkable : 0)),
                        targetPosition,
                        (targetPosition - monsterPositions[slot]).sqrMagnitude,
                        recoveryOffset.normalized);
                    continue;
                }

                if ((cell.Flags & FlowTargetCellData.Valid) == 0)
                {
                    results[slot] = MonsterTargetResult.None;
                    continue;
                }

                Vector2 offset = targetPosition - monsterPositions[slot];
                results[slot] = new MonsterTargetResult(
                    (byte)(MonsterTargetResult.HasTarget |
                           (playerWalkable[sourceIndex] != 0
                               ? MonsterTargetResult.TargetWalkable : 0)),
                    targetPosition,
                    offset.sqrMagnitude,
                    cell.WorldDirection);
            }
        }
    }
}
