using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// 汇总 Setting 页面子视图并向 Presenter 转发用户操作。
    /// </summary>
    public sealed class SettingView : MonoBehaviour
    {
        [SerializeField] private AudioSettingView _audioView;
        [SerializeField] private InputBindingSettingView _inputBindingView;
        [SerializeField] private SettingPanelTabView _panelTabs;
        [SerializeField] private RebindOverlayView _rebindOverlay;
        [SerializeField] private Button _restoreDefaultButton;
        [SerializeField] private Button _backButton;

        public event Action<SettingVolumeChannel, float> OnVolumeChanged;
        public event Action<int> OnRebindRequested;
        public event Action OnRestoreDefaultRequested;
        public event Action OnBackRequested;

        /// <summary>
        /// 绑定所有 Setting 子视图和操作按钮事件。
        /// </summary>
        private void Awake()
        {
            _audioView.OnVolumeChanged += HandleVolumeChanged;
            _inputBindingView.OnRebindRequested += HandleRebindRequested;
            _restoreDefaultButton.onClick.AddListener(HandleRestoreDefaultRequested);
            _backButton.onClick.AddListener(HandleBackRequested);
        }

        /// <summary>
        /// 销毁 Setting 页面时解除所有子视图事件。
        /// </summary>
        private void OnDestroy()
        {
            _audioView.OnVolumeChanged -= HandleVolumeChanged;
            _inputBindingView.OnRebindRequested -= HandleRebindRequested;
            _restoreDefaultButton.onClick.RemoveListener(HandleRestoreDefaultRequested);
            _backButton.onClick.RemoveListener(HandleBackRequested);
        }

        /// <summary>
        /// 使用按键目录初始化固定数量的按键行。
        /// </summary>
        public void InitializeBindings(IReadOnlyList<InputBindingDefinition> definitions)
        {
            _inputBindingView.Initialize(definitions);
        }

        /// <summary>
        /// 刷新三项音量控件。
        /// </summary>
        public void RefreshAudio(AudioSettingsData data)
        {
            _audioView.Refresh(data);
        }

        /// <summary>
        /// 刷新全部按键行的当前按键文本。
        /// </summary>
        public void RefreshBindings(IReadOnlyList<string> displayStrings)
        {
            _inputBindingView.Refresh(displayStrings);
        }

        /// <summary>
        /// 每次进入 Setting 时显示配置的默认分类 Panel。
        /// </summary>
        public void ShowDefaultPanel()
        {
            _panelTabs.ShowDefaultTab();
        }

        /// <summary>
        /// 切换改键等待遮罩并锁定或恢复其他设置交互。
        /// </summary>
        public void SetRebinding(bool rebinding)
        {
            _audioView.SetInteractable(!rebinding);
            _inputBindingView.SetInteractable(!rebinding);
            _panelTabs.SetInteractable(!rebinding);
            _restoreDefaultButton.interactable = !rebinding;
            _backButton.interactable = !rebinding;

            if (rebinding)
            {
                _rebindOverlay.Show();
            }
            else
            {
                _rebindOverlay.Hide();
            }
        }

        /// <summary>
        /// 将音量事件转发给 Presenter。
        /// </summary>
        private void HandleVolumeChanged(SettingVolumeChannel channel, float value)
        {
            OnVolumeChanged?.Invoke(channel, value);
        }

        /// <summary>
        /// 将按键行请求转发给 Presenter。
        /// </summary>
        private void HandleRebindRequested(int index)
        {
            OnRebindRequested?.Invoke(index);
        }

        /// <summary>
        /// 将恢复默认按钮请求转发给 Presenter。
        /// </summary>
        private void HandleRestoreDefaultRequested()
        {
            OnRestoreDefaultRequested?.Invoke();
        }

        /// <summary>
        /// 将返回按钮请求转发给 Presenter。
        /// </summary>
        private void HandleBackRequested()
        {
            OnBackRequested?.Invoke();
        }
    }
}
