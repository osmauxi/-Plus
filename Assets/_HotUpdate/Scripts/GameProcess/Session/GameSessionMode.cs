namespace ProjectGame.HotFix.Core.Session
{
    /// <summary>
    /// 本次游戏会话的明确模式 
    /// </summary>
    public enum GameSessionMode : byte
    {
        None = 0,
        SinglePlayer = 1,
        Multiplayer = 2
    }
}