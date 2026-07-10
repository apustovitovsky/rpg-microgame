using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.CommandSystem;
using UnityEngine;

namespace Game.Interaction
{
    public sealed class InteractCommandHandler :
        WorldCommandHandler<InteractCommand>
    {
        private readonly IInteractable _interactable;

        public InteractCommandHandler(
            IInteractable interactable)
        {
            _interactable = interactable
                ?? throw new ArgumentNullException(nameof(interactable));
        }

        public override async UniTask<CommandResult> HandleAsync(
            InteractCommand command,
            Guid targetInstanceId,
            CancellationToken token)
        {
            if (command.InteractorInstanceId == Guid.Empty ||
                targetInstanceId == Guid.Empty ||
                command.InteractorInstanceId == targetInstanceId)
            {
                return CommandResult.Rejected;
            }

            var context = new InteractionContext(
                command.InteractorInstanceId,
                command.InteractorPosition,
                targetInstanceId);

            var distance = Vector3.Distance(
                context.Origin,
                _interactable.InteractionPoint);

            if (distance > _interactable.MaxRange ||
                !_interactable.CanInteract(context))
            {
                return CommandResult.Rejected;
            }

            if (token.IsCancellationRequested)
                return CommandResult.Cancelled;

            await _interactable.InteractAsync(context, token);

            return token.IsCancellationRequested
                ? CommandResult.Cancelled
                : CommandResult.Completed;
        }
    }
}