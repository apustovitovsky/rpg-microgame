using Etheria.Features.Actor;

using Etheria.Game.Player;
using Etheria.Game.Targeting;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Player
{
    public sealed class PlayerController : IPlayerController
    {
        private IActorInputHandler _currentActorHandler;
        private readonly IPlayerAvatarProvider _controlledActorProvider;

        public PlayerController(
            IPlayerAvatarProvider controlledActorProvider)
        {
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
        }

        public void Unpossess()
        {
            if (_currentActorHandler == null) return;

            _currentActorHandler = null;
            _controlledActorProvider.Clear();
        }
    }
}

