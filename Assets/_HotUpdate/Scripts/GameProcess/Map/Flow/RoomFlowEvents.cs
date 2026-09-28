using ProjectGame.HotFix.Core.Events;

namespace ProjectGame.HotFix.Gameplay.Map.Flow
{
    public enum RoomGameplayState : byte
    {
        Dormant = 0,
        Gathering = 1,
        Combat = 2,
        Cleared = 3
    }

    public enum RoomFogState : byte
    {
        Hidden = 0,
        Revealed = 1,
        Visited = 2
    }

    public readonly struct RoomStateChangedEvent : ILocalEvent
    {
        public int GenerationId { get; }
        public int RoomId { get; }
        public RoomGameplayState State { get; }

        public RoomStateChangedEvent(int generationId, int roomId, RoomGameplayState state)
        {
            GenerationId = generationId;
            RoomId = roomId;
            State = state;
        }
    }

    public readonly struct PlayerRoomChangedEvent : ILocalEvent
    {
        public int GenerationId { get; }
        public ulong ClientId { get; }
        public int PreviousRoomId { get; }
        public int RoomId { get; }

        public PlayerRoomChangedEvent(int generationId, ulong clientId, int previousRoomId, int roomId)
        {
            GenerationId = generationId;
            ClientId = clientId;
            PreviousRoomId = previousRoomId;
            RoomId = roomId;
        }
    }

    public readonly struct RoomFogChangedEvent : ILocalEvent
    {
        public int GenerationId { get; }
        public int RoomId { get; }
        public RoomFogState State { get; }

        public RoomFogChangedEvent(int generationId, int roomId, RoomFogState state)
        {
            GenerationId = generationId;
            RoomId = roomId;
            State = state;
        }
    }

    public readonly struct RoomFlowSnapshotAppliedEvent : ILocalEvent
    {
        public int GenerationId { get; }

        public RoomFlowSnapshotAppliedEvent(int generationId)
        {
            GenerationId = generationId;
        }
    }

    public readonly struct RoomEncounterFailedEvent : ILocalEvent
    {
        public int GenerationId { get; }
        public int RoomId { get; }
        public string Reason { get; }

        public RoomEncounterFailedEvent(int generationId, int roomId, string reason)
        {
            GenerationId = generationId;
            RoomId = roomId;
            Reason = reason;
        }
    }

    public readonly struct BossRoomClearedEvent : ILocalEvent
    {
        public int GenerationId { get; }
        public int RoomId { get; }

        public BossRoomClearedEvent(int generationId, int roomId)
        {
            GenerationId = generationId;
            RoomId = roomId;
        }
    }
}
