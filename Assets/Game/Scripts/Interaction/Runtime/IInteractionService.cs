using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Interaction
{
    public interface IInteractionService
    {
        UniTask<bool> TryInteractAsync(
            IWorldHandle interactor,
            WorldId targetWorldId,
            CancellationToken token);
    }
}