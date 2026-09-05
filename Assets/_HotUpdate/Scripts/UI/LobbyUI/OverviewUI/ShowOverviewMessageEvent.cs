using ProjectGame.HotFix.Core.Events;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// 请求在大厅 Overview 页面显示一条临时消息 
    /// </summary>
    public readonly struct ShowOverviewMessageEvent : ILocalEvent
    {
        public const float DefaultVisibleDuration = 3f;

        public string Message { get; }
        public float VisibleDuration { get; }

        public ShowOverviewMessageEvent(
            string message,
            float visibleDuration = DefaultVisibleDuration)
        {
            Message = message;
            VisibleDuration = visibleDuration;
        }
    }
}
