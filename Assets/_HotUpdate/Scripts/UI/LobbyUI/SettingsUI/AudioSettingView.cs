using System;
using ProjectGame.HotFix.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// 管理三项音量 Slider、百分比文本和交互事件。
    /// </summary>
    public sealed class AudioSettingView : MonoBehaviour
    {
        [Header("主音量")]
        [SerializeField] private Slider _masterSlider;
        [SerializeField] private TMP_Text _masterPercentText;

        [Header("音乐音量")]
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private TMP_Text _musicPercentText;

        [Header("音效音量")]
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private TMP_Text _sfxPercentText;

        public event Action<SettingVolumeChannel, float> OnVolumeChanged;

        /// <summary>
        /// 配置 Slider 范围并绑定三个音量事件。
        /// </summary>
        private void Awake()
        {
            ConfigureSlider(_masterSlider);
            ConfigureSlider(_musicSlider);
            ConfigureSlider(_sfxSlider);

            _masterSlider.onValueChanged.AddListener(HandleMasterChanged);
            _musicSlider.onValueChanged.AddListener(HandleMusicChanged);
            _sfxSlider.onValueChanged.AddListener(HandleSfxChanged);
        }

        /// <summary>
        /// 销毁视图时解除 Slider 事件。
        /// </summary>
        private void OnDestroy()
        {
            _masterSlider.onValueChanged.RemoveListener(HandleMasterChanged);
            _musicSlider.onValueChanged.RemoveListener(HandleMusicChanged);
            _sfxSlider.onValueChanged.RemoveListener(HandleSfxChanged);
        }

        /// <summary>
        /// 无事件地刷新三个 Slider 与百分比文本。
        /// </summary>
        public void Refresh(AudioSettingsData data)
        {
            _masterSlider.SetValueWithoutNotify(data.MasterVolume);
            _musicSlider.SetValueWithoutNotify(data.MusicVolume);
            _sfxSlider.SetValueWithoutNotify(data.SfxVolume);
            SetPercentText(_masterPercentText, data.MasterVolume);
            SetPercentText(_musicPercentText, data.MusicVolume);
            SetPercentText(_sfxPercentText, data.SfxVolume);
        }

        /// <summary>
        /// 统一启用或禁用三个音量 Slider。
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            _masterSlider.interactable = interactable;
            _musicSlider.interactable = interactable;
            _sfxSlider.interactable = interactable;
        }

        /// <summary>
        /// 响应主音量修改并刷新对应百分比。
        /// </summary>
        private void HandleMasterChanged(float value)
        {
            SetPercentText(_masterPercentText, value);
            OnVolumeChanged?.Invoke(SettingVolumeChannel.Master, value);
        }

        /// <summary>
        /// 响应音乐音量修改并刷新对应百分比。
        /// </summary>
        private void HandleMusicChanged(float value)
        {
            SetPercentText(_musicPercentText, value);
            OnVolumeChanged?.Invoke(SettingVolumeChannel.Music, value);
        }

        /// <summary>
        /// 响应音效音量修改并刷新对应百分比。
        /// </summary>
        private void HandleSfxChanged(float value)
        {
            SetPercentText(_sfxPercentText, value);
            OnVolumeChanged?.Invoke(SettingVolumeChannel.Sfx, value);
        }

        /// <summary>
        /// 将 Slider 固定配置为 0 到 1 的连续范围。
        /// </summary>
        private static void ConfigureSlider(Slider slider)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
        }

        /// <summary>
        /// 将线性音量格式化为整数百分比。
        /// </summary>
        private static void SetPercentText(TMP_Text target, float value)
        {
            target.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }
    }
}
