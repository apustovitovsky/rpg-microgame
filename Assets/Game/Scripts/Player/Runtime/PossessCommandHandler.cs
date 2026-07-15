using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Commands;
using Game.Control;
using Game.Interaction;
using Game.Targeting;

namespace Game.Player
{
    public readonly struct PossessCommand :
        ICommand
    {
    }

    public sealed class PossessCommandHandler :
        CommandHandler<PossessCommand>,
        IDisposable
    {
        private readonly PlayerControlService _playerControl;
        private readonly IPossessionEndpoint _endpoint;
        private readonly IInteractor _interactor;
        private readonly ITargetProvider _targetProvider;

        public PossessCommandHandler(
            PlayerControlService playerControl,
            IPossessionEndpoint endpoint,
            IInteractor interactor,
            ITargetProvider targetProvider)
        {
            _playerControl = playerControl
                ?? throw new ArgumentNullException(
                    nameof(playerControl));

            _endpoint = endpoint
                ?? throw new ArgumentNullException(
                    nameof(endpoint));

            _interactor = interactor
                ?? throw new ArgumentNullException(
                    nameof(interactor));

            _targetProvider = targetProvider
                ?? throw new ArgumentNullException(
                    nameof(targetProvider));
        }

        public override UniTask<CommandResult> HandleAsync(
            PossessCommand command,
            Guid targetInstanceId,
            CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return UniTask.FromResult(
                    CommandResult.Cancelled);
            }

            return UniTask.FromResult(
                _playerControl.Attach(
                    targetInstanceId,
                    _endpoint,
                    _interactor,
                    _targetProvider));
        }

        public void Dispose()
        {
            _playerControl.ReleaseIfCurrent(_endpoint);
        }
    }
}