using System;
using System.Collections.Generic;

namespace ProjectGame.HotFix.Core.Events
{
    /// <summary>
    /// 批量管理事件订阅。
    /// 常用于 MonoBehaviour 的 OnEnable / OnDisable 生命周期。
    /// </summary>
    public sealed class EventSubscriptionGroup : IDisposable
    {
        private readonly List<IDisposable> _subscriptions = new();
        private bool _disposed;

        public int Count => _subscriptions.Count;

        public void Add(IDisposable subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException(nameof(subscription));
            }

            if (_disposed)
            {
                subscription.Dispose();
                throw new ObjectDisposedException(nameof(EventSubscriptionGroup));
            }

            _subscriptions.Add(subscription);
        }

        public void Clear()
        {
            //倒序遍历，避免在Dispose过程中修改集合导致异常
            for (int i = _subscriptions.Count - 1; i >= 0; i--)
            {
                _subscriptions[i]?.Dispose();
            }

            _subscriptions.Clear();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Clear();
            _disposed = true;
        }
    }

    public static class EventSubscriptionExtensions
    {
        //允许使用链式调用，在绑定事件的同时可以直接.AddTo(group)加入事件订阅组中。
        //subscription参数前加了this关键字，表示这是一个扩展方法，可以直接在IDisposable对象上调用。
        public static IDisposable AddTo(this IDisposable subscription,EventSubscriptionGroup group)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            group.Add(subscription);
            return subscription;
        }
    }
}