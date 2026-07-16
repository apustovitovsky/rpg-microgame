using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Commands;
using Game.Core;

namespace Game.Dialogue.Commands
{
    public sealed class DialogueParticipantCoordinator :
        IDialogueParticipantCoordinator
    {
        private readonly ICommandBus _commands;

        public DialogueParticipantCoordinator(
            ICommandBus commands)
        {
            _commands = commands
                ?? throw new ArgumentNullException(nameof(commands));
        }

        public async UniTask<IUniTaskAsyncDisposable> EnterAsync(
            DialogueSession session,
            CancellationToken cancellationToken)
        {
            var initiatorLease =
                await _commands.RequestRequiredAsync(
                    session.InitiatorInstanceId,
                    new EnterDialogueCommand(
                        session.Id,
                        session.SpeakerInstanceId),
                    cancellationToken);

            try
            {
                var speakerLease =
                    await _commands.RequestRequiredAsync(
                        session.SpeakerInstanceId,
                        new EnterDialogueCommand(
                            session.Id,
                            session.InitiatorInstanceId),
                        cancellationToken);

                return AsyncLeaseGroup.Combine(
                    initiatorLease,
                    speakerLease);
            }
            catch
            {
                await initiatorLease.DisposeAsync();
                throw;
            }
        }
    }
}