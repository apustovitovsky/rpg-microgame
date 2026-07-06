using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Interaction
{
    public interface IInteractable
    {
        bool CanInteract(InteractionContext context);

        UniTask InteractAsync(
            InteractionContext context,
            CancellationToken token);
    }
}