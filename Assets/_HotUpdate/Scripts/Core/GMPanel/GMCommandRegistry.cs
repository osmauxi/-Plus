using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Core.DebugTools
{
    /// <summary>
    /// Model层，负责注册和管理所有的GM调试指令数据
    /// </summary>
    public static class GMCommandRegistry
    {
        private static readonly Dictionary<string, List<DebugCommand>> _allCommands = new Dictionary<string, List<DebugCommand>>();

        public static void Register(string category, string name, Action callback)
        {
            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(name) || callback == null) 
                return;

            if (!_allCommands.ContainsKey(category))
            {
                _allCommands[category] = new List<DebugCommand>(5);
            }

            if (_allCommands[category].Exists(c => c.Name == name))
            {
                Debug.LogWarning($"[GM] 重复注册了指令: {category} -> {name}");
                return;
            }

            _allCommands[category].Add(new DebugCommand(category, name, callback));
        }

        public static Dictionary<string, List<DebugCommand>> GetAllCommands() => _allCommands;

        public static List<DebugCommand> GetCommandsByCategory(string category)
        {
            if (_allCommands.TryGetValue(category, out var list))
                return list;
            return null;
        }
    }
}