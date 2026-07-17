using System;
using UnityEngine;

namespace Game.Dialogue
{
    public readonly struct DialogueSessionContext
    {
        public DialogueSessionContext(
            Guid sessionId,
            Guid otherParticipantInstanceId,
            Vector3 otherParticipantPosition)
        {
            SessionId = sessionId;
            OtherParticipantInstanceId =
                otherParticipantInstanceId;
            OtherParticipantPosition =
                otherParticipantPosition;
        }

        public Guid SessionId { get; }

        public Guid OtherParticipantInstanceId { get; }

        public Vector3 OtherParticipantPosition { get; }
    }
}