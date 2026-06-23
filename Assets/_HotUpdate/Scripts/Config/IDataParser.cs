using System;
using System.Collections.Generic;

namespace ProjectGame.HotFix.Config
{
    /// <summary>
    /// 数据解析管线策略模式接口，以后同时支持Excel，Json，Xml等数据的读取解析。
    /// </summary>
    public interface IDataParser
    {
        /// <summary>
        /// 将原始的二进制字节流，反序列化为最终的强类型配置字典
        /// </summary>
        /// <param name="rawData">从Addressables读出来的原始bytes数据</param>
        Dictionary<K, V> Parse<K, V>(byte[] rawData);
    }
}