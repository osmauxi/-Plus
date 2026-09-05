using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Settings;
using UnityEngine;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// 管理 Setting 页面全部按键行的初始化和刷新 
    /// </summary>
    public sealed class InputBindingSettingView : MonoBehaviour
    {
        [SerializeField] private InputBindingRowView[] _rows;

        public event Action<int> OnRebindRequested;

        /// <summary>
        /// 绑定所有按键行的点击转发事件 
        /// </summary>
        private void Awake()
        {
            foreach (InputBindingRowView row in _rows)
            {
                row.OnRebindClicked += HandleRebindRequested;
            }
        }

        /// <summary>
        /// 销毁列表时解除全部按键行事件 
        /// </summary>
        private void OnDestroy()
        {
            foreach (InputBindingRowView row in _rows)
            {
                row.OnRebindClicked -= HandleRebindRequested;
            }
        }

        /// <summary>
        /// 使用固定按键目录初始化每一行的名称和顺序 
        /// </summary>
        public void Initialize(IReadOnlyList<InputBindingDefinition> definitions)
        {
            if (_rows.Length != definitions.Count)
            {
                throw new InvalidOperationException(
                    $"按键行数量 {_rows.Length} 与定义数量 {definitions.Count} 不一致 ");
            }

            for (int index = 0; index < _rows.Length; index++)
            {
                _rows[index].Bind(index, definitions[index].DisplayName, string.Empty);
            }
        }

        /// <summary>
        /// 完整刷新每一行当前实际生效的按键名称 
        /// </summary>
        public void Refresh(IReadOnlyList<string> displayStrings)
        {
            if (_rows.Length != displayStrings.Count)
            {
                throw new InvalidOperationException(
                    $"按键行数量 {_rows.Length} 与显示数据数量 {displayStrings.Count} 不一致 ");
            }

            for (int index = 0; index < _rows.Length; index++)
            {
                _rows[index].SetBindingDisplay(displayStrings[index]);
            }
        }

        /// <summary>
        /// 统一控制全部更改按钮是否可交互 
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            foreach (InputBindingRowView row in _rows)
            {
                row.SetInteractable(interactable);
            }
        }

        /// <summary>
        /// 将按键行索引转发给 SettingView 
        /// </summary>
        private void HandleRebindRequested(int index)
        {
            OnRebindRequested?.Invoke(index);
        }
    }
}
