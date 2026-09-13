namespace ProjectGame.HotFix.UI.Gameplay.EffectRoll
{
    /// <summary>
    /// Presenter交给卡片View的纯展示数据，不暴露Weapon配置对象。
    /// </summary>
    public readonly struct EffectRollCardModel
    {
        public ushort EffectId { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public string Tag { get; }
        public string IconAddress { get; }

        public EffectRollCardModel(ushort effectId, string displayName, string description,
            string tag, string iconAddress)
        {
            EffectId = effectId;
            DisplayName = displayName ?? string.Empty;
            Description = description ?? string.Empty;
            Tag = tag ?? string.Empty;
            IconAddress = iconAddress ?? string.Empty;
        }
    }
}
