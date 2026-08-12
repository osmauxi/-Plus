namespace ProjectGame.HotFix.Gameplay.Map.View
{
    /// <summary>
    /// 房间插槽相对于房间自身的局部方向。
    /// GridGraph使用四个标准方向；
    /// Corridor策略中的自由插槽可以使用None。
    /// </summary>
    public enum ConnectorDirection : byte
    {
        None = 0,
        North = 1,
        East = 2,
        South = 3,
        West = 4
    }
}