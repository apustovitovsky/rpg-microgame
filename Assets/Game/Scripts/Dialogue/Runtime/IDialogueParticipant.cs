using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Dialogue
{
    public interface IDialogueParticipant
    {
        DialogueEvaluationStatus Evaluate(
            Guid initiatorInstanceId);

        UniTask<DialogueStartResult> StartDialogueAsync(
            Guid initiatorInstanceId,
            CancellationToken cancellationToken);
    }
}