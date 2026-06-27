using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ProjectGame.HotFix.UI.Lobby
{
    public class OverviewView : MonoBehaviour
    {
        [Header("核心交互区")]
        [SerializeField] private MainActionButtonView _readyButton;
        [SerializeField] private TMP_Text _readyBtnText;
        [SerializeField] private MainActionButtonView _joinButton;

        [Header("个人配装区")]
        [SerializeField] private EquipmentSlotView[] _equipmentSlots;

        [Header("系统辅助区")]
        [SerializeField] private MainActionButtonView _settingsButton;

        public event Action OnReadyClicked;
        public event Action OnJoinedClicked;
        public event Action OnWeaponSelectClicked;
        public event Action OnSettingsClicked;
        public event Action<int> OnEquipmentSlotClicked;


        private void Awake()
        {
            _readyButton.OnClicked += () => OnReadyClicked?.Invoke();
            _joinButton.OnClicked += () => OnWeaponSelectClicked?.Invoke();
            _settingsButton.OnClicked += () => OnSettingsClicked?.Invoke();

            if (_equipmentSlots != null)
            {
                foreach (var slot in _equipmentSlots)
                {
                    slot.OnSlotClicked += (index) => OnEquipmentSlotClicked?.Invoke(index);
                }
            }
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

       
    }
}