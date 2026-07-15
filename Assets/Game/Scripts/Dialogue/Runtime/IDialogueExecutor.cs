using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Dialogue
{
    public interface IDialogueExecutor
    {
        UniTask ExecuteAsync(
            DialogueSession session,
            CancellationToken cancellationToken);
    }
}