using ProjectGame.Core.Monsters;
using ProjectGame.Core.Navigation;
using ProjectGame.HotFix.Gameplay.Map;
using ProjectGame.HotFix.Gameplay.Navigation;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 怪物模拟的执行后端。策略只负责计算，不拥有网络、表现或房间生命周期。
    /// </summary>
    public interface IMonsterSimulationBackend
    {
        bool HasFlowField { get; }
        int PendingMeleeAttackCount { get; }
        int TopologyRebuildCount { get; }
        int SourceRebuildCount { get; }
        int CostRebuildCount { get; }
        uint LastRebuildTick { get; }

        void Tick(
            MonsterWorld world,
            MonsterPlayerTargetBuffer players,
            uint tick,
            float deltaTime,
            float groundY,
            MonsterOccupancySnapshot occupancySnapshot);

        void ExecutePendingMeleeAttacks(
            MonsterWorld world,
            float groundY,
            MonsterMeleePhysicsResolver resolver);

        bool TryGetFlowFieldDebugCell(int x, int y, out FlowFieldCellDebugData cell);

        bool TryGetTopologyReferenceFlowFieldDebugCell(
            int x,
            int y,
            out FlowFieldCellDebugData cell);

        bool IsFlowProgressCorrectionCell(int x, int y);
        Vector2 GetRawSeparation(int storageIndex);
        void EnsureCapacity(int capacity);
        void ResetDiagnostics();
    }

    /// <summary>
    /// 房间级固定的模拟执行模式。
    /// </summary>
    public enum MonsterSimulationBackendKind : byte
    {
        ManagedSoA = 0,
        ManualNativeSoABurst = 1,
        EntitiesEcs = 2,
    }

    /// <summary>
    /// 创建房间使用的模拟策略。后续后端在这里接入，房间编排不需要变化。
    /// </summary>
    public static class MonsterSimulationBackendFactory
    {
        public static IMonsterSimulationBackend Create(
            MonsterSimulationBackendKind kind,
            MonsterRuntimeCatalog configs,
            MonsterAttackCatalog attackProfiles,
            RuntimeNavigationGrid navigationGrid,
            Transform roomRoot)
        {
            return kind switch
            {
                MonsterSimulationBackendKind.ManagedSoA =>
                    new MonsterSimulation(configs, attackProfiles, navigationGrid, roomRoot),
                _ => throw new System.NotSupportedException(
                    $"Monster 模拟后端尚未接入：{kind}。当前可用后端为 ManagedSoA。"),
            };
        }
    }
}
