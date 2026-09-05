using ProjectGame.HotFix.Core.Events;

namespace ProjectGame.HotFix.Gameplay.Events
{
    /// <summary>
    /// Server Gameplay逻辑请求进入下一层 
    /// 只表达“请求发生”，具体是否允许以及如何转层由GameLevelFlowController决定 
    /// </summary>
    public readonly struct NextLevelRequestedEvent : ILocalEvent
    {
    }
}
