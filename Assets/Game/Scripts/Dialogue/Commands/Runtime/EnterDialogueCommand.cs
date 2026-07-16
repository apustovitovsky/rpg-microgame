using System;
using Game.Commands;

namespace Game.Dialogue.Commands
{
    public readonly struct EnterDialogueCommand :
        ICommand
    {
        public EnterDialogueCommand(
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