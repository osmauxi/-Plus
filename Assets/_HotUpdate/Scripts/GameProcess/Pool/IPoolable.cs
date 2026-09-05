namespace ProjectGame.HotFix.Gameplay.Pooling
{
    /// <summary>
    /// 池对象可选实现的复用生命周期 
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 对象即将激活并交给调用方前执行 
        /// 用于重置生命值、计时器、刚体状态等 
        /// </summary>
        void OnRentFromPool();

        /// <summary>
        /// 对象即将关闭并放回池中前执行 
        /// 用于取消订阅、停止异步任务、清理临时状态等 
        /// </summary>
        void OnReturnToPool();
    }
}