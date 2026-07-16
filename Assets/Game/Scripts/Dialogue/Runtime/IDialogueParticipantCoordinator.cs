using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Dialogue
{
    public interface IDialogueParticipantCoordinator
    {
        UniTask<IUniTaskAsyncDisposable> EnterAsync(
            DialogueSession session,
            CancellationToken cancellationToken);
    }
}