using System;

namespace Game.Loot
{
    public enum LootSessionOpenStatus
    {
        None = 0,
        Opened = 1,
        InvalidRequest = 2,
        AlreadyOpen = 3,
    }

    public readonly struct LootSessionOpenResult
    {
        internal LootSessionOpenResult(
            LootSessionOpenStatus status,
            Guid sessionId)
        {
            Status = status;
            SessionId = sessionId;
        }

        public LootSessionOpenStatus Status { get; }

        public Guid SessionId { get; }

        public bool Succeeded =>
            Status == LootSessionOpenStatus.Opened;
    }
}