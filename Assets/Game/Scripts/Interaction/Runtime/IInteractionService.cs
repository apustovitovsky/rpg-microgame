using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Interaction
{
    public interface IInteractionService
    {
        UniTask<InteractionResult> TryInteractAsync(
            InteractionContext context,
            CancellationToken token);
    }
}