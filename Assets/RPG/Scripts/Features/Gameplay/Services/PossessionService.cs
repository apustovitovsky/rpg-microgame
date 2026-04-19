using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public sealed class PossessionService : IPossessionService
    {
        private readonly IGameplayInputRouter _gameplayInput;
        private readonly ICameraService _cameraService;
        private IPlayerInputHandler _playerInputHandler;

        public PossessionService(
            IGameplayInputRouter gameplayInput,
            ICameraService cameraService)
        {
            _gameplayInput = gameplayInput;
            _cameraService = cameraService;
        }

        public void Possess(LifetimeScope scope)
        {
            if (scope == null) return;

            Unpossess();

            _playerInputHandler = scope.Container.Resolve<IPlayerInputHandler>();
            var controller = scope.Container.Resolve<CharacterController>();

            _gameplayInput.SetHandler(_playerInputHandler);
            _cameraService.SetHandler(_playerInputHandler);
            _cameraService.SetTarget(controller.transform);
        }

        public void Unpossess()
        {
            if (_playerInputHandler == null) return;

            _gameplayInput.RemoveHandler(_playerInputHandler);
            _cameraService.RemoveHandler(_playerInputHandler);
            _cameraService.RemoveTarget();

            _playerInputHandler = null;
        }
    }
}
