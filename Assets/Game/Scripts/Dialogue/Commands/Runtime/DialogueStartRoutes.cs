using System;
using Cysharp.Threading.Tasks;
using Game.Commands;

namespace Game.Dialogue.Commands
{
    public sealed class DialogueStartRoutes :
        ICommandRoutes,
        ICommandHandler<
            StartDialogueCommand,
            DialogueStartResult>
    {
        private readonly IDialogueParticipant _participant;

        public DialogueStartRoutes(
            IDialogueParticipant participant)
        {
            _participant = participant
                ?? throw new ArgumentNullException(
                    nameof(participant));
        }

        public CommandOrdering Ordering =>
            CommandOrdering.Drop;

        public UniTask<DialogueStartResult> HandleAsync(
            StartDialogueCommand command,
            CommandContext context)
        {
            return _participant.StartDialogueAsync(
                command.InitiatorInstanceId,
                context.CancellationToken);
        }
    }
}