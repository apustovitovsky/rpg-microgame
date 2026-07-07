using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Interaction
{
    public interface IInteractable
    {
        float MaxRange { get; }
        bool CanInteract(InteractionContext context);

        UniTask InteractAsync(
            InteractionContext context,
            CancellationToken token);
    }
}