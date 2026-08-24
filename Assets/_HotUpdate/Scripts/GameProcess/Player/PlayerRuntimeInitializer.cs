using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Core.Session;
using ProjectGame.HotFix.Gameplay.Pooling;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player
{
    /// <summary>
    /// 单个 PlayerRuntimeRoot 的本地初始化编排器。
    ///
    /// NetworkObject 在任意 Peer 上 Spawn 后：
    /// OwnerClientId → PlayerSessionData → Character → Weapon。
    ///
    /// 不负责生成玩家，也不负责网络同步玩家状态。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(PlayerAppearanceController))]
    [RequireComponent(typeof(PlayerEquipmentController))]
    public sealed class PlayerRuntimeInitializer : NetworkBehaviour, IPoolable
    {
        private PlayerAppearanceController _appearanceController;
        private PlayerEquipmentController _equipmentController;
        private CancellationTokenSource _initializeCts;

        public bool IsInitializing { get; private set; }
        public bool IsInitialized { get; private set; }
        /// <summary>本次 Network Spawn 生命周期是否已经确定初始化失败。</summary>
        public bool HasInitializationFailed { get; private set; }
        /// <summary>用于 Ready 屏障和日志诊断的简短失败原因；完整异常仍写入 Console。</summary>
        public string InitializationError { get; private set; }

        private void Awake()
        {
            _appearanceController = GetComponent<PlayerAppearanceController>();
            _equipmentController = GetComponent<PlayerEquipmentController>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            CancelInitialization();

            _initializeCts = new CancellationTokenSource();
            InitializePlayerAsync(_initializeCts.Token).Forget();
        }

        public override void OnNetworkDespawn()
        {
            // NetworkObject 已经不再代表一个有效玩家，
            // 先终止这个 Spawn 生命周期中的异步初始化。
            CancelInitialization();

            base.OnNetworkDespawn();
        }

        private async UniTask InitializePlayerAsync(CancellationToken cancellationToken)
        {
            IsInitializing = true;
            IsInitialized = false;
            HasInitializationFailed = false;
            InitializationError = null;

            try
            {
                if (!GameSessionContext.TryGetPlayer(OwnerClientId, out PlayerSessionData sessionData))
                    throw new InvalidOperationException($"找不到玩家会话数据：ClientId={OwnerClientId}");

                // 身体必须先存在，武器才有 EquipmentSocket 可以挂载。
                await _appearanceController.LoadAsync(sessionData, cancellationToken);
                await _equipmentController.LoadInitialWeaponAsync(sessionData, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                IsInitialized = true;

                Debug.Log($"[{nameof(PlayerRuntimeInitializer)}] 玩家初始化完成：ClientId={OwnerClientId}，CharacterId={sessionData.CharacterId}，WeaponId={sessionData.WeaponId}");
            }
            catch (OperationCanceledException)
            {
                // Despawn / 回池导致的正常取消，不需要打印错误。
            }
            catch (Exception exception)
            {
                HasInitializationFailed = true;
                InitializationError = exception.Message;
                Debug.LogError($"[{nameof(PlayerRuntimeInitializer)}] 玩家初始化失败：ClientId={OwnerClientId}\n{exception}");
            }
            finally
            {
                IsInitializing = false;
            }
        }

        /// <summary>
        /// 对象从 SyncObjectPool 再次租出时，保证没有上一轮残留状态。
        /// 此时 NetworkObject 还没有 Spawn。
        /// </summary>
        public void OnRentFromPool()
        {
            CancelInitialization();
            ClearRuntimeState();
        }

        /// <summary>
        /// NetworkObject 已完成 OnNetworkDespawn 后执行。
        /// 清理当前玩家动态加载的本地表现资源。
        /// </summary>
        public void OnReturnToPool()
        {
            CancelInitialization();
            ClearRuntimeState();
        }

        private void ClearRuntimeState()
        {
            // 必须先卸武器。
            // Weapon 的 AnimationBridge 位于 Character 上，
            // 如果 Character 先释放，就无法正常 UnbindWeapon。
            _equipmentController?.ClearWeapon();
            _appearanceController?.Clear();

            IsInitializing = false;
            IsInitialized = false;
            HasInitializationFailed = false;
            InitializationError = null;
        }

        private void CancelInitialization()
        {
            if (_initializeCts == null)
                return;

            _initializeCts.Cancel();
            _initializeCts.Dispose();
            _initializeCts = null;
        }

        private void OnDestroy()
        {
            CancelInitialization();
            ClearRuntimeState();
        }
    }
}
