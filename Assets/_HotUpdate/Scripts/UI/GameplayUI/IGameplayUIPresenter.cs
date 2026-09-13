namespace ProjectGame.HotFix.UI.Gameplay
{
    /// <summary>
    /// Presenter使用接口向Manager提出导航请求。bool表示已接受；生命周期回调内的请求延后执行。
    /// </summary>
    public interface IGameplayUINavigation
    {
        bool Show(GameplayUIId id);
        bool Hide(GameplayUIId id);
        bool Toggle(GameplayUIId id);
        bool TryNavigateBack();
    }

    /// <summary>
    /// 生命周期由Manager控制
    /// </summary>
    public interface IGameplayUIPresenter
    {
        GameplayUIId Id { get; }
        GameplayUIPolicy Policy { get; }
        bool IsInitialized { get; }
        bool IsOpen { get; }
        bool IsVisible { get; }
        bool IsInteractive { get; }

        void Initialize(IGameplayUINavigation navigation);
        void SetPresentation(bool open, bool visible, bool interactive);
        void Refresh();
        bool TryHandleBackRequest();
        void Shutdown();
    }
}
