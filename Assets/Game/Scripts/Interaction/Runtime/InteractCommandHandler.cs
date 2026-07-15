using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Commands;
using UnityEngine;

namespace Game.Interaction
{
    public sealed class InteractCommandHandler :
        CommandHandler<InteractCommand>
    {
        private readonly IInteractable _target;

        public InteractCommandHandler(
            IInteractable target)
        {
            _target = target
                ?? throw new ArgumentNullException(nameof(target));
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
                command.InteractionOrigin,
                targetInstanceId);

            var distance = Vector3.Distance(
                context.Origin,
                _target.InteractionPoint);

            if (distance > _target.MaxRange ||
                !_target.CanInteract(context))
            {
                return CommandResult.Rejected;
            }

            token.ThrowIfCancellationRequested();

            var result = await _target.InteractAsync(
                context,
                token);

            return result.Status switch
            {
                InteractionStatus.Completed =>
                    CommandResult.Completed,

                InteractionStatus.Busy =>
                    CommandResult.Busy,

                _ => CommandResult.Rejected
            };
        }
    }
}