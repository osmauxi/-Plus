using System;
using System.Threading;

namespace ProjectGame.HotFix.Core.Events
{
    /// <summary>
    /// 单个事件订阅句柄。
    /// Dispose后会自动从事件总线中取消订阅。
    /// </summary>
    public sealed class EventSubscription : IDisposable
    {
        //订阅后方法会自动封装一个取消订阅委托，保存在_disposeAction中
        private Action _disposeAction;

        public bool IsDisposed => _disposeAction == null;

        public EventSubscription(Action disposeAction)
        {
            _disposeAction = disposeAction ?? throw new ArgumentNullException(nameof(disposeAction));
        }

        public void Dispose()
        {
            //原子操作，保证线程安全，防止多线程同时调用Dispose导致重复取消订阅
            //Interlocked.Exchange会将_disposeAction设置为null，并返回原来的值
            var action = Interlocked.Exchange(ref _disposeAction, null);
            action?.Invoke();
        }
    }
}