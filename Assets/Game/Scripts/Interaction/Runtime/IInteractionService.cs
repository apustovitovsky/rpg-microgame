using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Interaction
{
    public interface IInteractionService
    {
        UniTask<InteractionResult> TryInteractAsync(
            WorldId interactorWorldId,
            WorldId targetWorldId,
            CancellationToken token);
    }
}