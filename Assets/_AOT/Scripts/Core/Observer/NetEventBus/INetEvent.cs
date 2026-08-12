using Unity.Netcode;

namespace ProjectGame.HotFix.Core.NetworkEvents
{
    /// <summary>
    /// 网络瞬时事件标记接口。
    /// 所有通过 NetEventBus 发送的事件都必须显式实现 NGO 的 INetworkSerializable。
    /// </summary>
    public interface INetEvent : INetworkSerializable
    {
    }
}