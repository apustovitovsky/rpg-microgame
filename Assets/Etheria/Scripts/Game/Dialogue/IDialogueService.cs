using UnityEngine;

namespace Etheria.Game.Dialogue
{
    public interface IDialogueService
    {
        bool IsRunning { get; }

        bool TryStart(
            string nodeName,
            IDialogueParticipant participant,
            Transform interlocutor);
    }
}