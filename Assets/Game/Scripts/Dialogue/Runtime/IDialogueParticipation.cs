using System;

namespace Game.Dialogue
{
    public interface IDialogueParticipation
    {
        event Action<DialogueSessionContext> ContextEntered;

        event Action<DialogueSessionContext> ContextExited;

        bool IsReadyFor(
            Guid sessionId);

        bool TryEnter(
            DialogueSessionContext context);

        bool TryMarkReady(
            Guid sessionId);

        bool TryExit(
            Guid sessionId);
    }
}