using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// 显示一项操作名称、当前按键和更改按钮 
    /// </summary>
    public sealed class InputBindingRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _actionNameText;
        [SerializeField] private TMP_Text _bindingDisplayText;
        [SerializeField] private Button _rebindButton;

        public event Action<int> OnRebindClicked;

        private int _index;

        /// <summary>
        /// 绑定当前行的更改按钮 
        /// </summary>
        private void Awake()
        {
            _rebindButton.onClick.AddListener(HandleRebindClicked);
        }

        /// <summary>
        /// 销毁当前行时解除按钮事件 
        /// </summary>
        private void OnDestroy()
        {
            _rebindButton.onClick.RemoveListener(HandleRebindClicked);
        }

        /// <summary>
        /// 绑定行索引、操作名称和当前按键文本 
        /// </summary>
        public void Bind(int index, string actionName, string bindingDisplay)
        {
            _index = index;
            _actionNameText.text = actionName;
            _bindingDisplayText.text = bindingDisplay;
        }

        /// <summary>
        /// 只刷新当前按键的显示文本 
        /// </summary>
        public void SetBindingDisplay(string bindingDisplay)
        {
            _bindingDisplayText.text = bindingDisplay;
        }

        /// <summary>
        /// 控制本行更改按钮是否可交互 
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            _rebindButton.interactable = interactable;
        }

        /// <summary>
        /// 将本行索引转发给按键列表视图 
        /// </summary>
        private void HandleRebindClicked()
        {
            OnRebindClicked?.Invoke(_index);
        }
    }
}
