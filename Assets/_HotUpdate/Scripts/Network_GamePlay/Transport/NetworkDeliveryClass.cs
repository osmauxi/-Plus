namespace ProjectGame.HotFix.Gameplay.Network
{
    /// <summary>
    /// Gameplay层表达的传输语义。
    /// </summary>
    public enum NetworkDeliveryClass : byte
    {
        Command = 0,
        FullSnapshot = 1,
        DeltaSnapshot = 2,
        ReliableEvent = 3,
        UnreliableEvent = 4,
    }
}
