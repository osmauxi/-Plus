using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProjectGame.HotFix.Config
{
    ///<summary>
    ///明文JSON解析策略
    ///</summary>
    public class JsonModParser : IDataParser
    {
        public Dictionary<K, V> Parse<K, V>(byte[] rawData)
        {
            Debug.Log("[ConfigPipeline] 正在使用 [JSON Mod 策略] 解析玩家本地补丁...");

            // 将 bytes 转化为字符串
            string jsonStr = Encoding.UTF8.GetString(rawData);

            // TODO: 之后引入你喜欢的 Json 库（如 JsonUtility 或 Newtonsoft.Json）进行解析
            // return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<K, V>>(jsonStr);

            return new Dictionary<K, V>(); // 暂时留空防御报错
        }
    }
}