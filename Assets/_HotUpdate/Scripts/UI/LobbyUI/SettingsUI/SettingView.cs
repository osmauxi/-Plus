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
        [SerializeField] private Toggle _openMicrophoneToggle;
        [SerializeField] private Button _restoreDefaultButton;
        [SerializeField] private Button _backButton;

        public event Action<SettingVolumeChannel, float> OnVolumeChanged;
        public event Action<VoiceTransmitMode> OnVoiceModeChanged;
        public event Action<int> OnRebindRequested;
        public event Action OnRestoreDefaultRequested;
        public event Action OnBackRequested;

        private void Awake()
        {
            _audioView.OnVolumeChanged += HandleVolumeChanged;
            _inputBindingView.OnRebindRequested += HandleRebindRequested;
            _openMicrophoneToggle.onValueChanged.AddListener(
                HandleOpenMicrophoneChanged);
            _restoreDefaultButton.onClick.AddListener(
                HandleRestoreDefaultRequested);
            _backButton.onClick.AddListener(HandleBackRequested);
        }

        private void OnDestroy()
        {
            _audioView.OnVolumeChanged -= HandleVolumeChanged;
            _inputBindingView.OnRebindRequested -= HandleRebindRequested;
            _openMicrophoneToggle.onValueChanged.RemoveListener(
                HandleOpenMicrophoneChanged);
            _restoreDefaultButton.onClick.RemoveListener(
                HandleRestoreDefaultRequested);
            _backButton.onClick.RemoveListener(HandleBackRequested);
        }

        public void InitializeBindings(
            IReadOnlyList<InputBindingDefinition> definitions)
        {
            _inputBindingView.Initialize(definitions);
        }

        public void RefreshAudio(AudioSettingsData data)
        {
            _audioView.Refresh(data);
        }

        public void RefreshVoice(VoiceSettingsData data)
        {
            _openMicrophoneToggle.SetIsOnWithoutNotify(
                data.TransmitMode == VoiceTransmitMode.OpenMicrophone);
        }

        public void RefreshBindings(IReadOnlyList<string> displayStrings)
        {
            _inputBindingView.Refresh(displayStrings);
        }

        public void ShowDefaultPanel()
        {
            _panelTabs.ShowDefaultTab();
        }

        public void SetRebinding(bool rebinding)
        {
            _audioView.SetInteractable(!rebinding);
            _inputBindingView.SetInteractable(!rebinding);
            _panelTabs.SetInteractable(!rebinding);
            _openMicrophoneToggle.interactable = !rebinding;
            _restoreDefaultButton.interactable = !rebinding;
            _backButton.interactable = !rebinding;

            if (rebinding)
                _rebindOverlay.Show();
            else
                _rebindOverlay.Hide();
        }

        private void HandleVolumeChanged(
            SettingVolumeChannel channel,
            float value)
        {
            OnVolumeChanged?.Invoke(channel, value);
        }

        private void HandleOpenMicrophoneChanged(bool isOn)
        {
            OnVoiceModeChanged?.Invoke(
                isOn
                    ? VoiceTransmitMode.OpenMicrophone
                    : VoiceTransmitMode.PushToTalk);
        }

        private void HandleRebindRequested(int index)
        {
            OnRebindRequested?.Invoke(index);
        }

        private void HandleRestoreDefaultRequested()
        {
            OnRestoreDefaultRequested?.Invoke();
        }

        private void HandleBackRequested()
        {
            OnBackRequested?.Invoke();
        }
    }
}