using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Commands;

namespace Game.Dialogue.Commands
{
    public sealed class DialogueParticipantLease :
        IDialogueParticipantLease
    {
        private readonly ICommandDispatch _commands;
        private readonly DialogueSession _session;

        private bool _isDisposed;

        public DialogueParticipantLease(
            ICommandDispatch commands,
            DialogueSession session)
        {
            _commands = commands
                ?? throw new ArgumentNullException(nameof(commands));

            _session = session
                ?? throw new ArgumentNullException(nameof(session));
        }

        public async UniTask DisposeAsync()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            await UniTask.WhenAll(
                _commands.SendAsync(
                    _session.InitiatorInstanceId,
                    new ExitDialogueCommand(_session.Id),
                    CancellationToken.None),
                _commands.SendAsync(
                    _session.SpeakerInstanceId,
                    new ExitDialogueCommand(_session.Id),
                    CancellationToken.None));
        }
    }
}