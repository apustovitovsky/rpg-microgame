using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Dialogue
{
    public interface IDialogueParticipantLifecycle
    {
        UniTask<IUniTaskAsyncDisposable> EnterAsync(
            DialogueParticipantContext context,
            CancellationToken cancellationToken);
    }
}