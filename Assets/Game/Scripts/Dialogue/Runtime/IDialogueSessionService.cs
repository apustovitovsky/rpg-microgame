using System;

namespace Game.Dialogue
{
    public interface IDialogueSessionService
    {
        DialogueSessionResult TryOpen(
            Guid initiatorInstanceId,
            Guid participantInstanceId);

        bool TryGet(
            Guid sessionId,
            out DialogueSession session);

        bool TryGetActive(
            out DialogueSession session);

        bool Close(Guid sessionId);
    }
}