using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Dialogue
{
    public interface IDialogueParticipantLifecycle
    {
        UniTask<IDialogueParticipantLease> EnterAsync(
            DialogueParticipantContext context,
            CancellationToken cancellationToken);
    }

    public readonly struct DialogueParticipantContext
    {
        public DialogueParticipantContext(
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