using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MyScripts.Core
{
    public static class EventRegister
    //所有事件名都在这里注册，更轻松的处理所有事件引用
    {
        #region Lobby Events
        /// <summary>
        /// 在玩家加入房间时触发，带一个LobbyEventArgs参数
        /// </summary>
        public const string OnJoinedLobby = "OnJoinedLobby";
        /// <summary>
        /// 在玩家加入房间后，房间信息更新时触发，带一个LobbyEventArgs参数
        /// </summary>
        public const string OnJoinedLobbyUpdate = "OnJoinedLobbyUpdate";
        /// <summary>
        /// 在玩家被踢出房间时触发，带一个LobbyEventArgs参数
        /// </summary>
        public const string OnKickedFromLobby = "OnKickedFromLobby";
        /// <summary>
        /// 在房间列表更新时触发，带一个OnLobbyListChangedEventArgs参数
        /// </summary>
        public const string OnLobbyListChanged = "OnLobbyListChanged";
        /// <summary>
        /// 在玩家离开房间时触发，不带参数
        /// </summary>
        public const string OnLeftLobby = "OnLeftLobby";
        /// <summary>
        /// 在玩家改名字时触发，不带参数
        /// </summary>
        public const string OnNameChanged = "OnNameChanged";
        /// <summary>
        /// 在游戏开始时触发，不带参数
        /// </summary>
        public const string OnGameStarted = "OnGameStarted";
        #endregion
        #region Room Events
        /// <summary>
        /// 玩家离开房间时触发,无参数
        /// </summary>
        public const string OnPlayerLeaveRoom = "OnPlayerLeaveRoom";
        /// <summary>
        /// 玩家进入新房间触发，传入一个MapNode参数表示进入的房间
        /// </summary>
        public const string OnPlayerEnterRoom = "OnPlayerEnterRoom";
        /// <summary>
        /// 玩家QE转向时触发，无参数
        /// </summary>
        public const string OnCameraDirectionChanged = "OnCameraDirectionChanged";
        /// <summary>
        /// 玩家生成时触发，传入一个GameObject参数表示玩家对象
        /// </summary>
        public const string OnGamePlayStart = "OnGamePlayStart";
        /// <summary>
        /// 地图完成生成进入玩家状态时触发，无参数
        /// </summary>
        public const string OnPlayerStateStart = "OnPlayerStateStart";
        #endregion
    }
}
