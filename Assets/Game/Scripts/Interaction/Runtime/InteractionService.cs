using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;
using UnityEngine;

namespace Game.Interaction
{
    public sealed class InteractionService : IInteractionService
    {
        private readonly IWorldRegistry<IInteractor> _interactors;
        private readonly IWorldRegistry<IInteractable> _interactables;

        public InteractionService(
            IWorldRegistry<IInteractor> interactors,
            IWorldRegistry<IInteractable> interactables)
        {
            _interactors = interactors;
            _interactables = interactables;
        }

        public async UniTask<InteractionResult> TryInteractAsync(
            WorldId interactorWorldId,
            WorldId targetWorldId,
            CancellationToken token)
        {
            if (!_interactors.TryGet(interactorWorldId, out var interactor))
                return InteractionResult.InteractorNotFound;

            if (!_interactables.TryGet(targetWorldId, out var interactable))
                return InteractionResult.InteractableNotFound;

            var distance = Vector3.Distance(
                interactor.InteractionOrigin,
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