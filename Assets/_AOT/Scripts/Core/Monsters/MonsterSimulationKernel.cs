using ProjectGame.Core.Navigation;
using Unity.Profiling;
using UnityEngine;

namespace ProjectGame.Core.Monsters
{
    /// <summary>
    /// 怪物 EntityTick 的 AOT 数据内核。
    /// 输入和输出都是连续数组，HotFix 只负责环境更新与战斗结算。
    /// </summary>
    public static class MonsterSimulationKernel
    {
        private static readonly ProfilerMarker EntityTickMarker =
            new("Monster.Simulation.EntityTick");
        private static readonly ProfilerMarker EntityTargetMarker =
            new("Monster.Simulation.EntityTick.Target");
        private static readonly ProfilerMarker EntityAttackMarker =
            new("Monster.Simulation.EntityTick.Attack");
        private static readonly ProfilerMarker EntityNavigationMarker =
            new("Monster.Simulation.EntityTick.Navigation");
        private static readonly ProfilerMarker EntitySeparationMarker =
            new("Monster.Simulation.EntityTick.Separation");
        private static readonly ProfilerMarker EntityConstraintMarker =
            new("Monster.Simulation.EntityTick.Constraint");
        private static readonly ProfilerMarker EntityConstraintKernelMarker =
            new("Monster.Simulation.EntityTick.Constraint.Kernel");

        public static int Tick(
            MonsterMetaData[] meta,
            MonsterMotionData[] motion,
            MonsterStatusData[] status,
            MonsterAttackData[] attack,
            MonsterPresentationData[] presentation,
            MonsterSimulationConfig[] configs,
            int[] activeSlots,
            int activeCount,
            int[] fineCellOfSlot,
            FlowTargetCellData[] flowCellData,
            Vector2[] playerPositions,
            byte[] playerWalkable,
            Vector2[] targetPositionScratch,
            MonsterTargetResult[] targetResults,
            Vector2[] separation,
            RoomGridTransformData gridTransform,
            byte[] walkableCells,
            int gridWidth,
            int gridHeight,
            uint tick,
            float deltaTime,
            MonsterAttackRequestData[] attackRequests)
        {
            using var entityTickScope = EntityTickMarker.Auto();

            using (EntityTargetMarker.Auto())
            {
                FlowTargetKernel.ResolveBatch(
                    activeSlots,
                    activeCount,
                    targetPositionScratch,
                    fineCellOfSlot,
                    flowCellData,
                    playerPositions,
                    playerWalkable,
                    targetResults);
            }

            int attackRequestCount = 0;
            for (int activeIndex = 0; activeIndex < activeCount; activeIndex++)
            {
                int slot = activeSlots[activeIndex];
                ref MonsterMotionData currentMotion = ref motion[slot];
                MonsterTargetResult target = targetResults[slot];
                bool hasTarget = (target.Flags & MonsterTargetResult.HasTarget) != 0;
                bool isRecovering = (target.Flags & MonsterTargetResult.Recovering) != 0;
                bool targetWalkable = (target.Flags & MonsterTargetResult.TargetWalkable) != 0;
                ref readonly MonsterSimulationConfig config =
                    ref configs[meta[slot].ConfigIndex];

                if (!HasReached(tick, status[slot].StunEndTick))
                {
                    currentMotion.Velocity = Vector2.zero;
                    continue;
                }

                if (isRecovering)
                {
                    Vector2 recoveryVelocity = target.FlowDirection * config.MoveSpeed *
                        GetSpeedMultiplier(ref status[slot], tick);
                    Integrate(ref currentMotion, recoveryVelocity, deltaTime);
                    continue;
                }

                using (EntityAttackMarker.Auto())
                {
                    TickAttack(
                        ref attack[slot],
                        ref presentation[slot],
                        slot,
                        config,
                        tick,
                        hasTarget,
                        target.TargetDistanceSqr,
                        attackRequests,
                        ref attackRequestCount);
                }

                Vector2 navigationVelocity;
                using (EntityNavigationMarker.Auto())
                {
                    navigationVelocity = CalculateVelocity(
                        ref currentMotion,
                        ref status[slot],
                        attack[slot].Phase,
                        config,
                        tick,
                        hasTarget,
                        target.TargetPosition,
                        target.FlowDirection,
                        targetWalkable);
                }

                Vector2 velocity = navigationVelocity;
                if (attack[slot].Phase == MonsterAttackPhase.Ready)
                {
                    using (EntitySeparationMarker.Auto())
                    {
                        velocity = ApplySeparation(
                            velocity,
                            separation[slot],
                            ref status[slot],
                            config,
                            tick);
                    }
                }

                using (EntityConstraintMarker.Auto())
                {
                    using (EntityConstraintKernelMarker.Auto())
                    {
                        velocity = MovementConstraintKernel.Constrain(
                            gridTransform,
                            walkableCells,
                            gridWidth,
                            gridHeight,
                            currentMotion.Position,
                            navigationVelocity,
                            velocity,
                            deltaTime);
                    }
                }

                Integrate(ref currentMotion, velocity, deltaTime);
            }

            return attackRequestCount;
        }

        private static void TickAttack(
            ref MonsterAttackData attack,
            ref MonsterPresentationData presentation,
            int slot,
            in MonsterSimulationConfig config,
            uint tick,
            bool hasTarget,
            float targetDistanceSqr,
            MonsterAttackRequestData[] attackRequests,
            ref int attackRequestCount)
        {
            if (attack.Phase == MonsterAttackPhase.Ready)
            {
                if (!hasTarget || targetDistanceSqr > config.AttackRangeSqr)
                    return;

                attack.Phase = MonsterAttackPhase.Windup;
                attack.PhaseEndTick = tick + config.WindupTicks;
                presentation.AttackSequence++;
                return;
            }

            if (!HasReached(tick, attack.PhaseEndTick))
                return;

            if (attack.Phase == MonsterAttackPhase.Windup)
            {
                attackRequests[attackRequestCount++] = new MonsterAttackRequestData(
                    slot,
                    config.AttackProfileIndex,
                    config.AttackDamage);
                attack.Phase = MonsterAttackPhase.Recovery;
                attack.PhaseEndTick = tick + config.RecoveryTicks;
                return;
            }

            attack.Phase = MonsterAttackPhase.Ready;
            attack.PhaseEndTick = 0;
        }

        private static Vector2 CalculateVelocity(
            ref MonsterMotionData motion,
            ref MonsterStatusData status,
            MonsterAttackPhase attackPhase,
            in MonsterSimulationConfig config,
            uint tick,
            bool hasTarget,
            Vector2 targetPosition,
            Vector2 flowDirection,
            bool targetWalkable)
        {
            if (attackPhase != MonsterAttackPhase.Ready || !hasTarget)
                return Vector2.zero;

            Vector2 direction = flowDirection;
            Vector2 targetOffset = targetPosition - motion.Position;
            if (targetOffset.sqrMagnitude <= config.AttackRangeSqr || !targetWalkable)
                return Vector2.zero;

            if (direction.sqrMagnitude <= 0.000001f)
                direction = targetOffset.normalized;
            else
                direction = (direction * 0.75f + targetOffset.normalized * 0.25f).normalized;

            return direction * config.MoveSpeed * GetSpeedMultiplier(ref status, tick);
        }

        private static Vector2 ApplySeparation(
            Vector2 navigationVelocity,
            Vector2 separation,
            ref MonsterStatusData status,
            in MonsterSimulationConfig config,
            uint tick)
        {
            Vector2 velocity = navigationVelocity + separation;
            float maxSpeed = config.MoveSpeed * GetSpeedMultiplier(ref status, tick);
            float maxSpeedSqr = maxSpeed * maxSpeed;
            if (velocity.sqrMagnitude > maxSpeedSqr)
                velocity = velocity.normalized * maxSpeed;
            return velocity;
        }

        private static void Integrate(
            ref MonsterMotionData motion,
            Vector2 velocity,
            float deltaTime)
        {
            motion.Velocity = velocity;
            motion.Position += velocity * deltaTime;
            if (velocity.sqrMagnitude > 0.000001f)
                motion.Yaw = Mathf.Atan2(velocity.x, velocity.y) * Mathf.Rad2Deg;
        }

        private static float GetSpeedMultiplier(ref MonsterStatusData status, uint tick)
        {
            if (HasReached(tick, status.SlowEndTick))
            {
                status.SpeedMultiplier = 1f;
                return 1f;
            }

            return status.SpeedMultiplier;
        }

        private static bool HasReached(uint tick, uint deadline) =>
            unchecked((int)(tick - deadline)) >= 0;
    }
}
