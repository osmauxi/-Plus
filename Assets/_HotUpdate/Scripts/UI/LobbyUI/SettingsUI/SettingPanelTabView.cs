using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// 保存一个分类按钮与对应设置 Panel 的映射 
    /// </summary>
    [Serializable]
    public sealed class SettingPanelTabEntry
    {
        [SerializeField] private Button _button;
        [SerializeField] private Graphic _highlightTarget;
        [SerializeField] private GameObject _panel;

        public Button Button => _button;
        public Graphic HighlightTarget => _highlightTarget;
        public GameObject Panel => _panel;
    }

    /// <summary>
    /// 管理 Setting 分类按钮，并保证任意时刻只显示一个内容 Panel 
    /// </summary>
    public sealed class SettingPanelTabView : MonoBehaviour
    {
        [SerializeField] private SettingPanelTabEntry[] _tabs;
        [SerializeField] private int _defaultTabIndex;
        [SerializeField] private Color _normalColor = new Color(0.15f, 0.19f, 0.24f, 0.98f);
        [SerializeField] private Color _selectedColor = new Color(1f, 0.58f, 0.12f, 1f);

        private UnityAction[] _tabCallbacks;
        private int _currentTabIndex = -1;

        public int CurrentTabIndex => _currentTabIndex;

        /// <summary>
        /// 为每个分类按钮建立固定索引回调，并显示默认 Panel 
        /// </summary>
        private void Awake()
        {
            _tabCallbacks = new UnityAction[_tabs.Length];
            for (int index = 0; index < _tabs.Length; index++)
            {
                int capturedIndex = index;
                _tabCallbacks[index] = () => ShowTab(capturedIndex);
                _tabs[index].Button.onClick.AddListener(_tabCallbacks[index]);
            }

            ShowDefaultTab();
        }

        /// <summary>
        /// 销毁分类栏时解除所有运行时按钮回调 
        /// </summary>
        private void OnDestroy()
        {
            for (int index = 0; index < _tabs.Length; index++)
            {
                _tabs[index].Button.onClick.RemoveListener(_tabCallbacks[index]);
            }
        }

        /// <summary>
        /// 显示默认分类，并隐藏其余内容 Panel 
        /// </summary>
        public void ShowDefaultTab()
        {
            ShowTab(_defaultTabIndex);
        }

        /// <summary>
        /// 显示指定索引的 Panel，并同步全部分类按钮高亮 
        /// </summary>
        public void ShowTab(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= _tabs.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(tabIndex), tabIndex, null);
            }

            _currentTabIndex = tabIndex;
            for (int index = 0; index < _tabs.Length; index++)
            {
                bool selected = index == tabIndex;
                _tabs[index].Panel.SetActive(selected);
                _tabs[index].HighlightTarget.color = selected ? _selectedColor : _normalColor;
            }
        }

        /// <summary>
        /// 统一控制所有分类按钮是否可以交互 
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            foreach (SettingPanelTabEntry tab in _tabs)
            {
                tab.Button.interactable = interactable;
            }
        }
    }
}
