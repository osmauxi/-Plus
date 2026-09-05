namespace ProjectGame.HotFix.Network.Runtime
{
    /// <summary>
    /// 上层网络预制件的稳定身份
    /// 这里只登记框架级、场景级网络Root
    /// </summary>
    public enum NetworkPrefabId 
    {
        Invalid = 0,
        NetworkSessionRoot = 1,
        LobbyNetworkRoot = 2,
        GameRuntimeNetworkRoot = 3
    }
}
