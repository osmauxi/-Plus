using System;
using TMPro;
using UnityEngine;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>负责 Overview 固定 UI 控件的显示和事件转发。</summary>
    public class OverviewView : MonoBehaviour
    {
        [Header("核心交互区")]
        [SerializeField] private MainActionButtonView _readyButton;
        [SerializeField] private TMP_Text _readyBtnText;

        [Header("加入房间控制区")]
        [SerializeField] private MainActionButtonView _joinGameToggleBtn;
        [SerializeField] private TMP_Text _joinGameBtnText;

        [Header("个人配装区")]
        [SerializeField] private EquipmentSlotView[] _equipmentSlots;

        [Header("系统辅助区")]
        [SerializeField] private MainActionButtonView _settingsButton;

        /// <summary>开始游戏或准备按钮点击事件。</summary>
        public event Action OnReadyClicked;
        /// <summary>加入房间切换按钮点击事件。</summary>
        public event Action OnJoinGameToggle;
        /// <summary>设置按钮点击事件。</summary>
        public event Action OnSettingsClicked;
        /// <summary>装备槽点击事件，索引依次为皮肤、武器和道具。</summary>
        public event Action<int> OnEquipmentSlotClicked;

        /// <summary>绑定 Overview 页面内固定控件的点击事件。</summary>
        private void Awake()
        {
            _readyButton.OnClicked += () => OnReadyClicked?.Invoke();
            _joinGameToggleBtn.OnClicked += () => OnJoinGameToggle?.Invoke();
            _settingsButton.OnClicked += () => OnSettingsClicked?.Invoke();

            for (int i = 0; i < _equipmentSlots.Length; i++)
            {
                int index = i;
                _equipmentSlots[i].OnSlotClicked += (_) => OnEquipmentSlotClicked?.Invoke(index);
            }
        }

        /// <summary>设置准备按钮的文本和颜色。</summary>
        public void SetStartGameBtnState(string text, Color color)
        {
            _readyBtnText.text = text;
            _readyBtnText.color = color;
        }

        /// <summary>把准备按钮恢复为默认开始游戏状态。</summary>
        public void ResetStartGameBtnToDefault()
        {
            _readyBtnText.text = "开始游戏";
            _readyBtnText.color = Color.white;
        }

        /// <summary>更新加入房间按钮文本。</summary>
        public void SetJoinGameBtnText(string text)
        {
            _joinGameBtnText.text = text;
        }

        /// <summary>更新加入房间按钮文本颜色。</summary>
        public void SetJoinGameBtnColor(Color color)
        {
            _joinGameBtnText.color = color;
        }
    }
}
