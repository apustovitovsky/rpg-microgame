using System;
using Cysharp.Threading.Tasks;
using Game.Commands;
using Game.Control;
using Game.Interaction;
using Game.Targeting;

namespace Game.Player
{
    public sealed class PossessionRoutes :
        ICommandRoutes,
        ICommandHandler<
            PossessCommand,
            PossessionResult>,
        IDisposable
    {
        private readonly PlayerControlService _playerControl;
        private readonly IPossessionEndpoint _endpoint;
        private readonly IInteractor _interactor;
        private readonly ITargetProvider _targetProvider;

        public PossessionRoutes(
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

        public CommandOrdering Ordering =>
            CommandOrdering.Drop;

        public UniTask<PossessionResult> HandleAsync(
            PossessCommand command,
            CommandContext context)
        {
            return UniTask.FromResult(
                _playerControl.Attach(
                    context.ReceiverId,
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