using System;

namespace ProjectGame.HotFix.Core.Events
{
    //Exception标记EventDispatchException为一个合法的异常类型，可以被try-catch捕获和处理。
    public sealed class EventDispatchException : Exception
    {
        //BusName属性表示发生异常的事件总线名称，类型为string。
        public string BusName { get; }
        //EventType属性表示发生异常的事件类型，类型为Type。
        public Type EventType { get; }

        public EventDispatchException(string busName,Type eventType,Exception innerException)
            : base(
                $"Exception occurred while dispatching local event. " +
                $"Bus: {busName}, Event: {eventType.FullName}",
                innerException)
        {
            BusName = busName;
            EventType = eventType;
        }
    }
}