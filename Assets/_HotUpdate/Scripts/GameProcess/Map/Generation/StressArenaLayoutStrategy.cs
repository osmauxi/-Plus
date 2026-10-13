using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map.Generation
{
    /// <summary>
    /// 大规模怪物导航压测地图参数。
    /// 这里只配置房间布局；怪物数量与生成节奏由 MonsterNavigationStressTestManager 控制。
    /// </summary>
    [Serializable]
    public sealed class StressArenaGenerationProfile
    {
        [Tooltip("压测房间根节点的统一缩放。缩放只扩大世界空间，不会增加固定 70x70 导航网格的 Cell 数量。")]
        [SerializeField, Min(0.1f)] private float _roomScale = 4f;

        public float RoomScale => _roomScale;

        public void Validate()
        {
            if (_roomScale <= 0f || float.IsNaN(_roomScale) || float.IsInfinity(_roomScale))
                throw new InvalidOperationException($"压测房间缩放非法：{_roomScale}");
        }
    }

    /// <summary>
    /// 只生成一个起始房间的压测布局。房间没有连接，玩家仍通过正常关卡流程出生。
    /// </summary>
    public sealed class StressArenaLayoutStrategy : IMapLayoutStrategy
    {
        private readonly float _roomScale;

        public MapLayoutStrategyType StrategyType => MapLayoutStrategyType.StressArena;
        public ConnectionPresentationMode ConnectionMode => ConnectionPresentationMode.Seamless;
        public int Version => 1;

        public StressArenaLayoutStrategy(float roomScale)
        {
            _roomScale = roomScale;
        }

        public MapLayout Generate(MapGenerationRequest request)
        {
            request.ValidateCommon();

            MapRoomDefinition[] rooms =
            {
                new MapRoomDefinition(0, RoomType.Start, Vector3.zero, Quaternion.identity, 0)
            };

            // 当前 MapLayout 要求 BossRoomId 必须引用有效房间。压测场没有 Boss，
            // 暂用起始房间作为兼容 ID；玩法判断仍以 RoomType 为准。
            return new MapLayout(
                request.Seed,
                0,
                StrategyType,
                Version,
                _roomScale,
                0,
                0,
                rooms,
                Array.Empty<MapConnectionDefinition>());
        }
    }
}
