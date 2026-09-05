using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Settings;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// 协调 Setting 页面、本地设置、音频应用和 Input System 改键 
    /// </summary>
    [RequireComponent(typeof(SettingView))]
    public sealed class SettingPresenter : BaseLobbyPresenter
    {
        [Header("Setting 视图")]
        [SerializeField] private SettingView _view;

        [Header("运行时设置资源")]
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private InputActionAsset _inputActions;

        private SettingSaveService _saveService;
        private AudioSettingService _audioService;
        private InputRebindService _inputRebindService;
        private IReadOnlyList<InputBindingDefinition> _bindingDefinitions;
        private GameUserSettingsData _settings;
        private int _lastRebindCanceledFrame = -1;

        /// <summary>
        /// 创建 Setting 服务、绑定界面事件并恢复本地设置 
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _saveService = new SettingSaveService();
            _audioService = new AudioSettingService(_audioMixer);
            _inputRebindService = new InputRebindService(_inputActions);
            _bindingDefinitions = InputBindingCatalog.CreateDefault();

            _view.InitializeBindings(_bindingDefinitions);
            BindViewEvents();
            LoadAndApplySettings();
        }

        /// <summary>
        /// 销毁 Presenter 时解除 View 事件并释放改键操作 
        /// </summary>
        protected override void OnDestroy()
        {
            UnbindViewEvents();
            _inputRebindService.Dispose();
            base.OnDestroy();
        }

        /// <summary>
        /// 离开 Setting 时取消改键、保存数据并关闭交互 
        /// </summary>
        public override void Sleep()
        {
            _inputRebindService.CancelRebind();
            _view.SetRebinding(false);
            SaveSettings();
            base.Sleep();
        }

        /// <summary>
        /// 应用退出前保存当前本地设置 
        /// </summary>
        private void OnApplicationQuit()
        {
            SaveSettings();
        }

        /// <summary>
        /// 每次唤醒 Setting 时重新读取、应用并刷新全部设置 
        /// </summary>
        protected override void RenderView()
        {
            LoadAndApplySettings();
            _view.SetRebinding(false);
            _view.ShowDefaultPanel();
            RefreshView();
        }

        /// <summary>取消进行中的改键；其余情况复用返回按钮流程回到 Overview </summary>
        public override bool TryHandleBackRequest()
        {
            if (_inputRebindService.IsRebinding)
            {
                _inputRebindService.CancelRebind();
                return true;
            }

            if (_lastRebindCanceledFrame == Time.frameCount)
                return true;

            HandleBackRequested();
            return true;
        }

        /// <summary>
        /// 订阅 SettingView 抛出的全部用户操作 
        /// </summary>
        private void BindViewEvents()
        {
            _view.OnVolumeChanged += HandleVolumeChanged;
            _view.OnRebindRequested += HandleRebindRequested;
            _view.OnRestoreDefaultRequested += HandleRestoreDefaultRequested;
            _view.OnBackRequested += HandleBackRequested;
        }

        /// <summary>
        /// 解除 SettingView 的全部用户操作事件 
        /// </summary>
        private void UnbindViewEvents()
        {
            _view.OnVolumeChanged -= HandleVolumeChanged;
            _view.OnRebindRequested -= HandleRebindRequested;
            _view.OnRestoreDefaultRequested -= HandleRestoreDefaultRequested;
            _view.OnBackRequested -= HandleBackRequested;
        }

        /// <summary>
        /// 读取本地 JSON，并立即应用音频与按键 Override 
        /// </summary>
        private void LoadAndApplySettings()
        {
            _settings = _saveService.Load();
            _audioService.Apply(_settings.Audio);

            if (!_inputRebindService.ApplyBindingOverrides(_settings.InputBindingOverridesJson))
            {
                _settings.InputBindingOverridesJson = string.Empty;
                SaveSettings();
            }
        }

        /// <summary>
        /// 完整刷新音量控件和全部按键行 
        /// </summary>
        private void RefreshView()
        {
            _view.RefreshAudio(_settings.Audio);
            _view.RefreshBindings(
                _inputRebindService.GetBindingDisplayStrings(_bindingDefinitions));
        }

        /// <summary>
        /// 实时应用音量修改并立即保存到本地 
        /// </summary>
        private void HandleVolumeChanged(SettingVolumeChannel channel, float value)
        {
            switch (channel)
            {
                case SettingVolumeChannel.Master:
                    _settings.Audio.MasterVolume = value;
                    break;
                case SettingVolumeChannel.Music:
                    _settings.Audio.MusicVolume = value;
                    break;
                case SettingVolumeChannel.Sfx:
                    _settings.Audio.SfxVolume = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(channel), channel, null);
            }

            _audioService.Apply(_settings.Audio);
            SaveSettings();
        }

        /// <summary>
        /// 锁定 Setting 交互并启动指定按键行的交互式改键 
        /// </summary>
        private void HandleRebindRequested(int index)
        {
            if (_inputRebindService.IsRebinding)
            {
                return;
            }

            _view.SetRebinding(true);
            _inputRebindService.StartRebind(
                _bindingDefinitions[index],
                HandleRebindCompleted,
                HandleRebindCanceled);
        }

        /// <summary>
        /// 保存成功改键后的 Override 并刷新按键列表 
        /// </summary>
        private void HandleRebindCompleted()
        {
            _settings.InputBindingOverridesJson =
                _inputRebindService.SaveBindingOverridesAsJson();
            SaveSettings();
            _view.SetRebinding(false);
            RefreshView();
        }

        /// <summary>
        /// 取消改键后恢复 Setting 交互和原按键显示 
        /// </summary>
        private void HandleRebindCanceled()
        {
            _lastRebindCanceledFrame = Time.frameCount;
            _view.SetRebinding(false);
            RefreshView();
        }

        /// <summary>
        /// 恢复默认音量和默认按键并立即保存、刷新 
        /// </summary>
        private void HandleRestoreDefaultRequested()
        {
            _inputRebindService.CancelRebind();
            _settings = GameUserSettingsData.CreateDefault();
            _inputRebindService.RestoreDefaults();
            _audioService.Apply(_settings.Audio);
            SaveSettings();
            _view.SetRebinding(false);
            RefreshView();
        }

        /// <summary>
        /// 取消可能存在的改键、保存设置并返回 Overview 
        /// </summary>
        private void HandleBackRequested()
        {
            _inputRebindService.CancelRebind();
            SaveSettings();
            LobbyUIManager.Instance.ChangeScreen(LobbyScreenState.Overview);
        }

        /// <summary>
        /// 将内存中的设置数据写入 user_settings.json 
        /// </summary>
        private void SaveSettings()
        {
            _saveService.Save(_settings);
        }
    }
}
