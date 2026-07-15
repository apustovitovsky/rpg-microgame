using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Commands;

namespace Game.Dialogue.Commands
{
    public sealed class StartDialogueCommandHandler :
        CommandHandler<StartDialogueCommand>
    {
        private readonly IDialogueParticipant _participant;

        public StartDialogueCommandHandler(
            IDialogueParticipant participant)
        {
            _participant = participant
                ?? throw new ArgumentNullException(
                    nameof(participant));
        }

        public override async UniTask<CommandResult> HandleAsync(
            StartDialogueCommand command,
            Guid receiverInstanceId,
            CancellationToken token)
        {
            var result = await _participant.StartDialogueAsync(
                command.InitiatorInstanceId,
                token);

            return result.Status switch
            {
                DialogueStartStatus.Started =>
                    CommandResult.Completed,

                DialogueStartStatus.Busy =>
                    CommandResult.Busy,

                _ => CommandResult.Rejected
            };
        }
    }
}