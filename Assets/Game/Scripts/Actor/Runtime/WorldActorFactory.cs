using Game.Interaction;
using Game.Targeting;
using Game.World;
using VContainer;

namespace Game.Actor
{
    public sealed class WorldActorFactory
    {
        private readonly IObjectResolver _resolver;

        public WorldActorFactory(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public WorldActor Create(
            WorldId worldId,
            string displayName)
        {
            var view = _resolver.Resolve<IActorView>();

            _resolver.TryResolve<IActorTravelEndpoint>(
                out var travel);

            _resolver.TryResolve<IActorDialogueEndpoint>(
                out var dialogue);

            _resolver.TryResolve<IInteractable>(
                out var interaction);

            _resolver.TryResolve<ITargetProvider>(
                out var targetProvider);

            _resolver.TryResolve<IActorInputBinder>(
                out var inputBinder);

            return new(
                worldId,
                displayName,
                view,
                travel,
                targetProvider,
                inputBinder,
                dialogue,
                interaction);
        }
    }
}