using UnityEngine;

namespace Etheria.Game.Dialogue
{
    public interface IDialogueParticipant
    {
        void OnDialogueStarted(Transform interlocutor);
        void OnDialogueCompleted();
    }
}