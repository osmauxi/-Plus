using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Map.Generation;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map.View
{
    /// <summary>
    /// 将纯粹的逻辑地图数据转换为包含具体预制体模板、旋转角度和本地门插槽状态的视觉构建方案
    /// </summary>
    public sealed class GridRoomTemplateSelector
    {
        public MapBuildPlan Resolve(
            MapLayout layout,
            IReadOnlyList<RoomTemplateConfig> templates,
            ConnectionPresentationMode connectionPresentationMode)
        {
            if (layout == null)
                throw new ArgumentNullException(nameof(layout));

            if (templates == null || templates.Count == 0)
                throw new ArgumentException("房间模板列表不能为空。", nameof(templates));

            List<MapRoomBuildDefinition> buildRooms = new List<MapRoomBuildDefinition>(layout.Rooms.Count);

            for (int i = 0; i < layout.Rooms.Count; i++)
            {
                MapRoomDefinition room = layout.Rooms[i];
                ConnectorMask requiredWorldMask = CalculateRequiredWorldMask(layout, room);
                TemplateSelection selection = SelectTemplate(
                    room,
                    requiredWorldMask,
                    templates,
                    layout.Seed,
                    layout.StrategyType);

                buildRooms.Add
                (
                    new MapRoomBuildDefinition
                    (
                        room.RoomId,
                        room.RoomType,
                        selection.Template.TemplateId,
                        room.WorldPosition,
                        room.WorldRotation,
                        room.DistanceFromStart,
                        requiredWorldMask,
                        selection.RotationIndex
                    )
                );
            }

            return new MapBuildPlan(layout, buildRooms, connectionPresentationMode);
        }

        private static ConnectorMask CalculateRequiredWorldMask(MapLayout layout, MapRoomDefinition room)
        {
            ConnectorMask mask = ConnectorMask.None;
            IReadOnlyList<int> neighborIds = layout.GetNeighborRoomIds(room.RoomId);

            for (int i = 0; i < neighborIds.Count; i++)
            {
                if (!layout.TryGetRoom(neighborIds[i], out MapRoomDefinition neighbor))
                    throw new InvalidOperationException($"Room {room.RoomId} 引用了不存在的邻居：{neighborIds[i]}");

                Vector3 offset = neighbor.WorldPosition - room.WorldPosition;

                //GridGraph的连接必须是水平或垂直方向。
                //出现明显斜向偏移，说明布局数据不符合当前选择器规范。
                bool hasX = Mathf.Abs(offset.x) > 0.01f;
                bool hasZ = Mathf.Abs(offset.z) > 0.01f;

                if (hasX == hasZ)
                    throw new InvalidOperationException($"Room {room.RoomId} 到 Room {neighbor.RoomId} 不是有效的网格正交连接。");

                if (hasX)
                    mask |= offset.x > 0f ? ConnectorMask.East : ConnectorMask.West;
                else
                    mask |= offset.z > 0f ? ConnectorMask.North : ConnectorMask.South;
            }

            return mask;
        }

        private static TemplateSelection SelectTemplate(
            MapRoomDefinition room,
            ConnectorMask requiredWorldMask,
            IReadOnlyList<RoomTemplateConfig> templates,
            int mapSeed,
            MapLayoutStrategyType strategyType)
        {
            List<TemplateCandidate> candidates = new List<TemplateCandidate>();
            int highestPriority = int.MinValue;

            for (int i = 0; i < templates.Count; i++)
            {
                RoomTemplateConfig template = templates[i];

                if (!template.SupportsStrategy(strategyType))
                    continue;

                if (template.RoomType != room.RoomType)
                    continue;

                List<int> validRotations = CollectValidRotations(template, requiredWorldMask);

                if (validRotations.Count == 0)
                    continue;

                if (template.Priority > highestPriority)
                {
                    highestPriority = template.Priority;
                    candidates.Clear();
                }

                if (template.Priority == highestPriority)
                    candidates.Add(new TemplateCandidate(template, validRotations));
            }

            if (candidates.Count == 0)
                throw new InvalidOperationException(
                    $"找不到兼容房间模板：Room={room.RoomId}，Type={room.RoomType}，" +
                    $"Strategy={strategyType}，Mask={requiredWorldMask}");

            //每个房间使用独立随机种子。
            //即使以后调整其他房间的选择顺序，也不会改变当前房间的结果。
            int roomSeed = unchecked(mapSeed * 486187739 + room.RoomId * 16777619 + (int)room.RoomType * 7919);
            System.Random random = new System.Random(roomSeed);

            TemplateCandidate selectedCandidate = SelectWeightedCandidate(candidates, random);
            int rotationIndex = selectedCandidate.ValidRotations[random.Next(selectedCandidate.ValidRotations.Count)];

            return new TemplateSelection(selectedCandidate.Template, rotationIndex);
        }

        private static List<int> CollectValidRotations(RoomTemplateConfig template, ConnectorMask requiredWorldMask)
        {
            List<int> validRotations = new List<int>(4);

            for (int rotationIndex = 0; rotationIndex < 4; rotationIndex++)
            {
                if (!ConnectorMaskUtility.IsRotationAllowed(template.AllowedRotations, rotationIndex))
                    continue;

                ConnectorMask rotatedSupportedMask = ConnectorMaskUtility.RotateClockwise(template.SupportedConnectorMask, rotationIndex);

                if (!ConnectorMaskUtility.ContainsAll(rotatedSupportedMask, requiredWorldMask))
                    continue;

                if (!template.AllowUnusedConnectors && rotatedSupportedMask != requiredWorldMask)
                    continue;

                validRotations.Add(rotationIndex);
            }

            return validRotations;
        }

        /// <summary>
        /// 按照模板给出的权重进行加权随机抽取
        /// </summary>
        /// <param name="candidates"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private static TemplateCandidate SelectWeightedCandidate(List<TemplateCandidate> candidates, System.Random random)
        {
            float totalWeight = 0f;

            for (int i = 0; i < candidates.Count; i++)
                totalWeight += candidates[i].Template.Weight;

            double roll = random.NextDouble() * totalWeight;
            float accumulatedWeight = 0f;

            for (int i = 0; i < candidates.Count; i++)
            {
                accumulatedWeight += candidates[i].Template.Weight;

                if (roll <= accumulatedWeight)
                    return candidates[i];
            }

            return candidates[candidates.Count - 1];
        }
        /// <summary>
        /// 在过滤和挑选的中间阶段使用，代表一个合格模板
        /// </summary>
        private sealed class TemplateCandidate
        {
            public RoomTemplateConfig Template { get; }
            public List<int> ValidRotations { get; }

            public TemplateCandidate(RoomTemplateConfig template, List<int> validRotations)
            {
                Template = template;
                ValidRotations = validRotations;
            }
        }
        /// <summary>
        /// 在SelectTemplate方法的最后输出阶段使用，代表经过权重随机抽取后的最终决定
        /// </summary>
        private readonly struct TemplateSelection
        {
            public RoomTemplateConfig Template { get; }
            public int RotationIndex { get; }

            public TemplateSelection(RoomTemplateConfig template, int rotationIndex)
            {
                Template = template;
                RotationIndex = rotationIndex;
            }
        }
    }
}
