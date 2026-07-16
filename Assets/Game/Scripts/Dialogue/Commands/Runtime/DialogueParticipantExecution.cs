using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Commands;
using Game.Core;

namespace Game.Dialogue.Commands
{
    public sealed class DialogueParticipantExecution :
        ICommandExecutionGroup,
        ICommandExecution<
            EnterDialogueSessionCommand,
            IUniTaskAsyncDisposable>
    {
        private readonly IEnumerable<IDialogueParticipantLifecycle>
            _lifecycles;

        public DialogueParticipantExecution(
            IEnumerable<IDialogueParticipantLifecycle> lifecycles)
        {
            _lifecycles = lifecycles
                ?? throw new ArgumentNullException(nameof(lifecycles));
        }

        public CommandExecutionPolicy ExecutionPolicy =>
            CommandExecutionPolicy.Sequential;

        public async UniTask<IUniTaskAsyncDisposable> ExecuteAsync(
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

            var leases = new AsyncLeaseGroup();

            try
            {
                var participantContext =
                    new DialogueSessionContext(
                        command.SessionId,
                        command.OtherParticipantInstanceId);

                foreach (var lifecycle in _lifecycles)
                {
                    leases.Add(
                        await lifecycle.EnterAsync(
                            participantContext,
                            context.CancellationToken));
                }

                return leases;
            }
            catch
            {
                await leases.DisposeAsync();
                throw;
            }
        }
    }
}