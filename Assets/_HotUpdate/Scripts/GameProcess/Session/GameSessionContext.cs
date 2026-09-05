using System;
using System.Collections.Generic;

namespace ProjectGame.HotFix.Core.Session
{
    /// <summary>
    /// 跨LobbyScene 和 GameRuntimeScene保存本局会话快照 
    /// 它只保存数据，不负责生成玩家、网络同步或游戏逻辑 
    /// 返回主菜单或结束会话时需要 Clear 
    /// </summary>
    public static class GameSessionContext
    {
        private static PlayerSessionData[] _players = Array.Empty<PlayerSessionData>();

        public static event Action<PlayerSessionData> PlayerUpdated;

        public static GameSessionMode Mode { get; private set; }

        public static bool IsConfigured => Mode != GameSessionMode.None;

        public static bool IsSinglePlayer => Mode == GameSessionMode.SinglePlayer;

        public static bool IsMultiplayer => Mode == GameSessionMode.Multiplayer;

        public static IReadOnlyList<PlayerSessionData> Players => _players;

        public static int PlayerCount => _players.Length;

        public static void Configure(GameSessionMode mode, IReadOnlyList<PlayerSessionData> players)
        {
            if (mode == GameSessionMode.None)
                throw new ArgumentException("不能使用 None 配置游戏会话 ", nameof(mode));

            if (players == null || players.Count == 0)
                throw new ArgumentException("游戏会话至少需要一名玩家 ", nameof(players));

            PlayerSessionData[] snapshot = new PlayerSessionData[players.Count];
            HashSet<ulong> clientIds = new HashSet<ulong>();

            for (int i = 0; i < players.Count; i++)
            {
                PlayerSessionData player = players[i];

                if (!clientIds.Add(player.ClientId))
                    throw new InvalidOperationException($"会话玩家 ClientId 重复：{player.ClientId}");

                snapshot[i] = player;
            }

            // 固定顺序有利于生成点分配和调试结果保持一致 
            Array.Sort(snapshot, (left, right) => left.ClientId.CompareTo(right.ClientId));

            Mode = mode;
            _players = snapshot;
        }

        public static bool TryGetPlayer(ulong clientId, out PlayerSessionData playerData)
        {
            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i].ClientId != clientId)
                    continue;

                playerData = _players[i];
                return true;
            }

            playerData = default;
            return false;
        }

        /// <summary>更新指定玩家的当前武器，并通知会话数据消费者 </summary>
        public static bool UpdateWeapon(ulong clientId, int weaponId)
        {
            return TryUpdatePlayer(
                clientId,
                player => player.WithWeaponId(weaponId));
        }

        /// <summary>更新指定玩家的当前道具，并通知会话数据消费者 </summary>
        public static bool UpdateItem(ulong clientId, int itemId)
        {
            return TryUpdatePlayer(
                clientId,
                player => player.WithItemId(itemId));
        }

        /// <summary>一次性更新指定玩家的武器和道具 </summary>
        public static bool UpdateEquipment(ulong clientId, int weaponId, int itemId)
        {
            return TryUpdatePlayer(
                clientId,
                player => player.WithEquipment(weaponId, itemId));
        }

        private static bool TryUpdatePlayer(
            ulong clientId,
            Func<PlayerSessionData, PlayerSessionData> update)
        {
            if (!IsConfigured)
                return false;

            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i].ClientId != clientId)
                    continue;

                PlayerSessionData updatedPlayer = update(_players[i]);
                _players[i] = updatedPlayer;
                PlayerUpdated?.Invoke(updatedPlayer);
                return true;
            }

            return false;
        }

        public static void Clear()
        {
            Mode = GameSessionMode.None;
            _players = Array.Empty<PlayerSessionData>();
        }
    }
}
