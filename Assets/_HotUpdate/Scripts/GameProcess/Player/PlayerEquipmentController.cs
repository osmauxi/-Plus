using System;
using System.Collections.Generic;
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
            {
                if (WeaponId == sessionData.WeaponId)
                    return;

                throw new InvalidOperationException($"初始武器已经加载：Current={WeaponId}，Requested={sessionData.WeaponId}");
            }

            if (_loadCts != null)
                throw new InvalidOperationException("武器模型正在加载中，不能重复调用 ");

            if (_appearanceController == null || !_appearanceController.IsLoaded)
                throw new InvalidOperationException("角色模型尚未加载，无法挂载武器 ");

            PlayerModelView modelView = _appearanceController.ModelView;
            CharacterAnimationBridge animationBridge = _appearanceController.AnimationBridge;

            Config_Lobby_Weapons config = GetWeaponConfig(sessionData.WeaponId);
            EquipmentSlot equipmentSlot = ParseEquipmentSlot(config.WeaponSpawnSlot, nameof(config.WeaponSpawnSlot));
            WeaponPose weaponPose = ParseWeaponPose(config.WeaponEquipAnim);
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

                if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
                {
                    ReleaseLoadedHandle(handle);
                    throw new InvalidOperationException($"武器 WeaponId={sessionData.WeaponId} 加载失败：{config.ModleName}");
                }

                GameObject weaponInstance = handle.Result;

                if (!weaponInstance.TryGetComponent(out WeaponView weaponView))
                {
                    ReleaseLoadedHandle(handle);
                    throw new InvalidOperationException($"武器 WeaponId={sessionData.WeaponId} 缺少 {nameof(WeaponView)} ");
                }

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

        private static Config_Lobby_Weapons GetWeaponConfig(int weaponId)
        {
            Dictionary<int, Config_Lobby_Weapons> table = ConfigManager.Instance.GetTable<Config_Lobby_Weapons>();

            if (table != null && table.TryGetValue(weaponId, out Config_Lobby_Weapons config))
                return config;

            throw new KeyNotFoundException($"不存在武器配置：WeaponId={weaponId}");
        }

        private static EquipmentSlot ParseEquipmentSlot(int value, string fieldName)
        {
            if (value < byte.MinValue || value > byte.MaxValue || !Enum.IsDefined(typeof(EquipmentSlot), (byte)value))
                throw new ArgumentOutOfRangeException(fieldName, value, "无效的装备槽位 ");

            EquipmentSlot slot = (EquipmentSlot)(byte)value;

            if (slot == EquipmentSlot.None)
                throw new ArgumentOutOfRangeException(fieldName, value, "装备槽位不能为 None ");

            return slot;
        }

        private static WeaponPose ParseWeaponPose(int value)
        {
            if (value < byte.MinValue || value > byte.MaxValue || !Enum.IsDefined(typeof(WeaponPose), (byte)value))
                throw new ArgumentOutOfRangeException(nameof(value), value, "无效的武器动画姿势 ");

            return (WeaponPose)(byte)value;
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