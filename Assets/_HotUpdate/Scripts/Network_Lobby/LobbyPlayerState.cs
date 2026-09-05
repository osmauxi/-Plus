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

        //防断线重连的唯一不变ID，由第一次启动生成固定的ID串或者其他固定ID 取决于连接方式
        public FixedString64Bytes PersistentPlayerId;

        //玩家昵称
        public FixedString32Bytes PlayerName;

        //服务器分配的稳定大厅展位
        public int StandIndex;

        //玩家选择的角色模型ID
        public int CharacterId;

        //选择的武器ID
        public int WeaponId;

        //选择的道具ID
        public int ItemId;

        //是否已准备就绪
        public bool IsReady;

        /// <summary>按固定字段顺序序列化大厅玩家状态 </summary>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PersistentPlayerId);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref StandIndex);
            serializer.SerializeValue(ref CharacterId);
            serializer.SerializeValue(ref WeaponId);
            serializer.SerializeValue(ref ItemId);
            serializer.SerializeValue(ref IsReady);
        }

        /// <summary>比较全部同步字段，用于 NetworkList 脏标记判断 </summary>
        public bool Equals(LobbyPlayerState other)
        {
            return ClientId == other.ClientId &&
                   PersistentPlayerId == other.PersistentPlayerId &&
                   PlayerName == other.PlayerName &&
                   StandIndex == other.StandIndex &&
                   CharacterId == other.CharacterId &&
                    WeaponId == other.WeaponId &&
                    ItemId == other.ItemId &&
                    IsReady == other.IsReady;
        }
    }
}
