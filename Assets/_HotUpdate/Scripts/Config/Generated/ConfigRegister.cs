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
                case "Config_Weapon":
                    var dict_Config_Weapon = MessagePackSerializer.Deserialize<Dictionary<int, Config_Weapon>>(bytes);
                    ConfigManager.Instance.RegisterTable(dict_Config_Weapon);
                    break;
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
                case "Config_LocalObjectPool":
                    var dict_Config_LocalObjectPool = MessagePackSerializer.Deserialize<Dictionary<int, Config_LocalObjectPool>>(bytes);
                    ConfigManager.Instance.RegisterTable(dict_Config_LocalObjectPool);
                    break;
                case "Config_LocalVFXPool":
                    var dict_Config_LocalVFXPool = MessagePackSerializer.Deserialize<Dictionary<int, Config_LocalVFXPool>>(bytes);
                    ConfigManager.Instance.RegisterTable(dict_Config_LocalVFXPool);
                    break;
                case "Config_SyncObjectPool":
                    var dict_Config_SyncObjectPool = MessagePackSerializer.Deserialize<Dictionary<int, Config_SyncObjectPool>>(bytes);
                    ConfigManager.Instance.RegisterTable(dict_Config_SyncObjectPool);
                    break;
                case "Config_RoomTemplate":
                    var dict_Config_RoomTemplate = MessagePackSerializer.Deserialize<Dictionary<int, Config_RoomTemplate>>(bytes);
                    ConfigManager.Instance.RegisterTable(dict_Config_RoomTemplate);
                    break;
                default:
                    Debug.LogWarning($"[ConfigRegister] 未知的配置表名: {addressableName}，检查Address标签是否打错");
                    break;
            }
        }
    }
}
