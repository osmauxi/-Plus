using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Gameplay.Network;
using ProjectGame.HotFix.Gameplay.Runtime;
using Unity.Profiling;
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
        private static readonly ProfilerMarker ApplyAuthorityMarker =
            new("Monster.View.ApplyAuthority");
        private static readonly ProfilerMarker ApplyClientMarker =
            new("Monster.View.ApplyClient");

        [SerializeField] private MonsterVATRenderProfileCatalog _profileCatalog;

        private MonsterRuntimeService _runtime;
        private MonsterViewWorld _viewWorld;
        private MonsterVATRenderWorld _vatWorld;

        public bool IsInitialized { get; private set; }
        public int ViewSlotCount => _viewWorld?.ViewSlotCount ?? 0;
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
            RenderPipelineManager.beginContextRendering += HandleBeginContextRendering;
            enabled = true;
        }

        private void OnRoomEnded() => DisposeViewWorld();

        private void OnSpawnBatchReceived(MonsterSpawnBatch batch)
        {
            _viewWorld.ApplySpawnBatch(batch);
            _viewWorld.SyncReplica(_runtime.Replica, Time.unscaledTime);
        }

        private void OnPresentationFrameApplied(
            MonsterPresentationFrame frame,
            MonsterFrameApplyResult result)
        {
            if (result != MonsterFrameApplyResult.Applied)
                return;

            if (GameNetworkRuntime.Gameplay.Transport.IsServer)
            {
                using var applyScope = ApplyAuthorityMarker.Auto();
                _viewWorld.SyncReplica(_runtime.Replica, Time.unscaledTime);
            }
        }

        private void LateUpdate()
        {
            if (GameNetworkRuntime.Gameplay.Transport.IsServer)
            {
                _viewWorld.Update(
                    _runtime.Replica,
                    Time.deltaTime,
                    Time.unscaledDeltaTime,
                    Time.unscaledTime);
                return;
            }

            using (ApplyClientMarker.Auto())
            {
                _viewWorld.Update(
                    _runtime.Replica,
                    Time.deltaTime,
                    Time.unscaledDeltaTime,
                    Time.unscaledTime);
            }
        }

        private void HandleBeginContextRendering(ScriptableRenderContext context,List<Camera> cameras)
            => _viewWorld.Render();

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
