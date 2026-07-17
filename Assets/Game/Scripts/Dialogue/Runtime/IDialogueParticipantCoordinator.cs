using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Dialogue
{
    public interface IDialogueParticipantCoordinator
    {
        UniTask EnterAsync(
            DialogueSession session,
            CancellationToken cancellationToken);

        UniTask ExitAsync(
            DialogueSession session,
            CancellationToken cancellationToken);
    }
}