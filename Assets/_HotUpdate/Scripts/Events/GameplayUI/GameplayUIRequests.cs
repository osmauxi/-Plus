using ProjectGame.HotFix.Core.Events;

namespace ProjectGame.HotFix.UI.Gameplay
{
    public enum GameplayUICommand : byte
    {
        Show,
        Hide,
        Toggle,
        Back,
        CloseTop,
        CloseAllScreens,
        ResetToGameplay,
    }

    public readonly struct GameplayUIRequest : ILocalEvent
    {
        public GameplayUICommand Command { get; }
        public GameplayUIId Id { get; }

        public GameplayUIRequest(GameplayUICommand command, GameplayUIId id = GameplayUIId.None)
        {
            Command = command;
            Id = id;
        }
    }

    /// <summary>
    /// Gameplay 业务层的本机请求门面。仅在 UI 已初始化且 GamePlaying 时接收打开请求；
    /// 不缓存跨场景请求，不执行网络同步。需要操作结果的 UI 代码使用 Manager 的 bool 接口。
    /// </summary>
    public static class GameplayUIRequests
    {
        public static void Show(GameplayUIId id) => Send(GameplayUICommand.Show, id);
        public static void Hide(GameplayUIId id) => Send(GameplayUICommand.Hide, id);
        public static void Toggle(GameplayUIId id) => Send(GameplayUICommand.Toggle, id);
        public static void Back() => Send(GameplayUICommand.Back);
        public static void CloseTop() => Send(GameplayUICommand.CloseTop);
        public static void CloseAllScreens() => Send(GameplayUICommand.CloseAllScreens);
        public static void ResetToGameplay() => Send(GameplayUICommand.ResetToGameplay);

        private static void Send(GameplayUICommand command, GameplayUIId id = GameplayUIId.None)
            => LocalEvents.Publish(new GameplayUIRequest(command, id));
    }

    public readonly struct GameplayUIVisibilityChangedEvent : ILocalEvent
    {
        public GameplayUIId Id { get; }
        public bool IsOpen { get; }
        public bool IsVisible { get; }

        public GameplayUIVisibilityChangedEvent(GameplayUIId id, bool isOpen, bool isVisible)
        {
            Id = id;
            IsOpen = isOpen;
            IsVisible = isVisible;
        }
    }

    public readonly struct GameplayUITopChangedEvent : ILocalEvent
    {
        public GameplayUIId Previous { get; }
        public GameplayUIId Current { get; }

        public GameplayUITopChangedEvent(GameplayUIId previous, GameplayUIId current)
        {
            Previous = previous;
            Current = current;
        }
    }
}
