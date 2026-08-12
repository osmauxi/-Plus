using ProjectGame.HotFix.Gameplay.Input;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Movement
{
    /// <summary>
    /// 本地玩家输入适配器。
    ///
    /// InputManager + Camera
    /// ↓
    /// PlayerLocomotionInput
    /// ↓
    /// PlayerLocomotionController
    ///
    /// 以后网络预测接入时，可以把最终 LocomotionInput 写入 InputPayload，
    /// 而服务器继续调用同一个 PlayerLocomotionController.Simulate。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerLocomotionController))]
    public sealed class LocalPlayerLocomotionDriver : NetworkBehaviour
    {
        [Header("世界空间输入相机")]
        [Tooltip("用于把移动输入和鼠标指针转换为世界空间方向。可由 PlayerCameraController 注入；未指定时自动使用带 MainCamera 标签的相机。")]
        [InspectorName("世界相机")]
        [SerializeField] private Camera _worldCamera;

        private PlayerLocomotionController _locomotionController;

        private bool _canDrive;

        private void Awake()
        {
            _locomotionController = GetComponent<PlayerLocomotionController>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _canDrive = IsOwner;

            if (_canDrive && _worldCamera == null)
                _worldCamera = Camera.main;
        }

        public override void OnNetworkDespawn()
        {
            _canDrive = false;
            base.OnNetworkDespawn();
        }

        private void Update()
        {
            if (!_canDrive)
                return;

            InputManager inputManager = InputManager.Instance;

            if (inputManager == null || !inputManager.IsInitialized)
                return;

            if (_worldCamera == null)
            {
                _worldCamera = Camera.main;

                if (_worldCamera == null)
                    return;
            }

            PlayerLocomotionInput locomotionInput = CollectLocomotionInput(inputManager);

            _locomotionController.Simulate(locomotionInput, Time.deltaTime);
        }

        /// <summary>
        /// 未来 PlayerCameraController 初始化完成后可以主动绑定，
        /// 避免依赖 Camera.main。
        /// </summary>
        public void BindCamera(Camera worldCamera)
        {
            _worldCamera = worldCamera;
        }

        private PlayerLocomotionInput CollectLocomotionInput(InputManager inputManager)
        {
            Vector2 moveInput = inputManager.Move;

            Vector3 cameraForward = Flatten(_worldCamera.transform.forward);
            Vector3 cameraRight = Flatten(_worldCamera.transform.right);

            if (cameraForward.sqrMagnitude > 0.000001f)
                cameraForward.Normalize();

            if (cameraRight.sqrMagnitude > 0.000001f)
                cameraRight.Normalize();

            Vector3 worldMove =
                cameraForward * moveInput.y +
                cameraRight * moveInput.x;

            // 键盘对角输入有可能 > 1，限制到单位圆。
            if (worldMove.sqrMagnitude > 1f)
                worldMove.Normalize();

            Vector3 aimDirection = inputManager.AimHeld
                ? ResolveAimDirection(inputManager.PointerPosition)
                : Vector3.zero;

            return new PlayerLocomotionInput(
                worldMove,
                aimDirection,
                inputManager.AimHeld,
                inputManager.SprintHeld);
        }

        private Vector3 ResolveAimDirection(Vector2 pointerPosition)
        {
            Ray ray = _worldCamera.ScreenPointToRay(pointerPosition);

            // 游戏完全位于同一水平面，所以直接用玩家当前 Y 建 AimPlane。
            Plane aimPlane = new Plane(Vector3.up, transform.position);

            if (!aimPlane.Raycast(ray, out float distance))
                return Flatten(transform.forward).normalized;

            Vector3 aimPoint = ray.GetPoint(distance);
            Vector3 aimDirection = Flatten(aimPoint - transform.position);

            if (aimDirection.sqrMagnitude <= 0.000001f)
                return Flatten(transform.forward).normalized;

            return aimDirection.normalized;
        }

        private static Vector3 Flatten(Vector3 value)
        {
            value.y = 0f;
            return value;
        }
    }
}
