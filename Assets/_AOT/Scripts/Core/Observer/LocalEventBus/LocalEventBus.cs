using System;
using System.Collections.Generic;

namespace ProjectGame.HotFix.Core.Events
{
    /// <summary>
    /// 强类型本地事件总线
    /// </summary>
    public sealed class LocalEventBus : IEventBus
    {
        public static LocalEventBus Global { get; } = new LocalEventBus("GlobalLocalEventBus");

        //线程安全的锁对象，用于保护对_streams字典的访问，任何对字典_streams的增删改查动作，都必须申请这把锁
        private readonly object _gate = new();
        private readonly Dictionary<Type, IEventStream> _streams = new();
        private readonly Action<Exception> _exceptionHandler;
        private readonly string _busName;

        public LocalEventBus(string busName = "LocalEventBus",Action<Exception> exceptionHandler = null)
        {
            _busName = string.IsNullOrWhiteSpace(busName)? "LocalEventBus": busName;

            _exceptionHandler = exceptionHandler ?? DefaultExceptionHandler;
        }

        //有参订阅方法
        public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, ILocalEvent
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            return GetOrCreateStream<TEvent>().Subscribe(handler);
        }
        //无参订阅方法
        public IDisposable Subscribe<TEvent>(Action handler) where TEvent : struct, ILocalEvent
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            return Subscribe<TEvent>(_ => handler.Invoke());
        }

        public void Publish<TEvent>(TEvent eventData) where TEvent : struct, ILocalEvent
        {
            var stream = TryGetStream<TEvent>();
            stream?.Publish(eventData);
        }

        public void Publish<TEvent>() where TEvent : struct, ILocalEvent
        {
            //无参事件会包装一个默认的TEvent实例并发布
            Publish(default(TEvent));
        }

        public bool HasSubscribers<TEvent>() where TEvent : struct, ILocalEvent
        {
            return GetSubscriberCount<TEvent>() > 0;
        }

        public int GetSubscriberCount<TEvent>() where TEvent : struct, ILocalEvent
        {
            var stream = TryGetStream<TEvent>();
            return stream?.Count ?? 0;
        }

        public void Clear<TEvent>() where TEvent : struct, ILocalEvent
        {
            IEventStream stream = null;

            lock(_gate)
            {
                var type = typeof(TEvent);

                if(_streams.TryGetValue(type, out stream))
                {
                    _streams.Remove(type);
                }
            }

            stream?.Clear();
        }

        public void Clear()
        {
            List<IEventStream> streams;

            lock (_gate)
            {
                streams = new List<IEventStream>(_streams.Values);
                _streams.Clear();
            }

            for (int i = 0; i < streams.Count; i++)
            {
                streams[i].Clear();
            }
        }

        private EventStream<TEvent> GetOrCreateStream<TEvent>() where TEvent : struct, ILocalEvent
        {
            var type = typeof(TEvent);

            lock (_gate)
            {
                //_streams中存的是IEventStream接口类型的对象，实际存储的是EventStream<TEvent>对象，所以有一层还原。
                if (_streams.TryGetValue(type, out var existingStream))
                {
                    return (EventStream<TEvent>)existingStream;
                }

                var newStream = new EventStream<TEvent>(_busName,_exceptionHandler);

                _streams.Add(type, newStream);
                return newStream;
            }
        }

        private EventStream<TEvent> TryGetStream<TEvent>() where TEvent : struct, ILocalEvent
        {
            var type = typeof(TEvent);

            lock (_gate)
            {
                return _streams.TryGetValue(type, out var stream) ? (EventStream<TEvent>)stream : null;
            }
        }

        private static void DefaultExceptionHandler(Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }

        private interface IEventStream
        {
            int Count { get; }

            void Clear();
        }

        //一个事件对应一个EventStream<T>，被统一存在_streams字典中。
        private sealed class EventStream<TEvent> : IEventStream where TEvent : struct, ILocalEvent
        {
            private readonly object _gate = new();
            private readonly List<EventSubscriber> _subscribers = new();
            private readonly Action<Exception> _exceptionHandler;
            private readonly string _busName;

            private int _nextSubscriberId;

            public int Count
            {
                get
                {
                    lock (_gate)
                    {
                        return _subscribers.Count;
                    }
                }
            }

            public EventStream(string busName,Action<Exception> exceptionHandler)
            {
                _busName = busName;
                _exceptionHandler = exceptionHandler;
            }

            public IDisposable Subscribe(Action<TEvent> handler)
            {
                var subscriber = new EventSubscriber(++_nextSubscriberId,handler);

                lock (_gate)
                {
                    _subscribers.Add(subscriber);
                }

                return new EventSubscription(() => RemoveSubscriber(subscriber.Id));
            }

            public void Publish(TEvent eventData)
            {
                EventSubscriber[] snapshot;
                //不让系统带锁进行费时遍历，所以先把_subscribers的快照取出来，然后在快照上进行遍历
                //这样可以减少在事件处理过程中阻塞其他线程操作的情况。
                //但相对的，会出现ToArray的数组分配开销
                lock (_gate)
                {
                    if (_subscribers.Count == 0)
                    {
                        return;
                    }
                    //这里是复制一份_subscribers，保证本次派发使用固定的订阅列表不会出现突然多事件和少事件的情况
                    //相对于List，数组读取更快
                    snapshot = _subscribers.ToArray();
                }

                for (int i = 0; i < snapshot.Length; i++)
                {
                    try
                    {
                        snapshot[i].Handler.Invoke(eventData);
                    }
                    catch (Exception exception)
                    {
                        _exceptionHandler?.Invoke(new EventDispatchException(_busName,typeof(TEvent),exception));
                    }
                }
            }

            public void Clear()
            {
                lock (_gate)
                {
                    _subscribers.Clear();
                }
            }

            private void RemoveSubscriber(int subscriberId)
            {
                lock (_gate)
                {
                    for (int i = _subscribers.Count - 1; i >= 0; i--)
                    {
                        if (_subscribers[i].Id == subscriberId)
                        {
                            _subscribers.RemoveAt(i);
                            return;
                        }
                    }
                }
            }

            private readonly struct EventSubscriber
            {
                public readonly int Id;
                public readonly Action<TEvent> Handler;

                public EventSubscriber(int id, Action<TEvent> handler)
                {
                    Id = id;
                    Handler = handler;
                }
            }
        }
    }
}