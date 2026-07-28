using System;
using System.IO;
using UnityEngine;

namespace ProjectGame.HotFix.Settings
{
    /// <summary>
    /// 负责 user_settings.json 的读取与写入。
    /// </summary>
    public sealed class SettingSaveService
    {
        private const string FileName = "user_settings.json";

        private readonly string _filePath;

        /// <summary>
        /// 使用持久化目录建立设置文件路径。
        /// </summary>
        public SettingSaveService()
        {
            _filePath = Path.Combine(Application.persistentDataPath, FileName);
        }

        /// <summary>
        /// 从磁盘读取设置，文件缺失或损坏时返回默认值。
        /// </summary>
        public GameUserSettingsData Load()
        {
            if (!File.Exists(_filePath))
            {
                return GameUserSettingsData.CreateDefault();
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                GameUserSettingsData data = JsonUtility.FromJson<GameUserSettingsData>(json);
                if (data == null)
                {
                    throw new InvalidDataException("JSON 未生成有效的设置对象。");
                }

                data.Normalize();
                return data;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"设置文件读取失败，将使用默认设置。Path: {_filePath}\n{exception}");
                return GameUserSettingsData.CreateDefault();
            }
        }

        /// <summary>
        /// 把当前设置格式化后写入持久化目录。
        /// </summary>
        public void Save(GameUserSettingsData data)
        {
            data.Normalize();
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_filePath, json);
        }
    }
}
