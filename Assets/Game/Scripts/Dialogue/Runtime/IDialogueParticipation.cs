using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Dialogue
{
    public interface IDialogueParticipation
    {
        event Action<DialogueSessionContext> ContextEntered;

        event Action<DialogueSessionContext> ContextExited;

        bool TryEnter(
            DialogueSessionContext context);

        UniTask WaitUntilReadyAsync(
            Guid sessionId,
            CancellationToken cancellationToken);

        bool TryMarkReady(
            Guid sessionId);

        bool TryExit(
            Guid sessionId);
    }
}