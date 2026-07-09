using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;
using UnityEngine;

namespace Game.Interaction
{
    public sealed class InteractionService :
        IInteractionService,
        IInteractionRegistrationService
    {
        private readonly WorldIndex<IInteractable> _interactables = new();

        public IDisposable RegisterInteractable(
            WorldId worldId,
            IInteractable interactable)
        {
            return _interactables.Register(
                worldId,
                interactable);
        }

        public async UniTask<InteractionResult> TryInteractAsync(
            InteractionContext context,
            CancellationToken token)
        {
            if (context.InteractorWorldId.IsEmpty)
                return InteractionResult.InvalidInteractor;

            if (context.TargetWorldId.IsEmpty)
                return InteractionResult.InvalidTarget;

            if (context.InteractorWorldId == context.TargetWorldId)
                return InteractionResult.SameObject;

            if (!_interactables.TryGet(context.TargetWorldId, out var interactable))
                return InteractionResult.InteractableNotFound;

            var distance = Vector3.Distance(
                context.Origin,
                interactable.InteractionPosition);

            if (distance > interactable.MaxRange)
                return InteractionResult.OutOfRange;

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