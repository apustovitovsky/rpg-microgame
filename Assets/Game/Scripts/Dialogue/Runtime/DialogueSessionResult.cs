using System;

namespace Game.Dialogue
{
    public enum DialogueSessionStatus
    {
        Opened = 0,
        AlreadyOpen = 1,
        InvalidRequest = 2
    }

    public readonly struct DialogueSessionResult
    {
        public DialogueSessionResult(
            DialogueSessionStatus status,
            Guid sessionId)
        {
            Status = status;
            SessionId = sessionId;
        }

        public DialogueSessionStatus Status { get; }

        public Guid SessionId { get; }

        public bool Succeeded =>
            Status == DialogueSessionStatus.Opened;
    }
}