using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace ProjectGame.HotFix.Core.Network
{
    /// <summary>
    /// 开房/加房的通用参数包
    /// </summary>
    public struct MatchmakingParams
    {
        public string IpAddress;
        public ushort Port;
        public string RoomName;
        public string Password;
        public int MaxPlayers;
    }

    /// <summary>
    /// 用于给 UI 渲染大厅列表的数据结构，这里做伏笔
    /// </summary>
    public struct LobbyRoomInfo
    {
        public string RoomId;
        public string RoomName;
        public int CurrentPlayers;
        public int MaxPlayers;
    }

    /// <summary>
    /// 网络联机策略接口，也做伏笔。
    /// </summary>
    public interface IMatchmakingStrategy
    {
        /// <summary>
        /// 这个策略是否支持“获取房间列表”？
        /// PTP只能IP直连，但是标准连接模式支持房间搜索
        /// </summary>
        bool SupportsLobbyList { get; }

        //UniTask等效协程和异步Task，但是可回调，0GC，可Try Catch。
        /// <summary>
        /// 作为Host创建房间并挂起等待
        /// </summary>
        UniTask<bool> StartHostAsync(MatchmakingParams parameters);

        /// <summary>
        /// 作为 Client 尝试加入房间 (触发 ConnectionApproval)
        /// </summary>
        UniTask<bool> StartClientAsync(MatchmakingParams parameters);

        /// <summary>
        /// 拉取大厅房间列表
        /// </summary>
        UniTask<List<LobbyRoomInfo>> GetLobbyListAsync();

        /// <summary>
        /// 离开房间/断开连接
        /// </summary>
        void Disconnect();
    }
}