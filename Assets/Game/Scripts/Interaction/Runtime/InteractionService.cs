using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;
using UnityEngine;

namespace Game.Interaction
{
    public sealed class InteractionService : IInteractionService
    {
        private readonly IWorldRegistry<IInteractable> _interactables;

        public InteractionService(
            IWorldRegistry<IInteractable> interactables)
        {
            _interactables = interactables;
        }

        public async UniTask<InteractionResult> TryInteractAsync(
            WorldId interactorWorldId,
            Vector3 interactionOrigin,
            WorldId targetWorldId,
            CancellationToken token)
        {
            if (interactorWorldId.IsEmpty)
                return InteractionResult.InvalidInteractor;

            if (targetWorldId.IsEmpty)
                return InteractionResult.InvalidTarget;

            if (interactorWorldId == targetWorldId)
                return InteractionResult.SameObject;

            if (!_interactables.TryGet(targetWorldId, out var interactable))
                return InteractionResult.InteractableNotFound;

            var distance = Vector3.Distance(
                interactionOrigin,
                interactable.InteractionPosition);

            if (distance > interactable.MaxRange)
                return InteractionResult.OutOfRange;

            var context = new InteractionContext(
                interactorWorldId,
                targetWorldId);

            if (!interactable.CanInteract(context))
                return InteractionResult.Rejected;

            if (token.IsCancellationRequested)
                return InteractionResult.Cancelled;

            await interactable.InteractAsync(
                context,
                token);

            return token.IsCancellationRequested
                ? InteractionResult.Cancelled
                : InteractionResult.Succeeded;
        }
    }
}