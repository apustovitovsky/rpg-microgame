using System;
using Game.Core;
using Game.Player;
using VContainer.Unity;

namespace Game.Dialogue.Player
{
    public sealed class PlayerDialogueInputController :
        IInitializable,
        IDisposable
    {
        private readonly IInstanceIdentity _identity;
        private readonly IPlayerControl _control;
        private readonly IPlayerUiInput _input;
        private readonly IDialogueParticipation _participation;

        private IDisposable _uiInputLease;
        private Guid _sessionId;

        public PlayerDialogueInputController(
            IInstanceIdentity identity,
            IPlayerControl control,
            IPlayerUiInput input,
            IDialogueParticipation participation)
        {
            _identity = identity
                ?? throw new ArgumentNullException(nameof(identity));

            _control = control
                ?? throw new ArgumentNullException(nameof(control));

            _input = input
                ?? throw new ArgumentNullException(nameof(input));

            _participation = participation
                ?? throw new ArgumentNullException(
                    nameof(participation));
        }

        public void Initialize()
        {
            _participation.ContextEntered +=
                OnContextEntered;

            _participation.ContextExited +=
                OnContextExited;
        }

        public void Dispose()
        {
            _participation.ContextEntered -=
                OnContextEntered;

            _participation.ContextExited -=
                OnContextExited;

            ReleaseUiInput();
        }

        private void OnContextEntered(
            DialogueSessionContext context)
        {
            if (_control.ControlledInstanceId !=
                _identity.InstanceId)
            {
                return;
            }

            if (_uiInputLease != null)
            {
                throw new InvalidOperationException(
                    "Player UI input is already acquired for " +
                    "another dialogue.");
            }

            _uiInputLease = _input.AcquireUiInput();
            _sessionId = context.SessionId;

            if (!_participation.TryMarkReady(
                    context.SessionId))
            {
                ReleaseUiInput();

                throw new InvalidOperationException(
                    "Player dialogue participation was not " +
                    "entered before readiness.");
            }
        }

        private void OnContextExited(
            DialogueSessionContext context)
        {
            if (_sessionId != context.SessionId)
            {
                return;
            }

            ReleaseUiInput();
        }

        private void ReleaseUiInput()
        {
            var uiInputLease = _uiInputLease;

            _uiInputLease = null;
            _sessionId = Guid.Empty;

            uiInputLease?.Dispose();
        }
    }
}