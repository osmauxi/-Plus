using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectGame.HotFix.Settings
{
    /// <summary>
    /// 负责 Gameplay ActionMap 的显示、交互式改键和 Override 序列化。
    /// </summary>
    public sealed class InputRebindService : IDisposable
    {
        private const string GameplayMapName = "Gameplay";

        private readonly InputActionAsset _inputActions;
        private InputActionRebindingExtensions.RebindingOperation _operation;
        private InputAction _rebindingAction;
        private bool _actionWasEnabled;
        private Action _onCompleted;
        private Action _onCanceled;

        public bool IsRebinding => _operation != null;

        /// <summary>
        /// 缓存必须由场景绑定的 InputActionAsset。
        /// </summary>
        public InputRebindService(InputActionAsset inputActions)
        {
            _inputActions = inputActions;
        }

        /// <summary>
        /// 返回指定按键当前实际生效的可读名称。
        /// </summary>
        public string GetBindingDisplayString(InputBindingDefinition definition)
        {
            InputAction action = FindAction(definition.ActionName);
            int bindingIndex = FindBindingIndex(action, definition.BindingName);
            return action.GetBindingDisplayString(bindingIndex);
        }

        /// <summary>
        /// 返回所有按键定义当前实际生效的可读名称。
        /// </summary>
        public IReadOnlyList<string> GetBindingDisplayStrings(IReadOnlyList<InputBindingDefinition> definitions)
        {
            var displayStrings = new string[definitions.Count];
            for (int index = 0; index < definitions.Count; index++)
            {
                displayStrings[index] = GetBindingDisplayString(definitions[index]);
            }

            return displayStrings;
        }

        /// <summary>
        /// 启动一次键鼠交互式改键，并排除鼠标移动与 ESC。
        /// </summary>
        public void StartRebind(
            InputBindingDefinition definition,
            Action onCompleted,
            Action onCanceled)
        {
            if (IsRebinding)
            {
                return;
            }

            _rebindingAction = FindAction(definition.ActionName);
            int bindingIndex = FindBindingIndex(_rebindingAction, definition.BindingName);
            _actionWasEnabled = _rebindingAction.enabled;
            _onCompleted = onCompleted;
            _onCanceled = onCanceled;

            _rebindingAction.Disable();
            _operation = _rebindingAction.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("<Mouse>/position")
                .WithControlsExcluding("<Mouse>/delta")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnComplete(HandleCompleted)
                .OnCancel(HandleCanceled)
                .Start();
        }

        /// <summary>
        /// 取消当前改键操作并保留原 Binding。
        /// </summary>
        public void CancelRebind()
        {
            _operation?.Cancel();
        }

        /// <summary>
        /// 清空所有 Binding Override 并恢复资产默认按键。
        /// </summary>
        public void RestoreDefaults()
        {
            CancelRebind();
            _inputActions.RemoveAllBindingOverrides();
        }

        /// <summary>
        /// 先清空旧 Override，再从本地 JSON 恢复按键设置。
        /// </summary>
        public bool ApplyBindingOverrides(string json)
        {
            CancelRebind();
            _inputActions.RemoveAllBindingOverrides();

            try
            {
                if (!string.IsNullOrEmpty(json))
                {
                    _inputActions.LoadBindingOverridesFromJson(json);
                }

                return true;
            }
            catch (Exception exception)
            {
                _inputActions.RemoveAllBindingOverrides();
                Debug.LogWarning($"按键 Override JSON 损坏，已恢复默认按键。\n{exception}");
                return false;
            }
        }

        /// <summary>
        /// 把当前所有 Binding Override 序列化为 JSON。
        /// </summary>
        public string SaveBindingOverridesAsJson()
        {
            return _inputActions.SaveBindingOverridesAsJson();
        }

        /// <summary>
        /// 释放可能仍在等待输入的 RebindingOperation。
        /// </summary>
        public void Dispose()
        {
            if (IsRebinding)
            {
                _operation.Cancel();
            }

            DisposeOperation();
        }

        /// <summary>
        /// 完成交互式改键并通知 Presenter 保存结果。
        /// </summary>
        private void HandleCompleted(InputActionRebindingExtensions.RebindingOperation operation)
        {
            Action callback = _onCompleted;
            FinishOperation();
            callback?.Invoke();
        }

        /// <summary>
        /// 取消交互式改键并通知 Presenter 恢复界面。
        /// </summary>
        private void HandleCanceled(InputActionRebindingExtensions.RebindingOperation operation)
        {
            Action callback = _onCanceled;
            FinishOperation();
            callback?.Invoke();
        }

        /// <summary>
        /// 恢复 Action 原启用状态并释放操作对象。
        /// </summary>
        private void FinishOperation()
        {
            if (_actionWasEnabled)
            {
                _rebindingAction.Enable();
            }

            DisposeOperation();
        }

        /// <summary>
        /// Dispose 当前操作并清空一次性回调引用。
        /// </summary>
        private void DisposeOperation()
        {
            _operation?.Dispose();
            _operation = null;
            _rebindingAction = null;
            _onCompleted = null;
            _onCanceled = null;
        }

        /// <summary>
        /// 从 Gameplay ActionMap 中取得必须存在的 Action。
        /// </summary>
        private InputAction FindAction(string actionName)
        {
            return _inputActions.FindActionMap(GameplayMapName, true).FindAction(actionName, true);
        }

        /// <summary>
        /// 根据 Composite Part 名称或键鼠分组定位目标 Binding。
        /// </summary>
        private static int FindBindingIndex(InputAction action, string bindingName)
        {
            for (int index = 0; index < action.bindings.Count; index++)
            {
                InputBinding binding = action.bindings[index];
                if (!string.IsNullOrEmpty(bindingName))
                {
                    if (binding.isPartOfComposite && binding.name == bindingName)
                    {
                        return index;
                    }

                    continue;
                }

                if (!binding.isComposite && !binding.isPartOfComposite &&
                    binding.groups.Contains("Keyboard&Mouse"))
                {
                    return index;
                }
            }

            throw new InvalidOperationException(
                $"Action '{action.name}' 找不到 Binding '{bindingName}'。");
        }
    }
}
