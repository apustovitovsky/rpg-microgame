using System;
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

    public interface IInteractionRegistrationService
    {
        IDisposable RegisterInteractable(
            Guid instanceId,
            IInteractable interactable);
    }
}