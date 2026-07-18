using System;
using Cysharp.Threading.Tasks;
using Game.Commands;

namespace Game.Dialogue.Commands
{
    public sealed class DialogueParticipationExecution :
        ICommandExecutionGroup,
        ICommandExecution<
            EnterDialogueSessionCommand>,
        ICommandExecution<
            ExitDialogueSessionCommand>
    {
        private readonly IDialogueParticipation
            _participation;

        public DialogueParticipationExecution(
            IDialogueParticipation participation)
        {
            _participation = participation
                ?? throw new ArgumentNullException(
                    nameof(participation));
        }

        public CommandExecutionPolicy ExecutionPolicy =>
            CommandExecutionPolicy.Switch;

        public async UniTask ExecuteAsync(
            EnterDialogueSessionCommand command,
            CommandContext context)
        {
            if (command.SessionId == Guid.Empty ||
                command.OtherParticipantInstanceId == Guid.Empty ||
                context.ReceiverId == Guid.Empty ||
                context.ReceiverId ==
                command.OtherParticipantInstanceId)
            {
                throw new ArgumentException(
                    "Dialogue participant command is invalid.",
                    nameof(command));
            }

            var participantContext =
                new DialogueSessionContext(
                    command.SessionId,
                    command.OtherParticipantInstanceId,
                    command.OtherParticipantPosition);

            if (!_participation.TryEnter(
                    participantContext))
            {
                throw new InvalidOperationException(
                    "Actor is already participating in " +
                    "another dialogue session.");
            }

            try
            {
                await _participation.WaitUntilReadyAsync(
                    command.SessionId,
                    context.CancellationToken);
            }
            catch
            {
                _participation.TryExit(
                    command.SessionId);

                throw;
            }
        }

        public UniTask ExecuteAsync(
            ExitDialogueSessionCommand command,
            CommandContext context)
        {
            if (command.SessionId == Guid.Empty ||
                context.ReceiverId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Dialogue participant exit command is invalid.",
                    nameof(command));
            }

            _participation.TryExit(command.SessionId);

            return UniTask.CompletedTask;
        }
    }
}