using System;
using System.Collections.Generic;
using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Core.Events;
using ProjectGame.HotFix.Gameplay.Events;
using ProjectGame.HotFix.Gameplay.Player;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using ProjectGame.HotFix.Gameplay.Runtime;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProjectGame.HotFix.Gameplay.CameraSystem
{
    /// <summary>
    /// Gameplay Camera Effect 的表现调度器 
    ///
    /// 外部只通过 CameraEffects.Play / Set 发出请求；
    /// 本类负责把一个语义效果拆成 Shake、Zoom 等具体表现 
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraEffectManager : MonoBehaviour, IGameRuntimeService
    {
        #region 单个Effect可配置参数

        [Serializable]
        private sealed class EffectDefinition
        {
            [Tooltip("Gameplay侧使用的语义ID")]
            public CameraEffectId Id;

            [Header("震动")]
            [Tooltip("是否通过 Cinemachine Impulse 播放镜头震动 ")]
            public bool EnableShake;
            [Tooltip("默认 Impulse 强度，最终值还会乘以请求的Intensity")]
            [Min(0f)] public float ShakeAmplitude = 1f;

            public CameraShakeDirectionMode ShakeDirectionMode = CameraShakeDirectionMode.Random;
            [Tooltip("方向性震动中，水平位移方向的权重。")]
            [Range(0f, 1f)]
            public float DirectionWeight = 1f;

            [Tooltip("额外加入的随机扰动比例，避免连续射击震动完全机械一致。")]
            [Range(0f, 1f)]
            public float RandomWeight = 0.15f;

            [Header("Zoom效果")]
            [Tooltip("是否修改观察高度，瞬时请求使用Kick，持续请求使用Modifier ")]
            public bool EnableZoom;
            [Tooltip("对 PlayerCameraController 基础 ViewHeight 的附加值 负数表示镜头拉近 ")]
            public float ZoomOffset;
            [Tooltip("Zoom 效果进入或瞬时冲击建立时的平滑时间 ")]
            [Min(0f)] public float ZoomInSmoothTime = 0.08f;
            [Tooltip("Zoom 效果退出或瞬时冲击释放时的平滑时间 ")]
            [Min(0f)] public float ZoomOutSmoothTime = 0.12f;

            [Header("FOV效果")]
            [Tooltip("是否修改虚拟相机 FOV；瞬时请求使用 Kick，持续请求使用 Modifier ")]
            public bool EnableFov;

            [Tooltip("对基础 FOV 的附加值 负数收窄视野，正数扩大视野 ")]
            public float FovOffset;

            [Tooltip("FOV 效果进入或瞬时冲击建立时的平滑时间 ")]
            [Min(0f)] public float FovInSmoothTime = 0.05f;
            [Tooltip("FOV 效果退出或瞬时冲击释放时的平滑时间 ")]
            [Min(0f)] public float FovOutSmoothTime = 0.12f;
        }

        #endregion

        #region Inspector Configuration

        [Header("场景引用")]
        [Tooltip("接收 Zoom、FOV 与 Aim 构图命令的 Gameplay Camera Controller ")]
        [SerializeField] private PlayerCameraController _cameraController;
        [Tooltip("产生 Cinemachine Impulse 的震动源 ")]
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        [Header("效果配置")]
        [Tooltip("把语义效果 ID 映射为 Shake、Zoom 与 FOV 的组合参数 ")]
        [SerializeField] private List<EffectDefinition> _effects = new();

        #endregion

        #region Runtime State

        private readonly Dictionary<CameraEffectId, EffectDefinition> _effectLookup = new();

        private IDisposable _playSubscription;
        private IDisposable _setSubscription;

        private PlayerSyncController _localPlayerSyncController;
        private uint _lastShotSequence;
        private uint _lastHitSequence;
        private bool _wasAiming;
        private bool _wasDead;
        private bool _hasLocalPlayerBaseline;

        /// <summary>当前服务是否已完成引用解析、配置索引和事件订阅 </summary>
        public bool IsInitialized { get; private set; }

        #endregion

        #region Service Lifecycle

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return UniTask.CompletedTask;

            cancellationToken.ThrowIfCancellationRequested();
            ResolveReferences();
            BuildLookup();

            _playSubscription = LocalEvents.Subscribe<CameraEffectPlayRequestedEvent>(HandlePlayRequested);
            _setSubscription = LocalEvents.Subscribe<CameraEffectSetRequestedEvent>(HandleSetRequested);

            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        private void Update()
        {
            if (!IsInitialized)
                return;

            PlayerRuntime localPlayer = PlayerManager.Instance != null
                ? PlayerManager.Instance.LocalPlayer
                : null;
            PlayerSyncController syncController = localPlayer != null
                ? localPlayer.GetComponent<PlayerSyncController>()
                : null;

            if (_localPlayerSyncController != syncController)
            {
                ReleaseLocalPlayerEffects();
                _localPlayerSyncController = syncController;

                if (_localPlayerSyncController != null)
                    CaptureLocalPlayerBaseline();

                return;
            }

            if (_localPlayerSyncController == null)
                return;

            if (!_hasLocalPlayerBaseline)
            {
                CaptureLocalPlayerBaseline();
                return;
            }

            ConsumeLocalPlayerCameraEvents();
        }

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            ReleaseLocalPlayerEffects();
            ClearPersistentEffects();
            DisposeSubscriptions();
            _effectLookup.Clear();
            IsInitialized = false;
            return UniTask.CompletedTask;
        }

        private void OnDestroy()
        {
            ClearPersistentEffects();
            DisposeSubscriptions();
        }

        private void DisposeSubscriptions()
        {
            _playSubscription?.Dispose();
            _setSubscription?.Dispose();
            _playSubscription = null;
            _setSubscription = null;
        }

        #endregion

        #region Local Player Event Bridge

        /// <summary>
        /// 消费本地玩家同步时间线上的确定性事件序号与状态边沿 
        /// Shot/Hit 序号支持预测回滚与 uint 回绕；Aim/Dead 只响应状态变化 
        /// </summary>
        private void ConsumeLocalPlayerCameraEvents()
        {
            var action = _localPlayerSyncController.ActionState;
            var control = _localPlayerSyncController.ControlState;

            if (action.ShotSequence != _lastShotSequence)
            {
                if (TickMath.IsNewer(action.ShotSequence, _lastShotSequence))
                    CameraEffects.Play(CameraEffectId.RifleFire);

                _lastShotSequence = action.ShotSequence;
            }

            if (action.HitSequence != _lastHitSequence)
            {
                if (TickMath.IsNewer(action.HitSequence, _lastHitSequence))
                    CameraEffects.Play(CameraEffectId.PlayerHit);

                _lastHitSequence = action.HitSequence;
            }

            bool isDead = control.IsDead;
            bool isAiming = control.IsAiming && !isDead;

            if (isAiming != _wasAiming)
                CameraEffects.Set(CameraEffectId.Aim, isAiming);

            if (isDead && !_wasDead)
                CameraEffects.Play(CameraEffectId.PlayerDeath);

            _wasAiming = isAiming;
            _wasDead = isDead;
        }

        private void CaptureLocalPlayerBaseline()
        {
            var action = _localPlayerSyncController.ActionState;
            var control = _localPlayerSyncController.ControlState;

            _lastShotSequence = action.ShotSequence;
            _lastHitSequence = action.HitSequence;
            _wasDead = control.IsDead;
            _wasAiming = control.IsAiming && !_wasDead;
            _hasLocalPlayerBaseline = true;

            if (_wasAiming)
                CameraEffects.Set(CameraEffectId.Aim, true);
        }

        private void ReleaseLocalPlayerEffects()
        {
            if (_hasLocalPlayerBaseline && _wasAiming)
                CameraEffects.Set(CameraEffectId.Aim, false);

            _localPlayerSyncController = null;
            _lastShotSequence = 0;
            _lastHitSequence = 0;
            _wasAiming = false;
            _wasDead = false;
            _hasLocalPlayerBaseline = false;
        }

        #endregion

        #region Effect Dispatch

        private void HandlePlayRequested(CameraEffectPlayRequestedEvent eventData)
        {
            if (!_effectLookup.TryGetValue(eventData.Id, out EffectDefinition effect))
                return;

            float intensity = Mathf.Max(0f, eventData.Intensity);

            if (effect.EnableShake)
                PlayShake(effect, eventData, intensity);

            if (effect.EnableZoom)
            {
                _cameraController.PlayCameraZoomKick(effect.ZoomOffset * intensity,effect.ZoomInSmoothTime,effect.ZoomOutSmoothTime);
            }

            if (effect.EnableFov)
            {
                _cameraController.PlayCameraFovKick(
                    effect.FovOffset * intensity,
                    effect.FovInSmoothTime,
                    effect.FovOutSmoothTime);
            }
        }

        private void HandleSetRequested(CameraEffectSetRequestedEvent eventData)
        {
            // Aim 构图属于语义状态，即使 Inspector 暂时漏配 Aim 效果也必须正常开关 
            if (eventData.Id == CameraEffectId.Aim)
                _cameraController.SetAimComposition(eventData.Active);

            if (!_effectLookup.TryGetValue(eventData.Id, out EffectDefinition effect))
                return;

            float intensity = Mathf.Max(0f, eventData.Intensity);

            if (effect.EnableZoom)
            {
                float zoomOffset = eventData.Active ? effect.ZoomOffset * intensity : 0f;
                float zoomSmoothTime = eventData.Active ? effect.ZoomInSmoothTime : effect.ZoomOutSmoothTime;

                _cameraController.SetCameraZoomModifier(
                    eventData.Id,
                    zoomOffset,
                    zoomSmoothTime);
            }

            if (effect.EnableFov)
            {
                float fovOffset = eventData.Active ? effect.FovOffset * intensity : 0f;
                float fovSmoothTime = eventData.Active ? effect.FovInSmoothTime : effect.FovOutSmoothTime;

                _cameraController.SetCameraFovModifier(
                    eventData.Id,
                    fovOffset,
                    fovSmoothTime);
            }
        }

        private void PlayShake(EffectDefinition effect,CameraEffectPlayRequestedEvent eventData,float intensity)
        {
            if (_impulseSource == null)
                return;

            float amplitude =
                effect.ShakeAmplitude *
                intensity;

            if (effect.ShakeDirectionMode ==
                CameraShakeDirectionMode.Random)
            {
                _impulseSource.GenerateImpulseWithForce(
                    amplitude);

                return;
            }

            Vector3 direction =
                ResolveShakeDirection(
                    effect,
                    eventData);

            // DirectionWeight 可以控制“明确方向”与普通震屏之间的占比。
            direction *=
                amplitude *
                effect.DirectionWeight;

            _impulseSource.GenerateImpulseWithVelocity(
                direction);
        }

        private Vector3 ResolveShakeDirection(EffectDefinition effect,CameraEffectPlayRequestedEvent eventData)
        {
            if (effect.ShakeDirectionMode == CameraShakeDirectionMode.Random ||
                !eventData.HasDirection)
            {
                return Random.insideUnitSphere.normalized;
            }

            Vector3 worldDirection = eventData.Direction;
            worldDirection.y = 0f;

            if (worldDirection.sqrMagnitude <= 0.0001f)
                return Random.insideUnitSphere.normalized;

            worldDirection.Normalize();

            if (effect.ShakeDirectionMode == CameraShakeDirectionMode.Recoil)
                worldDirection = -worldDirection;

            // Cinemachine Impulse 的方向最终需要与玩家屏幕看到的左右前后保持一致，
            // 因此将世界方向转换到当前 WorldCamera 的局部空间。
            Vector3 localDirection =
                _cameraController.WorldCamera.transform
                    .InverseTransformDirection(worldDirection);

            localDirection.Normalize();

            if (effect.RandomWeight > 0f)
            {
                Vector3 random =
                    Random.insideUnitSphere.normalized;

                localDirection =
                    Vector3.Lerp(
                        localDirection,
                        random,
                        effect.RandomWeight).normalized;
            }

            return localDirection;
        }
        private void ClearPersistentEffects()
        {
            if (_cameraController == null)
                return;

            foreach (EffectDefinition effect in _effectLookup.Values)
            {
                if (effect.EnableZoom)
                {
                    _cameraController.SetCameraZoomModifier(
                        effect.Id,
                        0f,
                        effect.ZoomOutSmoothTime);
                }

                if (effect.EnableFov)
                {
                    _cameraController.SetCameraFovModifier(
                        effect.Id,
                        0f,
                        effect.FovOutSmoothTime);
                }
            }

            _cameraController.SetAimComposition(false);
        }

        #endregion

        #region Reference and Configuration Resolution

        private void ResolveReferences()
        {
            if (_cameraController == null)
                _cameraController = GetComponent<PlayerCameraController>();

            if (_cameraController == null)
                throw new InvalidOperationException($"{nameof(CameraEffectManager)} 找不到 {nameof(PlayerCameraController)} ");

            if (_impulseSource == null)
                _impulseSource = GetComponent<CinemachineImpulseSource>();

            if (_impulseSource == null)
                _impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        }

        private void BuildLookup()
        {
            _effectLookup.Clear();

            if (_effects == null)
                return;

            for (int i = 0; i < _effects.Count; i++)
            {
                EffectDefinition effect = _effects[i];
                if (effect == null || effect.Id == CameraEffectId.None)
                    continue;

                if (_effectLookup.ContainsKey(effect.Id))
                {
                    Debug.LogWarning(
                        $"[{nameof(CameraEffectManager)}] 效果 {effect.Id} 重复配置，" +
                        "将使用列表中最后一项 ",
                        this);
                }

                _effectLookup[effect.Id] = effect;
            }
        }

        #endregion
    }
}
