using Etheria.Features.Actor;
using Etheria.Features.Camera;
using Etheria.Features.Input;
using Etheria.Game.Camera;
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
        private IActorInputHandler _currentActorHandler;
        private readonly IPlayerAvatarProvider _controlledActorProvider;

        public PlayerController(
            IGameplayInputRouter gameplayInput,
            IPlayerLookService playerLookService,
            IPlayerAvatarProvider controlledActorProvider)
        {
            _gameplayInput = gameplayInput;
            _playerLookService = playerLookService;
            _controlledActorProvider = controlledActorProvider;
        }



        public void Possess(LifetimeScope actorScope)
        {
            Unpossess();

            var runtimeRefs = actorScope.Container.Resolve<ActorRuntimeRefs>();
            _currentActorHandler = actorScope.Container.Resolve<IActorInputHandler>();

            var cameraPivot = runtimeRefs.CameraPivot != null ? runtimeRefs.CameraPivot : actorScope.transform;

            var targetable = actorScope.Container.Resolve<ITargetable>();


            var context = new PlayerAvatarContext(
                actorScope.transform,
                cameraPivot,
                _currentActorHandler,
                targetable);

            _controlledActorProvider.Set(context);

            _gameplayInput.SetHandler(_currentActorHandler);
            _playerLookService.SetHandler(_currentActorHandler);
            _playerLookService.SetTarget(actorScope.transform, cameraPivot);
        }

        public void Unpossess()
        {
            if (_currentActorHandler == null) return;

            _gameplayInput.RemoveHandler(_currentActorHandler);
            _playerLookService.RemoveHandler(_currentActorHandler);
            _playerLookService.RemoveTarget();

            _currentActorHandler = null;
            _controlledActorProvider.Clear();
        }
    }
}

