using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Commands;

namespace Game.Dialogue.Commands
{
    public sealed class DialogueParticipantCoordinator :
        IDialogueParticipantCoordinator
    {
        private readonly ICommandDispatch _commands;

        public DialogueParticipantCoordinator(
            ICommandDispatch commands)
        {
            _commands = commands
                ?? throw new ArgumentNullException(nameof(commands));
        }

        public async UniTask<IDialogueParticipantLease> EnterAsync(
            DialogueSession session,
            CancellationToken cancellationToken)
        {
            var initiatorResult = await _commands.SendAsync(
                session.InitiatorInstanceId,
                new EnterDialogueCommand(
                    session.Id,
                    session.SpeakerInstanceId),
                cancellationToken);

            EnsureEntered(
                session.InitiatorInstanceId,
                initiatorResult);

            try
            {
                var speakerResult = await _commands.SendAsync(
                    session.SpeakerInstanceId,
                    new EnterDialogueCommand(
                        session.Id,
                        session.InitiatorInstanceId),
                    cancellationToken);

                EnsureEntered(
                    session.SpeakerInstanceId,
                    speakerResult);
            }
            catch
            {
                await _commands.SendAsync(
                    session.InitiatorInstanceId,
                    new ExitDialogueCommand(session.Id),
                    CancellationToken.None);

                throw;
            }

            return new DialogueParticipantLease(
                _commands,
                session);
        }

        private static void EnsureEntered(
            Guid participantInstanceId,
            CommandResult result)
        {
            if (result == CommandResult.Completed)
                return;

            throw new InvalidOperationException(
                $"Dialogue participant '{participantInstanceId}' " +
                $"could not enter dialogue: {result}.");
        }
    }
}