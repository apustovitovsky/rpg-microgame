using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Gameplay
{
    public sealed class PossessionService : IPossessionService
    {
        private readonly IGameplayInputRouter _gameplayInput;
        private readonly IPlayerLookService _playerLookService;
        private readonly ICameraService _cameraService;
        private IActorInputHandler _currentActorHandler;

        public PossessionService(
            IGameplayInputRouter gameplayInput,
            IPlayerLookService playerLookService,
            ICameraService cameraService)
        {
            _gameplayInput = gameplayInput;
            _playerLookService = playerLookService;
            _cameraService = cameraService;
        }

        public void Possess(LifetimeScope actorScope)
        {
            Unpossess();

            var runtimeRefs = actorScope.Container.Resolve<ActorRuntimeRefs>();
            _currentActorHandler = actorScope.Container.Resolve<IActorInputHandler>();
            var cameraPivot = runtimeRefs.CameraPivot != null ? runtimeRefs.CameraPivot : actorScope.transform;

            _gameplayInput.SetHandler(_currentActorHandler);
            _playerLookService.SetHandler(_currentActorHandler);
            _playerLookService.SetTarget(actorScope.transform, cameraPivot);
            _cameraService.SetTarget(cameraPivot);
        }

        public void Unpossess()
        {
            if (_currentActorHandler == null) return;

            _gameplayInput.RemoveHandler(_currentActorHandler);
            _playerLookService.RemoveHandler(_currentActorHandler);
            _playerLookService.RemoveTarget();
            _cameraService.RemoveTarget();

            _currentActorHandler = null;
        }
    }
}
