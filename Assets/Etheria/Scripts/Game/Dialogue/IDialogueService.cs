using UnityEngine;

namespace Etheria.Game.Dialogue
{
    public interface IDialogueService
    {
        bool IsRunning { get; }
        string DefaultSpeakerId { get; }

        bool TryStart(
            string characterId,
            IDialogueParticipant participant,
            Transform interlocutor);
    }
}