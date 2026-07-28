using System.Collections.Generic;
using System.Linq;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Core.Network;
using ProjectGame.HotFix.Lobby;
using UnityEngine;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// 管理物品分类、选中数据以及 ItemSelectView 的完整刷新。
    /// </summary>
    [RequireComponent(typeof(ItemSelectView))]
    public class ItemSelectPresenter : BaseLobbyPresenter
    {
        private ItemSelectView _view;
        private readonly Dictionary<ItemCategory, List<ItemSlotData>> _catalog =
            new Dictionary<ItemCategory, List<ItemSlotData>>();

        private bool _isReadonly;
        private LobbyPlayerState _readonlyPlayerData;
        private ItemCategory _currentCategory = ItemCategory.Weapon;
        private int _currentSelectedId = -1;

        /// <summary>
        /// 缓存 View、加载配置目录并绑定界面事件。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            _view = GetComponent<ItemSelectView>();

            InitCatalog();
            _view.OnTabClicked += SwitchCategory;
            _view.OnSlotClicked += HandleSlotSelect;
            _view.OnConfirmClicked += HandleConfirm;
        }

        /// <summary>
        /// 从大厅配置表构建皮肤、武器和道具目录。
        /// </summary>
        private void InitCatalog()
        {
            var skinTable = ConfigManager.Instance.GetTable<Config_Lobby_Skins>();
            var skinList = new List<ItemSlotData>();
            foreach (var pair in skinTable)
            {
                Config_Lobby_Skins config = pair.Value;
                skinList.Add(new ItemSlotData
                {
                    Id = config.SkinID,
                    Name = config.Name,
                    Description = config.Description,
                    Category = ItemCategory.Skin,
                    ResourcePath = config.ModleName,
                    IconPath = config.IconName,
                });
            }
            _catalog[ItemCategory.Skin] = skinList;

            var weaponTable = ConfigManager.Instance.GetTable<Config_Lobby_Weapons>();
            var weaponList = new List<ItemSlotData>();
            foreach (var pair in weaponTable)
            {
                Config_Lobby_Weapons config = pair.Value;
                weaponList.Add(new ItemSlotData
                {
                    Id = config.WeaponID,
                    Name = config.Name,
                    Description = config.Description,
                    Category = ItemCategory.Weapon,
                    ResourcePath = config.ModleName,
                    IconPath = config.IconName,
                });
            }
            _catalog[ItemCategory.Weapon] = weaponList;

            var itemTable = ConfigManager.Instance.GetTable<Config_Lobby_Items>();
            var itemList = new List<ItemSlotData>();
            foreach (var pair in itemTable)
            {
                Config_Lobby_Items config = pair.Value;
                itemList.Add(new ItemSlotData
                {
                    Id = config.ItemID,
                    Name = config.Name,
                    Description = config.Description,
                    Category = ItemCategory.Item,
                    ResourcePath = config.ModleName,
                    IconPath = config.IconName,
                });
            }
            _catalog[ItemCategory.Item] = itemList;
        }

        /// <summary>
        /// 设置本地玩家可编辑模式的入口分类和选中项。
        /// </summary>
        public void EnterWithCategory(ItemCategory category, int selectedId)
        {
            _isReadonly = false;
            _readonlyPlayerData = default;
            _currentCategory = category;
            _currentSelectedId = selectedId;
        }

        /// <summary>
        /// 设置其他玩家只读查看模式及其当前装备数据。
        /// </summary>
        public void EnterAsReadonly(LobbyPlayerState playerData)
        {
            _isReadonly = true;
            _readonlyPlayerData = playerData;
            _currentCategory = ItemCategory.Weapon;
            _currentSelectedId = playerData.WeaponId;
        }

        /// <summary>
        /// 完成基类启动流程。
        /// </summary>
        protected override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// 按当前玩家和分类重新渲染全部选择界面状态。
        /// </summary>
        protected override void RenderView()
        {
            _currentSelectedId = GetSelectedId(_currentCategory);
            FullRefreshUI();
        }

        /// <summary>让返回键复用确认按钮的退出流程并返回 Overview。</summary>
        public override bool TryHandleBackRequest()
        {
            HandleConfirm();
            return true;
        }

        /// <summary>
        /// 切换分类并完整刷新 Tab、信息面板、格子和选中高亮。
        /// </summary>
        private void SwitchCategory(ItemCategory newCategory)
        {
            _currentCategory = newCategory;
            _currentSelectedId = GetSelectedId(_currentCategory);
            FullRefreshUI();

            float position = newCategory switch
            {
                ItemCategory.Skin => 0f,
                ItemCategory.Weapon => 0.33f,
                ItemCategory.Item => 0.67f,
                _ => 0f
            };
            _view.ScrollToPosition(position);
        }

        /// <summary>
        /// 提交本地装备变更并立即完整刷新当前分类。
        /// </summary>
        private void HandleSlotSelect(int slotId)
        {
            if (_isReadonly)
            {
                return;
            }

            _currentSelectedId = slotId;
            LobbyOverviewCoordinator coordinator = LobbyUIManager.Instance.OverviewCoordinator;

            switch (_currentCategory)
            {
                case ItemCategory.Skin:
                    coordinator.RequestCharacterChange(slotId);
                    break;
                case ItemCategory.Weapon:
                    coordinator.RequestWeaponChange(slotId);
                    break;
                case ItemCategory.Item:
                    coordinator.RequestItemChange(slotId);
                    break;
            }

            FullRefreshUI();
        }

        /// <summary>
        /// 取得当前查看玩家在指定分类下已装备的配置 ID。
        /// </summary>
        private int GetSelectedId(ItemCategory category)
        {
            if (_isReadonly)
            {
                return GetReadonlySelectedId(category);
            }

            LobbyPlayerState player = LobbyUIManager.Instance.OverviewCoordinator.LocalPlayerData;
            return category switch
            {
                ItemCategory.Skin => player.CharacterId,
                ItemCategory.Weapon => player.WeaponId,
                ItemCategory.Item => player.ItemId,
                _ => player.WeaponId
            };
        }

        /// <summary>
        /// 取得只读玩家在指定分类下已装备的配置 ID。
        /// </summary>
        private int GetReadonlySelectedId(ItemCategory category)
        {
            return category switch
            {
                ItemCategory.Skin => _readonlyPlayerData.CharacterId,
                ItemCategory.Weapon => _readonlyPlayerData.WeaponId,
                ItemCategory.Item => _readonlyPlayerData.ItemId,
                _ => _readonlyPlayerData.WeaponId
            };
        }

        /// <summary>
        /// 退出选择状态并返回 Overview 页面。
        /// </summary>
        private void HandleConfirm()
        {
            _isReadonly = false;
            _readonlyPlayerData = default;
            LobbyUIManager.Instance.ChangeScreen(LobbyScreenState.Overview);
        }

        /// <summary>
        /// 同步刷新分类高亮、详情内容、格子内容和物品高亮。
        /// </summary>
        private void FullRefreshUI()
        {
            List<ItemSlotData> list = _catalog[_currentCategory];
            if (!list.Any(data => data.Id == _currentSelectedId))
            {
                _currentSelectedId = list[0].Id;
            }

            ItemSlotData currentData = list.First(data => data.Id == _currentSelectedId);
            _view.SetTabHighlight(_currentCategory);
            _view.UpdateItemInfo(currentData);
            _view.RefreshGrid(list, _currentSelectedId);
        }
    }
}
