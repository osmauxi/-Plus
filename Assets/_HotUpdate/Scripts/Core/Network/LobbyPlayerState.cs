using Unity.Netcode;
using Unity.Collections;
using System;

namespace ProjectGame.HotFix.Core.Network
{
    /// <summary>
    /// 大厅玩家状态数据体
    /// </summary>
    public struct LobbyPlayerState : INetworkSerializable, IEquatable<LobbyPlayerState>
    {
        //NGO分配的ID，可变但唯一
        public ulong ClientId;

        //防断线重连的唯一不变ID，由第一次启动生成固定的ID串或者其他固定ID。取决于连接方式
        public FixedString64Bytes PersistentPlayerId;

        //玩家昵称
        public FixedString32Bytes PlayerName;

        //玩家选择的角色模型ID
        public int CharacterId;

        //选择的武器ID
        public int WeaponId;

        //是否已准备就绪
        public bool IsReady;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PersistentPlayerId);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref CharacterId);
            serializer.SerializeValue(ref WeaponId);
            serializer.SerializeValue(ref IsReady);
        }

        //IEquatable标记这个结构体可以通过Equals方法来判定是否相等，这里用来当脏标记，避免固定时长的重复同步
        public bool Equals(LobbyPlayerState other)
        {
            return ClientId == other.ClientId &&
                   PersistentPlayerId == other.PersistentPlayerId &&
                   PlayerName == other.PlayerName &&
                   CharacterId == other.CharacterId &&
                   WeaponId == other.WeaponId &&
                   IsReady == other.IsReady;
        }
    }
}
