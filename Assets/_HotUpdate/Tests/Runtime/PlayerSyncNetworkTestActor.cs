using ProjectGame.HotFix.Gameplay.Player.Sync;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Tests.Runtime
{
    /// <summary>为跨进程同步实测提供确定性连续输入，并采样远端表现 </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerSyncController))]
    public sealed class PlayerSyncNetworkTestActor : NetworkBehaviour
    {
        private PlayerSyncController _syncController;
        private Vector3 _lastPresentationPosition;
        private bool _hasPresentationPosition;

        public int PresentationSampleCount { get; private set; }
        public int PresentationMovingSampleCount { get; private set; }
        public int PresentationStallSampleCount { get; private set; }
        public float PresentationStepSum { get; private set; }
        public float PresentationMaxStep { get; private set; }

        public PlayerSyncController SyncController => _syncController;

        private void Awake()
        {
            _syncController = GetComponent<PlayerSyncController>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _lastPresentationPosition = transform.position;
            _hasPresentationPosition = true;
            Debug.Log($"[PlayerSyncNetworkTestActor] Spawn NetworkObjectId={NetworkObjectId}, " +
                $"OwnerClientId={OwnerClientId}, IsServer={IsServer}, IsOwner={IsOwner}");
        }

        public override void OnNetworkDespawn()
        {
            Debug.Log($"[PlayerSyncNetworkTestActor] Despawn NetworkObjectId={NetworkObjectId}, " +
                $"OwnerClientId={OwnerClientId}, IsServer={IsServer}, IsOwner={IsOwner}");
            base.OnNetworkDespawn();
        }

        private void Update()
        {
            if (!IsSpawned)
                return;

            if (IsOwner)
                SubmitDeterministicInput();

            if (!IsOwner && !IsServer)
                SampleRemotePresentation();
        }

        private void SubmitDeterministicInput()
        {
            float cycle = Mathf.Repeat(Time.unscaledTime, 12f);
            PlayerInputCommand input = default;

            if (cycle < 3f)
            {
                input.WorldMove = Vector2.up;
            }
            else if (cycle < 6f)
            {
                input.WorldMove = new Vector2(0.7071068f, 0.7071068f);
                input.Buttons = PlayerInputButtons.SprintHeld;
            }
            else if (cycle < 9f)
            {
                input.WorldMove = Vector2.right;
                input.AimDirection = Vector2.left;
                input.Buttons = PlayerInputButtons.AimHeld;
            }
            else if (cycle < 10.5f)
            {
                input.WorldMove = Vector2.down;
            }

            _syncController.SubmitLocalInput(input);
        }

        private void SampleRemotePresentation()
        {
            Vector3 current = transform.position;
            if (!_hasPresentationPosition)
            {
                _lastPresentationPosition = current;
                _hasPresentationPosition = true;
                return;
            }

            float step = Vector3.Distance(current, _lastPresentationPosition);
            PresentationSampleCount++;
            PresentationStepSum += step;
            PresentationMaxStep = Mathf.Max(PresentationMaxStep, step);

            if (step > 0.0001f)
                PresentationMovingSampleCount++;
            else
                PresentationStallSampleCount++;

            _lastPresentationPosition = current;
        }
    }
}
