using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Commands;

namespace Game.Interaction
{
    public sealed class InteractCommandHandler :
        CommandHandler<InteractCommand>
    {
        private readonly IInteractor _interactor;
        private readonly IInteractionService _interactionService;

        public InteractCommandHandler(
            IInteractor interactor,
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

            var succeeded =
                await _interactionService.TryInteractAsync(
                    context,
                    token);

            return succeeded
                ? CommandResult.Completed
                : CommandResult.Rejected;
        }
    }
}