using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Config
{
    public class MessagePackBinaryParser : IDataParser
    {
        public Dictionary<K, V> Parse<K, V>(byte[] rawData)
        {
            Debug.Log($"[ConfigPipeline] 正在使用 [MessagePack 二进制策略] 解析数据，数据大小: {rawData.Length} bytes");
            //使用MessagePack的反序列化方法将原始字节流转换为Dictionary<K, V>
            return MessagePack.MessagePackSerializer.Deserialize<Dictionary<K, V>>(rawData);
        }
    }


}
