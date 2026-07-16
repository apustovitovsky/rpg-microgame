using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Commands;

namespace Game.Dialogue.Commands
{
    public sealed class EnterDialogueCommandHandler :
        CommandHandler<EnterDialogueCommand>
    {
        private readonly IEnumerable<IDialogueParticipantLifecycle>
            _lifecycles;

        private readonly DialogueParticipantSessionStore _sessions;

        public EnterDialogueCommandHandler(
            IEnumerable<IDialogueParticipantLifecycle> lifecycles,
            DialogueParticipantSessionStore sessions)
        {
            _lifecycles = lifecycles
                ?? throw new ArgumentNullException(nameof(lifecycles));

            _sessions = sessions
                ?? throw new ArgumentNullException(nameof(sessions));
        }

        public override async UniTask<CommandResult> HandleAsync(
            EnterDialogueCommand command,
            Guid receiverInstanceId,
            CancellationToken token)
        {
            if (command.SessionId == Guid.Empty ||
                command.OtherParticipantInstanceId == Guid.Empty ||
                receiverInstanceId == Guid.Empty ||
                receiverInstanceId ==
                command.OtherParticipantInstanceId)
            {
                return CommandResult.Rejected;
            }

            if (_sessions.Contains(command.SessionId))
                return CommandResult.Busy;

            var context = new DialogueParticipantContext(
                command.SessionId,
                command.OtherParticipantInstanceId);

            var composite =
                new CompositeDialogueParticipantLease();

            try
            {
                foreach (var lifecycle in _lifecycles)
                {
                    var lease = await lifecycle.EnterAsync(
                        context,
                        token);

                    composite.Add(lease);
                }

                _sessions.Add(
                    command.SessionId,
                    composite);

                return CommandResult.Completed;
            }
            catch (OperationCanceledException)
                when (token.IsCancellationRequested)
            {
                await composite.DisposeAsync();

                return CommandResult.Cancelled;
            }
            catch
            {
                await composite.DisposeAsync();
                throw;
            }
        }
    }
}