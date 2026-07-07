using Game.Interaction;
using Game.Pickup;
using Game.Targeting;
using Game.World;
using VContainer;

namespace Game.Actor
{
    public sealed class ActorWorldObjectFactory
    {
        private readonly IObjectResolver _resolver;

        public ActorWorldObjectFactory(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public IWorldObject Create(WorldId worldId)
        {
            var view = _resolver.Resolve<IActorView>();

            var builder = new WorldObjectBuilder()
                .Add<IActorView>(view);

            if (_resolver.TryResolve<IActorInputBinder>(out var inputBinder))
                builder.Add<IActorInputBinder>(inputBinder);

            if (_resolver.TryResolve<ITargetProvider>(out var targetProvider))
                builder.Add<ITargetProvider>(targetProvider);

            if (_resolver.TryResolve<IInteractable>(out var interaction))
                builder.Add<IInteractable>(interaction);

            if (_resolver.TryResolve<IActorDialogueEndpoint>(out var dialogue))
                builder.Add<IActorDialogueEndpoint>(dialogue);

            if (_resolver.TryResolve<IActorTravelEndpoint>(out var travel))
                builder.Add<IActorTravelEndpoint>(travel);

            if (_resolver.TryResolve<IPickupEffectHandlerProvider>(out var pickupEffects))
                builder.Add<IPickupEffectHandlerProvider>(pickupEffects);

            return builder.Build(
                worldId,
                view.Root.gameObject);
        }
    }
}