using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Gameplay.Network;
using ProjectGame.HotFix.Gameplay.Runtime;
using UnityEngine;
using RuntimeLocalObjectPool = ProjectGame.HotFix.Gameplay.Pooling.LocalObjectPool;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 连接 Monster Replica 与本地对象池的 GameRoot 表现服务
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MonsterViewRuntimeService : MonoBehaviour, IGameRuntimeService, IMonsterViewPool
    {
        private MonsterRuntimeService _runtime;
        private RuntimeLocalObjectPool _objectPool;
        private MonsterViewWorld _viewWorld;
        private float _lastFrameTime;
        private float _frameInterval;

        public bool IsInitialized { get; private set; }

        private void Awake() => enabled = false;

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return;

            _runtime = MonsterRuntimeService.Instance;
            _objectPool = RuntimeLocalObjectPool.Instance;
            if (_runtime == null || !_runtime.IsInitialized || _objectPool == null || !_objectPool.IsInitialized)
                throw new InvalidOperationException(
                    "MonsterViewRuntimeService 必须在 MonsterRuntimeService 与 LocalObjectPool 后初始化。");

            await _objectPool.PreparePoolsAsync(_runtime.Catalogs.Views.LocalPoolIds, cancellationToken);

            _runtime.RoomBegan += OnRoomBegan;
            _runtime.RoomEnded += OnRoomEnded;
            _runtime.SpawnBatchReceived += OnSpawnBatchReceived;
            _runtime.PresentationFrameApplied += OnPresentationFrameApplied;
            _runtime.AuthorityTickCompleted += OnAuthorityTickCompleted;
            enabled = false;
            IsInitialized = true;
        }

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            if (!IsInitialized)
                return UniTask.CompletedTask;

            _runtime.RoomBegan -= OnRoomBegan;
            _runtime.RoomEnded -= OnRoomEnded;
            _runtime.SpawnBatchReceived -= OnSpawnBatchReceived;
            _runtime.PresentationFrameApplied -= OnPresentationFrameApplied;
            _runtime.AuthorityTickCompleted -= OnAuthorityTickCompleted;
            DisposeViewWorld();
            _runtime = null;
            _objectPool = null;
            IsInitialized = false;
            return UniTask.CompletedTask;
        }

        private void OnRoomBegan(MonsterRoomBeginData data)
        {
            DisposeViewWorld();
            _viewWorld = new MonsterViewWorld(
                this,
                _runtime.Catalogs.Views,
                new MonsterPoseQuantizer(data.QuantizationOrigin, data.QuantizationSize));
            _frameInterval = GameNetworkRuntime.Gameplay.Clock.TickDeltaTime * _runtime.PresentationIntervalTicks;
            _lastFrameTime = Time.unscaledTime;
            enabled = true;
        }

        private void OnRoomEnded() => DisposeViewWorld();

        private void OnSpawnBatchReceived(MonsterSpawnBatch batch)
        {
            _viewWorld.ApplySpawnBatch(batch);
            if (GameNetworkRuntime.Gameplay.Transport.IsServer)
                _viewWorld.ApplyAuthority(_runtime.AuthorityRoom.World);
            else
                _viewWorld.ApplyReplica(_runtime.Replica, 1f);
        }

        private void OnAuthorityTickCompleted() =>
            _viewWorld.ApplyAuthority(_runtime.AuthorityRoom.World);

        private void OnPresentationFrameApplied(
            MonsterPresentationFrame frame, MonsterFrameApplyResult result)
        {
            if (result == MonsterFrameApplyResult.Applied)
                _lastFrameTime = Time.unscaledTime;
        }

        private void LateUpdate()
        {
            if (GameNetworkRuntime.Gameplay.Transport.IsServer)
                return;
            float alpha = Mathf.Clamp01((Time.unscaledTime - _lastFrameTime) / _frameInterval);
            _viewWorld.ApplyReplica(_runtime.Replica, alpha);
        }

        public IMonsterViewHandle Rent(string localPoolId)
        {
            GameObject instance = _objectPool.Rent(
                localPoolId,
                Vector3.up * _runtime.RoomGroundY,
                Quaternion.identity,
                transform);
            return instance.GetComponent<MonsterViewHandle>();
        }

        public void Return(IMonsterViewHandle view) => _objectPool.Return(view.Instance);

        private void DisposeViewWorld()
        {
            enabled = false;
            ReleaseViewWorld();
        }

        private void OnDisable() => ReleaseViewWorld();

        private void ReleaseViewWorld()
        {
            MonsterViewWorld viewWorld = _viewWorld;
            _viewWorld = null;
            viewWorld?.Dispose();
        }

        private void OnDestroy() => ShutdownAsync(CancellationToken.None).Forget();
    }
}
