using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ProjectGame.HotFix.UI.Lobby
{
    public class OverviewView : MonoBehaviour
    {
        [Header("核心交互区")]
        [SerializeField] private Button _readyButton;
        [SerializeField] private TMP_Text _readyBtnText;

        [Header("个人配装区")]
        [SerializeField] private Button _weaponSelectButton;
        [SerializeField] private TMP_Text _currentWeaponText;

        [Header("系统辅助区")]
        [SerializeField] private Button _settingsButton;

        public event Action OnReadyClicked;
        public event Action OnWeaponSelectClicked;
        public event Action OnSettingsClicked;

        private void Awake()
        {
            // 内部消化 UI 监听，转化为纯逻辑事件
            _readyButton.onClick.AddListener(() => OnReadyClicked?.Invoke());
            _weaponSelectButton.onClick.AddListener(() => OnWeaponSelectClicked?.Invoke());
            _settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
        }

        public void SetReadyState(bool isReady)
        {
            if (isReady)
            {
                _readyBtnText.text = "取消准备";
                _readyBtnText.color = new Color(1f, 0.8f, 0.2f);
            }
            else
            {
                _readyBtnText.text = "准备就绪";
                _readyBtnText.color = Color.white;
            }
        }

        public void SetWeaponInfo(int weaponId)
        {
            _currentWeaponText.text = $"当前配装: 武器 [{weaponId}]";
        }
    }
}