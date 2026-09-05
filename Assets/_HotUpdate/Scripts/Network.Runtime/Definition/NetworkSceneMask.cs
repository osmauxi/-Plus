using System;

namespace ProjectGame.HotFix.Network.Runtime
{
    /// <summary>
    /// 网络场景掩码
    /// </summary>
    [Flags]
    // Unity serializes enum fields up to 32 bits. An ulong backing type caused Catalog
    // SceneMask values to be omitted from assets; RPCs may still transport them as ulong.
    public enum NetworkSceneMask : int
    {
        None = 0,

        Lobby = 1 << 0,
        GameRuntime = 1 << 1,
        GameUI = 1 << 2
    }
}
