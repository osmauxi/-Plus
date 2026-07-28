using System;

namespace ProjectGame.HotFix.Core.DebugTools
{
    /// <summary>
    /// 单条GM调试指令数据体
    /// </summary>
    public class DebugCommand
    {
        public string Category { get; private set; } //分类
        public string Name { get; private set; }     //指令名称
        public Action Callback { get; private set; } //点击后触发的回调方法

        public DebugCommand(string category, string name, Action callback)
        {
            Category = category;
            Name = name;
            Callback = callback;
        }
    }
}