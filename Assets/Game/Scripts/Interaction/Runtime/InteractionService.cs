using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using UnityEngine;

namespace Game.Interaction
{
    public sealed class InteractionService :
        IInteractionService,
        IRegistryWriter<IInteractable>
    {
        private readonly Registry<IInteractable> _interactables =
            new();

        public void Add(
            Guid id,
            IInteractable value)
        {
            _interactables.Add(id, value);
        }

        public bool Remove(
            Guid id,
            IInteractable expectedValue)
        {
            return _interactables.Remove(
                id,
                expectedValue);
        }

        public async UniTask<bool> TryInteractAsync(
            InteractionContext context,
            CancellationToken token)
        {
            if (context.InteractorInstanceId == Guid.Empty ||
                context.TargetInstanceId == Guid.Empty ||
                context.InteractorInstanceId ==
                context.TargetInstanceId)
            {
                return false;
            }

            if (!_interactables.TryGet(
                    context.TargetInstanceId,
                    out var interactable))
            {
                return false;
            }

            var distance = Vector3.Distance(
                context.Origin,
                interactable.InteractionPoint);

            if (distance > interactable.MaxRange ||
                !interactable.CanInteract(context))
            {
                return false;
            }

            token.ThrowIfCancellationRequested();

            await interactable.InteractAsync(
                context,
                token);

            token.ThrowIfCancellationRequested();

            return true;
        }
    }
}