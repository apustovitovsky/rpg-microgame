using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Commands;

namespace Game.Interaction
{
    public sealed class InteractCommandHandler :
        CommandHandler<InteractCommand>
    {
        private readonly IInteractionSource _interactor;
        private readonly IInteractionService _interactionService;

        public InteractCommandHandler(
            IInteractionSource interactor,
            IInteractionService interactionService)
        {
            _interactor = interactor
                ?? throw new ArgumentNullException(nameof(interactor));

            _interactionService = interactionService
                ?? throw new ArgumentNullException(
                    nameof(interactionService));
        }

        public override async UniTask<CommandResult> HandleAsync(
            InteractCommand command,
            Guid interactorInstanceId,
            CancellationToken token)
        {
            var context = new InteractionContext(
                interactorInstanceId,
                _interactor.InteractionOrigin,
                command.TargetInstanceId);

            var result = await _interactionService.TryInteractAsync(
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