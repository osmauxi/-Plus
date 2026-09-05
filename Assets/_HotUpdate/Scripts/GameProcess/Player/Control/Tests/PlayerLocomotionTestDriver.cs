using ProjectGame.HotFix.Gameplay.Player.Stamina;
using ProjectGame.HotFix.Gameplay.Player.State;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectGame.HotFix.Gameplay.Player.Movement
{
    /// <summary>
    /// 独立移动测试场景使用的输入适配器 
    ///
    /// 它只负责把键鼠输入转换成 PlayerLocomotionInput，实际状态、体力和
    /// CharacterController 移动仍全部走正式的纯 C# PlayerLocomotionController → PlayerMotor 链 
    /// 这样无需启动 NetworkManager 或完整 GameRuntimeScene 也能测试移动手感 
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerLocomotionTestDriver : MonoBehaviour
    {
        [Header("移动测试场景")]
        [Tooltip("用于将键鼠输入转换到世界空间的相机 未指定时自动使用带 MainCamera 标签的相机 ")]
        [InspectorName("世界相机")]
        [SerializeField] private Camera _worldCamera;
        [Tooltip("启用后，相机会保持初始偏移并跟随玩家移动；关闭后，相机固定在场景中的初始位置 ")]
        [InspectorName("相机跟随玩家")]
        [SerializeField] private bool _followPlayerWithCamera = true;
        [Tooltip("启用后在 Game 视图左上角显示按键说明、移动模式、运动阶段、速度和体力；关闭后隐藏测试信息面板 ")]
        [InspectorName("显示诊断信息")]
        [SerializeField] private bool _showDiagnostics = true;

        [Header("移动模拟配置")]
        [Tooltip("独立测试场景构造纯 C# PlayerMotor 时使用的移动参数 ")]
        [InspectorName("玩家移动参数")]
        [SerializeField] private PlayerMovementConfig _movementConfig = new();

        [Tooltip("独立测试场景构造纯 C# PlayerLocomotionController 时使用的体力参数 ")]
        [InspectorName("玩家体力参数")]
        [SerializeField] private PlayerStaminaConfig _staminaConfig = new();

        [Tooltip("独立测试场景构造玩家动作状态机时使用的受击、射击和换弹参数 ")]
        [InspectorName("玩家动作参数")]
        [SerializeField] private PlayerActionConfig _actionConfig = new();

        private PlayerLocomotionController _locomotionController;

        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private Vector3 _lastAimDirection;
        private Vector3 _lastMoveDirection;
        private Vector3 _cameraOffset;

        private void Awake()
        {
            CharacterController characterController = GetComponent<CharacterController>();
            IPlayerCharacterBody body = new CharacterControllerPlayerBody(transform, characterController);
            PlayerMotor motor = new(body, _movementConfig);
            _locomotionController = new PlayerLocomotionController(motor, _staminaConfig, _actionConfig);
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;

            if (_worldCamera == null)
                _worldCamera = Camera.main;

            if (_worldCamera != null)
                _cameraOffset = _worldCamera.transform.position - transform.position;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.rKey.wasPressedThisFrame)
                ResetPlayer();

            Vector2 planarInput = ReadPlanarInput(keyboard);
            Vector3 worldMove = ResolveWorldMove(planarInput);

            bool aimHeld = Mouse.current != null && Mouse.current.rightButton.isPressed;
            bool sprintHeld = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            Vector3 aimDirection = aimHeld ? ResolveAimDirection() : Vector3.zero;

            _lastMoveDirection = worldMove;
            _lastAimDirection = aimDirection;

            PlayerLocomotionInput locomotionInput = new(
                worldMove,
                aimDirection,
                aimHeld,
                sprintHeld);

            _locomotionController.Simulate(locomotionInput, Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (_followPlayerWithCamera && _worldCamera != null)
                _worldCamera.transform.position = transform.position + _cameraOffset;
        }

        private void ResetPlayer()
        {
            _locomotionController.Warp(_spawnPosition, _spawnRotation);
            _locomotionController.ResetRuntimeState();
        }

        private static Vector2 ReadPlanarInput(Keyboard keyboard)
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                horizontal -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                horizontal += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                vertical -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                vertical += 1f;

            return Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
        }

        private Vector3 ResolveWorldMove(Vector2 planarInput)
        {
            if (_worldCamera == null)
                _worldCamera = Camera.main;

            Vector3 forward = _worldCamera == null
                ? Vector3.forward
                : Flatten(_worldCamera.transform.forward);
            Vector3 right = _worldCamera == null
                ? Vector3.right
                : Flatten(_worldCamera.transform.right);

            if (forward.sqrMagnitude > 0.000001f)
                forward.Normalize();
            if (right.sqrMagnitude > 0.000001f)
                right.Normalize();

            return Vector3.ClampMagnitude(
                forward * planarInput.y + right * planarInput.x,
                1f);
        }

        private Vector3 ResolveAimDirection()
        {
            if (_worldCamera == null)
                return Flatten(transform.forward).normalized;

            Vector2 pointerPosition = Mouse.current == null
                ? new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
                : Mouse.current.position.ReadValue();

            Ray ray = _worldCamera.ScreenPointToRay(pointerPosition);
            Plane aimPlane = new(Vector3.up, transform.position);

            if (!aimPlane.Raycast(ray, out float distance))
                return Flatten(transform.forward).normalized;

            Vector3 direction = Flatten(ray.GetPoint(distance) - transform.position);
            return direction.sqrMagnitude <= 0.000001f
                ? Flatten(transform.forward).normalized
                : direction.normalized;
        }

        private void OnGUI()
        {
            if (!_showDiagnostics || _locomotionController == null)
                return;

            PlayerMotionState motion = _locomotionController.MotionState;

            GUI.Box(new Rect(16f, 16f, 390f, 178f), "Player Locomotion Test (No Animator)");
            GUI.Label(new Rect(32f, 46f, 360f, 22f), "Move: WASD / Arrow Keys");
            GUI.Label(new Rect(32f, 68f, 360f, 22f), "Sprint: Hold Shift    Aim: Hold Right Mouse");
            GUI.Label(new Rect(32f, 90f, 360f, 22f), "Reset: R");
            GUI.Label(new Rect(32f, 120f, 360f, 22f),
                $"Mode: {_locomotionController.ControlState.LocomotionMode}    Phase: {motion.Phase}");
            GUI.Label(new Rect(32f, 142f, 360f, 22f),
                $"Speed: {motion.Speed:F2}    Stamina: {_locomotionController.NormalizedStamina:P0}");
            GUI.Label(new Rect(32f, 164f, 360f, 22f),
                $"Position: {transform.position.x:F1}, {transform.position.z:F1}");
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position + Vector3.up * 0.1f;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, origin + _lastMoveDirection * 2f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, origin + _lastAimDirection * 2.5f);
        }

        private static Vector3 Flatten(Vector3 value)
        {
            value.y = 0f;
            return value;
        }
    }
}
