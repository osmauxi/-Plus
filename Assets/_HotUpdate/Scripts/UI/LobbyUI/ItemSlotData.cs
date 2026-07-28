using System.Collections.Generic;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// Skin / Weapon / Item 的统一数据类
    /// 配置读取完成后从这里填充数据
    /// </summary>
    public class ItemSlotData
    {
        public int Id;
        public string Name;
        public string Description;
        public ItemCategory Category;
        public string ResourcePath;
        public string IconPath;

        public Dictionary<string, float> Stats = new Dictionary<string, float>();

        /// <summary>读取指定属性值，并在属性不存在时返回调用方提供的默认值。</summary>
        public float GetStat(string key, float defaultValue = 0f)
        {
            return Stats.TryGetValue(key, out float v) ? v : defaultValue;
        }
    }
}
