using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Dialogue
{
    public interface IDialogueParticipant
    {
        DialogueEvaluationStatus Evaluate(
            Guid initiatorInstanceId,
            Vector3 initiatorPosition,
            Vector3 speakerPosition);

        UniTask<DialogueStartResult> StartDialogueAsync(
            Guid initiatorInstanceId,
            Vector3 initiatorPosition,
            Vector3 speakerPosition,
            CancellationToken cancellationToken);
    }
}