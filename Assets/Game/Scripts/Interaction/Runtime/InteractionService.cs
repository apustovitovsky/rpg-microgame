using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using UnityEngine;

namespace Game.Interaction
{
    public sealed class InteractionService :
        IInteractionService,
        IRegistryWriter<IInteractionTarget>
    {
        private readonly Registry<IInteractionTarget> _interactables =
            new();

        public void Add(
            Guid id,
            IInteractionTarget value)
        {
            _interactables.Add(id, value);
        }

        public bool Remove(
            Guid id,
            IInteractionTarget expectedValue)
        {
            return _interactables.Remove(
                id,
                expectedValue);
        }

        public async UniTask<InteractionResult> TryInteractAsync(
            InteractionContext context,
            CancellationToken token)
        {
            if (context.InteractorInstanceId == Guid.Empty ||
                context.TargetInstanceId == Guid.Empty ||
                context.InteractorInstanceId ==
                context.TargetInstanceId)
            {
                return InteractionResult.Rejected;
            }

            if (!_interactables.TryGet(
                    context.TargetInstanceId,
                    out var interactable))
            {
                return InteractionResult.Rejected;
            }

            var distance = Vector3.Distance(
                context.Origin,
                interactable.InteractionPoint);

            if (distance > interactable.MaxRange ||
                !interactable.CanInteract(context))
            {
                return InteractionResult.Rejected;
            }

            token.ThrowIfCancellationRequested();

            var result = await interactable.InteractAsync(
                context,
                token);

            token.ThrowIfCancellationRequested();

            return result;
        }
    }
}