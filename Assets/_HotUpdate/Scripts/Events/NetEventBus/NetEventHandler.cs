namespace ProjectGame.HotFix.Core.NetworkEvents
{
    //声明一个泛型委托类型，用于处理网络事件 该委托接受两个参数：
    //一个类型为 TEvent 的事件数据和一个 ulong 类型的发送者客户端 ID TEvent 必须是一个结构体，并且实现了 INetEvent 接口 
    public delegate void NetEventHandler<TEvent>(TEvent eventData,ulong senderClientId) where TEvent : struct, INetEvent;
}