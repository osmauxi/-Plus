// ====================================================
// 本文件由工具自动生成，请勿手动修改！
// ====================================================
using System;
using MessagePack;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Config
{
    public static class ConfigRegister
    {
        public static void ParseAndRegister(string addressableName, byte[] bytes)
        {
            switch (addressableName)
            {
                case "Config_Item":
                    var dict_Config_Item = MessagePackSerializer.Deserialize<Dictionary<int, Config_Item>>(bytes);
                    ConfigManager.Instance.RegisterTable(dict_Config_Item);
                    break;
                default:
                    Debug.LogWarning($"[ConfigRegister] 未知的配置表名: {addressableName}，检查Address标签是否打错");
                    break;
            }
        }
    }
}
