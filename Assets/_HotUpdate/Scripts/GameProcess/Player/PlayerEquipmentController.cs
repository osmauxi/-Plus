using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Character;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Core.Session;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectGame.HotFix.Gameplay.Player
{
    /// <summary>
    /// 玩家装备表现控制器 
    /// 第一版只负责根据 WeaponId 加载并挂载武器模型 
    /// 不负责射击、弹药、伤害等战斗逻辑 
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerEquipmentController : MonoBehaviour
    {
        [SerializeField] private PlayerAppearanceController _appearanceController;

        private AsyncOperationHandle<GameObject> _weaponHandle;
        private CancellationTokenSource _loadCts;

        public bool IsWeaponLoaded { get; private set; }
        public int WeaponId { get; private set; } = -1;
        public WeaponView CurrentWeaponView { get; private set; }

        /// <summary>
        /// 加载 Lobby 带入 Gameplay 的初始武器 
        /// 第一版 WeaponId 同样视为固定初始数据 
        /// </summary>
        public async UniTask LoadInitialWeaponAsync(PlayerSessionData sessionData, CancellationToken cancellationToken)
        {
            if (IsWeaponLoaded)
                return;

            PlayerModelView modelView = _appearanceController.ModelView;
            CharacterAnimationBridge animationBridge = _appearanceController.AnimationBridge;

            Config_Lobby_Weapons config = ConfigManager.Instance.GetTable<Config_Lobby_Weapons>()[sessionData.WeaponId];
            EquipmentSlot equipmentSlot = (EquipmentSlot)(byte)config.WeaponSpawnSlot;
            WeaponPose weaponPose = (WeaponPose)(byte)config.WeaponEquipAnim;
            Transform parent = modelView.GetEquipmentSocket(equipmentSlot);

            _loadCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            CancellationToken loadToken = _loadCts.Token;

            AsyncOperationHandle<GameObject> handle = default;

            try
            {
                handle = Addressables.InstantiateAsync(config.ModleName, parent);
                await handle.ToUniTask();

                if (loadToken.IsCancellationRequested)
                {
                    ReleaseLoadedHandle(handle);
                    loadToken.ThrowIfCancellationRequested();
                }

                WeaponView weaponView = handle.Result.GetComponent<WeaponView>();
                weaponView.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                _weaponHandle = handle;
                CurrentWeaponView = weaponView;
                WeaponId = sessionData.WeaponId;
                IsWeaponLoaded = true;

                animationBridge.BindWeapon(weaponView, weaponPose);
                animationBridge.TriggerEquip();

                Debug.Log($"[{nameof(PlayerEquipmentController)}] 初始武器加载完成：WeaponId={WeaponId}");
            }
            finally
            {
                _loadCts?.Dispose();
                _loadCts = null;
            }
        }

        public void ClearWeapon()
        {
            CancelLoad();

            _appearanceController?.AnimationBridge?.UnbindWeapon();

            if (_weaponHandle.IsValid())
                Addressables.ReleaseInstance(_weaponHandle);

            _weaponHandle = default;

            CurrentWeaponView = null;
            WeaponId = -1;
            IsWeaponLoaded = false;
        }

        private void CancelLoad()
        {
            _loadCts?.Cancel();
        }

        private static void ReleaseLoadedHandle(AsyncOperationHandle<GameObject> handle)
        {
            if (!handle.IsValid())
                return;

            if (handle.Status == AsyncOperationStatus.Succeeded)
                Addressables.ReleaseInstance(handle);
            else
                Addressables.Release(handle);
        }

        private void OnDestroy()
        {
            ClearWeapon();
        }
    }
}
