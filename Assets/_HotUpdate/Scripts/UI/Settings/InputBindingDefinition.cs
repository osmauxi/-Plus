using System.Collections.Generic;

namespace ProjectGame.HotFix.Settings
{
    /// <summary>
    /// 描述 Setting 中一行按键与 InputAction Binding 的映射。
    /// </summary>
    public readonly struct InputBindingDefinition
    {
        public readonly string DisplayName;
        public readonly string ActionName;
        public readonly string BindingName;

        /// <summary>
        /// 建立显示名称、Action 名称和 Composite Part 名称的映射。
        /// </summary>
        public InputBindingDefinition(string displayName, string actionName, string bindingName = "")
        {
            DisplayName = displayName;
            ActionName = actionName;
            BindingName = bindingName;
        }
    }

    /// <summary>
    /// 提供 Setting v1.0 固定支持的按键目录。
    /// </summary>
    public static class InputBindingCatalog
    {
        /// <summary>
        /// 按 UI 展示顺序创建九个可修改按键定义。
        /// </summary>
        public static IReadOnlyList<InputBindingDefinition> CreateDefault()
        {
            return new[]
            {
                new InputBindingDefinition("前进", "Move", "up"),
                new InputBindingDefinition("后退", "Move", "down"),
                new InputBindingDefinition("左移", "Move", "left"),
                new InputBindingDefinition("右移", "Move", "right"),
                new InputBindingDefinition("跳跃", "Jump"),
                new InputBindingDefinition("交互", "Interact"),
                new InputBindingDefinition("开火", "Fire"),
                new InputBindingDefinition("瞄准", "Aim"),
                new InputBindingDefinition("换弹", "Reload")
            };
        }
    }
}
