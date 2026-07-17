using System;
using Game.Commands;
using UnityEngine;

namespace Game.Dialogue.Commands
{
    public readonly struct EnterDialogueSessionCommand :
        ICommand
    {
        public EnterDialogueSessionCommand(
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