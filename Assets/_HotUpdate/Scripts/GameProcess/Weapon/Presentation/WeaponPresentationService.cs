using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Gameplay.Network;
using ProjectGame.HotFix.Gameplay.Player;
using ProjectGame.HotFix.Gameplay.Pooling;
using ProjectGame.HotFix.Gameplay.Runtime;
using UnityEngine;
using RuntimeLocalObjectPool = ProjectGame.HotFix.Gameplay.Pooling.LocalObjectPool;

namespace ProjectGame.HotFix.Gameplay.Weapon.Presentation
{
    /// <summary>
    /// GameRoot 范围的 Weapon 表现桥接服务。
    /// Server 收集权威 Weapon 事件并广播；Client 只生成本地表现对象和 VFX。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeaponPresentationService : MonoBehaviour,
        IGameRuntimeService, IWeaponPresentationReceiver
    {
        [SerializeField] private string _projectilePoolId = "Bullet";
        [SerializeField] private string _worldImpactVfxPoolId = "VFX_OnHit";
        [SerializeField] private string _targetImpactVfxPoolId = "HitBlood";
        [SerializeField, Min(0.1f)] private float _tombstoneLifetime = 1f;
        [SerializeField, Range(0, 8)] private int _maximumFastForwardTicks = 4;

        private readonly List<ShotContext> _pendingShots = new(16);
        private readonly List<ProjectileSpawn> _pendingSpawns = new(64);
        private readonly List<ProjectileImpact> _pendingImpacts = new(64);
        private readonly List<ProjectileState> _pendingRemoved = new(64);
        private readonly List<WeaponSpecialVfxEvent> _pendingSpecialEffects = new(32);
        private readonly Dictionary<ProjectileKey, WeaponProjectileView> _projectileViews = new();
        private readonly Dictionary<ProjectileKey, float> _tombstones = new();
        private readonly List<ProjectileKey> _expiredTombstones = new(32);
        private readonly Dictionary<ulong, PlayerWeaponPresentationController> _weaponViews = new();

        private GameplayNetworkRuntime _network;
        private WeaponRuntimeService _weaponRuntime;
        private WeaponPresentationTransport _transport;
        private RuntimeLocalObjectPool _objectPool;
        private LocalVFXPool _vfxPool;
        private WeaponSpecialEffectVfxPlayer _specialVfx;

        public static WeaponPresentationService Instance { get; private set; }
        public bool IsInitialized { get; private set; }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return;
            if (Instance != null && Instance != this)
                throw new InvalidOperationException("场景中存在重复 WeaponPresentationService。");

            _network = GameNetworkRuntime.Gameplay;
            _weaponRuntime = WeaponRuntimeService.Instance;
            _objectPool = RuntimeLocalObjectPool.Instance;
            _vfxPool = LocalVFXPool.Instance;
            _specialVfx = GetComponent<WeaponSpecialEffectVfxPlayer>();
            if (_specialVfx == null)
                _specialVfx = gameObject.AddComponent<WeaponSpecialEffectVfxPlayer>();

            if (_network == null || !_network.IsInitialized ||
                _weaponRuntime == null || !_weaponRuntime.IsInitialized)
                throw new InvalidOperationException(
                    "WeaponPresentationService 必须在网络与 WeaponRuntimeService 后初始化。");
            if (_objectPool == null || !_objectPool.IsInitialized ||
                _vfxPool == null || !_vfxPool.IsInitialized)
                throw new InvalidOperationException(
                    "WeaponPresentationService 必须在本地对象池和 VFX 池后初始化。");

            await UniTask.WhenAll(
                _objectPool.PreparePoolAsync(_projectilePoolId, cancellationToken),
                _vfxPool.PreparePoolAsync(_worldImpactVfxPoolId, cancellationToken),
                _vfxPool.PreparePoolAsync(_targetImpactVfxPoolId, cancellationToken));

            _transport = new WeaponPresentationTransport(_network.Transport);
            _transport.Initialize(this);

            if (_network.Transport.IsServer)
            {
                _weaponRuntime.ShotFired += QueueShot;
                _weaponRuntime.ProjectileSpawned += QueueSpawn;
                _weaponRuntime.ProjectileImpact += QueueImpact;
                _weaponRuntime.ProjectileRemoved += QueueRemoved;
                _weaponRuntime.SpecialEffectVfx += QueueSpecialEffectVfx;
                // 本服务在 WeaponRuntimeService 之后初始化，所以调用顺序是先模拟、后发送。
                _network.Clock.TickCompleted += FlushServerFrame;
            }

            Instance = this;
            IsInitialized = true;
        }

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            if (!IsInitialized)
                return UniTask.CompletedTask;

            if (_network != null && _network.Transport.IsServer)
            {
                _network.Clock.TickCompleted -= FlushServerFrame;
                if (_weaponRuntime != null)
                {
                    _weaponRuntime.ShotFired -= QueueShot;
                    _weaponRuntime.ProjectileSpawned -= QueueSpawn;
                    _weaponRuntime.ProjectileImpact -= QueueImpact;
                    _weaponRuntime.ProjectileRemoved -= QueueRemoved;
                    _weaponRuntime.SpecialEffectVfx -= QueueSpecialEffectVfx;
                }
            }

            _transport?.Shutdown();
            _transport = null;

            foreach (WeaponProjectileView view in _projectileViews.Values)
                if (view != null && _objectPool != null) _objectPool.Return(view.gameObject);

            _projectileViews.Clear();
            _tombstones.Clear();
            _expiredTombstones.Clear();
            _weaponViews.Clear();
            _specialVfx?.Clear();
            ClearPendingEvents();
            _network = null;
            _weaponRuntime = null;
            _objectPool = null;
            _vfxPool = null;
            _specialVfx = null;
            IsInitialized = false;
            if (Instance == this) Instance = null;
            return UniTask.CompletedTask;
        }

        public void RegisterWeaponView(ulong entityId, PlayerWeaponPresentationController controller)
        {
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));
            _weaponViews[entityId] = controller;
        }

        public void UnregisterWeaponView(ulong entityId, PlayerWeaponPresentationController controller)
        {
            if (_weaponViews.TryGetValue(entityId, out PlayerWeaponPresentationController current) &&
                current == controller)
                _weaponViews.Remove(entityId);
        }

        private void QueueShot(ShotContext shot) => _pendingShots.Add(shot);
        private void QueueSpawn(ProjectileSpawn spawn) => _pendingSpawns.Add(spawn);
        private void QueueImpact(ProjectileImpact impact) => _pendingImpacts.Add(impact);
        private void QueueRemoved(ProjectileState projectile) => _pendingRemoved.Add(projectile);
        private void QueueSpecialEffectVfx(WeaponSpecialVfxEvent effect) =>
            _pendingSpecialEffects.Add(effect);

        private void FlushServerFrame(uint tick)
        {
            if (!IsInitialized ||
                (_pendingShots.Count == 0 && _pendingSpawns.Count == 0 &&
                 _pendingImpacts.Count == 0 && _pendingRemoved.Count == 0 &&
                 _pendingSpecialEffects.Count == 0))
                return;

            try
            {
                // Host 本机直接消费一次，不经过回环网络消息。
                if (_network.Transport.IsClient)
                    ApplyFrameLocally();

                foreach (ulong clientId in _network.RemoteClientIds)
                {
                    _transport.SendFrame(clientId, tick,
                        _pendingShots, _pendingSpawns, _pendingImpacts, _pendingRemoved,
                        _pendingSpecialEffects);
                }
            }
            finally
            {
                ClearPendingEvents();
            }
        }

        private void ApplyFrameLocally()
        {
            for (int i = 0; i < _pendingShots.Count; i++) ReceiveShot(_pendingShots[i]);
            for (int i = 0; i < _pendingSpawns.Count; i++) ReceiveProjectileSpawn(_pendingSpawns[i]);
            for (int i = 0; i < _pendingImpacts.Count; i++) ReceiveProjectileImpact(_pendingImpacts[i]);
            for (int i = 0; i < _pendingRemoved.Count; i++) ReceiveProjectileRemoved(_pendingRemoved[i]);
            for (int i = 0; i < _pendingSpecialEffects.Count; i++)
                ReceiveSpecialEffectVfx(_pendingSpecialEffects[i]);
        }

        private void ClearPendingEvents()
        {
            _pendingShots.Clear();
            _pendingSpawns.Clear();
            _pendingImpacts.Clear();
            _pendingRemoved.Clear();
            _pendingSpecialEffects.Clear();
        }

        public void ReceiveShot(in ShotContext shot)
        {
            if (_weaponViews.TryGetValue(
                    shot.OwnerEntityId, out PlayerWeaponPresentationController controller))
                controller.PlayAuthoritativeShot(shot.ShotSequence);
        }

        public void ReceiveProjectileSpawn(in ProjectileSpawn spawn)
        {
            ProjectileKey key = new(spawn.Projectile.ShotId, spawn.Projectile.ProjectileId);
            if (_projectileViews.ContainsKey(key) || IsTombstoned(key))
                return;

            Quaternion rotation = spawn.Projectile.Velocity.sqrMagnitude > 0.000001f
                ? Quaternion.LookRotation(spawn.Projectile.Velocity.normalized, Vector3.up)
                : Quaternion.identity;
            GameObject instance = _objectPool.Rent(
                _projectilePoolId, spawn.Projectile.Position, rotation);
            if (!instance.TryGetComponent(out WeaponProjectileView view))
            {
                Debug.LogError(
                    $"Weapon 表现子弹缺少 {nameof(WeaponProjectileView)}：{instance.name}");
                _objectPool.Return(instance);
                return;
            }

            view.Launch(
                spawn,
                CalculateFastForward(spawn.SpawnTick, spawn.Projectile.RemainingLifeTime));
            _projectileViews.Add(key, view);
        }

        public void ReceiveProjectileImpact(in ProjectileImpact impact)
        {
            ProjectileKey key = new(impact.Projectile.ShotId, impact.Projectile.ProjectileId);
            if (_projectileViews.TryGetValue(key, out WeaponProjectileView view))
            {
                view.ApplyImpact(impact);
                if (impact.Resolution == ProjectileHitResolution.Destroy)
                {
                    _projectileViews.Remove(key);
                    _tombstones[key] = Time.unscaledTime + _tombstoneLifetime;
                }
            }
            else if (impact.Resolution == ProjectileHitResolution.Destroy)
            {
                _tombstones[key] = Time.unscaledTime + _tombstoneLifetime;
            }

            string poolId = impact.HasTarget ? _targetImpactVfxPoolId : _worldImpactVfxPoolId;
            Quaternion rotation = impact.Normal.sqrMagnitude > 0.000001f
                ? Quaternion.LookRotation(impact.Normal.normalized, Vector3.up)
                : Quaternion.identity;
            _vfxPool.Play(
                poolId,
                impact.Point,
                rotation,
                impact.HasTarget ? impact.VfxWeight : impact.Projectile.SizeMultiplier);
        }

        public void ReceiveProjectileRemoved(in ProjectileState projectile)
        {
            ProjectileKey key = new(projectile.ShotId, projectile.ProjectileId);
            if (_projectileViews.Remove(key, out WeaponProjectileView view))
                view.ApplyRemoved(projectile);
            _tombstones[key] = Time.unscaledTime + _tombstoneLifetime;
        }

        public void ReceiveSpecialEffectVfx(in WeaponSpecialVfxEvent effect) =>
            _specialVfx.Play(effect);

        private float CalculateFastForward(uint fireTick, float remainingLifeTime)
        {
            if (_network == null || _maximumFastForwardTicks <= 0)
                return 0f;

            uint currentTick = _network.Transport.IsServer
                ? _network.Clock.CurrentTick
                : _network.Clock.EstimatedServerTick;
            uint elapsedTicks = unchecked(currentTick - fireTick);
            if (elapsedTicks > (uint)_maximumFastForwardTicks)
                elapsedTicks = (uint)_maximumFastForwardTicks;
            return Mathf.Min(remainingLifeTime, elapsedTicks * _network.Clock.TickDeltaTime);
        }

        private bool IsTombstoned(in ProjectileKey key)
        {
            if (!_tombstones.TryGetValue(key, out float expiresAt))
                return false;
            if (Time.unscaledTime <= expiresAt)
                return true;
            _tombstones.Remove(key);
            return false;
        }

        private void Update()
        {
            if (_tombstones.Count == 0)
                return;

            float now = Time.unscaledTime;
            _expiredTombstones.Clear();
            foreach (KeyValuePair<ProjectileKey, float> pair in _tombstones)
                if (now > pair.Value) _expiredTombstones.Add(pair.Key);
            for (int i = 0; i < _expiredTombstones.Count; i++)
                _tombstones.Remove(_expiredTombstones[i]);
        }

        private void OnDestroy()
        {
            ShutdownAsync(CancellationToken.None).Forget();
        }

        private readonly struct ProjectileKey : IEquatable<ProjectileKey>
        {
            private readonly ulong _shotId;
            private readonly uint _projectileId;

            public ProjectileKey(ulong shotId, uint projectileId)
            {
                _shotId = shotId;
                _projectileId = projectileId;
            }

            public bool Equals(ProjectileKey other) =>
                _shotId == other._shotId && _projectileId == other._projectileId;
            public override bool Equals(object obj) => obj is ProjectileKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(_shotId, _projectileId);
        }
    }
}
