using ProjectGame.HotFix.Core.Events;
using ProjectGame.HotFix.Gameplay.State;

namespace ProjectGame.HotFix.Gameplay.Events
{
    public readonly struct GameStateChangedEvent : ILocalEvent
    {
        public readonly GameState PreviousState;
        public readonly GameState CurrentState;

        public GameStateChangedEvent(GameState previousState,GameState currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }
    }

    /// <summary>
    /// 本机收到当前层数变化 
    /// </summary>
    public readonly struct GameLevelChangedEvent : ILocalEvent
    {
        public readonly int PreviousLevel;
        public readonly int CurrentLevel;

        public GameLevelChangedEvent(int previousLevel,int currentLevel)
        {
            PreviousLevel = previousLevel;
            CurrentLevel = currentLevel;
        }
    }
}
