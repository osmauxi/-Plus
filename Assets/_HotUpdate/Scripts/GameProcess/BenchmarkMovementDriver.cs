using ProjectGame.HotFix.Gameplay.Player.Sync;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Benchmarking
{
    /// <summary>在普通本地输入之后、Gameplay 网络 Tick 之前提交固定路线输入。</summary>
    [DefaultExecutionOrder(50)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerSyncController))]
    public sealed class BenchmarkMovementDriver : MonoBehaviour
    {
        private static readonly Vector3[] RouteOffsets =
        {
            new(12f, 0f, 0f),
            new(12f, 0f, 12f),
            new(-12f, 0f, 12f),
            new(-12f, 0f, -12f),
            new(12f, 0f, -12f),
            Vector3.zero,
        };

        private PlayerSyncController _syncController;
        private Vector3 _routeOrigin;
        private Vector3 _lastPosition;
        private int _targetIndex;
        private bool _configured;

        public int SubmittedCommandCount { get; private set; }
        public int WaypointsVisited { get; private set; }
        public float DistanceTravelled { get; private set; }

        public void Configure(int seed)
        {
            _syncController = GetComponent<PlayerSyncController>();
            _routeOrigin = transform.position;
            _lastPosition = transform.position;
            _targetIndex = (int)((uint)seed % (uint)RouteOffsets.Length);
            SubmittedCommandCount = 0;
            WaypointsVisited = 0;
            DistanceTravelled = 0f;
            _configured = true;
        }

        private void Update()
        {
            if (!_configured || _syncController == null || !_syncController.IsSimulationInitialized)
                return;

            Vector3 position = transform.position;
            DistanceTravelled += Vector3.Distance(position, _lastPosition);
            _lastPosition = position;

            Vector3 target = _routeOrigin + RouteOffsets[_targetIndex];
            Vector3 delta = target - position;
            delta.y = 0f;
            if (delta.sqrMagnitude <= 2.25f)
            {
                _targetIndex = (_targetIndex + 1) % RouteOffsets.Length;
                WaypointsVisited++;
                target = _routeOrigin + RouteOffsets[_targetIndex];
                delta = target - position;
                delta.y = 0f;
            }

            Vector3 direction = delta.sqrMagnitude > 0.000001f
                ? delta.normalized
                : Vector3.zero;
            var command = new PlayerInputCommand
            {
                WorldMove = new Vector2(direction.x, direction.z),
                AimDirection = Vector2.zero,
                Buttons = PlayerInputButtons.None,
                ReloadRequestSequence = 0,
            };
            _syncController.SubmitLocalInput(command);
            SubmittedCommandCount++;
        }
    }
}
