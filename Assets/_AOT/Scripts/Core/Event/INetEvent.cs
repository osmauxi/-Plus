using Unity.Netcode;

public interface INetEvent : INetworkSerializable
{
    //自动转发，True时，服务器不需要进行验证，直接走转发
    bool AutoBroadcast => false;
}