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
            ReleaseViewWorld();
            _runtime = null;
            IsInitialized = false;
            return UniTask.CompletedTask;
        }

        private void OnRoomBegan(MonsterRoomBeginData data)
        {
            ReleaseViewWorld();
            _vatWorld = new MonsterVATRenderWorld(data.GroundY);
            _viewWorld = new MonsterViewWorld(
                _runtime.Catalogs.Views,
                _profileCatalog,
                new MonsterPoseQuantizer(data.QuantizationOrigin, data.QuantizationSize),
                _vatWorld);
            RenderPipelineManager.beginContextRendering += HandleBeginContextRendering;
            enabled = true;
        }

        private void OnRoomEnded() => ReleaseViewWorld();

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

        private void OnDisable() => ReleaseViewWorld();

        private void ReleaseViewWorld()
        {
            MonsterViewWorld viewWorld = _viewWorld;
            MonsterVATRenderWorld vatWorld = _vatWorld;
            _viewWorld = null;
            _vatWorld = null;
            enabled = false;
            if (vatWorld == null)
                return;
            RenderPipelineManager.beginContextRendering -= HandleBeginContextRendering;
            viewWorld?.Dispose();
            vatWorld.Dispose();
        }

        private void OnDestroy() => ShutdownAsync(CancellationToken.None).Forget();
    }
}
