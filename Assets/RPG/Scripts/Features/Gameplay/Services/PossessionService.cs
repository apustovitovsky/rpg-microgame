using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public sealed class PossessionService : IPossessionService
    {
        private readonly IGameplayInputRouter _gameplayInput;
        private readonly ICameraService _cameraService;
        private IActorInputHandler _currentActorHandler;

        public PossessionService(
            IGameplayInputRouter gameplayInput,
            ICameraService cameraService)
        {
            _gameplayInput = gameplayInput;
            _cameraService = cameraService;
        }

        public void Possess(GameObject actor)
        {
            Unpossess();

            if (!actor.TryGetComponent<LifetimeScope>(out var actorScope))
            {
                throw new UnityException($"Actor '{actor.name}' is missing a {nameof(LifetimeScope)}.");
            }

            _currentActorHandler = actorScope.Container.Resolve<IActorInputHandler>();

            _gameplayInput.SetHandler(_currentActorHandler);
            _cameraService.SetHandler(_currentActorHandler);
            _cameraService.SetTarget(actor.transform);
        }

        public void Unpossess()
        {
            if (_currentActorHandler == null) return;

            _gameplayInput.RemoveHandler(_currentActorHandler);
            _cameraService.RemoveHandler(_currentActorHandler);
            _cameraService.RemoveTarget();

            _currentActorHandler = null;
        }
    }
}
