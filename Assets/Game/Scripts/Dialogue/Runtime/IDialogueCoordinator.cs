using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Dialogue
{
    public interface IDialogueCoordinator
    {
        bool TryGetActive(out DialogueSession session);

        DialogueEvaluationStatus Evaluate(
            DialogueRequest request);

        UniTask<DialogueRunResult> RunAsync(
            DialogueRequest request,
            CancellationToken cancellationToken);
    }
}