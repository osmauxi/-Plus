using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Character;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Core.Network;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectGame.HotFix.Lobby
{
    /// <summary>
    /// 大厅模型渲染器 只负责把指定玩家状态渲染到指定展位，不感知 UI 或 NGO 
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AvatarResManager : MonoBehaviour
    {
        [SerializeField] private LobbyStandLayout _layout;

        private sealed class StationRuntime
        {
            public ulong ClientId = ulong.MaxValue;
            public int CharacterId = -1;
            public int WeaponId = -1;
            public int ItemId = -1;
            public int Revision;

            public ulong DesiredClientId = ulong.MaxValue;
            public int DesiredCharacterId = -1;
            public int DesiredWeaponId = -1;
            public int DesiredItemId = -1;

            public GameObject CharacterInstance;
            public WeaponView WeaponView;
            public GameObject ItemInstance;

            public AsyncOperationHandle<GameObject> CharacterHandle;
            public AsyncOperationHandle<GameObject> WeaponHandle;
            public AsyncOperationHandle<GameObject> ItemHandle;

            public PlayerModelView ModelView;
            public CharacterAnimationBridge AnimationBridge;
        }

        private StationRuntime[] _stations;

        private void Awake()
        {
            _stations = new StationRuntime[_layout.Count];
            for (int i = 0; i < _stations.Length; i++)
                _stations[i] = new StationRuntime();
        }

        /// <summary>
        /// 销毁时取消异步结果并释放全部 Addressables 实例 
        /// </summary>
        private void OnDestroy()
        {
            foreach (StationRuntime station in _stations)
            {
                station.Revision++;
                ReleaseStation(station);
            }
        }

        /// <summary>
        /// 将可空玩家状态应用到指定展位模型，这是唯一的对外方法接口
        /// </summary>
        public void ApplyStandState(int standIndex, LobbyPlayerState? state)
        {
            StationRuntime station = _stations[standIndex];
            if (!state.HasValue)
            {
                station.Revision++;
                ReleaseStation(station);
                return;
            }

            //进行一次快速判断，如果玩家状态与当前展位状态一致则不做任何操作 
            LobbyPlayerState value = state.Value;
            bool isCurrent = station.DesiredClientId == value.ClientId
                && station.DesiredCharacterId == value.CharacterId
                && station.DesiredWeaponId == value.WeaponId
                && station.DesiredItemId == value.ItemId;

            if (isCurrent)
                return;

            station.DesiredClientId = value.ClientId;
            station.DesiredCharacterId = value.CharacterId;
            station.DesiredWeaponId = value.WeaponId;
            station.DesiredItemId = value.ItemId;
            int revision = ++station.Revision;
            //用户点击是即时的，异步加载可能会延迟，所以这里不等待异步结果（.Forget()），直接丢弃 
            UpdateStationAsync(standIndex, value, revision).Forget();
        }

        /// <summary>
        /// 按角色、武器、道具顺序异步更新一个展位 
        /// </summary>
        private async UniTask UpdateStationAsync(int index, LobbyPlayerState state, int revision)
        {
            StationRuntime station = _stations[index];
            Transform anchor = _layout.GetPlayerSpawnPos(index);

            bool characterChanged = station.ClientId != state.ClientId
                || station.CharacterId != state.CharacterId
                || station.CharacterInstance == null;

            //玩家角色发生变化时，先释放旧角色和装备，再加载新角色 
            if (characterChanged)
            {
                ReleaseEquipment(station);
                ReleaseCharacter(station);

                AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(ResolveSkinAddress(state.CharacterId), anchor);
                bool loadSucceeded = await TryCompleteLoadAsync(handle,$"角色 CharacterId={state.CharacterId}", station, revision);

                //如果在异步加载过程中展位被更新，则直接释放加载结果并返回 
                if (revision != station.Revision)
                {
                    ReleaseLoadedHandle(handle);
                    return;
                }

                if (!loadSucceeded)
                {
                    ReleaseLoadedHandle(handle);
                    return;
                }

                station.CharacterHandle = handle;
                station.CharacterInstance = handle.Result;
                station.ClientId = state.ClientId;
                station.CharacterId = state.CharacterId;
                station.WeaponId = -1;
                station.ItemId = -1;
                BindCharacterComponents(station);
            }

            if (station.WeaponId != state.WeaponId)
                await UpdateWeaponAsync(station, state.WeaponId, revision);

            //await等待武器加载完成后，可能已经切换了展位状态，所以需要再次检查修订号，防止继续加载错误的Item
            if (revision != station.Revision)
                return;

            if (station.ItemId != state.ItemId)
                await UpdateItemAsync(station, state.ItemId, revision);
        }

        /// <summary>
        /// 替换指定展位角色当前挂载的武器 
        /// </summary>
        private async UniTask UpdateWeaponAsync(StationRuntime station, int weaponId, int revision)
        {
            ReleaseWeapon(station);

            Config_Lobby_Weapons config = GetWeaponConfig(weaponId);
            EquipmentSlot equipmentSlot = ParseEquipmentSlot(config.WeaponSpawnSlot, nameof(config.WeaponSpawnSlot));
            WeaponPose weaponPose = ParseWeaponPose(config.WeaponEquipAnim);

            Transform parent = station.ModelView.GetEquipmentSocket(equipmentSlot);

            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(config.ModleName, parent);
            bool loadSucceeded = await TryCompleteLoadAsync(handle, $"武器 WeaponId={weaponId}", station, revision);

            if (revision != station.Revision)
            {
                ReleaseLoadedHandle(handle);
                return;
            }

            if (!loadSucceeded)
            {
                ReleaseLoadedHandle(handle);
                return;
            }

            WeaponView weaponView = handle.Result.GetComponent<WeaponView>();

            if (weaponView == null)
            {
                ReleaseLoadedHandle(handle);
                throw new InvalidOperationException($"武器 WeaponId={weaponId} 缺少 {nameof(WeaponView)} ");
            }

            weaponView.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);


            station.WeaponHandle = handle;
            station.WeaponView = weaponView;
            station.WeaponId = weaponId;

            station.AnimationBridge.BindWeapon(weaponView, weaponPose);
            station.AnimationBridge.TriggerEquip();
        }

        /// <summary>
        /// 替换指定展位角色当前挂载的道具 
        /// </summary>
        private async UniTask UpdateItemAsync(StationRuntime station, int itemId, int revision)
        {
            ReleaseItem(station);
            Config_Lobby_Items config = GetItemConfig(itemId);
            EquipmentSlot equipmentSlot = ParseEquipmentSlot(
                config.ItemSpawnSlot,
                nameof(config.ItemSpawnSlot));
            Transform parent = station.ModelView.GetEquipmentSocket(equipmentSlot);

            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(
                config.ModleName, parent);
            bool loadSucceeded = await TryCompleteLoadAsync(handle, $"道具 ItemId={itemId}", station, revision);

            if (revision != station.Revision)
            {
                ReleaseLoadedHandle(handle);
                return;
            }

            if (!loadSucceeded)
            {
                ReleaseLoadedHandle(handle);
                return;
            }

            station.ItemHandle = handle;
            station.ItemInstance = handle.Result;
            station.ItemInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            station.ItemId = itemId;
        }

        private static WeaponPose ParseWeaponPose(int value)
        {
            if (value < byte.MinValue || value > byte.MaxValue || !Enum.IsDefined(typeof(WeaponPose), (byte)value))
                throw new ArgumentOutOfRangeException(nameof(value), value, "无效的武器动画姿势 ");

            return (WeaponPose)value;
        }

        /// <summary>
        /// 通过皮肤配置解析角色 Addressable 地址 
        /// </summary>
        private static string ResolveSkinAddress(int skinId)
        {
            Dictionary<int, Config_Lobby_Skins> table = ConfigManager.Instance.GetTable<Config_Lobby_Skins>();
            if (table.TryGetValue(skinId, out Config_Lobby_Skins config))
                return config.ModleName;

            throw new KeyNotFoundException($"不存在大厅皮肤配置：{skinId}");
        }

        /// <summary>
        /// 取得指定武器 ID 的大厅展示配置 
        /// </summary>
        private static Config_Lobby_Weapons GetWeaponConfig(int weaponId)
        {
            Dictionary<int, Config_Lobby_Weapons> table = ConfigManager.Instance.GetTable<Config_Lobby_Weapons>();
            if (table.TryGetValue(weaponId, out Config_Lobby_Weapons config))
                return config;

            throw new KeyNotFoundException($"不存在大厅武器配置：{weaponId}");
        }

        /// <summary>
        /// 取得指定道具 ID 的大厅展示配置 
        /// </summary>
        private static Config_Lobby_Items GetItemConfig(int itemId)
        {
            Dictionary<int, Config_Lobby_Items> table = ConfigManager.Instance.GetTable<Config_Lobby_Items>();
            if (table.TryGetValue(itemId, out Config_Lobby_Items config))
                return config;

            throw new KeyNotFoundException($"不存在大厅道具配置：{itemId}");
        }

        /// <summary>
        /// 把配置整数转换成有效的角色装备槽位 
        /// </summary>
        private static EquipmentSlot ParseEquipmentSlot(int value, string fieldName)
        {
            if (value < byte.MinValue || value > byte.MaxValue || !Enum.IsDefined(typeof(EquipmentSlot), (byte)value))
                throw new ArgumentOutOfRangeException(fieldName, value, "无效的装备槽位 ");

            EquipmentSlot slot = (EquipmentSlot)(byte)value;

            if (slot == EquipmentSlot.None)
                throw new ArgumentOutOfRangeException(fieldName, value, "装备槽位不能为 None ");

            return slot;
        }

        /// <summary>
        /// 校验武器配置中的 Animator 姿势整数 
        /// </summary>
        private static int ValidateEquipmentPose(int value)
        {
            if (value < 0 || value > (int)WeaponPose.Pistol)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $"武器动画必须位于 {(int)WeaponPose.Rifle}~{(int)WeaponPose.Pistol}");
            }

            return value;
        }

        /// <summary>
        /// 缓存角色预制件上的装备挂点组件 
        /// </summary>
        private static void BindCharacterComponents(StationRuntime station)
        {
            if (!station.CharacterInstance.TryGetComponent(out PlayerModelView modelView))
                throw new InvalidOperationException(
                    $"角色预制体 {station.CharacterInstance.name} 缺少 {nameof(PlayerModelView)} ");

            if (modelView.AnimationBridge == null)
                throw new InvalidOperationException(
                    $"角色预制体 {station.CharacterInstance.name} 的 {nameof(PlayerModelView)} 没有配置 {nameof(CharacterAnimationBridge)} ");

            station.ModelView = modelView;
            station.AnimationBridge = modelView.AnimationBridge;
        }
        

        /// <summary>
        /// 释放一个展位的全部资源并重置身份 
        /// </summary>
        private static void ReleaseStation(StationRuntime station)
        {
            ReleaseEquipment(station);
            ReleaseCharacter(station);
            station.ClientId = ulong.MaxValue;
            station.DesiredClientId = ulong.MaxValue;
            station.DesiredCharacterId = -1;
            station.DesiredWeaponId = -1;
            station.DesiredItemId = -1;
        }

        /// <summary>
        /// 释放一个展位的武器和道具资源 
        /// </summary>
        private static void ReleaseEquipment(StationRuntime station)
        {
            ReleaseWeapon(station);
            ReleaseItem(station);
        }

        /// <summary>
        /// 释放武器实例并重置武器状态 
        /// </summary>
        private static void ReleaseWeapon(StationRuntime station)
        {
            station.AnimationBridge?.UnbindWeapon();

            if (station.WeaponHandle.IsValid())
                Addressables.ReleaseInstance(station.WeaponHandle);

            station.WeaponHandle = default;
            station.WeaponView = null;
            station.WeaponId = -1;
        }

        /// <summary>
        /// 释放道具实例并重置道具状态 
        /// </summary>
        private static void ReleaseItem(StationRuntime station)
        {
            if (station.ItemHandle.IsValid())
                Addressables.ReleaseInstance(station.ItemHandle);

            station.ItemHandle = default;
            station.ItemInstance = null;
            station.ItemId = -1;
        }

        /// <summary>
        /// 释放角色实例并清理角色挂点 
        /// </summary>
        private static void ReleaseCharacter(StationRuntime station)
        {
            if (station.CharacterHandle.IsValid())
                Addressables.ReleaseInstance(station.CharacterHandle);

            station.CharacterHandle = default;
            station.CharacterInstance = null;
            station.CharacterId = -1;

            station.ModelView = null;
            station.AnimationBridge = null;
        }

        /// <summary>
        /// 释放尚未接管或已经过期的异步实例句柄 
        /// </summary>
        private static void ReleaseLoadedHandle(AsyncOperationHandle<GameObject> handle)
        {
            if (!handle.IsValid())
                return;

            //Addressable加载成功了，Gameobject已经被实例化了，直接释放实例，防止内存泄漏 
            //如果加载失败了，Gameobject没有被实例化，直接释放句柄 
            if (handle.Status == AsyncOperationStatus.Succeeded)
                Addressables.ReleaseInstance(handle);
            else
                Addressables.Release(handle);
        }

        /// <summary>
        /// 等待 Addressables 实例化结束并把资源异常记录为可定位日志 
        /// </summary>
        private static async UniTask<bool> TryCompleteLoadAsync(AsyncOperationHandle<GameObject> handle,
            string resourceDescription, StationRuntime station, int revision)
        {
            try
            {
                await handle.ToUniTask();
                return handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded;
            }
            catch (Exception exception)
            {
                // 场景卸载会释放其跟踪的实例句柄；已被替换/销毁的展位不再消费这个结果。
                if (station.Revision == revision)
                    Debug.LogError($"[AvatarResManager] {resourceDescription} 加载失败：{exception.Message}");
                return false;
            }
        }
    }
}
