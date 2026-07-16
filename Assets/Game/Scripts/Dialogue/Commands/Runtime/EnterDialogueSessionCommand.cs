using System;
using Cysharp.Threading.Tasks;
using Game.Commands;

namespace Game.Dialogue.Commands
{
    public readonly struct EnterDialogueSessionCommand :
        ICommand<IUniTaskAsyncDisposable>
    {
        public EnterDialogueSessionCommand(
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