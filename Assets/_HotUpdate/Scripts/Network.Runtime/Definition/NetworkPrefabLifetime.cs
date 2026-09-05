namespace ProjectGame.HotFix.Network.Runtime
{
    /// <summary>
    /// NetworkScopeManager对网络Root的自动生命周期策略
    /// </summary>
    public enum NetworkPrefabLifetime
    {
        /// <summary>
        /// 在注册的需要Scene中生成，跨场景加载进行判定，加载不支持的场景时被销毁
        /// </summary>
        SceneScoped = 0,

        /// <summary>
        /// DDOL，用于跨场景加载的网络Root，生命周期与游戏运行时一致
        /// </summary>
        Persistent = 1
    }
}