using Etheria.Game.Actor;
using Etheria.Game.Player;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Player
{
    public sealed class PlayerController : IPlayerController
    {
        private readonly IPlayerAvatarProvider _controlledActorProvider;

        public PlayerController(
            IPlayerAvatarProvider controlledActorProvider)
        {
            _controlledActorProvider = controlledActorProvider;
        }

        public void Possess(LifetimeScope actorScope)
        {
            Unpossess();

            var controllableActor = actorScope.Container.Resolve<IControllableActor>();
            var context = new PlayerAvatarContext(
                controllableActor.Root,
                controllableActor.CameraPivot,
                controllableActor.Handlers,
                controllableActor.Targetable);

            _controlledActorProvider.Set(context);
        }

        public void Unpossess()
        {
            _controlledActorProvider.Clear();
        }
    }
}
