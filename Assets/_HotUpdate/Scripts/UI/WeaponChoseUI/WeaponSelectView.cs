using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ProjectGame.HotFix.UI.Lobby
{
    public class WeaponSelectView : MonoBehaviour
    {
        [Header("交互按钮")]
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _confirmButton;

        [Header("信息面板 (TextMeshPro)")]
        [SerializeField] private TMP_Text _weaponNameText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private Image _powerBar; // 进度条图片
        [SerializeField] private Image _speedBar;

        public event Action OnPrevClicked;
        public event Action OnNextClicked;
        public event Action OnConfirmClicked;

        private void Awake()
        {
            _prevButton.onClick.AddListener(() => OnPrevClicked?.Invoke());
            _nextButton.onClick.AddListener(() => OnNextClicked?.Invoke());
            _confirmButton.onClick.AddListener(() => OnConfirmClicked?.Invoke());
        }

        public void UpdateWeaponInfo(string name, string desc, float power01, float speed01)
        {
            _weaponNameText.text = name;
            _descriptionText.text = desc;
            _powerBar.fillAmount = power01;
            _speedBar.fillAmount = speed01;
        }
    }
}