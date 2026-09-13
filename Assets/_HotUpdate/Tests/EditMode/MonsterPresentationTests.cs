using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectGame.HotFix.Gameplay.Monsters;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Tests
{
    public sealed class MonsterPresentationTests
    {
        [Test]
        public void PoseQuantization_UsesExpectedLayoutAndBoundedError()
        {
            var quantizer = new MonsterPoseQuantizer(new Vector2(-20, -10), new Vector2(40, 20));
            uint packed = quantizer.Pack(new Vector2(3.25f, -4.75f), 271f);
            Assert.That(packed & 0xFFF, Is.LessThanOrEqualTo(4095));
            Assert.That((packed >> 12) & 0xFFF, Is.LessThanOrEqualTo(4095));
            quantizer.Unpack(packed, out Vector2 position, out float yaw);
            Assert.That(position.x, Is.EqualTo(3.25f).Within(40f / 4095f));
            Assert.That(position.y, Is.EqualTo(-4.75f).Within(20f / 4095f));
            Assert.That(Mathf.DeltaAngle(yaw, 271f), Is.EqualTo(0).Within(360f / 256f));
        }

        [Test]
        public void FullFrame_UsesActiveMask_AndReplicaInterpolatesWithoutViews()
        {
            var quantizer = new MonsterPoseQuantizer(Vector2.zero, new Vector2(100, 100));
            var world = new MonsterWorld();
            world.Create(2, new Vector2(1, 1), 0);
            world.Create(3, new Vector2(2, 2), 90);
            world.Create(4, new Vector2(3, 3), 180);
            world.Kill(1);
            world.Presentation[2] = new MonsterPresentationData { AttackSequence = 5 };

            MonsterPresentationFrame first = MonsterPresentationPackingSystem.Capture(world, 10, quantizer);
            Assert.That(first.ActiveMask.Count, Is.EqualTo(1));
            Assert.That(first.ActiveStates.Count, Is.EqualTo(2));
            Assert.That(first.IsActive(0), Is.True);
            Assert.That(first.IsActive(1), Is.False);
            Assert.That(first.IsActive(2), Is.True);

            var plans = new[]
            {
                new MonsterSpawnPlan(2, new Vector2(1, 1), 0),
                new MonsterSpawnPlan(3, new Vector2(2, 2), 90),
                new MonsterSpawnPlan(4, new Vector2(3, 3), 180),
            };
            var replica = new MonsterReplica();
            replica.BeginRoom(10);
            replica.ApplySpawnBatch(MonsterSpawnBatch.FromPlans(0, plans, quantizer), quantizer);
            Assert.That(replica.Apply(first, quantizer), Is.EqualTo(MonsterFrameApplyResult.Applied));
            Assert.That(replica.TryGetState(1, 1, out MonsterReplicaState dead), Is.True);
            Assert.That(dead.IsActive, Is.False);
            Assert.That(replica.TryGetState(2, 1, out MonsterReplicaState current), Is.True);
            Assert.That(current.AttackSequence, Is.EqualTo(5));

            world.Motion[2].Position = new Vector2(7, 3);
            world.Motion[2].Yaw = 270;
            MonsterPresentationFrame second = MonsterPresentationPackingSystem.Capture(world, 11, quantizer);
            Assert.That(replica.Apply(second, quantizer), Is.EqualTo(MonsterFrameApplyResult.Applied));
            Assert.That(replica.TryGetState(2, 0.5f, out MonsterReplicaState interpolated), Is.True);
            Assert.That(interpolated.Position.x, Is.EqualTo(5).Within(0.03f));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(interpolated.Yaw, 225)), Is.LessThan(1.5f));
            Assert.That(replica.Apply(first, quantizer), Is.EqualTo(MonsterFrameApplyResult.Stale));
        }

        [Test]
        public void Replica_RejectsOldRoomFramesAndFramesAheadOfReliableSpawn()
        {
            var quantizer = new MonsterPoseQuantizer(Vector2.zero, Vector2.one * 10);
            var world = new MonsterWorld();
            world.Create(0, Vector2.one, 0);
            MonsterPresentationFrame frame = MonsterPresentationPackingSystem.Capture(world, 19, quantizer);
            var replica = new MonsterReplica();
            replica.BeginRoom(20);
            Assert.That(replica.Apply(frame, quantizer), Is.EqualTo(MonsterFrameApplyResult.BeforeRoomStart));

            replica.BeginRoom(0);
            Assert.That(replica.Apply(frame, quantizer), Is.EqualTo(MonsterFrameApplyResult.MissingSpawnBatch));
        }

        [Test]
        public void FrameValidation_RejectsMaskStateMismatchAndBitsOutsideSlotCount()
        {
            Assert.Throws<ArgumentException>(() => new MonsterPresentationFrame(1, 1,
                new byte[] { 1 }, Array.Empty<MonsterPackedState>()));
            Assert.Throws<ArgumentException>(() => new MonsterPresentationFrame(1, 1,
                new byte[] { 2 }, Array.Empty<MonsterPackedState>()));
        }

        [Test]
        public void NetworkCodec_RoundTripsSpawnAndPresentation_WithExactLengthValidation()
        {
            var quantizer = new MonsterPoseQuantizer(Vector2.zero, Vector2.one * 10);
            var plans = new[]
            {
                new MonsterSpawnPlan(7, new Vector2(1, 2), 30),
                new MonsterSpawnPlan(8, new Vector2(3, 4), 60),
            };
            MonsterSpawnBatch spawn = MonsterSpawnBatch.FromPlans(12, plans, quantizer);
            var spawnBytes = new byte[MonsterNetworkCodec.GetSpawnByteCount(spawn) + 4];
            int spawnLength = MonsterNetworkCodec.WriteSpawn(spawn, spawnBytes, 2);
            MonsterSpawnBatch spawnCopy = MonsterNetworkCodec.ReadSpawn(spawnBytes, 2, spawnLength);
            Assert.That(spawnCopy.StartSlot, Is.EqualTo(12));
            Assert.That(spawnCopy.Items[1].ConfigIndex, Is.EqualTo(8));
            Assert.That(spawnCopy.Items[1].PackedPose, Is.EqualTo(spawn.Items[1].PackedPose));

            var world = new MonsterWorld();
            world.Create(7, plans[0].Position, plans[0].Yaw);
            world.Create(8, plans[1].Position, plans[1].Yaw);
            world.Kill(0);
            world.Presentation[1] = new MonsterPresentationData { AttackSequence = 255 };
            MonsterPresentationFrame frame = MonsterPresentationPackingSystem.Capture(world, uint.MaxValue, quantizer);
            var frameBytes = new byte[MonsterNetworkCodec.GetPresentationByteCount(frame)];
            int frameLength = MonsterNetworkCodec.WritePresentation(frame, frameBytes);
            MonsterPresentationFrame frameCopy = MonsterNetworkCodec.ReadPresentation(frameBytes, 0, frameLength);
            Assert.That(frameCopy.ServerTick, Is.EqualTo(uint.MaxValue));
            Assert.That(frameCopy.SlotCount, Is.EqualTo(2));
            Assert.That(frameCopy.IsActive(0), Is.False);
            Assert.That(frameCopy.ActiveStates[0].AttackSequence, Is.EqualTo(255));
            Assert.Throws<ArgumentException>(() => MonsterNetworkCodec.ReadPresentation(frameBytes, 0, frameLength - 1));

            Assert.Throws<NotSupportedException>(() => ((IList<byte>)frame.ActiveMask)[0] = 0);
            Assert.Throws<NotSupportedException>(() => ((IList<MonsterSpawnData>)spawn.Items)[0] = default);
        }

        [Test]
        public void NetworkCodec_RoundTripsRoomBegin_WithGroundYAndExactLength()
        {
            var data = new MonsterRoomBeginData(new Vector2(-30, -25), new Vector2(60, 50),
                -0.1f, uint.MaxValue - 2);
            var bytes = new byte[MonsterNetworkCodec.GetRoomBeginByteCount() + 3];
            int length = MonsterNetworkCodec.WriteRoomBegin(data, bytes, 2);
            MonsterRoomBeginData copy = MonsterNetworkCodec.ReadRoomBegin(bytes, 2, length);

            Assert.That(copy.QuantizationOrigin, Is.EqualTo(data.QuantizationOrigin));
            Assert.That(copy.QuantizationSize, Is.EqualTo(data.QuantizationSize));
            Assert.That(copy.GroundY, Is.EqualTo(data.GroundY));
            Assert.That(copy.MinimumAcceptedTick, Is.EqualTo(data.MinimumAcceptedTick));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                MonsterNetworkCodec.ReadRoomBegin(bytes, 2, length - 1));
        }

        [Test]
        public void ViewWorld_CreatesFromReliableSpawn_UpdatesOnce_AndReturnsDeadViews()
        {
            var quantizer = new MonsterPoseQuantizer(Vector2.zero, Vector2.one * 10);
            MonsterRuntimeCatalog runtime = RuntimeCatalog(2);
            var views = new MonsterViewCatalog(new[]
            {
                new MonsterViewConfig(0, "pool-a"),
                new MonsterViewConfig(1, "pool-b"),
            }, runtime);
            var pool = new FakeViewPool();
            using var viewWorld = new MonsterViewWorld(pool, views, quantizer);
            var plans = new[]
            {
                new MonsterSpawnPlan(0, new Vector2(1, 2), 0),
                new MonsterSpawnPlan(1, new Vector2(3, 4), 90),
            };
            MonsterSpawnBatch batch = MonsterSpawnBatch.FromPlans(0, plans, quantizer);
            viewWorld.ApplySpawnBatch(batch);

            Assert.That(pool.RentedPoolIds, Is.EqualTo(new[] { "pool-a", "pool-b" }));
            Assert.That(viewWorld.ActiveViewCount, Is.EqualTo(2));

            var replica = new MonsterReplica();
            replica.BeginRoom(1);
            replica.ApplySpawnBatch(batch, quantizer);
            var authority = new MonsterWorld();
            authority.Create(0, new Vector2(5, 6), 45);
            authority.Create(1, new Vector2(3, 4), 90);
            authority.Presentation[0] = new MonsterPresentationData { AttackSequence = 1 };
            authority.Kill(1);
            MonsterPresentationFrame frame = MonsterPresentationPackingSystem.Capture(authority, 1, quantizer);
            Assert.That(replica.Apply(frame, quantizer), Is.EqualTo(MonsterFrameApplyResult.Applied));

            viewWorld.ApplyReplica(replica, 1);
            Assert.That(pool.Views[0].AttackCount, Is.EqualTo(1));
            Assert.That(pool.Views[0].Position.x, Is.EqualTo(5).Within(0.01f));
            Assert.That(pool.Views[1].ReturnCount, Is.EqualTo(1));
            Assert.That(viewWorld.ActiveViewCount, Is.EqualTo(1));

            viewWorld.ApplyReplica(replica, 1);
            Assert.That(pool.Views[0].AttackCount, Is.EqualTo(1), "同一序列不能重复触发动画");
        }

        [Test]
        public void ViewWorld_RollsBackEntireUnpublishedSpawnWhenPoolFails()
        {
            var quantizer = new MonsterPoseQuantizer(Vector2.zero, Vector2.one * 10);
            MonsterRuntimeCatalog runtime = RuntimeCatalog(2);
            var views = new MonsterViewCatalog(new[]
            {
                new MonsterViewConfig(0, "pool-a"),
                new MonsterViewConfig(1, "pool-b"),
            }, runtime);
            var pool = new FakeViewPool { FailRentIndex = 1 };
            using var viewWorld = new MonsterViewWorld(pool, views, quantizer);
            MonsterSpawnBatch batch = MonsterSpawnBatch.FromPlans(0, new[]
            {
                new MonsterSpawnPlan(0, Vector2.zero, 0),
                new MonsterSpawnPlan(1, Vector2.one, 0),
            }, quantizer);

            Assert.Throws<InvalidOperationException>(() => viewWorld.ApplySpawnBatch(batch));
            Assert.That(viewWorld.SlotCount, Is.EqualTo(0));
            Assert.That(viewWorld.ActiveViewCount, Is.EqualTo(0));
            Assert.That(pool.Views[0].ReturnCount, Is.EqualTo(1));
        }

        private static MonsterRuntimeCatalog RuntimeCatalog(int count)
        {
            var configs = new MonsterRuntimeConfig[count];
            for (int i = 0; i < count; i++)
                configs[i] = new MonsterRuntimeConfig(i, 1,
                    MonsterTargetModule.NearestTarget, MonsterMoveModule.DirectChase,
                    MonsterAttackModule.MeleeAttack, 1, 1, 1, 1, 0, 0);
            return new MonsterRuntimeCatalog(configs);
        }

        private sealed class FakeView : IMonsterViewHandle
        {
            public Vector2 Position;
            public float Yaw;
            public int AttackCount;
            public int ReturnCount;
            public void SetPose(Vector2 position, float yaw) { Position = position; Yaw = yaw; }
            public void PlayAttack() => AttackCount++;
        }

        private sealed class FakeViewPool : IMonsterViewPool
        {
            public readonly List<string> RentedPoolIds = new List<string>();
            public readonly List<FakeView> Views = new List<FakeView>();
            public int FailRentIndex = -1;

            public IMonsterViewHandle Rent(string localPoolId)
            {
                int index = RentedPoolIds.Count;
                RentedPoolIds.Add(localPoolId);
                if (index == FailRentIndex) return null;
                var view = new FakeView();
                Views.Add(view);
                return view;
            }

            public void Return(IMonsterViewHandle view) => ((FakeView)view).ReturnCount++;
        }
    }
}
