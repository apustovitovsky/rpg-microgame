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

            if (actorScope.Container == null)
            {
                if (!actorScope.autoRun)
                {
                    actorScope.Build();
                }
                else
                {
                    throw new UnityException(
                        $"Actor '{actor.name}' scope container is not ready yet. Possession must happen after the actor scope is built.");
                }
            }

            var runtimeRefs = actorScope.Container.Resolve<ActorRuntimeRefs>();
            _currentActorHandler = actorScope.Container.Resolve<IActorInputHandler>();

            _gameplayInput.SetHandler(_currentActorHandler);
            _cameraService.SetHandler(_currentActorHandler);
            _cameraService.SetTarget(actor.transform, runtimeRefs.CameraPivot != null ? runtimeRefs.CameraPivot : actor.transform);
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
