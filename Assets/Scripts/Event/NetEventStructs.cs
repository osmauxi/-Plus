using Unity.Netcode;

public struct GameStartStruct : INetEvent
{
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
    }
}

public struct PlayerEnterRoomStruct : INetEvent 
{
    public ulong NodeId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref NodeId);
    }
}

public struct GamePlayStartStruct : INetEvent
{
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
       
    }
}
