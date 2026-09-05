using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// [View] 统一物品选择面板
    /// 上部分 InfPanel：展示当前选中项目名称、描述、属性条 (通用)
    /// 下部分 ChosePanel：分类 Tab + ScrollView 网格，ItemSlotView 组成
    /// 外加一个确认按钮返回概览
    /// </summary>
    public class ItemSelectView : MonoBehaviour
    {
        [Header("信息面板")]
        [SerializeField] private TMP_Text _itemNameText;
        [SerializeField] private TMP_Text _itemDescriptionText;

        [Header("选择面板")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Transform _gridContent;         
        [SerializeField] private ItemSlotView _itemSlotPrefab;    

        [Header("分类 Tab 按钮")]
        [SerializeField] private MainActionButtonView _tabSkin;
        [SerializeField] private MainActionButtonView _tabWeapon;
        [SerializeField] private MainActionButtonView _tabItem;

        [Header("确认按钮")]
        [SerializeField] private MainActionButtonView _confirmButton;

        // 事件分发
        public event Action<ItemCategory> OnTabClicked;
        public event Action<int> OnSlotClicked;
        public event Action OnConfirmClicked;

        // 格子对象池
        private List<ItemSlotView> _activeSlots = new List<ItemSlotView>();
        private Stack<ItemSlotView> _inactivePool = new Stack<ItemSlotView>();
        /// <summary>绑定分类、物品和确认按钮事件 </summary>
        private void Awake()
        {
            _tabSkin.OnClicked += () => OnTabClicked?.Invoke(ItemCategory.Skin);
            _tabWeapon.OnClicked += () => OnTabClicked?.Invoke(ItemCategory.Weapon);
            _tabItem.OnClicked += () => OnTabClicked?.Invoke(ItemCategory.Item);
            _confirmButton.OnClicked += () => OnConfirmClicked?.Invoke();
        }

        #region InfPanel 刷新

        /// <summary>
        /// 更新信息面板显示
        /// </summary>
        public void UpdateItemInfo(ItemSlotData data)
        {
            _itemNameText.text = data.Name;
            _itemDescriptionText.text = data.Description;
        }

        #endregion

        #region ChosePanel 格子管理 (View 只负责池化与排布，不碰数据逻辑)

        /// <summary>
        /// 传入当前分类的所有数据，View 负责实例化/回收格子并 Bind
        /// 使用对象池避免重复 Destroy/Instantiate
        /// </summary>
        public void RefreshGrid(List<ItemSlotData> items, int selectedId)
        {
            // 1. 超出部分移入对象池 (SetActive false，不销毁)
            while (_activeSlots.Count > items.Count)
            {
                var last = _activeSlots[_activeSlots.Count - 1];
                _activeSlots.RemoveAt(_activeSlots.Count - 1);
                last.OnClicked -= HandleSlotClick;
                last.gameObject.SetActive(false);
                _inactivePool.Push(last);
            }

            // 2. 更新已有 + 不足时从池取或新建
            for (int i = 0; i < items.Count; i++)
            {
                ItemSlotView slot;
                if (i < _activeSlots.Count)
                {
                    // 复用已激活的格子
                    slot = _activeSlots[i];
                }
                else
                {
                    // 优先从池中取，否则 Instantiate
                    if (_inactivePool.Count > 0)
                    {
                        slot = _inactivePool.Pop();
                        slot.transform.SetParent(_gridContent);
                    }
                    else
                    {
                        slot = Instantiate(_itemSlotPrefab, _gridContent);
                    }
                    _activeSlots.Add(slot);
                }

                // 确保格子可见
                slot.gameObject.SetActive(true);
                slot.Bind(items[i]);
                slot.SetHighlight(items[i].Id == selectedId);
                slot.OnClicked -= HandleSlotClick; // 防重复
                slot.OnClicked += HandleSlotClick;
            }
        }

        /// <summary>把物品格点击事件转发给 Presenter </summary>
        private void HandleSlotClick(int id)
        {
            OnSlotClicked?.Invoke(id);
        }

        /// <summary>
        /// 滑动 ScrollView 到指定分类的偏移 (由 Presenter 调用)
        /// </summary>
        public void ScrollToPosition(float normalizedPosition)
        {
            _scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(normalizedPosition);
        }

        /// <summary>
        /// 更新分类 Tab 高亮
        /// </summary>
        public void SetTabHighlight(ItemCategory category)
        {
            _tabSkin.SetHighlight(category == ItemCategory.Skin);
            _tabWeapon.SetHighlight(category == ItemCategory.Weapon);
            _tabItem.SetHighlight(category == ItemCategory.Item);
        }

        #endregion
    }
}
