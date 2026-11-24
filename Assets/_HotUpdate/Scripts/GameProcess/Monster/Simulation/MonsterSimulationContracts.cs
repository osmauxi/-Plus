using ProjectGame.Core.Monsters;
using ProjectGame.HotFix.Gameplay.Map;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 房间层交给模拟后端的一次 Tick 输入。
    /// 该结构只组织当前已有的数据引用，不在 Tick 中创建临时集合。
    /// 后续 Native SoA 后端可以用相同语义构建自己的 Native 输入视图。
    /// </summary>
    public readonly struct MonsterSimulationTickInput
    {
        public readonly MonsterWorld World;
        public readonly MonsterPlayerTargetBuffer Players;
        public readonly MonsterOccupancySnapshot OccupancySnapshot;
        public readonly uint Tick;
        public readonly float DeltaTime;
        public readonly float GroundY;

        public MonsterSimulationTickInput(
            MonsterWorld world,
            MonsterPlayerTargetBuffer players,
            MonsterOccupancySnapshot occupancySnapshot,
            uint tick,
            float deltaTime,
            float groundY)
        {
            World = world;
            Players = players;
            OccupancySnapshot = occupancySnapshot;
            Tick = tick;
            DeltaTime = deltaTime;
            GroundY = groundY;
        }
    }

    /// <summary>
    /// 模拟后端输出的本 Tick 结果。
    /// 怪物字段直接写回 World 的 SoA 数组，输出结构只携带需要跨 Tick 阶段消费的请求。
    /// </summary>
    public readonly struct MonsterSimulationTickOutput
    {
        public readonly MonsterAttackRequestData[] MeleeAttackRequests;
        public readonly int MeleeAttackCount;

        public MonsterSimulationTickOutput(
            MonsterAttackRequestData[] meleeAttackRequests,
            int meleeAttackCount)
        {
            MeleeAttackRequests = meleeAttackRequests;
            MeleeAttackCount = meleeAttackCount;
        }
    }
}
