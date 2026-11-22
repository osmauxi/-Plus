using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Character;
using ProjectGame.HotFix.Gameplay.Player.Movement;
using ProjectGame.HotFix.Gameplay.Player.State;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectGame.HotFix.Gameplay.Player.ControlTests
{
    /// <summary>
    /// ControlFeelTest 的表现副本。
    /// 运动和状态仍来自正式 PlayerSimulation，这里只把状态映射到正式角色 Animator、IK 和武器桥。
    /// </summary>
    [DefaultExecutionOrder(-300)]
    [DisallowMultipleComponent]
    public sealed class ControlFeelTestPresentationDriver : MonoBehaviour
    {
        private static readonly int VelocityXHash = Animator.StringToHash("VelocityX");
        private static readonly int VelocityZHash = Animator.StringToHash("VelocityZ");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int HasMoveInputHash = Animator.StringToHash("HasMoveInput");
        private static readonly int LocomotionModeHash = Animator.StringToHash("LocomotionMode");
        private static readonly int IsDeadHash = Animator.StringToHash("IsDead");
        private static readonly int IsHitReactingHash = Animator.StringToHash("IsHitReacting");
        private static readonly int IsReloadingHash = Animator.StringToHash("IsReloading");
        private static readonly int IsPivotingHash = Animator.StringToHash("IsPivoting");
        private static readonly int ShootHash = Animator.StringToHash("Shoot");
        private static readonly int FireStateHash = Animator.StringToHash("Fire");

        private const string CharacterAddress = "Character_Gameplay_Anbi_Generic";
        private const string WeaponAddress = "Character_Weapon_00";
        private const string UpperBodyLayerName = "UpperBody Aim";

        [SerializeField, Min(0f)] private float _locomotionDampTime = 0.08f;
        [SerializeField, Min(0f)] private float _speedDampTime = 0.1f;
        [SerializeField, Min(0f)] private float _upperBodyBlendSpeed = 24f;
        [SerializeField, Min(0f)] private float _upperBodyAimSmoothTime = 0.02f;
        [SerializeField, Min(0f)] private float _weaponAimAngularSpeed = 1800f;
        [SerializeField, Min(0f)] private float _weaponAimPoseBlendSpeed = 24f;
        [SerializeField, Range(0f, 90f)] private float _upperBodyAimMaxYaw = 60f;
        [SerializeField] private Vector3 _upperBodyAimBoneWeights = new(0.2f, 0.35f, 0.45f);
        [SerializeField, Min(0f)] private float _turnLeanMaxAngle = 3f;
        [SerializeField, Min(1f)] private float _turnLeanAngularSpeed = 540f;
        [SerializeField, Min(0f)] private float _turnLeanSmoothTime = 0.12f;
        [Header("基础动作过渡")]
        [SerializeField, Min(0f)] private float _baseStateBlendTime = 0.08f;
        [SerializeField, Min(0f)] private float _pivotStateBlendTime = 0.05f;
        [Header("渲染位姿补帧")]
        [SerializeField, Min(0f)] private float _presentationTickDuration = 1f / 30f;
        [SerializeField, Min(0.1f)] private float _presentationSnapDistance = 2f;
        [SerializeField, Range(1f, 180f)] private float _presentationSnapRotationAngle = 90f;
        [Header("测试角色比例")]
        [SerializeField, Range(0.6f, 1f)] private float _visualScale = 0.82f;

        private Transform _playerRoot;
        private Transform _visualRoot;
        private PlayerSimulation _simulation;
        private AsyncOperationHandle<GameObject> _characterHandle;
        private AsyncOperationHandle<GameObject> _weaponHandle;
        private PlayerModelView _modelView;
        private Animator _animator;
        private CharacterAnimationBridge _animationBridge;
        private WeaponView _weaponView;
        private Transform _leanRoot;
        private Transform _spine;
        private Transform _chest;
        private Transform _upperChest;
        private Quaternion _leanRootBaseRotation;
        private int _upperBodyLayerIndex = -1;
        private float _upperBodyWeight;
        private float _aimYaw;
        private float _aimYawVelocity;
        private float _leanAngle;
        private float _leanVelocity;
        private bool _fireRequested;
        private int _fireRequestFrame = -1;
        private int _lastBaseStateHash;
        private CancellationTokenSource _loadCts;
        private Vector3 _presentationFromPosition;
        private Vector3 _presentationToPosition;
        private Quaternion _presentationFromRotation;
        private Quaternion _presentationToRotation;
        private float _presentationElapsed;
        private bool _hasPresentationPose;
        private Vector3 _presentationAimDirection;
        private bool _hasPresentationAimDirection;

        public Transform Muzzle => _weaponView != null ? _weaponView.Muzzle : null;
        public bool IsLoaded => _animator != null && _animator.isActiveAndEnabled;

        public void SetAimDirection(Vector3 worldDirection, bool active)
        {
            worldDirection.y = 0f;
            _presentationAimDirection = worldDirection.sqrMagnitude > 0.000001f
                ? worldDirection.normalized
                : Vector3.zero;
            _hasPresentationAimDirection = active && _presentationAimDirection.sqrMagnitude > 0.000001f;
        }

        public void Initialize(Transform playerRoot, Transform visualRoot, PlayerSimulation simulation)
        {
            _playerRoot = playerRoot;
            _visualRoot = visualRoot;
            _simulation = simulation;
            _presentationFromPosition = _playerRoot.position;
            _presentationToPosition = _playerRoot.position;
            _presentationFromRotation = _playerRoot.rotation;
            _presentationToRotation = _playerRoot.rotation;
            _presentationElapsed = _presentationTickDuration;
            _hasPresentationPose = true;
            _visualRoot.SetPositionAndRotation(_playerRoot.position, _playerRoot.rotation);
            _visualRoot.localScale = Vector3.one * _visualScale;
            _loadCts = new CancellationTokenSource();
            LoadPresentationAsync(_loadCts.Token).Forget();
        }

        public void PlayShot()
        {
            if (_animator == null)
                return;

            _animator.SetTrigger(ShootHash);
            _fireRequested = true;
            _fireRequestFrame = Time.frameCount;
            _weaponView?.PlayFireVFX();
        }

        private async UniTaskVoid LoadPresentationAsync(CancellationToken cancellationToken)
        {
            _characterHandle = Addressables.InstantiateAsync(CharacterAddress, _visualRoot);
            await _characterHandle.ToUniTask(cancellationToken: cancellationToken);
            if (_characterHandle.Status != AsyncOperationStatus.Succeeded)
                throw new InvalidOperationException($"测试角色加载失败：{CharacterAddress}");

            _characterHandle.Result.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _modelView = _characterHandle.Result.GetComponent<PlayerModelView>();
            _animator = _modelView.Animator;
            _animationBridge = _modelView.AnimationBridge;
            _leanRoot = _modelView.LeanRoot != null ? _modelView.LeanRoot : _modelView.transform;
            _spine = _modelView.AnimationSpine;
            _chest = _modelView.AnimationChest;
            _upperChest = _modelView.AnimationUpperChest;
            _leanRootBaseRotation = _leanRoot.localRotation;
            _upperBodyLayerIndex = _animator.GetLayerIndex(UpperBodyLayerName);
            if (_upperBodyLayerIndex >= 0)
                _animator.SetLayerWeight(_upperBodyLayerIndex, 0f);
            _animator.applyRootMotion = false;

            Transform weaponSocket = _modelView.GetEquipmentSocket(EquipmentSlot.RightHand);
            _weaponHandle = Addressables.InstantiateAsync(WeaponAddress, weaponSocket);
            await _weaponHandle.ToUniTask(cancellationToken: cancellationToken);
            if (_weaponHandle.Status != AsyncOperationStatus.Succeeded)
                throw new InvalidOperationException($"测试武器加载失败：{WeaponAddress}");

            _weaponHandle.Result.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _weaponView = _weaponHandle.Result.GetComponent<WeaponView>();
            _animationBridge.BindWeapon(_weaponView, WeaponPose.Rifle);
            _animationBridge.SetWeaponAimResponse(
                _weaponAimAngularSpeed,
                _weaponAimPoseBlendSpeed);
        }

        private void LateUpdate()
        {
            if (_simulation == null || _animator == null || !_animator.isActiveAndEnabled)
                return;

            float deltaTime = Time.deltaTime;
            UpdatePresentationPose(deltaTime);
            PlayerMotionState motion = _simulation.MotionState;
            PlayerControlState control = _simulation.ControlState;

            Vector3 localVelocity = motion.LocalVelocity;
            Vector3 localDirection = motion.Speed > 0.001f
                ? localVelocity / motion.Speed
                : Vector3.zero;
            _animator.SetFloat(VelocityXHash, Mathf.Clamp(localDirection.x, -1f, 1f), _locomotionDampTime, deltaTime);
            _animator.SetFloat(VelocityZHash, Mathf.Clamp(localDirection.z, -1f, 1f), _locomotionDampTime, deltaTime);
            _animator.SetFloat(SpeedHash, motion.Speed, _speedDampTime, deltaTime);
            _animator.SetBool(HasMoveInputHash, motion.HasMoveInput);
            _animator.SetInteger(LocomotionModeHash, (int)control.LocomotionMode);
            _animator.SetBool(IsDeadHash, control.IsDead);
            _animator.SetBool(IsHitReactingHash, control.IsHitReacting);
            _animator.SetBool(IsReloadingHash, control.IsReloading);
            _animator.SetBool(IsPivotingHash, motion.IsPivoting && !control.IsAiming);

            UpdateBaseLocomotionState(motion, control);
            UpdateUpperBodyLayer(control, deltaTime);
            UpdateAim(control, deltaTime);
            UpdateTurnLean(motion, control, deltaTime);

            bool allowAim = control.IsAiming && control.IsAlive && !control.IsHitReacting && !control.IsReloading;
            _animationBridge.SetLeftHandIKAllowed(control.IsAlive && !control.IsHitReacting && !control.IsReloading);
            _animationBridge.SetRightHandIKAllowed(control.IsAlive);
            Vector2 aim = _simulation.AimDirection;
            if (_hasPresentationAimDirection)
                aim = new Vector2(_presentationAimDirection.x, _presentationAimDirection.z);
            _animationBridge.SetWeaponAimDirection(new Vector3(aim.x, 0f, aim.y), allowAim && aim.sqrMagnitude > 0.000001f);
        }

        private void UpdateUpperBodyLayer(in PlayerControlState control, float deltaTime)
        {
            if (_upperBodyLayerIndex < 0)
                return;

            bool allow = control.IsAiming || _fireRequested || control.IsReloading;
            if (!allow || !control.IsAlive || control.IsHitReacting)
            {
                _fireRequested = false;
                _animator.ResetTrigger(ShootHash);
            }
            else if (_fireRequested && Time.frameCount > _fireRequestFrame + 1)
            {
                AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(_upperBodyLayerIndex);
                bool firePlaying = state.shortNameHash == FireStateHash;
                if (_animator.IsInTransition(_upperBodyLayerIndex))
                    firePlaying |= _animator.GetNextAnimatorStateInfo(_upperBodyLayerIndex).shortNameHash == FireStateHash;
                if (!firePlaying)
                    _fireRequested = false;
            }

            float target = allow && control.IsAlive && !control.IsHitReacting ? 1f : 0f;
            _upperBodyWeight = Mathf.MoveTowards(_upperBodyWeight, target, _upperBodyBlendSpeed * deltaTime);
            _animator.SetLayerWeight(_upperBodyLayerIndex, _upperBodyWeight);
        }

        private void UpdateBaseLocomotionState(
            in PlayerMotionState motion,
            in PlayerControlState control)
        {
            string stateName;
            float blendTime;

            if (motion.IsPivoting && !control.IsAiming)
            {
                stateName = "Base FullBody.Pivot";
                blendTime = _pivotStateBlendTime;
            }
            else if (!motion.HasMoveInput)
            {
                stateName = "Base FullBody.Idle";
                blendTime = _baseStateBlendTime;
            }
            else if (control.IsAiming)
            {
                stateName = "Base FullBody.Aim Move";
                blendTime = _baseStateBlendTime;
            }
            else if (control.IsSprinting)
            {
                stateName = "Base FullBody.Sprint Loop";
                blendTime = _baseStateBlendTime;
            }
            else
            {
                stateName = "Base FullBody.Move";
                blendTime = _baseStateBlendTime;
            }

            int stateHash = Animator.StringToHash(stateName);
            if (stateHash == _lastBaseStateHash)
                return;

            _animator.CrossFadeInFixedTime(stateHash, blendTime, 0, 0f);
            _lastBaseStateHash = stateHash;
        }

        private void UpdateAim(in PlayerControlState control, float deltaTime)
        {
            Vector2 aim = _simulation.AimDirection;
            if (_hasPresentationAimDirection)
                aim = new Vector2(_presentationAimDirection.x, _presentationAimDirection.z);
            Vector3 bodyForward = _visualRoot.forward;
            bodyForward.y = 0f;
            float targetYaw = 0f;
            if (control.IsAiming && aim.sqrMagnitude > 0.000001f && bodyForward.sqrMagnitude > 0.000001f)
            {
                targetYaw = Mathf.Clamp(
                    Vector3.SignedAngle(bodyForward.normalized, new Vector3(aim.x, 0f, aim.y).normalized, Vector3.up),
                    -_upperBodyAimMaxYaw,
                    _upperBodyAimMaxYaw);
            }

            _aimYaw = _upperBodyAimSmoothTime <= 0f
                ? targetYaw
                : Mathf.SmoothDampAngle(_aimYaw, targetYaw, ref _aimYawVelocity, _upperBodyAimSmoothTime, Mathf.Infinity, deltaTime);
            float weightedYaw = _aimYaw * _upperBodyWeight;
            float total = Mathf.Max(0f, _upperBodyAimBoneWeights.x) * (_spine != null ? 1f : 0f) +
                Mathf.Max(0f, _upperBodyAimBoneWeights.y) * (_chest != null ? 1f : 0f) +
                Mathf.Max(0f, _upperBodyAimBoneWeights.z) * (_upperChest != null ? 1f : 0f);
            if (total <= 0.0001f || Mathf.Abs(weightedYaw) <= 0.001f)
                return;

            ApplyBoneYaw(_spine, weightedYaw * Mathf.Max(0f, _upperBodyAimBoneWeights.x) / total);
            ApplyBoneYaw(_chest, weightedYaw * Mathf.Max(0f, _upperBodyAimBoneWeights.y) / total);
            ApplyBoneYaw(_upperChest, weightedYaw * Mathf.Max(0f, _upperBodyAimBoneWeights.z) / total);
        }

        private void UpdateTurnLean(in PlayerMotionState motion, in PlayerControlState control, float deltaTime)
        {
            bool canLean = !control.IsAiming && control.IsAlive && !control.IsHitReacting && motion.IsMoving && !motion.IsPivoting;
            float target = canLean
                ? -Mathf.Clamp(motion.AngularSpeed / _turnLeanAngularSpeed, -1f, 1f) * _turnLeanMaxAngle
                : 0f;
            _leanAngle = _turnLeanSmoothTime <= 0f
                ? target
                : Mathf.SmoothDampAngle(_leanAngle, target, ref _leanVelocity, _turnLeanSmoothTime, Mathf.Infinity, deltaTime);
            _leanRoot.localRotation = _leanRootBaseRotation * Quaternion.AngleAxis(_leanAngle, Vector3.forward);
        }

        private void ApplyBoneYaw(Transform bone, float yaw)
        {
            if (bone != null)
                bone.rotation = Quaternion.AngleAxis(yaw, _visualRoot.up) * bone.rotation;
        }

        private void UpdatePresentationPose(float deltaTime)
        {
            Vector3 simulationPosition = _playerRoot.position;
            Quaternion simulationRotation = _playerRoot.rotation;
            float duration = Mathf.Max(0.0001f, _presentationTickDuration);

            if (!_hasPresentationPose ||
                (simulationPosition - _presentationToPosition).sqrMagnitude >=
                _presentationSnapDistance * _presentationSnapDistance ||
                Quaternion.Angle(simulationRotation, _presentationToRotation) >= _presentationSnapRotationAngle)
            {
                _presentationFromPosition = simulationPosition;
                _presentationToPosition = simulationPosition;
                _presentationFromRotation = simulationRotation;
                _presentationToRotation = simulationRotation;
                _presentationElapsed = duration;
                _hasPresentationPose = true;
                _visualRoot.SetPositionAndRotation(simulationPosition, simulationRotation);
                return;
            }

            bool positionChanged =
                (simulationPosition - _presentationToPosition).sqrMagnitude > 0.00000001f;
            bool rotationChanged =
                Quaternion.Angle(simulationRotation, _presentationToRotation) > 0.001f;

            if (positionChanged || rotationChanged)
            {
                float currentT = Mathf.Clamp01(_presentationElapsed / duration);
                _presentationFromPosition = Vector3.LerpUnclamped(
                    _presentationFromPosition,
                    _presentationToPosition,
                    currentT);
                _presentationFromRotation = Quaternion.SlerpUnclamped(
                    _presentationFromRotation,
                    _presentationToRotation,
                    currentT);
                _presentationToPosition = simulationPosition;
                _presentationToRotation = simulationRotation;
                _presentationElapsed = 0f;
            }

            _presentationElapsed = Mathf.Min(
                duration,
                _presentationElapsed + Mathf.Max(0f, deltaTime));
            float t = Mathf.Clamp01(_presentationElapsed / duration);
            _visualRoot.SetPositionAndRotation(
                Vector3.LerpUnclamped(_presentationFromPosition, _presentationToPosition, t),
                Quaternion.SlerpUnclamped(_presentationFromRotation, _presentationToRotation, t));
        }

        private void OnDestroy()
        {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            if (_weaponHandle.IsValid())
                Addressables.ReleaseInstance(_weaponHandle);
            if (_characterHandle.IsValid())
                Addressables.ReleaseInstance(_characterHandle);
        }
    }
}
