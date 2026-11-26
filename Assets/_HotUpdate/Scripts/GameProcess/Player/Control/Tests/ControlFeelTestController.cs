using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Core.Events;
using ProjectGame.HotFix.Gameplay.CameraSystem;
using ProjectGame.HotFix.Gameplay.Events;
using ProjectGame.HotFix.Gameplay.Player.Movement;
using ProjectGame.HotFix.Gameplay.Player.Stamina;
using ProjectGame.HotFix.Gameplay.Player.State;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectGame.HotFix.Gameplay.Player.ControlTests
{
    /// <summary>
    /// 独立的玩家控制手感测试器。
    ///
    /// 这份测试器只提供输入采集和表现承载，移动、瞄准转向、冲刺、急停和速度曲线
    /// 直接调用正式 PlayerSimulation 链路。它不接入网络同步、正式武器系统或关卡流程。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ControlFeelTestController : MonoBehaviour
    {
        [Header("正式移动规则")]
        [SerializeField] private PlayerMovementConfig _movementConfig = new();
        [SerializeField] private PlayerStaminaConfig _staminaConfig = new();
        [SerializeField] private PlayerActionConfig _actionConfig = new();
        [Min(1f)] [SerializeField] private float _simulationTickRate = 30f;

        [Header("镜头响应")]
        [Min(0f)] [SerializeField] private float _shotFovKick = 2.2f;
        [Min(0f)] [SerializeField] private float _shotZoomKick = -0.45f;
        [Min(0f)] [SerializeField] private float _shotShakeStrength = 1.2f;

        [Header("测试场景比例")]
        [SerializeField] private Vector3 _targetScale = new(1.8f, 3f, 1.8f);

        [Header("射击反馈")]
        [Min(0.01f)] [SerializeField] private float _fireInterval = 0.12f;
        [Min(1f)] [SerializeField] private float _shotDistance = 40f;
        [Min(0.01f)] [SerializeField] private float _tracerLifetime = 0.045f;
        [SerializeField] private AudioClip _shotClip;

        private readonly List<ControlFeelTestTarget> _targets = new();
        private CharacterController _playerController;
        private Transform _player;
        private Transform _visualRoot;
        private Transform _muzzle;
        private Camera _camera;
        private Camera _aimReferenceCamera;
        private PlayerCameraController _cameraController;
        private CinemachineImpulseSource _shotImpulseSource;
        private IDisposable _aimReferenceCameraSubscription;
        private AudioSource _audioSource;
        private Light _muzzleLight;
        private float _fireTimer;
        private int _shotCount;
        private Vector3 _aimDirection = Vector3.forward;
        private Vector3 _aimWorldPosition;
        private PlayerSimulation _simulation;
        private ControlFeelTestPresentationDriver _presentationDriver;
        private PlayerInputCommand _latestInput;
        private float _simulationTickDeltaTime;
        private float _simulationAccumulator;
        private uint _simulationTick;

        private void Awake()
        {
            DisableRuntimeFlowForTest();
            BuildTestSceneIfNeeded();

            CharacterControllerPlayerBody body =
                new(_player, _playerController);
            PlayerMotor motor = new(body, _movementConfig);
            PlayerLocomotionController locomotion =
                new(motor, _staminaConfig, _actionConfig);
            _simulation = new PlayerSimulation(locomotion);
            _simulationTickDeltaTime = 1f / Mathf.Max(_simulationTickRate, 1f);
            _latestInput = PlayerInputCommand.CreateNeutral(0u);

            _presentationDriver = GetComponent<ControlFeelTestPresentationDriver>();
            if (_presentationDriver == null)
                _presentationDriver = gameObject.AddComponent<ControlFeelTestPresentationDriver>();
            _presentationDriver.Initialize(_player, _visualRoot, _simulation);

            _cameraController = FindFirstObjectByType<PlayerCameraController>();
            if (_cameraController == null)
                throw new InvalidOperationException(
                    "ControlFeelTest 场景缺少现有 PlayerCameraController。请先放入 GameRoot prefab。");

            Camera worldCamera = _cameraController.WorldCamera;
            if (worldCamera != null)
                _camera = worldCamera;

            _aimReferenceCameraSubscription =
                LocalEvents.Subscribe<GameplayAimReferenceCameraChangedEvent>(
                    HandleAimReferenceCameraChanged);
            InitializeCameraAsync().Forget();
        }

        private static void DisableRuntimeFlowForTest()
        {
            GameObject gameRoot = GameObject.Find("GameRoot_Test");
            if (gameRoot == null)
                return;

            Transform cameraRig = gameRoot.transform.Find("PlayerCameraRig");
            foreach (Transform child in gameRoot.transform)
            {
                if (child != cameraRig)
                    child.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
                return;

            CollectInput();
            AdvanceSimulation(deltaTime);
            UpdateFire(deltaTime);
            UpdateMuzzleFlash(deltaTime);

            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                ResetPlayer();
        }

        private void AdvanceSimulation(float deltaTime)
        {
            if (_simulation == null)
                return;

            _simulationAccumulator += deltaTime;

            while (_simulationAccumulator >= _simulationTickDeltaTime)
            {
                _simulationAccumulator -= _simulationTickDeltaTime;
                _simulationTick++;
                _simulation.Simulate(
                    _latestInput.WithTick(_simulationTick),
                    _simulationTickDeltaTime);
            }
        }

        private void CollectInput()
        {
            Keyboard keyboard = Keyboard.current;
            Vector2 moveInput = Vector2.zero;

            if (keyboard != null)
            {
                moveInput.x = (keyboard.dKey.isPressed ? 1f : 0f) -
                    (keyboard.aKey.isPressed ? 1f : 0f);
                moveInput.y = (keyboard.wKey.isPressed ? 1f : 0f) -
                    (keyboard.sKey.isPressed ? 1f : 0f);
            }

            Vector3 cameraForward = Flatten(_camera.transform.forward);
            Vector3 cameraRight = Flatten(_camera.transform.right);

            if (cameraForward.sqrMagnitude > 0.000001f)
                cameraForward.Normalize();
            if (cameraRight.sqrMagnitude > 0.000001f)
                cameraRight.Normalize();

            Vector3 worldMove = cameraForward * moveInput.y +
                cameraRight * moveInput.x;

            if (worldMove.sqrMagnitude > 1f)
                worldMove.Normalize();

            bool aimHeld = Mouse.current != null &&
                Mouse.current.rightButton.isPressed;
            bool fireHeld = Mouse.current != null &&
                Mouse.current.leftButton.isPressed;
            bool sprintHeld = keyboard != null &&
                keyboard.leftShiftKey.isPressed;

            Mouse mouse = Mouse.current;
            Vector3 aimDirection = Vector3.zero;
            Camera aimCamera = _aimReferenceCamera != null
                ? _aimReferenceCamera
                : _camera;

            if (aimHeld && mouse != null)
            {
                Ray ray = aimCamera.ScreenPointToRay(
                    mouse.position.ReadValue());
                Plane ground = new(Vector3.up, Vector3.zero);

                if (ground.Raycast(ray, out float distance))
                {
                    _aimWorldPosition = ray.GetPoint(distance);
                    aimDirection = Flatten(_aimWorldPosition - _player.position);

                    if (aimDirection.sqrMagnitude > 0.0001f)
                        aimDirection.Normalize();
                    else
                        aimDirection = Flatten(_player.forward).normalized;
                }
            }

            _aimDirection = aimDirection;
            _presentationDriver?.SetAimDirection(aimDirection, aimHeld);
            _cameraController.SetAimComposition(aimHeld);
            _cameraController.SetCameraFovModifier(
                CameraEffectId.Aim,
                aimHeld ? -8f : 0f,
                aimHeld ? 0.12f : 0.16f);

            if (aimHeld && aimDirection.sqrMagnitude > 0.0001f)
                CameraEffects.UpdateAimTarget(_aimWorldPosition);

            PlayerInputButtons buttons = PlayerInputButtons.None;
            if (aimHeld) buttons |= PlayerInputButtons.AimHeld;
            if (sprintHeld) buttons |= PlayerInputButtons.SprintHeld;
            if (fireHeld) buttons |= PlayerInputButtons.FireHeld;

            _latestInput = new PlayerInputCommand
            {
                WorldMove = new Vector2(worldMove.x, worldMove.z),
                AimDirection = new Vector2(aimDirection.x, aimDirection.z),
                Buttons = buttons,
                ReloadRequestSequence = 0,
            };
        }

        private void UpdateFire(float deltaTime)
        {
            if (!_latestInput.AimHeld || !_latestInput.FireHeld)
            {
                _fireTimer = 0f;
                if (_muzzleLight != null)
                    _muzzleLight.enabled = false;
                return;
            }

            _fireTimer -= deltaTime;
            if (_fireTimer > 0f)
                return;

            _fireTimer = _fireInterval;
            FireShot();
        }

        private void FireShot()
        {
            _shotCount++;
            _presentationDriver?.PlayShot();
            _cameraController.PlayCameraFovKick(
                _shotFovKick,
                attackSmoothTime: 0.02f,
                releaseSmoothTime: 0.1f);
            _cameraController.PlayCameraZoomKick(
                _shotZoomKick,
                attackSmoothTime: 0.015f,
                releaseSmoothTime: 0.08f);
            PlayShotImpulse();

            if (_muzzleLight != null)
            {
                _muzzleLight.enabled = true;
                _muzzleLight.intensity = 8f;
            }

            if (_audioSource != null && _shotClip != null)
                _audioSource.PlayOneShot(_shotClip);

            Transform shotMuzzle = _presentationDriver != null && _presentationDriver.Muzzle != null
                ? _presentationDriver.Muzzle
                : _muzzle;
            Vector3 start = shotMuzzle != null
                ? shotMuzzle.position
                : _player.position + Vector3.up * 1.3f;
            Vector3 end = start + _aimDirection * _shotDistance;
            if (Physics.Raycast(start, _aimDirection, out RaycastHit hit, _shotDistance))
            {
                end = hit.point;
                ControlFeelTestTarget target = hit.collider.GetComponent<ControlFeelTestTarget>();
                if (target != null)
                    target.Hit();
            }

            StartCoroutine(ShowTracer(start, end));
        }

        private void UpdateMuzzleFlash(float deltaTime)
        {
            if (_muzzleLight == null)
                return;

            Transform shotMuzzle = _presentationDriver != null && _presentationDriver.Muzzle != null
                ? _presentationDriver.Muzzle
                : _muzzle;
            if (shotMuzzle != null && _muzzleLight.transform.parent != shotMuzzle)
                _muzzleLight.transform.SetParent(shotMuzzle, false);

            _muzzleLight.intensity = Mathf.MoveTowards(
                _muzzleLight.intensity,
                0f,
                80f * deltaTime);

            if (_muzzleLight.intensity <= 0.01f)
                _muzzleLight.enabled = false;
        }

        private void PlayShotImpulse()
        {
            if (_shotImpulseSource == null || _shotShakeStrength <= 0f)
                return;

            Vector3 direction = Flatten(_aimDirection);
            if (direction.sqrMagnitude <= 0.0001f)
                direction = Flatten(_player.forward);
            if (direction.sqrMagnitude <= 0.0001f)
                direction = Vector3.forward;

            _shotImpulseSource.GenerateImpulseWithVelocity(
                -direction.normalized * _shotShakeStrength);
        }

        private System.Collections.IEnumerator ShowTracer(Vector3 start, Vector3 end)
        {
            GameObject tracerObject = new("ShotTracer");
            LineRenderer tracer = tracerObject.AddComponent<LineRenderer>();
            tracer.startWidth = 0.045f;
            tracer.endWidth = 0.012f;
            tracer.positionCount = 2;
            tracer.material = CreateMaterial(new Color(1f, 0.74f, 0.2f));
            tracer.SetPosition(0, start);
            tracer.SetPosition(1, end);
            yield return new WaitForSeconds(_tracerLifetime);
            Destroy(tracerObject);
        }

        private void HandleAimReferenceCameraChanged(
            GameplayAimReferenceCameraChangedEvent eventData)
        {
            _aimReferenceCamera = eventData.AimReferenceCamera;
        }

        private async UniTaskVoid InitializeCameraAsync()
        {
            await _cameraController.InitializeAsync(CancellationToken.None);

            LocalEvents.Publish(new GameplayCameraTargetRequestedEvent(
                this,
                _visualRoot,
                _visualRoot,
                priority: 0,
                snap: true));
        }

        private void OnDestroy()
        {
            _aimReferenceCameraSubscription?.Dispose();
            _aimReferenceCameraSubscription = null;

            if (_cameraController != null)
                LocalEvents.Publish(new GameplayCameraTargetReleasedEvent(this));
        }

        private void ResetPlayer()
        {
            _simulation.ResetRuntimeState();
            _simulation.Warp(Vector3.up, Quaternion.identity);
            _simulationTick = 0u;
            _simulationAccumulator = 0f;
            _latestInput = PlayerInputCommand.CreateNeutral(0u);
            LocalEvents.Publish<GameplayCameraSnapRequestedEvent>();
        }

        private static Vector3 Flatten(Vector3 value)
        {
            value.y = 0f;
            return value;
        }

        private void BuildTestSceneIfNeeded()
        {
            if (_camera == null)
                _camera = Camera.main;

            if (_player == null)
            {
                GameObject player = new("ControlFeelPlayer");
                player.name = "ControlFeelPlayer";
                player.transform.position = Vector3.zero;
                _playerController = player.AddComponent<CharacterController>();
                _playerController.height = 2f;
                _playerController.radius = 0.6f;
                _playerController.center = Vector3.up;
                _player = player.transform;

                GameObject visualRoot = new("VisualRoot");
                visualRoot.transform.SetParent(_player, false);
                _visualRoot = visualRoot.transform;

                GameObject muzzle = new("Muzzle");
                muzzle.transform.SetParent(_player, false);
                muzzle.transform.localPosition = new Vector3(0f, 1.25f, 0.85f);
                _muzzle = muzzle.transform;
            }

            if (_camera == null)
            {
                GameObject cameraObject = new("ControlFeelCamera");
                cameraObject.tag = "MainCamera";
                _camera = cameraObject.AddComponent<Camera>();
                _camera.transform.position = new Vector3(0f, 17f, -10f);
                _camera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
                _camera.clearFlags = CameraClearFlags.SolidColor;
                _camera.backgroundColor = new Color(0.035f, 0.045f, 0.06f);
                cameraObject.AddComponent<CinemachineBrain>();
                cameraObject.AddComponent<AudioListener>();
            }

            if (GameObject.Find("ControlFeelGround") == null)
            {
                GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "ControlFeelGround";
                ground.transform.localScale = Vector3.one * 5f;
                ground.GetComponent<Renderer>().material = CreateMaterial(new Color(0.18f, 0.2f, 0.23f));
            }

            if (GameObject.Find("ControlFeelLight") == null)
            {
                GameObject lightObject = new("ControlFeelLight");
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.25f;
                lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.spatialBlend = 0f;
                if (_shotClip == null)
                    _shotClip = CreateShotClip();
            }

            if (_shotImpulseSource == null)
            {
                _shotImpulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
                _shotImpulseSource.m_ImpulseDefinition.m_ImpulseChannel = 1;
                _shotImpulseSource.m_ImpulseDefinition.m_ImpulseType =
                    CinemachineImpulseDefinition.ImpulseTypes.Uniform;
                _shotImpulseSource.m_ImpulseDefinition.m_ImpulseShape =
                    CinemachineImpulseDefinition.ImpulseShapes.Recoil;
                _shotImpulseSource.m_ImpulseDefinition.m_ImpulseDuration = 0.14f;
                _shotImpulseSource.m_ImpulseDefinition.m_AmplitudeGain = 1f;
            }

            if (_muzzleLight == null)
            {
                GameObject lightObject = new("MuzzleFlash");
                lightObject.transform.SetParent(_muzzle, false);
                _muzzleLight = lightObject.AddComponent<Light>();
                _muzzleLight.type = LightType.Point;
                _muzzleLight.range = 5f;
                _muzzleLight.color = new Color(1f, 0.55f, 0.15f);
                _muzzleLight.enabled = false;
            }

            if (_targets.Count == 0)
                CreateTargets();
        }

        private void CreateTargets()
        {
            Vector3[] positions =
            {
                new(0f, 1f, 8f), new(6f, 1f, 10f), new(-7f, 1f, 12f),
                new(10f, 1f, 2f), new(-11f, 1f, 2f), new(5f, 1f, -8f),
                new(-6f, 1f, -9f), new(13f, 1f, -10f),
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                targetObject.name = $"ControlFeelTarget_{i + 1:00}";
                Vector3 position = positions[i];
                position.y = _targetScale.y * 0.5f;
                targetObject.transform.position = position;
                targetObject.transform.localScale = _targetScale;
                ControlFeelTestTarget target = targetObject.AddComponent<ControlFeelTestTarget>();
                target.Initialize(this, new Color(0.9f, 0.12f, 0.12f));
                _targets.Add(target);
            }
        }

        private AudioClip CreateShotClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.08f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            AudioClip clip = AudioClip.Create("ProceduralShot", sampleCount, 1, sampleRate, false);
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Exp(-38f * t);
                float body = Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.45f;
                float crack = Mathf.Sin(2f * Mathf.PI * 1900f * t) * 0.16f;
                samples[i] = (body + crack) * envelope;
            }
            clip.SetData(samples, 0);
            return clip;
        }

        private static Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new(shader);
            material.color = color;
            return material;
        }

        private void OnGUI()
        {
            if (_simulation == null)
                return;

            GUI.color = Color.white;
            GUILayout.BeginArea(new Rect(18f, 18f, 460f, 180f), GUI.skin.box);
            GUILayout.Label("Control Feel Test");
            GUILayout.Label("WASD 移动    Shift 冲刺    鼠标右键瞄准    鼠标左键开枪    R 重置");
            GUILayout.Label($"Speed {_simulation.MotionState.Velocity.magnitude:F2}    Aim {_aimDirection.x:F2}, {_aimDirection.z:F2}");
            GUILayout.Label($"Shots {_shotCount}    Targets {_targets.Count}");
            GUILayout.Label("观察重点  松键是否立即停下、反向是否干脆、开枪是否产生光效/音效/镜头冲击");
            GUILayout.EndArea();
        }

        internal void RespawnTarget(ControlFeelTestTarget target)
        {
            Vector2 random = UnityEngine.Random.insideUnitCircle.normalized *
                UnityEngine.Random.Range(9f, 15f);
            target.transform.position = new Vector3(random.x, _targetScale.y * 0.5f, random.y);
        }
    }

    public sealed class ControlFeelTestTarget : MonoBehaviour
    {
        private ControlFeelTestController _owner;
        private Renderer _renderer;
        private Color _baseColor;
        private float _flashRemaining;

        internal void Initialize(ControlFeelTestController owner, Color color)
        {
            _owner = owner;
            _renderer = GetComponent<Renderer>();
            _baseColor = color;
            _renderer.material = CreateMaterial(color);
        }

        internal void Hit()
        {
            _flashRemaining = 0.1f;
            _owner.RespawnTarget(this);
        }

        private void Update()
        {
            _flashRemaining = Mathf.MoveTowards(_flashRemaining, 0f, Time.deltaTime);
            if (_renderer != null)
                _renderer.material.color = _flashRemaining > 0f ? Color.white : _baseColor;
        }

        private static Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new(shader);
            material.color = color;
            return material;
        }
    }
}
