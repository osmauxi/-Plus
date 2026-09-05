namespace ProjectGame.HotFix.Network.Runtime
{
    /// <summary>
    /// 一个 NetworkSession 只能选择一种物理场景同步后端。
    /// </summary>
    public enum NetworkSceneBackend
    {
        Addressables = 0,
        NgoIntegrated = 1
    }
}
