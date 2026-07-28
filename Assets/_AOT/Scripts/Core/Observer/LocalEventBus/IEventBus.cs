using System;
namespace ProjectGame.HotFix.Core.Events
{
    public interface IEventBus
    {
        //TEvent是一种命名规范，表示事件类型必须是结构体
        IDisposable Subscribe<TEvent>(Action<TEvent> handler)
            where TEvent : struct, ILocalEvent;

        IDisposable Subscribe<TEvent>(Action handler)
            where TEvent : struct, ILocalEvent;

        void Publish<TEvent>(TEvent eventData)
            where TEvent : struct, ILocalEvent;

        void Publish<TEvent>()
            where TEvent : struct, ILocalEvent;

        bool HasSubscribers<TEvent>()
            where TEvent : struct, ILocalEvent;

        int GetSubscriberCount<TEvent>()
            where TEvent : struct, ILocalEvent;

        void Clear<TEvent>()
            where TEvent : struct, ILocalEvent;

        void Clear();
    }
}
