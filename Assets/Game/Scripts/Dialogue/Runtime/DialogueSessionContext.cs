using System;

namespace Game.Dialogue
{
    public readonly struct DialogueSessionContext
    {
        public DialogueSessionContext(
            Guid sessionId,
            Guid otherParticipantInstanceId)
        {
            SessionId = sessionId;
            OtherParticipantInstanceId =
                otherParticipantInstanceId;
        }

        public Guid SessionId { get; }

        public Guid OtherParticipantInstanceId { get; }
    }
}