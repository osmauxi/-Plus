using System;
using ProjectGame.HotFix.Core.Events;
using ProjectGame.HotFix.Gameplay.Events;
using ProjectGame.HotFix.Gameplay.Input;
using ProjectGame.HotFix.Gameplay.Player;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Movement
{
    /// <summary>
    /// 本地玩家输入适配器。
    ///
    /// InputManager + Camera
    /// ↓
    /// PlayerInputCommand
    /// ↓
    /// PlayerSyncController
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerSyncController))]
    [RequireComponent(typeof(PlayerRuntimeInitializer))]
    public sealed class LocalPlayerLocomotionDriver : NetworkBehaviour
    {
        [Header("世界空间输入相机")]
        [Tooltip("用于把移动输入和鼠标指针转换为世界空间方向。可由 PlayerCameraController 注入；未指定时自动使用带 MainCamera 标签的相机。")]
        [InspectorName("世界相机")]
        [SerializeField] private Camera _worldCamera;

        // 接收最新输入意图，并在网络固定 Tick 中分配 Tick、预测和发送。
        private PlayerSyncController _syncController;
        // 防止角色外观、武器和 Animator 仍在异步装载时提前开放 Gameplay 输入。
        private PlayerRuntimeInitializer _runtimeInitializer;
        // 世界相机由 Camera System 通过本地事件发布；订阅跨对象池 Spawn 生命周期保留。
        private IDisposable _worldCameraSubscription;

        // 只有本 NetworkObject 的 Owner 才允许读取本机 InputManager。
        private bool _canDrive;
        // 每次 ReloadPressedThisFrame 递增一次，并持续随输入发送，直到下一次按下。
        // ushort 允许自然回绕；状态机通过与 LastReloadRequestSequence 不相等检测新请求。
        private ushort _reloadRequestSequence;

        /// <summary>缓存同一玩家根对象上的同步总控；此时不读取网络身份或输入。</summary>
        private void Awake()
        {
            _syncController = GetComponent<PlayerSyncController>();
            _runtimeInitializer = GetComponent<PlayerRuntimeInitializer>();
            _worldCameraSubscription =
                LocalEvents.Subscribe<GameplayWorldCameraChangedEvent>(HandleWorldCameraChanged);
        }

        /// <summary>网络生成后确定 Owner 身份，并为本次会话重建 Reload 边沿序号。</summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _canDrive = IsOwner;
            _reloadRequestSequence = 0;

            if (_canDrive && _worldCamera == null)
                _worldCamera = Camera.main;
        }

        /// <summary>网络销毁时停止采集输入并清理会话级边沿序号，防止对象池复用携带旧请求。</summary>
        public override void OnNetworkDespawn()
        {
            _canDrive = false;
            _reloadRequestSequence = 0;
            base.OnNetworkDespawn();
        }

        /// <summary>
        /// 每个渲染帧采集最新输入意图；不在这里推进 Simulation。
        /// PlayerSyncController 会在下一个固定网络 Tick 读取这份最新值并赋予正式 Tick 编号。
        /// </summary>
        private void Update()
        {
            if (!_canDrive)
                return;

            if (_runtimeInitializer == null || !_runtimeInitializer.IsInitialized)
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

            if (inputManager.ReloadPressedThisFrame)
            {
                // unchecked 明确允许 65535→0；序号只承担相邻请求去重，不表示累计换弹次数。
                _reloadRequestSequence = unchecked((ushort)(_reloadRequestSequence + 1));
            }

            PlayerLocomotionInput input = CollectLocomotionInput(inputManager);
            _syncController.SubmitLocalInput(ToCommand(input));
        }

        /// <summary>
        /// 未来 PlayerCameraController 初始化完成后可以主动绑定，
        /// 避免依赖 Camera.main。
        /// </summary>
        public void BindCamera(Camera worldCamera)
        {
            _worldCamera = worldCamera;
        }

        /// <summary>
        /// Camera System 的观察者通知入口。玩家输入层只接收最终世界相机，
        /// 不查询 PlayerCameraController、Cinemachine 或场景单例。
        /// </summary>
        private void HandleWorldCameraChanged(GameplayWorldCameraChangedEvent eventData)
        {
            BindCamera(eventData.WorldCamera);
        }

        private void OnDestroy()
        {
            _worldCameraSubscription?.Dispose();
            _worldCameraSubscription = null;
        }

        /// <summary>
        /// 把设备空间输入转换为世界平面意图。
        /// Camera 只用于坐标转换；结果不携带相机引用，可安全进入预测和服务器模拟。
        /// </summary>
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
                inputManager.SprintHeld,
                inputManager.FireHeld,
                _reloadRequestSequence);
        }

        /// <summary>
        /// 从屏幕指针发射射线，与玩家当前高度的水平面求交并返回单位方向。
        /// 射线平行或交点过近时退回身体 Forward，避免向零向量归一化。
        /// </summary>
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

        /// <summary>移除 Y 分量，把相机或射线方向投影到玩家 Gameplay 使用的 XZ 平面。</summary>
        private static Vector3 Flatten(Vector3 value)
        {
            value.y = 0f;
            return value;
        }

        /// <summary>
        /// 把本地移动输入压缩成网络命令：XZ Vector3 映射到 Vector2(x,z)，持续状态写入位标志，
        /// Reload 边沿单独使用序号传递。Tick 留空，由 PlayerSyncController 在固定步中填写。
        /// </summary>
        private static PlayerInputCommand ToCommand(in PlayerLocomotionInput input)
        {
            PlayerInputButtons buttons = PlayerInputButtons.None;
            if (input.AimHeld) buttons |= PlayerInputButtons.AimHeld;
            if (input.SprintHeld) buttons |= PlayerInputButtons.SprintHeld;
            if (input.FireHeld) buttons |= PlayerInputButtons.FireHeld;

            return new PlayerInputCommand
            {
                WorldMove = new Vector2(input.WorldMove.x, input.WorldMove.z),
                AimDirection = new Vector2(input.AimDirection.x, input.AimDirection.z),
                Buttons = buttons,
                ReloadRequestSequence = input.ReloadRequestSequence,
            };
        }
    }
}
