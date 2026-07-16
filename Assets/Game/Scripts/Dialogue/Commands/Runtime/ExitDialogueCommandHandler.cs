using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Commands;

namespace Game.Dialogue.Commands
{
    public sealed class ExitDialogueCommandHandler :
        CommandHandler<ExitDialogueCommand>
    {
        private readonly DialogueParticipantSessionStore _sessions;

        public ExitDialogueCommandHandler(
            DialogueParticipantSessionStore sessions)
        {
            _sessions = sessions
                ?? throw new ArgumentNullException(nameof(sessions));
        }

        public override async UniTask<CommandResult> HandleAsync(
            ExitDialogueCommand command,
            Guid receiverInstanceId,
            CancellationToken token)
        {
            if (command.SessionId == Guid.Empty)
                return CommandResult.Rejected;

            if (!_sessions.TryTake(
                    command.SessionId,
                    out var lease))
            {
                return CommandResult.Completed;
            }

            await lease.DisposeAsync();

            return CommandResult.Completed;
        }
    }
}