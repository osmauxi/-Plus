using System;

namespace ProjectGame.HotFix.Core.Events
{
    /// <summary>
    /// 本地事件总线的全局访问门面。
    /// </summary>
    public static class LocalEvents
    {
        private static IEventBus _bus = LocalEventBus.Global;

        public static IEventBus Bus => _bus;

        public static void SetBus(IEventBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public static void ResetToGlobal()
        {
            _bus = LocalEventBus.Global;
        }

        public static IDisposable Subscribe<TEvent>(Action<TEvent> handler)
            where TEvent : struct, ILocalEvent
        {
            return _bus.Subscribe(handler);
        }

        public static IDisposable Subscribe<TEvent>(Action handler)
            where TEvent : struct, ILocalEvent
        {
            return _bus.Subscribe<TEvent>(handler);
        }

        public static void Publish<TEvent>(TEvent eventData)
            where TEvent : struct, ILocalEvent
        {
            _bus.Publish(eventData);
        }

        public static void Publish<TEvent>()
            where TEvent : struct, ILocalEvent
        {
            _bus.Publish<TEvent>();
        }

        public static bool HasSubscribers<TEvent>()
            where TEvent : struct, ILocalEvent
        {
            return _bus.HasSubscribers<TEvent>();
        }

        public static int GetSubscriberCount<TEvent>()
            where TEvent : struct, ILocalEvent
        {
            return _bus.GetSubscriberCount<TEvent>();
        }

        public static void Clear<TEvent>()
            where TEvent : struct, ILocalEvent
        {
            _bus.Clear<TEvent>();
        }

        public static void Clear()
        {
            _bus.Clear();
        }
    }
}