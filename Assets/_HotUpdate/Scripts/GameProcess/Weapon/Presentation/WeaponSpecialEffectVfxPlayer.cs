using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Player;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Weapon.Presentation
{
    /// <summary>
    /// 特殊 Effect 的轻量本地表现。全部由代码生成，不依赖旧 VFX Graph、Prefab 或对象池配置。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeaponSpecialEffectVfxPlayer : MonoBehaviour
    {
        private sealed class TimedRing
        {
            public GameObject Root;
            public LineRenderer Line;
            public float StartedAt;
            public float Duration;
            public float StartRadius;
            public float EndRadius;
            public Color Color;
        }

        private sealed class TimedObject
        {
            public GameObject Root;
            public float ExpiresAt;
        }

        private sealed class AreaVisual
        {
            public GameObject Root;
            public LineRenderer Ring;
            public ParticleSystem Particles;
            public float Radius;
            public float ExpiresAt;
        }

        private sealed class CloudVisual
        {
            public GameObject Root;
            public ParticleSystem Particles;
            public float Radius;
            public Vector3 FollowVelocity;
            public bool HasPosition;
        }

        private readonly List<TimedRing> _rings = new(16);
        private readonly List<TimedObject> _timedObjects = new(32);
        private readonly Dictionary<ulong, AreaVisual> _areas = new();
        private readonly Dictionary<ulong, CloudVisual> _clouds = new();
        private readonly List<ulong> _expiredIds = new(8);

        private Material _particleMaterial;
        private Material _lineMaterial;

        public void Play(in WeaponSpecialVfxEvent effect)
        {
            EnsureMaterials();
            switch (effect.Type)
            {
                case WeaponSpecialVfxType.Execution:
                    PlayExecution(effect.Position, effect.Radius, effect.Duration);
                    break;
                case WeaponSpecialVfxType.Shockwave:
                    PlayShockwave(effect.Position, effect.Radius, effect.Duration);
                    break;
                case WeaponSpecialVfxType.RadiationArea:
                    UpsertRadiationArea(effect);
                    break;
                case WeaponSpecialVfxType.KineticBoost:
                    PlayKineticBoost(effect.Position, effect.Radius);
                    break;
                case WeaponSpecialVfxType.StormCloudStart:
                    StartStormCloud(effect.OwnerEntityId, effect.Radius);
                    break;
                case WeaponSpecialVfxType.StormCloudStop:
                    StopStormCloud(effect.OwnerEntityId);
                    break;
                case WeaponSpecialVfxType.LightningArc:
                    PlayLightningArc(effect);
                    break;
            }
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            UpdateRings(now);
            UpdateTimedObjects(now);
            UpdateAreas(now);
            UpdateClouds();
        }

        private void PlayExecution(Vector3 position, float radius, float duration)
        {
            Color red = new(1f, 0.08f, 0.02f, 1f);
            Color gold = new(1f, 0.65f, 0.08f, 1f);
            float visualRadius = Mathf.Max(1.8f, radius);
            CreateRing("ExecutionRing", position + Vector3.up * 0.08f,
                visualRadius, 0.08f, duration, red, 0.18f);
            CreateBurst("ExecutionBurst", position + Vector3.up * 0.8f,
                gold, duration, 0.55f, 6f, 0.24f, 64, visualRadius * 0.45f);
            CreateTimedLine("ExecutionSlashA",
                position + new Vector3(-visualRadius * 0.65f, 0.1f, -0.2f),
                position + new Vector3(visualRadius * 0.65f, 2.2f, 0.2f),
                0.2f, gold, duration);
            CreateTimedLine("ExecutionSlashB",
                position + new Vector3(visualRadius * 0.65f, 0.1f, -0.2f),
                position + new Vector3(-visualRadius * 0.65f, 2.2f, 0.2f),
                0.2f, red, duration);
            CreateTimedLine("ExecutionBeam", position + Vector3.up * 0.05f,
                position + Vector3.up * 3.2f, 0.12f, gold, duration);
        }

        private void PlayShockwave(Vector3 position, float radius, float duration)
        {
            Color color = new(1f, 0.32f, 0.04f, 1f);
            float visualRadius = Mathf.Max(2f, radius);
            CreateRing("ShockwaveRing", position + Vector3.up * 0.05f,
                0.15f, visualRadius, duration, color, 0.2f);
            CreateRing("ShockwaveCore", position + Vector3.up * 0.08f,
                0.1f, visualRadius * 0.72f, duration * 0.7f,
                new Color(1f, 0.85f, 0.25f, 1f), 0.1f);
            CreateBurst("ShockwaveBurst", position + Vector3.up * 0.12f,
                color, duration, 0.45f, Mathf.Max(4f, visualRadius * 2f),
                0.2f, 72, 0.35f);
        }

        private void PlayKineticBoost(Vector3 position, float radius)
        {
            Color color = new(0.1f, 0.8f, 1f, 1f);
            CreateRing("KineticBoostRing", position + Vector3.up * 0.1f,
                radius, radius * 0.2f, 0.35f, color, 0.08f);
            CreateBurst("KineticBoostBurst", position + Vector3.up * 0.5f,
                color, 0.45f, 0.3f, 2.4f, 0.1f, 24, radius);
        }

        private void PlayLightningArc(in WeaponSpecialVfxEvent effect)
        {
            Vector3 start = effect.Position;
            Vector3 end = effect.TargetPosition;
            float duration = effect.Duration;
            if (effect.OwnerEntityId != 0 &&
                _clouds.TryGetValue(effect.OwnerEntityId, out CloudVisual cloud))
                start = cloud.Root.transform.position;
            const int pointCount = 9;
            GameObject root = new("LightningArc");
            LineRenderer line = CreateLine(root, pointCount, 0.075f,
                new Color(0.45f, 0.85f, 1f, 1f), true);
            Vector3 direction = end - start;
            Vector3 side = Vector3.Cross(direction.normalized, Vector3.up);
            if (side.sqrMagnitude < 0.01f) side = Vector3.right;
            Vector3 secondSide = Vector3.Cross(direction.normalized, side).normalized;
            float jitter = Mathf.Min(0.3f, direction.magnitude * 0.08f);
            for (int i = 0; i < pointCount; i++)
            {
                float t = i / (float)(pointCount - 1);
                Vector3 point = Vector3.Lerp(start, end, t);
                if (i > 0 && i < pointCount - 1)
                    point += side * Random.Range(-jitter, jitter) +
                             secondSide * Random.Range(-jitter, jitter);
                line.SetPosition(i, point);
            }
            _timedObjects.Add(new TimedObject
            {
                Root = root,
                ExpiresAt = Time.unscaledTime + Mathf.Max(0.08f, duration),
            });
            CreateBurst("LightningSpark", end + Vector3.up * 0.15f,
                Color.white, 0.2f, 0.15f, 2.2f, 0.07f, 12, 0.18f);
        }

        private void UpsertRadiationArea(in WeaponSpecialVfxEvent effect)
        {
            if (!_areas.TryGetValue(effect.InstanceId, out AreaVisual visual))
            {
                GameObject root = new("RadiationArea");
                root.transform.position = effect.Position;
                LineRenderer ring = CreateLine(root, 49, 0.08f,
                    new Color(0.45f, 1f, 0.08f, 0.8f), false);
                ParticleSystem particles = CreateParticles(
                    "RadiationMotes", root.transform, new Color(0.6f, 1f, 0.12f, 0.75f),
                    true, 1f, 0.9f, 0.12f, 0f, 18f, effect.Radius * 0.85f);
                visual = new AreaVisual { Root = root, Ring = ring, Particles = particles };
                _areas.Add(effect.InstanceId, visual);
            }

            visual.Root.transform.position = effect.Position;
            visual.Radius = effect.Radius;
            visual.ExpiresAt = Time.unscaledTime + effect.Duration;
            ParticleSystem.ShapeModule shape = visual.Particles.shape;
            shape.radius = effect.Radius * 0.85f;
            SetCircle(visual.Ring, effect.Radius);
        }

        private void StartStormCloud(ulong ownerEntityId, float radius)
        {
            if (_clouds.TryGetValue(ownerEntityId, out CloudVisual existing))
            {
                existing.Radius = radius;
                ParticleSystem.ShapeModule existingShape = existing.Particles.shape;
                existingShape.radius = Mathf.Clamp(radius * 0.1f, 0.65f, 1.05f);
                return;
            }

            GameObject root = new("StormCloud");
            ParticleSystem particles = CreateParticles(
                "StormCloudParticles", root.transform, new Color(0.18f, 0.24f, 0.42f, 0.75f),
                true, 1.8f, 1.4f, 0.55f, 0.08f, 22f,
                Mathf.Clamp(radius * 0.1f, 0.65f, 1.05f));
            ParticleSystem.MainModule main = particles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            var visual = new CloudVisual
            {
                Root = root,
                Particles = particles,
                Radius = radius,
            };
            if (TryGetPlayerTransform(ownerEntityId, out Transform player))
            {
                root.transform.position = GetCloudTarget(player);
                visual.HasPosition = true;
            }
            _clouds.Add(ownerEntityId, visual);
        }

        private void StopStormCloud(ulong ownerEntityId)
        {
            if (!_clouds.Remove(ownerEntityId, out CloudVisual visual)) return;
            DestroyObject(visual.Root);
        }

        private void CreateRing(string name, Vector3 position, float startRadius,
            float endRadius, float duration, Color color, float width)
        {
            GameObject root = new(name);
            root.transform.position = position;
            LineRenderer line = CreateLine(root, 49, width, color, false);
            SetCircle(line, startRadius);
            _rings.Add(new TimedRing
            {
                Root = root,
                Line = line,
                StartedAt = Time.unscaledTime,
                Duration = Mathf.Max(0.05f, duration),
                StartRadius = startRadius,
                EndRadius = endRadius,
                Color = color,
            });
        }

        private void CreateBurst(string name, Vector3 position, Color color,
            float duration, float particleLifetime, float speed, float size,
            int count, float radius)
        {
            GameObject root = new(name);
            root.transform.position = position;
            ParticleSystem particles = CreateParticles(name + "Particles", root.transform,
                color, false, duration, particleLifetime, size, speed, 0f, radius);
            particles.Emit(count);
            _timedObjects.Add(new TimedObject
            {
                Root = root,
                ExpiresAt = Time.unscaledTime + duration + particleLifetime,
            });
        }

        private void CreateTimedLine(string name, Vector3 start, Vector3 end,
            float width, Color color, float duration)
        {
            GameObject root = new(name);
            LineRenderer line = CreateLine(root, 2, width, color, true);
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            _timedObjects.Add(new TimedObject
            {
                Root = root,
                ExpiresAt = Time.unscaledTime + duration,
            });
        }

        private ParticleSystem CreateParticles(string name, Transform parent, Color color,
            bool loop, float duration, float lifetime, float size, float speed,
            float rate, float radius)
        {
            GameObject child = new(name);
            child.transform.SetParent(parent, false);
            ParticleSystem particles = child.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = particles.main;
            main.loop = loop;
            main.duration = Mathf.Max(0.1f, duration);
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = loop ? 96 : 64;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = rate;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius;

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = _particleMaterial;
            particles.Play(true);
            return particles;
        }

        private LineRenderer CreateLine(GameObject root, int pointCount,
            float width, Color color, bool worldSpace)
        {
            LineRenderer line = root.AddComponent<LineRenderer>();
            line.sharedMaterial = _lineMaterial;
            line.useWorldSpace = worldSpace;
            line.loop = false;
            line.positionCount = pointCount;
            line.startWidth = width;
            line.endWidth = width * 0.45f;
            line.startColor = color;
            line.endColor = color;
            line.numCornerVertices = 2;
            line.numCapVertices = 2;
            return line;
        }

        private static void SetCircle(LineRenderer line, float radius)
        {
            const int segmentCount = 48;
            line.positionCount = segmentCount + 1;
            for (int i = 0; i <= segmentCount; i++)
            {
                float angle = i * Mathf.PI * 2f / segmentCount;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f,
                    Mathf.Sin(angle) * radius));
            }
        }

        private void UpdateRings(float now)
        {
            for (int i = _rings.Count - 1; i >= 0; i--)
            {
                TimedRing ring = _rings[i];
                float t = Mathf.Clamp01((now - ring.StartedAt) / ring.Duration);
                SetCircle(ring.Line, Mathf.Lerp(ring.StartRadius, ring.EndRadius, t));
                Color color = ring.Color;
                color.a *= 1f - t;
                ring.Line.startColor = color;
                ring.Line.endColor = color;
                if (t < 1f) continue;
                DestroyObject(ring.Root);
                _rings.RemoveAt(i);
            }
        }

        private void UpdateTimedObjects(float now)
        {
            for (int i = _timedObjects.Count - 1; i >= 0; i--)
            {
                if (now < _timedObjects[i].ExpiresAt) continue;
                DestroyObject(_timedObjects[i].Root);
                _timedObjects.RemoveAt(i);
            }
        }

        private void UpdateAreas(float now)
        {
            _expiredIds.Clear();
            foreach (KeyValuePair<ulong, AreaVisual> pair in _areas)
            {
                AreaVisual visual = pair.Value;
                if (now >= visual.ExpiresAt)
                {
                    _expiredIds.Add(pair.Key);
                    continue;
                }
                SetCircle(visual.Ring,
                    visual.Radius * (1f + Mathf.Sin(now * 4f) * 0.025f));
            }
            for (int i = 0; i < _expiredIds.Count; i++)
            {
                ulong id = _expiredIds[i];
                DestroyObject(_areas[id].Root);
                _areas.Remove(id);
            }
        }

        private void UpdateClouds()
        {
            foreach (KeyValuePair<ulong, CloudVisual> pair in _clouds)
            {
                CloudVisual visual = pair.Value;
                if (!TryGetPlayerTransform(pair.Key, out Transform player)) continue;
                Vector3 target = GetCloudTarget(player);
                if (!visual.HasPosition)
                {
                    visual.Root.transform.position = target;
                    visual.HasPosition = true;
                    continue;
                }
                visual.Root.transform.position = Vector3.SmoothDamp(
                    visual.Root.transform.position,
                    target,
                    ref visual.FollowVelocity,
                    0.32f,
                    Mathf.Infinity,
                    Time.unscaledDeltaTime);
            }
        }

        private static Vector3 GetCloudTarget(Transform player) =>
            player.TransformPoint(new Vector3(1.25f, 0.65f, 0.8f));

        private static bool TryGetPlayerTransform(ulong entityId, out Transform playerTransform)
        {
            PlayerManager manager = PlayerManager.Instance;
            if (manager != null)
            {
                IReadOnlyList<PlayerRuntime> players = manager.RuntimePlayers;
                for (int i = 0; i < players.Count; i++)
                {
                    PlayerRuntime player = players[i];
                    if (player == null || player.NetworkObjectId != entityId) continue;
                    playerTransform = player.transform;
                    return true;
                }
            }
            playerTransform = null;
            return false;
        }

        private void EnsureMaterials()
        {
            if (_particleMaterial != null) return;
            Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
                                    Shader.Find("Particles/Standard Unlit") ??
                                    Shader.Find("UI/Default");
            Shader lineShader = Shader.Find("Sprites/Default") ??
                                Shader.Find("Universal Render Pipeline/Unlit") ?? particleShader;
            _particleMaterial = new Material(particleShader)
            {
                name = "WeaponSpecialVfxParticles",
                hideFlags = HideFlags.HideAndDontSave,
            };
            _lineMaterial = new Material(lineShader)
            {
                name = "WeaponSpecialVfxLines",
                hideFlags = HideFlags.HideAndDontSave,
            };
        }

        public void Clear()
        {
            for (int i = 0; i < _rings.Count; i++) DestroyObject(_rings[i].Root);
            for (int i = 0; i < _timedObjects.Count; i++) DestroyObject(_timedObjects[i].Root);
            foreach (AreaVisual visual in _areas.Values) DestroyObject(visual.Root);
            foreach (CloudVisual visual in _clouds.Values) DestroyObject(visual.Root);
            _rings.Clear();
            _timedObjects.Clear();
            _areas.Clear();
            _clouds.Clear();
            _expiredIds.Clear();
            DestroyObject(_particleMaterial);
            DestroyObject(_lineMaterial);
            _particleMaterial = null;
            _lineMaterial = null;
        }

        private static void DestroyObject(Object target)
        {
            if (target == null) return;
            if (Application.isPlaying) Destroy(target);
            else DestroyImmediate(target);
        }

        private void OnDestroy() => Clear();
    }
}
