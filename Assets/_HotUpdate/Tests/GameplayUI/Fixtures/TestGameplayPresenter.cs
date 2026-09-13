using System.Threading;
using ProjectGame.HotFix.UI.Gameplay;

namespace ProjectGame.HotFix.Tests.GameplayUI
{
    public sealed class TestGameplayPresenter : BaseGameplayPresenter<TestGameplayView>
    {
        public override GameplayUIId Id => GameplayUIId.PlayerStatus;
        public int InitializeCount, OpenCount, CloseCount, RenderCount, ShutdownCount;
        public CancellationToken CurrentVisibleToken => VisibleToken;
        protected override void OnInitialize() => InitializeCount++;
        protected override void OnOpened() => OpenCount++;
        protected override void OnClosed() => CloseCount++;
        protected override void OnShutdown() => ShutdownCount++;
        protected override void RenderView() => RenderCount++;
    }
}
