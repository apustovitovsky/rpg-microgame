using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Interaction
{
    public interface IInteractionService
    {
        UniTask<InteractionResult> TryInteractAsync(
            InteractionContext request,
            CancellationToken token);
    }

    public interface IInteractionRegistrationService
    {
        IDisposable RegisterInteractable(
            WorldId worldId,
            IInteractable interactable);
    }
}