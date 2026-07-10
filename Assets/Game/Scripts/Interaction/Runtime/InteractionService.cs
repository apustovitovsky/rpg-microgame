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
        private readonly InstanceIndex<IInteractable> _interactables =
            new();

        public IDisposable RegisterInteractable(
            Guid instanceId,
            IInteractable interactable)
        {
            return _interactables.Register(
                instanceId,
                interactable);
        }

        public async UniTask<InteractionResult> TryInteractAsync(
            InteractionContext context,
            CancellationToken token)
        {
            if (context.InteractorInstanceId == Guid.Empty)
                return InteractionResult.InvalidInteractor;

            if (context.TargetInstanceId == Guid.Empty)
                return InteractionResult.InvalidTarget;

            if (context.InteractorInstanceId ==
                context.TargetInstanceId)
            {
                return InteractionResult.SameObject;
            }

            if (!_interactables.TryGet(
                    context.TargetInstanceId,
                    out var interactable))
            {
                return InteractionResult.InteractableNotFound;
            }

            var distance = Vector3.Distance(
                context.Origin,
                interactable.InteractionPoint);

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