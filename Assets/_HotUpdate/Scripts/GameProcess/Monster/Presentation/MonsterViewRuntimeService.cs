using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Gameplay.Network;
using ProjectGame.HotFix.Gameplay.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 连接 Monster Replica 与 VAT RenderWorld 的 GameRoot 表现服务。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MonsterViewRuntimeService : MonoBehaviour, IGameRuntimeService
    {
        [SerializeField] private MonsterVATRenderProfileCatalog _profileCatalog;

        private MonsterRuntimeService _runtime;
        private MonsterViewWorld _viewWorld;
        private MonsterVATRenderWorld _vatWorld;
        private float _lastFrameTime;
        private float _frameInterval;

        public bool IsInitialized { get; private set; }
        public int RenderSlotCount => _viewWorld?.SlotCount ?? 0;
        public int ActiveRenderCount => _viewWorld?.ActiveRenderCount ?? 0;

        private void Awake() => enabled = false;

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return UniTask.CompletedTask;

            _runtime = MonsterRuntimeService.Instance;
            if (_runtime == null || !_runtime.IsInitialized || _profileCatalog == null)
                throw new InvalidOperationException(
                    "MonsterViewRuntimeService 必须在 MonsterRuntimeService 后初始化，并配置 Monster VAT Profile Catalog。");

            _runtime.RoomBegan += OnRoomBegan;
            _runtime.RoomEnded += OnRoomEnded;
            _runtime.SpawnBatchReceived += OnSpawnBatchReceived;
            _runtime.PresentationFrameApplied += OnPresentationFrameApplied;
            enabled = false;
            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            if (!IsInitialized)
                return UniTask.CompletedTask;

            _runtime.RoomBegan -= OnRoomBegan;
            _runtime.RoomEnded -= OnRoomEnded;
            _runtime.SpawnBatchReceived -= OnSpawnBatchReceived;
            _runtime.PresentationFrameApplied -= OnPresentationFrameApplied;
            DisposeViewWorld();
            _runtime = null;
            IsInitialized = false;
            return UniTask.CompletedTask;
        }

        private void OnRoomBegan(MonsterRoomBeginData data)
        {
            DisposeViewWorld();
            _vatWorld = new MonsterVATRenderWorld(data.GroundY);
            _viewWorld = new MonsterViewWorld(
                _runtime.Catalogs.Views,
                _profileCatalog,
                new MonsterPoseQuantizer(data.QuantizationOrigin, data.QuantizationSize),
                _vatWorld);
            _frameInterval =
                GameNetworkRuntime.Gameplay.Clock.TickDeltaTime
                * _runtime.PresentationIntervalTicks;
            _lastFrameTime = Time.unscaledTime;
            RenderPipelineManager.beginContextRendering += HandleBeginContextRendering;
            enabled = true;
        }

        private void OnRoomEnded() => DisposeViewWorld();

        private void OnSpawnBatchReceived(MonsterSpawnBatch batch)
        {
            _viewWorld.ApplySpawnBatch(batch);
            _viewWorld.ApplyReplica(_runtime.Replica, 1f);
        }

        private void OnPresentationFrameApplied(
            MonsterPresentationFrame frame,
            MonsterFrameApplyResult result)
        {
            if (result != MonsterFrameApplyResult.Applied)
                return;

            _lastFrameTime = Time.unscaledTime;
            if (GameNetworkRuntime.Gameplay.Transport.IsServer)
                _viewWorld.ApplyReplica(_runtime.Replica, 1f);
        }

        private void LateUpdate()
        {
            if (!GameNetworkRuntime.Gameplay.Transport.IsServer)
            {
                float alpha = Mathf.Clamp01(
                    (Time.unscaledTime - _lastFrameTime) / _frameInterval);
                _viewWorld.ApplyReplica(_runtime.Replica, alpha);
            }

            _viewWorld.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void HandleBeginContextRendering(
            ScriptableRenderContext context,
            List<Camera> cameras) => _viewWorld.Render();

        private void DisposeViewWorld()
        {
            enabled = false;
            ReleaseViewWorld();
        }

        private void OnDisable() => ReleaseViewWorld();

        private void ReleaseViewWorld()
        {
            RenderPipelineManager.beginContextRendering -= HandleBeginContextRendering;
            MonsterViewWorld viewWorld = _viewWorld;
            _viewWorld = null;
            viewWorld?.Dispose();
            _vatWorld?.Dispose();
            _vatWorld = null;
        }

        private void OnDestroy() => ShutdownAsync(CancellationToken.None).Forget();
    }
}
