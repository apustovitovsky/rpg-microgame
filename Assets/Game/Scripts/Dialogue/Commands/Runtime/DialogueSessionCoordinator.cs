using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Commands;

namespace Game.Dialogue.Commands
{
    public sealed class DialogueSessionCoordinator :
        IDialogueParticipantCoordinator
    {
        private readonly ICommandBus _commands;

        public DialogueSessionCoordinator(
            ICommandBus commands)
        {
            _commands = commands
                ?? throw new ArgumentNullException(nameof(commands));
        }

        public async UniTask EnterAsync(
            DialogueSession session,
            CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.WhenAll(
                    SendRequiredAsync(
                        session.InitiatorInstanceId,
                        new EnterDialogueSessionCommand(
                            session.Id,
                            session.SpeakerInstanceId,
                            session.SpeakerPosition),
                        cancellationToken),

                    SendRequiredAsync(
                        session.SpeakerInstanceId,
                        new EnterDialogueSessionCommand(
                            session.Id,
                            session.InitiatorInstanceId,
                            session.InitiatorPosition),
                        cancellationToken));
            }
            catch
            {
                await ExitBothAsync(session);

                throw;
            }
        }

        public UniTask ExitAsync(
            DialogueSession session,
            CancellationToken _)
        {
            return ExitBothAsync(session);
        }

        private async UniTask ExitBothAsync(
            DialogueSession session)
        {
            try
            {
                await SendRequiredAsync(
                    session.InitiatorInstanceId,
                    new ExitDialogueSessionCommand(
                        session.Id),
                    CancellationToken.None);
            }
            finally
            {
                await SendRequiredAsync(
                    session.SpeakerInstanceId,
                    new ExitDialogueSessionCommand(
                        session.Id),
                    CancellationToken.None);
            }
        }

        private async UniTask SendRequiredAsync(
            Guid targetInstanceId,
            ICommand command,
            CancellationToken cancellationToken)
        {
            var result = await _commands.SendAsync(
                targetInstanceId,
                command,
                cancellationToken);

            if (result.IsDelivered)
            {
                return;
            }

            throw new InvalidOperationException(
                $"Command request to '{targetInstanceId}' failed: " +
                $"{result.Status}.");
        }
    }
}