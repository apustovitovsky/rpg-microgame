using Etheria.Features.Actor;
using Etheria.Features.Camera;
using Etheria.Features.Input;
using Etheria.Game.Player;
using Etheria.Game.Targeting;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Player
{
    public sealed class PlayerController : IPlayerController
    {
        private readonly IGameplayInputRouter _gameplayInput;
        private readonly IPlayerLookService _playerLookService;
        private readonly ICameraService _cameraService;
        private IActorInputHandler _currentActorHandler;
        private readonly IControlledTargetProvider _controlledTarget;

        public PlayerController(
            IGameplayInputRouter gameplayInput,
            IPlayerLookService playerLookService,
            ICameraService cameraService,
            IControlledTargetProvider controlledTarget)
        {
            _gameplayInput = gameplayInput;
            _playerLookService = playerLookService;
            _cameraService = cameraService;
            _controlledTarget = controlledTarget;
        }



        public void Possess(LifetimeScope actorScope)
        {
            Unpossess();

            var runtimeRefs = actorScope.Container.Resolve<ActorRuntimeRefs>();
            _currentActorHandler = actorScope.Container.Resolve<IActorInputHandler>();
            _controlledTarget.SetTarget(actorScope.Container.Resolve<ITargetable>());
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
            _controlledTarget.ClearTarget();

            _currentActorHandler = null;
        }
    }
}

