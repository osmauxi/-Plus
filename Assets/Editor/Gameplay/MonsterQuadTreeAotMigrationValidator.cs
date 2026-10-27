using System;
using ProjectGame.Core.Spatial;
using ProjectGame.HotFix.Gameplay.Monsters;
using UnityEditor;
using UnityEngine;

namespace ProjectGame.Editor.Gameplay
{
    public static class MonsterQuadTreeAotMigrationValidator
    {
        private const float RoomHalfSize = 50f;

        private readonly struct Query
        {
            public readonly Vector2 Start;
            public readonly Vector2 End;
            public readonly float Radius;

            public Query(Vector2 start, Vector2 end, float radius)
            {
                Start = start;
                End = end;
                Radius = radius;
            }
        }

        [MenuItem("Tools/性能基准/验证四叉树 AOT 一致性")]
        public static void Validate()
        {
            GameObject root = new GameObject("QuadTreeParityRoot");
            try
            {
                ValidateScenario(root.transform, "空房间", Array.Empty<Vector2>(), Array.Empty<byte>());
                ValidateScenario(root.transform, "5012 固定随机", CreateRandomPositions(5012, 12345), null);
                ValidateScenario(root.transform, "稀疏 Slot", CreateRandomPositions(257, 2718), CreateSparseMask(257));
                ValidateScenario(root.transform, "边界与越界", new[]
                {
                    new Vector2(-50f, -50f), new Vector2(50f, 50f),
                    new Vector2(-50f, 50f), new Vector2(50f, -50f),
                    new Vector2(-50.01f, 0f), new Vector2(50.01f, 0f),
                    new Vector2(0f, -50.01f), new Vector2(0f, 50.01f),
                }, null);
                ValidateScenario(root.transform, "分裂阈值 16", CreateRepeatedPositions(16), null);
                ValidateScenario(root.transform, "分裂阈值 17", CreateRepeatedPositions(17), null);
                ValidateScenario(root.transform, "同位置与最大深度", CreateRepeatedPositions(96), null);
                ValidateIncrementalOperations(root.transform);
                Debug.Log("四叉树 AOT 一致性验证通过：Build、Query、节点结构、候选顺序和增量操作均一致。");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void ValidateScenario(
            Transform root,
            string name,
            Vector2[] positions,
            byte[] activeMask)
        {
            activeMask ??= CreateActiveMask(positions.Length);
            MonsterWorld world = CreateWorld(positions, activeMask);
            var reference = new MonsterSpatialIndex(root, RoomHalfSize, Math.Max(64, positions.Length));
            var kernel = new MonsterQuadTreeKernel(Vector2.zero, RoomHalfSize, Math.Max(64, positions.Length));
            kernel.Build(positions, activeMask, positions.Length);

            Query[] queries =
            {
                new(new Vector2(-48f, -48f), new Vector2(48f, 48f), 0.25f),
                new(Vector2.zero, Vector2.zero, 8f),
                new(new Vector2(-60f, 0f), new Vector2(60f, 0f), 1.5f),
                new(new Vector2(75f, 75f), new Vector2(80f, 80f), 0.1f),
            };

            for (int i = 0; i < queries.Length; i++)
                CompareQuery(name, world, reference, kernel, queries[i], positions.Length);

            if (world.AliveCount > 0)
                CompareNodes(name, reference, kernel);
        }

        private static void ValidateIncrementalOperations(Transform root)
        {
            const int capacity = 64;
            Vector2[] positions = new Vector2[capacity];
            byte[] activeMask = new byte[capacity];
            MonsterWorld world = new MonsterWorld(capacity);
            for (int i = 0; i < 20; i++)
            {
                positions[i] = new Vector2(i - 10f, i * 0.5f - 5f);
                activeMask[i] = 1;
                world.Create(0, positions[i], 0f, 1f);
            }

            var reference = new MonsterSpatialIndex(root, RoomHalfSize, capacity);
            var kernel = new MonsterQuadTreeKernel(Vector2.zero, RoomHalfSize, capacity);
            kernel.Build(positions, activeMask, world.SlotCount);
            Query query = new(new Vector2(-30f, -30f), new Vector2(30f, 30f), 2f);
            CompareQuery("增量操作初始", world, reference, kernel, query, capacity);

            Vector2 addedPosition = new(12f, -8f);
            int addedSlot = world.Create(0, addedPosition, 0f, 1f);
            positions[addedSlot] = addedPosition;
            activeMask[addedSlot] = 1;
            reference.Add(world, addedSlot);
            kernel.Add(addedSlot, addedPosition);
            CompareQuery("增量 Add", world, reference, kernel, query, capacity);

            world.Damage(3, 1f);
            activeMask[3] = 0;
            reference.Remove(3);
            kernel.Remove(3);
            CompareQuery("增量 Remove", world, reference, kernel, query, capacity);

            Vector2 offset = new(24f, -13f);
            world.Push(4, offset);
            positions[4] += offset;
            reference.Relocate(world, 4);
            kernel.Relocate(4, positions[4]);
            CompareQuery("增量 Relocate", world, reference, kernel, query, capacity);
            CompareNodes("增量操作", reference, kernel);
        }

        private static void CompareQuery(
            string name,
            MonsterWorld world,
            MonsterSpatialIndex reference,
            MonsterQuadTreeKernel kernel,
            Query query,
            int capacity)
        {
            int[] expected = new int[Math.Max(1, capacity)];
            int[] actual = new int[Math.Max(1, capacity)];
            int expectedCount = reference.QuerySweptCircle(
                world, query.Start, query.End, query.Radius, expected);
            int actualCount = kernel.QuerySweptCircle(
                query.Start, query.End, query.Radius, actual);

            if (expectedCount != actualCount)
                throw new InvalidOperationException(
                    $"{name} 候选数量不一致：HotUpdate={expectedCount}, AOT={actualCount}。");
            for (int i = 0; i < expectedCount; i++)
            {
                if (expected[i] != actual[i])
                    throw new InvalidOperationException(
                        $"{name} 候选顺序不一致：Index={i}, HotUpdate={expected[i]}, AOT={actual[i]}。");
            }

            if (world.AliveCount > 0 &&
                reference.LastQueryVisitedNodeCount != kernel.LastQueryVisitedNodeCount)
                throw new InvalidOperationException(
                    $"{name} 访问节点数不一致：HotUpdate={reference.LastQueryVisitedNodeCount}, " +
                    $"AOT={kernel.LastQueryVisitedNodeCount}。");
        }

        private static void CompareNodes(
            string name,
            MonsterSpatialIndex reference,
            MonsterQuadTreeKernel kernel)
        {
            if (reference.NodeCount != kernel.NodeCount)
                throw new InvalidOperationException(
                    $"{name} 节点数量不一致：HotUpdate={reference.NodeCount}, AOT={kernel.NodeCount}。");

            for (int i = 0; i < reference.NodeCount; i++)
            {
                reference.TryGetDebugNode(i, out MonsterSpatialNodeDebugData expected);
                kernel.TryGetNode(i, out MonsterQuadTreeNodeData actual);
                if ((expected.Center - actual.Center).sqrMagnitude > 0.000001f ||
                    !Mathf.Approximately(expected.Size, actual.Size) ||
                    expected.Depth != actual.Depth ||
                    expected.MonsterCount != actual.MonsterCount ||
                    expected.IsLeaf != actual.IsLeaf ||
                    expected.MortonCode != actual.MortonCode)
                    throw new InvalidOperationException($"{name} 节点 {i} 数据不一致。");
            }
        }

        private static MonsterWorld CreateWorld(Vector2[] positions, byte[] activeMask)
        {
            MonsterWorld world = new MonsterWorld(Math.Max(64, positions.Length));
            for (int i = 0; i < positions.Length; i++)
            {
                int slot = world.Create(0, positions[i], 0f, 1f);
                if (activeMask[slot] == 0)
                    world.Damage(slot, 1f);
            }
            return world;
        }

        private static Vector2[] CreateRandomPositions(int count, int seed)
        {
            var random = new System.Random(seed);
            var positions = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                positions[i] = new Vector2(
                    (float)(random.NextDouble() * 96d - 48d),
                    (float)(random.NextDouble() * 96d - 48d));
            }
            return positions;
        }

        private static Vector2[] CreateRepeatedPositions(int count)
        {
            var positions = new Vector2[count];
            for (int i = 0; i < positions.Length; i++)
                positions[i] = new Vector2(3f, -7f);
            return positions;
        }

        private static byte[] CreateActiveMask(int count)
        {
            var mask = new byte[count];
            for (int i = 0; i < mask.Length; i++)
                mask[i] = 1;
            return mask;
        }

        private static byte[] CreateSparseMask(int count)
        {
            byte[] mask = CreateActiveMask(count);
            for (int i = 1; i < mask.Length; i += 3)
                mask[i] = 0;
            return mask;
        }
    }
}
