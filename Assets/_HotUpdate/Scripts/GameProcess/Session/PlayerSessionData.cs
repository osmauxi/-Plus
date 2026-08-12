namespace ProjectGame.HotFix.Core.Session
{
    /// <summary>
    /// 从 Lobby 带入 Gameplay 的只读玩家数据。
    /// 这里只保留游戏阶段真正需要的信息。
    /// </summary>
    public readonly struct PlayerSessionData
    {
        public ulong ClientId { get; }
        public string PersistentPlayerId { get; }
        public string PlayerName { get; }
        public int CharacterId { get; }
        public int WeaponId { get; }
        public int ItemId { get; }

        public PlayerSessionData(ulong clientId, string persistentPlayerId, string playerName, int characterId, int weaponId, int itemId)
        {
            ClientId = clientId;
            PersistentPlayerId = persistentPlayerId;
            PlayerName = playerName;
            CharacterId = characterId;
            WeaponId = weaponId;
            ItemId = itemId;
        }

        public PlayerSessionData WithWeaponId(int weaponId)
        {
            return new PlayerSessionData(
                ClientId,
                PersistentPlayerId,
                PlayerName,
                CharacterId,
                weaponId,
                ItemId);
        }

        public PlayerSessionData WithItemId(int itemId)
        {
            return new PlayerSessionData(
                ClientId,
                PersistentPlayerId,
                PlayerName,
                CharacterId,
                WeaponId,
                itemId);
        }

        public PlayerSessionData WithEquipment(int weaponId, int itemId)
        {
            return new PlayerSessionData(
                ClientId,
                PersistentPlayerId,
                PlayerName,
                CharacterId,
                weaponId,
                itemId);
        }
    }
}
