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
                case "Config_Lobby_Skins":
                    var dict_Config_Lobby_Skins = MessagePackSerializer.Deserialize<Dictionary<int, Config_Lobby_Skins>>(bytes);
                    ConfigManager.Instance.RegisterTable(dict_Config_Lobby_Skins);
                    break;
                case "Config_Lobby_Weapons":
                    var dict_Config_Lobby_Weapons = MessagePackSerializer.Deserialize<Dictionary<int, Config_Lobby_Weapons>>(bytes);
                    ConfigManager.Instance.RegisterTable(dict_Config_Lobby_Weapons);
                    break;
                case "Config_Lobby_Items":
                    var dict_Config_Lobby_Items = MessagePackSerializer.Deserialize<Dictionary<int, Config_Lobby_Items>>(bytes);
                    ConfigManager.Instance.RegisterTable(dict_Config_Lobby_Items);
                    break;
                default:
                    Debug.LogWarning($"[ConfigRegister] 未知的配置表名: {addressableName}，检查Address标签是否打错");
                    break;
            }
        }
    }
}
