using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using Game.Player;

namespace Game.Dialogue.Player
{
    public sealed class PlayerDialogueLifecycle :
        IDialogueParticipantLifecycle
    {
        private readonly IInstanceIdentity _identity;
        private readonly IPlayerControl _control;
        private readonly IPlayerUiInput _input;

        public PlayerDialogueLifecycle(
            IInstanceIdentity identity,
            IPlayerControl control,
            IPlayerUiInput input)
        {
            _identity = identity
                ?? throw new ArgumentNullException(nameof(identity));

            _control = control
                ?? throw new ArgumentNullException(nameof(control));

            _input = input
                ?? throw new ArgumentNullException(nameof(input));
        }

        public UniTask<IUniTaskAsyncDisposable> EnterAsync(
            DialogueParticipantContext context,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_control.ControlledInstanceId != _identity.InstanceId)
            {
                return UniTask.FromResult<IUniTaskAsyncDisposable>(
                    EmptyLease.Instance);
            }

            return UniTask.FromResult<IUniTaskAsyncDisposable>(
                new PlayerUiInputLease(
                    _input.AcquireUiInput()));
        }

        private sealed class PlayerUiInputLease :
            IUniTaskAsyncDisposable
        {
            private IDisposable _inputLease;

            public PlayerUiInputLease(
                IDisposable inputLease)
            {
                _inputLease = inputLease
                    ?? throw new ArgumentNullException(
                        nameof(inputLease));
            }

            public UniTask DisposeAsync()
            {
                var inputLease = _inputLease;
                _inputLease = null;

                inputLease?.Dispose();

                return UniTask.CompletedTask;
            }
        }

        private sealed class EmptyLease :
            IUniTaskAsyncDisposable
        {
            public static readonly EmptyLease Instance =
                new EmptyLease();

            private EmptyLease()
            {
            }

            public UniTask DisposeAsync()
            {
                return UniTask.CompletedTask;
            }
        }
    }
}