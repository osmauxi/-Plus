namespace ProjectGame.HotFix.UI.Gameplay
{
    /// <summary>
    /// 本机 Gameplay UI 模块标识；放在 Events 程序集以供 Gameplay 发出请求。
    /// </summary>
    public enum GameplayUIId : byte
    {
        None = 0,
        GameplayHUD = 1,
        EffectRoll = 2,
        PlayerStatus = 3,
        Settings = 4,
    }
}
