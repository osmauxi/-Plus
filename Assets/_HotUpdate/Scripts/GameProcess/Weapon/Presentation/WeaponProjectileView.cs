using ProjectGame.HotFix.Gameplay.Pooling;
using UnityEngine;
using RuntimeLocalObjectPool = ProjectGame.HotFix.Gameplay.Pooling.LocalObjectPool;

namespace ProjectGame.HotFix.Gameplay.Weapon.Presentation
{
    /// <summary>
    /// 虚拟子弹对应的本地纯表现对象。没有碰撞、伤害和网络权威，只负责移动、拖尾和延迟回池。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeaponProjectileView : MonoBehaviour, IPoolable
    {
        [SerializeField] private Renderer[] _bodyRenderers;
        [SerializeField] private TrailRenderer _trail;
        [SerializeField, Min(0f)] private float _minimumVisibleTime = 0.04f;

        private Vector3 _velocity;
        private float _remainingLifeTime;
        private float _spawnTime;
        private float _returnTime;
        private float _baseTrailWidth = 1f;
        private bool _launched;
        private bool _returnPending;

        public ulong ShotId { get; private set; }
        public uint ProjectileId { get; private set; }

        private void Awake()
        {
            if (_trail == null)
                _trail = GetComponentInChildren<TrailRenderer>(true);
            if (_bodyRenderers == null || _bodyRenderers.Length == 0)
            {
                Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
                int bodyCount = 0;
                for (int i = 0; i < renderers.Length; i++)
                    if (renderers[i] != _trail) bodyCount++;

                _bodyRenderers = new Renderer[bodyCount];
                int writeIndex = 0;
                for (int i = 0; i < renderers.Length; i++)
                    if (renderers[i] != _trail) _bodyRenderers[writeIndex++] = renderers[i];
            }

            if (_trail != null)
                _baseTrailWidth = _trail.widthMultiplier;
        }

        public void Launch(in ProjectileSpawn spawn, float fastForwardSeconds)
        {
            ShotId = spawn.Projectile.ShotId;
            ProjectileId = spawn.Projectile.ProjectileId;
            _velocity = spawn.Projectile.Velocity;
            _remainingLifeTime = Mathf.Max(0f, spawn.Projectile.RemainingLifeTime - fastForwardSeconds);
            _spawnTime = Time.time;
            _returnPending = false;
            _launched = true;

            float visualSize = Mathf.Max(0.01f, spawn.VisualSize);
            transform.localScale = new Vector3(visualSize, visualSize, visualSize * 2f);
            transform.position = spawn.Projectile.Position + _velocity * fastForwardSeconds;
            FaceVelocity();

            SetBodyVisible(true);
            if (_trail != null)
            {
                _trail.widthMultiplier = _baseTrailWidth * Mathf.Max(0.25f, visualSize / 0.1f);
                _trail.Clear();
                _trail.emitting = true;
            }
        }

        public void ApplyImpact(in ProjectileImpact impact)
        {
            if (!_launched || impact.Projectile.ProjectileId != ProjectileId ||
                impact.Projectile.ShotId != ShotId)
                return;

            transform.position = impact.Point;
            if (impact.Resolution == ProjectileHitResolution.Continue)
            {
                _velocity = impact.Projectile.Velocity;
                _remainingLifeTime = Mathf.Max(0f, impact.Projectile.RemainingLifeTime);
                FaceVelocity();
                return;
            }

            BeginReturn();
        }

        public void ApplyRemoved(in ProjectileState projectile)
        {
            if (!_launched || projectile.ProjectileId != ProjectileId || projectile.ShotId != ShotId)
                return;

            transform.position = projectile.Position;
            BeginReturn();
        }

        private void Update()
        {
            if (_returnPending)
            {
                if (Time.time >= _returnTime)
                    ReturnToPool();
                return;
            }

            if (!_launched)
                return;

            float deltaTime = Time.deltaTime;
            transform.position += _velocity * deltaTime;
            _remainingLifeTime -= deltaTime;
            if (_remainingLifeTime <= 0f)
                BeginReturn();
        }

        private void BeginReturn()
        {
            if (_returnPending)
                return;

            _launched = false;
            _returnPending = true;
            SetBodyVisible(false);
            if (_trail != null)
                _trail.emitting = false;

            float visibleDelay = Mathf.Max(0f, _spawnTime + _minimumVisibleTime - Time.time);
            float trailDelay = _trail != null ? Mathf.Max(0f, _trail.time) : 0f;
            _returnTime = Time.time + Mathf.Max(visibleDelay, trailDelay);
        }

        private void ReturnToPool()
        {
            _returnPending = false;
            if (RuntimeLocalObjectPool.Instance != null)
                RuntimeLocalObjectPool.Instance.Return(gameObject);
            else
                Destroy(gameObject);
        }

        private void FaceVelocity()
        {
            if (_velocity.sqrMagnitude > 0.000001f)
                transform.rotation = Quaternion.LookRotation(_velocity.normalized, Vector3.up);
        }

        private void SetBodyVisible(bool visible)
        {
            if (_bodyRenderers == null)
                return;
            for (int i = 0; i < _bodyRenderers.Length; i++)
                if (_bodyRenderers[i] != null) _bodyRenderers[i].enabled = visible;
        }

        public void OnRentFromPool()
        {
            ShotId = 0;
            ProjectileId = 0;
            _velocity = Vector3.zero;
            _remainingLifeTime = 0f;
            _launched = false;
            _returnPending = false;
            SetBodyVisible(true);
            if (_trail != null)
            {
                _trail.widthMultiplier = _baseTrailWidth;
                _trail.emitting = false;
                _trail.Clear();
            }
        }

        public void OnReturnToPool()
        {
            _launched = false;
            _returnPending = false;
            SetBodyVisible(false);
            if (_trail != null)
            {
                _trail.emitting = false;
                _trail.Clear();
            }
        }
    }
}
