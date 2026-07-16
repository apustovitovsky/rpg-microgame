using System;
using Cysharp.Threading.Tasks;
using Game.Commands;
using UnityEngine;

namespace Game.Interaction
{
    public sealed class InteractionExecution :
        ICommandExecutionGroup,
        ICommandExecution<
            InteractCommand,
            InteractionResult>
    {
        private readonly IInteractable _target;

        public InteractionExecution(
            IInteractable target)
        {
            _target = target
                ?? throw new ArgumentNullException(nameof(target));
        }

        public CommandExecutionPolicy ExecutionPolicy =>
            CommandExecutionPolicy.Drop;

        public UniTask<InteractionResult> ExecuteAsync(
            InteractCommand command,
            CommandContext context)
        {
            if (command.InteractorInstanceId == Guid.Empty ||
                context.ReceiverId == Guid.Empty ||
                command.InteractorInstanceId ==
                context.ReceiverId)
            {
                return UniTask.FromResult(
                    InteractionResult.Rejected);
            }

            var interactionContext = new InteractionContext(
                command.InteractorInstanceId,
                command.InteractionOrigin,
                context.ReceiverId);

            var distance = Vector3.Distance(
                interactionContext.Origin,
                _target.InteractionPoint);

            if (distance > _target.MaxRange ||
                !_target.CanInteract(interactionContext))
            {
                return UniTask.FromResult(
                    InteractionResult.Rejected);
            }

            return _target.InteractAsync(
                interactionContext,
                context.CancellationToken);
        }
    }
}