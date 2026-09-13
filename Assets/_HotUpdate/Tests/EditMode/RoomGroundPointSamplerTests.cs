using System.Collections.Generic;
using NUnit.Framework;
using ProjectGame.HotFix.Gameplay.Spawning;
using UnityEngine;

namespace ProjectGame.HotFix.Tests.EditMode
{
    public sealed class RoomGroundPointSamplerTests
    {
        [Test]
        public void Sampling_AcceptsGroundOnlyWhenItIsTheNearestHit()
        {
            int groundLayer = LayerMask.NameToLayer("Ground");
            int wallLayer = LayerMask.NameToLayer("Wall");
            var room = new GameObject("room-sampling-test");
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                ground.name = "ground";
                ground.layer = groundLayer;
                ground.transform.SetPositionAndRotation(new Vector3(0, -0.5f, 0), Quaternion.identity);
                ground.transform.localScale = new Vector3(20, 1, 20);
                blocker.name = "non-ground-blocker";
                blocker.layer = wallLayer;
                blocker.transform.SetPositionAndRotation(new Vector3(0, 2, 0), Quaternion.identity);
                blocker.transform.localScale = new Vector3(20, 1, 20);
                Physics.SyncTransforms();

                var region = new RoomSpawnRegion(room.transform, new Vector3(0, 3, 0),
                    new Vector3(8, 10, 8), (1 << groundLayer) | (1 << wallLayer), 1 << groundLayer);
                var sampler = new RoomGroundPointSampler();
                var results = new List<Vector3>();
                Assert.That(sampler.TrySampleMany(region, 1, 1f, 0f,
                    new System.Random(7), results, 8), Is.False);
                Assert.That(results, Is.Empty);

                blocker.GetComponent<Collider>().enabled = false;
                Physics.SyncTransforms();
                Assert.That(sampler.TrySampleMany(region, 3, 1f, 0f,
                    new System.Random(7), results, 8), Is.True);
                Assert.That(results, Has.Count.EqualTo(3));
                for (int i = 0; i < results.Count; i++)
                    Assert.That(results[i].y, Is.EqualTo(0f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(blocker);
                Object.DestroyImmediate(ground);
                Object.DestroyImmediate(room);
            }
        }
    }
}
