using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectGame.HotFix.Config
{
    public class ConfigManager
    {
        public static ConfigManager instance;
        public static ConfigManager Instance => instance ??= new ConfigManager();
        //所有热更数据项的存储字典
        private readonly Dictionary<Type, object> _allConfigs = new Dictionary<Type, object>();

        private IDataParser _baseDataParser;
        private IDataParser _modDataParser;

        public void Init() 
        {
            _baseDataParser = new MessagePackBinaryParser();
            _modDataParser = new JsonModParser();

            Debug.Log("[ConfigManager] 数据解析管线初始化完毕 ");
        }

        /// <summary>
        /// 加载某张特定的表
        /// </summary>
        public Dictionary<int, T> LoadTable<T>(byte[] rawData)
        {
            if (_baseDataParser == null) Init();

            // 第一步：用基底管线极速加载原版二进制数据
            Dictionary<int, T> baseDict = _baseDataParser.Parse<int, T>(rawData);

            // TODO (未来 Mod 接入点): 
            // 如果存在对应的 JSON Mod 文件，调用 _modDataParser 解析，并覆盖到 baseDict 中 
            // 比如: ModMergeUtility.Merge(baseDict, _modDataParser.Parse(modJsonString));

            return baseDict;
        }
        public void RegisterTable<T>(Dictionary<int, T> dict)
        {
            _allConfigs[typeof(T)] = dict;
        }
        
        public void ClearAll()
        {
            _allConfigs.Clear();
        }

        /// <summary>
        /// 极速获取指定配置表的字典 (时间复杂度 O(1)，无 GC)
        /// 用法: var itemDict = ConfigManager.Instance.GetTable<Config_Item>();
        /// </summary>
        public Dictionary<int, T> GetTable<T>()
        {
            Type t = typeof(T);
            if (_allConfigs.TryGetValue(t, out object dictObj))
            {
                return dictObj as Dictionary<int, T>;
            }
            Debug.LogError($"[ConfigManager] 找不到表 {t.Name}，请检查是否打上了 ConfigData 标签！");
            return null;
        }

        /// <summary>
        /// 一键全量加载所有核心配表
        /// </summary>
        public async UniTask LoadAllConfigsAsync() 
        {
            if (_baseDataParser == null) 
                Init();

            Debug.Log("[ConfigManager] 开始批量拉取 ConfigData 标签下的所有二进制配表...");

            // Addressables结合UniTask的丝滑写法
            var handle = Addressables.LoadAssetsAsync<TextAsset>("Config", (asset) =>
            {
                ConfigRegister.ParseAndRegister(asset.name, asset.bytes);
            });

            //直接调用 ToUniTask()，完美接入 UniTask 的零 GC 等待环
            await handle.ToUniTask();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"<color=green>[ConfigManager] 全量配置表加载完毕！共 {_allConfigs.Count} 张表 </color>");
            }
            else
            {
                Debug.LogError("[ConfigManager] 批量加载失败！");
            }

            Addressables.Release(handle);
        }
    }
}